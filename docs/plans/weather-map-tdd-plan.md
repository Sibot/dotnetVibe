# Weather Map — TDD Implementation Plan

Confirmed intent: [weather-map.md](../intent/weather-map.md)

## TDD workflow (every slice)

1. Write one failing test naming the behavior
2. `dotnet test` → FAIL
3. Minimal code to pass
4. `dotnet test` → PASS
5. Refactor with green tests
6. Check off the box below

---

## Phase 0 — Test harness

- [x] Create `DotnetVibe.ApiService.Tests` project and add to solution
- [x] Add package references
- [x] Add smoke test
- [x] Verify `dotnet test` passes

---

## Phase 1 — Active location resolver (pure logic)

- [x] Anonymous user → browser coordinates
- [x] Authenticated, no pins → browser coordinates
- [x] Authenticated, pins exist, user selects pin → pin coordinates
- [x] Authenticated, user chooses "current position" → browser coordinates
- [x] Authenticated with pins, none selected → first pin by sort order

---

## Phase 2 — Weather provider (HTTP boundary, stubbed)

- [x] Define `IWeatherProvider` and `LocationForecast` DTOs
- [x] Test: stub `HttpMessageHandler` maps fixture JSON to `LocationForecast`
- [x] Test: provider handles HTTP error gracefully
- [x] Implement `OpenMeteoWeatherProvider` + mapper
- [x] Register `HttpClient` + provider in DI

---

## Phase 3 — Pinned locations service (EF in-memory tests)

- [x] Add `PinnedLocation` entity
- [x] Test: create location for user
- [x] Test: list returns only calling user's locations
- [x] Test: update renames (owner only)
- [x] Test: delete removes (404 for wrong user)
- [x] Test: reject invalid coordinates
- [x] Add EF migration

---

## Phase 4 — Forecast API endpoint (integration)

- [x] `WebApplicationFactory` with fake `IWeatherProvider`
- [x] Test: 200 returns current + daily forecast JSON
- [x] Test: 400 for invalid coordinates
- [x] Test: no authentication required
- [x] Wire minimal endpoint

---

## Phase 5 — Pinned locations API (integration + auth)

- [x] Test auth handler injects `sub` claim
- [x] Test: GET without token → 401
- [x] Test: POST creates → 201
- [x] Test: PUT another user's id → 404
- [x] Test: DELETE removes item
- [x] Wire CRUD endpoints

---

## Phase 6 — Web API client

- [x] `GetForecastAsync(lat, lon)` calls correct endpoint
- [x] Location CRUD methods send bearer token
- [x] Register client in Web DI

---

## Phase 7 — Blazor UI + map

- [x] Page renders loading state
- [x] Anonymous: browser location + forecast
- [x] Signed in with pins: location picker
- [x] Switching location refetches forecast
- [x] Pin management UI: add, rename, delete
- [x] "Use current position" when signed in
- [x] Leaflet map: single pin at active coordinates
- [x] Forecast panel: current + multi-day
- [ ] Manual smoke test via Aspire AppHost

---

## Phase 8 — Hardening

- [x] Coordinate validation at API boundary
- [x] Generic error responses (no stack traces to client)
- [x] Config in appsettings
- [x] Full `dotnet test` suite green
- [x] Update README with `/weather-map` tour entry
