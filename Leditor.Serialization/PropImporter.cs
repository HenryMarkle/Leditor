using System.Diagnostics.Contracts;
using Drizzle.Lingo.Runtime.Parser;
using Leditor.Data;
using Leditor.Data.Props;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Settings;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using ParseException = Leditor.Serialization.Exceptions.ParseException;
using PropNotFoundException = Leditor.Data.Props.Exceptions.PropNotFoundException;

namespace Leditor.Serialization;

#nullable enable

public static class PropImporter
{
    internal static PropDefinition GetPropDefinition(AstNode.PropertyList properties)
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

    [Pure]
    internal static PropSettings GetPropSettings(in AstNode.PropertyList settings, in PropDefinition definition)
    {
        var renderOrderAst = Utils.Get<AstNode.Base>(settings, "renderorder");
        var seedAst = Utils.Get<AstNode.Base>(settings, "seed");
        var renderTimeAst = Utils.Get<AstNode.Base>(settings, "rendertime");

        var renderOrder = Utils.GetInt(renderOrderAst);
        var seed = Utils.GetInt(seedAst);
        var renderTime = Utils.GetInt(renderTimeAst);
        
        
        var ropePointsAst = Utils.TryGet<AstNode.List>(settings, "points");
        var ropeReleaseAst = Utils.TryGet<AstNode.Base>(settings, "release");

        var variationAst = Utils.TryGet<AstNode.Number>(settings, "variation");
        var variation = 1;
        
        if (definition is IVaried && Utils.TryGetInt(variationAst, out var parsedVariation))
            variation = parsedVariation;
        
        var customDepthAst = Utils.TryGet<AstNode.Base>(settings, "customdepth");
        var applyColorAst = Utils.TryGet<AstNode.Number>(settings, "applycolor");
        var thicknessAst = Utils.TryGet<AstNode.Number>(settings, "thickness");
        
        switch (definition)
        {
            case Standard standard: return standard.NewSettings(renderOrder, seed, renderTime);

            case VariedStandard variedStandard:
            {
                var propSettings = (VariedStandardSettings) variedStandard.NewSettings(renderOrder, seed, renderTime);

                propSettings.Variation = variation;

                return propSettings;
            }
            
            case Soft soft: return soft.NewSettings(renderOrder, seed, renderTime);

            case VariedSoft variedSoft:
            {
                var propSettings = (VariedSoftSettings) variedSoft.NewSettings(renderOrder, seed, renderTime);

                propSettings.Variation = variation;

                propSettings.ApplyColor = Utils.TryGetInt(applyColorAst, out var applyColor) ? applyColor : 0;

                propSettings.CustomDepth = Utils.TryGetInt(customDepthAst, out var customDepth) ? customDepth : 0;
                
                return propSettings;
            }
            
            case SimpleDecal simpleDecal: return simpleDecal.NewSettings(renderOrder, seed, renderTime);

            case VariedDecal variedDecal:
            {
                var propSettings = (VariedDecalSettings)variedDecal.NewSettings(renderOrder, seed, renderTime);

                propSettings.Variation = variation;
                
                propSettings.CustomDepth = Utils.TryGetInt(customDepthAst, out var customDepth) ? customDepth : 0;

                return propSettings;
            }
            
            case SoftEffect softEffect: return softEffect.NewSettings(renderOrder, seed, renderTime);

            case ColoredSoft coloredSoft: return coloredSoft.NewSettings(renderOrder, seed, renderTime);

            case Rope rope:
            {
                var propSettings = (RopeSettings) rope.NewSettings(renderOrder, seed, renderTime);
                
                if (ropePointsAst is null) 
                    throw new PropParseException($"Prop \"{definition.Name}\" is of rope type, yet has no rope points");
                
                var points = ropePointsAst.Values
                    .Cast<AstNode.GlobalCall>()
                    .Select(Utils.GetIntVector)
                    .ToArray();

                propSettings.Points = points;

                propSettings.Release = Utils.TryGetInt(ropeReleaseAst, out var release)
                    ? (RopeRelease)release
                    : RopeRelease.None;

                if (definition.Name is "wire" or "Zero-G Wire")
                {
                    propSettings.Thickness = Utils.TryGetFloat(thicknessAst, out var thickness) ? thickness : 0;
                }

                if (definition.Name is "Zero-G Tube")
                {
                    propSettings.ApplyColor = Utils.TryGetInt(applyColorAst, out var applyColor) ? applyColor : 0;
                }
                
                return propSettings;
            }
                
            case Long longProp: return longProp.NewSettings(renderOrder, seed, renderTime);

            case Antimatter antimatter:
            {
                var propSettings = (AntimatterSettings) antimatter.NewSettings(renderOrder, seed, renderTime);

                propSettings.CustomDepth = Utils.TryGetInt(customDepthAst, out var customDepth) ? customDepth : 0;
                
                return propSettings;
            }
            
            default: throw new NotImplementedException();
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
                    
                    var tile = GetPropDefinition(propList);
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
                        
                        return GetPropDefinition(propList);
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

    internal static Prop GetProp(AstNode.List list, PropDex dex)
    {
        int depth;
        string name;
        Quad quad;
        
        if (list.Values[1] is not AstNode.String s)
            throw new PropParseException("Invalid prop name");
        name = s.Value;
        
        //
        if (!dex.TryGetProp(name, out var definition) || definition is null) 
            throw new PropNotFoundException(name);
        //
        
        if (!Utils.TryGetInt(list.Values[0], out depth)) 
            throw new PropParseException($"Prop \"{name}\" has invalid depth value");


        if (list.Values[3] is not AstNode.List l) 
            throw new PropParseException($"Prop \"{name}\" has invalid quad format: expected a list of quad points");

        if (!Utils.TryGetQuad(l, out quad))
            throw new PropParseException($"Prop \"{name}\" has invalid quad points");
        

        // Extras

        var extras = list.Values[4];

        if (extras is not AstNode.PropertyList pl) 
            throw new PropParseException($"Prop \"{name}\" was expected to have additional information after quad points");

        var settingsAst = Utils.TryGet<AstNode.PropertyList>(pl, "settings") 
                       ?? throw new PropParseException($"Prop \"{name}\" has no settings");

        var settings = GetPropSettings(settingsAst, definition);

        //

        return new Prop(definition, settings, quad, depth);
    }

    internal static async IAsyncEnumerable<Prop> GetPropsAsync(AstNode.List list, PropDex dex) 
    {
        var propsAst = list.Values.Cast<AstNode.List>();

        foreach (var prop in propsAst) yield return await Task.Factory.StartNew(() => GetProp(prop, dex));
    }

    public static async Task<List<Prop>> GetPropsAsync(string propsLine, PropDex dex)
    {
        var obj = LingoParser.Expression.ParseOrThrow(propsLine);

        if (obj is not AstNode.PropertyList pl) throw new InvalidFormatException($"Props section was not a property list");
    
        var propList = Utils.Get<AstNode.List>(pl, "props");
    
        var propTasks = propList.Values
            .Cast<AstNode.List>()
            .Select(p => Task.Factory.StartNew(() => GetProp(p, dex)));

        List<Prop> props = [];

        foreach (var task in propTasks) props.Add(await task);

        return props;
    }
}