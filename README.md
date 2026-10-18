# Transfermarkt QA

UI test automation for transfermarkt.com (.NET 8, NUnit, Playwright).

## Solution structure

| Project | Purpose |
|---|---|
| `Transfermarkt.QA.Framework` | Reusable test infrastructure: settings, `BaseUiTest`, `HttpErrorMonitor`, `HttpErrorLog` |
| `Transfermarkt.QA.Framework.Tests` | Tests of the framework itself (`Unit` and `Integration` folders) |
| `Transfermarkt.QA.UiTests` | End-to-end UI tests of the site (page objects, API mocks, tests) |

Tests are tagged with NUnit categories: `Unit`, `Integration`, `E2E`.

```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=E2E"
```

## Unit vs integration testing in this solution

**Unit tests** (`Framework.Tests/Unit`, category `Unit`) exercise a single class in isolation, in memory:
no browser, no network, no timing. They are fast and fully deterministic.
`HttpErrorLogTests` covers `HttpErrorLog`, a deliberately pure class that decides which responses count as failures.

**Integration tests** (`Framework.Tests/Integration`, category `Integration`) check that several real components
work together: here the Playwright browser, the network layer and `HttpErrorMonitor` feeding `HttpErrorLog`.
`NetworkInterceptionTests` runs a real browser and injects a `500` via Playwright route interception, then asserts
the monitor records it. It is slower than a unit test, but still never touches the live site.

**E2E tests** (`UiTests`, category `E2E`) are the product-level UI tests against transfermarkt.com. They are not
unit or integration tests of our code; they are the thing the framework exists to support.

## Why these examples were chosen

- **The tests target the framework's own logic, not the site.** The riskiest piece of custom code is the HTTP 500
  detection used by the UI tests. If it silently failed, those tests would pass vacuously, so it is the part most
  worth proving.
- **`HttpErrorLog` is the unit example** because its rules are pure logic with many edge cases that are cheap to
  check without a browser: exact host matching (no subdomain or bare-domain false positives, case-insensitive),
  capturing only `500` (not `404`, `501`, `503`), behaviour with and without a host filter, and the failure report text.
- **`HttpErrorMonitor` wired to a browser is the integration example** because unit tests cannot show that the
  Playwright `Response` event is actually subscribed and reaches the log. Interception makes the failure
  deterministic, so the test does not depend on the live site misbehaving. It polls with a delayed NUnit constraint
  instead of a fixed sleep.
- **The split keeps the pyramid healthy:** many cheap unit tests for the logic, one targeted integration test for
  the wiring, and a small number of E2E tests for user-visible behaviour.

## How to run the tests and reproduce the results

Prerequisites: [.NET 8 SDK](https://dotnet.microsoft.com/download) or newer, and PowerShell (for the Playwright browser install).

```bash
dotnet build
pwsh src/Transfermarkt.QA.UiTests/bin/Debug/net8.0/playwright.ps1 install chromium
```

Run everything and write a console log plus a TRX report:

```bash
dotnet test --logger "trx;LogFileName=test-results.trx" --logger "console;verbosity=normal" --results-directory test-results
```

Or run one layer: `dotnet test --filter "Category=Unit"` (also `Integration`, `E2E`).
Useful switches: `HEADED=1` shows the browser, `BROWSER=firefox` changes the browser, `Environment=stage` loads `appsettings.stage.json`.
Failure screenshots and traces go to an `artifacts` folder next to the test binaries.

## Proof of a test run

The output of a real run (18 Oct 2026, Windows 11, .NET SDK 10.0.402, targeting net8.0, Chromium) is committed in [`docs/test-run`](docs/test-run):

- [`console-output.txt`](docs/test-run/console-output.txt): full console output, including stack traces and Playwright call logs.
- [`test-results.trx`](docs/test-run/test-results.trx): machine-readable report (opens in Visual Studio or any TRX viewer).

| Suite | Result |
|---|---|
| `Framework.Tests` (Unit + Integration) | 8 of 8 passed |
| `UiTests` (E2E, live site) | 5 of 5 passed |

The unit and integration tests are deterministic. The E2E tests run against the live site, so they can be affected by it.

Known live-site behaviour: during development, `Main_Navigation_Flow` failed intermittently because the site sometimes served a page
with no highlighted (`active`) item in the main menu. The cause was not identified (blocking third-party requests and `SlowMo` made no
measurable difference) and the problem stopped reproducing. The test keeps a strict assertion that the clicked section's menu item has the
`active` class, so if the site misbehaves again this test will fail rather than hide it.
Two real defects were fixed: `BaseUrl` now points at `https://www.transfermarkt.com` (the host the site redirects to), and the monitored host
for HTTP 500 errors was changed to match.
