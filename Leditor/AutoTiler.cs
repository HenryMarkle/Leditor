using Leditor.Data.Tiles;

namespace Leditor;

#nullable enable

public class AutoTiler
{
    public record PackMeta(
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
    
    internal record Pack(
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

    internal AutoTiler(IEnumerable<PackMeta> data)
    {
        if (GLOBALS.TileDex is null) throw new NullReferenceException("TileDex is not set");

        List<Pack> packs = [];

        foreach (var packMeta in data)
        {
            if (string.IsNullOrEmpty(packMeta.Name)) 
                throw new NullReferenceException("Pack name cannot be empty or null");

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
            
            var newPack = new Pack(packMeta.Name,
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

        Packs = packs;
        SelectedPack = packs is [] ? null : packs[0];
    }
    
    internal List<Pack> Packs { get; private set; }
    internal Pack? SelectedPack { get; set; }

    private TileDefinition? ResolveNode(Node node, Pack pack) => node switch
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
    internal List<ResolvedTile> Resolve(LinkedList<Data.Coords> path)
    {
        if (SelectedPack is null) throw new NullReferenceException("No selected auto-tiler pack");
        
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

            var resolvedTile = ResolveNode(newNode, SelectedPack);

            tiles.Add(new ResolvedTile { Coords = currentCoord, Tile = resolvedTile });
        }

        return tiles;
    }

    internal IEnumerable<ResolvedTile> ResolveLinear(LinkedList<Data.Coords> path)
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

                tiles[^1] = new ResolvedTile { Coords = previousCoord, Tile = ResolveNode(nodes[^1], SelectedPack) };
            }
                
            skip_prev:

            nodes.Add(node);
            coorIndices[currentCoord] = nodes.Count;
            tiles.Add(new ResolvedTile { Coords = currentCoord, Tile = ResolveNode(node, SelectedPack) });
        }

        return tiles;
    }
}
