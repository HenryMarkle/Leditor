using Pidgin;
using System.Numerics;

using Leditor.Data.Effects;
using Leditor.Data.Tiles;
using Leditor.Data.Geometry;
using Leditor.Data.Props.Legacy;
using Leditor.Serialization.Parser;

namespace Leditor.Serialization;


public static class Importers
{

    private static GeoFeature GetGeoFeaturesFromList(IEnumerable<int> seq)
    {
        var features = GeoFeature.None;

        foreach (var f in seq) features |= Geo.FeatureID(f);

        return features;
    }

    public static string StringifyBase(AstNode.Base b)
    {
        System.Text.StringBuilder sb = new();

        switch (b)
        {
            case AstNode.Number number:
                sb.Append(number.Value);
                break;

            case AstNode.String str:
                sb.Append($"\"{str.Value}\"");
                break;

            case AstNode.GlobalCall call:
                sb.Append($"{call.Name}(");

                sb.AppendJoin(", ", call.Arguments.Select(StringifyBase));

                sb.Append(')');
                break;

            case AstNode.List list:
                sb.Append('[');

                sb.AppendJoin(", ", list.Values.Select(StringifyBase));

                sb.Append(']');
                break;

            case AstNode.Symbol symbol:
                sb.Append($"#{symbol.Value}");
                break;


            case AstNode.PropertyList dict:
                sb.Append('[');

                sb.AppendJoin(", ", dict.Values.Select(p => $"{StringifyBase(p.Key)}: {StringifyBase(p.Value)}"));

                sb.Append(']');
                break;
        }

        return sb.ToString();
    }

    static float NumberToFloat(AstNode.Base number)
    {
        switch (number)
        {
            case AstNode.UnaryOperator u:
                if (u.Type == AstNode.UnaryOperatorType.Negate)
                    return (float)((AstNode.Number)u.Expression).Value.DecimalValue * -1;

                throw new ArgumentException("argument is not a number", nameof(number));

            case AstNode.Number n: return (float)n.Value.DecimalValue;

            default:
                throw new ArgumentException("argument is not a number", nameof(number));
        }
    }

    static int NumberToInteger(AstNode.Base number)
    {
        switch (number)
        {
            case AstNode.UnaryOperator u:
                if (u.Type == AstNode.UnaryOperatorType.Negate)
                    return ((AstNode.Number)u.Expression).Value.IntValue * -1;

                throw new ArgumentException("argument is not a number", nameof(number));

            case AstNode.Number n: return n.Value.IntValue;

            default:
                throw new ArgumentException("argument is not a number", nameof(number));
        }
    }

    public static Data.Materials.MaterialDefinition GetInitMaterial(AstNode.Base @base)
    {
#nullable enable
        var propertyList = (AstNode.PropertyList)@base;

        var nameBase = (AstNode.Base?)propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "nm").Value;
        var colorBase = (AstNode.Base?)propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "color").Value;

        if (nameBase is null) throw new MaterialParseException("Missing \"nm\" property", StringifyBase(@base));
        if (colorBase is null) throw new MaterialParseException("Missing \"color\" property", StringifyBase(@base));

        var colorGlobalCall = ((AstNode.GlobalCall)colorBase);

        if (colorGlobalCall.Name != "color") throw new MaterialParseException("Invalid \"color\" value", StringifyBase(@base));

        var color = new Color(
            ((AstNode.Number)colorGlobalCall.Arguments[0]).Value.IntValue,
            ((AstNode.Number)colorGlobalCall.Arguments[1]).Value.IntValue,
            ((AstNode.Number)colorGlobalCall.Arguments[2]).Value.IntValue,
            255
            );

        return new(((AstNode.String)nameBase).Value, color, Data.Materials.MaterialRenderType.CustomUnified);
    }

    public static (string[] categories, Data.Materials.MaterialDefinition[][]) GetMaterialInit(string text)
    {
#nullable enable
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
                    materials[^1] = [.. materials[^1], material];
                }
                catch
                {
                    GLOBALS.Logger?.Error($"Failed to parse custom material: \"{(@base is null ? "<NULL>" : StringifyBase(@base))}\"");
                }
            }
        }

        return ([.. categories], [.. materials]);
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
            _ => throw new Exception($"Invalid light angle value: {StringifyBase(angleBase)}")
        };

        var flatness = flatnessBase switch
        {
            AstNode.Number n => n.Value.IntValue,
            AstNode.UnaryOperator u => ((AstNode.Number)u.Expression).Value.IntValue * -1,
            _ => throw new Exception($"Invalid flatness value: {StringifyBase(flatnessBase)}")
        };

        return (angle, flatness);
    }

    public static InitPropBase GetInitProp(AstNode.Base @base)
    {
        var propertList = ((AstNode.PropertyList)@base).Values;

        string name = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
        string typeStr = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;

        // retrieve all possible properties

        AstNode.String? colorTreatment = (AstNode.String?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "colortreatment").Value;
        AstNode.Number? bevel = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "bevel").Value;
        AstNode.Number? depth = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "depth").Value;
        AstNode.GlobalCall? sz = (AstNode.GlobalCall?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "sz").Value;
        AstNode.GlobalCall? pxlSize = (AstNode.GlobalCall?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "pxlsize").Value;
        AstNode.List? repeatL = (AstNode.List?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value;
        AstNode.Number? vars = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "vars").Value;
        AstNode.Number? round = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "round").Value;
        AstNode.Number? selfShade = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "selfshade").Value;
        AstNode.Number? highLightBorder = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "highlightborder").Value;
        AstNode.Number? depthAffectHilites = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "depthaffecthilites").Value;
        AstNode.Number? shadowBorder = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "shadowborder").Value;
        AstNode.Number? smoothShading = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "smoothshading").Value;
        AstNode.Number? random = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "random").Value;
        AstNode.Number? colorize = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "colorize").Value;
        AstNode.Number? contourExp = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "contourexp").Value;

        // Custom Ropes

        AstNode.Number? rope_segmentLength = null;
        AstNode.Number? rope_collisionDepth = null;
        AstNode.Number? rope_segmentRadius = null;
        AstNode.Number? rope_segRad = null;
        AstNode.Number? rope_grav = null;
        AstNode.Number? rope_friction = null;
        AstNode.Number? rope_airFric = null;
        AstNode.Number? rope_stiff = null;
        AstNode.Number? rope_previewEvery = null;
        AstNode.Number? rope_edgeDirection = null;
        AstNode.Number? rope_rigid = null;
        AstNode.Number? rope_selfPush = null;
        AstNode.Number? rope_sourcePush = null;

        if (typeStr == "customrope")
        {
            rope_segmentLength = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "segmentlength").Value;
            rope_collisionDepth = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "collisiondepth").Value;
            rope_segmentRadius = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "segmentradius").Value;
            rope_segRad = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "segrad").Value;
            rope_grav = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "grav").Value;
            rope_friction = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "friction").Value;
            rope_airFric = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "airfric").Value;
            rope_stiff = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "stiff").Value;
            rope_previewEvery = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "previewevery").Value;
            rope_edgeDirection = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "edgedirection").Value;
            rope_rigid = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "rigid").Value;
            rope_selfPush = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "selfpush").Value;
            rope_sourcePush = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "sourcepush").Value;
        }

        if (typeStr == "customLong")
        {
            rope_segmentLength = (AstNode.Number?)propertList.FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == "segmentlength").Value;
        }

        InitPropColorTreatment getColorTreatment() => colorTreatment is null
                    ? InitPropColorTreatment.Standard
                    : colorTreatment.Value switch { "standard" => InitPropColorTreatment.Standard, "bevel" => InitPropColorTreatment.Bevel, _ => throw new InvalidInitPropertyValueException("", StringifyBase(@base), "colorTreatment", colorTreatment.Value) };

        int getIntProperty(AstNode.Number? property) => property is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(property))
                    : NumberToInteger(property);

        float getFloatProperty(AstNode.Number? property) => property is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(property))
                    : NumberToFloat(property);

        //

        var type = typeStr switch
        {
            "standard" => InitPropType_Legacy.Standard,
            "variedStandard" => InitPropType_Legacy.VariedStandard,
            "soft" => InitPropType_Legacy.Soft,
            "variedSoft" => InitPropType_Legacy.VariedSoft,
            "simpleDecal" => InitPropType_Legacy.SimpleDecal,
            "variedDecal" => InitPropType_Legacy.VariedDecal,
            "antimatter" => InitPropType_Legacy.Antimatter,
            "softEffect" => InitPropType_Legacy.SoftEffect,
            "customlong" => InitPropType_Legacy.CustomLong,
            "customrope" => InitPropType_Legacy.CustomRope,
            "coloredSoft" => InitPropType_Legacy.ColoredSoft,

            _ => throw new InvalidPropTypeException(typeStr, StringifyBase(@base))
        };

        InitPropBase init = type switch
        {
            InitPropType_Legacy.Standard => new InitStandardProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                getColorTreatment(),
                getColorTreatment() is InitPropColorTreatment.Bevel
                    ? getIntProperty(bevel)
                    : bevel is null
                        ? 0
                        : NumberToInteger(bevel),
                sz is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(sz))
                    : (NumberToInteger((AstNode.Number)sz.Arguments[0]), NumberToInteger((AstNode.Number)sz.Arguments[1])),
                repeatL is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(repeatL))
                    : repeatL.Values.Select(n => NumberToInteger((AstNode.Number)n)).ToArray()
                ),

            InitPropType_Legacy.VariedStandard => new InitVariedStandardProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                getColorTreatment(),
                getColorTreatment() is InitPropColorTreatment.Bevel
                    ? getIntProperty(bevel)
                    : bevel is null
                        ? 0
                        : NumberToInteger(bevel),
                sz is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(sz))
                    : (NumberToInteger((AstNode.Number)sz.Arguments[0]), NumberToInteger((AstNode.Number)sz.Arguments[1])),
                repeatL is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(repeatL))
                    : repeatL.Values.Select(n => NumberToInteger((AstNode.Number)n)).ToArray(),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random)
                ),

            InitPropType_Legacy.Soft => new InitSoftProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
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
                depth is null ? 0 : getIntProperty(depth)
            ),

            InitPropType_Legacy.VariedSoft => new InitVariedSoftProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                getIntProperty(round),
                getIntProperty(contourExp),
                getIntProperty(selfShade),
                getFloatProperty(highLightBorder),
                getIntProperty(depthAffectHilites),
                getIntProperty(shadowBorder),
                getIntProperty(smoothShading),
                pxlSize is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                    : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]), NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random),
                getIntProperty(colorize)),

            InitPropType_Legacy.SimpleDecal => new InitSimpleDecalProp(name, type, depth is null ? 0 : getIntProperty(depth)),
            InitPropType_Legacy.VariedDecal => new InitVariedDecalProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                pxlSize is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                    : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]), NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random)
            ),
            InitPropType_Legacy.Antimatter => new InitAntimatterProp(name, type, depth is null ? 0 : getIntProperty(depth), getFloatProperty(contourExp)),
            InitPropType_Legacy.CustomLong => new InitCustomLongProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                getColorTreatment(),
                bevel is null ? 0 : getIntProperty(bevel),
                random is null ? 0 : getIntProperty(random),
                repeatL is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(repeatL))
                    : repeatL.Values.Select(n => NumberToInteger((AstNode.Number)n)).ToArray(),
                pxlSize is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                    : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]), NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(rope_segmentLength),
                vars is null ? 1 : getIntProperty(vars)

            ),
            InitPropType_Legacy.CustomRope => new InitCustomRopeProp(
                name,
                type,
                depth is null ? 0 : getIntProperty(depth),
                getIntProperty(rope_segmentLength),
                getIntProperty(rope_collisionDepth),
                getFloatProperty(rope_segmentRadius),
                getFloatProperty(rope_grav),
                getFloatProperty(rope_friction),
                getFloatProperty(rope_airFric),
                getIntProperty(rope_stiff) > 0,
                Color.Red,
                getIntProperty(rope_previewEvery),
                getFloatProperty(rope_edgeDirection),
                getFloatProperty(rope_rigid),
                getFloatProperty(rope_selfPush),
                getFloatProperty(rope_sourcePush),
                getColorTreatment(),
                bevel is null ? 0 : getIntProperty(bevel)
            ),
            InitPropType_Legacy.ColoredSoft => new InitColoredSoftProp(name, type,
                depth is null ? 0 : getIntProperty(depth),
                pxlSize is null
                ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]),
                    NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                round is null ? 0 : getIntProperty(round),
                getFloatProperty(contourExp),
                getIntProperty(selfShade),
                getFloatProperty(highLightBorder),
                getFloatProperty(depthAffectHilites),
                getFloatProperty(shadowBorder),
                getIntProperty(smoothShading),
                getIntProperty(colorize)),
            _ => throw new InvalidPropTypeException(typeStr, StringifyBase(@base))
        };

        return init;
    }



    public static ((string category, Color color)[] categories, InitPropBase[][] init) GetPropsInit(string text)
    {
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<(string, Color)> categories = [];
        List<InitPropBase[]> props = [];

        var counter = 0;

        foreach (var line in lines)
        {
            counter++;

            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            // category
            if (line.StartsWith('-'))
            {
                props.Add([]);

                AstNode.List headerList;

                try
                {
                    headerList = (AstNode.List)LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));
                }
                catch
                {
                    throw new PropInitParseException("failed to parse prop init line", line);
                }

                string name = ((AstNode.String)headerList.Values[0]).Value;

                var colorArgs = ((AstNode.GlobalCall)headerList.Values[1]).Arguments;
                var headerColor = (((AstNode.Number)colorArgs[0]).Value.IntValue, ((AstNode.Number)colorArgs[1]).Value.IntValue, ((AstNode.Number)colorArgs[2]).Value.IntValue);

                categories.Add((name, new(headerColor.Item1, headerColor.Item2, headerColor.Item3, 255)));
            }
            // prop list
            else
            {
                try
                {
                    var obj = LingoParser.Expression.ParseOrThrow(line);

                    InitPropBase prop;
                    prop = GetInitProp(obj);
                    props[^1] = [.. props[^1], prop];
                }
                catch (Exception e)
                {
                    throw new InitParseException($"Failed to load prop init at line {counter}.", "", e);
                }
            }
        }

        return (categories.ToArray(), props.ToArray());
    }

    /// <summary>
    /// Retrieves the props from an object parsed from line (9) of the project file
    /// </summary>
    /// <param name="base">parsed <see cref="AstNode.Base"/> object</param>
    /// <returns>a list of (type, position, prop) tuples, where 'position' points to the definition index in the appropriate definitions array depending on the type (prop/tile)</returns>
    /// <exception cref="PropNotFoundException">prop not not found in neither the prop definitions nor the tile definitions</exception>
    /// <exception cref="MissingInitPropertyException">retrieved prop is missing a setting</exception>
    public static List<Prop_Legacy> GetProps(AstNode.Base? @base)
    {
        if (@base is null || @base is not AstNode.PropertyList pl)
        {
            return [];
        }

        var list = (AstNode.List)(pl).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "props").Value;

        List<Prop_Legacy> props = [];

        Random rng = new();

        foreach (var prop in list.Values)
        {
            var casted = ((AstNode.List)prop).Values;

            var depth = NumberToInteger(casted[0]);
            var name = ((AstNode.String)casted[1]).Value;

            var indexGlobalCall = (AstNode.GlobalCall)casted[2];

            var quads = ((AstNode.List)casted[3]).Values.Select(q =>
            {
                var globalCallArgs = ((AstNode.GlobalCall)q).Arguments;

                return new Vector2(NumberToInteger(globalCallArgs[0]) * 1.25f, NumberToInteger(globalCallArgs[1]) * 1.25f);
            }).ToArray();

            var type = InitPropType_Legacy.Tile;

            (int category, int index) position;
            TileDefinition? tileDefinition = null;

            // check if it's a prop

            for (var c = 0; c < GLOBALS.Props.Length; c++)
            {
                for (var p = 0; p < GLOBALS.Props[c].Length; p++)
                {
                    var initProp = GLOBALS.Props[c][p];

                    if (initProp.Name == name)
                    {
                        type = initProp.Type;
                        position = (c, p);

                        goto end_check;
                    }
                }
            }

            // check if it's a long prop

            for (var c = 0; c < GLOBALS.LongProps.Length; c++)
            {
                var initProp = GLOBALS.LongProps[c];

                if (initProp.Name == name)
                {
                    type = initProp.Type;
                    position = (-1, c);

                    goto end_check;
                }
            }

            // check if it's a rope prop

            for (var c = 0; c < GLOBALS.Ropes.Length; c++)
            {
                var initProp = GLOBALS.Ropes[c];

                if (initProp.Name == name)
                {
                    type = InitPropType_Legacy.Rope;
                    position = (-1, c);

                    goto end_check;
                }
            }

            // if not found, then check if it's a tile

            var found = GLOBALS.TileDex?.TryGetTile(name, out tileDefinition) ?? false;

            if (!found) throw new PropNotFoundException(name);

            position = (-1, -1);

        end_check:

            var extraPropList = (AstNode.PropertyList)casted[4];

            var settingsPropList = (AstNode.PropertyList)extraPropList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "settings").Value;
            AstNode.List? ropePoints = (AstNode.List?)extraPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "points").Value;

            AstNode.Number? variation = (AstNode.Number?)settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "variation").Value;
            AstNode.Base? release = (AstNode.Base?)settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "release").Value;
            AstNode.Base? customDepth = (AstNode.Base?)settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "customdepth").Value;
            AstNode.Number? applyColor = (AstNode.Number?)settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "applycolor").Value;
            AstNode.Number? thickness = (AstNode.Number?)settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "thickness").Value;

            int baseToNumber(AstNode.Base? @base)
            {
                if (@base is null) return 0;

                switch (@base)
                {
                    case AstNode.Number number: return number.Value.IntValue;

                    case AstNode.UnaryOperator op:
                        if (op.Type == AstNode.UnaryOperatorType.Negate)
                        {
                            if (op.Expression is AstNode.Number containedNumber)
                            {
                                return containedNumber.Value.IntValue * -1;
                            }
                            else throw new Exception($"Prop \"{name}\" had a value that was expected to be a number");
                        }
                        else throw new Exception($"Prop \"{name}\" had an invalid operator");

                    default:
                        throw new Exception($"Prop \"{name}\" had a value that was expected to be a number");
                }
            }

            int getIntProperty(AstNode.Number? property, string propertyName = "") => property is null
                    ? throw new MissingInitPropertyException($"{name}: #{propertyName}", StringifyBase(@base), nameof(property))
                    : NumberToInteger(property);

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
                }, "release") switch
                { 1 => PropRopeRelease.Right, -1 => PropRopeRelease.Left, _ => PropRopeRelease.None },
                    name is "Wire" or "Zero-G Wire" ? thickness is null ? 2 : NumberToFloat(thickness) : null,
                    name == "Zero-G Tube" ? (applyColor is null ? 0 : getIntProperty(applyColor, "applyColor")) : null),
                InitPropType_Legacy.VariedDecal => new PropVariedDecalSettings(depth, rng.Next(1000), 0, deNullVariation, baseToNumber(customDepth)),
                InitPropType_Legacy.VariedSoft => new PropVariedSoftSettings(depth, rng.Next(1000), 0, deNullVariation, baseToNumber(customDepth),
                    ((InitVariedSoftProp)GLOBALS.Props[position.category][position.index]).Colorize == 1 ? (applyColor is null ? 0 : getIntProperty(applyColor, "applyColor")) : null),
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
                        return new Vector2(NumberToInteger(args[0]), NumberToInteger(args[1]));
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

    public static List<Data.RenderCamera> GetCameras(AstNode.Base @base)
    {
        var props = (AstNode.PropertyList)@base;

        var cameras = ((AstNode.List)props.Values
            .Single(p => ((AstNode.Symbol)p.Key).Value == "cameras")
            .Value)
            .Values
            .Cast<AstNode.GlobalCall>()
            .Select(c =>
            {
                var x = NumberToFloat(c.Arguments[0]);
                var y = NumberToFloat(c.Arguments[1]);

                return (x, y);
            })
            .ToArray();

        var quads = ((AstNode.List?)props.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "quads").Value)
            ?.Values
            ?.Cast<AstNode.List>()
            ?.Select(l =>
            {
                var currentQuads = l.Values.Cast<AstNode.List>().Select(q =>
                {
                    var angle = NumberToInteger(q.Values[0]);
                    var radius = NumberToFloat(q.Values[1]);

                    return (angle, radius);
                }).ToArray();

                return (currentQuads[0], currentQuads[1], currentQuads[2], currentQuads[3]);
            })
            ?.ToArray() ?? [((0, 0), (0, 0), (0, 0), (0, 0))];

        var result = new Data.RenderCamera[cameras.Length];

        for (int c = 0; c < cameras.Length; c++)
        {
            var currentQuads = quads[c];

            // var topLeft = new Vector2((float) (currentQuads.Item1.radius * Math.Cos(currentQuads.Item1.angle)), (float) (currentQuads.Item1.radius * Math.Sin(currentQuads.Item1.angle)));
            // var topRight = new Vector2((float) (currentQuads.Item2.radius * Math.Cos(currentQuads.Item2.angle)), (float) (currentQuads.Item2.radius * Math.Sin(currentQuads.Item2.angle)));
            // var bottomRight = new Vector2((float) (currentQuads.Item3.radius * Math.Cos(currentQuads.Item3.angle)), (float) (currentQuads.Item3.radius * Math.Sin(currentQuads.Item3.angle)));
            // var bottomLeft = new Vector2((float) (currentQuads.Item4.radius * Math.Cos(currentQuads.Item4.angle)), (float) (currentQuads.Item4.radius * Math.Sin(currentQuads.Item4.angle)));

            var topLeft = currentQuads.Item1;
            var topRight = currentQuads.Item2;
            var bottomRight = currentQuads.Item3;
            var bottomLeft = currentQuads.Item4;

            result[c] = new()
            {
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

    public static Effect[] GetEffects(AstNode.Base @base, int width, int height)
    {
        var matrixProp = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "effects").Value;

        var effectList = ((AstNode.List)matrixProp).Values.Select(e =>
        {
            var props = ((AstNode.PropertyList)e).Values;

            var name = ((AstNode.String)props.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;

            var mtxWidth = ((AstNode.List)props.Single(p => ((AstNode.Symbol)p.Key).Value == "mtrx").Value).Values.Cast<AstNode.List>().ToArray();

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

                        var b => throw new Exception($"Invalid effect option choice: {StringifyBase(b)}")
                    };

                    return new EffectOptions(optionName, options, choice);
                }).ToArray() ?? throw new NullReferenceException($"No options found for effect \"{name}\"");

            var matrix = new double[height, width];

            for (var x = 0; x < mtxWidth.Length; x++)
            {
                var mtxHeight = mtxWidth[x].Values.Cast<AstNode.Number>().Select(n => n.Value.DecimalValue).ToArray();

                for (int y = 0; y < mtxHeight.Length; y++)
                {
                    matrix[y, x] = mtxHeight[y];
                }
            }


            return new Effect() { Name = name, Options = options, Matrix = matrix };
        }).ToArray();


        return effectList;
    }

    public static BufferTiles GetBufferTiles(AstNode.Base @base)
    {
        var pair = ((AstNode.PropertyList)@base).Values.Single(s => ((AstNode.Symbol)s.Key).Value == "extratiles");
        int[] list = ((AstNode.List)pair.Value).Values.Cast<AstNode.Number>().Select(e => e.Value.IntValue).ToArray();

        return new BufferTiles
        {
            Left = list[0],
            Top = list[1],
            Right = list[2],
            Bottom = list[3],
        };
    }

    public static Geo[,,] GetGeoMatrix(AstNode.Base @base, out int height, out int width)
    {
        if (@base is not AstNode.List) throw new ArgumentException("object is not a list", nameof(@base));

        var columns = ((AstNode.List)@base).Values;

        height = 0;
        width = columns.Length;

        for (int x = 0; x < columns.Length; x++)
        {
            if (columns[x] is not AstNode.List) throw new Exception($"column ({x}) is not a list");

            for (int y = 0; y < ((AstNode.List)columns[x]).Values.Length; y++)
            {
                height = ((AstNode.List)columns[x]).Values.Length;


                if (((AstNode.List)((AstNode.List)columns[x]).Values[y]) is not AstNode.List)
                    throw new Exception($"element ({y}) at row ({x}) is not a list");
            }
        }

        Geo[,,] matrix = new Geo[height, width, 3];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
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

                matrix[y, x, 0] = new()
                {
                    Type = (GeoType)((AstNode.Number)((AstNode.List)layer1).Values[0]).Value.IntValue,
                    Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 1] = new()
                {
                    Type = (GeoType)((AstNode.Number)((AstNode.List)layer2).Values[0]).Value.IntValue,
                    Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 2] = new()
                {
                    Type = (GeoType)((AstNode.Number)((AstNode.List)layer3).Values[0]).Value.IntValue,
                    Features = GetGeoFeaturesFromList(((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };
            }
        }

        return matrix;
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
        var terrain = (AstNode.Number)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultterrain").Value;

        return terrain.Value.IntValue == 1;
    }

    public static (int waterLevel, bool waterInFront) GetWaterData(AstNode.Base @base)
    {
        var waterLevelBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "waterlevel").Value;
        var waterInFrontBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "waterinfront").Value;

        var waterLevel = NumberToInteger(waterLevelBase);
        var waterInFront = NumberToInteger(waterInFrontBase) != 0;

        return (waterLevel, waterInFront);
    }

    public static int GetSeed(AstNode.Base @base)
    {
        var seedBase = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tileseed").Value;

        return NumberToInteger(seedBase);
    }

    public static string GetDefaultMaterial(AstNode.Base @base)
    {
        var materialBase = (AstNode.String)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultmaterial").Value;

        return materialBase.Value;
    }

    public static (int, int) GetLevelDimensions(AstNode.Base node)
    {
        var szAst = (node as AstNode.PropertyList)!.Values.FirstOrDefault(k => string.Equals((k.Key as AstNode.Symbol)!.Value, "size", StringComparison.OrdinalIgnoreCase)).Value;
        var point = (AstNode.GlobalCall)szAst;

        return ((point.Arguments[0] as AstNode.Number)!.Value.IntValue, (point.Arguments[1] as AstNode.Number)!.Value.IntValue);
    }

    public static CustomEffectDef[] GetCustomEffectsFromInit()
    {
        List<CustomEffectDef> list = [];

        if (!File.Exists(GLOBALS.Paths.CustomEffectsInitPath)) return [];

        var lines = File
            .ReadAllText(GLOBALS.Paths.CustomEffectsInitPath)
            .ReplaceLineEndings()
            .Split(Environment.NewLine);

        string currentCategory = "";

        for (var l = 0; l < lines.Length; l++)
        {
            if (string.IsNullOrWhiteSpace(lines[l]) || lines[l].StartsWith("--")) continue;

            if (lines[l].StartsWith('-')) // Category
            {
                currentCategory = lines[l][1..];
            }
            else
            {
                try
                {
                    var parsed = LingoParser.Expression.ParseOrThrow(lines[l]);

                    if (parsed is not AstNode.PropertyList props) continue;

                    var nameObj = props.Values.FirstOrDefault(k => ((AstNode.Symbol)k.Key).Value == "nm").Value;

                    if (nameObj is not AstNode.String { Value: string } name) continue;

                    var pickColor = props.Values.Any(
                        k => k is { Key: AstNode.Symbol { Value: "pickColor" }, Value: AstNode.Number { Value.IntValue: 1 } }
                    );

                    var can3D = props.Values.Any(
                        k => k is { Key: AstNode.Symbol { Value: "can3D" }, Value: AstNode.Number { Value.IntValue: 2 } }
                    );

                    var type = props.Values.FirstOrDefault(k => k is { Key: AstNode.Symbol { Value: "tp" } }).Value;

                    if (type is not AstNode.String typeStr) throw new Exceptions.ParseException($"Missing required property #tp");

                    list.Add(new(name.Value, currentCategory, typeStr.Value)
                    {
                        PickColor = pickColor,
                        Can3D = can3D,
                    });
                }
                catch (Exception e)
                {
                    throw new Exceptions.ParseException($"Failed to parse custom effect at line {l + 1}", e);
                }
            }
        }
        
        return [..list];
    }
}