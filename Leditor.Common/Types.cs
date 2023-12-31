namespace Leditor.Common;

using System.Numerics;
using System.Runtime.CompilerServices;

public struct RunCell {
    public int Geo { get; set; } = 0;
    public bool[] Stackables { get; set; } = new bool[22];

    public RunCell() {
        Geo = 0;
        Stackables = new bool[22];
    }
}

public enum TileType { Default, Material, TileHead, TileBody }

public struct TileCell {
    public TileType Type { get; set; }
    public dynamic Data { get; set; }

    public TileCell()
    {
        Type = TileType.Default;
        Data = new TileDefault();
    }

    public readonly override string ToString() => Data.ToString();
}


public struct TileDefault
{
    public int Value => 0;

    public readonly override string ToString() => $"TileDefault";
}

public struct TileMaterial(string data)
{
    private string _data = data;

    public string Name
    {
        readonly get => _data;
        set { _data = value; }
    }

    public override string ToString() => $"TileMaterial(\"{_data}\")";
}

public struct TileHead(int category, int position, string name)
{
    private (int, int, string) _data = (category, position, name);
    
    public (int, int, string) CategoryPostition
    {
        readonly get => _data;
        set { _data = value; }
    }

    public readonly override string ToString() => $"TileHead({_data.Item1}, {_data.Item2}, \"{_data.Item3}\")";
}

public struct TileBody(int x, int y, int z)
{
    private (int x, int y, int z) _data = (x, y, z);
    public (int x, int y, int z) HeadPosition
    {
        readonly get => _data;
        set { _data = value; }
    }

    public readonly override string ToString() => $"TileBody({_data.x}, {_data.y}, {_data.z})";
}

public enum InitTileType { 
    Box, 
    VoxelStruct, 
    VoxelStructRandomDisplaceHorizontal,
    VoxelStructRandomDisplaceVertical,
    VoxelStructRockType,
    VoxelStructSandtype
}

public readonly record struct InitTile(
    string Name,
    (int, int) Size,
    int[] Specs,
    int[] Specs2,
    InitTileType Type,
    int[] Repeat,
    int BufferTiles,
    int Rnd,
    int PtPos,
    string[] Tags
);

public readonly record struct BufferTiles(int Left, int Right, int Top, int Bottom);

public class CameraQuads(
    Vector2 topLeft, 
    Vector2 topRight, 
    Vector2 bottomRight, 
    Vector2 bottomLeft
) {
    public Vector2 TopLeft { get; set; } = topLeft; 
    public Vector2 TopRight { get; set; } = topRight;
    public Vector2 BottomRight { get; set; } = bottomRight; 
    public Vector2 BottomLeft { get; set; } = bottomLeft;
};

public record CameraQuadsRecord(
    (int Angle, float Radius) TopLeft, 
    (int Angle, float Radius) TopRight, 
    (int Angle, float Radius) BottomRight, 
    (int Angle, float Radius) BottomLeft
);

public class RenderCamera {
    public (float x, float y) Coords { get; set; }
    public CameraQuads Quads { get; set; }
}
