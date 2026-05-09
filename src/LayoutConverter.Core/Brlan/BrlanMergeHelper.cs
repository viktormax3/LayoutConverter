using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LayoutConverter.Core.Schema.Rlan;

namespace LayoutConverter.Core.Brlan;

public static class BrlanMergeHelper
{
    /// <summary>
    /// Realiza un merge secuencial de archivos BRLAN generados por split (prefijo_LayoutName).
    /// </summary>
    public static Document MergeSequential(string layoutName, IReadOnlyList<string> splitBrlanPaths)
    {
        var masterDoc = new Document
        {
            head = new Head
            {
                create = new HeadCreate { user = "MergeHelper", date = DateTime.Now },
                generator = new HeadGenerator { name = "LayoutConverter", version = "1.0.0" }
            },
            body = new DocumentBody()
        };

        var masterRlan = new RLAN { startFrame = 0, convertStartFrame = 0 };
        var masterTags = new List<AnimTag>();
        var mergedContents = new Dictionary<string, AnimContent>();

        int currentOffset = 0;

        foreach (var path in splitBrlanPaths)
        {
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);
            
            // Extraer el tagName asumiendo el patrón "LayoutName_TagName"
            string tagName = fileNameWithoutExt;
            if (fileNameWithoutExt.StartsWith(layoutName + "_", StringComparison.OrdinalIgnoreCase))
            {
                tagName = fileNameWithoutExt.Substring(layoutName.Length + 1);
            }

            var subDoc = BrlanBinaryReader.ReadDocument(path);
            var subRlan = subDoc.body?.rlan?.FirstOrDefault();
            
            if (subRlan == null) continue;

            // Determinar la duración de la pieza (asumiendo que arranca en 0 internamente si fue spliteada)
            int pieceDuration = subRlan.endFrame - subRlan.startFrame;

            // Crear el AnimTag correspondiente para el maestro
            var tag = new AnimTag
            {
                name = tagName,
                fileName = tagName, // OldCode usa fileName="Start" y luego le prepende "LayoutName_"
                startFrame = currentOffset,
                endFrame = currentOffset + pieceDuration,
                animLoop = AnimLoopType.OneTime // OldCode por defecto en el primer split
            };
            masterTags.Add(tag);

            // Copiar y empalmar los contenidos de la animación
            if (subRlan.animContent != null)
            {
                // Solo para el master RLAN animType, tomamos el del primer chunk
                if (currentOffset == 0) masterRlan.animType = subRlan.animType;

                foreach (var content in subRlan.animContent)
                {
                    if (!mergedContents.TryGetValue(content.name, out var masterContent))
                    {
                        masterContent = new AnimContent { name = content.name, Items = Array.Empty<AnimTarget>() };
                        mergedContents[content.name] = masterContent;
                    }

                    // Iterar sobre los targets (TranslateY, ScaleX, etc.)
                    var mergedTargets = new List<AnimTarget>(masterContent.Items);
                    foreach (var target in content.Items)
                    {
                        // Desplazar todos los keyframes sumándoles el currentOffset
                        var shiftedTarget = ShiftTargetKeys(target, currentOffset);
                        
                        // Fusionar con un target existente del mismo tipo (si lo hay)
                        var existingTarget = mergedTargets.FirstOrDefault(t => t.target == shiftedTarget.target);
                        if (existingTarget != null)
                        {
                            existingTarget.key = MergeKeyframes(existingTarget.key, shiftedTarget.key);
                        }
                        else
                        {
                            mergedTargets.Add(shiftedTarget);
                        }
                    }
                    masterContent.Items = mergedTargets.ToArray();
                }
            }

            // Mover el offset para la siguiente animación en el empalme
            currentOffset += pieceDuration;
        }

        masterRlan.endFrame = currentOffset;
        masterRlan.convertEndFrame = currentOffset;
        masterRlan.animContent = mergedContents.Values.ToArray();

        masterDoc.body.animTag = masterTags.ToArray();
        masterDoc.body.rlan = new[] { masterRlan };

        return masterDoc;
    }

    private static AnimTarget ShiftTargetKeys(AnimTarget target, int offset)
    {
        // Duplicar el target para no mutar el original leído del archivo
        var newTarget = target.Duplicate(target.key);
        
        if (offset != 0 && newTarget.key != null)
        {
            var newKeys = new Hermite[newTarget.key.Length];
            for (int i = 0; i < newTarget.key.Length; i++)
            {
                var k = newTarget.key[i];
                newKeys[i] = k.Duplicate(k.frame + offset);
            }
            newTarget.key = newKeys;
        }
        
        return newTarget;
    }

    private static Hermite[] MergeKeyframes(Hermite[] existing, Hermite[] incoming)
    {
        var allKeys = new List<Hermite>(existing);
        
        foreach (var k in incoming)
        {
            // Remover keyframes exactos en el empalme para que no se superpongan
            // Mantiene el incoming si hay colisión (para asegurar la continuidad de la nueva curva)
            allKeys.RemoveAll(e => Math.Abs(e.frame - k.frame) < 0.001f);
            allKeys.Add(k);
        }

        return allKeys.OrderBy(k => k.frame).ToArray();
    }
}
