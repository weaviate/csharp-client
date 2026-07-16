#!/usr/bin/env python3
"""Fail when public query/generate methods document a summary but not their parameters.

CS1573 only fires when some parameters are documented but not all; CS1591 only
fires when the method has no doc comment at all. Methods with a <summary> and
zero <param> tags (Hybrid before this fix) slip through both. This check closes
that gap for the query/generate client surface.
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

METHOD_RE = re.compile(
    r"(?P<docs>(?:[ \t]*///.*\n)+)\s*"
    r"(?:(?:\s*\[[^\]]+\]\s*)*)"
    r"(?P<mods>public\s+(?:static\s+)?(?:async\s+)?[\w<>\[\],\.\?\s]+\s+)?"
    r"(?P<name>\w+)\s*\((?P<sig>[^)]*)\)"
    r"(?:\s*where\s+[^{;]+)?\s*(?:=>|\{|;)",
    re.MULTILINE,
)


def is_scoped(path: Path) -> bool:
    rel = path.relative_to(CLIENT_ROOT).as_posix()
    return any(rel.startswith(prefix) for prefix in SCOPED_PREFIXES)


def parse_param_names(signature: str) -> list[str]:
    names: list[str] = []
    for part in signature.split(","):
        part = part.strip()
        if not part:
            continue
        part = re.sub(r"\[[^\]]*\]", "", part)
        part = part.split("=")[0].strip()
        part = re.sub(r"^this\s+", "", part)
        tokens = part.split()
        if tokens:
            names.append(tokens[-1].lstrip("@"))
    return names


def check_file(path: Path) -> list[str]:
    text = path.read_text(encoding="utf-8")
    rel = path.relative_to(ROOT).as_posix()
    issues: list[str] = []

    for match in METHOD_RE.finditer(text):
        docs = match.group("docs")
        if "<summary>" not in docs:
            continue

        param_names = parse_param_names(match.group("sig"))
        if not param_names:
            continue

        doc_params = re.findall(r'<param name="([^"]+)"', docs)
        line = text[: match.start()].count("\n") + 1
        method = match.group("name")

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

    return issues


def main() -> int:
    issues: list[str] = []
    for path in sorted(CLIENT_ROOT.rglob("*.cs")):
        if not is_scoped(path):
            continue
        issues.extend(check_file(path))

    if not issues:
        print(
            "XML param docs OK for query/generate client methods "
            f"({', '.join(SCOPED_PREFIXES)})"
        )
        return 0

    print(f"XML param doc check failed ({len(issues)} issue(s)):", file=sys.stderr)
    for issue in issues:
        print(f"  {issue}", file=sys.stderr)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
