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

        return new RunCell(0, sta);
    }
}

public class RunCell(int geo, bool[] stackables) {
    public int geo = geo;
    public bool[] stackables = stackables;

    public RunCell() : this(0, new bool[22]) {}

    public Cell ToCell() {
        List<int> sta = [];

        for (int i = 1; i < stackables.Length; i++) {
            if (stackables[i]) sta.Add(i);
        }

        return new Cell(geo, [.. sta]);
    }
}

public struct BufferTiles(int left, int right, int top, int bottom) {
    public int Left { get; set; } = left;
    public int Right { get; set; } = right;
    public int Top { get; set; } = top;
    public int Bottom { get; set; } = bottom;
}
