$base = "C:\Users\Administrator\UnityProjects\MetaXRSim\Assets\Games\4_3b_hemispheres_hand_scale"
$models = "C:\Users\Administrator\GitHub\agy-1000-models\models"

$games = @(
    @{ slug = "galaxy_cauldron"; model = "702-soup-bowl.glb"; dest = "galaxy_cauldron.glb" },
    @{ slug = "singing_bowl_sanctuary"; model = "777-ramen-noodle-bowl.glb"; dest = "singing_bowl.glb" },
    @{ slug = "fingerboard_bowl"; model = "702-soup-bowl.glb"; dest = "skate_bowl.glb" },
    @{ slug = "observatory_dome"; model = "971-satellite-dish.glb"; dest = "observatory_dome.glb" },
    @{ slug = "medusa_jellyfish"; model = "186-jellyfish.glb"; dest = "jellyfish.glb" }
)

foreach ($g in $games) {
    $dir = Join-Path $base $g.slug
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Models") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $dir "Scripts\Editor") | Out-Null
    Copy-Item -Path (Join-Path $models $g.model) -Destination (Join-Path $dir ("Models\" + $g.dest)) -Force
    Write-Host ("Created and copied model for " + $g.slug)
}
