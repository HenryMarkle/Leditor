using Leditor.Data;
using Leditor.Data.Tiles;

namespace Leditor.Types;

#nullable enable

internal record struct Place(Coords Position, TileDefinition Tile);

internal record struct Remove(Coords Position, TileDefinition Tile);