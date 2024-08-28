using Leditor.Serialization.Parser;
using Leditor.Data.Materials;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using Color = Leditor.Data.Color;

namespace Leditor.Serialization;

public static class MaterialImporter
{
    
    public static MaterialDefinition GetInitMaterial(AstNode.Base @base)
    {
        var propertyList = (AstNode.PropertyList)@base;

        var nameBase = (AstNode.Base?) propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "nm").Value;
        var colorBase = (AstNode.Base?) propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "color").Value;

        if (nameBase is null) throw new MissingPropertyParseException("nm");
        if (colorBase is null) throw new MissingPropertyParseException("color");

        var colorGlobalCall = ((AstNode.GlobalCall)colorBase);

        if (colorGlobalCall.Name != "color") throw new InvalidFormatException("Invalid \"color\" value");

        var color = new Color(
            ((AstNode.Number)colorGlobalCall.Arguments[0]).Value.IntValue,
            ((AstNode.Number)colorGlobalCall.Arguments[1]).Value.IntValue,
            ((AstNode.Number)colorGlobalCall.Arguments[2]).Value.IntValue, 
            255
        );

        return new(((AstNode.String)nameBase).Value, color, Data.Materials.MaterialRenderType.CustomUnified);
    }

    public static (string[] categories, MaterialDefinition[][]) GetMaterialInit(string text)
    {
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<string> categories = [];
        List<Data.Materials.MaterialDefinition[]> materials = [];

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-'))
            {
                categories.Add(line.TrimStart('-'));
                materials.Add([]);
            }
            else
            {
                AstNode.Base? @base = null;
                try
                {
                    @base = LingoParser.Expression.ParseOrThrow(line);
                    var material = GetInitMaterial(@base);
                    materials[^1] = [..materials[^1], material];
                }
                catch
                {
                }
            }
        }

        return ([..categories], [..materials]);
    }
    
    public static async Task<MaterialDefinition[]> GetMaterialInitAsync(string initPath, Serilog.ILogger? logger = null)
    {
        var text = await File.ReadAllTextAsync(initPath);
    
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);
    
        List<Task<MaterialDefinition>> matTasks = new(lines.Length);

        var count = 0;

        foreach (var line in lines)
        {
            count++;

            if (string.IsNullOrEmpty(line) || line.StartsWith('-')) continue;

            matTasks.Add(Task.Factory.StartNew(() => {
                MaterialDefinition? init = null;

                try
                {
                    var obj = LingoParser.Expression.ParseOrThrow(line);
                    init = GetInitMaterial(obj);
                }
                catch (Pidgin.ParseException parseExc)
                {
                    logger?.Error(parseExc, $"[MaterialImporter::GetMaterialInitAsync] Failed to parse material init at line {count}");
                }
                catch (Exceptions.ParseException e)
                {
                    logger?.Error(e, $"[MaterialImporter::GetMaterialInitAsync] Failed to import material init at line {count}");
                }

                return init!;
            }));
        }

        await Task.WhenAll(matTasks);

        return matTasks
            .Where(t => t.IsCompletedSuccessfully)
            .Select(t => t.Result)
            .ToArray();
    }
}