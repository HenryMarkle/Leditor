namespace Leditor;

internal enum TilerId
{
    Vertical, Horizontal,
    TopLeft, TopRight,
    BottomLeft, BottomRight,
    LeftBottomRight, BottomLeftTop, LeftTopRight, TopRightBottom,
    Cross
}

internal readonly record struct TilerPack(
    (int Category, int Index) Vertical,
    (int Category, int Index) Horizontal,
    (int Category, int Index) TopLeft,
    (int Category, int Index) TopRight,
    (int Category, int Index) BottomRight,
    (int Category, int Index) BottomLeft,
    (int Category, int Index) LeftBottomRight,
    (int Category, int Index) BottomLeftTop,
    (int Category, int Index) LeftTopRight,
    (int Category, int Index) TopRightBottom,
    (int Category, int Index) Cross)
{
    public (int, int) this[TilerId id] => id switch
    {
        TilerId.Vertical => Vertical,
        TilerId.Horizontal => Horizontal,
        TilerId.TopLeft => TopLeft,
        TilerId.TopRight => TopRight,
        TilerId.BottomRight => BottomRight,
        TilerId.BottomLeft => BottomLeft,
        TilerId.LeftBottomRight => LeftBottomRight,
        TilerId.BottomLeftTop => BottomLeftTop,
        TilerId.LeftTopRight => LeftTopRight,
        TilerId.TopRightBottom => TopRightBottom,
        TilerId.Cross => Cross,
        
        _ => throw new ArgumentException($"Invalid id: {id}")
    };
}

internal sealed class AutoTiler
{
    public TilerPack Pack { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="matrix">must be 3*3</param>
    /// <returns></returns>
    public static TilerId Determine(bool[,] matrix)
    {
        if (matrix.GetLength(0) != 3 || matrix.GetLength(1) != 3) 
            throw new ArgumentException("Invalid matrix size");

        return (
                       false, matrix[0, 1],        false,
                matrix[1, 0],         true, matrix[1, 2],
                       false, matrix[2, 1],        false
        ) switch {
            (
                    _   , false, _,
                    true,     _, true,
                    _   , false, _
            ) => TilerId.Horizontal,
            (
                    _    , true, _,
                    false,    _, false,
                    _    , true, _
            ) => TilerId.Vertical,
            (
                    _    , true, _,
                     true,    _, true,
                    _    , true, _
            ) => TilerId.Cross,
        };
    }
    
    /// <summary>
    /// This function assumes that all provided indices are valid and in-bounds.
    /// </summary>
    /// <param name="path">A list of vertices resembling a path</param>
    /// <returns>A list of each vertex associated with the tile to place</returns>
    public IEnumerable<(int x, int y, (int category, int index))> Resolve(bool[,] path)
    {
        return [];
    }
}