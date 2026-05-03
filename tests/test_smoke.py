"""
Smoke test suite for canon #2 — Sentinel Prime Network (Forge-Stack-A1).

Verifies the repo's structural promise per the README:
  - 01_BACK_END/ FastAPI service (server.py + supporting docs)
  - 02_MIDDLE_LAYER/ orchestration tier
  - 03_FRONT_END/ presentation tier
  - 04_CONTEXT/ shared context + identity files
  - LICENSE + README + identity convention ('Sentinel Prime Network')
"""
from pathlib import Path

ROOT = Path(__file__).parent.parent


class TestRepoStructure:
    def test_readme_exists(self):
        assert (ROOT / "README.md").exists()

    def test_readme_has_h1(self):
        c = (ROOT / "README.md").read_text(encoding="utf-8")
        assert c.strip().startswith("#")

    def test_readme_mentions_sentinel_prime(self):
        c = (ROOT / "README.md").read_text(encoding="utf-8").lower()
        assert "sentinel prime" in c

    def test_readme_mentions_forge_stack_a1(self):
        """README should explain that 'Forge-Stack-A1' is internal-only, not public."""
        c = (ROOT / "README.md").read_text(encoding="utf-8")
        assert "Forge-Stack-A1" in c

    def test_license_exists(self):
        assert (ROOT / "LICENSE").exists()

    def test_license_is_mit(self):
        c = (ROOT / "LICENSE").read_text(encoding="utf-8")
        assert "MIT" in c


class TestArchitectureFolders:
    """Verify each tier promised in the README exists."""

    def test_back_end_folder_exists(self):
        d = ROOT / "01_BACK_END"
        assert d.exists() and d.is_dir()

    def test_middle_layer_folder_exists(self):
        d = ROOT / "02_MIDDLE_LAYER"
        assert d.exists() and d.is_dir()

    def test_front_end_folder_exists(self):
        d = ROOT / "03_FRONT_END"
        assert d.exists() and d.is_dir()

    def test_context_folder_exists(self):
        d = ROOT / "04_CONTEXT"
        assert d.exists() and d.is_dir()

    def test_each_tier_has_content(self):
        for tier in ("01_BACK_END", "02_MIDDLE_LAYER", "03_FRONT_END", "04_CONTEXT"):
            d = ROOT / tier
            files = list(d.iterdir())
            assert files, f"{tier} is empty"


class TestBackendModule:
    def test_server_module_present(self):
        assert (ROOT / "01_BACK_END" / "server.py").exists()

    def test_server_module_nonempty(self):
        size = (ROOT / "01_BACK_END" / "server.py").stat().st_size
        assert size > 100, f"server.py too small ({size} bytes)"


class TestSupplementaryDocs:
    def test_integration_memo_present(self):
        candidates = list(ROOT.glob("INTEGRATION_MEMO*"))
        assert candidates, "INTEGRATION_MEMO file missing"

    def test_polish_notes_present(self):
        assert (ROOT / "POLISH_NOTES.md").exists()

    def test_technical_blueprint_present(self):
        candidates = list(ROOT.glob("Technical blueprint*"))
        assert candidates, "Technical blueprint file missing"

    def test_docs_folder_exists(self):
        assert (ROOT / "docs").exists() and (ROOT / "docs").is_dir()


class TestRepoCleanliness:
    def test_no_pycache_in_root(self):
        assert not (ROOT / "__pycache__").exists()

    def test_no_stray_pyc_files(self):
        pycs = [p for p in ROOT.rglob("*.pyc") if "__pycache__" not in str(p)]
        assert not pycs, f"found stray .pyc files: {pycs}"

    def test_no_env_files_committed(self):
        envs = list(ROOT.rglob(".env"))
        assert not envs, f"committed .env files: {envs}"
