# Sentinel Prime Network

> **Local-first AI workstation backend** where Claude / Gemini / Copilot collaborate on a shared codebase under Shannon's direction.

## Project Identity

The canonical public name for this repo is **Sentinel Prime Network**.
The GitHub slug is `sentinel-prime-network`.
`Forge-Stack-A1` is the internal stack/workspace label for this build, not a separate public product name.

![Python](https://img.shields.io/badge/python-3.11+-blue.svg)
![FastAPI](https://img.shields.io/badge/framework-FastAPI-009688.svg)
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)
![Status](https://img.shields.io/badge/status-MVP-success)

---

## What this is

A three-tier AI orchestration stack:

- **`01_BACK_END/`** - FastAPI service (`server.py`) implementing the query interface, document store, zone migration, and handoff persistence
- **`02_MIDDLE_LAYER/`** - Protocol activation, query routing, and pipeline definitions
- **`03_FRONT_END/`** - `QuantumNexusSimulation.jsx` (React component) plus dashboard spec
- **`04_CONTEXT/`** - Glyphic Codex master, schemas, zone instructions, and project identity

## Quick Start

```bash
pip install fastapi "uvicorn[standard]" pydantic
cd 01_BACK_END
uvicorn server:app --reload --port 8000
```

Open <http://localhost:8000/docs> for the OpenAPI / Swagger UI.

## Endpoints

| Method | Path | Purpose |
|---|---|---|
| GET | `/` | Service identity plus activation phrase |
| GET | `/api/health` | Liveness check |
| POST | `/api/activate` | Run session activation pipeline |
| GET | `/api/docs` | List documents (filter by `?zone=GREEN/YELLOW/RED`) |
| POST | `/api/docs` | Create a document with full field-schema validation |
| POST | `/api/docs/{doc_id}/migrate` | Migrate document between zones |
| POST | `/api/query` | Route a request to the correct node |
| POST | `/api/handoff` | Persist `HANDOFF_STATE.json` |
| GET | `/api/handoff` | Read current handoff state |
| GET | `/api/routes` | Show the routing table |

## Three-Zone System

- **GREEN** - cognitive load 7-10, active session work
- **YELLOW** - cognitive load 4-6, reference and synthesis
- **RED** - cognitive load 1-3, archived and crystallized

Documents migrate down a tier when they stabilize. Migration triggers are codified in `01_BACK_END/zone_rules.md`.

## Query Interface

| QueryType | Trigger words | Node |
|---|---|---|
| RETRIEVAL | find, locate, search | NODE-002 A1-Vault |
| BUILD | write, create, generate | NODE-005 Context-Packager |
| SYNC | push, commit, save | NODE-004 GitHub-Sync |
| HANDOFF | end session, closing | NODE-003 Handoff-Manager |
| ROUTE | who handles, which node | NODE-007 Request-Router |

## Tech Stack

- **Backend:** Python 3.11+, FastAPI, Pydantic v2
- **Storage (current):** Local JSON files in `_runtime/` for mock-first DX
- **Storage (planned):** Azure Cosmos DB
- **AI Services (planned):** Azure OpenAI
- **Protocol Layer (planned):** FastMCP
- **Frontend:** React JSX plus HTML/CSS/JS dashboard

## Status

- `server.py` MVP complete with 10 endpoints
- `QuantumNexusSimulation.jsx` enhanced with 11 nodes and visualization effects
- Field schema, zone rules, and query interface are specified and implemented
- Cosmos DB integration: planned, not started
- Azure OpenAI integration: planned, not started
- FastMCP wrapper: planned, not started
- `TASK-020` dashboard HTML: planned, not started

## Project Documents

- `INTEGRATION_MEMO_a10_a2.md.txt` - synthesis of prior ChatGPT QNF session and JSX integration
- `04_CONTEXT/GLYPHIC_CODEX_MASTER.md.txt` - symbolic language reference
- `04_CONTEXT/project_identity.md.txt` - authoritative project identity note
- `04_CONTEXT/LOCAL_DB_SCHEMA.md.txt` - local DB schema
- `04_CONTEXT/architect_profile.md.txt` - Shannon Brian Kelly profile
- `04_CONTEXT/glossary.md.txt` - terminology

## Author

**Shannon Brian Kelly** - Coconut Head / The Architect.  
Healthcare CNA -> AI Systems Developer career transition.

Built in collaboration with Claude AI (Anthropic).

## License

MIT - see `LICENSE` (or sibling repo `neural-lattice-cognitive-architecture`).

---

*Sentinel Prime Network. Internal stack label: Forge-Stack-A1. Local. Private. Three-tier. Built for sustained sessions.*
