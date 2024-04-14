using Leditor.Data.Tiles;

namespace Leditor.Serialization;

#nullable enable

public static class Exporters
{
    private static int[] ComposeStackables(bool[] stackables)
    {
        List<int> list = [];

        for (var i = 1; i < stackables.Length; i++)
        {
            if (stackables[i]) list.Add(i);
        }
        
        return [..list];
    }
    
    private static string Export(RunCell[,,] matrix)
    {
        System.Text.StringBuilder builder = new();

        builder.Append('[');
        
        for (var x = 0; x < matrix.GetLength(1); x++)
        {
            builder.Append('[');

            for (var y = 0; y < matrix.GetLength(0); y++)
            {
                builder.Append('[');

                for (var z = 0; z < 3; z++)
                {
                    var cell = matrix[y, x, z];
                    
                    builder.Append($"[{cell.Geo}, [{string.Join(", ", ComposeStackables(cell.Stackables))}]]");

                    if (z != 2) builder.Append(", ");
                }

                builder.Append(']');
                
                if (y != matrix.GetLength(0) - 1) builder.Append(", ");
            }
            
            builder.Append(']');
            
            if (x != matrix.GetLength(1) - 1) builder.Append(", ");
        }
        
        builder.Append(']');

        return builder.ToString();
    }

    private static string Export(TileCell[,,] matrix, string defaultMaterial)
    {
        System.Text.StringBuilder builder = new();

        builder.Append("[#lastKeys: [#L: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #c: 0, #q: 0], #Keys: [#L: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #c: 0, #q: 0], #workLayer: 1, #lstMsPs: point(48, 37), #tlMatrix: [");

        for (var x = 0; x < matrix.GetLength(1); x++)
        {
            builder.Append('[');
            
            for (var y = 0; y < matrix.GetLength(0); y++)
            {
                builder.Append('[');

                for (var z = 0; z < 3; z++)
                {
                    var cell = matrix[y, x, z];
                    
                    var tp = cell.Type switch
                    {
                        TileType.Default => "default",
                        TileType.Material => "material",
                        TileType.TileHead => "tileHead",
                        TileType.TileBody => "tileBody",
                        
                        _ => throw new Exception("Invalid tile type")
                    };

                    var data = cell.Data switch
                    {
                        TileDefault => "0",
                        TileMaterial m => $"\"{m.Name}\"",
                        TileHead h => $"[point(1, 1), \"{(h.Definition?.Name ?? h.Name)}\"]",
                        TileBody b => $"[point({b.HeadPosition.x}, {b.HeadPosition.y}), {b.HeadPosition.z}]",
                        
                        _ => throw new Exception("Invalid tile data")
                    };
                    
                    builder.Append($"[#tp: \"{tp}\", #Data: {data}]");

                    if (z != 2) builder.Append(", ");
                }
                
                builder.Append(']');
                
                if (y != matrix.GetLength(0) - 1) builder.Append(", ");
            }

            builder.Append(']');
            if (x != matrix.GetLength(1) - 1) builder.Append(", ");
        }
        
        builder.Append($"], #defaultMaterial: \"{defaultMaterial}\", #toolType: \"material\", #toolData: \"BigMetal\", #tmPos: point(1, 5), #tmSavPosL: [5, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 6, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], #specialEdit: 0]");
        
        return builder.ToString();
    }

    private static string Export((string name, EffectOptions[] options, double[,] matrix)[] effects)
    {
        System.Text.StringBuilder builder = new();

        builder.Append("[#lastKeys: [#n: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #e: 0, #r: 0, #f: 0], #Keys: [#n: 0, #m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #e: 0, #r: 0, #f: 0], #lstMsPs: point(44, 27), #effects: [");

        
        for (var e = 0; e < effects.Length; e++)
        {
            var (name, options, mtx) = effects[e];
            
            builder.Append($"[#nm: \"{name}\", #tp: \"{GLOBALS.EffectType(name)}\", #mtrx: [");

            for (var x = 0; x < mtx.GetLength(1); x++)
            {
                builder.Append('[');
                
                for (var y = 0; y < mtx.GetLength(0); y++)
                {
                    builder.Append($"{mtx[y, x]:0.0000}");
                    if (y != mtx.GetLength(0) - 1) builder.Append(", ");
                }

                builder.Append(']');

                if (x != mtx.GetLength(1) - 1) builder.Append(", ");
            }
            
            builder.Append("], #Options: [");

            for (var op = 0; op < options.Length; op++)
            {
                var option = options[op];
                
                builder.Append($"[\"{option.Name}\", [{string.Join(", ", option.Options.Select(str => $"\"{str}\""))}], {(option.Choice is string s ? $"\"{s}\"" : option.Choice)}]");
                
                if (op != options.Length - 1) builder.Append(", ");
            }
            
            builder.Append(']');

            if (GLOBALS.EffectType(name) == "standardErosion")
            {
                var repeatsExists = GLOBALS.EffectRepeats.TryGetValue(name, out var repeats);
                var openAreasExists = GLOBALS.EffectOpenAreas.TryGetValue(name, out var openAreas);
                builder.Append($", #repeats: {(repeatsExists ? repeats : 0)}, #affectOpenAreas: {(openAreasExists ? openAreas : 0):0.0000}");
            }

            builder.Append(']');

            if (e != effects.Length - 1) builder.Append(", ");
        }

        builder.Append(
            "], #emPos: point(3, 1), #editEffect: 10, #selectEditEffect: 10, #mode: \"editEffect\", #brushSize: 3]");
        
        
        return builder.ToString();
    }

    private static string Export(int angle, int flatness)
    {
        System.Text.StringBuilder builder = new();

        builder.Append($"[#pos: point(694, 189), #rot: -34, #sz: point(82, 196), #col: 1, #Keys: [#m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #r: 0, #f: 0, #z: 0, #m: 0], #lastKeys: [#m1: 0, #m2: 0, #w: 0, #a: 0, #s: 0, #d: 0, #r: 0, #f: 0, #z: 0, #m: 0], #lastTm: 3262047, #lightAngle: {angle}, #flatness: {flatness}, #lightRect: rect(1000, 1000, -1000, -1000), #paintShape: \"pxl\"]");
        
        return builder.ToString();
    }

    private static string Export(bool terrainMedium, bool light, (int width, int height) size, (int left, int top, int right, int bottom) bufferTiles, int seed)
    {
        System.Text.StringBuilder builder = new();

        builder.Append($"[#timeLimit: 4800, #defaultTerrain: {(terrainMedium ? 1 : 0)}, #maxFlies: 10, #flySpawnRate: 50, #lizards: [], #ambientSounds: [], #music: \"NONE\", #tags: [], #lightType: \"Static\", #waterDrips: 1, #lightRect: rect(0, 0, 1040, 800), #Matrix: []]");
        builder.Append('\r');
        builder.Append($"[#mouse: 1, #lastMouse: 1, #mouseClick: 0, #pal: 1, #pals: [[#detCol: color( 255, 0, 0 )]], #eCol1: 1, #eCol2: 2, #totEcols: 5, #tileSeed: {seed}, #colGlows: [0, 0], #size: point({size.width}, {size.height}), #extraTiles: [{bufferTiles.left}, {bufferTiles.top}, {bufferTiles.right}, {bufferTiles.bottom}], #light: {(light ? 1 : 0)}]");
        
        return builder.ToString();
    }

    private static string Export(List<RenderCamera> cameras)
    {
        System.Text.StringBuilder builder = new();

        var camerasString = string.Join(", ", cameras.Select(c => $"point({c.Coords.X}, {c.Coords.Y})"));
        var quadsString = string.Join(", ", cameras.Select(c => $"[[{c.Quad.TopLeft.angle}, {c.Quad.TopLeft.radius:0.0000}], [{c.Quad.TopRight.angle}, {c.Quad.TopRight.radius:0.0000}], [{c.Quad.BottomRight.angle}, {c.Quad.BottomRight.radius:0.0000}], [{c.Quad.BottomLeft.angle}, {c.Quad.BottomLeft.radius:0.0000}]]"));
        
        builder.Append(
            $"[#cameras: [{camerasString}], #selectedCamera: 0, #quads: [{quadsString}], #Keys: [#n: 0, #d: 0, #e: 0, #p: 0], #lastKeys: [#n: 0, #d: 0, #e: 0, #p: 0]]");
        
        return builder.ToString();
    }

    private static string Export(int waterLevel, bool waterInFront)
    {
        System.Text.StringBuilder builder = new();

        builder.Append($"[#waterLevel: {waterLevel}, #waterInFront: {(waterInFront ? 1 : 0)}, #waveLength: 60, #waveAmplitude: 5, #waveSpeed: 10]");

        return builder.ToString();
    }

    private static string Export((InitPropType type, TileDefinition? tile, (int category, int index) position, Prop prop)[] props)
    {
        System.Text.StringBuilder builder = new();

        builder.Append("[#props: [");

        for (var p = 0; p < props.Length; p++)
        {
            var (type, _, (category, index), prop) = props[p];

            builder.Append('[');

            builder.Append(prop.Depth);
            builder.Append(", ");
            builder.Append($"\"{prop.Name}\"");
            builder.Append(", ");
            builder.Append($"point({(category == -1 ? category + 2 : category + 1)}, {index+1})");
            builder.Append(", ");
            builder.Append($"[point({prop.Quads.TopLeft.X:0.0000}, {prop.Quads.TopLeft.Y:0.0000}), point({prop.Quads.TopRight.X:0.0000}, {prop.Quads.TopRight.Y:0.0000}), point({prop.Quads.BottomRight.X:0.0000}, {prop.Quads.BottomRight.Y:0.0000}), point({prop.Quads.BottomLeft.X:0.0000}, {prop.Quads.BottomLeft.Y:0.0000})]");
            builder.Append(", ");

            var settingsString = prop.Extras.Settings switch
            {
                PropRopeSettings rope => $"[#renderorder: {rope.RenderOrder}, #seed: {rope.Seed}, #renderTime: {rope.RenderTime}, #release: {(rope.Release switch { PropRopeRelease.Left => -1, PropRopeRelease.Right => 1, _ => 0 })}{(rope.Thickness is null ? "" : $", #thickness: {rope.Thickness}")}{(rope.ApplyColor is null ? "": $", #applyColor: {rope.ApplyColor}")}]",
                PropVariedSettings variedStandard => $"[#renderorder: {variedStandard.RenderOrder}, #seed: {variedStandard.Seed}, #renderTime: {variedStandard.RenderTime}, #variation: {variedStandard.Variation}]",
                PropVariedSoftSettings variedSoft => $"[#renderorder: {variedSoft.RenderOrder}, #seed: {variedSoft.Seed}, #renderTime: {variedSoft.RenderTime}, #variation: {variedSoft.Variation}, #customDepth: {variedSoft.CustomDepth}{(variedSoft.ApplyColor is null ? "" : $", #applyColor: {variedSoft.ApplyColor}")}]",
                PropVariedDecalSettings variedDecal => $"[#renderorder: {variedDecal.RenderOrder}, #seed: {variedDecal.Seed}, #renderTime: {variedDecal.RenderTime}, #variation: {variedDecal.Variation}, #customDepth: {variedDecal.CustomDepth}]",
                PropAntimatterSettings antimatter => $"[#renderorder: {antimatter.RenderOrder}, #seed: {antimatter.Seed}, #renderTime: {antimatter.RenderTime}, #customDepth: {antimatter.CustomDepth}]",
                PropSoftEffectSettings softEffect => $"[#renderorder: {softEffect.RenderOrder}, #seed: {softEffect.Seed}, #renderTime: {softEffect.RenderTime}, #customDepth: {softEffect.CustomDepth}]",
                PropSoftSettings soft => $"[#renderorder: {soft.RenderOrder}, #seed: {soft.Seed}, #renderTime: {soft.RenderTime}, #customDepth: {soft.CustomDepth}]",
                PropSimpleDecalSettings simpleDecal => $"[#renderorder: {simpleDecal.RenderOrder}, #seed: {simpleDecal.Seed}, #renderTime: {simpleDecal.RenderTime}, #customDepth: {simpleDecal.CustomDepth}]",
                { } basic => $"[#renderorder: {basic.RenderOrder}, #seed: {basic.Seed}, #renderTime: {basic.RenderTime}]"
            };

            var pointsString = string.Join(", ", prop.Extras.RopePoints.Select(point => $"point({point.X*1.25f:0.0000}, {point.Y*1.25f:0.0000})"));
            
            builder.Append($"[#settings: {settingsString}{(type == InitPropType.Rope ? $", #points: [{pointsString}]" : "")}]");
            
            builder.Append(']');
            if (p != props.Length - 1) builder.Append(", ");
        }
        
        builder.Append("], #lastKeys: [#w: 0, #a: 0, #s: 0, #d: 0, #L: 0, #n: 0, #m1: 0, #m2: 0, #c: 0, #z: 0], #Keys: [#w: 0, #a: 0, #s: 0, #d: 0, #L: 0, #n: 0, #m1: 0, #m2: 0, #c: 0, #z: 0], #workLayer: 3, #lstMsPs: point(0, 0), #pmPos: point(21, 10), #pmSavPosL: [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 12, 7, 14, 10, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1], #propRotation: 0, #propStretchX: 1, #propStretchY: 1, #propFlipX: 1, #propFlipY: 1, #depth: 0, #color: 0]");
        
        return builder.ToString();
    }

    internal static async Task<string> ExportAsync(LevelState level, char newLine = '\r')
    {
        var geoTask = Task.Factory.StartNew(() => Export(level.GeoMatrix));
        var tileTask = Task.Factory.StartNew(() => Export(level.TileMatrix, level.DefaultMaterial));
        var effTask = Task.Factory.StartNew(() => Export(level.Effects));
        var lightTask = Task.Factory.StartNew(() => Export(level.LightAngle, level.LightFlatness));
        var generalTask = Task.Factory.StartNew(() => Export(level.DefaultTerrain, level.LightMode, (level.Width, level.Height), level.Padding, level.Seed));
        var camsTask = Task.Factory.StartNew(() => Export(level.Cameras));
        var envTask = Task.Factory.StartNew(() => Export(level.WaterLevel, level.WaterAtFront));
        var propsTask = Task.Factory.StartNew(() => Export(level.Props));

        var allTasks = Task.WhenAll([
            geoTask, 
            tileTask, 
            effTask, 
            lightTask, 
            generalTask, 
            camsTask, 
            envTask, 
            propsTask
        ]);

        await allTasks;

        System.Text.StringBuilder builder = new();

        builder.Append(await geoTask);
        builder.Append(newLine);
        builder.Append(await tileTask);
        builder.Append(newLine);
        builder.Append(await effTask);
        builder.Append(newLine);
        builder.Append(await lightTask);
        builder.Append(newLine);
        builder.Append(await generalTask);
        builder.Append(newLine);
        builder.Append(await camsTask);
        builder.Append(newLine);
        builder.Append(await envTask);
        builder.Append(newLine);
        builder.Append(await propsTask);
        
        return builder.ToString();
    }
}