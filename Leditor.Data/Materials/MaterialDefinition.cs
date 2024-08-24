using Leditor.Data.Generic;
using Raylib_cs;

namespace Leditor.Data.Materials;

public enum MaterialRenderType {
    Unified,
    Tiles,
    Pipe,
    Invisible,
    LargeTrash,
    Dirt,
    Ceramic,
    DensePipe,
    Ridge,
    CeramicA,
    CeramicB,
    RandomPipes,
    Rock,
    RoughRock,
    Sandy,
    MegaTrash,
    WV,

    CustomUnified
}

public class MaterialDefinition : IDefinition
{
    public string Name { get; init; }
    public Color Color { get; init; }
    public MaterialRenderType RenderType { get; init; }

    public Texture2D Texture { get; set; }

    private readonly int _hashCode;

    public MaterialDefinition(string name, Color color, MaterialRenderType renderType)
    {
        Name = name;
        Color = color;
        RenderType = renderType;

        _hashCode = name.GetHashCode();
    }

    public MaterialDefinition(string name, Color color)
    {
        Name = name;
        Color = color;
        RenderType = MaterialRenderType.CustomUnified;

        _hashCode = name.GetHashCode();
    }

    public static implicit operator (string, Color)(MaterialDefinition m) => (m.Name, m.Color);

    public static bool operator ==(MaterialDefinition? lhs, MaterialDefinition? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;

        return lhs.GetHashCode() == rhs.GetHashCode();
    }

    public static bool operator !=(MaterialDefinition? lhs, MaterialDefinition? rhs)
    {
        if (lhs is null && rhs is null) return false;
        if (lhs is null || rhs is null) return true;

        return lhs.GetHashCode() != rhs.GetHashCode();
    }

    public void Deconstruct(out string name, out Color color) {
        name = Name;
        color = Color;
    }

    public override int GetHashCode() => _hashCode;

    public override bool Equals(object? obj)
    {
        if (obj is not MaterialDefinition m) return false;

        return _hashCode == m.GetHashCode();
    }
};
