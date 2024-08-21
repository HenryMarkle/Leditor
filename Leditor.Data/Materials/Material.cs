using Leditor.Data.Generic;

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

public class Material : IIdentifiable<string>
{

    public string Name { get; init; }
    public Color Color { get; init; }
    public MaterialRenderType RenderType { get; init; }

    private readonly int _hashCode;

    public Material(string name, Color color, MaterialRenderType renderType)
    {
        Name = name;
        Color = color;
        RenderType = renderType;

        _hashCode = name.GetHashCode();
    }

    public Material(string name, Color color)
    {
        Name = name;
        Color = color;
        RenderType = MaterialRenderType.CustomUnified;

        _hashCode = name.GetHashCode();
    }

    public static implicit operator (string, Color)(Material m) => (m.Name, m.Color);

    public static bool operator ==(Material? lhs, Material? rhs)
    {
        if (lhs is null && rhs is null) return true;
        if (lhs is null || rhs is null) return false;

        return lhs.GetHashCode() == rhs.GetHashCode();
    }

    public static bool operator !=(Material? lhs, Material? rhs)
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
        if (obj is not Material m) return false;

        return _hashCode == m.GetHashCode();
    }
};
