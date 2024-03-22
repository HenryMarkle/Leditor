using System.Numerics;
using Leditor.Data.Props.Definitions;
using Leditor.Data.Props.Exceptions;
using Leditor.Data.Props.Settings;

namespace Leditor.Data.Props;

#nullable disable

public struct PropQuad
{
    public Vector2 TopLeft { get; set; }
    public Vector2 TopRight { get; set; }
    public Vector2 BottomRight { get; set; }
    public Vector2 BottomLeft { get; set; }
}

public class Prop(PropDefinition definition, PropSettings settings, int depth = 0)
{
    public int Depth { get; set; } = depth;
    
    //
    
    private PropQuad _quad;

    public PropQuad Quad
    {
        get => _quad; 
        set => _quad = value;
    }
    
    public ref PropQuad QuadRef => ref _quad;
    
    //

    public PropDefinition Definition { get; init; } = definition;
    
    //

    public PropSettings Settings { get; set; } = settings;
    
    //

    public static Prop Instantiate(string name, PropDex dex)
    {
        try
        {
            var definition = dex.GetDefinition(name);
            return new Prop(definition, definition.NewSettings());
        }
        catch (KeyNotFoundException e)
        {
            throw new PropNotFoundException(name, e);
        }
    }
}