# Auditoría de Paridad Binaria: BrlytBinaryWriter vs OldCode

Comparación exhaustiva entre nuestro `BrlytBinaryWriter.cs` y el compilador original (`bv.cs` + `db.cs` + `bl.cs`).

> [!CAUTION]
> Se han encontrado **múltiples discrepancias críticas** que explican los diffs persistentes. Cada una afecta bytes diferentes del binario final.

---

## 🔴 BUG 1: Versión del Binario — Round-trip inflado

**Archivo:** [BrlytBinaryWriter.cs](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/src/LayoutConverter.Core/Brlyt/BrlytBinaryWriter.cs#L72-L80) + [BrlytBinaryReader.cs](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/src/LayoutConverter.Core/Brlyt/BrlytBinaryReader.cs#L180-L187)

**Lo que hace OldCode:** La versión binaria **NO proviene del XML**. En [bl.cs:375](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bl.cs#L375):
```csharp
ushort num = (A_4 ? 8 : 10); // A_4 es un flag de CLI (--banner / -g)
```
- `--banner` → versión `0x0008`
- Sin `--banner` → versión `0x000A`

**Lo que hacemos nosotros:**
- **Reader:** `0x0008` → `"1.2.0"` 
- **Writer:** `"1.2.0"` → `0x0008 + minor(2)` = `0x000A` ❌

**Resultado:** Cada round-trip infla la versión de `0x0008` a `0x000A`. El byte 7 del binario cambia.

**Fix:** El writer debe **preservar** la versión binaria original del documento, no derivarla del XML.

---

## 🔴 BUG 2: Flags de Material — `ShouldWrite*` es un concepto inventado

**Archivo:** [BrlytBinaryWriter.cs](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/src/LayoutConverter.Core/Brlyt/BrlytBinaryWriter.cs#L908-L935)

**Lo que hace OldCode** en [bv.cs:1333-1349](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bv.cs#L1333-L1349):
```csharp
// swapTable: null check solamente
byte b = ((A_1.swapTable != null) ? 1 : 0);

// alphaCompare: null check solamente
byte b2 = ((A_1.alphaCompare != null) ? 1 : 0);

// blendMode: null check solamente  
byte b3 = ((A_1.blendMode != null) ? 1 : 0);

// matColReg: null check solamente
int num5 = ((A_1.matColReg != null) ? 1 : 0);
```

**Lo que hacemos nosotros:**
```csharp
ShouldWriteSwapTables()   → compara contra "identity" R=R,G=G,B=B,A=A ❌
ShouldWriteAlphaCompare() → compara contra Always/0/And/Always/0 ❌
ShouldWriteBlendMode()    → compara contra Blend/SrcAlpha/InvSrcAlpha/Copy ❌
ShouldWriteMatColReg()    → compara contra white(255,255,255,255) ❌
```

**Resultado:** Los flags de material en el `mat1` header son diferentes. Si el XML dice que tiene `alphaCompare` (aun con valor default), OldCode lo serializa. Nosotros lo omitimos → los flags bit-packed son distintos → todo el mat1 se desalinea.

**Fix:** Reemplazar **todas las funciones `ShouldWrite*`** por simples null-checks, igual que OldCode.

---

## 🔴 BUG 3: Orden de texturas — Nuestro ordenamiento cambió con el desconvertidor

**Archivo:** [BrlytDocumentContext.cs](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/src/LayoutConverter.Core/Brlyt/BrlytDocumentContext.cs#L106-L114)

**Lo que hace OldCode:** Usa `SortedList<string, TextureFile>` en [bv.cs:1288](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bv.cs#L1288) y [bt.cs](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bt.cs) — `SortedList` usa `Comparer<string>.Default` que es **ordinal** en la cultura invariante.

**Lo que hacemos:** En la última edición, cambiamos a `StringComparer.OrdinalIgnoreCase` lo cual reordena diferente en nombres con mayúsculas/minúsculas.

**Fix:** Usar `StringComparer.Ordinal` (case-sensitive) que es el comparador por defecto de `SortedList<string, T>`.

---

## 🟡 BUG 4: BlendMode defaults — byte packing incorrecto cuando es null

**Lo que hace OldCode** en [bv.cs:241-246](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bv.cs#L241-L246):
```csharp
// Defaults ANTES de verificar null:
cr = 1;          // type=Blend
*((ref cr) + 1) = 4;  // src=SrcAlpha
*((ref cr) + 2) = 5;  // dst=InvSrcAlpha
*((ref cr) + 3) = 15; // op=Set (!!)
```
Nota: el default de `op` es **`Set (15)`**, NO `Copy (3)`.

**Lo que hacemos:** Nuestro `WriteBlendMode` con `mode is null` no escribe nada (ya que `ShouldWriteBlendMode` lo filtra). Pero si el flag dice que sí hay blendMode, nuestros defaults son Copy(3) no Set(15).

---

## 🟡 BUG 5: AlphaCompare defaults — valor cuando null

**Lo que hace OldCode** en [bv.cs:388](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytwrt/bv.cs#L388):
```csharp
ie ie = 119;  // 0x77 = comp0=Always(7) | comp1=Always(7) packed
*((ref ie) + 1) = 0;  // op=And
*((ref ie) + 2) = 0;  // ref0
*((ref ie) + 3) = 0;  // ref1
```
Cuando `A_1 != null`, sobreescribe estos defaults. Cuando `A_1 is null`, escribe `{0x77, 0, 0, 0}`.

**Lo que hacemos:** Nunca escribimos este bloque si `ShouldWriteAlphaCompare` retorna false. Pero el viejo código **siempre lo escribe** si el flag lo pide, incluso con defaults.

---

## 🟢 Correcto: Orden de bloques de material

Nuestro orden coincide con [db.cs:227-514](file:///c:/Users/kerid/Downloads/Nw4r.LayoutTools_integration_stage2/OldCode/nw4r_lytcvtr/db.cs#L227-L514):
1. Header (64 bytes) ✅
2. TexMap[] ✅
3. TexMatrix[] ✅
4. TexCoordGen[] ✅  
5. ChannelControl ✅
6. MatColReg ✅
7. SwapTable ✅
8. IndirectMatrix[] ✅
9. IndirectStage[] ✅
10. TevStage[] ✅
11. AlphaCompare ✅
12. BlendMode ✅

---

## Plan de Acción (priorizado por impacto)

| # | Fix | Impacto | Complejidad |
|---|-----|---------|-------------|
| 1 | **Eliminar `ShouldWrite*`** → null-check puro | 🔴 Alto — afecta todos los mat1 flags | Bajo |
| 2 | **Versión binaria** → preservar del documento, no derivar del XML | 🔴 Alto — afecta byte 7 del header | Bajo |
| 3 | **Comparer de texturas** → `StringComparer.Ordinal` | 🟡 Medio — reordena txl1 y los índices tex | Bajo |
| 4 | **BlendMode default op** → Set(15) no Copy(3) | 🟡 Medio — afecta materiales con blend null | Bajo |
| 5 | **AlphaCompare packing** → verificar 0x77 vs 7<<4\|7 | 🟢 Bajo — ya correcto si se escribe | Bajo |
