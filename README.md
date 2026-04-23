# LayoutConverter

LayoutConverter is a .NET 8 tool for converting layout XML assets into binary layout, animation, and texture resources. It includes both a command-line entry point and a small Windows GUI.

The project currently focuses on the RLYT/BRLYT and RLAN/BRLAN workflows, plus TGA to TPL texture conversion. The implementation is fixture-driven: sample inputs and expected binary outputs live in `samples/` so changes can be checked byte-for-byte.

## Repository Layout

- `src/LayoutConverter.Core`: XML schema models, document loaders, and binary writers.
- `src/LayoutConverter.Conversion`: conversion pipeline, routing, validation, and external resource export/import.
- `src/LayoutConverter.Cli`: console entry point.
- `src/LayoutConverter.Gui`: minimal Windows Forms front end over the same conversion pipeline.
- `samples/Base`: source fixtures.
- `samples/Expected`: expected outputs for the main Banner fixture.
- `samples/AnimFixtures`: focused animation fixtures for BRLAN edge cases.
- `Test`: auxiliary binary/text dumps used while checking BRLYT parity.

Local reference material, editor state, build outputs, logs, and scratch conversion outputs are intentionally ignored.

## Current Status

Implemented and fixture-validated:

- RLYT to BRLYT.
- RLYT external texture export to `timg/*.tpl`.
- RLAN to BRLAN.
- Split animation output by `AnimTag` with `-g`.
- `--omit-samekey`, `--omit-samekey-all`, and `--bake-infinity`.
- BRLAN `pai1`, `pat1`, and `pah1` sections.
- BRLAN `TexturePattern` resource tables.
- Animation texture reference export with `--cvtr-ref-tex-only`.
- TGA to TPL for `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- `NW4R_TGA` passthrough for prepacked GX texture payloads, including indexed and CMPR payloads when present in the TGA metadata.
- TPL to TGA for non-indexed TPL formats `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- BRLAN to RLAN XML reconstruction for `pat1`, `pah1`, and `pai1` animation data.
- BRLYT to partial RLYT XML reconstruction for `lyt1`, `txl1`, `fnl1`, pane hierarchy, picture/text/window/bounding panes, groups, and common material data.

Validated fixture coverage:

- `samples/Base/Banner/Layout/Banner.rlyt`: BRLYT and all referenced TPL textures match expected outputs.
- `samples/Base/Banner/Layout/Banner.rlan`: split BRLAN outputs match expected outputs.
- `samples/AnimFixtures/Coverage/Layout/Coverage.rlan`: covers `TexturePattern`, `MaterialColor`, `IndTextureSRT`, synthetic single-output animation, and `--bake-infinity`.
- `samples/AnimFixtures/TagShare/Layout/TagShare.rlan`: covers `pat1`, `pah1`, `AnimShare`, and tag groups.

## Build

```powershell
dotnet build LayoutConverter.sln -m:1 -v:minimal
```

If your local .NET SDK has workload resolver or parallel build issues, project-level builds are usually enough:

```powershell
dotnet build src\LayoutConverter.Conversion\LayoutConverter.Conversion.csproj -v:minimal
```

## Usage

Minimal GUI:

```powershell
dotnet run --project src\LayoutConverter.Gui\LayoutConverter.Gui.csproj
```

The GUI accepts `.rlyt`, `.xmlyt`, `.rlan`, `.xmlan`, and `.tpl` files or folders, writes to a selected output directory, and exposes the common layout/animation options without going through the CLI.

Legacy-style positional output:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\Base\Banner\Layout\Banner.rlyt scratch_compare
```

Explicit output option:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll --output scratch_compare samples\Base\Banner\Layout\Banner.rlyt
```

Banner-oriented layout profile:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll --banner samples\Base\Banner\Layout\Banner.rlyt scratch_compare
```

Split animation by tags:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll -g --omit-samekey --banner samples\Base\Banner\Layout\Banner.rlan scratch_anim
```

Single synthetic animation output:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\AnimFixtures\Coverage\Layout\Coverage.rlan scratch_anim
```

TPL to TGA:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\Expected\timg\Base.tpl scratch_reverse
```

BRLAN to RLAN:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\AnimFixtures\Coverage\expected_normal\anim\Coverage.brlan scratch_reverse
```

BRLYT to partial RLYT:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\Expected\blyt\Banner.brlyt scratch_reverse
```

## Output Layout

RLYT conversion writes:

- `DESTDIR/blyt/*.brlyt`
- `DESTDIR/timg/*.tpl`
- `DESTDIR/font/*`
- `DESTDIR/fnta/*`

RLAN conversion writes:

- `DESTDIR/anim/*.brlan`

TPL conversion writes:

- `DESTDIR/*.tga`

BRLAN conversion writes:

- `DESTDIR/*.rlan`

BRLYT conversion writes:

- `DESTDIR/*.rlyt`

## Validation Snippets

Compare Banner layout and texture outputs:

```powershell
$oldRoot = (Resolve-Path 'samples\Expected').Path.TrimEnd('\')
$newRoot = (Resolve-Path 'scratch_compare').Path.TrimEnd('\')
Get-ChildItem $oldRoot -Recurse -File |
  Where-Object { $_.Extension -in '.brlyt','.tpl' } |
  ForEach-Object {
    $rel = $_.FullName.Substring($oldRoot.Length + 1)
    $new = Join-Path $newRoot $rel
    [pscustomobject]@{
      File = $rel
      Match = (Test-Path $new) -and ((Get-FileHash $_.FullName -Algorithm SHA256).Hash -eq (Get-FileHash $new -Algorithm SHA256).Hash)
      OldSize = $_.Length
      NewSize = if (Test-Path $new) { (Get-Item $new).Length } else { $null }
    }
  } | Format-Table -AutoSize
```

Compare focused animation fixtures:

```powershell
$pairs = @(
  @('Coverage normal', 'samples\AnimFixtures\Coverage\expected_normal\anim\Coverage.brlan', 'scratch_anim\anim\Coverage.brlan'),
  @('Coverage bake', 'samples\AnimFixtures\Coverage\expected_bake\anim\Coverage.brlan', 'scratch_anim_bake\anim\Coverage.brlan'),
  @('TagShare', 'samples\AnimFixtures\TagShare\expected_g\anim\TagShare_Intro.brlan', 'scratch_tagshare\anim\TagShare_Intro.brlan')
)
foreach ($p in $pairs) {
  $expected = Get-FileHash $p[1] -Algorithm SHA256
  $actual = Get-FileHash $p[2] -Algorithm SHA256
  [pscustomobject]@{ Case = $p[0]; Match = $expected.Hash -eq $actual.Hash }
}
```

## Supported Texture Path

Texture support is scoped to the layout conversion pipeline. The project does not try to replace the full original TGA utility library; it only implements the behavior needed to export layout-referenced textures to TPL.

The standard layout texture path currently supports:

- Truecolor, grayscale, and color-mapped TGA.
- Uncompressed and RLE TGA.
- Top/bottom and left/right image origins.
- TPL formats `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- `NW4R_TGA` additional-information passthrough for prepacked `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, `RGBA8`, `CMPR`, `C4`, `C8`, and `C14` payloads.
- Reverse TPL decoding to uncompressed 32-bit TGA for `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.

Known texture gaps that are secondary to the main RLYT/RLAN converter scope:

- Direct `CMPR` encoding from ordinary TGA pixels.
- Reverse `CMPR` decoding to TGA.
- Direct indexed TPL encoding from ordinary color-mapped TGA to `C4`, `C8`, and `C14`.
- Reverse indexed TPL decoding to TGA.
- Palette bank generation for ordinary indexed textures.
- Mipmaps and advanced sampler/LOD data.

Known reverse-conversion gaps:

- BRLYT `mat1` currently reconstructs material names, black/white color registers, texture maps, texture matrices, and texture coordinate generators. Full TEV/Revo payload reconstruction is still pending.
- BRLYT `txt1`, `wnd1`, and `bnd1` have basic payload readers. They still need broader fixture coverage.
- BRLYT `usd1` payloads are not reconstructed yet.
- BRLYT to RLYT output is intended as a structural recovery stage until full material TEV/Revo and user data readers are implemented.

These are intentionally not treated as blockers for the primary layout/animation workflow unless a real layout fixture depends on them. `NW4R_TGA` passthrough already covers prepacked indexed and CMPR texture payloads used by layout assets.

Recommended order for optional texture follow-up work:

- Add small fixtures for `IA4`, `IA8`, `RGB565`, `RGBA8`, RLE, indexed TGA, and non-block-aligned dimensions.
- Add direct indexed palettes only if a real layout conversion needs them outside `NW4R_TGA`.
- Add direct `CMPR` only when needed, since byte-perfect compression depends on matching block/color selection behavior.
- Add mipmaps only if they appear in a layout fixture that is expected to round-trip through the converter.

## Notes

- XML version guards target `1.2.*`.
- `--no-check-version` bypasses the version guard.
- `--xsd-validate` can validate with an installed schema set when configured locally.
- `.vscode/`, build outputs, scratch folders, logs, and private reference material are ignored by Git.
