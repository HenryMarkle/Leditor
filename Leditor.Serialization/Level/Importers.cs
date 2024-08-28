namespace Leditor.Serialization.Level;

using System.Numerics;

using static Raylib_cs.Raylib;
using Raylib_cs;

using Pidgin;

using Leditor.Serialization.Parser;

using Leditor.Data;
using Leditor.Data.Tiles;
using Leditor.Data.Effects;
using Leditor.Data.Materials;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Props;

public static class Importers
{
    
    public static List<Data.RenderCamera> GetCameras(AstNode.Base @base) {
        var props = (AstNode.PropertyList)@base;

        var cameras = ((AstNode.List) props.Values
            .Single(p => ((AstNode.Symbol)p.Key).Value == "cameras")
            .Value)
            .Values
            .Cast<AstNode.GlobalCall>()
            .Select(c => {
                var x = Serialization.Utils.GetInt(c.Arguments[0]);
                var y = Serialization.Utils.GetInt(c.Arguments[1]);

                return (x, y);
            })
            .ToArray();

        var quads = ((AstNode.List?) props.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "quads").Value)
            ?.Values
            ?.Cast<AstNode.List>()
            ?.Select(l => {
                var currentQuads = l.Values.Cast<AstNode.List>().Select(q => {
                    var angle = Serialization.Utils.GetInt(q.Values[0]);
                    var radius = Serialization.Utils.GetInt(q.Values[1]);

                    return (angle, radius);
                }).ToArray();

                return (currentQuads[0], currentQuads[1], currentQuads[2], currentQuads[3]);
            })
            ?.ToArray() ?? [ ((0, 0), (0, 0), (0, 0), (0, 0)) ];

        var result = new Data.RenderCamera[cameras.Length];

        for (int c = 0; c < cameras.Length; c++) {
            var currentQuads = quads[c];

            // var topLeft = new Vector2((float) (currentQuads.Item1.radius * Math.Cos(currentQuads.Item1.angle)), (float) (currentQuads.Item1.radius * Math.Sin(currentQuads.Item1.angle)));
            // var topRight = new Vector2((float) (currentQuads.Item2.radius * Math.Cos(currentQuads.Item2.angle)), (float) (currentQuads.Item2.radius * Math.Sin(currentQuads.Item2.angle)));
            // var bottomRight = new Vector2((float) (currentQuads.Item3.radius * Math.Cos(currentQuads.Item3.angle)), (float) (currentQuads.Item3.radius * Math.Sin(currentQuads.Item3.angle)));
            // var bottomLeft = new Vector2((float) (currentQuads.Item4.radius * Math.Cos(currentQuads.Item4.angle)), (float) (currentQuads.Item4.radius * Math.Sin(currentQuads.Item4.angle)));

            var topLeft = currentQuads.Item1;
            var topRight = currentQuads.Item2;
            var bottomRight = currentQuads.Item3;
            var bottomLeft = currentQuads.Item4;
            
            result[c] = new() {
                Coords = new Vector2(cameras[c].x, cameras[c].y),

                Quad = new Data.CameraQuad(
                    topLeft, 
                    topRight, 
                    bottomRight, 
                    bottomLeft)
            };
        }

        return result.ToList();
    }


    public static (int width, int height) GetLevelSize(AstNode.Base node)
    {
        var point = (node as AstNode.PropertyList)?.Values.FirstOrDefault(s => s.Key is AstNode.Symbol { Value: "size" }).Value;

        if (point == null) return (-1, -1);

        var w = (point as AstNode.GlobalCall)?.Arguments[0];
        var h = (point as AstNode.GlobalCall)?.Arguments[1];

        if (w is null || h is null) return (-1, -1);

        return (Serialization.Utils.GetInt(w as AstNode.Number), Serialization.Utils.GetInt(h as AstNode.Number));
    }

    public static (int width, int height) GetLevelSizeFromGeoMatrix(AstNode.Base node)
    {
        if (node is not AstNode.List) throw new ArgumentException("object is not a list", nameof(node));

        var columns = ((AstNode.List)node).Values;

        var height = 0;
        var width = columns.Length;

        for (int x = 0; x < columns.Length; x++) {
            if (columns[x] is not AstNode.List) throw new Exception($"column ({x}) is not a list");

            for (int y = 0; y < ((AstNode.List)columns[x]).Values.Length; y++) {
                height = ((AstNode.List)columns[x]).Values.Length;


                if (((AstNode.List)((AstNode.List)columns[x]).Values[y]) is not AstNode.List)
                    throw new Exception($"element ({y}) at row ({x}) is not a list");
            }
        }

        return (width, height);
    }

    public static (int waterLevel, bool waterInFront) GetWaterData(AstNode.Base @base)
    {
        var waterLevelBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "waterlevel").Value;
        var waterInFrontBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "waterinfront").Value;

        var waterLevel = Serialization.Utils.GetInt(waterLevelBase);
        var waterInFront = Serialization.Utils.GetInt(waterInFrontBase) != 0;

        return (waterLevel, waterInFront);
    }


    public static int GetSeed(AstNode.Base @base)
    {
        var seedBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tileseed").Value;

        return Serialization.Utils.GetInt(seedBase);
    }

    public static bool GetLightMode(AstNode.Base @base)
    {
        var light = (AstNode.Number?)((AstNode.PropertyList)@base).Values
            .SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "light").Value;

        if (light is null) return true;

        return light.Value.IntValue == 1;
    }

    public static bool GetTerrainMedium(AstNode.Base @base)
    {
        var terrain = (AstNode.Number) ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultterrain").Value;

        return terrain.Value.IntValue == 1;
    }

    
    public static (int angle, int flatness) GetLightSettings(AstNode.Base @base)
    {
        var propList = (AstNode.PropertyList)@base;

        var angleBase = propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "lightangle").Value;
        var flatnessBase = propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "flatness").Value;

        var angle = angleBase switch
        {
            AstNode.Number n => n.Value.IntValue,
            AstNode.UnaryOperator u => ((AstNode.Number)u.Expression).Value.IntValue * -1,
            _ => throw new Exception($"Invalid light angle value")
        };
        
        var flatness = flatnessBase switch
        {
            AstNode.Number n => n.Value.IntValue,
            AstNode.UnaryOperator u => ((AstNode.Number)u.Expression).Value.IntValue * -1,
            _ => throw new Exception($"Invalid flatness value")
        };

        return (angle, flatness);
    }


    public static BufferTiles GetBufferTiles(AstNode.Base @base) {
        var pair = ((AstNode.PropertyList)@base).Values.Single(s => ((AstNode.Symbol)s.Key).Value == "extratiles");
        int[] list = ((AstNode.List)pair.Value).Values.Cast<AstNode.Number>().Select(e => e.Value.IntValue).ToArray();

        return new BufferTiles {
            Left = list[0],
            Top = list[1],
            Right = list[2],
            Bottom = list[3],
        };
    }


    public static string GetDefaultMaterial(AstNode.Base @base)
    {
        var materialBase = (AstNode.String) ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultmaterial").Value;

        return materialBase.Value;
    }

    
    public static Effect[] GetEffects(AstNode.Base @base, int width, int height) {
        var matrixProp = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "effects").Value;

        var effectList = ((AstNode.List)matrixProp).Values.Select(e => {
            var props = ((AstNode.PropertyList)e).Values;

            var name = ((AstNode.String) props.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
            
            var mtxWidth = ((AstNode.List) props.Single(p => ((AstNode.Symbol)p.Key).Value == "mtrx").Value).Values.Cast<AstNode.List>().ToArray();

            var optionsList =
                ((AstNode.List?)props.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "options").Value)?.Values;

            var options = optionsList
                ?.Cast<AstNode.List>()
                .Select(o =>
                {
                    var optionName = ((AstNode.String)o.Values[0]).Value;
                    var options = ((AstNode.List)o.Values[1]).Values.Cast<AstNode.String>().Select(s => s.Value).ToArray();
                    dynamic choice = o.Values[2] switch
                    {
                        AstNode.String s => s.Value,
                        AstNode.Number n => n.Value.IntValue,
                        AstNode.UnaryOperator u => u.Type == AstNode.UnaryOperatorType.Negate
                            ? ((AstNode.Number)u.Expression).Value.IntValue * -1
                            : ((AstNode.Number)u.Expression).Value.IntValue,
                        
                        var b => throw new Exception($"Invalid effect option choice")
                    };

                    return new EffectOptions(optionName, options, choice);
                }).ToArray() ?? throw new NullReferenceException($"No options found for effect \"{name}\"");
            
            var matrix = new double[height, width];

            for (var x = 0; x < mtxWidth.Length; x++) {
                var mtxHeight = mtxWidth[x].Values.Cast<AstNode.Number>().Select(n => n.Value.DecimalValue).ToArray();

                for (int y = 0; y < mtxHeight.Length; y++) {
                    matrix[y, x] = mtxHeight[y];
                }
            }


            return new Effect() { Name = name, Options = options, Matrix = matrix };
        }).ToArray();
        

        return effectList;
    }
    
    public class LoadFileResult
{
    public bool Success { get; init; } = false;
    
    public int Seed { get; set; }
    public int WaterLevel { get; set; }
    public bool WaterInFront { get; set; }

    public int Width { get; init; } = 0;
    public int Height { get; init; } = 0;

    public BufferTiles BufferTiles { get; init; } = new();
    public Effect[] Effects { get; init; } = [];

    public bool LightMode { get; init; }
    public bool DefaultTerrain { get; set; }
    public MaterialDefinition DefaultMaterial { get; set; }

    public Data.Geometry.Geo[,,]? GeoMatrix { get; init; } = null;
    public Tile[,,]? TileMatrix { get; init; } = null;
    public Data.Color[,,]? MaterialColorMatrix { get; init; } = null;
    public Prop_Legacy[]? PropsArray { get; init; } = null;

    public Image LightMapImage { get; init; }
    
    public (int angle, int flatness) LightSettings { get; init; }

    public List<RenderCamera> Cameras { get; set; } = [];

    public string Name { get; init; } = "New Project";

    public Exception? PropsLoadException { get; init; } = null;
}

    public static async Task<LoadFileResult> LoadProjectAsync(
        string filePath,
        IDictionary<string, TileDefinition> tiles,
        IDictionary<string, InitPropBase> props,
        IDictionary<string, MaterialDefinition> materials
    )
    {
        var text = (await File.ReadAllTextAsync(filePath)).ReplaceLineEndings().Split(Environment.NewLine);

        var lightMapFileName = Path.Combine(Path.GetDirectoryName(filePath)!,
            Path.GetFileNameWithoutExtension(filePath) + ".png");

        if (text.Length < 7) return new LoadFileResult();

        var sizeObjTask = Task.Factory.StartNew(() => 
            LingoParser.Expression.ParseOrThrow(text[6]));

        var objTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[0]));
        var tilesObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[1]));
        var terrainObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[4]));
        var obj2Task = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[5]));
        var effObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[2]));
        var lightObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[3]));
        var camsObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[6]));
        var waterObjTask = Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[7]));
        
        var propsObjTask = string.IsNullOrEmpty(text[8]) ? null : Task.Factory.StartNew(() =>
            LingoParser.Expression.ParseOrThrow(text[8]));

        AstNode.Base sizeObj;
        AstNode.Base obj;
        AstNode.Base tilesObj;
        AstNode.Base terrainModeObj;
        AstNode.Base obj2;
        AstNode.Base effObj;
        AstNode.Base lightObj;
        AstNode.Base camsObj;
        AstNode.Base waterObj;
        AstNode.Base? propsObj;

        Exception? propsLoadException = null;

        try {
            sizeObj = await sizeObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 6", e);
        }

        try {
            obj = await objTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 1", e);
        }

        try {
            tilesObj = await tilesObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 2", e);
        }

        try {
            terrainModeObj = await terrainObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 5", e);
        }

        try {
            obj2 = await obj2Task;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 6", e);
        }

        try {
            effObj = await effObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 3", e);
        }

        try {
            lightObj = await lightObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 4", e);
        }

        try {
            camsObj = await camsObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 7", e);
        }

        try {
            waterObj = await waterObjTask;
        } catch (Exception e) {
            throw new Exception("Failed to parse level project file at line 8", e);
        }

        try {
            propsObj = propsObjTask is null ? null : await propsObjTask;
        } catch (Exception e) {
            propsObj = null;
            propsLoadException = e;
            // throw new Exception("Failed to parse level project file at line 9", e);
        }

        var size = GetLevelSize(sizeObj);


        var mtx = await GeoImporter.GetGeoMatrixAsync(obj);
        
        if (size.width == -1)
        {
            size = (mtx.GetLength(1), mtx.GetLength(0));    
        }

        var tlMtx2 = TileImporter.GetTileMatrix_NoExcept(tilesObj, materials, tiles);
        var defaultMaterial = GetDefaultMaterial(tilesObj);
        var buffers = GetBufferTiles(obj2);
        var terrain = GetTerrainMedium(terrainModeObj);
        var lightMode = GetLightMode(obj2);
        var seed = GetSeed(obj2);
        var waterData = GetWaterData(waterObj);
        var effects = GetEffects(effObj, size.width, size.height);
        var cams = GetCameras(camsObj);

        for (var x = 0; x < size.width; x++) {
            for (var y = 0; y < size.height; y++) {
                for (var z = 0; z < 3; z++) {
                    var body = tlMtx2[y, x, z];

                    if (body.Type is TileCellType.Body)
                    {
                        var (hx, hy, hz) = body.HeadPosition;

                        hx--;
                        hy--;
                        hz--;
                    
                        if (
                            hx < 0 || hx >= tlMtx2.GetLength(1) ||
                            hy < 0 || hy >= tlMtx2.GetLength(0) ||
                            hz < 0 || hz >= 3) continue;

                        var head = tlMtx2[hy, hx, hz];

                        if (head.Type is not TileCellType.Head) continue;

                        var newBody = body with {
                            TileDefinition = head.TileDefinition,
                            UndefinedName = head.UndefinedName     
                        };

                        tlMtx2[y, x, z] = newBody;
                    }
                }
            }
        }

        MaterialDefinition defMatFound = materials["Concrete"];

        foreach (var material in materials.Values) {
            if (string.Equals(defaultMaterial, material.Name, StringComparison.Ordinal)) {
                defMatFound = material;
                goto skipMatSearch;
            }
        }

        skipMatSearch:

        // TODO: catch PropNotFoundException
        var  legacy_props =  PropImporter.GetProps_Legacy(propsObj, props, tiles);

        // var parsed_props = propsObj is null 
        //     ? default! 
        //     : await PropImporter.GetPropsAsync(propsObj, props);

        var lightSettings = GetLightSettings(lightObj);

        // map material colors

        Data.Color[,,] materialColors = Utils.NewMaterialColorMatrix(size.width, size.height, new(0, 0, 0, 255));

        for (int y = 0; y < size.height; y++)
        {
            for (int x = 0; x < size.width; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var cell = tlMtx2[y, x, z];

                    if (cell.Type is not TileCellType.Material) continue;

                    var materialName = cell.MaterialDefinition?.Name ?? cell.UndefinedName;

                    if (materials.TryGetValue(materialName!, out var foundMat))
                        materialColors[y, x, z] = foundMat.Color;
                }
            }
        }

        // Light map

        var lightMap = File.Exists(lightMapFileName) 
            ? Raylib.LoadImage(lightMapFileName) 
            : Raylib.GenImageColor(size.width * 20 + 300, size.height * 20 + 300, new Data.Color());

        //

        return new LoadFileResult
        {
            Success = true,
            Seed = seed,
            WaterLevel = waterData.waterLevel,
            WaterInFront = waterData.waterInFront,
            Width = size.width,
            Height = size.height,
            DefaultMaterial = defMatFound,
            BufferTiles = buffers,
            GeoMatrix = mtx,
            TileMatrix = tlMtx2,
            LightMode = lightMode,
            DefaultTerrain = terrain,
            MaterialColorMatrix = materialColors,
            Effects = effects,
            LightMapImage = lightMap,
            Cameras = cams,
            PropsArray = legacy_props.ToArray(),
            LightSettings = lightSettings,
            Name = Path.GetFileNameWithoutExtension(filePath),
            PropsLoadException = propsLoadException
        };
    }
}