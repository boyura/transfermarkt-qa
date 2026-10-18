# Design and technology choices

What the design is, how it meets the requirements, and why each technology was chosen.
How to run, proof of a run, assumptions and limitations: see the [README](../README.md).

## 1. Design

### Layers

```
Tests (UiTests/Tests)                one scenario per test; all assertions live here
   │ use
Page objects + components            locators and user actions; no assertions
(UiTests/Pages, Pages/Components)
   │ use
API mocks (UiTests/Mocks)            control backend data where a scenario needs it
   │
Site base test                       cookie consent for transfermarkt.com
(UiTests/Core/TransfermarktUiTest)
   │ extends
Reusable framework                   settings, browser context, timeouts, HTTP 500
(Transfermarkt.QA.Framework)         monitoring, diagnostics; no site knowledge
   │ is tested by
Framework tests                      unit + integration tests of the framework
(Transfermarkt.QA.Framework.Tests)
```

| Project | Contents |
|---|---|
| `Framework` | `TestSettings` (config, `Environment=stage` override) · `BaseUiTest` (isolated context, fixed locale/timezone/viewport/UA, timeouts, tracing, failure artifacts, fail on HTTP 500) · `HttpErrorMonitor`, `HttpErrorLog` |
| `UiTests` | Everything transfermarkt-specific: pages, components, mocks, E2E tests |
| `Framework.Tests` | Unit + integration tests of the 500 detection (see README [test levels](../README.md#test-approach-and-test-levels)) |

### Key decisions

- **Page objects with components.** Header, navigation, search, login form and PL table are components; pages compose them. Page objects expose intent (`ClickAsync(NavSection.Gaming)`, `SearchAsync("…")`, `Row(i).Points`) and contain no assertions, so a failure always points at the test and one locator change fixes one place.
- **Locators**, in order of preference: roles and accessible names → stable ids/attributes (`form#schnellsuche`, `input[name='query']`) → classes only as a last resort (`#notice .accept-all`). Table rows are matched by club name. No XPath, no positional selectors.
- **Controlled backend data.** `PremierLeagueTableApiMock` keeps the real API response and overrides only `trend`, giving three deterministic cases (none, up, down) without hard-coded standings.
- **Isolation and parallelism.** A fresh `BrowserContext` per test. Fixtures run in parallel and tests within a fixture run sequentially. Max 2 workers, because the site is bot-protected.
- **Diagnostics.** A trace is recorded for every test and dropped on pass. On failure, a full-page screenshot and the trace are attached and can be replayed in Trace Viewer.

### Scenarios

| Test | Proves | Dynamic content handled |
|---|---|---|
| `Main_Navigation_Flow` | Each section opens its exact URL and is highlighted; logo returns to Discover | CDN/fresh pages; known site bug (no highlight), reported as a warning |
| `Header_Search_KnownClub_ShowsMatchingClubs` | Results page keeps the term, N > 0 hits, every club name contains the term | Changing hit counts |
| `Login_Form_Behaviour` | Initial state, required fields, submit enabled/disabled, remember-me, unknown-user error, close | On-demand form |
| `Home_PremierLeagueTable_HasExpectedStructureAndData` | Headers, 10 rows with logo/name/numeric stats, full-table link in the same tab | Lazy shadow-DOM widget, live standings |
| `Home_PremierLeagueTable_TrendIconFollowsBackendResponse` | Trend icon matches the backend value for the right club | Live trends, made deterministic by mocking |

HTTP 500 monitoring covers only `www.transfermarkt.com`; third-party hosts (ads, trackers) are ignored on purpose.

## 2. Requirements

| Requirement | Where it is covered |
|---|---|
| **Stable, handles dynamic UI** | Web-first `Expect` (10 s retry), `ConsentState` storage state, isolation (above). Details: README [Limitations](../README.md#assumptions-and-limitations) |
| **No static waits** | Every wait targets a condition. `NetworkIdle` was rejected because ads keep the network busy. See README [Limitations](../README.md#assumptions-and-limitations) |
| **Documented workarounds** | README [Assumptions and limitations](../README.md#assumptions-and-limitations), plus code comments where each workaround lives |
| **Proof of execution** | README [Test execution evidence](../README.md#test-execution-evidence) |

## 3. Technology

| Choice | Why |
|---|---|
| **Playwright** (vs Selenium, Cypress) | Built-in auto-waiting and retrying `Expect` · network interception (`RouteAsync`, `FetchAsync`) for the trend mock · `StorageState` for one-time consent · pierces shadow DOM, `FrameLocator` for the consent iframe · Trace Viewer · Chromium/Firefox/WebKit without driver management. Cypress is JS-only and weaker with iframes and tabs |
| **C# / .NET 8 LTS** | Static typing for page objects, enums (`NavSection`) and DTOs (`CompetitionTablesDto`) · official Playwright binding · builds on newer SDKs (recorded run: 10.0.402) |
| **NUnit** (vs xUnit, MSTest) | Official `Microsoft.Playwright.NUnit` / `PageTest` · categories `Unit`/`Integration`/`E2E` for `--filter` · fixture-level parallelism · `Assert.Warn` for known site defects · attachments |
| **System.Text.Json** | No extra dependency. `[JsonExtensionData]` passes untouched fields through |
| **`appsettings.json` + env overrides** | Standard .NET convention. URL, hosts, UA and timeouts change without touching code |
| **VSTest `html`/`console` loggers** | No extra packages, works on any CI. Traces and screenshots are attached on failure. Playwright's HTML report is Node-only |
