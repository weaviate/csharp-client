#!/usr/bin/env python3
"""Fail when public query/generate methods document a summary but not their parameters.

CS1573 only fires when some parameters are documented but not all; CS1591 only
fires when the method has no doc comment at all. Methods with a <summary> and
zero <param> tags (Hybrid before this fix) slip through both. This check closes
that gap for the query/generate client surface.

Declarations are located with a small brace/paren scanner rather than a single
regex: a regex cannot balance the nested parentheses that appear in real
signatures (``= default(CancellationToken)``), in tuple return types
(``Task<(int, string)>``), or in attribute arguments.
"""

from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CLIENT_ROOT = ROOT / "src" / "Weaviate.Client"

# Query/generate API partials (excludes TypedDataClient and other surfaces).
SCOPED_PREFIXES = (
    "QueryClient.",
    "GenerateClient.",
    "Typed/TypedQueryClient.",
    "Typed/TypedGenerateClient.",
)

DOC_RUN_RE = re.compile(r"(?:[ \t]*///.*\n)+")

# Trailing identifier of a declaration header, optionally generic: `Hybrid<T>`.
NAME_RE = re.compile(r"(?P<name>\w+)\s*(?P<generic><[^<>()]*>)?\s*$")

# A header must look like a declaration, not a random expression.
HEADER_SANITY_RE = re.compile(r"^[\w\s<>\[\],\.\?\(\):@]+$")

# Only the public surface is in scope (private helpers may document a summary
# alone). This mirrors the `public` requirement the original regex encoded.
PUBLIC_RE = re.compile(r"\bpublic\b")

# A method can never be named with a reserved word. Without this, the `new()`
# in a type constraint (`class Foo<T> where T : class, new()`) parses as a
# zero-parameter method called `new`.
RESERVED_NAMES = frozenset(
    {
        "new",
        "class",
        "struct",
        "record",
        "interface",
        "enum",
        "where",
        "return",
        "if",
        "switch",
        "while",
        "for",
        "foreach",
        "lock",
        "using",
        "catch",
        "fixed",
        "nameof",
        "typeof",
        "sizeof",
        "default",
        "base",
        "this",
        "get",
        "set",
        "init",
    }
)

# How far past a doc comment we are willing to look for the `(`.
MAX_HEADER_SCAN = 800


def is_scoped(path: Path) -> bool:
    rel = path.relative_to(CLIENT_ROOT).as_posix()
    return any(rel.startswith(prefix) for prefix in SCOPED_PREFIXES)


def _skip_literal(text: str, i: int) -> int:
    """text[i] starts a string/char literal. Return the index just past it."""
    if text.startswith('@"', i):
        i += 2
        while i < len(text):
            if text[i] == '"':
                if text.startswith('""', i):
                    i += 2
                    continue
                return i + 1
            i += 1
        return i
    quote = text[i]
    i += 1
    while i < len(text):
        if text[i] == "\\":
            i += 2
            continue
        if text[i] == quote:
            return i + 1
        i += 1
    return i


def _match_bracket(text: str, i: int, open_ch: str, close_ch: str) -> int:
    """text[i] == open_ch. Return the index just past the matching close_ch.

    Literal-aware, so a bracket or paren inside a string does not unbalance it.
    """
    depth = 0
    while i < len(text):
        ch = text[i]
        if ch in "\"'" or text.startswith('@"', i):
            i = _skip_literal(text, i)
            continue
        if ch == open_ch:
            depth += 1
        elif ch == close_ch:
            depth -= 1
            if depth == 0:
                return i + 1
        i += 1
    return -1


def _skip_trivia_and_attributes(text: str, i: int) -> int:
    """Skip whitespace, // and /* */ comments, and [attribute] blocks."""
    while i < len(text):
        if text[i].isspace():
            i += 1
        elif text.startswith("//", i):
            nl = text.find("\n", i)
            i = len(text) if nl == -1 else nl + 1
        elif text.startswith("/*", i):
            end = text.find("*/", i)
            i = len(text) if end == -1 else end + 2
        elif text[i] == "[":
            nxt = _match_bracket(text, i, "[", "]")
            if nxt == -1:
                return -1
            i = nxt
        else:
            return i
    return i


def iter_declarations(text: str):
    """Yield (docs, name, signature, offset) for every doc-commented declaration."""
    for run in DOC_RUN_RE.finditer(text):
        docs = run.group(0)
        i = _skip_trivia_and_attributes(text, run.end())
        if i == -1 or i >= len(text):
            continue

        # Walk forward to the '(' that opens the parameter list. Angle brackets
        # are tracked so `Task<(int, string)> Foo(` does not stop early, and a
        # `;` or `{` before any '(' means this is a property/class, not a method.
        j = i
        angle = 0
        limit = min(len(text), i + MAX_HEADER_SCAN)
        open_paren = -1
        while j < limit:
            ch = text[j]
            if ch in "\"'" or text.startswith('@"', j):
                j = _skip_literal(text, j)
                continue
            if ch == "<":
                angle += 1
            elif ch == ">":
                angle = max(0, angle - 1)
            elif ch == "(":
                if angle == 0:
                    open_paren = j
                    break
                # a tuple inside a generic return type: skip it wholesale
                nxt = _match_bracket(text, j, "(", ")")
                if nxt == -1:
                    break
                j = nxt
                continue
            elif ch in ";{}" and angle == 0:
                break
            elif (
                angle == 0
                and text.startswith("where", j)
                and not (j and (text[j - 1].isalnum() or text[j - 1] == "_"))
                and not text[j + 5 : j + 6].isalnum()
            ):
                # A *type's* constraint clause; a method's `where` follows its
                # parameter list, so reaching one first means this is a type.
                break
            j += 1

        if open_paren == -1:
            continue

        header = text[i:open_paren]
        if not HEADER_SANITY_RE.match(header):
            continue
        if not PUBLIC_RE.search(header):
            continue
        name_match = NAME_RE.search(header)
        if not name_match:
            continue

        close_paren = _match_bracket(text, open_paren, "(", ")")
        if close_paren == -1:
            continue
        sig = text[open_paren + 1 : close_paren - 1]

        # Must actually be a declaration: `where` clause, then =>, { or ;
        tail = text[close_paren : close_paren + 400]
        tail = re.sub(r"^\s*where\s+[^{;=]+", "", tail)
        if not re.match(r"\s*(=>|\{|;)", tail):
            continue

        name = name_match.group("name")
        if name in RESERVED_NAMES:
            continue
        if name_match.group("generic"):
            name += name_match.group("generic")
        yield docs, name, sig, i


def split_top_level(signature: str) -> list[str]:
    """Split a parameter list on commas that are not nested in <>, () or []."""
    parts: list[str] = []
    depth = 0
    start = 0
    i = 0
    while i < len(signature):
        ch = signature[i]
        if ch in "\"'" or signature.startswith('@"', i):
            i = _skip_literal(signature, i)
            continue
        if ch in "<([":
            depth += 1
        elif ch in ">)]":
            depth = max(0, depth - 1)
        elif ch == "," and depth == 0:
            parts.append(signature[start:i])
            start = i + 1
        i += 1
    parts.append(signature[start:])
    return parts


def parse_param_names(signature: str) -> list[str]:
    names: list[str] = []
    for part in split_top_level(signature):
        part = part.strip()
        if not part:
            continue
        part = re.sub(r"\[[^\]]*\]", "", part)
        # Drop the default value; `= default(CancellationToken)` may hold a comma.
        part = part.split("=")[0].strip()
        part = re.sub(r"^this\s+", "", part)
        tokens = part.split()
        if tokens:
            names.append(tokens[-1].lstrip("@"))
    return names


def check_file(path: Path) -> tuple[list[str], int]:
    text = path.read_text(encoding="utf-8")
    rel = path.relative_to(ROOT).as_posix()
    issues: list[str] = []
    scanned = 0

    for docs, method, sig, offset in iter_declarations(text):
        scanned += 1
        if "<summary>" not in docs:
            continue

        param_names = parse_param_names(sig)
        if not param_names:
            continue

        doc_params = re.findall(r'<param name="([^"]+)"', docs)
        line = text[:offset].count("\n") + 1

        if not doc_params:
            issues.append(
                f"{rel}:{line}: {method} has <summary> but no <param> tags "
                f"({len(param_names)} parameters)"
            )
            continue

        missing = [name for name in param_names if name not in doc_params]
        extra = [name for name in doc_params if name not in param_names]
        if missing:
            issues.append(
                f"{rel}:{line}: {method} missing <param> for: {', '.join(missing)}"
            )
        if extra:
            issues.append(
                f"{rel}:{line}: {method} has undocumented extra <param> tags: "
                f"{', '.join(extra)}"
            )

    return issues, scanned


def main() -> int:
    if not CLIENT_ROOT.is_dir():
        print(
            f"XML param doc check failed: client root not found at {CLIENT_ROOT}",
            file=sys.stderr,
        )
        return 1

    issues: list[str] = []
    files_scanned = 0
    declarations = 0
    for path in sorted(CLIENT_ROOT.rglob("*.cs")):
        if not is_scoped(path):
            continue
        files_scanned += 1
        file_issues, scanned = check_file(path)
        issues.extend(file_issues)
        declarations += scanned

    # A path/layout drift must fail loudly rather than silently pass forever.
    if files_scanned == 0:
        print(
            "XML param doc check failed: scanned 0 files under "
            f"{CLIENT_ROOT} matching {', '.join(SCOPED_PREFIXES)}. "
            "The check is misconfigured (did the layout move?).",
            file=sys.stderr,
        )
        return 1

    if not issues:
        print(
            f"XML param docs OK: {declarations} declaration(s) in "
            f"{files_scanned} file(s) matching {', '.join(SCOPED_PREFIXES)}"
        )
        return 0

    print(
        f"XML param doc check failed ({len(issues)} issue(s) across "
        f"{declarations} declaration(s) in {files_scanned} file(s)):",
        file=sys.stderr,
    )
    for issue in issues:
        print(f"  {issue}", file=sys.stderr)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
