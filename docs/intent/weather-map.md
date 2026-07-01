# Weather Map — Intent

## Outcome

A geographic weather map at `/weather-map` showing a **real forecast** for the active location: current conditions on a pin, multi-day forecast in a panel.

## Users

Anyone using dotnetVibe to check weather for places they care about.

## Location rules

| State | Default location |
|--------|------------------|
| Not signed in | Browser geolocation |
| Signed in, has pinned place(s) | User picks one active pin |
| Signed in, no pins yet | Browser geolocation until they pin one |
| Signed in | Option to switch to "current position" |

## Account features

- Multiple pinned places per user (full add / rename / delete in v1)
- Pins stored server-side, keyed by JWT `sub`
- One location shown on the map at a time

## Freshness

Load forecast on page open and when switching location. No background polling in v1.

## Out of scope (v1)

- Auto-refresh timer while page is open
- All pinned places visible on the map at once
- Radar / regional layers
- Replacing the existing demo `/weather` page

## Implementation plan

See [weather-map-tdd-plan.md](../plans/weather-map-tdd-plan.md).
