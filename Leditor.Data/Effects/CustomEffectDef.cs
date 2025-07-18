namespace Leditor.Data.Effects;

public class CustomEffectDef(string name, string category, string type)
{
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Type { get; set; } = type;
    public bool Can3D { get; set; }
    public bool PickColor { get; set; }

    public EffectOptions[] GenerateOptions()
    {
        List<EffectOptions> options = [];

        if (Name is "individual" or "individualHanger" or "indiviualClinger")
            options.Add(new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1"));
        else
            options.Add(new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"));

        if (PickColor)
            options.Add(new("Color", ["Color1", "Color2", "Dead"], "Color1"));

        if (Type is "wall" && Can3D)
            options.Add(new("3D", ["On", "Off"], "Off"));

        if (Type is "clinger" or "standardClinger")
            options.Add(new("Slide", ["Left", "Right", "Random"], "Random"));

        if (Type is "grower" or "hanger" or "clinger")
            options.Add(new("Require In-Bounds", ["Yes", "No"], "No"));

        return [.. options];
    }
}