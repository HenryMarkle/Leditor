using Leditor.Serialization.Parser;
using Leditor.Data.Materials;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using Color = Leditor.Data.Color;
using ParseException = Leditor.Serialization.Exceptions.ParseException;

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
        var rndAst = Utils.TryGet<AstNode.Number>(propertyList, "rnd");

        var name = nameAst?.Value ?? throw new MissingTileDefinitionPropertyException("nm");
        
        var sizeArgs = sizeAst ?? throw new MissingTileDefinitionPropertyException(name, "sz");
        var size = Utils.GetIntPair(sizeArgs);
        var bfTiles = Utils.GetInt(bfTilesAst ?? throw new MissingTileDefinitionPropertyException(name, "bfTiles"));
        var rnd = rndAst is null ? 0 : Utils.GetInt(rndAst);
        
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

        
        return new TileDefinition(name, size, tp, bfTiles, specs, repeatL, tags, rnd);
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
            var lineNumber = l + 1;
            
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
                        throw new ParseException($"Failed to parse tile definition category from string at line {lineNumber}", e);
                    }

                    try
                    {
                        category = GetTileDefinitionCategory(categoryObject);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException(
                            $"Failed to get tile definition category from parsed string at line {lineNumber}", e);
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
                        throw new ParseException($"Failed to parse tile definition from string at line {lineNumber}", e);
                    }

                    try
                    {
                        return GetTile(tileObject);
                    }
                    catch (ParseException e)
                    {
                        throw new ParseException($"Failed to get tile definition from parsed string at line {lineNumber}", e);
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

    public static async Task<TileDefinition[]> ParseInitAsync_NoCategories(string initPath, Serilog.ILogger? logger = null)
    {
        var initData = await File.ReadAllTextAsync(initPath);

        if (string.IsNullOrEmpty(initData))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = initData.ReplaceLineEndings().Split(Environment.NewLine);

        List<Task<TileDefinition>> tileTasks = new(lines.Length);

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            var lineNumber = line + 1;
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-')) continue;

            tileTasks.Add(Task.Factory.StartNew(() =>
            {
                AstNode.Base tileObject;

                try
                {
                    tileObject = LingoParser.Expression.ParseOrThrow(line);
                }
                catch (Exception e)
                {
                    logger?.Error(e, $"[ParseInitAsync_NoCategories] Failed to parse tile definition from string at line {lineNumber}");
                    throw new ParseException($"[ParseInitAsync_NoCategories] Failed to parse tile definition from string at line {lineNumber}", e);
                }

                try
                {
                    return GetTile(tileObject);
                }
                catch (ParseException e)
                {
                    logger?.Error(e, $"[ParseInitAsync_NoCategories] Failed to get tile definition from parsed string at line {lineNumber}");

                    throw new ParseException($"[ParseInitAsync_NoCategories] Failed to get tile definition from parsed string at line {lineNumber}", e);
                }
            }));
            
        }
        
        await Task.WhenAll(tileTasks);

        return tileTasks
            .Where(t => t.IsCompletedSuccessfully)
            .Select(t => t.Result)
            .ToArray();
    }

    public static Tile GetTileCell_NoExcept(
        AstNode.Base node,
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles,
        Serilog.ILogger? logger = null
    )
    {
        string tp = ((AstNode.String)((AstNode.PropertyList)node).Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;
        AstNode.Base data = ((AstNode.PropertyList)node).Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "data").Value;

        Tile cell;

        switch (tp) {
            case "default":
                cell = new();                
                break;
            
            case "material": 
            {
                var name = ((AstNode.String)data).Value;

                if (!materials.TryGetValue(name, out var material))
                {
                    logger?.Warning($"[TileImporter::GetTileCell_NoExcept] Undefined material \"{name}\"");
                }

                cell = new Tile(material, name);
                break;
            }

            case "tileHead":
            {
                AstNode.Base[] asList = ((AstNode.List)data).Values;

                var name2 = ((AstNode.String)asList[1]).Value;

                if (!tiles.TryGetValue(name2, out var tileDefinition))
                {
                    logger?.Warning($"[TileImporter::GetTileCell_NoExcept] Undefined tile \"{name2}\"");
                }

                var secondChainHolder = (tileDefinition!.Tags.Contains("Chain Holder") && asList.Length > 2)
                    ? (asList[2] is AstNode.GlobalCall gc ? Utils.GetIntPair(gc) : (-1, -1)) 
                    : (-1, -1);;

                cell = new Tile(tileDefinition, name2) { SecondChainHolderPosition = secondChainHolder };
                break;
            }

            case "tileBody":
            {
                AstNode.Base[] asList2 = ((AstNode.List)data).Values;

                var zPosition = ((AstNode.Number)asList2[1]).Value.IntValue;
                
                var (category, position) = Utils.GetIntPair((AstNode.GlobalCall)asList2[0]);

                cell = new(category, position, zPosition);
                break;
            }

            default:
                throw new Exception($"Unknown tile type: \"{tp}\"");
        };

        return cell;
    }

    public static Tile GetTileCell(
        AstNode.Base node,
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles
    )
    {
        string tp = ((AstNode.String)((AstNode.PropertyList)node).Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;
        AstNode.Base data = ((AstNode.PropertyList)node).Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "data").Value;

        Tile cell;

        switch (tp) {
            case "default":
                cell = new();                
                break;
            
            case "material": 
            {
                var name = ((AstNode.String)data).Value;

                if (!materials.TryGetValue(name, out var material))
                    throw new MaterialNotFoundException(name);

                cell = new(material);
                break;
            }

            case "tileHead":
            {
                AstNode.Base[] asList = ((AstNode.List)data).Values;

                var name2 = ((AstNode.String)asList[1]).Value;
                
                if (!tiles.TryGetValue(name2, out var tileDefinition))
                    throw new TileNotFoundException(name2);

                var secondChainHolder = (tileDefinition!.Tags.Contains("Chain Holder") && asList.Length > 2)
                    ? (asList[2] is AstNode.GlobalCall gc ? Utils.GetIntPair(gc) : (-1, -1)) 
                    : (-1, -1);

                cell = new(tileDefinition) { SecondChainHolderPosition = secondChainHolder };
                break;
            }

            case "tileBody":
            {
                AstNode.Base[] asList2 = ((AstNode.List)data).Values;

                var zPosition = ((AstNode.Number)asList2[1]).Value.IntValue;
                
                var (category, position) = Utils.GetIntPair((AstNode.GlobalCall)asList2[0]);

                cell = new(category, position, zPosition);
                break;
            }

            default:
                throw new Exception($"Unknown tile type: \"{tp}\"");
        };

        return cell;
    }

    public static Tile[,,] GetTileMatrix(
        AstNode.Base node, 
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles
    )
    {
        if (node is not AstNode.PropertyList) throw new ArgumentException("object is not a property list", nameof(node));

        var tlMatrix = (AstNode.List)((AstNode.PropertyList)node).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

        var columns = tlMatrix.Values;

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

        Tile[,,] matrix = new Tile[height, width, 3];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {

                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.PropertyList || layer2 is not AstNode.PropertyList || layer3 is not AstNode.PropertyList)
                    throw new Exception($"Layers are not property lists at column ({x}), row ({y})");

                var tile1 = GetTileCell(layer1, materials, tiles);
                var tile2 = GetTileCell(layer2, materials, tiles);
                var tile3 = GetTileCell(layer3, materials, tiles);

                matrix[y, x, 0] = tile1;
                matrix[y, x, 1] = tile2;
                matrix[y, x, 2] = tile3;
            }
        }

        return matrix;
    }

    public static Tile[,,] GetTileMatrix_NoExcept(
        AstNode.Base node, 
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles,
        Serilog.ILogger? logger = null
    )
    {
        if (node is not AstNode.PropertyList) throw new ArgumentException("object is not a property list", nameof(node));

        var tlMatrix = (AstNode.List)((AstNode.PropertyList)node).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

        var columns = tlMatrix.Values;

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

        Tile[,,] matrix = new Tile[height, width, 3];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.PropertyList || layer2 is not AstNode.PropertyList || layer3 is not AstNode.PropertyList)
                    throw new Exception($"Layers are not property lists at column ({x}), row ({y})");

                var tile1 = GetTileCell_NoExcept(layer1, materials, tiles, logger);
                var tile2 = GetTileCell_NoExcept(layer2, materials, tiles, logger);
                var tile3 = GetTileCell_NoExcept(layer3, materials, tiles, logger);

                matrix[y, x, 0] = tile1;
                matrix[y, x, 1] = tile2;
                matrix[y, x, 2] = tile3;
            }
        }

        return matrix;
    }

    public static async Task<Tile[,,]> GetTileMatrixAsync(
        AstNode.Base node, 
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles
    )
    {
        if (node is not AstNode.PropertyList) throw new ArgumentException("object is not a property list", nameof(node));

        var tlMatrix = (AstNode.List)((AstNode.PropertyList)node).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

        var columns = tlMatrix.Values;

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

        Tile[,,] matrix = new Tile[height, width, 3];

        List<Task> parseTasks = [];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.PropertyList || layer2 is not AstNode.PropertyList || layer3 is not AstNode.PropertyList)
                    throw new Exception($"Layers are not property lists at column ({x}), row ({y})");

                parseTasks.Add(Task.Run(async () => {
                    var tile1 = Task.Factory.StartNew(() => GetTileCell(layer1, materials, tiles));
                    var tile2 = Task.Factory.StartNew(() => GetTileCell(layer2, materials, tiles));
                    var tile3 = Task.Factory.StartNew(() => GetTileCell(layer3, materials, tiles));

                    matrix[y, x, 0] = await tile1;
                    matrix[y, x, 1] = await tile2;
                    matrix[y, x, 2] = await tile3;
                }));
            }
        }

        await Task.WhenAll(parseTasks);

        return matrix;
    }
    public static async Task<Tile[,,]> GetTileMatrixAsync_NoExcept(
        AstNode.Base node, 
        IDictionary<string, MaterialDefinition> materials,
        IDictionary<string, TileDefinition> tiles,
        Serilog.ILogger? logger
    )
    {
        if (node is not AstNode.PropertyList) throw new ArgumentException("object is not a property list", nameof(node));

        var tlMatrix = (AstNode.List)((AstNode.PropertyList)node).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

        var columns = tlMatrix.Values;

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

        Tile[,,] matrix = new Tile[height, width, 3];

        List<Task> parseTasks = [];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.PropertyList || layer2 is not AstNode.PropertyList || layer3 is not AstNode.PropertyList)
                    throw new Exception($"Layers are not property lists at column ({x}), row ({y})");

                parseTasks.Add(Task.Run(async () => {
                    var tile1 = Task.Factory.StartNew(() => GetTileCell_NoExcept(layer1, materials, tiles, logger));
                    var tile2 = Task.Factory.StartNew(() => GetTileCell_NoExcept(layer2, materials, tiles, logger));
                    var tile3 = Task.Factory.StartNew(() => GetTileCell_NoExcept(layer3, materials, tiles, logger));

                    matrix[y, x, 0] = await tile1;
                    matrix[y, x, 1] = await tile2;
                    matrix[y, x, 2] = await tile3;
                }));
            }
        }

        await Task.WhenAll(parseTasks);

        return matrix;
    }
}