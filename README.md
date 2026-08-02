# TabSale

TabSale is a mobile-first, offline-capable point-of-sale PWA for clubs, volunteers and temporary event stalls. The server uses ASP.NET Core Razor Pages and SQLite; the selling surface continues to work from its cached app shell and queues receipts in IndexedDB until the server is reachable again.

## Start development

```powershell
docker compose -f compose.dev.yml up --build -d --force-recreate
```

Open <http://localhost:9262>. The first request opens the one-time administrator setup. Following the project rule, test changes by rebuilding and redeploying the container rather than relying on host live reload.

The convenience script calculates the next nightly version and performs the same rebuild/redeploy:

```powershell
./scripts/redeploy-dev.ps1
```

## Release stack

```powershell
$env:TABSALE_VERSION = "1.0.1-20260802"
docker compose -f compose.release.yml up --build -d --force-recreate
```

## Persistent data

The complete state is held in `/app/data` inside the named Docker volume. It contains SQLite and the ASP.NET data-protection keys required for sessions to survive container restarts. TabSale does not create its own backups; stop writes and back up or restore the complete volume.

## Versions

- Release: `<major>.<minor>.<build>-<builddate>`
- Development: `nightly-<build>-<builddate>`
- Local: `local-<builddate>`

Pass the calculated value as `TABSALE_VERSION` when building a container. Git commits use the locally configured author identity.
