using System.Diagnostics.Contracts;
using System.Numerics;
using Leditor.Serialization.Parser;
using Leditor.Data;
using Leditor.Data.Props;
using Leditor.Data.Props.Legacy;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Settings;
using Leditor.Data.Tiles;
using Leditor.Serialization.Exceptions;
using Pidgin;
using ParseException = Leditor.Serialization.Exceptions.ParseException;
using PropNotFoundException = Leditor.Data.Props.Exceptions.PropNotFoundException;

namespace Leditor.Serialization;

public static class PropImporter
{
    
    public static List<Prop_Legacy>GetProps_Legacy(
        AstNode.Base? @base, 
        IDictionary<string, InitPropBase> propDefs,
        IDictionary<string, TileDefinition> tileDefs,
        Serilog.ILogger? logger = null
    )
    {
        if (@base is null) {
            return [];
        }

        var list = (AstNode.List)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "props").Value;

        List<Prop_Legacy> props = [];

        Random rng = new();

        foreach (var prop in list.Values)
        {
            var casted = ((AstNode.List)prop).Values;

            var depth = Utils.GetInt(casted[0]);
            var name = ((AstNode.String)casted[1]).Value;

            var indexGlobalCall = (AstNode.GlobalCall)casted[2];

            var quads = ((AstNode.List)casted[3]).Values.Select(q =>
            {
                var globalCallArgs = ((AstNode.GlobalCall)q).Arguments;

                return new Vector2(Utils.GetInt(globalCallArgs[0]) * 1.25f, Utils.GetInt(globalCallArgs[1]) * 1.25f);
            }).ToArray();

            var type = InitPropType_Legacy.Tile;

            (int category, int index) position;
            TileDefinition? tileDefinition = null;
            InitPropBase? init = null;
            
            if (!tileDefs.TryGetValue(name, out tileDefinition)) {

                if (propDefs.TryGetValue(name, out init))
                {
                    type = init.Type;
                }
            }

            position = (-1, -1);

            var extraPropList = (AstNode.PropertyList)casted[4];

            var settingsPropList = (AstNode.PropertyList)extraPropList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "settings").Value;
            AstNode.List? ropePoints = (AstNode.List?) extraPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "points").Value;

            AstNode.Number? variation = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "variation").Value;
            AstNode.Base? release = (AstNode.Base?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "release").Value;
            AstNode.Base? customDepth = (AstNode.Base?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "customdepth").Value;
            AstNode.Number? applyColor = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "applycolor").Value;
            AstNode.Number? thickness = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "thickness").Value;

            int baseToNumber(AstNode.Base? @base) {
                if (@base is null) return 0;

                switch (@base) {
                    case AstNode.Number number: return number.Value.IntValue;
                    
                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate) {
                            if (op.Expression is AstNode.Number containedNumber) {
                                return containedNumber.Value.IntValue * -1; 
                            }
                            else throw new Exception($"Prop \"{name}\" had a value that was expected to be a number");
                        } else throw new Exception($"Prop \"{name}\" had an invalid operator");

                    default:
                        throw new Exception($"Prop \"{name}\" had a value that was expected to be a number");
                }
            }

            int getIntProperty(AstNode.Number? property, string propertyName = "") => property is null
                    ? throw new Exception($"{name}: #{propertyName}")
                    : Utils.GetInt(property);

            var deNullVariation = variation is null ? 0 : getIntProperty(variation, "variation") - 1;

            if (deNullVariation < 0) deNullVariation = 0;

            BasicPropSettings settings = type switch
            {
                InitPropType_Legacy.VariedStandard => new PropVariedSettings(depth, rng.Next(1000), 0, deNullVariation),
                InitPropType_Legacy.Rope => new PropRopeSettings(depth, rng.Next(1000), 0, getIntProperty(release switch
                {
                    AstNode.Number n => n,
                    AstNode.UnaryOperator u => (AstNode.Number)u.Expression,
                    _ => null
                }, "release") switch { 1 => PropRopeRelease.Right, -1 => PropRopeRelease.Left, _ => PropRopeRelease.None }, 
                    name is "Wire" or "Zero-G Wire" ? thickness is null ? 2 : Utils.GetInt(thickness) : null, 
                    name == "Zero-G Tube" ? (applyColor is null ? 0 : getIntProperty(applyColor, "applyColor")) : null),
                InitPropType_Legacy.VariedDecal => new PropVariedDecalSettings(depth, rng.Next(1000), 0, deNullVariation, baseToNumber(customDepth)),
                InitPropType_Legacy.VariedSoft => new PropVariedSoftSettings(depth, rng.Next(1000), 0, deNullVariation, baseToNumber(customDepth),
                    (init as InitVariedSoftProp)!.Colorize == 1 ? (applyColor is null ? 0 : getIntProperty(applyColor, "applyColor")) : null),
                InitPropType_Legacy.SimpleDecal => new PropSimpleDecalSettings(depth, rng.Next(1000), 0, baseToNumber(customDepth)),
                InitPropType_Legacy.Soft => new PropSoftSettings(depth, rng.Next(1000), 0, baseToNumber(customDepth)),
                InitPropType_Legacy.SoftEffect => new PropSoftEffectSettings(depth, rng.Next(1000), 0, baseToNumber(customDepth)),
                InitPropType_Legacy.Antimatter => new PropAntimatterSettings(depth, rng.Next(1000), 0, baseToNumber(customDepth)),
                _ => new BasicPropSettings(seed: rng.Next(1000))
            };

            var parsedProp = new Prop_Legacy(depth, name,
                new(quads[0], quads[1], quads[2], quads[3]))
            {
                Extras = new PropExtras_Legacy(
                    settings: settings, 
                    ropePoints: (ropePoints?.Values.Select(p =>
                    {
                        var args = ((AstNode.GlobalCall)p).Arguments;
                        return new Vector2(Utils.GetInt(args[0]), Utils.GetInt(args[1]));
                    }).ToArray() ?? [])
                ),
                Type = type,
                Tile = tileDefinition,
                Position = position
            };

            props.Add(parsedProp);
        }
        return props;
    }

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

    public static async Task<PropDefinition[]> ParseInitAsync_NoCategories(string initPath, Serilog.ILogger? logger = null)
    {
        var text = await File.ReadAllTextAsync(initPath);
        
        if (string.IsNullOrEmpty(text)) return [];

        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<Task<PropDefinition>> definitionTasks = [];

        definitionTasks.Capacity = lines.Length;

        for (var l = 0; l < lines.Length; l++)
        {
            var line = lines[l];
            
            if (string.IsNullOrEmpty(line) || line.StartsWith('-')) continue;

            var l1 = l;

            definitionTasks.Add(Task.Factory.StartNew(() =>
            {
                AstNode.Base propObject;

                try
                {
                    propObject = LingoParser.Expression.ParseOrThrow(line);
                }
                catch (Exception e)
                {
                    logger?.Error(e, "[ParseInitAsync_NoCategories] Failed to parse tile definition from string at line {l1}: {NewLine}{Exception}", l1 + 1);
                    throw new ParseException($"Failed to parse tile definition from string at line {l1 + 1}", e);
                }

                try
                {
                    if (propObject is not AstNode.PropertyList propList)
                    {
                        logger?.Error("[ParseInitAsync_NoCategories] Prop definition is not a property list at line {l1}", l1 + 1);
                        throw new ParseException($"Prop definition is not a property list at line {l1 + 1}");   
                    }
                    
                    return GetPropDefinition(propList);
                }
                catch (ParseException e)
                {
                    logger?.Error(e, "[ParseInitAsync_NoCategories] Failed to get tile definition from parsed string at line {l1}: {NewLine}{Exception}", l1 + 1);
                    throw new ParseException($"Failed to get tile definition from parsed string at line {l1 + 1}", e);
                }
            }));
        }

        await Task.WhenAll(definitionTasks);

        return definitionTasks
            .Where(t => {
                if (t.IsFaulted)
                {
                    logger?.Error(
                        t.Exception, 
                        "[ParseInitAsync_NoCategories] Failed to parse prop"
                    );
                }

                return t.IsCompletedSuccessfully;
            })
            .Select(t => t.Result)
            .ToArray();
    }

    public static InitPropBase GetLegacyInitProp(AstNode.Base @base)
    {
        var propertList = ((AstNode.PropertyList)@base).Values;

        string name = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
        string typeStr = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;

        // retrieve all possible properties

        AstNode.String? colorTreatment      = (AstNode.String?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "colortreatment").Value;
        AstNode.Number? bevel               = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "bevel").Value;
        AstNode.Number? depth               = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "depth").Value;
        AstNode.GlobalCall? sz              = (AstNode.GlobalCall?) propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "sz").Value;
        AstNode.GlobalCall? pxlSize         = (AstNode.GlobalCall?) propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "pxlsize").Value;
        AstNode.List? repeatL               = (AstNode.List?)       propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value;
        AstNode.Number? vars                = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "vars").Value;
        AstNode.Number? round               = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "round").Value;
        AstNode.Number? selfShade           = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "selfshade").Value;
        AstNode.Number? highLightBorder     = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "highlightborder").Value;
        AstNode.Number? depthAffectHilites  = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "depthaffecthilites").Value;
        AstNode.Number? shadowBorder        = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "shadowborder").Value;
        AstNode.Number? smoothShading       = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "smoothshading").Value;
        AstNode.Number? random              = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "random").Value;
        AstNode.Number? colorize            = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "colorize").Value;
        AstNode.Number? contourExp          = (AstNode.Number?)     propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "contourexp").Value;

        InitPropColorTreatment getColorTreatment() => colorTreatment is null
                    ? InitPropColorTreatment.Standard
                    : colorTreatment.Value switch { "standard" => InitPropColorTreatment.Standard, "bevel" => InitPropColorTreatment.Bevel, _ => throw new Exception() };

        int getIntProperty(AstNode.Number? property) => property is null
                    ? throw new Exception()
                    : Utils.GetInt(property);

        float getFloatProperty(AstNode.Number? property) => property is null
                    ? throw new Exception()
                    : Utils.GetInt(property);

        //

        var type = typeStr switch
        {
            "standard"          => InitPropType_Legacy.Standard,
            "variedStandard"    => InitPropType_Legacy.VariedStandard,
            "soft"              => InitPropType_Legacy.Soft,
            "variedSoft"        => InitPropType_Legacy.VariedSoft,
            "simpleDecal"       => InitPropType_Legacy.SimpleDecal,
            "variedDecal"       => InitPropType_Legacy.VariedDecal,
            "antimatter"        => InitPropType_Legacy.Antimatter,
            "softEffect"        => InitPropType_Legacy.SoftEffect,
            "long"              => InitPropType_Legacy.Long,
            "coloredSoft"       => InitPropType_Legacy.ColoredSoft,

            _ => throw new InvalidPropTypeException(typeStr)
        };

        InitPropBase init = type switch
        {
            InitPropType_Legacy.Standard => new InitStandardProp(
                name, 
                type,
                depth is null ? 0 :  getIntProperty(depth),
                getColorTreatment(),
                getColorTreatment() is InitPropColorTreatment.Bevel 
                    ? getIntProperty(bevel) 
                    : bevel is null 
                        ? 0 
                        : Utils.GetInt(bevel),
                sz is null
                    ? throw new Exception()
                    : (Utils.GetInt((AstNode.Number)sz.Arguments[0]), Utils.GetInt((AstNode.Number)sz.Arguments[1])),
                repeatL is null
                    ? throw new Exception()
                    : repeatL.Values.Select(n => Utils.GetInt((AstNode.Number)n)).ToArray()
                ),

            InitPropType_Legacy.VariedStandard => new InitVariedStandardProp(
                name, 
                type,
                depth is null ? 0 :  getIntProperty(depth),
                getColorTreatment(),
                getColorTreatment() is InitPropColorTreatment.Bevel 
                    ? getIntProperty(bevel) 
                    : bevel is null 
                        ? 0 
                        : Utils.GetInt(bevel),
                sz is null
                    ? throw new Exception()
                    : (Utils.GetInt((AstNode.Number)sz.Arguments[0]), Utils.GetInt((AstNode.Number)sz.Arguments[1])),
                repeatL is null
                    ? throw new Exception()
                    : repeatL.Values.Select(n => Utils.GetInt((AstNode.Number)n)).ToArray(),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random)
                ),
            
            InitPropType_Legacy.Soft => new InitSoftProp(
                name,
                type,
                depth is null ? 0 :  getIntProperty(depth),
                getIntProperty(round),
                getFloatProperty(contourExp),
                getIntProperty(selfShade),
                getFloatProperty(highLightBorder),
                getFloatProperty(depthAffectHilites),
                getFloatProperty(shadowBorder),
                getIntProperty(smoothShading)
                ),
            
            InitPropType_Legacy.SoftEffect => new InitSoftEffectProp(
                name,
                type,
                depth is null ? 0 :  getIntProperty(depth)
            ),

            InitPropType_Legacy.VariedSoft => new InitVariedSoftProp(
                name,
                type,
                depth is null ? 0 :  getIntProperty(depth),
                getIntProperty(round),
                getIntProperty(contourExp),
                getIntProperty(selfShade),
                getFloatProperty(highLightBorder),
                getIntProperty(depthAffectHilites),
                getIntProperty(shadowBorder),
                getIntProperty(smoothShading),
                pxlSize is null
                    ? throw new Exception()
                    : (Utils.GetInt((AstNode.Number)pxlSize.Arguments[0]), Utils.GetInt((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random),
                getIntProperty(colorize)),

            InitPropType_Legacy.SimpleDecal => new InitSimpleDecalProp(name, type, depth is null ? 0 :  getIntProperty(depth)),
            InitPropType_Legacy.VariedDecal => new InitVariedDecalProp(
                name, 
                type, 
                depth is null ? 0 :  getIntProperty(depth),
                pxlSize is null
                    ? throw new Exception()
                    : (Utils.GetInt((AstNode.Number)pxlSize.Arguments[0]), Utils.GetInt((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random)
            ),
            InitPropType_Legacy.Antimatter => new InitAntimatterProp(name, type, depth is null ? 0 :  getIntProperty(depth), getFloatProperty(contourExp)),
            InitPropType_Legacy.Long => new InitLongProp(name, type, depth is null ? 0 : getIntProperty(depth)),
            InitPropType_Legacy.ColoredSoft => new InitColoredSoftProp(name, type, 
                depth is null ? 0 : getIntProperty(depth), 
                pxlSize is null
                ? throw new Exception()
                : (Utils.GetInt((AstNode.Number)pxlSize.Arguments[0]), 
                    Utils.GetInt((AstNode.Number)pxlSize.Arguments[1])), 
                round is null ? 0 : getIntProperty(round), 
                getFloatProperty(contourExp),
                getIntProperty(selfShade),
                getFloatProperty(highLightBorder),
                getFloatProperty(depthAffectHilites),
                getFloatProperty(shadowBorder),
                getIntProperty(smoothShading),
                getIntProperty(colorize)),
            _ => throw new InvalidPropTypeException(typeStr)
        };

        return init;
    }

    public static async Task<InitPropBase[]> ParseLegacyInitAsync_NoCategories(string initPath, Serilog.ILogger? logger = null)
    {
        var text = await File.ReadAllTextAsync(initPath);
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<InitPropBase> props = [];

        var counter = 0;

        foreach (var line in lines)
        {
            counter++;

            if (string.IsNullOrEmpty(line) || line.StartsWith('-')) continue;

            var obj = LingoParser.Expression.ParseOrThrow(line);

            InitPropBase prop;

            try {
                prop = GetLegacyInitProp(obj);
            } catch (Exception e) {
                throw new Exception($"Failed to load prop init at line {counter}.", e);
            }

            props.Add(prop);
        }

        return props.ToArray();
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

    internal static Prop GetProp(AstNode.List list, IDictionary<string, PropDefinition> props)
    {
        int depth;
        string name;
        Quad quad;
        
        if (list.Values[1] is not AstNode.String s)
            throw new PropParseException("Invalid prop name");
        name = s.Value;
        
        //
        if (!props.TryGetValue(name, out var definition) || definition is null) 
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

    internal static async Task<Prop[]> GetPropsAsync(AstNode.Base node, IDictionary<string, PropDefinition> props) 
    {
        if (node is null) {
            return [];
        }

        var list = (AstNode.List)((AstNode.PropertyList)node).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "props").Value;

        var propsAst = list.Values.Cast<AstNode.List>();

        List<Task<Prop>> parsed = [];

        foreach (var prop in propsAst) parsed.Add(Task.Factory.StartNew(() => GetProp(prop, props)));

        await Task.WhenAll(parsed);

        return parsed.Select(t => t.Result).ToArray();
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