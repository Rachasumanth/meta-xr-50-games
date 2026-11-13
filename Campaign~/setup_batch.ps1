$base = "C:\Users\Administrator\UnityProjects\MetaXRSim\Assets\Games\4_3a_hemispheres_pinch_scale"
$models = "C:\Users\Administrator\GitHub\agy-1000-models\models"

$games = @(
    @{ slug = "phantom_shell_game"; model = "702-soup-bowl.glb"; dest = "walnut_shell.glb" },
    @{ slug = "micro_wok_toss"; model = "720-wok-pan.glb"; dest = "micro_wok.glb" },
    @{ slug = "snapping_turtle_bunker"; model = "175-sea-turtle.glb"; dest = "turtle_bunker.glb" },
    @{ slug = "deep_space_dish"; model = "971-satellite-dish.glb"; dest = "satellite_dish.glb" },
    @{ slug = "bey_cap_clash"; model = "071-alien-ufo-saucer.glb"; dest = "bey_cap.glb" }
)

foreach ($g in $games) {
    $dir = Join-Path $base $g.slug
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Models") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts\Editor") | Out-Null
    Copy-Item -Path (Join-Path $models $g.model) -Destination (Join-Path $dir ("Models\" + $g.dest)) -Force
    Write-Host ("Created and copied model for " + $g.slug)
}
