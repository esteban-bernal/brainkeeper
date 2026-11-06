#!/usr/bin/env python3
"""Valida el frontmatter de notas Markdown contra schemas/<type>.schema.json.

Uso:
  python3 validate.py <ruta-a-nota.md> [...]
  python3 validate.py --all <ruta-al-vault>

Salida: una línea por nota, OK o los errores. Código de salida 1 si alguna falla.
Equivalente temporal del hook pre-write del motor .NET (Fase 1a).
"""
import sys, json, pathlib, re
try:
    import yaml, jsonschema
except ImportError:
    sys.exit("pip install pyyaml jsonschema")

HERE = pathlib.Path(__file__).parent
SCHEMAS = {p.stem.replace(".schema", ""): json.load(open(p)) for p in HERE.glob("*.schema.json")}
FM = re.compile(r"^---\n(.*?)\n---\n", re.S)

def check(path: pathlib.Path) -> list[str]:
    text = path.read_text(encoding="utf-8")
    m = FM.match(text)
    if not m:
        return ["sin frontmatter"]
    try:
        fm = yaml.safe_load(m.group(1)) or {}
    except yaml.YAMLError as e:
        return [f"YAML inválido: {e}"]
    for k in ("created", "updated", "deadline", "revisit"):
        if k in fm and hasattr(fm[k], "isoformat"):
            fm[k] = fm[k].isoformat()
    t = fm.get("type")
    if t not in SCHEMAS:
        return [f"type desconocido: {t!r}"]
    v = jsonschema.Draft202012Validator(SCHEMAS[t], format_checker=jsonschema.FormatChecker())
    errs = [f"{'/'.join(map(str, e.path)) or '(raíz)'}: {e.message}" for e in v.iter_errors(fm)]
    if fm.get("layer") == "wiki" and "## Mi postura" not in text:
        errs.append("página de wiki sin sección '## Mi postura'")
    return errs

def main(argv):
    if argv[:1] == ["--all"]:
        root = pathlib.Path(argv[1])
        files = [p for p in root.rglob("*.md") if not any(s in p.parts for s in ("raw", "index", "archive", "core", ".obsidian", ".templates")) and p.name not in ("README.md", "CLAUDE.md", "gaps.md")]
    else:
        files = [pathlib.Path(a) for a in argv]
    bad = 0
    for f in files:
        errs = check(f)
        if errs:
            bad += 1
            print(f"FAIL {f}")
            for e in errs:
                print(f"   - {e}")
        else:
            print(f"OK   {f}")
    sys.exit(1 if bad else 0)

if __name__ == "__main__":
    main(sys.argv[1:])
