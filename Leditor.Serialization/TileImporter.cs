using Drizzle.Lingo.Runtime.Parser;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;
using Leditor.Serialization.Exceptions;
using Pidgin;
using Color = Leditor.Data.Color;
using ParseException = Leditor.Serialization.Exceptions.ParseException;
using Texture2D = Leditor.RL.Managed.Texture2D;
// ReSharper disable CollectionNeverQueried.Global
// ReSharper disable CollectionNeverUpdated.Global

// ReSharper disable MemberCanBePrivate.Global

namespace Leditor.Serialization;

public class TileImporter
{
    public List<(string name, Color color)> Categories { get; } = [];
    public List<List<TileDefinition>> Definitions { get; } = [];
        
    public async Task<IEnumerable<Func<Task>>> PrepareInitParseAsync(string initPath)
    {
        Categories.Clear();
        Definitions.Clear();
        
        var text = await File.ReadAllTextAsync(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<Func<Task>> tasks = new(lines.Length);

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-'))
            {
                var l1 = l;
                
                tasks.Add(() => Task.Factory.StartNew(() =>
                {
                    AstNode.Base categoryObject;
                    (string name, Color color) category;

                    try
                    {
                        categoryObject = LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));
                    }
                    catch (Exception e)
                    {
                        throw new ParseException($"Failed to parse tile definition category from string at line {l1 + 1}", e);
                    }

                    try
                    {
                        category = GetTileDefinitionCategory(categoryObject);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException(
                            $"Failed to get tile definition category from parsed string at line {l1 + 1}", e);
                    }

                    Categories.Add(category);
                }));
            }
            else
            {
                var l2 = l;
                tasks.Add(() => Task.Factory.StartNew(() =>
                {
                    AstNode.Base tileObject;

                    try
                    {
                        tileObject = LingoParser.Expression.ParseOrThrow(line);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException($"Failed to parse tile definition from string at line {l2 + 1}", e);
                    }

                    try
                    {
                        return GetTile(tileObject);
                    }
                    catch (ParseException e)
                    {
                        throw new ParseException($"Failed to get tile definition from parsed string at line {l2 + 1}", e);
                    }
                }));
            }
        }

        return tasks;
    }
    
    internal static TileDefinition GetTile(AstNode.Base @base)
    {
        if (@base is not AstNode.PropertyList propertyList)
            throw new TileDefinitionParseException("Argument is not a property list");
        
        //

        var nameAst = Utils.TryGet<AstNode.String>(propertyList, "nm");
        var sizeAst = Utils.TryGet<AstNode.GlobalCall>(propertyList, "sz");
        var typeAst = Utils.TryGet<AstNode.String>(propertyList, "tp");
        var specs1Ast = Utils.TryGet<AstNode.List>(propertyList, "specs");
        var specs2Ast = Utils.TryGet<AstNode.List>(propertyList, "specs2");
        var specs3Ast = Utils.TryGet<AstNode.List>(propertyList, "specs3");
        var repeatLAst = Utils.TryGet<AstNode.List>(propertyList, "repeatl");
        var bfTilesAst = Utils.TryGet<AstNode.Number>(propertyList, "bftiles");
        var tagsAst = Utils.TryGet<AstNode.List>(propertyList, "tags");

        var name = nameAst?.Value ?? throw new MissingTileDefinitionPropertyException("nm");
        
        var sizeArgs = sizeAst ?? throw new MissingTileDefinitionPropertyException(name, "sz");
        var size = Utils.GetIntPair(sizeArgs);
        var bfTiles = Utils.GetInt(bfTilesAst ?? throw new MissingTileDefinitionPropertyException(name, "bfTiles"));
        
        // specs
        
        var specs1 = specs1Ast?
            .Values
            .Select((n, index) => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            if (op.Expression is AstNode.Number containedNumber) {
                                return containedNumber.Value.IntValue * -1; 
                            }
                            else throw new TileDefinitionParseException(name, $"Property \"specs\" contains a non-number value at index {index}");

                        } else throw new TileDefinitionParseException(name, $"Invalid property \"specs\" number operator at index {index}");

                    default:
                        throw new TileDefinitionParseException(name, $"Invalid property \"specs\" value at index {index}");
                }
        })
            .ToArray() ?? throw new MissingTileDefinitionPropertyException(name, "specs");
        
        int[] specs2, specs3;

        if (specs2Ast is not null) {
            specs2 = specs2Ast.Values.Select((n, index) => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            if (op.Expression is AstNode.Number containedNumber) {
                                return containedNumber.Value.IntValue * -1; 
                            }
                            else throw new TileDefinitionParseException(name, $"Propert \"specs2\" contains a non-number value at index {index}");
                        } else throw new TileDefinitionParseException(name, $"Invalid property \"specs2\" number operator at index {index}");

                    default:
                        throw new TileDefinitionParseException(name, $"Invalid property \"specs2\" value at index {index}");
                }
        }).ToArray();
        } else { specs2 = []; }
        
        if (specs3Ast is not null) {
            specs3 = specs3Ast.Values.Select((n, index) => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            if (op.Expression is AstNode.Number containedNumber) {
                                return containedNumber.Value.IntValue * -1; 
                            }
                            else throw new TileDefinitionParseException(name, $"Property \"specs3\" contains a non-number value at index {index}");
                        } else throw new TileDefinitionParseException(name, $"Invalid property \"specs\" number operator at index {index}");

                    default:
                        throw new TileDefinitionParseException(name, $"Invalid property \"specs\" value at index {index}");
                }
            }).ToArray();
        } else { specs3 = []; }
        
        // Merge specs
        
        var specs = new int[size.Item2, size.Item1,3];
        
        for (var x = 0; x < size.Item1; x++)
        {
            for (var y = 0; y < size.Item2; y++)
            {
                var index = x * size.Item2 + y;

                try
                {
                    specs[y, x, 0] = specs1[index];
                }
                catch
                {
                    specs[y, x, 0] = -1;
                }

                try
                {
                    specs[y, x, 1] = specs2 is [] ? -1 : specs2[index];
                }
                catch
                {
                    specs[y, x, 1] = -1;
                }

                try
                {
                    specs[y, x, 2] = specs3 is [] ? -1 : specs3[index];
                }
                catch
                {
                    specs[y, x, 2] = -1;
                }
            }
        }
        
        // type

        var typeStr = typeAst?.Value ?? throw new MissingTileDefinitionPropertyException(name, "tp");

        var tp = typeStr switch {
            "box" => TileType.Box,
            "voxelStruct" => TileType.VoxelStruct,
            "voxelStructRandomDisplaceHorizontal" => TileType.VoxelStructRandomDisplaceHorizontal,
            "voxelStructRandomDisplaceVertical" => TileType.VoxelStructRandomDisplaceVertical,
            "voxelStructRockType" => TileType.VoxelStructRockType,
            "voxelStructSandType" => TileType.VoxelStructSandType,

            _ => throw new TileDefinitionParseException(name, $"Invalid property \"tp\" value {typeStr}")
        };
        
        // repeatL

        int[] repeatL;

        if (repeatLAst is not null) {

            repeatL = ((AstNode.List) propertyList.Values
                .First(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value).Values
                .Select((n, index) => {
                        switch (n) {
                            case AstNode.Number number: return number.Value.IntValue;
                            
                            case AstNode.UnaryOperator op:
                                if (op.Type == AstNode.UnaryOperatorType.Negate) 
                                    return ((AstNode.Number)op.Expression).Value.IntValue * -1; 
                                
                                
                                throw new TileDefinitionParseException(name, $"Invalid property \"specs\" number operator at index {index}");

                            default:
                                throw new TileDefinitionParseException(name, $"Invalid property \"specs\" value at index {index}");
                        }
                }).ToArray();

        } 
        else if (tp is TileType.VoxelStruct) throw new MissingTileDefinitionPropertyException(name, "repeatL"); 
        else {
            repeatL = [];
        };
        
        // Tags

        string[] tags = tagsAst?.Values
            .Select((t, index) => {
                if (t is AstNode.String str) return str.Value;

                throw new TileDefinitionParseException(name, $"\"tags\" property contains a non-string value at index {index}");
            }).ToArray() ?? [];

        
        return new TileDefinition(name, size, tp, bfTiles, specs, repeatL, tags);
    }

    /// <summary>
    /// Retrieves the data of the tile definitions without textures
    /// </summary>
    /// <param name="initPath">Tile definition data</param>
    /// <returns>Tile definitions without textures</returns>
    public static ((string name, Color color)[], TileDefinition[][]) ParseInit(string initPath)
    {
        var text = File.ReadAllText(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        (string name, Color color)[] categories = [];
        TileDefinition[][] definitions = [];
        
        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            // Category
            if (line.StartsWith('-'))
            {
                AstNode.Base categoryObject;
                (string name, Color color) category;

                try
                {
                    categoryObject = LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));
                }
                catch (Exception e)
                {
                    throw new ParseException($"Failed to parse tile definition category from string at line {l + 1}", e);
                }

                try
                {
                    category = GetTileDefinitionCategory(categoryObject);
                }
                catch (Exception e)
                {
                    throw new ParseException(
                        $"Failed to get tile definition category from parsed string at line {l + 1}", e);
                }

                categories = [..categories, category];
                definitions = [..definitions, []];
            }
            // Tile
            else
            {
                AstNode.Base tileObject;

                try
                {
                    tileObject = LingoParser.Expression.ParseOrThrow(line);
                }
                catch (Exception e)
                {
                    throw new ParseException($"Failed to parse tile definition from string at line {l + 1}", e);
                }

                try
                {
                    var tile = GetTile(tileObject);
                    definitions[^1] = [..definitions[^1], tile];
                }
                catch (Exception e)
                {
                    throw new ParseException($"Failed to get tile definition from parsed string at line {l + 1}", e);
                }
            }
        }
        
        return (categories, definitions);
    }
    
    internal static (string, Color) GetTileDefinitionCategory(AstNode.Base @base)
    {
        var list = ((AstNode.List)@base).Values;

        var name = ((AstNode.String)list[0]).Value;
        var color = Utils.GetColor((AstNode.GlobalCall)list[1]);

        return (name, new Color(color));
    }

    public static async Task<((string name, Color color)[], TileDefinition[][])> ParseInitAsync(
        string initPath)
    {
        var text = await File.ReadAllTextAsync(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<Task<(string name, Color color)>> categoryTasks = [];
        List<List<Task<TileDefinition>>> definitionTasks = [];

        categoryTasks.Capacity = lines.Length;
        definitionTasks.Capacity = lines.Length;

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-'))
            {
                categoryTasks.Add(Task.Factory.StartNew(() =>
                {
                    AstNode.Base categoryObject;
                    (string name, Color color) category;

                    try
                    {
                        categoryObject = LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));
                    }
                    catch (Exception e)
                    {
                        throw new ParseException($"Failed to parse tile definition category from string at line {l + 1}", e);
                    }

                    try
                    {
                        category = GetTileDefinitionCategory(categoryObject);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException(
                            $"Failed to get tile definition category from parsed string at line {l + 1}", e);
                    }

                    return category;
                }));

                definitionTasks.Add([]);
            }
            else
            {
                definitionTasks[^1].Add(Task.Factory.StartNew(() =>
                {
                    AstNode.Base tileObject;

                    try
                    {
                        tileObject = LingoParser.Expression.ParseOrThrow(line);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException($"Failed to parse tile definition from string at line {l + 1}", e);
                    }

                    try
                    {
                        return GetTile(tileObject);
                    }
                    catch (ParseException e)
                    {
                        throw new ParseException($"Failed to get tile definition from parsed string at line {l + 1}", e);
                    }
                }));
            }
        }
        
        var categories = new (string name, Color color)[categoryTasks.Count];
        var definitions = new TileDefinition[definitionTasks.Count][];

        for (var i = 0; i < categoryTasks.Count; i++) categories[i] = await categoryTasks[i];
        for (var c = 0; c < definitionTasks.Count; c++)
        {
            definitions[c] = new TileDefinition[definitionTasks[c].Count];

            for (var t = 0; t < definitions[c].Length; t++) definitions[c][t] = await definitionTasks[c][t];
        }

        return (categories, definitions);
    }
}