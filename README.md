# LayoutConverter

LayoutConverter is a .NET 8 tool for converting layout XML assets into binary layout, animation, and texture resources. It includes both a command-line entry point and a small Windows GUI.

The project currently focuses on the main layout and animation workflows:

- `RLYT <-> BRLYT`
- `RLAN <-> BRLAN`
- `TGA <-> TPL`

The implementation is fixture-driven: sample inputs and expected binary outputs live in `samples/` so changes can be checked byte-for-byte.

## Repository Layout

- `src/LayoutConverter.Core`: XML schema models, document loaders, binary readers, and binary writers.
- `src/LayoutConverter.Conversion`: conversion pipeline, routing, validation, and external resource export/import.
- `src/LayoutConverter.Cli`: console entry point.
- `src/LayoutConverter.Gui`: minimal Windows Forms front end over the same conversion pipeline.
- `samples/Base`: source XML fixtures used for forward conversion checks.
- `samples/Expected`: expected binary outputs for the main Banner fixture.
- `samples/AnimFixtures`: focused animation fixtures for BRLAN edge cases.
- `samples/TitleLogo`: richer real-world fixture with `anim/`, `blyt/`, `timg/`, plus flat copies at the root for folder-based testing.
- `Test`: auxiliary binary/text dumps used while checking binary parity.

Local reference material, editor state, build outputs, logs, and scratch conversion outputs are intentionally ignored.

## Current Status

Implemented and fixture-validated:

- RLYT to BRLYT.
- BRLYT to RLYT.
- RLYT external texture export to `timg/*.tpl`.
- RLAN to BRLAN.
- BRLAN to RLAN.
- Split animation output by `AnimTag` with `-g`.
- `--omit-samekey`, `--omit-samekey-all`, and `--bake-infinity`.
- BRLAN `pai1`, `pat1`, and `pah1` sections.
- BRLAN `TexturePattern` resource tables.
- Animation texture reference export with `--cvtr-ref-tex-only`.
- TGA to TPL for `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- `NW4R_TGA` passthrough for prepacked GX texture payloads, including indexed and CMPR payloads when present in the TGA metadata.
- TPL to TGA for non-indexed TPL formats `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- BRLAN to RLAN XML reconstruction for `pat1`, `pah1`, and `pai1` animation data.
- BRLYT to RLYT XML reconstruction for `lyt1`, `txl1`, `fnl1`, pane hierarchy, picture/text/window/bounding panes, groups, `usd1`, and common material/Revo payload blocks.
- Reverse-path preservation of binary-only metadata when needed for byte-perfect roundtrip, such as BRLAN tag indices and BRLYT textbox or pane fields that do not map cleanly to the XML model.

Validated fixture coverage:

- `samples/Base/Banner/Layout/Banner.rlyt`: BRLYT and all referenced TPL textures match expected outputs.
- `samples/Expected/blyt/Banner.brlyt`: BRLYT to RLYT to BRLYT roundtrip matches byte-for-byte when the referenced TPL files are available beside the recovered RLYT.
- `samples/Base/Banner/Layout/Banner.rlan`: split BRLAN outputs match expected outputs.
- `samples/AnimFixtures/Coverage/Layout/Coverage.rlan`: covers `TexturePattern`, `MaterialColor`, `IndTextureSRT`, synthetic single-output animation, and `--bake-infinity`.
- `samples/AnimFixtures/TagShare/Layout/TagShare.rlan`: covers `pat1`, `pah1`, `AnimShare`, and tag groups.
- `samples/TitleLogo/anim/*.brlan`: BRLAN to RLAN to split BRLAN roundtrip matches byte-for-byte with `-g`, including preserved `pat1` tag indices.
- `samples/TitleLogo/blyt/TitleLogo.brlyt`: BRLYT to RLYT to BRLYT roundtrip matches byte-for-byte. Missing shared font files are reported as warnings and do not stop binary generation.

## Fixture Conventions

The `samples/` tree intentionally mixes a few fixture styles:

- `samples/Base/.../Layout`: XML-first fixtures used to validate forward conversion.
- `samples/Expected`: binary outputs expected from the main Banner forward path.
- `samples/AnimFixtures/.../Layout`: focused XML animation fixtures for edge-case coverage.
- `samples/TitleLogo/{anim,blyt,timg}`: binary-first fixture arranged like a typical extracted resource folder.
- `samples/TitleLogo/*` at the root: the same files flattened so folder-based CLI input can be tested directly.

Scratch outputs are intentionally not tracked:

- `scratch*`
- `samples/**/outputRefactor*`
- `samples/**/outputOldCode*`

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

The GUI accepts `.rlyt`, `.xmlyt`, `.rlan`, `.xmlan`, `.tpl` files or folders, writes to a selected output directory, and exposes the common layout and animation options without going through the CLI.

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

BRLYT to RLYT:

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\Expected\blyt\Banner.brlyt scratch_reverse
```

## Output Layout

RLYT conversion writes:

- `DESTDIR/blyt/*.brlyt`
- `DESTDIR/timg/*.tpl`
- `DESTDIR/font/*`
- `DESTDIR/fnta/*`

If a referenced shared font file is not present beside the source XML, the converter logs a warning and still writes the BRLYT binary.

RLAN conversion writes:

- `DESTDIR/anim/*.brlan`

TPL conversion writes:

- `DESTDIR/*.tga`

BRLAN conversion writes:

- `DESTDIR/*.rlan`

BRLYT conversion writes:

- `DESTDIR/*.rlyt`

## Validation Snippets

Recommended final regression pass:

```powershell
dotnet build LayoutConverter.sln -m:1 -v:minimal
```

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\Expected\blyt\Banner.brlyt scratch_banner_reverse
Copy-Item samples\Expected\timg\*.tpl scratch_banner_reverse -Force
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll scratch_banner_reverse\Banner.rlyt scratch_banner_roundtrip
```

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\TitleLogo\blyt\TitleLogo.brlyt scratch_titlelogo_reverse
Copy-Item samples\TitleLogo\timg\*.tpl scratch_titlelogo_reverse -Force
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll scratch_titlelogo_reverse\TitleLogo.rlyt scratch_titlelogo_roundtrip
```

```powershell
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll samples\TitleLogo\anim scratch_titlelogo_anim_reverse
dotnet src\LayoutConverter.Cli\bin\Debug\net8.0\layout-converter.dll -g scratch_titlelogo_anim_reverse scratch_titlelogo_anim_roundtrip
```

Banner and TitleLogo BRLYT hash check:

```powershell
Get-FileHash samples\Expected\blyt\Banner.brlyt -Algorithm SHA256
Get-FileHash scratch_banner_roundtrip\blyt\Banner.brlyt -Algorithm SHA256
Get-FileHash samples\TitleLogo\blyt\TitleLogo.brlyt -Algorithm SHA256
Get-FileHash scratch_titlelogo_roundtrip\blyt\TitleLogo.brlyt -Algorithm SHA256
```

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

Compare TitleLogo split animation roundtrip:

```powershell
$pairs = Get-ChildItem samples\TitleLogo\anim -Filter *.brlan | ForEach-Object {
  $roundtrip = Join-Path 'scratch_titlelogo_anim_roundtrip\anim' ($_.BaseName + '_' + $_.BaseName + '.brlan')
  [pscustomobject]@{
    Name = $_.Name
    Match = (Get-FileHash $_.FullName -Algorithm SHA256).Hash -eq (Get-FileHash $roundtrip -Algorithm SHA256).Hash
  }
}
$pairs | Format-Table -AutoSize
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

Known texture gaps that are secondary to the main layout and animation converter scope:

- Direct `CMPR` encoding from ordinary TGA pixels.
- Reverse `CMPR` decoding to TGA.
- Direct indexed TPL encoding from ordinary color-mapped TGA to `C4`, `C8`, and `C14`.
- Reverse indexed TPL decoding to TGA.
- Palette bank generation for ordinary indexed textures.
- Mipmaps and advanced sampler or LOD data.

Known remaining gaps:

- Direct `CMPR` encoding from ordinary decoded TGA pixels is still intentionally out of scope for now.
- Reverse indexed TPL decoding (`C4`, `C8`, `C14`) is still pending.
- Direct indexed TPL encoding from ordinary palette-based TGA is still pending.
- Reverse BRLYT coverage is strong for Banner and TitleLogo, but still needs broader fixture variety before it can be called universally complete.
- Some byte-perfect reverse cases rely on preserved binary-only metadata attributes in the reconstructed XML. That is intentional for compatibility.

These are intentionally not treated as blockers for the primary layout and animation workflow unless a real fixture depends on them. `NW4R_TGA` passthrough already covers prepacked indexed and CMPR texture payloads used by layout assets.

Recommended order for optional texture follow-up work:

- Add small fixtures for `IA4`, `IA8`, `RGB565`, `RGBA8`, RLE, indexed TGA, and non-block-aligned dimensions.
- Add direct indexed palettes only if a real layout conversion needs them outside `NW4R_TGA`.
- Add direct `CMPR` only when needed, since byte-perfect compression depends on matching block and color selection behavior.
- Add mipmaps only if they appear in a layout fixture that is expected to round-trip through the converter.

## Notes

- XML version guards target `1.2.*`.
- `--no-check-version` bypasses the version guard.
- `--xsd-validate` can validate with an installed schema set when configured locally.
- Shared font files may be reused across many layouts. Missing font files are reported as warnings during export and do not stop BRLYT generation.
- `.vscode/`, build outputs, scratch folders, logs, and private reference material are ignored by Git.
