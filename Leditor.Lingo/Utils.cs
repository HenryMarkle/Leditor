using Leditor.Lingo.Drizzle;
using Leditor.Common;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Pidgin;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Leditor.Lingo;

public static class Tools {
    public static string StringifyBase(AstNode.Base b) {
        System.Text.StringBuilder sb = new();
        
        switch (b) {
            case AstNode.Number number:
                sb.Append(number.Value);
                break;

            case AstNode.String str:
                sb.Append($"\"{str.Value}\"");
                break;

            case AstNode.GlobalCall call:
                sb.Append($"{call.Name}(");
                
                var stringArgs = sb.AppendJoin(", ", call.Arguments.Select(StringifyBase));

                sb.Append(')');
                break;

            case AstNode.List list:
                sb.Append('[');

                var items = sb.AppendJoin(", ", list.Values.Select(StringifyBase));

                sb.Append(']');
                break;

            case AstNode.Symbol symbol:
                sb.Append($"#{symbol.Value}");
                break;


            case AstNode.PropertyList dict:
                sb.Append('[');

                var props = sb.AppendJoin(", ", dict.Values.Select(p => $"{StringifyBase(p.Key)}: {StringifyBase(p.Value)}"));

                sb.Append(']');
                break;
        }

        return sb.ToString();
    }

    static float NumberToFloat(AstNode.Base number) {
        switch (number) {
            case AstNode.UnaryOperator u:
                if (u.Type == AstNode.UnaryOperatorType.Negate)
                    return (float)((AstNode.Number)u.Expression).Value.DecimalValue * -1;
                else 
                    throw new ArgumentException("argument is not a number", nameof(number));

            case AstNode.Number n: return (float) n.Value.DecimalValue;

            default:
                throw new ArgumentException("argument is not a number", nameof(number));
        }
    }

    static int NumberToInteger(AstNode.Base number) {
        switch (number) {
            case AstNode.UnaryOperator u:
                if (u.Type == AstNode.UnaryOperatorType.Negate)
                    return ((AstNode.Number)u.Expression).Value.IntValue * -1;
                else 
                    throw new ArgumentException("argument is not a number", nameof(number));

            case AstNode.Number n: return n.Value.IntValue;

            default:
                throw new ArgumentException("argument is not a number", nameof(number));
        }
    }

    public static List<RenderCamera> GetCameras(AstNode.Base @base) {
        var props = (AstNode.PropertyList)@base;

        var cameras = ((AstNode.List) props.Values
            .Single(p => ((AstNode.Symbol)p.Key).Value == "cameras")
            .Value)
            .Values
            .Cast<AstNode.GlobalCall>()
            .Select(c => {
                var x = NumberToFloat(c.Arguments[0]);
                var y = NumberToFloat(c.Arguments[1]);

                return (x, y);
            })
            .ToArray();

        var quads = ((AstNode.List) props.Values
            .Single(p => ((AstNode.Symbol)p.Key).Value == "quads")
            .Value)
            .Values
            .Cast<AstNode.List>()
            .Select(l => {
                var currentQuads = l.Values.Cast<AstNode.List>().Select(q => {
                    var angle = NumberToInteger(q.Values[0]);
                    var radius = NumberToFloat(q.Values[1]);

                    return (angle, radius);
                }).ToArray();

                return (currentQuads[0], currentQuads[1], currentQuads[2], currentQuads[3]);
            })
            .ToArray();

        var result = new RenderCamera[cameras.Length];

        for (int c = 0; c < cameras.Length; c++) {
            var currentQuads = quads[c];

            var topLeft = new Vector2((float) (currentQuads.Item1.radius * Math.Cos(currentQuads.Item1.angle)), (float) (currentQuads.Item1.radius * Math.Sin(currentQuads.Item1.angle)));
            var topRight = new Vector2((float) (currentQuads.Item2.radius * Math.Cos(currentQuads.Item2.angle)), (float) (currentQuads.Item2.radius * Math.Sin(currentQuads.Item2.angle)));
            var bottomRight = new Vector2((float) (currentQuads.Item3.radius * Math.Cos(currentQuads.Item3.angle)), (float) (currentQuads.Item3.radius * Math.Sin(currentQuads.Item3.angle)));
            var bottomLeft = new Vector2((float) (currentQuads.Item4.radius * Math.Cos(currentQuads.Item4.angle)), (float) (currentQuads.Item4.radius * Math.Sin(currentQuads.Item4.angle)));

            result[c] = new() {
                Coords = cameras[c],

                Quads = new CameraQuads(
                    topLeft, 
                    topRight, 
                    bottomRight, 
                    bottomLeft)
            };
        }

        return result.ToList();
    }

    public static (string Name, double[,] Matrix)[] GetEffects(AstNode.Base @base, int width, int height) {
        var matrixProp = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "effects").Value;

        var effectList = ((AstNode.List)matrixProp).Values.Select(e => {
            var props = ((AstNode.PropertyList)e).Values;

            var name = ((AstNode.String) props.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
            
            var mtxWidth = ((AstNode.List) props.Single(p => ((AstNode.Symbol)p.Key).Value == "mtrx").Value).Values.Cast<AstNode.List>().ToArray();

            var matrix = new double[height, width];

            for (int x = 0; x < mtxWidth.Length; x++) {
                var mtxHeight = mtxWidth[x].Values.Cast<AstNode.Number>().Select(n => n.Value.DecimalValue).ToArray();

                for (int y = 0; y < mtxHeight.Length; y++) {
                    matrix[y, x] = mtxHeight[y];
                }
            }


            return (name, matrix);
        }).ToArray();

        return effectList;
    }

    public static InitTile GetInitTile(AstNode.Base @base) {
        var propList = (AstNode.PropertyList)@base;

        string name = ((AstNode.String)propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
        
        AstNode.Base[] sizeArgs;

        // try {
        //     sizeArgs = ((AstNode.GlobalCall)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "sz").Value).Arguments;
        // } catch (Exception e) {
        //     if (e.Message.Contains("Sequence contains more than one matching element"))
        //         throw new Exception(innerException: e, message: "Field 'sz' was defined more than once");

        //     throw new Exception(innerException: e, message: "Error retrieving field 'sz': "+e.Message);
        // }

        sizeArgs = ((AstNode.GlobalCall)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "sz").Value).Arguments;
        
        var size = (((AstNode.Number)sizeArgs[0]).Value.IntValue, ((AstNode.Number)sizeArgs[1]).Value.IntValue);
        int[] specs = ((AstNode.List)propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "specs").Value)
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
        }).ToArray();
        var specs2Extracted = propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "specs2").Value;

        int[] specs2;

        if (specs2Extracted is AstNode.List specs2List) {
            specs2 = specs2List.Values.Select(n => {
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

        string tpString = ((AstNode.String)propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;

        InitTileType tp = tpString switch {
            "box" => InitTileType.Box,
            "voxelStruct" => InitTileType.VoxelStruct,
            "voxelStructRandomDisplaceHorizontal" => InitTileType.VoxelStructRandomDisplaceHorizontal,
            "voxelStructRandomDisplaceVertical" => InitTileType.VoxelStructRandomDisplaceVertical,
            "voxelStructRockType" => InitTileType.VoxelStructRockType,

            _ => throw new Exception("Invalid tile init tag: "+ tpString)
        };

        var repeatLOptional = propList.Values.Any(p => ((AstNode.Symbol)p.Key).Value == "repeatl");
    
        int[] repeatL;

        if (repeatLOptional) {
            repeatL = ((AstNode.List) propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value).Values.Select(n => {
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
        } else { repeatL = []; }

        int bfTiles = ((AstNode.Number) propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "bftiles").Value).Value.IntValue;
        string[] tags = ((AstNode.List) propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tags").Value).Values.Cast<AstNode.String>().Select(s => s.Value).ToArray();
    
        int rnd = ((AstNode.Number) propList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "rnd").Value).Value.IntValue;
    
        return new InitTile(name, size, specs, specs2, tp, repeatL, bfTiles, rnd, 0, tags);
    }

    public static ((string, (int, int, int))[], InitTile[][]) GetTileInit(string text) {
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<(string, (int, int, int))> keys = [];
        List<InitTile[]> tiles = [];
        

        foreach (var line in lines) {
            if (string.IsNullOrEmpty(line)) continue;

            if (line.StartsWith('-')) {
                tiles.Add([]);

                var headerList = (AstNode.List) LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));

                string name  = ((AstNode.String) headerList.Values[0]).Value;

                var colorArgs = ((AstNode.GlobalCall) headerList.Values[1]).Arguments;
                var headerColor = (((AstNode.Number)colorArgs[0]).Value.IntValue, ((AstNode.Number)colorArgs[1]).Value.IntValue, ((AstNode.Number)colorArgs[2]).Value.IntValue);

                keys.Add((name, headerColor));
            } else {
                var obj = LingoParser.Expression.ParseOrThrow(line);

                var tile = GetInitTile(obj);

                tiles[^1] = [..tiles[^1], tile ];
            }
        }

        return (keys.ToArray(), tiles.ToArray());
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

    public static RunCell[,,] GetGeoMatrix(AstNode.Base @base, out int height, out int width) {
        if (@base is not AstNode.List) throw new ArgumentException("object is not a list", nameof(@base));

        var columns = ((AstNode.List)@base).Values;

        height = 0;
        width = columns.Length;

        for (int x = 0; x < columns.Length; x++) {
            if (columns[x] is not AstNode.List) throw new Exception($"column ({x}) is not a list");

            for (int y = 0; y < ((AstNode.List)columns[x]).Values.Length; y++) {
                height = ((AstNode.List)columns[x]).Values.Length;


                if (((AstNode.List)((AstNode.List)columns[x]).Values[y]) is not AstNode.List)
                    throw new Exception($"element ({y}) at row ({x}) is not a list");
            }
        }

        RunCell[,,] matrix = new RunCell[height, width, 3];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.List || layer2 is not AstNode.List || layer3 is not AstNode.List)
                    throw new Exception($"layers are not lists at column ({x}), row ({y})");

                if (((AstNode.List)layer1).Values[0] is not AstNode.Number || 
                    ((AstNode.List)layer1).Values[1] is not AstNode.List ||
                    ((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                        throw new Exception($"invalid cell at column ({x}), row ({y}), layer (1)");

                if (((AstNode.List)layer2).Values[0] is not AstNode.Number || 
                    ((AstNode.List)layer2).Values[1] is not AstNode.List ||
                    ((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                        throw new Exception($"invalid cell at column ({x}), row ({y}), layer (2)");

                if (((AstNode.List)layer3).Values[0] is not AstNode.Number || 
                    ((AstNode.List)layer3).Values[1] is not AstNode.List ||
                    ((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                        throw new Exception($"invalid cell at column ({x}), row ({y}), layer (3)");

                matrix[y, x, 0] = new() { 
                    Geo = ((AstNode.Number)((AstNode.List)layer1).Values[0]).Value.IntValue,
                    Stackables = CommonUtils.DecomposeStackables(((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 1] = new() {
                    Geo = ((AstNode.Number)((AstNode.List)layer2).Values[0]).Value.IntValue,
                    Stackables = CommonUtils.DecomposeStackables(((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 2] = new() {
                    Geo = ((AstNode.Number)((AstNode.List)layer3).Values[0]).Value.IntValue,
                    Stackables = CommonUtils.DecomposeStackables(((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };
            }
        }

        return matrix;
    }

    public static TileCell GetTileCell(AstNode.Base @base) {
        string tp = ((AstNode.String)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;
        AstNode.Base data = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "data").Value;

        dynamic casted;

        switch (tp) {
            case "default": 
                casted = new TileDefault();
                break;
            
            case "material": 
                casted = new TileMaterial(((AstNode.String)data).Value);
                break;

            case "tileHead":
                AstNode.Base[] asList = ((AstNode.List)data).Values;

                string name = ((AstNode.String)asList[1]).Value;
                
                AstNode.Base[] pointArgs = ((AstNode.GlobalCall)asList[0]).Arguments;
                var category = ((AstNode.Number)pointArgs[0]).Value.IntValue;
                var position = ((AstNode.Number)pointArgs[1]).Value.IntValue;

                casted = new TileHead(category, position, name);
                break;

            case "tileBody":
                AstNode.Base[] asList2 = ((AstNode.List)data).Values;

                var zPosition = ((AstNode.Number)asList2[1]).Value.IntValue;
                
                AstNode.Base[] pointArgs2 = ((AstNode.GlobalCall)asList2[0]).Arguments;
                var category2 = ((AstNode.Number)pointArgs2[0]).Value.IntValue;
                var position2 = ((AstNode.Number)pointArgs2[1]).Value.IntValue;

                casted = new TileBody(category2, position2, zPosition);
                break;

            default:
                throw new Exception($"unknown tile type: \"{tp}\"");
        };

        return tp switch {
            "default" => new TileCell() { Type = TileType.Default, Data = casted  },
            "material" => new TileCell() { Type = TileType.Material, Data = casted },
            "tileHead" => new TileCell() { Type = TileType.TileHead, Data = casted },
            "tileBody" => new TileCell() { Type = TileType.TileBody, Data = casted },
            _ => throw new Exception($"unknown tile type: \"{tp}\"")
        };
    }

    public static TileCell[,,] GetTileMatrix(AstNode.Base @base, out int height, out int width) {
        if (@base is not AstNode.PropertyList) throw new ArgumentException("object is not a property list", nameof(@base));

        var tlMatrix = (AstNode.List)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

        var columns = tlMatrix.Values;

        height = 0;
        width = columns.Length;

        for (int x = 0; x < columns.Length; x++) {
            if (columns[x] is not AstNode.List) throw new Exception($"column ({x}) is not a list");

            for (int y = 0; y < ((AstNode.List)columns[x]).Values.Length; y++) {
                height = ((AstNode.List)columns[x]).Values.Length;


                if (((AstNode.List)((AstNode.List)columns[x]).Values[y]) is not AstNode.List)
                    throw new Exception($"element ({y}) at row ({x}) is not a list");
            }
        }

        TileCell[,,] matrix = new TileCell[height, width, 3];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                if (layer1 is not AstNode.PropertyList || layer2 is not AstNode.PropertyList || layer3 is not AstNode.PropertyList)
                    throw new Exception($"layers are not property lists at column ({x}), row ({y})");

                var tile1 = GetTileCell(layer1);
                var tile2 = GetTileCell(layer2);
                var tile3 = GetTileCell(layer3);

                matrix[y, x, 0] = tile1;
                matrix[y, x, 1] = tile2;
                matrix[y, x, 2] = tile3;
            }
        }

        return matrix;
    }
}