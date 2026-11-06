# Brainkeeper

**El segundo cerebro que no se degrada, con pruebas.**

Brainkeeper es un motor determinista en .NET que va debajo de cualquier agente (Claude Code, OpenClaw, Codex…) y garantiza lo que un prompt no puede garantizar sobre un vault de Markdown:

- Ninguna nota inválida entra (esquema por tipo, validado antes de tocar disco).
- Ninguna nota privada sale (visibilidad por nota, aislada por índice y permisos, con test).
- El contexto nunca crece: presupuesto fijo de tokens por skill, búsqueda jerárquica, nunca "cargar todo".
- Nada sube de capa sin aprobación humana.
- Ningún cambio de modelo o prompt se despliega si baja las evals.

El agente no se escribe: el motor se expone como **servidor MCP + hooks**. Las skills van en formato Agent Skills estándar.

## Estado

Fase 0 (noviembre 2026). Esqueleto de la solución, esquemas de nota y validador. Nada usable todavía.

## Estructura

```
src/
  Brainkeeper.Vault      lee/escribe Markdown, valida frontmatter, commit por lotes
  Brainkeeper.Indexer    índice jerárquico, grafo de enlaces, FTS5, embeddings
  Brainkeeper.Retriever  contexto con presupuesto por skill, abstención
  Brainkeeper.Promoter   paso entre capas con aprobación; consolidación determinista
  Brainkeeper.Linter     huérfanas, enlaces rotos, contradicciones, stale, privacidad
  Brainkeeper.Mcp        servidor MCP + hooks pre/post-write
  Brainkeeper.Api        worker: Telegram, scheduler, HTTP interno
tests/Brainkeeper.Tests
schemas/                 JSON Schema de los 7 tipos de nota + validador Python de transición
skills/                  SKILL.md de cada skill (formato Agent Skills)
evals/                   preguntas con respuesta conocida sobre un vault de prueba
docs/
```

## Empezar

```bash
./bootstrap.sh     # crea la solución .NET 10, añade paquetes, build + test
```

Requiere el SDK de .NET 10.

## Capas del vault

| Capa | Carpeta | Sube cuando |
| --- | --- | --- |
| 1 Captura | `inbox/` | se clasifica |
| 2 Diario | `daily/` | Esteban aprueba |
| 3 Wiki | `wiki/` | — |
| 4 Núcleo | `core/` | solo a mano |

Toda página de wiki termina en `## Mi postura`, que el motor prohíbe tocar al agente.

## Licencia

MIT.
