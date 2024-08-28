using Raylib_cs;

namespace Leditor.Data;

public readonly record struct CastMember(
    string Name, 
    int Offset, 
    Texture2D Texture
);