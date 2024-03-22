using System.Diagnostics.Contracts;
using Drizzle.Lingo.Runtime.Parser;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using Raylib_cs;
using Color = Leditor.Data.Color;
using ParseException = Leditor.Serialization.Exceptions.ParseException;
using Texture2D = Leditor.RL.Managed.Texture2D;

// ReSharper disable MemberCanBePrivate.Global

namespace Leditor.Serialization;

// ReSharper disable once RedundantNullableDirective
#nullable enable

public static class TileImporter
{
    [Pure]
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

        var sizeArgs = sizeAst ?? throw new MissingTileDefinitionPropertyException("sz");
        
        var name = nameAst?.Value ?? throw new MissingTileDefinitionPropertyException("nm");
        var size = Utils.GetIntPair(sizeArgs);
        var bfTiles = Utils.GetInt(bfTilesAst ?? throw new MissingTileDefinitionPropertyException("bfTiles"));
        
        // specs
        
        var specs1 = specs1Ast?
            .Values
            .Select(n => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            return ((AstNode.Number)op.Expression).Value.IntValue * -1; 
                        } else throw new Exception("Invalid number operator");

                    default:
                        throw new Exception("Invalid specs value");
                }
        })
            .ToArray() ?? throw new MissingTileDefinitionPropertyException("specs");
        
        int[] specs2, specs3;

        if (specs2Ast is not null) {
            specs2 = specs2Ast.Values.Select(n => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            return ((AstNode.Number)op.Expression).Value.IntValue * -1; 
                        } else throw new Exception("Invalid number operator");

                    default:
                        throw new Exception("Invalid specs value");
                }
        }).ToArray();
        } else { specs2 = []; }
        
        if (specs3Ast is not null) {
            specs3 = specs3Ast.Values.Select(n => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            return ((AstNode.Number)op.Expression).Value.IntValue * -1; 
                        } else throw new Exception("Invalid number operator");

                    default:
                        throw new Exception("Invalid specs value");
                }
            }).ToArray();
        } else { specs3 = []; }

        // type

        var typeStr = typeAst?.Value ?? throw new MissingTileDefinitionPropertyException("tp");

        var tp = typeStr switch {
            "box" => TileType.Box,
            "voxelStruct" => TileType.VoxelStruct,
            "voxelStructRandomDisplaceHorizontal" => TileType.VoxelStructRandomDisplaceHorizontal,
            "voxelStructRandomDisplaceVertical" => TileType.VoxelStructRandomDisplaceVertical,
            "voxelStructRockType" => TileType.VoxelStructRockType,
            "voxelStructSandType" => TileType.VoxelStructSandType,

            _ => throw new InvalidTileTypeException(typeStr)
        };
        
        // repeatL

        int[] repeatL;

        if (repeatLAst is not null) {
            repeatL = ((AstNode.List) propertyList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value).Values.Select(n => {
                switch (n) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) 
                            return ((AstNode.Number)op.Expression).Value.IntValue * -1; 
                        
                        
                        throw new ParseException("Invalid number operator");

                    default:
                        throw new ParseException("Invalid specs value");
                }
        }).ToArray();
        } else { repeatL = []; }
        
        return new TileDefinition(name, size, tp, bfTiles, specs1, specs2, specs3, repeatL);
    }
    
    [Pure]
    internal static (string, Color) GetTileDefinitionCategory(AstNode.Base @base)
    {
        var list = ((AstNode.List)@base).Values;

        var name = ((AstNode.String)list[0]).Value;
        var color = Utils.GetColor((AstNode.GlobalCall)list[1]);

        return (name, new Color(color));
    }
    
    [Pure]
    public static TileDex ImportTileDefinitions(
        string initPath, 
        string texturesDirectory, 
        bool strict = false)
    {
        var text = File.ReadAllText(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        var builder = new TileDexBuilder();

        var lastCategory = "";

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (line.StartsWith("--")) continue;

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
                    throw new ParseException($"Failed to get tile definition category from parsed string at line {l + 1}", e);
                }

                builder.Register(category.name, category.color);
                lastCategory = category.name;
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
                    builder.Register(
                        lastCategory, 
                        tile, 
                        new Texture2D(Path.Combine(texturesDirectory, $"{tile.Name}.png"))
                    );
                }
                catch (ParseException e)
                {
                    if (strict) throw new ParseException($"Failed to get tile definition from parsed string at line {l + 1}", e);
                }
            }
        }

        return builder.Build();
    }
}