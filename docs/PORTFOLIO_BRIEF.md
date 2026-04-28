# Portfolio Brief — Sentinel Prime Network

> **One-page recruiter overview.** Architecture detail lives in [`../README.md`](../README.md). Publication changelog lives in [`../POLISH_NOTES.md`](../POLISH_NOTES.md).

## TL;DR

**Local-first AI workstation and orchestration scaffold** built as a three-tier
stack: FastAPI backend, protocol/middleware layer, and front-end visualization
layer. The current MVP demonstrates structured routing, handoff persistence,
document-zone migration, and a standalone React visualization component for the
Quantum Nexus cognitive lattice.

## Naming

Canonical public name: **Sentinel Prime Network**.
Approved short name: `SPN`.
`Forge-Stack-A1` is the internal stack/workspace label for this build and should not be used as the public project name.

## Role demonstrated

**AI Platform Engineer / AI Tooling Builder** — backend API design, developer
workflow shaping, orchestration-layer thinking, and documentation discipline for
multi-agent collaboration.

## What this project demonstrates

| Capability | Evidence in the codebase |
|---|---|
| **FastAPI service design** | `01_BACK_END/server.py` implements health, activation, document, query, and handoff endpoints |
| **Three-zone memory workflow** | `01_BACK_END/zone_rules.md .txt` and the document migration routes define GREEN / YELLOW / RED movement |
| **Protocol-driven orchestration** | `02_MIDDLE_LAYER/` contains protocol activation, query routing, handoff rules, and pipeline definitions |
| **Frontend system thinking** | `03_FRONT_END/QuantumNexusSimulation.jsx` plus `dashboard_spec.md.txt` define the visualization layer |
| **Cross-session continuity** | `HANDOFF_STATE.json` persistence is part of the service contract exposed by `/api/handoff` |

## Honest scope statement

This project is an **MVP scaffold**, not a finished production deployment.

- Local FastAPI backend: present
- Query and handoff surface: present
- React visualization component: present
- Cosmos DB integration: planned, not started
- Azure OpenAI integration: planned, not started
- FastMCP wrapper: planned, not started

That makes it useful as a portfolio piece for **platform architecture and
orchestration design**, while staying honest about the implementation stage.

## Why it matters in the broader portfolio

This project sits between the lighter proof-of-concept systems and the more
production-graduated portfolio pieces:

- `Sentinel-of-sentinel-s-Forge` shows enterprise-grade feature integration
- `Quantum Nexus Forge` shows a smaller working orchestration MVP
- `Sentinel Prime Network` shows the reusable platform scaffold; `Forge-Stack-A1` is the internal stack label

Together, those three artifacts show the same architectural instincts at
different levels of maturity.

## Author

**Shannon Brian Kelly**  
Healthcare CNA -> AI Systems Developer transition  
Built in collaboration with Claude AI and Codex
