using Drizzle.Lingo.Runtime.Parser;
using Leditor.Data;
using Leditor.Data.Props.Definitions;
using Leditor.Serialization.Exceptions;
using Pidgin;
using ParseException = Leditor.Serialization.Exceptions.ParseException;

namespace Leditor.Serialization;

public static class PropImporter
{
    internal static PropDefinition GetProp(AstNode.PropertyList properties)
    {
        var nameAst = Utils.TryGet<AstNode.String>(properties, "nm") ??
                      throw new MissingPropDefinitionPropertyException("nm");
        
        var typeAst = Utils.TryGet<AstNode.String>(properties, "tp") ??
                      throw new MissingPropDefinitionPropertyException("tp");

        var depthAst = Utils.TryGet<AstNode.Base>(properties, "depth");

        var name = nameAst.Value;
        var depth = depthAst is null ? 0 : Utils.GetInt(depthAst);
        
        switch (typeAst.Value)
        {
            case "standard":
            {
                var sizeAst = Utils.TryGet<AstNode.GlobalCall>(properties, "sz") ??
                              throw new MissingPropDefinitionPropertyException("sz");
                
                var repeatAst = Utils.TryGet<AstNode.List>(properties, "repeatl") ??
                                throw new MissingPropDefinitionPropertyException("repeatl");

                return new Standard(
                    name, 
                    depth,
                    Utils.GetIntPair(sizeAst),
                    repeatAst.Values.Select(Utils.GetInt).ToArray()
                );
            }

            case "variedStandard":
            {
                var sizeAst = Utils.TryGet<AstNode.GlobalCall>(properties, "sz") ??
                              throw new MissingPropDefinitionPropertyException("sz");
                
                var repeatAst = Utils.TryGet<AstNode.List>(properties, "repeatl") ??
                                throw new MissingPropDefinitionPropertyException("repeatl");

                var varsAst = Utils.TryGet<AstNode.Base>(properties, "vars");
                var randomAst = Utils.TryGet<AstNode.Base>(properties, "random");
                
                return new VariedStandard(
                    name,
                    depth,
                    Utils.GetIntPair(sizeAst),
                    repeatAst.Values.Select(Utils.GetInt).ToArray(),
                    varsAst is null ? 1 : Utils.GetInt(varsAst),
                    randomAst is not null && Utils.GetInt(randomAst) != 0
                );
            }

            case "soft": return new Soft(name, depth);

            case "variedSoft":
            {
                var varsAst = Utils.TryGet<AstNode.Base>(properties, "vars");
                var randomAst = Utils.TryGet<AstNode.Base>(properties, "random");

                var colorizeAst = Utils.TryGet<AstNode.Base>(properties, "colorize");
                
                var sizeAst = Utils.TryGet<AstNode.GlobalCall>(properties, "pxlsize") ??
                              throw new MissingPropDefinitionPropertyException("pxlsize");

                var colorize = colorizeAst is not null && Utils.GetInt(colorizeAst) != 0;
                var size = Utils.GetIntPair(sizeAst);
                
                return new VariedSoft(
                    name,
                    depth,
                    varsAst is null ? 1 : Utils.GetInt(varsAst),
                    randomAst is not null && Utils.GetInt(randomAst) != 0,
                    colorize,
                    size
                );
            }

            case "simpleDecal": return new SimpleDecal(name, depth);

            case "variedDecal":
            {
                var sizeAst = Utils.TryGet<AstNode.GlobalCall>(properties, "pxlsize") ??
                              throw new MissingPropDefinitionPropertyException("pxlsize");
                
                var varsAst = Utils.TryGet<AstNode.Base>(properties, "vars");
                var randomAst = Utils.TryGet<AstNode.Base>(properties, "random");
                
                var size = Utils.GetIntPair(sizeAst);
                
                return new VariedDecal(
                    name,
                    depth,
                    size,
                    varsAst is null ? 1 : Utils.GetInt(varsAst),
                    randomAst is not null && Utils.GetInt(randomAst) != 0
                );
            }
                break;   
            
            case "antimatter": return new Antimatter(name, depth);    
            case "softEffect": return new SoftEffect(name, depth);

            case "long": return new Long(name, depth);
            case "coloredSoft":
            {
                var sizeAst = Utils.TryGet<AstNode.GlobalCall>(properties, "pxlsize") ??
                              throw new MissingPropDefinitionPropertyException("pxlsize");
                
                var colorizeAst = Utils.TryGet<AstNode.Base>(properties, "colorize");
                
                var size = Utils.GetIntPair(sizeAst);
                var colorize = colorizeAst is not null && Utils.GetInt(colorizeAst) != 0;
                
                return new ColoredSoft(
                    name,
                    depth,
                    size,
                    colorize
                );
            }
            
            default: throw new InvalidPropTypeException(typeAst.Value);
        }
    }
    public static (string, Color) GetPropDefinitionCategory(AstNode.Base @base)
    {
        var list = ((AstNode.List)@base).Values;

        var name = ((AstNode.String)list[0]).Value;
        var color = Utils.GetColor((AstNode.GlobalCall)list[1]);

        return (name, new Color(color));
    }

    public static ((string, Color)[], PropDefinition[][]) ParseInit(string initPath)
    {
        var text = File.ReadAllText(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        (string name, Color color)[] categories = [];
        PropDefinition[][] definitions = [];

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

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
                    category = GetPropDefinitionCategory(categoryObject);
                }
                catch (Exception e)
                {
                    throw new ParseException(
                        $"Failed to get tile definition category from parsed string at line {l + 1}", e);
                }
                
                categories = [..categories, category];
                definitions = [..definitions, []];
            }
            else
            {
                AstNode.Base propObject;

                try
                {
                    propObject = LingoParser.Expression.ParseOrThrow(line);
                }
                catch (Exception e)
                {
                    throw new ParseException($"Failed to parse tile definition from string at line {l + 1}", e);
                }
                
                try
                {
                    if (propObject is not AstNode.PropertyList propList) 
                        throw new ParseException($"Prop definition is not a property list at line {l + 1}");
                    
                    var tile = GetProp(propList);
                    definitions[^1] = [..definitions[^1], tile];
                }
                catch (ParseException e)
                {
                    throw new ParseException($"Failed to get tile definition from parsed string at line {l + 1}", e);
                }
            }
        }
        
        return (categories, definitions);
    }
    
    public static async Task<((string name, Color color)[], PropDefinition[][])> ParseInitAsync(string initPath)
    {
        var text = await File.ReadAllTextAsync(initPath);
        
        if (string.IsNullOrEmpty(text))
            throw new Exception("Tile definitions text cannot be empty");

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<Task<(string name, Color color)>> categoryTasks = [];
        List<List<Task<PropDefinition>>> definitionTasks = [];

        categoryTasks.Capacity = lines.Length;
        definitionTasks.Capacity = lines.Length;

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-'))
            {
                var l1 = l;
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
                        throw new ParseException($"Failed to parse tile definition category from string at line {l1 + 1}", e);
                    }

                    try
                    {
                        category = GetPropDefinitionCategory(categoryObject);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException(
                            $"Failed to get tile definition category from parsed string at line {l1 + 1}", e);
                    }

                    return category;
                }));

                definitionTasks.Add([]);
            }
            else
            {
                var l1 = l;
                definitionTasks[^1].Add(Task.Factory.StartNew(() =>
                {
                    AstNode.Base propObject;

                    try
                    {
                        propObject = LingoParser.Expression.ParseOrThrow(line);
                    }
                    catch (Exception e)
                    {
                        throw new ParseException($"Failed to parse tile definition from string at line {l1 + 1}", e);
                    }

                    try
                    {
                        if (propObject is not AstNode.PropertyList propList)
                            throw new ParseException($"Prop definition is not a property list at line {l1 + 1}");
                        
                        return GetProp(propList);
                    }
                    catch (ParseException e)
                    {
                        throw new ParseException($"Failed to get tile definition from parsed string at line {l1 + 1}", e);
                    }
                }));
            }
        }
        
        var categories = new (string name, Color color)[categoryTasks.Count];
        var definitions = new PropDefinition[definitionTasks.Count][];

        for (var i = 0; i < categoryTasks.Count; i++) categories[i] = await categoryTasks[i];
        for (var c = 0; c < definitionTasks.Count; c++)
        {
            definitions[c] = new PropDefinition[definitionTasks[c].Count];

            for (var t = 0; t < definitions[c].Length; t++) definitions[c][t] = await definitionTasks[c][t];
        }

        return (categories, definitions);
    }
}