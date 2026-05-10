# NW4R Layout Tools (LayoutConverter)

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

**NW4R Layout Tools** is a high-fidelity suite designed for bidirectional conversion between Nintendo's NW4R layout formats (`BRLYT`, `BRLAN`, `TPL`) and accessible, human-readable XML/TGA files. 

The project's primary mission is **Bit-Perfect Parity**: ensuring that any binary resource converted to XML and back to binary results in an identical byte sequence, preserving all original metadata and alignment.

---

## 🚀 Features

- **RLYT <-> BRLYT**: Full layout conversion including pane hierarchies, materials, texture maps, and user data.
- **RLAN <-> BRLAN**: Animation conversion supporting `pai1`, `pat1`, and `pah1` sections.
- **TGA <-> TPL**: High-quality texture conversion with support for `I4`, `I8`, `IA4`, `IA8`, `RGB565`, `RGB5A3`, and `RGBA8`.
- **Metadata Preservation**: Uses `NW4R_TGA` metadata and XML attributes to store binary-only fields that don't map naturally to layout schemas.
- **Batch Processing**: Powerful CLI for mass conversion and validation.
- **Validation Suite**: Built-in scripts for round-trip parity verification.

---

## 📂 Project Structure

The project is divided into focused modules located in `src/`:

| Project | Description |
| :--- | :--- |
| **LayoutConverter.Core** | The engine. Contains binary readers/writers, XML schemas, and sanitization logic. |
| **LayoutConverter.Conversion** | The pipeline. Manages routing, resource resolution, and external file exporting. |
| **LayoutConverter.Cli** | Command-line interface for automation and advanced usage. |
| **LayoutConverter.Gui** | A minimal Windows tool for quick, drag-and-drop conversions. |

---

## 🛠️ Current Status & Support

The project is currently in a **stable reconciliation state**, with strong coverage for Banner and TitleLogo layouts.

### ✅ Supported Formats
- **Layouts**: Picture, Text, Window, Bounding, and Group panes. Material blocks with multiple texture maps and TEV stages.
- **Animations**: Texture Pattern (`pat1`), Palette Change (`pah1`), and Animation Info (`pai1`).
- **Textures**: Standard TPL formats and `NW4R_TGA` passthrough for prepacked CMPR/Indexed payloads.

### 🚧 TODO / Roadmap
- [ ] **CMPR Encoding**: Direct encoding from raw TGA pixels (currently uses passthrough).
- [ ] **Indexed TPL Decoding**: Support for reversing `C4`, `C8`, and `C14` to TGA.
- [ ] **Mipmaps**: Support for multi-level textures and LOD metadata.
- [ ] **Extended Fixtures**: Broader validation across more games and edge cases.

---

## 📖 Usage

### Command Line (CLI)

The CLI is the core tool for automation and batch processing. It is distributed as a standalone executable.

**Forward Conversion (XML -> Binary):**
```powershell
# Basic conversion to BRLYT/BRLAN
.\layout-converter.exe MyLayout.rlyt ./output

# Banner-specific profile (ensures perfect compatibility with Wii System Menu Banners)
.\layout-converter.exe --banner MyLayout.rlyt ./output
```

**Reverse Conversion (Binary -> XML):**
```powershell
.\layout-converter.exe MyLayout.brlyt ./output_xml
```

**Animation Splitting:**
```powershell
# Extract specific animations into multiple .brlan files using tags
.\layout-converter.exe -g --omit-samekey MyAnimation.rlan ./output_anim
```

### Graphical Interface (GUI)

For users who prefer a visual approach, **LayoutConverter.Gui** provides a lightweight, drag-and-drop experience.

- **Drag & Drop**: Simply drop `.rlyt`, `.rlan`, or folders onto the window.
- **Visual Feedback**: Real-time logging and status updates for each conversion case.
- **Configurable**: Quick access to common options like `--banner` or animation splitting without memorizing flags.

```powershell
# To run from source:
dotnet run --project src\LayoutConverter.Gui\LayoutConverter.Gui.csproj
```

---

## 🧪 Validation & Quality Assurance

We maintain a rigorous validation process to ensure stability.

**Parity Check:**
Use the included PowerShell tools to verify that your changes haven't broken binary parity.
```powershell
# Validates that XML -> BRLYT -> XML -> BRLYT results in identical binaries
.\tools\Validate-RlytRoundTrip.ps1 -InputPath samples\Base\Banner\Layout\Banner.rlyt
```

---

## ⚠️ Known Issues

- **Version Guards**: Default XML versioning targets `1.2.*`. Use `--no-check-version` to bypass for older/newer formats.
- **CMPR Decoding**: Reverse conversion of CMPR textures is currently limited to passthrough metadata.
- **Font References**: Missing shared font files will trigger warnings but won't stop binary generation.

---

## 🤝 Contributing

This is a personal project focused on precision. If you find a layout that doesn't round-trip perfectly, please provide the original binary and the generated XML for debugging.

---

*Developed with ❤️ for the Wii Modding Community.*
