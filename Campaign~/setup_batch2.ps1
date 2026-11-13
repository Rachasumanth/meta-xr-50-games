$base = "C:\Users\Administrator\UnityProjects\MetaXRSim\Assets\Games\4_3a_hemispheres_pinch_scale"
$models = "C:\Users\Administrator\GitHub\agy-1000-models\models"

$games = @(
    @{ slug = "tardigrade_pond"; model = "876-petri-dish.glb"; dest = "tardigrade_pond.glb" },
    @{ slug = "paratrooper_basket"; model = "064-parachute.glb"; dest = "paratrooper_basket.glb" },
    @{ slug = "micro_mortar_alchemist"; model = "702-soup-bowl.glb"; dest = "micro_mortar.glb" },
    @{ slug = "limpet_suction"; model = "071-alien-ufo-saucer.glb"; dest = "limpet.glb" },
    @{ slug = "temple_bell"; model = "704-teacup-and-saucer.glb"; dest = "temple_bell.glb" }
)

foreach ($g in $games) {
    $dir = Join-Path $base $g.slug
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Models") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts\Editor") | Out-Null
    Copy-Item -Path (Join-Path $models $g.model) -Destination (Join-Path $dir ("Models\" + $g.dest)) -Force
    Write-Host ("Created and copied model for " + $g.slug)
}
