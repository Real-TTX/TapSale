$version = & "$PSScriptRoot/version.ps1" -Channel development
$env:TAPSALE_VERSION = $version
docker compose -f "$PSScriptRoot/../compose.dev.yml" up --build -d --force-recreate
