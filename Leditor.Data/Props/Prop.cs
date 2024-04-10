using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Settings;

namespace Leditor.Data.Props;

public sealed class Prop : IClone<Prop>
{
    private int _depth;

    /// <summary>
    /// Must be between -29 and 0.
    /// </summary>
    public int Depth
    {
        get => _depth;
        set
        {
            _depth = value switch
            {
                > 0 => 0,
                < -29 => -29,
                _ => value
            };
        }
    }
    
    //
    
    private Quad _quad;

    public Quad Quad
    {
        get => _quad; 
        set => _quad = value;
    }
    
    public ref Quad QuadRef => ref _quad;
    
    //

    public PropDefinition Definition { get; init; }
    
    //

    public PropSettings Settings { get; internal set; }
    
    //

    public Prop(PropDefinition definition, PropSettings settings, Quad quad, int depth)
    {
        Depth = depth;
        Quad = quad;
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
    
    //

    public void ResetSettings()
    {
        Settings = Definition.NewSettings();
    }

    public Prop Clone()
    {
        var depth = Depth;
        var quad = Quad;
        var settings = Settings.Clone();

        return new Prop(Definition, settings, quad, depth);
    }

    public static Prop Instantiate(PropDefinition definition, Quad quad) => new(definition, definition.NewSettings(), quad, 0);
}