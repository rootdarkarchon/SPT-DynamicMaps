using DynamicMaps;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Unity.VectorGraphics;
using UnityEngine;

public static class SvgUtils
{
    private static readonly Dictionary<string, Sprite> _spriteCache = [];

    private const int _vertexBudget = 60000;
    private static readonly Dictionary<string, int> _svgTessellationPresetCache = new();
    private static readonly VectorUtils.TessellationOptions[] _tessellationPresets =
    [
        new()
        {
            StepDistance = 8f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 5f,
            SamplingStepSize = 0.05f
        },
        new()
        {
            StepDistance = 12f,
            MaxCordDeviation = 1.0f,
            MaxTanAngleDeviation = 10f,
            SamplingStepSize = 0.08f
        },
        new()
        {
            StepDistance = 18f,
            MaxCordDeviation = float.MaxValue,
            MaxTanAngleDeviation = float.MaxValue,
            SamplingStepSize = 0.10f
        },
        new()
        {
            StepDistance = 28f,
            MaxCordDeviation = float.MaxValue,
            MaxTanAngleDeviation = float.MaxValue,
            SamplingStepSize = 0.15f
        },
        new()
        {
            StepDistance = 40f,
            MaxCordDeviation = float.MaxValue,
            MaxTanAngleDeviation = float.MaxValue,
            SamplingStepSize = 0.20f
        }
    ];

    public static Sprite GetOrLoadCachedSprite(string path)
    {
        if (_spriteCache.ContainsKey(path))
        {
            return _spriteCache[path];
        }

        var absolutePath = Path.Combine(Plugin.Path, path);
        return _spriteCache[path] = TryLoadSvgSprite(absolutePath);
    }

    public static Sprite TryLoadSvgSprite(string absolutePath)
    {
        var svgText = File.ReadAllText(absolutePath);

        using var reader = new StringReader(svgText);

        var sceneInfo = SVGParser.ImportSVG(
            reader,
            ViewportOptions.OnlyApplyRootViewBox,
            dpi: 0,
            pixelsPerUnit: 1f,
            windowWidth: 0,
            windowHeight: 0);

        Plugin.Log.LogDebug($"Scene viewport: {sceneInfo.SceneViewport}");

        if (!TryReadRootViewBox(svgText, out var viewBoxRect))
        {
            Plugin.Log.LogError("SVG has no valid root viewBox.");
            return null;
        }

        Plugin.Log.LogDebug($"Using root viewBox rect: {viewBoxRect}");

        var presetOrder = GetPresetOrder(absolutePath);

        foreach (var presetIndex in presetOrder)
        {
            var preset = _tessellationPresets[presetIndex];

            var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, preset, sceneInfo.NodeOpacity);
            Plugin.Log.LogDebug($"SVG geoms with preset {presetIndex}: {geoms?.Count ?? 0}");

            if (geoms == null || geoms.Count == 0)
                continue;

            geoms = geoms
                .Where(g => RectOverlaps(viewBoxRect, g.UnclippedBounds))
                .ToList();

            if (geoms.Count == 0)
            {
                Plugin.Log.LogDebug($"No overlapping geoms remained for preset {presetIndex}.");
                continue;
            }

            var vertexCount = CountVertices(geoms);
            var triangleCount = CountTriangles(geoms);

            Plugin.Log.LogDebug(
                $"Preset {presetIndex}: verts={vertexCount}, tris={triangleCount}, " +
                $"step={preset.StepDistance}, sample={preset.SamplingStepSize}, " +
                $"cord={preset.MaxCordDeviation}, tan={preset.MaxTanAngleDeviation}");

            if (vertexCount > _vertexBudget)
            {
                Plugin.Log.LogWarning(
                    $"Skipping preset {presetIndex} for {absolutePath}: vertex budget exceeded " +
                    $"({vertexCount} > {_vertexBudget}).");
                continue;
            }

            try
            {
                var sprite = VectorUtils.BuildSprite(
                    geoms,
                    viewBoxRect,
                    svgPixelsPerUnit: 1f,
                    alignment: VectorUtils.Alignment.Center,
                    customPivot: Vector2.zero,
                    gradientResolution: 64,
                    flipYAxis: true);

                Plugin.Log.LogDebug($"SVG sprite rect: {sprite.rect}, bounds: {sprite.bounds}");
                Plugin.Log.LogDebug($"SVG sprite vertices: {sprite.vertices.Length}, triangles: {sprite.triangles.Length / 3}");

                if (sprite.vertices.Length == 0 || sprite.triangles.Length == 0)
                {
                    Plugin.Log.LogWarning($"Preset {presetIndex} built an empty sprite for {absolutePath}, trying next preset.");
                    continue;
                }

                _svgTessellationPresetCache[absolutePath] = presetIndex;
                Plugin.Log.LogDebug($"Using SVG tessellation preset {presetIndex} for {absolutePath}");

                return sprite;
            }
            catch (System.Exception ex)
            {
                Plugin.Log.LogWarning($"Preset {presetIndex} failed for {absolutePath}: {ex.Message}");
            }
        }

        Plugin.Log.LogError($"Failed to build SVG sprite for {absolutePath} with all tessellation presets.");
        return null;
    }

    private static IEnumerable<int> GetPresetOrder(string relativePath)
    {
        if (_svgTessellationPresetCache.TryGetValue(relativePath, out var cached))
        {
            yield return cached;

            for (var i = cached + 1; i < _tessellationPresets.Length; i++)
                yield return i;

            for (var i = 0; i < cached; i++)
                yield return i;

            yield break;
        }

        for (var i = 0; i < _tessellationPresets.Length; i++)
            yield return i;
    }

    private static int CountVertices(List<VectorUtils.Geometry> geoms)
    {
        var total = 0;

        foreach (var geom in geoms)
            total += geom.Vertices?.Length ?? 0;

        return total;
    }

    private static int CountTriangles(List<VectorUtils.Geometry> geoms)
    {
        var total = 0;

        foreach (var geom in geoms)
            total += (geom.Indices?.Length ?? 0) / 3;

        return total;
    }

    private static bool TryReadRootViewBox(string svgText, out Rect rect)
    {
        rect = default;

        var doc = XDocument.Parse(svgText);
        var root = doc.Root;
        if (root == null || root.Name.LocalName != "svg")
            return false;

        var viewBox = root.Attribute("viewBox")?.Value;
        if (string.IsNullOrWhiteSpace(viewBox))
            return false;

        var parts = viewBox
            .Split(new[] { ' ', '\t', '\r', '\n', ',' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 4)
            return false;

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return false;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return false;
        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var w)) return false;
        if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var h)) return false;

        rect = new Rect(x, y, w, h);
        return w > 0f && h > 0f;
    }

    private static bool RectOverlaps(Rect a, Rect b)
    {
        return a.xMin < b.xMax &&
               a.xMax > b.xMin &&
               a.yMin < b.yMax &&
               a.yMax > b.yMin;
    }
}