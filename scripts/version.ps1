param(
    [ValidateSet('local', 'development', 'release')]
    [string]$Channel = 'local',
    [int]$Major = 1,
    [int]$Minor = 0
)

$buildDate = (Get-Date).ToUniversalTime().ToString('yyyyMMdd')
$commitCount = 0
$countText = git rev-list --count HEAD 2>$null
if ($LASTEXITCODE -eq 0 -and [int]::TryParse($countText, [ref]$commitCount)) {
    $buildNumber = $commitCount + 1
} else {
    $buildNumber = 1
}

$version = switch ($Channel) {
    'release' { "$Major.$Minor.$buildNumber-$buildDate" }
    'development' { "nightly-$buildNumber-$buildDate" }
    default { "local-$buildDate" }
}

$version
