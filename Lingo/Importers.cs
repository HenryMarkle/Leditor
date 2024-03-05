using Leditor.Lingo.Drizzle;
using Pidgin;
using System.Numerics;
using Leditor.Leditor.Lingo;

namespace Leditor.Lingo;

public static class Importers {
    /// Meaningless name; this function turns a sequel of stackable IDs to an array that can be used at leditor runtime
    private static bool[] DecomposeStackables(IEnumerable<int> seq)
    {
        bool[] bools = new bool[22];

        foreach (var i in seq) bools[i] = true;

        return bools;
    }
    
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

    static float NumberToFloat(AstNode.Base number) {
        switch (number) {
            case AstNode.UnaryOperator u:
                if (u.Type == AstNode.UnaryOperatorType.Negate)
                    return (float)((AstNode.Number)u.Expression).Value.DecimalValue * -1;
                
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
                
                throw new ArgumentException("argument is not a number", nameof(number));

            case AstNode.Number n: return n.Value.IntValue;

            default:
                throw new ArgumentException("argument is not a number", nameof(number));
        }
    }

    public static (string, Color) GetInitMaterial(AstNode.Base @base)
    {
        #nullable enable
        var propertyList = (AstNode.PropertyList)@base;

        var nameBase = (AstNode.Base?) propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "nm").Value;
        var colorBase = (AstNode.Base?) propertyList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "color").Value;

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
#nullable disable

        return (((AstNode.String)nameBase).Value, color);
    }

    public static (string[] categories, (string, Color)[][]) GetMaterialInit(string text)
    {
        #nullable enable
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<string> categories = [];
        List<(string, Color)[]> materials = [];

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
                    GLOBALS.Logger?.Error($"Failed to parse custom material: \"{(@base is null ? "<NULL>" : StringifyBase(@base))}\"");
                }
            }
        }

        #nullable disable
        return ([..categories], [..materials]);
    }
    
    public static InitPropBase GetInitProp(AstNode.Base @base)
    {
        var propertList = ((AstNode.PropertyList)@base).Values;

        string name = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
        string typeStr = ((AstNode.String)propertList.Single(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;

        // retrieve all possible properties

#nullable enable

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
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(colorTreatment))
                    : colorTreatment.Value switch { "standard" => InitPropColorTreatment.Standard, "bevel" => InitPropColorTreatment.Bevel, _ => throw new InvalidInitPropertyValueException("", StringifyBase(@base), "colorTreatment", colorTreatment.Value) };

        int getIntProperty(AstNode.Number? property) => property is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(property))
                    : NumberToInteger(property);

        float getFloatProperty(AstNode.Number? property) => property is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(property))
                    : NumberToFloat(property);
#nullable disable

        //

        var type = typeStr switch
        {
            "standard"          => InitPropType.Standard,
            "variedStandard"    => InitPropType.VariedStandard,
            "soft"              => InitPropType.Soft,
            "variedSoft"        => InitPropType.VariedSoft,
            "simpleDecal"       => InitPropType.SimpleDecal,
            "variedDecal"       => InitPropType.VariedDecal,
            "antimatter"        => InitPropType.Antimatter,
            "softeffect"        => InitPropType.SoftEffect,
            "long"              => InitPropType.Long,

            _ => throw new InvalidPropTypeException("", StringifyBase(@base), typeStr)
        };

        InitPropBase init = type switch
        {
            InitPropType.Standard => new InitStandardProp(
                name, 
                type,
                depth is null ? 0 :  getIntProperty(depth),
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

            InitPropType.VariedStandard => new InitVariedStandardProp(
                name, 
                type,
                depth is null ? 0 :  getIntProperty(depth),
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
            
            InitPropType.Soft => new InitSoftProp(
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
            
            InitPropType.SoftEffect => new InitSoftEffectProp(
                name,
                type,
                depth is null ? 0 :  getIntProperty(depth)
            ),

            InitPropType.VariedSoft => new InitVariedSoftProp(
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
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                    : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]), NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random),
                getIntProperty(colorize)),

            InitPropType.SimpleDecal => new InitSimpleDecalProp(name, type, depth is null ? 0 :  getIntProperty(depth)),
            InitPropType.VariedDecal => new InitVariedDecalProp(
                name, 
                type, 
                depth is null ? 0 :  getIntProperty(depth),
                pxlSize is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(pxlSize))
                    : (NumberToInteger((AstNode.Number)pxlSize.Arguments[0]), NumberToInteger((AstNode.Number)pxlSize.Arguments[1])),
                getIntProperty(vars),
                random is null ? 0 : getIntProperty(random)
            ),
            InitPropType.Antimatter => new InitAntimatterProp(name, type, depth is null ? 0 :  getIntProperty(depth), getFloatProperty(contourExp)),
            InitPropType.Long => new InitLongProp(name, type, depth is null ? 0 : getIntProperty(depth)),
            _ => throw new InvalidPropTypeException("", StringifyBase(@base), typeStr)
        };

        return init;
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

    public static ((string category, Color color)[] categories, InitPropBase[][] init) GetPropsInit(string text)
    {
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<(string, Color)> categories = [];
        List<InitPropBase[]> props = [];

        foreach (var line in lines)
        {
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
                var obj = LingoParser.Expression.ParseOrThrow(line);

                var prop = GetInitProp(obj);

                props[^1] = [.. props[^1], prop];
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
    public static List<(InitPropType type, (int category, int index) position, Prop prop)>GetProps(AstNode.Base @base)
    {
#nullable enable
        var list = (AstNode.List)((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "props").Value;

        List<(InitPropType type, (int category, int index) position, Prop prop)> props = [];

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

                return new Vector2(NumberToInteger(globalCallArgs[0]), NumberToInteger(globalCallArgs[1]));
            }).ToArray();

            var type = InitPropType.Tile;

            (int category, int index) position;
            
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
            
            for (var c = 0; c < GLOBALS.RopeProps.Length; c++)
            {
                var initProp = GLOBALS.RopeProps[c];
                    
                if (initProp.Name == name)
                {
                    type = initProp.Type;
                    position = (-1, c);
                        
                    goto end_check;
                }
            }
            
            // if not found, then check if it's a tile

            for (var c = 0; c < GLOBALS.Tiles.Length; c++)
            {
                for (var t = 0; t < GLOBALS.Tiles[c].Length; t++)
                {
                    if (GLOBALS.Tiles[c][t].Name != name) continue;
                    
                    position = (c, t);
                    goto end_check;
                }
            }
            
            throw new PropNotFoundException($"prop not found: {name}", name);

        end_check:

            var extraPropList = (AstNode.PropertyList)casted[4];

            var settingsPropList = (AstNode.PropertyList)extraPropList.Values.Single(p => ((AstNode.Symbol)p.Key).Value == "settings").Value;
            AstNode.List? ropePoints = (AstNode.List?) extraPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "points").Value;

            AstNode.Number? variation = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "variation").Value;
            AstNode.Base? release = (AstNode.Base?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "release").Value;
            AstNode.Number? customDepth = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "customdepth").Value;
            AstNode.Number? applyColor = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "applycolor").Value;
            AstNode.Number? thickness = (AstNode.Number?) settingsPropList.Values.SingleOrDefault(p => ((AstNode.Symbol)p.Key).Value == "thickness").Value;

            int getIntProperty(AstNode.Number? property) => property is null
                    ? throw new MissingInitPropertyException("", StringifyBase(@base), nameof(property))
                    : NumberToInteger(property);

            BasicPropSettings settings = type switch
            {
                InitPropType.VariedStandard => new PropVariedSettings(depth, rng.Next(1000), 0, getIntProperty(variation) - 1),
                InitPropType.Rope => new PropRopeSettings(depth, rng.Next(1000), 0, getIntProperty(release switch
                {
                    AstNode.Number n => n,
                    AstNode.UnaryOperator u => (AstNode.Number)u.Expression,
                    _ => null
                }) switch { 1 => PropRopeRelease.Right, -1 => PropRopeRelease.Left, _ => PropRopeRelease.None }, 
                    name is "Wire" or "Zero-G Wire" ? thickness is null ? 2 : NumberToFloat(thickness) : null, 
                    name == "Zero-G Tube" ? getIntProperty(applyColor) : null),
                InitPropType.VariedDecal => new PropVariedDecalSettings(depth, rng.Next(1000), 0, getIntProperty(variation) - 1, getIntProperty(customDepth)),
                InitPropType.VariedSoft => new PropVariedSoftSettings(depth, rng.Next(1000), 0, getIntProperty(variation) - 1, getIntProperty(customDepth),
                    ((InitVariedSoftProp)GLOBALS.Props[position.category][position.index]).Colorize == 0 ? 1 : null),
                InitPropType.SimpleDecal => new PropSimpleDecalSettings(depth, rng.Next(1000), 0, getIntProperty(customDepth)),
                InitPropType.Soft => new PropSoftSettings(depth, rng.Next(1000), 0, getIntProperty(customDepth)),
                InitPropType.SoftEffect => new PropSoftEffectSettings(depth, rng.Next(1000), 0, getIntProperty(customDepth)),
                InitPropType.Antimatter => new PropAntimatterSettings(depth, rng.Next(1000), 0, getIntProperty(customDepth)),
                _ => new BasicPropSettings(seed: rng.Next(1000))
            };

            var parsedProp = new Prop(depth, name, type == InitPropType.Tile,
                new(quads[0], quads[1], quads[2], quads[3]))
            {
                Extras = new PropExtras(
                    settings: settings, 
                    ropePoints: (ropePoints?.Values.Select(p =>
                    {
                        var args = ((AstNode.GlobalCall)p).Arguments;
                        return new Vector2(NumberToInteger(args[0])/1.25f, NumberToInteger(args[1])/1.25f);
                    }).ToArray() ?? [])
                ),
                IsTile = type == InitPropType.Tile
            };

            props.Add((type, position, parsedProp));
        }
#nullable disable
        return props;
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

            // var topLeft = new Vector2((float) (currentQuads.Item1.radius * Math.Cos(currentQuads.Item1.angle)), (float) (currentQuads.Item1.radius * Math.Sin(currentQuads.Item1.angle)));
            // var topRight = new Vector2((float) (currentQuads.Item2.radius * Math.Cos(currentQuads.Item2.angle)), (float) (currentQuads.Item2.radius * Math.Sin(currentQuads.Item2.angle)));
            // var bottomRight = new Vector2((float) (currentQuads.Item3.radius * Math.Cos(currentQuads.Item3.angle)), (float) (currentQuads.Item3.radius * Math.Sin(currentQuads.Item3.angle)));
            // var bottomLeft = new Vector2((float) (currentQuads.Item4.radius * Math.Cos(currentQuads.Item4.angle)), (float) (currentQuads.Item4.radius * Math.Sin(currentQuads.Item4.angle)));

            var topLeft = currentQuads.Item1;
            var topRight = currentQuads.Item2;
            var bottomRight = currentQuads.Item3;
            var bottomLeft = currentQuads.Item4;
            
            result[c] = new() {
                Coords = new Vector2(cameras[c].x, cameras[c].y),

                Quad = new CameraQuad(
                    topLeft, 
                    topRight, 
                    bottomRight, 
                    bottomLeft)
            };
        }

        return result.ToList();
    }

    public static (string Name, EffectOptions[] options, double[,] Matrix)[] GetEffects(AstNode.Base @base, int width, int height) {
        #nullable enable
        var matrixProp = ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "effects").Value;

        var effectList = ((AstNode.List)matrixProp).Values.Select(e => {
            var props = ((AstNode.PropertyList)e).Values;

            var name = ((AstNode.String) props.Single(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
            
            var mtxWidth = ((AstNode.List) props.Single(p => ((AstNode.Symbol)p.Key).Value == "mtrx").Value).Values.Cast<AstNode.List>().ToArray();

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

            for (var x = 0; x < mtxWidth.Length; x++) {
                var mtxHeight = mtxWidth[x].Values.Cast<AstNode.Number>().Select(n => n.Value.DecimalValue).ToArray();

                for (int y = 0; y < mtxHeight.Length; y++) {
                    matrix[y, x] = mtxHeight[y];
                }
            }


            return (name, options, matrix);
        }).ToArray();
        
        #nullable disable

        return effectList;
    }

    public static InitTile GetInitTile(AstNode.Base @base) {
        var propList = (AstNode.PropertyList)@base;

        string name = ((AstNode.String)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "nm").Value).Value;
        
        AstNode.Base[] sizeArgs;

        sizeArgs = ((AstNode.GlobalCall)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "sz").Value).Arguments;
        
        var size = (((AstNode.Number)sizeArgs[0]).Value.IntValue, ((AstNode.Number)sizeArgs[1]).Value.IntValue);
        int[] specs = ((AstNode.List)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "specs").Value)
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
        var specs2Extracted = propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "specs2").Value;

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

        string tpString = ((AstNode.String)propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "tp").Value).Value;

        InitTileType tp = tpString switch {
            "box" => InitTileType.Box,
            "voxelStruct" => InitTileType.VoxelStruct,
            "voxelStructRandomDisplaceHorizontal" => InitTileType.VoxelStructRandomDisplaceHorizontal,
            "voxelStructRandomDisplaceVertical" => InitTileType.VoxelStructRandomDisplaceVertical,
            "voxelStructRockType" => InitTileType.VoxelStructRockType,
            "voxelStructSandType" => InitTileType.VoxelStructSandtype,

            _ => throw new Exception("Invalid tile init tag: "+ tpString)
        };

        var repeatLOptional = propList.Values.Any(p => ((AstNode.Symbol)p.Key).Value == "repeatl");
    
        int[] repeatL;

        if (repeatLOptional) {
            repeatL = ((AstNode.List) propList.Values.First(p => ((AstNode.Symbol)p.Key).Value == "repeatl").Value).Values.Select(n => {
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

    public static ((string, Color)[], InitTile[][]) GetTileInit(string text) {
        var lines = text.ReplaceLineEndings().Split(Environment.NewLine);

        List<(string, Color)> keys = [];
        List<InitTile[]> tiles = [];
        

        foreach (var line in lines) {
            if (string.IsNullOrEmpty(line) || line.StartsWith("--")) continue;

            if (line.StartsWith('-')) {
                tiles.Add([]);

                var headerList = (AstNode.List) LingoParser.Expression.ParseOrThrow(line.TrimStart('-'));

                string name  = ((AstNode.String) headerList.Values[0]).Value;

                var colorArgs = ((AstNode.GlobalCall) headerList.Values[1]).Arguments;
                var headerColor = (((AstNode.Number)colorArgs[0]).Value.IntValue, ((AstNode.Number)colorArgs[1]).Value.IntValue, ((AstNode.Number)colorArgs[2]).Value.IntValue);

                keys.Add((name, new(headerColor.Item1, headerColor.Item2, headerColor.Item3, 255)));
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
                    Stackables = DecomposeStackables(((AstNode.List)((AstNode.List)layer1).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 1] = new() {
                    Geo = ((AstNode.Number)((AstNode.List)layer2).Values[0]).Value.IntValue,
                    Stackables = DecomposeStackables(((AstNode.List)((AstNode.List)layer2).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
                };

                matrix[y, x, 2] = new() {
                    Geo = ((AstNode.Number)((AstNode.List)layer3).Values[0]).Value.IntValue,
                    Stackables = DecomposeStackables(((AstNode.List)((AstNode.List)layer3).Values[1]).Values.Select(e => ((AstNode.Number)e).Value.IntValue))
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
                var category = pointArgs[0] is AstNode.UnaryOperator u ? (-1 * ((AstNode.Number)u.Expression).Value.IntValue) : ((AstNode.Number)pointArgs[0]).Value.IntValue;
                var position = pointArgs[1] is AstNode.UnaryOperator u2 ? (-1 * ((AstNode.Number)u2.Expression).Value.IntValue) : ((AstNode.Number)pointArgs[1]).Value.IntValue;

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

    public static bool GetLightMode(AstNode.Base @base)
    {
        var light = (AstNode.Number)((AstNode.PropertyList)@base).Values
            .Single(p => ((AstNode.Symbol)p.Key).Value == "light").Value;

        return light.Value.IntValue == 1;
    }

    public static bool GetTerrainMedium(AstNode.Base @base)
    {
        var terrain = (AstNode.Number) ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultterrain").Value;

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
        var materialBase = (AstNode.String) ((AstNode.PropertyList)@base).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "defaultmaterial").Value;

        return materialBase.Value;
    }
}