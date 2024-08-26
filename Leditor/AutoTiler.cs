using System.Windows.Forms;
using Leditor.Data.Tiles;

namespace Leditor;

#nullable enable

public class AutoTiler
{
    public record PathPackMeta(
        string Name,
        string Vertical,
        string Horizontal,
        string TopLeft,
        string TopRight,
        string BottomRight,
        string BottomLeft,
        string Cross,
        string LeftTopRight,
        string TopRightBottom,
        string RightBottomLeft,
        string BottomLeftTop);

    public record BoxPackMeta(
        string Name,

        string Left,
        string Top,
        string Right,
        string Bottom,

        string TopLeft,
        string TopRight,
        string BottomRight,
        string BottomLeft,

        string[] Inside
    );

    
    internal record PathPack(
        string Name,
        TileDefinition Vertical,
        TileDefinition Horizontal,
        TileDefinition TopLeft,
        TileDefinition TopRight,
        TileDefinition BottomRight,
        TileDefinition BottomLeft,
        TileDefinition? Cross = null,
        TileDefinition? LeftTopRight = null,
        TileDefinition? TopRightBottom = null,
        TileDefinition? RightBottomLeft = null,
        TileDefinition? BottomLeftTop = null);

    public record BoxPack(
        string Name,

        TileDefinition Left,
        TileDefinition Top,
        TileDefinition Right,
        TileDefinition Bottom,

        TileDefinition TopLeft,
        TileDefinition TopRight,
        TileDefinition BottomRight,
        TileDefinition BottomLeft,

        (TileDefinition, Tile[,,])[] Inside
    );

    internal struct Node
    {
        public bool Left { get; set; }
        public bool Top { get; set; }
        public bool Right { get; set; }
        public bool Bottom { get; set; }
    }

    internal struct ResolvedTile
    {
        public Data.Coords Coords { get; set; }
        public TileDefinition? Tile { get; set; }
    }
    
    //

    internal AutoTiler(IEnumerable<PathPackMeta> pathPacks, IEnumerable<BoxPackMeta> boxPacks)
    {
        if (GLOBALS.TileDex is null) throw new NullReferenceException("TileDex is not set");

        List<PathPack> packs = [];

        foreach (var packMeta in pathPacks)
        {
            if (string.IsNullOrEmpty(packMeta.Name)) 
                throw new NullReferenceException("Pack name cannot be null or empty");

            GLOBALS.TileDex.TryGetTile(packMeta.Vertical, out var vertical);
            GLOBALS.TileDex.TryGetTile(packMeta.Horizontal, out var horizontal);
            
            GLOBALS.TileDex.TryGetTile(packMeta.TopLeft, out var topLeft);
            GLOBALS.TileDex.TryGetTile(packMeta.TopRight, out var topRight);
            GLOBALS.TileDex.TryGetTile(packMeta.BottomRight, out var bottomRight);
            GLOBALS.TileDex.TryGetTile(packMeta.BottomLeft, out var bottomLeft);
            
            GLOBALS.TileDex.TryGetTile(packMeta.Cross, out var cross);
            GLOBALS.TileDex.TryGetTile(packMeta.LeftTopRight, out var leftTopRight);
            GLOBALS.TileDex.TryGetTile(packMeta.TopRightBottom, out var topRightBottom);
            GLOBALS.TileDex.TryGetTile(packMeta.RightBottomLeft, out var rightBottomLeft);
            GLOBALS.TileDex.TryGetTile(packMeta.BottomLeftTop, out var bottomLeftTop);

            if (vertical is null || 
            horizontal is null || 
            topLeft is null || 
            topRight is null ||
            bottomRight is null ||
            bottomLeft is null) continue;
            
            var newPack = new PathPack(packMeta.Name,
                vertical,
                horizontal,
                topLeft,
                topRight,
                bottomRight,
                bottomLeft,
                cross,
                leftTopRight,
                topRightBottom,
                rightBottomLeft,
                bottomLeftTop
            );
            
            packs.Add(newPack);
        }

        List<BoxPack> parsedBoxPacks = [];

        foreach (var pack in boxPacks)
        {
            if (string.IsNullOrEmpty(pack.Name)) 
                throw new NullReferenceException("Rect pack name cannot be null or empty");

            var left = GLOBALS.TileDex.GetTile(pack.Left);
            var top = GLOBALS.TileDex.GetTile(pack.Top);
            var right = GLOBALS.TileDex.GetTile(pack.Right);
            var bottom = GLOBALS.TileDex.GetTile(pack.Bottom);

            var topLeft = GLOBALS.TileDex.GetTile(pack.TopLeft);
            var topRight = GLOBALS.TileDex.GetTile(pack.TopRight);
            var bottomRight = GLOBALS.TileDex.GetTile(pack.BottomRight);
            var bottomLeft = GLOBALS.TileDex.GetTile(pack.BottomLeft);

            var insideTiles = pack.Inside.Select(i => {
                var tile = GLOBALS.TileDex.GetTile(i);
                var mtx = new Tile[tile.Size.Height, tile.Size.Width, 3];
                var center = Utils.GetTileHeadOrigin(tile);
                Utils.ForcePlaceTileWithoutGeo(mtx, tile, ((int)center.X, (int)center.Y, 0));

                return (tile, mtx);
            }).ToArray();

            var newPack = new BoxPack(pack.Name,
                left,
                top,
                right,
                bottom,
                topLeft,
                topRight,
                bottomRight,
                bottomLeft,
                insideTiles
            );

            parsedBoxPacks.Add(newPack);
        }

        PathPacks = packs;
        BoxPacks = parsedBoxPacks;

        SelectedPathPack = packs is [] ? null : packs[0];
        SelectedBoxPack = parsedBoxPacks is [] ? null : parsedBoxPacks[0];
    }
    
    internal List<PathPack> PathPacks { get; private set; }
    internal List<BoxPack> BoxPacks { get; private set;}

    internal PathPack? SelectedPathPack { get; set; }
    internal BoxPack? SelectedBoxPack { get; set; }

    private TileDefinition? ResolvePathNode(Node node, PathPack pack) => node switch
    {
        { Left: false, Top: false, Right: false, Bottom: false } 
            or 
        { Left: false, Top: true, Right: false, Bottom: false }
            or
        { Left: false, Top: false, Right: false, Bottom: true }
            or 
        { Left: false, Top: true, Right: false, Bottom: true } => pack.Vertical,
        
        
        { Left: false, Top: false, Right: true, Bottom: false }
            or
        { Left: true, Top: false, Right: false, Bottom: false }
            or
        { Left: true, Top: false, Right: true, Bottom: false } => pack.Horizontal,
        
        
        { Left: false, Top: false, Right: true, Bottom: true } => pack.TopLeft,
        { Left: true, Top: false, Right: false, Bottom: true } => pack.TopRight,
        { Left: true, Top: true, Right: false, Bottom: false } => pack.BottomRight,
        { Left: false, Top: true, Right: true, Bottom: false } => pack.BottomLeft,
        
        
        { Left: true, Top: true, Right: true, Bottom: true } => pack.Cross,
        
        
        { Left: true, Top: true, Right: true, Bottom: false } => pack.LeftTopRight,
        { Left: false, Top: true, Right: true, Bottom: true } => pack.TopRightBottom,
        { Left: true, Top: false, Right: true, Bottom: true } => pack.RightBottomLeft,
        { Left: true, Top: true, Right: false, Bottom: true } => pack.BottomLeftTop,
        
        // _ => pack.Cross
    };

    // Helps to determine tile connections
    internal List<ResolvedTile> ResolvePath(LinkedList<Data.Coords> path)
    {
        if (SelectedPathPack is null) throw new NullReferenceException("No selected auto-tiler pack");
        
        List<ResolvedTile> tiles = [];

        for (var current = path.First; current is not null; current = current.Next)
        {
            var currentCoord = current!.Value;

            var newNode = new Node();

            for (var next = path.First; next is not null; next = next.Next)
            {
                if (next == current) continue;
                
                var nextCoord = next!.Value;

                if (nextCoord.Y == currentCoord.Y)
                {
                    if (nextCoord.X == currentCoord.X + 1) newNode.Right = true;
                    else if (nextCoord.X == currentCoord.X - 1) newNode.Left = true;
                }
                else if (nextCoord.X == currentCoord.X)
                {
                    if (nextCoord.Y == currentCoord.Y + 1) newNode.Bottom = true;
                    else if (nextCoord.Y == currentCoord.Y - 1) newNode.Top = true;
                }
            }

            var resolvedTile = ResolvePathNode(newNode, SelectedPathPack);

            tiles.Add(new ResolvedTile { Coords = currentCoord, Tile = resolvedTile });
        }

        return tiles;
    }

    internal IEnumerable<ResolvedTile> ResolvePathLinear(LinkedList<Data.Coords> path)
    {
        Dictionary<Data.Coords, int> coorIndices = [];
        
        List<ResolvedTile> tiles = [];
        List<Node> nodes = [];

        for (var current = path.First; current is not null; current = current.Next)
        {
            var node = new Node();
            var currentCoord = current.Value;

            var exists = coorIndices.TryGetValue(currentCoord, out var duplicateCoordIndex);

            if (exists) node = nodes[duplicateCoordIndex];

            if (current.Previous is { } previous)
            {
                var previousCoord = previous.Value;
                    
                if (previousCoord == currentCoord) goto skip_prev;
                    
                if (previousCoord.Y == currentCoord.Y)
                {
                    if (previousCoord.X + 1 == currentCoord.X)
                    {
                        node.Left = true;
                        nodes[^1] = nodes[^1] with { Right = true };
                    }
                    else if (previousCoord.X - 1 == currentCoord.X)
                    {
                        node.Right = true;
                        nodes[^1] = nodes[^1] with { Left = true };
                    }
                }
                else if (previousCoord.X == currentCoord.X)
                {
                    if (previousCoord.Y + 1 == currentCoord.Y)
                    {
                        node.Top = true;
                        nodes[^1] = nodes[^1] with { Bottom = true };
                    }
                    else if (previousCoord.Y - 1 == currentCoord.Y)
                    {
                        node.Bottom = true;
                        nodes[^1] = nodes[^1] with { Top = true };
                    }
                }

                tiles[^1] = new ResolvedTile { Coords = previousCoord, Tile = ResolvePathNode(nodes[^1], SelectedPathPack!) };
            }
                
            skip_prev:

            nodes.Add(node);
            coorIndices[currentCoord] = nodes.Count;
            tiles.Add(new ResolvedTile { Coords = currentCoord, Tile = ResolvePathNode(node, SelectedPathPack!) });
        }

        return tiles;
    }

    // Generate Rect
    internal Tile[,,] GenerateBox(int width, int height) {
        if (width == 0 || height == 0) 
            throw new ArgumentException($"Cannot supply zero dimensions (width: {width}, height: {height}).");

        var matrix = new Tile[height, width, 3];

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                for (var z = 0; z < 3; z++) {
                    matrix[y, x, z] = new Tile();
                }
            }
        }

        var rnd = new Random();

        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                // Corners
                if (y == 0 && x == 0) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.TopLeft, (x, y, 0));
                    continue;
                } else if (y == 0 && x == width - 1) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.TopRight, (x, y, 0));
                    continue;
                } else if (x == 0 && y == height - 1) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.BottomLeft, (x, y, 0));
                    continue;
                } else if (x == width - 1 && y == height - 1) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.BottomRight, (x, y, 0));
                    continue;
                }

                // Sides

                if (y == 0) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.Top, (x, y, 0));
                    continue;
                } else if (y == height - 1) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.Bottom, (x, y, 0));
                    continue;
                }

                if (x == 0) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.Left, (x, y, 0));
                    continue;
                } else if (x == width - 1) {
                    Utils.ForcePlaceTileWithoutGeo(matrix, SelectedBoxPack!.Right, (x, y, 0));
                    continue;
                }

                // Inside

                if (SelectedBoxPack!.Inside is []) continue;

                if (matrix[y, x, 0].Type is not TileCellType.Default) continue;

                var (tile, mtx) = SelectedBoxPack!.Inside[rnd.Next(SelectedBoxPack!.Inside.Length)];
                
                for (var ty = 0; ty < tile.Size.Height; ty++) {
                    for (var tx = 0; tx < tile.Size.Width; tx++) {
                        for (var tz = 0; tz < 3; tz++) {
                            var sy = ty + y;
                            var sx = tx + x;

                            if (sy < 1 || sy >= matrix.GetLength(0) - 1 || sx < 1 || sx >= matrix.GetLength(1) - 1) continue;

                            var cell = mtx[ty, tx, tz];
                            
                            if (cell.Type is TileCellType.Body) {
                                var (bx, by, bz) = cell.HeadPosition;

                                cell = cell with { HeadPosition = (x + bx, y + by, bz) };
                            }

                            matrix[y + ty, x + tx, tz] = cell;
                        }
                    }
                }

                // for (var z = 0; z < 3; z++) {
                //     if (Utils.IsStrayTileBodyFragment(matrix, x, y, z)) matrix[y, x, z] = new TileCell();
                // }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Coordinates in matrix units
    /// </summary>
    internal IEnumerable<ResolvedTile> ResolveRect(int x, int y, int width, int height)
    {
        if (width is 0 || height is 0) return [];

        var x2 = x + width;
        var y2 = y + height;

        List<ResolvedTile> tiles = [];

        // Edges first

        // Top & Bottom
        for (var i = x; i <= x2; i++) {
            if (i == x) {
                tiles.Add(new ResolvedTile { Coords = new(x, y), Tile = SelectedBoxPack!.TopLeft });
                tiles.Add(new ResolvedTile { Coords = new(x, y2), Tile = SelectedBoxPack!.BottomLeft });
            } else if (i == x2) {
                tiles.Add(new ResolvedTile { Coords = new(i, y), Tile = SelectedBoxPack!.TopRight });
                tiles.Add(new ResolvedTile { Coords = new(i, y2), Tile = SelectedBoxPack!.BottomRight });
            } else {
                tiles.Add(new ResolvedTile { Coords = new(i, y), Tile = SelectedBoxPack!.Top });
                tiles.Add(new ResolvedTile { Coords = new(i, y2), Tile = SelectedBoxPack!.Bottom });
            }
        }

        // Left & Right
        for (var k = y + 1; k < y2; k++) {
            tiles.Add(new ResolvedTile { Coords = new(x, k), Tile = SelectedBoxPack!.Left });
            tiles.Add(new ResolvedTile { Coords = new(x, y2 - 1), Tile = SelectedBoxPack!.Right });
        }

        // Inside
        var rnd = new Random();

        for (var i = x + 1; i < x2; i++) {
            for (var k = y + 1; k < y2; k++) {
                
            }
        }

        return tiles;
    }
}
