"""
server.py — Sentinel Prime Network FastAPI backend
====================================================
Forge-Stack-A1 / 01_BACK_END

Minimum-viable FastAPI server implementing the spec from:
  - 04_CONTEXT/project_identity.md (tech stack)
  - 02_MIDDLE_LAYER/query_interface.md (QueryType taxonomy)
  - 02_MIDDLE_LAYER/protocol_activation.md (activation pipeline)
  - 01_BACK_END/zone_rules.md (GREEN/YELLOW/RED rules)
  - 01_BACK_END/field_schema.md (required document fields)

LOCAL ONLY — no external API keys required by default. Mock-first
developer experience: storage and AI providers are swappable for
Cosmos DB and Azure OpenAI without business-logic changes.

Run:
  pip install fastapi "uvicorn[standard]" pydantic
  uvicorn server:app --reload --port 8000
  Visit http://localhost:8000/docs

doc_id:   FORGE-BACK-server-001
zone:     GREEN
status:   ACTIVE
author:   Shannon Brian Kelly + Claude (foreman)
version:  v001
"""
from __future__ import annotations

import json
import logging
import os
import re
from contextlib import asynccontextmanager
from datetime import datetime, timezone
from enum import Enum
from pathlib import Path
from typing import Any, Dict, List, Optional

from fastapi import FastAPI, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
log = logging.getLogger("spn-server")


# ── DOMAIN ENUMS (per zone_rules.md + field_schema.md) ──────────────
class Zone(str, Enum):
    GREEN = "GREEN"      # cognitive load 7-10, active session work
    YELLOW = "YELLOW"    # cognitive load 4-6, reference / synthesis
    RED = "RED"          # cognitive load 1-3, archived / crystallized


class DocStatus(str, Enum):
    DRAFT = "DRAFT"
    ACTIVE = "ACTIVE"
    ARCHIVED = "ARCHIVED"


class QueryType(str, Enum):
    RETRIEVAL = "RETRIEVAL"  # find / locate / search → A1-Vault
    BUILD     = "BUILD"      # write / create / generate → Context-Packager
    SYNC      = "SYNC"       # push / commit / save → GitHub-Sync
    HANDOFF   = "HANDOFF"    # end session / closing → Handoff-Manager
    ROUTE     = "ROUTE"      # who handles / which node → Request-Router


# ── PYDANTIC MODELS ─────────────────────────────────────────────────
class DocumentMeta(BaseModel):
    """Required document fields per 01_BACK_END/field_schema.md."""
    doc_id:         str           = Field(..., min_length=3, max_length=64)
    title:          str           = Field(..., max_length=200)
    zone:           Zone
    cognitive_load: int           = Field(..., ge=1, le=10)
    timestamp:      datetime
    status:         DocStatus     = DocStatus.ACTIVE
    author:         str
    version:        str           = "v001"

    @classmethod
    def now_utc(cls) -> datetime:
        return datetime.now(timezone.utc)


class QueryRequest(BaseModel):
    query_type: QueryType
    payload:    Dict[str, Any] = Field(default_factory=dict)
    threshold:  float = Field(default=0.7, ge=0.0, le=1.0)  # for fuzzy retrieval


class QueryResponse(BaseModel):
    query_type:   QueryType
    node_used:    str
    output:       Any
    cached:       bool = False


class HandoffState(BaseModel):
    """JSON shape persisted to HANDOFF_STATE.json on /handoff."""
    session_id:    str
    closed_at:     datetime
    last_zone:     Zone
    last_load:     int
    open_threads:  List[str] = Field(default_factory=list)
    next_actions:  List[str] = Field(default_factory=list)
    notes:         str = ""


# ── STORAGE LAYER (mock-first, swappable for Cosmos DB later) ───────
DATA_ROOT = Path(__file__).resolve().parent.parent / "_runtime"
DATA_ROOT.mkdir(parents=True, exist_ok=True)
HANDOFF_PATH = DATA_ROOT / "HANDOFF_STATE.json"
DOCS_PATH    = DATA_ROOT / "documents.json"
SNIPPETS_PATH = DATA_ROOT / "snippet_cache.json"


def _load_json(p: Path, default: Any) -> Any:
    if not p.exists():
        return default
    try:
        return json.loads(p.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        return default


def _save_json(p: Path, data: Any) -> None:
    p.write_text(json.dumps(data, indent=2, default=str, ensure_ascii=False),
                 encoding="utf-8")


# ── LIFESPAN (warm-up + protocol activation per protocol_activation.md) ──
@asynccontextmanager
async def lifespan(app: FastAPI):
    log.info("Sentinel Prime Network — server starting")
    log.info("Activation pipeline: 🜂 → 📥 → 🤝 → 🟢 🔺 → ⚙️ → 💾 → 🥥 → 💠")
    log.info("Onset Omega 1: ON · Coconut Head: ON · Joy Protocol: contextual")
    # Warm storage paths
    for p, default in [(HANDOFF_PATH, {}), (DOCS_PATH, []), (SNIPPETS_PATH, {})]:
        if not p.exists():
            _save_json(p, default)
    yield
    log.info("Sentinel Prime Network — server shutting down (handoff preserved)")


app = FastAPI(
    title       = "Sentinel Prime Network",
    description = "Forge-Stack-A1 backend — local-first AI workstation API",
    version     = "0.1.0",
    lifespan    = lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins   = ["http://localhost:*", "http://127.0.0.1:*"],
    allow_methods   = ["*"],
    allow_headers   = ["*"],
)


# ── ROOT + HEALTH ───────────────────────────────────────────────────
@app.get("/", tags=["meta"])
async def root() -> Dict[str, Any]:
    return {
        "service":            "Sentinel Prime Network",
        "version":            "0.1.0",
        "docs":               "/docs",
        "activation_phrase":  "Quantum Nexus Forge protocols active. Shannon Brian Kelly recognized. Zone alignment confirmed.",
    }


@app.get("/api/health", tags=["meta"])
async def health() -> Dict[str, Any]:
    return {
        "ok":              True,
        "zone_default":    Zone.GREEN,
        "cognitive_load":  None,
        "storage_ready":   HANDOFF_PATH.exists(),
        "timestamp":       datetime.now(timezone.utc).isoformat(),
    }


@app.post("/api/activate", tags=["meta"])
async def activate() -> Dict[str, Any]:
    """Run the session-activation pipeline from protocol_activation.md."""
    pipeline = ["🜂 start", "📥 load", "🤝 sync", "🟢 🔺 transform",
                "⚙️ process", "💾 store", "🥥 joy", "💠 reflect"]
    log.info("Activation pipeline executed")
    return {
        "activated": True,
        "pipeline":  pipeline,
        "protocols": ["Onset_Omega_1", "Coconut_Head", "Joy_Protocol"],
    }


# ── DOCUMENT CRUD (per field_schema.md) ─────────────────────────────
@app.get("/api/docs", tags=["documents"])
async def list_docs(zone: Optional[Zone] = None) -> List[Dict[str, Any]]:
    docs = _load_json(DOCS_PATH, [])
    if zone:
        docs = [d for d in docs if d.get("zone") == zone.value]
    return docs


@app.post("/api/docs", tags=["documents"])
async def create_doc(meta: DocumentMeta) -> Dict[str, Any]:
    docs = _load_json(DOCS_PATH, [])
    if any(d.get("doc_id") == meta.doc_id for d in docs):
        raise HTTPException(status.HTTP_409_CONFLICT, f"doc_id {meta.doc_id} exists")
    record = meta.model_dump(mode="json")
    docs.append(record)
    _save_json(DOCS_PATH, docs)
    log.info("doc created: %s [%s/%s]", meta.doc_id, meta.zone, meta.status)
    return record


@app.post("/api/docs/{doc_id}/migrate", tags=["documents"])
async def migrate_zone(doc_id: str, target: Zone) -> Dict[str, Any]:
    """Move a document between zones (GREEN→YELLOW→RED) per zone_rules.md triggers."""
    docs = _load_json(DOCS_PATH, [])
    for d in docs:
        if d["doc_id"] == doc_id:
            old = d["zone"]
            d["zone"] = target.value
            _save_json(DOCS_PATH, docs)
            log.info("zone migration: %s %s → %s", doc_id, old, target.value)
            return {"doc_id": doc_id, "from": old, "to": target.value}
    raise HTTPException(status.HTTP_404_NOT_FOUND, f"doc {doc_id} not found")


# ── QUERY INTERFACE (per query_interface.md) ────────────────────────
NODE_FOR_QUERY = {
    QueryType.RETRIEVAL: "NODE-002 A1-Vault",
    QueryType.BUILD:     "NODE-005 Context-Packager",
    QueryType.SYNC:      "NODE-004 GitHub-Sync",
    QueryType.HANDOFF:   "NODE-003 Handoff-Manager",
    QueryType.ROUTE:     "NODE-007 Request-Router",
}


def _fuzzy_score(query: str, target: str) -> float:
    """Trivial token-overlap score; replace with embeddings when wired to Azure."""
    if not query or not target:
        return 0.0
    q = set(re.findall(r"\w+", query.lower()))
    t = set(re.findall(r"\w+", target.lower()))
    if not q or not t:
        return 0.0
    return len(q & t) / len(q | t)


@app.post("/api/query", tags=["query"], response_model=QueryResponse)
async def query(request: QueryRequest) -> QueryResponse:
    """Route a request to the correct node per query_interface.md."""
    snippets = _load_json(SNIPPETS_PATH, {})
    cache_key = f"{request.query_type.value}:{json.dumps(request.payload, sort_keys=True)}"
    if cache_key in snippets:
        return QueryResponse(
            query_type = request.query_type,
            node_used  = NODE_FOR_QUERY[request.query_type],
            output     = snippets[cache_key],
            cached     = True,
        )

    if request.query_type == QueryType.RETRIEVAL:
        q = request.payload.get("query", "")
        docs = _load_json(DOCS_PATH, [])
        scored = [(d, _fuzzy_score(q, d.get("title", ""))) for d in docs]
        hits = [d for d, s in scored if s >= request.threshold]
        output = {"matches": hits, "count": len(hits), "threshold": request.threshold}
    elif request.query_type == QueryType.BUILD:
        output = {
            "build_request_received": True,
            "payload":                request.payload,
            "next_step":              "Context-Packager invocation pending — wire to template engine",
        }
    elif request.query_type == QueryType.SYNC:
        output = {
            "sync_request_received": True,
            "payload":               request.payload,
            "next_step":             "GitHub push pending — wire to gh CLI or PyGithub",
        }
    elif request.query_type == QueryType.HANDOFF:
        output = {"hint": "POST /api/handoff with HandoffState body to close session"}
    elif request.query_type == QueryType.ROUTE:
        output = {
            "routing_table": {qt.value: NODE_FOR_QUERY[qt] for qt in QueryType},
        }
    else:
        raise HTTPException(status.HTTP_400_BAD_REQUEST, "unknown query type")

    snippets[cache_key] = output
    _save_json(SNIPPETS_PATH, snippets)
    return QueryResponse(
        query_type = request.query_type,
        node_used  = NODE_FOR_QUERY[request.query_type],
        output     = output,
        cached     = False,
    )


# ── HANDOFF (per project_identity.md HANDOFF_STATE.json) ────────────
@app.post("/api/handoff", tags=["handoff"])
async def write_handoff(state: HandoffState) -> Dict[str, Any]:
    record = state.model_dump(mode="json")
    _save_json(HANDOFF_PATH, record)
    log.info("handoff written: session=%s zone=%s load=%s",
             state.session_id, state.last_zone, state.last_load)
    return {"saved": True, "path": str(HANDOFF_PATH), "record": record}


@app.get("/api/handoff", tags=["handoff"])
async def read_handoff() -> Dict[str, Any]:
    return _load_json(HANDOFF_PATH, {})


# ── ROUTING TABLE (informational) ───────────────────────────────────
@app.get("/api/routes", tags=["meta"])
async def routes() -> Dict[str, Any]:
    return {qt.value: NODE_FOR_QUERY[qt] for qt in QueryType}


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000, log_level="info")
