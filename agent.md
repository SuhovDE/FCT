# agent.md — Autonomous Build Plan for **AeroPulse**

> **Goal**
> Build a production-grade, feature-parity remake of “B Tactical” as **AeroPulse** using **Blazor + DevExpress** (UI) and **Entity Framework Core** (multi-database). Follow the attached specs and replicate user/business behavior exactly—no speculative features.

---

## 0) Sources of Truth

* `docs/backlog.md` — canonical, assignable tasks and acceptance criteria.
* `docs/functions.md` — functional requirements & UX behavior at parity.
* `docs/Doku_BTactical.pdf` — official user manual; resolves ambiguity.

**Interpretation rule:** If `backlog.md` and `functions.md` omit a detail, align with `Doku_BTactical.pdf`. Record any assumption you make in `docs/ASSUMPTIONS.md`.

---

## 1) Mission & Non-Goals

**Mission:** Reproduce the full workflow end-to-end:

1. Import schedules → 2) Create & edit scenarios → 3) Assign loads & rules →
2. Compute event-level forecasts → 5) Analyze with groups/filters →
3. Check constraints (runway/curfew) → 7) Compare scenarios → 8) Export/Publish.

**Non-Goals (initial):** No new ML, no external SSO or vendor integrations, no features beyond parity (hooks may exist but stay disabled).

---

## 2) Tech Stack & Architecture

* **Frontend:** Blazor **Server** with **DevExpress Blazor** (DataGrid, Pivot/TreeList behaviors, Charts, Editors).
* **Backend:** ASP.NET Core 9+, C#, Clean Architecture (Domain / Application / Infrastructure / Web).
* **Data:** EF Core 9+ with providers **SqlServer** and **Npgsql** first-class; optional **MySql** and **Sqlite** for dev/tests.
* **CI/CD:** dotnet build/test, EF migrations, format/lint, basic UI smoke (Playwright optional).
* **Telemetry:** Serilog structured logs; basic perf metrics.
* **i18n:** English default; string resources prepared for localization.

**Solution layout**

```
/src
  /AeroPulse.Web            (Blazor UI & minimal endpoints)
  /AeroPulse.Application    (use-cases, services, validators)
  /AeroPulse.Domain         (entities, value objects, domain events)
  /AeroPulse.Infrastructure (DbContext, EF configs, providers)
  /AeroPulse.Shared         (DTOs, contracts, primitives)
  /AeroPulse.Tests          (unit + integration)
/docs                       (backlog.md, functions.md, Doku_BTactical.pdf, ADRs)
/tools                      (migrate/seed scripts)
/migrations                 (provider snapshots)
```

---

## 3) Environment & Multi-DB

* `appsettings.json` keys:

  * `Database.Provider` = `SqlServer` | `Postgres` | `MySql` | `Sqlite`
  * `ConnectionStrings:Default`
* EF: avoid provider-specific SQL; use value converters for enums/JSON/time.
* Scripts:

  * `tools/migrate.sqlserver.ps1` / `tools/migrate.postgres.ps1`
  * `tools/seed.ps1` (masters + demo scenario)

---

## 4) Core Domain

**Reference masters**

* `Airline`(Id, Iata, Icao, Name, Category)
* `Airport`(Id, Iata, Icao, Name, Country, RegionCode, TimeZone)
* `AircraftType`(Id, Code, Seats)

**Scenarios & flights**

* `Scenario`(Id, Name, Description, StartDate, EndDate, Status, IsArchived, ParentScenarioId?, CreatedBy, CreatedAt)
* `Flight`(Id, ScenarioId, FlightNo, AirlineId, OriginId, DestinationId, DepUtc, ArrUtc, AircraftTypeId, FrequencyPattern, Flags: IsCancelled, Meta)

**Loads & results**

* `FlightLoad`(FlightId, LoadFactorPct, TransferPct, BagsPerPax, Overrides?)
* `ForecastResult`(FlightId, Seats, Pax, PaxTransfer, Bags, CalcAt)  // materialized or on demand

**Groups & filters**

* `Group`(Id, Name, Kind:System|User, Entity: Flight|Airline|Airport|Route, DefinitionJson?, Owner?)
* `GroupMember`(GroupId, FlightId?)  // or criteria DSL in `DefinitionJson`

**Constraints & events**

* `RunwayConstraintTemplate`(Id, Name, RulesJson)  // rolling windows, ARR/DEP split
* `CurfewTemplate`(Id, Name, QuietHoursJson)
* `SpecialEvent`(Id, Name, Start, End, EffectJson)  // load adjustments

---

## 5) Functional Modules & Acceptance (High-Level)

### A) Schedule Import

**Upload → Map → Validate → Preview diff → Commit**

* Stream parse SIR/SSIM/CSV (50–200k rows).
* Mapping presets; required fields enforced.
* Validate refs (airline/airport/aircraft); create placeholders if allowed.
* Duplicate detection & safe upsert; summary: new/updated/skipped/errors.

### B) Scenario Management & Versioning

* Create, duplicate (“Save As”), archive, delete; Explorer with grid.
* Duplicate copies flights, loads, groups; audit entries for every action.

### C) Flights Editor (FEG UX)

* Virtualized DevExpress grid (100k+); inline edit & bulk ops.
* Add/edit/remove; frequency & equipment mass changes (filtered subset).
* Validation: times, overlaps, required refs.

### D) Turnaround Linking

* Auto-link ARR→DEP per airline/aircraft min/max ground rules.
* Strategies: FIFO/LIFO; “keep manual” vs “override all”; visual pairing.

### E) Cancellation Tool

* Cancel % or absolute count by date range + group filter.
* Prefer unlinked; if linked, cancel pairs; summary & reproducibility.

### F) Load Assignment & Adjustment

* Management mode (global knobs) vs Expert mode (per-flight grid).
* Bulk apply to filtered sets; pending vs applied highlights.
* Special events overlay (date ranges with uplift/downlift).

### G) Forecast Engine (Event-Level)

* Deterministic per-flight calc: seats × LF → Pax; × transfer → PaxTransfer; × bags/pax → Bags.
* Exclude cancelled; clamp to seats; stamp CalcAt.
* Full season (100k flights) < 30s on server-class host; cached aggregates.

### H) Runway/Throughput Constraints

* Template designer for rolling windows; ARR/DEP or combined.
* Heatmap/timeline of movements vs capacity; violations summarized.
* “Resolve”: shift to nearest feasible slots; fallback delete by priority (generated → manual → imported).

### I) Groups & Filters (System + User)

* System groups: Airline, Region, Domestic/International, Day-of-Week, Season.
* User groups: saved flight collections or rule-based definitions.
* Filter chips apply globally across grids/charts/exports.
* Composition: AND across dimensions, OR within dimension.

### J) Dashboards & Analysis

* KPI cards; monthly/weekly time-series; aggregates by Airline/Region/Group.
* Interactive filters; drill to flights; export PNG/PDF/CSV.

### K) Scenario Comparison (A/B)

* Pick two scenarios; compute deltas (abs/%), and by dimension.
* Optional flight-level added/removed list; Excel/PDF export.

### L) Reporting & Export

* Raw schedule (flights+results) export CSV/Excel.
* Aggregated report builder (dimensions, abs/% change).
* Save/load report configs; deterministic formatting.

### M) Publish Scenario

* Produce JSON/CSV bundle (flights + loads + links) with manifest & checksum.
* “Published Scenarios” list with version.

---

## 6) Cross-Cutting Requirements

* **Performance:** server-side filtering/paging; grid virtualization; cached aggregates; async calc.
* **Auditability:** log who/what/when for imports, edits, loads, cancels, linking.
* **Validation:** friendly errors; downloadable error CSVs.
* **Time zones:** store UTC; render by Airport.TimeZone.
* **Idempotence:** imports/bulk ops repeatable & safe; consistent outcomes.

---

## 7) Milestones

1. **Foundation (W1–2)** — Solution scaffold; EF multi-DB; seed masters; Scenario/Flight; basic grid.
2. **Import & Explorer (W3–4)** — Upload/mapping/validate/commit; Scenario Explorer; audit.
3. **Editing & Loads (W5–6)** — Bulk ops; Loads (Mgmt/Expert); groups & global filters.
4. **Forecast & Dashboards (W7–8)** — Forecast engine; aggregates; dashboards; exports.
5. **Constraints & Linking (W9–10)** — Runway templates + analysis + resolve; linking; cancellation tool.
6. **Compare & Publish (W11–12)** — A/B compare; reports; publish; perf hardening.

---

## 8) Issue Template (use in `backlog.md`)

**Title:** `[Module] Action — outcome`
**Purpose:** Business/user value.
**Scope:** What to build (feature-level).
**Inputs:** Data/UI inputs.
**Outputs:** DB changes, UI states, files.
**Acceptance Criteria:** Bullet, testable, fast to verify.
**Dependencies:** Other issues or modules.
**Notes:** Edge cases, perf, i18n, accessibility.

---

## 9) Definition of Done (per issue)

* Unit tests for services; integration tests for critical flows.
* UI validations & error states covered.
* Works on **SqlServer** and **Postgres**.
* Useful logs (no secrets); no unhandled exceptions.
* Docs updated: `CHANGELOG.md`, `ASSUMPTIONS.md`, and, if UX changes, `USER_NOTES.md`.

---

## 10) Coding & UX Guidelines

* Clean Architecture boundaries; domain is persistence-agnostic.
* Async + cancellation; long ops show progress (import/forecast).
* DevExpress: DataGrid for tabular, Charts for trends, scheduler-like timeline for movements.
* Centralized filter state; apply consistently across pages.
* Accessibility: keyboard grid nav; color-safe status.
* UX defaults: “Management mode” visible, “Expert mode” reveals granular controls.

---

## 11) Kick-Start Issues (map or adapt from `backlog.md`)

1. **[Import] CSV Upload & Mapping Wizard (Step 1–3)** — presets; required fields; 100k rows < 20s parse.
2. **[Import] Validate & Preview Diff (Step 4–5)** — unknown codes, duplicates, summary cards.
3. **[Scenario] Explorer (List/Create/Duplicate/Archive/Delete)** — full CRUD + audit.
4. **[Flights] Virtualized Grid + Inline Edit + Bulk Apply** — smooth at 100k; confirm bulk changes.
5. **[Loads] Management ↔ Expert Mode** — global LF/transfer/bags vs per-flight overrides.
6. **[Forecast] Event-Level Calculator** — deterministic; excludes cancelled; caches aggregates.
7. **[Constraints] Template Designer + Heatmap** — rolling windows; violations; CSV export.
8. **[Linking] Auto-Link & Manual Override** — FIFO/LIFO; keep manual; UI pairing.
9. **[Cancellation] Rate-Based Remove** — date range + group; paired cancellation.
10. **[Dashboards] KPI + Time-Series + Aggregates + Filters** — <1s warm refresh; export PNG/CSV.
11. **[Compare] Scenario A/B + Deltas + Export** — totals & by-dimension deltas; Excel/PDF.
12. **[Publish] Scenario Bundle Writer** — JSON/CSV + manifest + checksum; published list.

---

## 12) Testing Strategy

* **Unit:** parsers, forecast math, constraints, linking heuristics, group filtering.
* **Integration:** import → forecast → dashboard, and A/B compare flow.
* **Performance:** seed large scenarios; measure grid load & forecast time; tune.
* **UAT:** parity scripts derived from `functions.md` + manual; record outcomes.

---

## 13) Security & Data

* No secrets in logs; connection strings via env.
* No PII modeled; forecasting is event/flight-level only.
* (Optional) Roles: Admin (delete/archive), Planner (edit/forecast), Viewer (read/export).

---

## 14) References

* `docs/backlog.md` — assignable issues (authoritative).
* `docs/functions.md` — feature behavior & UX parity.
* `docs/Doku_BTactical.pdf` — official manual; tie-break on behavior.

---

### Final Note to the Agent

Build **parity first**. When uncertain, choose the simplest behavior that matches `Doku_BTactical.pdf`, document the assumption, and proceed.
