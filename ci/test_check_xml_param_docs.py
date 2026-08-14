#!/usr/bin/env python3
"""Self-tests for check_xml_param_docs.py.

Dependency-free (no pytest in this repo): run with `python3 ci/test_check_xml_param_docs.py`.
Each case pins one of the defects the checker previously had.
"""

from __future__ import annotations

import importlib.util
import io
import re
import sys
import tempfile
from contextlib import redirect_stderr, redirect_stdout
from pathlib import Path

HERE = Path(__file__).resolve().parent

_spec = importlib.util.spec_from_file_location("chk", HERE / "check_xml_param_docs.py")
assert _spec is not None, "could not build a module spec for check_xml_param_docs.py"
assert _spec.loader is not None, "module spec for check_xml_param_docs.py has no loader"
chk = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(chk)

FAILURES: list[str] = []


def check(label: str, condition: bool, detail: str = "") -> None:
    if condition:
        print(f"  PASS  {label}")
    else:
        print(f"  FAIL  {label}{(': ' + detail) if detail else ''}")
        FAILURES.append(label)


def decls(source: str):
    return list(chk.iter_declarations(source))


# ---------------------------------------------------------------- defect 1
# Generic methods were invisible: `(?P<name>\w+)\s*\(` cannot match `Hybrid<T>(`.
GENERIC_SRC = """
public static class Ext
{
    /// <summary>
    /// Performs a hybrid search.
    /// </summary>
    /// <param name="client">The client</param>
    public static Task<int> Hybrid<T>(this Client client, string query)
    {
        return null;
    }
}
"""


def test_generic_methods() -> None:
    print("defect 1 - generic methods are scanned")
    found = decls(GENERIC_SRC)
    names = [d[1] for d in found]
    check("Hybrid<T> is discovered", "Hybrid<T>" in names, f"got {names}")
    # and, being discovered, its missing param is reported
    with tempfile.TemporaryDirectory() as td:
        p = Path(td) / "QueryClient.Generic.cs"
        p.write_text(GENERIC_SRC, encoding="utf-8")
        issues, _scanned = chk.check_file(p, root=Path(td))
    check(
        "undocumented 'query' on the generic overload is reported",
        any("missing <param> for: query" in i for i in issues),
        f"got {issues}",
    )


# ---------------------------------------------------------------- defect 2
# Splitting the parameter list on every comma turned `Dictionary<string, object>`
# into a phantom parameter named `Dictionary<string` that no doc tag can satisfy.
def test_generic_parameter_types() -> None:
    print("defect 2 - commas inside generic type arguments")
    names = chk.parse_param_names(
        "Dictionary<string, object> filters, IList<KeyValuePair<int, string>> pairs, int limit"
    )
    check(
        "generic args do not create phantom parameters",
        names == ["filters", "pairs", "limit"],
        f"got {names}",
    )
    check(
        "no phantom 'Dictionary<string' parameter",
        not any("<" in n for n in names),
        f"got {names}",
    )


# ---------------------------------------------------------------- defect 3
# Zero scanned files printed OK and exited 0, so any path drift silently
# disabled the gate forever.
def test_empty_scan_fails() -> None:
    print("defect 3 - an empty scan fails loudly")
    with tempfile.TemporaryDirectory() as td:
        empty = Path(td) / "src" / "Weaviate.Client"
        empty.mkdir(parents=True)
        err = io.StringIO()
        with redirect_stdout(io.StringIO()), redirect_stderr(err):
            rc = chk.main(root=Path(td), client_root=empty)
    check("exit code is non-zero when 0 files are scanned", rc != 0, f"rc={rc}")
    check(
        "message explains the misconfiguration",
        "scanned 0 files" in err.getvalue(),
        f"stderr={err.getvalue()!r}",
    )


def test_missing_client_root_fails() -> None:
    print("defect 3b - a missing client root fails loudly")
    with tempfile.TemporaryDirectory() as td:
        err = io.StringIO()
        with redirect_stdout(io.StringIO()), redirect_stderr(err):
            rc = chk.main(root=Path(td), client_root=Path(td) / "does" / "not" / "exist")
    check("exit code is non-zero when the client root is absent", rc != 0, f"rc={rc}")
    check(
        "message names the missing root",
        "client root not found" in err.getvalue(),
        f"stderr={err.getvalue()!r}",
    )


# ---------------------------------------------------------------- defect 4
# Nested parens in a default value, a tuple return type, and parens/brackets
# inside an attribute string all defeated the `[^)]*` signature capture.
NESTED_SRC = """
public class C
{
    /// <summary>Does a thing.</summary>
    /// <param name="query">The query</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<int> WithDefaultParen(
        string query,
        CancellationToken cancellationToken = default(CancellationToken)
    )
    {
        return null;
    }

    /// <summary>Does a thing.</summary>
    /// <param name="query">The query</param>
    public Task<(int Count, string Name)> WithTupleReturn(string query)
    {
        return null;
    }

    /// <summary>Does a thing.</summary>
    /// <param name="query">The query</param>
    [Obsolete("use Other(x) instead [see docs]")]
    public Task<int> WithTrickyAttribute(string query)
    {
        return null;
    }
}
"""


def test_nested_parens_and_attributes() -> None:
    print("defect 4 - nested parens, tuple returns, tricky attributes")
    found = {d[1]: d[2] for d in decls(NESTED_SRC)}
    check(
        "method with `= default(CancellationToken)` is scanned",
        "WithDefaultParen" in found,
        f"got {sorted(found)}",
    )
    if "WithDefaultParen" in found:
        names = chk.parse_param_names(found["WithDefaultParen"])
        check(
            "its parameters parse correctly",
            names == ["query", "cancellationToken"],
            f"got {names}",
        )
    check(
        "method with a tuple return type is scanned",
        "WithTupleReturn" in found,
        f"got {sorted(found)}",
    )
    check(
        "method behind an attribute containing parens/brackets in a string is scanned",
        "WithTrickyAttribute" in found,
        f"got {sorted(found)}",
    )


# ------------------------------------------------- regression: no bad parses
TYPE_CONSTRAINT_SRC = """
/// <summary>
/// The typed query client
/// </summary>
public partial class TypedQueryClient<T>
    where T : class, new()
{
}
"""


def test_type_constraint_is_not_a_method() -> None:
    print("regression - a type's `where T : class, new()` is not a method")
    names = [d[1] for d in decls(TYPE_CONSTRAINT_SRC)]
    check("no phantom `new` declaration", "new" not in names, f"got {names}")


def test_real_tree_is_clean() -> None:
    print("integration - the real client tree passes")
    out = io.StringIO()
    with redirect_stdout(out), redirect_stderr(out):
        rc = chk.main()
    text = out.getvalue().strip()
    check("checker exits 0 on the current tree", rc == 0, text)
    m = re.search(r"(\d+) declaration\(s\) in (\d+) file\(s\)", text)
    check("it reports what it scanned", m is not None, text)
    if m:
        check(
            "it scanned a non-trivial number of declarations",
            int(m.group(1)) > 100 and int(m.group(2)) > 10,
            text,
        )
        print(f"        -> {m.group(1)} declarations in {m.group(2)} files")


def main() -> int:
    for fn in (
        test_generic_methods,
        test_generic_parameter_types,
        test_empty_scan_fails,
        test_missing_client_root_fails,
        test_nested_parens_and_attributes,
        test_type_constraint_is_not_a_method,
        test_real_tree_is_clean,
    ):
        fn()
    print()
    if FAILURES:
        print(f"FAILED ({len(FAILURES)}): {', '.join(FAILURES)}")
        return 1
    print("all self-tests passed")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
