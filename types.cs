using System.Text.Json;

namespace Leditor;

public struct Cell(int geo, int[] stackables)
{
    public int geo = geo;
    public int[] stackables = stackables;

    public Cell() : this(0, []) {}

    public readonly RunCell ToRunCell() {
        bool[] sta = new bool[16];

        foreach (var s in stackables) {
            sta[s] = true;
        }

        return new RunCell {
            Geo = geo,
            Stackables = sta,
        };
    }
}

public struct RunCell {
    public int Geo { get; set; } = 0;
    public bool[] Stackables { get; set; } = new bool[22];

    public RunCell() {
        Geo = 0;
        Stackables = new bool[22];
    }

    public Cell ToCell() {
        List<int> sta = [];

        for (int i = 1; i < Stackables.Length; i++) {
            if (Stackables[i]) sta.Add(i);
        }

        return new Cell(Geo, [.. sta]);
    }
}

public struct BufferTiles(int left, int right, int top, int bottom) {
    public int Left { get; set; } = left;
    public int Right { get; set; } = right;
    public int Top { get; set; } = top;
    public int Bottom { get; set; } = bottom;
}

