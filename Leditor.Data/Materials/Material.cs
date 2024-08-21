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

public readonly record struct Material(
    string Name, 
    Color Color, 
    MaterialRenderType RenderType
) : IIdentifiable<string> {
    public static implicit operator (string, Color)(Material m) => (m.Name, m.Color);

    public void Deconstruct(out string name, out Color color) {
        name = Name;
        color = Color;
    }
};
