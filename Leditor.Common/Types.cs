namespace Leditor.Common;

using System.Numerics;
using Quad = (int angle, float radius);

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
    public TileDataBase Data { get; set; }
}

public abstract class TileDataBase {
    public abstract dynamic Value { get; set; }
}

public class TileData1 : TileDataBase {
    public override dynamic Value {
        get => 0;
        set {  }
    }
}

public class TileData2(string data) : TileDataBase {
    private string _data = data;
    
    public override dynamic Value {
        get => _data;
        set { _data = value; }
    }
}

public class TileData3(int category, int position, string name) : TileDataBase {
    private (int, int, string) _data = (category, position, name);
    public override dynamic Value {
        get => _data;
        set { _data = value; }
    }
}

public class TileData4(int category, int position, int z) : TileDataBase {
    private (int, int, int) _data = (category, position, z);
    public override dynamic Value {
        get => _data;
        set { _data = value; }
    }
}

public enum InitTileType { 
    Box, 
    VoxelStruct, 
    VoxelStructRandomDisplaceHorizontal,
    VoxelStructRandomDisplaceVertical,
    VoxelStructRockType,
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
