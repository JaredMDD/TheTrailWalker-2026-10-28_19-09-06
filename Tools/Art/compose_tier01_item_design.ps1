# Label actual Unity item renders. No external image package or synthetic artwork.
Add-Type -AssemblyName System.Drawing
$ErrorActionPreference = 'Stop'
$itemRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$itemDocs = Join-Path $itemRoot 'Docs/Art/Tier01/ItemDesign'
$itemData = Get-Content (Join-Path $itemRoot 'Assets/_Game/Art/Items/Tier01Design/Tier01ItemDesignManifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$itemTitle = [Drawing.Font]::new('Segoe UI',32,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
$itemText = [Drawing.Font]::new('Segoe UI',17,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$itemSmall = [Drawing.Font]::new('Segoe UI',14,[Drawing.FontStyle]::Regular,[Drawing.GraphicsUnit]::Pixel)
$itemInk = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#302F2B'))
$itemMuted = [Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml('#676257'))

function Write-ItemSheet([object[]]$Rows,[string]$Title,[string]$File,[string]$View='study',[int]$Columns=5,[int]$Thumb=260,[string]$ImageDirectory=$itemDocs) {
    $rowCount = [int][Math]::Ceiling($Rows.Count / [double]$Columns)
    $cell = $Thumb+24; $rowSize=$Thumb+76
    $canvas = [Drawing.Bitmap]::new($Columns*$cell+32, 130+$rowCount*$rowSize)
    $graphics=[Drawing.Graphics]::FromImage($canvas)
    try {
        $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#E8E1D3'))
        $graphics.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.TextRenderingHint=[Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $graphics.DrawString($Title,$itemTitle,$itemInk,24,15)
        $graphics.DrawString('Tier 1 / faceted item art / actual Unity meshes / gameplay and device tests pending',$itemText,$itemMuted,24,62)
        for($i=0;$i -lt $Rows.Count;$i++) {
            $entry=$Rows[$i]; $x=16+($i%$Columns)*$cell; $y=100+[Math]::Floor($i/$Columns)*$rowSize
            $picture=[Drawing.Image]::FromFile((Join-Path $ImageDirectory ($entry.Model+'-'+$View+'.png')))
            try {$graphics.DrawImage($picture,[int]$x,[int]$y,$Thumb,$Thumb)} finally {$picture.Dispose()}
            $caption=[string]$entry.Name
            if($caption.Length -gt 33) {$caption=$caption.Substring(0,30)+'...'}
            $graphics.DrawString($caption,$itemText,$itemInk,[single]$x,[single]($y+$Thumb+8))
            $graphics.DrawString(('{0} / {1} triangles' -f $entry.Category,$entry.Triangles),$itemSmall,$itemMuted,[single]$x,[single]($y+$Thumb+34))
        }
        $canvas.Save((Join-Path $itemDocs $File),[Drawing.Imaging.ImageFormat]::Png)
    } finally {$graphics.Dispose();$canvas.Dispose()}
}

try {
    foreach($category in @('Harvest','Material','Tool','Wearable','Food','Misc','External')) {
        $rows=@($itemData.Items | Where-Object {$_.Category -eq $category})
        Write-ItemSheet $rows ('Tier-one '+$category.ToLower()+' designs') ('tier1-'+$category.ToLower()+'-designs.png')
    }
    $selected=@('PineLog','RawRabbitMeat','RainbowTrout','PinePlank','HempCord','FineHempCloth','PineAxeNormal','RabbitHat','RabbitBoots','StarterBackpack','AppleJuice','RoastedRabbit','ShrimpPie','PremiumApplePie','RabbitToy','CowMilk')
    $rows=@(foreach($name in $selected) {$itemData.Items | Where-Object {$_.Model -eq $name}})
    Write-ItemSheet $rows 'Tier-one item design overview' 'tier1-item-design-overview.png' 'study' 4 300
    Write-ItemSheet @($itemData.Items) 'Tier-one front-view icon study' 'tier1-front-icon-study.png' 'front' 8 150
    $merchantDocs=Join-Path $itemRoot 'Docs/Art/Tier01/MerchantItemDesign'
    if(Test-Path (Join-Path $merchantDocs 'Merchant-capture-manifest.json')) {
        $merchant=(Get-Content (Join-Path $merchantDocs 'Merchant-capture-manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json)
        $original=(Get-Content (Join-Path $itemRoot 'Assets/_Game/Art/Items/BalloonGoods/BalloonGoodsManifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json)
        $merchantRows=@(foreach($entry in $merchant.Items) {
            $model=[IO.Path]::GetFileNameWithoutExtension($entry.StudyImage).Replace('-study','')
            $sourceModel=$model.Replace('Merchant-','')
            $count=($original.Parts | Where-Object {$_.Name -eq $sourceModel} | Measure-Object -Property Triangles -Sum).Sum
            if(-not $count) {$count=($original.Parts | Where-Object {$_.Name -like 'MysteryBox*'} | Measure-Object -Property Triangles -Sum).Sum}
            [PSCustomObject]@{Model=$model;Name=$entry.Name;Category='Merchant surface';Triangles=$count}
        })
        Write-ItemSheet $merchantRows 'Tier-one merchant item and mystery-box surfaces' 'tier1-merchant-designs.png' 'study' 3 350 $merchantDocs
    }
    $accessoryDocs=Join-Path $itemRoot 'Docs/Art/Characters/Tier01Accessories'
    if(Test-Path (Join-Path $accessoryDocs 'Male-standing-0.png')) {
        $canvas=[Drawing.Bitmap]::new(1920,1400);$graphics=[Drawing.Graphics]::FromImage($canvas)
        try {
            $graphics.Clear([Drawing.ColorTranslator]::FromHtml('#E8E1D3'))
            $graphics.DrawString('Tier-one fitted hat and backpack',$itemTitle,$itemInk,28,18)
            $graphics.DrawString('Both current bodies. Static art fit only; not runtime-bound or animation validated.',$itemText,$itemMuted,28,68)
            $views=@('Male-standing-0','Male-standing-90','Male-standing-180','Male-standing-35','Female-standing-0','Female-standing-90','Female-standing-180','Female-standing-35')
            for($i=0;$i -lt $views.Count;$i++) {
                $x=20+($i%4)*475;$y=110+[Math]::Floor($i/4)*630
                $picture=[Drawing.Image]::FromFile((Join-Path $accessoryDocs ($views[$i]+'.png')))
                try {$graphics.DrawImage($picture,[int]$x,[int]$y,450,550)} finally {$picture.Dispose()}
                $graphics.DrawString($views[$i],$itemText,$itemInk,[single]$x,[single]($y+562))
            }
            $canvas.Save((Join-Path $itemDocs 'tier1-fitted-accessories-overview.png'),[Drawing.Imaging.ImageFormat]::Png)
        } finally {$graphics.Dispose();$canvas.Dispose()}
    }
    Write-Output 'Composed tier-one category, overview, icon and fitted-accessory sheets.'
} finally {$itemTitle.Dispose();$itemText.Dispose();$itemSmall.Dispose();$itemInk.Dispose();$itemMuted.Dispose()}
