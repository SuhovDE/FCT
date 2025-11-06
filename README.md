# Project Summary — “B Tactical” Rebuild (Blazor + EF Core). The new project name is AeroPulse

**Why this exists (motivation):**
Airports need a reliable, bottom-up tactical traffic forecasting tool that planners can trust for day-to-day decisions and rapid what-if analyses. Rebuilding B Tactical gives us vendor independence, continuity for teams migrating off legacy tools, and a modern, auditable platform that aligns forecasting with operations (capacity, stands/gates, security) in one place.

**Who it serves:**

* Airport ops planners & capacity teams
* Forecasting/traffic analytics & route development
* Management needing fast, scenario-based KPIs and reports

**What we’re building (capability at parity):**

* **Event-level forecast engine** (PAX, transfers, bags) with editable **load factors**
* **Scenario management & versioning** (duplicate, archive, compare A/B)
* **Flight schedule import** (SIR/SSIM/CSV), validation, and **bulk edits** (freq/equipment)
* **Flight Event Generator (FEG)**, **turnaround linking**, **cancellation rates**
* **Runway/throughput constraints** checks and automated spill/shift suggestions
* **Groups & filters** (system + user-defined) used consistently across views, edits, reports
* **Dashboards, reports, and exports** (summary, time-series, by airline/region/group)
* **Publish scenarios** downstream (e.g., to capacity planning)

**How we’ll build it (constraints & NFRs):**

* **Blazor + DevExpress** for UI; **Entity Framework Core** with **multi-DB** support
* Deterministic, auditable calculations; role-based access; full change history
* Scales to seasonal schedules (10k–100k+ flights); responsive grids & virtualization
* API-first extensibility for integrations (AODB, data lakes, downstream modules)

**Success looks like:**

* Feature-parity with the user manual and video (no scope gaps)
* Import → adjust → forecast → analyze → compare → publish in a single flow
* Sub-second interactions on filtered views; full-scenario diff/compare within seconds
* Clear, exportable dashboards and reports that stakeholders can act on

**Key risks & mitigations:**

* **Data quality/mapping:** guided import wizards, strict validation, reference masters
* **Performance:** pre-aggregation, server-side filtering, async calc, caching
* **UX complexity:** opinionated defaults (“management mode”), deep-dive “expert mode”
* **Change management:** templates, inline help, audit trails, reproducible scenarios

In short: this project delivers a modern, end-to-end tactical forecasting application that replicates B Tactical’s workflow exactly—while being cloud-ready, extensible, and fast for real operational use.
