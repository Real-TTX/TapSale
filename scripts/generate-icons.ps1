param([string]$WebRoot = (Join-Path $PSScriptRoot '../src/TapSale.Web/wwwroot'))

Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class NativeIconMethods {
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern bool DestroyIcon(IntPtr handle);
}
'@

function New-RoundedPath([float]$x, [float]$y, [float]$width, [float]$height, [float]$radius) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $diameter = $radius * 2
    $path.AddArc($x, $y, $diameter, $diameter, 180, 90)
    $path.AddArc($x + $width - $diameter, $y, $diameter, $diameter, 270, 90)
    $path.AddArc($x + $width - $diameter, $y + $height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($x, $y + $height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

function Fill-RoundedRect($graphics, $brush, [float]$x, [float]$y, [float]$width, [float]$height, [float]$radius) {
    $path = New-RoundedPath $x $y $width $height $radius
    $graphics.FillPath($brush, $path)
    $path.Dispose()
}

function New-TapSaleBitmap([int]$size) {
    $bitmap = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.ScaleTransform($size / 512.0, $size / 512.0)

    $ink = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#102A2A'))
    $brand = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#167D6D'))
    $mint = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#8DD3C7'))
    $lime = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#E4F26B'))
    $paper = [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml('#F8FAF5'))
    $brandGlow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(61, 22, 125, 109))
    $limeGlow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(23, 228, 242, 107))

    Fill-RoundedRect $graphics $ink 0 0 512 512 112
    $graphics.FillEllipse($brandGlow, 364, 34, 116, 116)
    $graphics.FillEllipse($limeGlow, 10, 350, 144, 144)
    Fill-RoundedRect $graphics $paper 176 64 160 168 20
    $graphics.FillRectangle($brand, 211, 105, 90, 20)
    $graphics.FillRectangle($brand, 246, 125, 20, 64)

    $top = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $top.AddPolygon([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new(134,205), [System.Drawing.PointF]::new(378,205),
        [System.Drawing.PointF]::new(441,343), [System.Drawing.PointF]::new(71,343)))
    $graphics.FillPath($lime, $top)
    $top.Dispose()
    Fill-RoundedRect $graphics $ink 137 237 142 70 16
    Fill-RoundedRect $graphics $mint 158 258 100 14 7
    Fill-RoundedRect $graphics $brand 158 282 64 10 5

    foreach ($x in @(288,328,368)) {
        Fill-RoundedRect $graphics $ink $x 239 26 27 8
        Fill-RoundedRect $graphics $ink $x 279 26 27 8
    }

    Fill-RoundedRect $graphics $lime 64 326 384 126 30
    Fill-RoundedRect $graphics $paper 96 352 320 72 19
    Fill-RoundedRect $graphics $ink 202 372 108 17 8.5
    $graphics.FillEllipse($brand, 362, 376, 24, 24)

    foreach ($item in @($ink,$brand,$mint,$lime,$paper,$brandGlow,$limeGlow)) { $item.Dispose() }
    $graphics.Dispose()
    return $bitmap
}

$iconDirectory = Join-Path $WebRoot 'icons'
$bitmap192 = New-TapSaleBitmap 192
$bitmap192.Save((Join-Path $iconDirectory 'app-icon-192.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap192.Dispose()

$bitmap512 = New-TapSaleBitmap 512
$bitmap512.Save((Join-Path $iconDirectory 'app-icon-512.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$bitmap512.Dispose()

$bitmap48 = New-TapSaleBitmap 48
$handle = $bitmap48.GetHicon()
$icon = [System.Drawing.Icon]::FromHandle($handle)
$stream = [System.IO.File]::Create((Join-Path $WebRoot 'favicon.ico'))
$icon.Save($stream)
$stream.Dispose()
$icon.Dispose()
[NativeIconMethods]::DestroyIcon($handle) | Out-Null
$bitmap48.Dispose()
