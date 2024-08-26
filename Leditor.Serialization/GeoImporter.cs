namespace Leditor.Serialization;

using Drizzle.Lingo.Runtime.Parser;
using Leditor.Data.Materials;
using Leditor.Data.Geometry;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using Color = Leditor.Data.Color;
using ParseException = Leditor.Serialization.Exceptions.ParseException;

public static class GeoImporter
{
    private static GeoFeature GetGeoFeaturesFromList(IEnumerable<int> seq)
    {
        var features = GeoFeature.None;

        foreach (var f in seq) features |= Geo.FeatureID(f);

        return features;
    }

    public static async Task<Geo[,,]> GetGeoMatrixAsync(AstNode.Base @base, Serilog.ILogger? logger = null) {
        if (@base is not AstNode.List) throw new ArgumentException("object is not a list", nameof(@base));

        var columns = ((AstNode.List)@base).Values;

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

        Geo[,,] matrix = new Geo[height, width, 3];

        List<Task> parseTasks = [];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var layer1 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[0];
                var layer2 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[1];
                var layer3 = ((AstNode.List)((AstNode.List)columns[x]).Values[y]).Values[2];

                parseTasks.Add(Task.Run(async () => {
                    if (layer1 is not AstNode.List || layer2 is not AstNode.List || layer3 is not AstNode.List)
                    {
                        logger?.Error($"[GeoImporter::GetGeoMatrixAsync] Invalid layer format (x: {x}, y: {y})");
                        throw new Exception($"layers are not lists at column ({x}), row ({y})");
                    }

                    var cell1 = Task.Factory.StartNew(() => {
                        if (((AstNode.List)layer1).Values[0] is not AstNode.Number || 
                            ((AstNode.List)layer1).Values[1] is not AstNode.List ||
                            ((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                                throw new Exception($"invalid cell at column ({x}), row ({y}), layer (1)");
                        
                        matrix[y, x, 0] = new() { 
                            Type = (GeoType) ((AstNode.Number)((AstNode.List)layer1).Values[0]).Value.IntValue,
                            Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                        };
                    });

                    var cell2 = Task.Factory.StartNew(() => {
                        if (((AstNode.List)layer2).Values[0] is not AstNode.Number || 
                            ((AstNode.List)layer2).Values[1] is not AstNode.List ||
                            ((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                                throw new Exception($"invalid cell at column ({x}), row ({y}), layer (2)");
                        
                        matrix[y, x, 1] = new() {
                            Type = (GeoType) ((AstNode.Number)((AstNode.List)layer2).Values[0]).Value.IntValue,
                            Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                        };
                    });

                    var cell3 = Task.Factory.StartNew(() => {
                        if (((AstNode.List)layer3).Values[0] is not AstNode.Number || 
                            ((AstNode.List)layer3).Values[1] is not AstNode.List ||
                            ((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Any(e => e is not AstNode.Number || ((AstNode.Number)e).Value.IsDecimal))
                                throw new Exception($"invalid cell at column ({x}), row ({y}), layer (3)");

                        matrix[y, x, 2] = new() {
                            Type = (GeoType) ((AstNode.Number)((AstNode.List)layer3).Values[0]).Value.IntValue,
                            Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                        };
                    });

                    await Task.WhenAll(cell1, cell2, cell3);

                    if (cell1.IsFaulted) logger?.Error(cell1.Exception, $"[GeoImporter::GetGeoMatrixAsync] failed to parse geo cell at (x: {x}, y: {y}, z: 0)");
                    if (cell2.IsFaulted) logger?.Error(cell2.Exception, $"[GeoImporter::GetGeoMatrixAsync] failed to parse geo cell at (x: {x}, y: {y}, z: 1)");
                    if (cell3.IsFaulted) logger?.Error(cell3.Exception, $"[GeoImporter::GetGeoMatrixAsync] failed to parse geo cell at (x: {x}, y: {y}, z: 2)");
                }));

            }
        }

        await Task.WhenAll(parseTasks);

        return matrix;
    }
}