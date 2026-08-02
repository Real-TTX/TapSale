$version = & "$PSScriptRoot/version.ps1" -Channel development
$env:TABSALE_VERSION = $version
docker compose -f "$PSScriptRoot/../compose.dev.yml" up --build -d --force-recreate
