using System.Numerics;
using ImGuiNET;
using Leditor.Data.Tiles;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using RenderTexture2D = Leditor.RL.Managed.RenderTexture2D;
using Leditor.Types;

namespace Leditor.Pages;

#nullable enable

internal class TileEditorPage : EditorPage, IDisposable
{
    private Camera2D _camera = new() { Zoom = 1.0f };

    private readonly GlobalShortcuts _gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;
    private readonly TileShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.TileEditor;

    private bool _materialTileSwitch;
    
    private int _tileCategoryIndex;
    private int _tileIndex;
    
    private int _materialCategoryIndex;
    private int _materialIndex;

    private int _tileCategorySearchIndex;
    private int _tileSearchIndex;
    
    private int _materialCategorySearchIndex;
    private int _materialSearchIndex;

    private int _hoveredTileCategoryIndex = -1;
    private int _hoveredTileIndex = -1;

    private TileDefinition? _hoveredTile;
    private TileDefinition? _currentTile;

    private TileDefinition[] _currentCategoryTiles = GLOBALS.TileDex?.GetTilesOfCategory(GLOBALS.TileDex.OrderedCategoryNames[0]) ?? [];
    private (string name, Data.Color color) _currentCategory = (GLOBALS.TileDex?.OrderedCategoryNames[0] ?? "", GLOBALS.TileDex?.GetCategoryColor(GLOBALS.TileDex.OrderedCategoryNames[0]) ?? Color.White);
    
    private bool _clickTracker;
    private bool _tileCategoryFocus;
    private int _tileCategoryScrollIndex;
    private int _tileScrollIndex;
    private int _materialCategoryScrollIndex;
    private int _materialScrollIndex;

    private bool _highlightPaths;

    private bool _drawClickTracker;
    private bool _eraseClickTracker;
    private bool _copyClickTracker;

    private bool _deepTileCopy = true;

    private bool _showGrid;

    private int _prevPosX = -1;
    private int _prevPosY = -1;

    private int _copyFirstX = -1;
    private int _copyFirstY = -1;

    private int _copySecondX = -1;
    private int _copySecondY = -1;

    private bool _tooltip = true;

    private bool _showLayer1Tiles = true;
    private bool _showLayer2Tiles = true;
    private bool _showLayer3Tiles = true;

    private bool _showTileLayer1 = true;
    private bool _showTileLayer2 = true;
    private bool _showTileLayer3 = true;
    
    private string _searchText = string.Empty;
    private bool _isSearching;
    private Task _searchTask = default!;

    

    // Auto Tiler
    private LinkedList<Data.Coords> _autoTilerPath = new();
    private AutoTiler? _autoTiler;
    private bool _autoTilingCancled;

    /// <summary>
    /// 0 - Micro
    /// 1 - Macro
    /// </summary>
    private int _autoTilerMethod;
    private bool _autoTilerMacroFirstClick;
    private Data.Coords _autoTilerFirstClickPos;
    private bool _autoTilerMacroYAxisFirst;

    private bool _autoTilingWithGeo = true;
    private bool _autoTilerLinearAlgorithm = true;



    /// <summary>
    /// 0 - default
    /// 1 - with geo
    /// 2 - without geo
    /// 3 - paste with geo
    /// 4 - paste without geo
    /// 5 - auto-tiling
    /// 6 - rect
    /// </summary>
    private int _placeMode;

    private Rectangle _prevCopiedRectangle;
    private Rectangle _copyRectangle;
    private TileCell[,,] _copyBuffer = new TileCell[0,0,0];
    private RunCell[,,] _copyGeoBuffer = new RunCell[0, 0, 0];

    private (string category, Data.Color color, TileDefinition tile)[] _tileMatches = [];
    private (int category, int[] materials)[] _materialIndices = [];

    private RenderTexture2D _previewTooltipRT = new(0, 0);
    private RenderTexture2D _tileSpecsPanelRT = new(0, 0);
    private RenderTexture2D _tileTexturePanelRT = new(0, 0);
    
    private bool _shouldRedrawLevel = true;

    private bool _tileSpecDisplayMode;
    
    private int _tilePanelWidth = 400;
    private int _materialBrushRadius;

    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isTilesWinHovered;
    private bool _isTilesWinDragged;
    
    private bool _isSpecsWinHovered;
    private bool _isSpecsWinDragged;
    
    private bool _isSettingsWinHovered;
    private bool _isSettingsWinDragged;

    public void ResolveAutoTiler() {
        var resolvedTiles = _autoTilerLinearAlgorithm 
            ? _autoTiler?.ResolvePathLinear(_autoTilerPath) 
            : _autoTiler?.ResolvePath(_autoTilerPath);

        foreach (var resolved in resolvedTiles!)
        {
            if (!Utils.InBounds(resolved.Coords, GLOBALS.Level.TileMatrix) || resolved.Tile is null) continue;

            var actions = _autoTilingWithGeo 
                ? ForcePlaceTileWithGeo(resolved.Tile, resolved.Coords) 
                : ForcePlaceTileWithoutGeo(resolved.Tile, resolved.Coords);
            
            GLOBALS.Gram.Proceed(new Gram.GroupAction<TileCell>(actions));
        }        
    }
    
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        
        _tileSpecsPanelRT.Dispose();
        _tileTexturePanelRT.Dispose();
        _previewTooltipRT.Dispose();
    }

    ~TileEditorPage()
    {
        if (!Disposed) throw new InvalidOperationException("Page not disposed by consumer");
    }

    private void Search()
    {
        
        // Search tiles
        if (_materialTileSwitch)
        {
            if (GLOBALS.TileDex is null) return;
            var dex = GLOBALS.TileDex;
            
            List<(string category, Color color, TileDefinition tile)> matches = [];
            
            if (string.IsNullOrEmpty(_searchText))
            {
                _tileMatches = [];
                return;
            }
            
            for (var categoryIndex = 0; categoryIndex < dex.OrderedCategoryNames.Length; categoryIndex++)
            {
                var category = dex.OrderedCategoryNames[categoryIndex];
                var tiles = dex.GetTilesOfCategory(category);
                
                for (var tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
                {
                    var tile = tiles[tileIndex];

                    if (tile.Name.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase)) 
                        matches.Add((category, dex.GetCategoryColor(category), tile));
                }
            }
            
            _tileMatches = [..matches];

            if (_tileMatches.Length > 0)
            {
                _tileCategorySearchIndex = _tileSearchIndex = 0;
            }
        }
        // Search materials
        else
        {
            List<(int categories, int[] materials)> indices = [];
            
            if (string.IsNullOrEmpty(_searchText))
            {
                _materialIndices = [];
                return;
            }
            
            for (var categoryIndex = 0; categoryIndex < GLOBALS.Materials.Length; categoryIndex++)
            {
                List<int> matchedMaterials = [];
                
                for (var materialIndex = 0; materialIndex < GLOBALS.Materials[categoryIndex].Length; materialIndex++)
                {
                    var (material, _) =  GLOBALS.Materials[categoryIndex][materialIndex];
                    
                    if (material.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase)) matchedMaterials.Add(materialIndex);

                    if (materialIndex == GLOBALS.Materials[categoryIndex].Length - 1 &&
                        matchedMaterials.Count > 0)
                    {
                        indices.Add((categoryIndex, [..matchedMaterials]));
                    }
                }
            }

            _materialIndices = [..indices];

            if (_materialIndices.Length > 0)
            {
                _materialCategorySearchIndex = _materialSearchIndex = 0;
                
                _materialCategoryIndex = _materialIndices[0].category;
                _materialIndex = _materialIndices[0].materials[0];
            }
        }
    }

    private readonly List<Gram.ISingleMatrixAction<TileCell>> _tempActions = [];

    /// <summary>
    /// Has no regard to matrix bounds and gets unscaled coords of a matrix
    /// </summary>>
    private static Rectangle GetSingleTileRect(int x, int y, int z, int scale)
    {
        ref var cell = ref GLOBALS.Level.TileMatrix[y, x, z];

        Rectangle rectangle;
        var emptyRect = new Rectangle();

        switch (cell.Data)
        {
            case TileHead h:
            {
                // If tile is undefined
                if (h.Definition is null) return emptyRect;
                
                var tileInit = h.Definition;
                var (width, height) = tileInit.Size;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new Vector2(x, y), head);

                rectangle = new Rectangle(start.X*scale, start.Y*scale, width*scale, height*scale);

                return rectangle;
            }

            case TileBody b:
            {
                var (headX, headY, headZ) = b.HeadPosition;

                // This is done because Lingo is 1-based index
                var supposedHead = GLOBALS.Level.TileMatrix[headY - 1, headX - 1, headZ - 1];

                // if the head was not found, only delete the given tile body
                if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
                    return emptyRect;

                var headTile = (TileHead)supposedHead.Data;
            
                var tileInit = headTile.Definition!;
                var (width, height) = tileInit.Size;
                
                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new Vector2(headX, headY), Raymath.Vector2AddValue(head, 1));

                rectangle = new Rectangle(start.X*scale, start.Y*scale, width*scale, height*scale);

                return rectangle;
            }
            
            default: return new Rectangle(x*scale, y*scale, scale, scale);
        }
    }

    private void ToNextTileCategory()
    {
        _tileCategoryIndex++;
        
        if (GLOBALS.Settings.GeneralSettings.CycleMenus) _tileCategoryIndex %= GLOBALS.TileCategories.Length;
        else Utils.Restrict(ref _tileCategoryIndex, 0, GLOBALS.TileCategories.Length-1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _tileIndex = 0;
        else Utils.Restrict(ref _tileIndex, 0, GLOBALS.Tiles[_tileCategoryIndex].Length-1);
    }

    private void ToPreviousCategory()
    {
        _tileCategoryIndex--;

        if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _tileCategoryIndex, 0, GLOBALS.Tiles.Length - 1);
        else Utils.Restrict(ref _tileCategoryIndex, 0, GLOBALS.Tiles.Length - 1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _tileIndex = 0;
        else Utils.Restrict(ref _tileIndex, 0, GLOBALS.Tiles[_tileCategoryIndex].Length-1);
    }

    private void ToNextMaterialCategory()
    {
        _materialCategoryIndex++;

        if (GLOBALS.Settings.GeneralSettings.CycleMenus) _materialCategoryIndex %= GLOBALS.MaterialCategories.Length;
        else Utils.Restrict(ref _materialCategoryIndex, 0, GLOBALS.MaterialCategories.Length - 1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _materialIndex = 0;
        else Utils.Restrict(ref _materialIndex, 0, GLOBALS.Materials[_materialCategoryIndex].Length-1);
    }
    
    private void ToPreviousMaterialCategory()
    {
        _materialCategoryIndex--;

        if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _materialCategoryIndex, 0, GLOBALS.MaterialCategories.Length - 1);
        else Utils.Restrict(ref _materialCategoryIndex, 0, GLOBALS.MaterialCategories.Length - 1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _materialIndex = 0;
        else Utils.Restrict(ref _materialIndex, 0, GLOBALS.Materials[_materialCategoryIndex].Length-1);
    }

    private static bool IsCoordsInBounds(Coords position)
    {
        var outX = position.X < 0 || position.X >= GLOBALS.Level.Width;
        var outY = position.Y < 0 || position.Y >= GLOBALS.Level.Height;
        var outZ = position.Z is < 0 or > 2;

        return !outX && !outY && !outZ;
    }

    private static void Undo()
    {
        var currentAction = GLOBALS.Gram.Current;

        UndoThis(currentAction);
        GLOBALS.Gram.Undo();
        return;

        void UndoThis(Gram.IAction action)
        {
            switch (action)
            {
                case Gram.TileAction tileAction:
                {
                    if (!IsCoordsInBounds(tileAction.Position)) return;

                    var (x, y, z) = tileAction.Position;

                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileAction.Old);

                    if (tileAction.Old.Data is TileMaterial m) 
                        GLOBALS.Level.MaterialColors[y, x, z] = GLOBALS.MaterialColors[m.Name];
                    
                }
                    break;

                case Gram.TileGeoAction tileGeoAction:
                {
                    if (!IsCoordsInBounds(tileGeoAction.Position)) return;

                    var (x, y, z) = tileGeoAction.Position;
                    
                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileGeoAction.Old.Item1);
                    GLOBALS.Level.GeoMatrix[y, x, z] = Utils.CopyGeoCell(tileGeoAction.Old.Item2);
                }
                    break;
                
                case Gram.GroupAction<TileCell> groupAction:
                    foreach (var a in groupAction.Actions.Reverse()) UndoThis(a);
                    break;
            }
        }
    }
    
    private static void Redo()
    {
        var currentAction = GLOBALS.Gram.Current;

        GLOBALS.Gram.Redo();
        UndoThis(currentAction);
        return;

        void UndoThis(Gram.IAction action)
        {
            switch (action)
            {
                case Gram.TileAction tileAction:
                {
                    if (!IsCoordsInBounds(tileAction.Position)) return;

                    var (x, y, z) = tileAction.Position;

                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileAction.New);

                    if (tileAction.Old.Data is TileMaterial m) 
                        GLOBALS.Level.MaterialColors[y, x, z] = GLOBALS.MaterialColors[m.Name];
                    
                }
                    break;

                case Gram.TileGeoAction tileGeoAction:
                {
                    if (!IsCoordsInBounds(tileGeoAction.Position)) return;
                    
                    var (x, y, z) = tileGeoAction.Position;
                    
                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileGeoAction.New.Item1);
                    GLOBALS.Level.GeoMatrix[y, x, z] = Utils.CopyGeoCell(tileGeoAction.New.Item2);
                }
                    break;
                
                case Gram.GroupAction<TileCell> groupAction:
                    foreach (var a in groupAction.Actions) UndoThis(a);
                    break;
            }
        }
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> RemoveMaterial(int x, int y, int z, int radius)
    {
        List<Gram.ISingleMatrixAction<TileCell>> actions = [];
        
        for (var lx = -radius; lx < radius+1; lx++)
        {
            var matrixX = x + lx;
            
            if (matrixX < 0 || matrixX >= GLOBALS.Level.Width) continue;
            
            for (var ly = -radius; ly < radius+1; ly++)
            {
                var matrixY = y + ly;
                
                if (matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                var cell = GLOBALS.Level.TileMatrix[matrixY, matrixX, z];
                
                if (cell.Type != TileType.Default && cell.Type != TileType.Material) continue;

                var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                
                actions.Add(new Gram.TileAction((lx, ly, x), cell, newCell));
                
                GLOBALS.Level.TileMatrix[matrixY, matrixX, z] = newCell;
            }
        }
        
        return actions;
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> PlaceMaterial((string name, Color color) material, (int x, int y, int z) position, int radius)
    {
        var (x, y, z) = position;

        List<Gram.ISingleMatrixAction<TileCell>> actions = [];

        for (var lx = -radius; lx < radius+1; lx++)
        {
            var matrixX = x + lx;
            
            if (matrixX < 0 || matrixX >= GLOBALS.Level.Width) continue;
            
            for (var ly = -radius; ly < radius+1; ly++)
            {
                var matrixY = y + ly;
                
                if (matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                var cell = GLOBALS.Level.TileMatrix[matrixY, matrixX, z];
                
                if (cell.Type != TileType.Default && cell.Type != TileType.Material) continue;

                var newCell = new TileCell { Type = TileType.Material, Data = new TileMaterial(material.name) }; // waste of space, ik
                actions.Add(new Gram.TileAction((matrixX, matrixY, z), cell, newCell));
                
                GLOBALS.Level.TileMatrix[matrixY, matrixX, z] = newCell;
                GLOBALS.Level.MaterialColors[matrixY, matrixX, z] = material.color;
            }
        }
        
        return actions;
    }

    private static List<Gram.ISingleMatrixAction<TileCell>> RemoveTile(int mx, int my, int mz)
    {
        while (true)
        {
            var cell = GLOBALS.Level.TileMatrix[my, mx, mz];

            List<Gram.ISingleMatrixAction<TileCell>> actions = [];

            if (cell.Data is TileHead h)
            {
                // tile is undefined
                if (h.Definition is null)
                {
                    var oldCell = GLOBALS.Level.TileMatrix[my, mx, mz];
                    var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                    GLOBALS.Level.TileMatrix[my, mx, mz] = newCell;

                    return [new Gram.TileAction((mx, my, mz), oldCell, newCell)];
                }

                var data = h;
                var tileInit = h.Definition;
                var (width, height) = tileInit.Size;

                var specs = tileInit.Specs;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new(mx, my), head);

                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var matrixX = x + (int)start.X;
                        var matrixY = y + (int)start.Y;

                        if (matrixX < 0 || matrixX >= GLOBALS.Level.Width || matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                        var specsIndex = x * height + y;

                        var spec = specs[y, x, 0];
                        var spec2 = specs[y, x, 1];
                        var spec3 = specs[y, x, 2];

                        if (spec != -1)
                        {
                            var oldCell = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz];
                            var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new Gram.TileAction((matrixX, matrixY, mz), oldCell, newCell));

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                        }

                        if (mz != 2 && spec2 != -1)
                        {
                            var oldCell2 = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1];
                            var newCell2 = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new Gram.TileAction((matrixX, matrixY, mz + 1), oldCell2, newCell2));

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newCell2;
                        }

                        if (mz == 0 && spec3 != -1)
                        {
                            var oldCell3 = GLOBALS.Level.TileMatrix[matrixY, matrixX, 2];
                            var newCell3 = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new Gram.TileAction((matrixX, matrixY, 2), oldCell3, newCell3));

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, 2] = newCell3;
                        }
                    }
                }
            }
            else if (cell.Type == TileType.TileBody)
            {
                var (headX, headY, headZ) = ((TileBody)cell.Data).HeadPosition;

                // This is done because Lingo is 1-based index
                var supposedHead = GLOBALS.Level.TileMatrix[headY - 1, headX - 1, headZ - 1];

                // if the head was not found, only delete the given tile body
                if (supposedHead.Data is not TileHead)
                {
                    GLOBALS.Level.TileMatrix[my, mx, mz] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                    return [];
                }

                mx = headX - 1;
                my = headY - 1;
                mz = headZ - 1;
                continue;
            }

            return actions;
        }
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithGeo(
        in TileDefinition init,
        (int x, int y, int z) matrixPosition
    )
    {
        var (mx, my, mz) = matrixPosition;
        var (width, height) = init.Size;
        
        var specs = init.Specs;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(new Vector2(mx, my), head);
        
        List<Gram.ISingleMatrixAction<TileCell>> actions = [];
        
        // remove pre-existing tiles in the way

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;
                
                if (
                    !(matrixX >= 0 &&
                     matrixX < GLOBALS.Level.GeoMatrix.GetLength(1) &&
                     matrixY >= 0 &&
                     matrixY < GLOBALS.Level.GeoMatrix.GetLength(0))
                ) continue;
                
                var spec = specs[y, x, 0];
                var spec2 = specs[y, x, 1];
                var spec3 = specs[y, x, 2];

                if (spec != -1)
                {
                    if (GLOBALS.Level.TileMatrix[matrixY, matrixX, mz].Data is TileHead)
                        actions.AddRange(RemoveTile(matrixX, matrixY, mz));
                }

                if (spec2 != -1 && mz != 2)
                {
                    if (GLOBALS.Level.TileMatrix[matrixY, matrixX, mz+1].Data is TileHead)
                        actions.AddRange(RemoveTile(matrixX, matrixY, mz+1));
                }

                if (spec3 != -1 && mz == 0)
                {
                    if (GLOBALS.Level.TileMatrix[matrixY, matrixX, 2].Data is TileHead)
                        actions.AddRange(RemoveTile(matrixX, matrixY, 2));
                }
            }
        }
        
        // Place new tile

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;

                // This function depends on the rest of the program to guarantee that all level matrices have the same x and y dimensions
                if (
                    matrixX >= 0 &&
                    matrixX < GLOBALS.Level.GeoMatrix.GetLength(1) &&
                    matrixY >= 0 &&
                    matrixY < GLOBALS.Level.GeoMatrix.GetLength(0)
                )
                {
                    var spec = specs[y, x, 0];
                    var spec2 = specs[y, x, 1];
                    var spec3 = specs[y, x, 2];
                    
                    // If it's the tile head
                    if (x == (int)head.X && y == (int)head.Y)
                    {
                        // Place the head of the tile at matrixPosition
                        var newHead = new TileCell
                        {
                            Type = TileType.TileHead,
                            Data = new TileHead(init)
                        };
                        
                        actions.Add(new Gram.TileAction(matrixPosition, CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, mz]), CopyTileCell(newHead)));
                        
                        GLOBALS.Level.TileMatrix[my, mx, mz] = newHead;
                    }
                    
                    if (spec != -1)
                    {
                        GLOBALS.Level.GeoMatrix[matrixY, matrixX, mz].Geo = spec;
        
                        if (!(x == (int)head.X && y == (int)head.Y))
                        {
                            var newCell = new TileCell
                            {
                                Type = TileType.TileBody,
                                Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                            };
                            
                            actions.Add(new Gram.TileAction((matrixX, matrixY, mz),
                                CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, mz]), CopyTileCell(newCell)));
                            
                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                        }
                    }
                    
                    if (spec2 != -1 && mz != 2)
                    {
                        GLOBALS.Level.GeoMatrix[matrixY, matrixX, mz + 1].Geo = spec2;
                        
                        var newerCell = new TileCell
                        {
                            Type = TileType.TileBody,
                            Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                        };
                        
                        actions.Add(new Gram.TileAction((matrixX, matrixY, mz+1),
                            CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, mz+1]), CopyTileCell(newerCell)));
                        
                        GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newerCell;
                    }

                    if (spec3 != -1 && mz == 2)
                    {
                        GLOBALS.Level.GeoMatrix[matrixY, matrixX, 2].Geo = spec3;
                        
                        var newerCell = new TileCell
                        {
                            Type = TileType.TileBody,
                            Data = new TileBody(mx + 1, my + 1, 3) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                        };
                        
                        actions.Add(new Gram.TileAction((matrixX, matrixY, 2),
                            CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, 2]), CopyTileCell(newerCell)));
                        
                        GLOBALS.Level.TileMatrix[matrixY, matrixX, 2] = newerCell;
                    }
                }
            }
        }

        return actions;
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithoutGeo(
        in TileDefinition init,
        (int x, int y, int z) matrixPosition
    )
    {
        var (mx, my, mz) = matrixPosition;
        var (width, height) = init.Size;
        var specs = init.Specs;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(new(mx, my), head);
        
        List<Gram.ISingleMatrixAction<TileCell>> actions = [];
        
        // Remove pre-existing tile in the way
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;
                
                if (
                    !(matrixX >= 0 &&
                      matrixX < GLOBALS.Level.GeoMatrix.GetLength(1) &&
                      matrixY >= 0 &&
                      matrixY < GLOBALS.Level.GeoMatrix.GetLength(0))
                ) continue;
                
                var specsIndex = x*height + y;

                var spec = specs[y, x, 0];
                var spec2 = specs[y, x, 1];
                var spec3 = specs[y, x, 2];

                if (spec != -1)
                {
                    if (GLOBALS.Level.TileMatrix[matrixY, matrixX, mz].Data is TileHead)
                        actions.AddRange(RemoveTile(matrixX, matrixY, mz));
                }

                if (spec2 != -1 && mz != 2)
                {
                    if (GLOBALS.Level.TileMatrix[matrixY, matrixX, mz+1].Data is TileHead)
                        actions.AddRange(RemoveTile(matrixX, matrixY, mz+1));
                }
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var matrixX = x + (int)start.X;
                var matrixY = y + (int)start.Y;

                // This function depends on the rest of the program to guarantee that all level matrices have the same x and y dimensions
                if (
                    matrixX >= 0 &&
                    matrixX < GLOBALS.Level.GeoMatrix.GetLength(1) &&
                    matrixY >= 0 &&
                    matrixY < GLOBALS.Level.GeoMatrix.GetLength(0)
                )
                {
                    var specsIndex = x*height + y;

                    var spec = specs[y, x, 0];
                    var spec2 = specs[y, x, 1];
                    var spec3 = specs[y, x, 2];
                    
                    // If it's the tile head
                    if (x == (int)head.X && y == (int)head.Y)
                    {
                        // Place the head of the tile at matrixPosition
                        var newHead = new TileCell
                        {
                            Type = TileType.TileHead,
                            Data = new TileHead(init)
                        };
                        
                        actions.Add(new Gram.TileAction(matrixPosition, CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, mz]), CopyTileCell(newHead)));
                        
                        GLOBALS.Level.TileMatrix[my, mx, mz] = newHead;
                    }
                    
                    if (spec != -1)
                    {
                        // If it's the tile head
                        if (!(x == (int)head.X && y == (int)head.Y))
                        {
                            var newCell = new TileCell
                            {
                                Type = TileType.TileBody,
                                Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                            };
                            
                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                            
                            actions.Add(new Gram.TileAction(
                                (matrixX, matrixY, mz),
                                CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, mz]),
                                CopyTileCell(newCell)));
                        }
                    }
                    
                    if (spec2 != -1 && mz != 2)
                    {
                        var newerCell = new TileCell
                        {
                            Type = TileType.TileBody,
                            Data = new TileBody(mx + 1, my + 1, mz + 1) // <- Indices are incremented by 1 because Lingo is 1-based indexed
                        };
                        
                        GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newerCell;
                        actions.Add(new Gram.TileAction(
                            (matrixX, matrixY, mz+1),
                            CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, mz]),
                            CopyTileCell(newerCell)));
                    }
                }
            }
        }
        
        return actions;
    }

    private static TileCell CopyTileCell(TileCell cell) => new()
    {
        Type = cell.Type,
        Data = cell.Data switch
        {
            TileDefault => new TileDefault(),
            TileMaterial m => new TileMaterial(m.Name),
            TileHead h => new TileHead(h.Definition),
            TileBody b => new TileBody(b.HeadPosition.x, b.HeadPosition.y, b.HeadPosition.z),
                                            
            _ => new TileDefault()
        }
    };

    private void UpdatePreviewToolTip()
    {
        if (_hoveredTile is null) return;
        
        _previewTooltipRT.Dispose();
            
        _previewTooltipRT = new RenderTexture2D(
            _hoveredTile.Size.Width*16, 
            _hoveredTile.Size.Height*16
        );
            
        BeginTextureMode(_previewTooltipRT);
        ClearBackground(Color.Black with { A = 0 });
                                    
        Printers.DrawTilePreview(
            _hoveredTile,
            GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black,
            new Vector2(0, 0),
            16
        );
        EndTextureMode();
    }

    private void UpdateTileSpecsPanel()
    {
        if (_currentTile is null) return;
        
        var (width, height) = _currentTile.Size;
        
        const int scale = 20;
        
        _tileSpecsPanelRT.Dispose();

        _tileSpecsPanelRT = new RenderTexture2D(scale*width + 10, scale*height + 10);
        
        BeginTextureMode(_tileSpecsPanelRT);
        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 0 } : Color.White);

        var specs = _currentTile.Specs;
        
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var spec = specs[y, x, 0];
                var spec2 = specs[y, x, 1];
                var spec3 = specs[y, x, 2];

                var specOrigin = new Vector2(scale*x + 5, scale*y + 5);

                if (spec is >= 0 and < 9 and not 8)
                {
                    if (spec3 is >= 0 and < 9 and not 8) Printers.DrawTileSpec(
                        spec3,
                        specOrigin + new Vector2(10, 10),
                        scale,
                        GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 with { A = 255 }
                    );
                    
                    if (spec2 is >= 0 and < 9 and not 8) Printers.DrawTileSpec(
                        spec2,
                        specOrigin + new Vector2(5, 5),
                        scale,
                        GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 with { A = 255 }
                    );
                    
                    Printers.DrawTileSpec(
                        spec,
                        specOrigin,
                        scale,
                        GLOBALS.Settings.GeneralSettings.DarkTheme 
                            ? Color.White 
                            : GLOBALS.Settings.GeometryEditor.LayerColors.Layer1
                    );
                }
            }
        }
        
        EndTextureMode();
    }

    private void UpdateTileTexturePanel()
    {
        if (_currentTile is null) return;

        var color = Color.Gray;

        if (!string.IsNullOrEmpty(_currentCategory.name)) color = _currentCategory.color;

        var (width, height) = _currentTile.Size;
        
        _tileTexturePanelRT.Dispose();
        _tileTexturePanelRT = new RenderTexture2D(
            width*20 + 20, 
            height*20 + 20);
                    
        BeginTextureMode(_tileTexturePanelRT);
        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 0 } : Color.Gray);

        Printers.DrawTileAsPropColored(
            _currentTile, 
            new Vector2(0, 0), 
            new  Vector2(-10, -10), 
            color, 
            0, 
            20
        );

        EndTextureMode();
    }

    public void OnPageUpdated(int previous, int @next) {
        _shouldRedrawLevel = true;
    }

    public override void Draw()
    {
        if (_autoTiler is null)
        {
            _autoTiler = new AutoTiler(
                GLOBALS.Settings.TileEditor.AutoTilerPathPacks, 
                GLOBALS.Settings.TileEditor.AutoTilerRectPacks
            );
        }
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var teWidth = GetScreenWidth();
        var teHeight = GetScreenHeight();

        var tileMouseWorld = GetScreenToWorld2D(GetMousePosition(), _camera);
        var tileMouse = GetMousePosition();
        
        var tilePanelRect = new Rectangle(teWidth - _tilePanelWidth, 0, _tilePanelWidth, teHeight);
        var panelMenuHeight = tilePanelRect.Height - 270;
        var specsRect = new Rectangle(teWidth - 200, teHeight - 200, 200, 200);
        
        var layer3Rect = new Rectangle(10, teHeight - 80, 40, 40);
        var layer2Rect = new Rectangle(20, teHeight - 90, 40, 40);
        var layer1Rect = new Rectangle(30, teHeight - 100, 40, 40);
        
        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 
            ? -1 
            : tileMouseWorld.Y > GLOBALS.Level.Height *GLOBALS.Scale 
                ? GLOBALS.Level.Height*GLOBALS.Scale 
                : (int)tileMouseWorld.Y / GLOBALS.Scale;
        
        var tileMatrixX = tileMouseWorld.X < 0 
            ? -1 
            : tileMouseWorld.X > GLOBALS.Level.Width*GLOBALS.Scale 
                ? GLOBALS.Level.Width*GLOBALS.Scale 
                : (int)tileMouseWorld.X / GLOBALS.Scale;

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        var canDrawTile =
                          !_isSettingsWinHovered && 
                          !_isSettingsWinDragged && 
                          !_isSpecsWinHovered &&
                          !_isSpecsWinDragged &&
                          !_isTilesWinHovered &&
                          !_isTilesWinDragged &&
                          !_isShortcutsWinHovered &&
                          !_isShortcutsWinDragged &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));
        
        var categoriesPageSize = (int)panelMenuHeight / 26;

        var currentMaterialInit = GLOBALS.Materials[_materialCategoryIndex][_materialIndex];

        var isTileLegal = Utils.IsTileLegal(_currentTile, new Vector2(tileMatrixX, tileMatrixY));

        #region TileEditorShortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        // Handle wheel event
        
        // handle zoom
        
        if (canDrawTile || _clickTracker)
        {
            // handle mouse drag
            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
            {
                _clickTracker = true;
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsMouseButtonReleased(_shortcuts.DragLevel.Button)) _clickTracker = false;
       
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                if (IsKeyDown(_shortcuts.ResizeMaterialBrush.Key))
                {
                    _materialBrushRadius += tileWheel > 0 ? 1 : -1;

                    if (_materialBrushRadius < 0) _materialBrushRadius = 0;
                }
                else
                {
                    var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                    _camera.Offset = GetMousePosition();
                    _camera.Target = mouseWorldPosition;
                    _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
                    if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
                }
            }
        }
        
        
        // Skip the rest of the shortcuts when searching
        if (_isSearching) goto skipShortcuts;
        
        // Undo

        if (_shortcuts.Undo.Check(ctrl, shift, alt)) {
            Undo();
            _shouldRedrawLevel = true;
        }
        
        // Redo
        
        if (_shortcuts.Redo.Check(ctrl, shift, alt)) {
            Redo();
            _shouldRedrawLevel = true;
        }
        
        //
        
        // Copy Shortcuts

        if (_shortcuts.CopyTiles.Check(ctrl, shift, alt)) _placeMode = _placeMode == 1 ? 0 : 1;
        
        if (_shortcuts.PasteTilesWithGeo.Check(ctrl, shift, alt)) _placeMode = _placeMode == 3 ? 0 : 3;
        if (_shortcuts.PasteTilesWithoutGeo.Check(ctrl, shift, alt)) _placeMode = _placeMode == 4 ? 0 : 4;
        
        //
        
        if (_shortcuts.PickupItem.Check(ctrl, shift, alt) && canDrawTile && inMatrixBounds)
        {
            switch (GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer].Data)
            {
                case TileMaterial:
                    var result = Utils.PickupMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer);

                    if (result is not null)
                    {
                        _materialCategoryIndex = result!.Value.category;
                        _materialIndex = result!.Value.index;
                    }

                    _materialTileSwitch = false;
                    break;
                
                case TileHead:
                case TileBody:
                    _currentTile = Utils.PickupTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                    (_tileCategoryIndex, _tileIndex) = Utils.GetTileIndex(_currentTile);
                    _materialTileSwitch = _currentTile is not null && (_tileCategoryIndex, _tileIndex) is not (-1, -1);
                    break;
            }
        }

        
        // handle mouse drag and brush resize

        if (canDrawTile || _clickTracker)
        {
            // handle mouse drag
            if (_shortcuts.AltDragLevel.Check(ctrl, shift, alt, true))
            {
                _clickTracker = true;
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsKeyReleased(_shortcuts.AltDragLevel.Key)) _clickTracker = false;
        }

        // handle placing tiles

        if (canDrawTile)
        {
            switch (_placeMode)
            {
                case 0: // None
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt, true) && inMatrixBounds && ( !_materialTileSwitch || _currentTile is not null))
                    {
                        // _clickTracker = true;
                        if (_materialTileSwitch)
                        {
                            if (_shortcuts.ForcePlaceTileWithGeo.Check(ctrl, shift, alt, true))
                            {
                                if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                                {
                                    var actions = ForcePlaceTileWithGeo(
                                        _currentTile!, 
                                        (tileMatrixX, tileMatrixY, GLOBALS.Layer)
                                    );
                                    
                                    foreach (var action in actions) _tempActions.Add(action);
                                }
                            } 
                            else if (_shortcuts.ForcePlaceTileWithoutGeo.Check(ctrl, shift, alt, true))
                            {
                                if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                                {
                                    var actions = ForcePlaceTileWithoutGeo(
                                        _currentTile!, 
                                        (tileMatrixX, tileMatrixY, GLOBALS.Layer)
                                    );
                                    
                                    foreach (var action in actions) _tempActions.Add(action);
                                }
                            }
                            else
                            {
                                if (isTileLegal)
                                {
                                    if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                                    {
                                        var actions = ForcePlaceTileWithGeo(
                                            _currentTile!,
                                            (tileMatrixX, tileMatrixY, GLOBALS.Layer)
                                        );
                                        
                                        foreach (var action in actions) _tempActions.Add(action);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                            {
                                var actions = PlaceMaterial(currentMaterialInit, (tileMatrixX, tileMatrixY, GLOBALS.Layer), _materialBrushRadius);

                                foreach (var action in actions) _tempActions.Add(action);
                            }
                        }

                        _prevPosX = tileMatrixX;
                        _prevPosY = tileMatrixY;
                        
                        _drawClickTracker = true;
                        _shouldRedrawLevel = true;
                    }
                    if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) && _drawClickTracker)
                    {
                        GLOBALS.Gram.Proceed(new Gram.GroupAction<TileCell>([.._tempActions]));
                        _tempActions.Clear();
                        
                        _prevPosX = -1;
                        _prevPosY = -1;
                        
                        _drawClickTracker = false;
                        _shouldRedrawLevel = true;
                    }
                }
                    break;

                case 1: // Copy tile with geo
                {
                    if ((_shortcuts.Draw.Check(ctrl, shift, alt, true) ||
                         _shortcuts.AltDraw.Check(ctrl, shift, alt, true)) && !_copyClickTracker)
                    {
                        _copyClickTracker = true;
                        
                        _copyFirstX = tileMatrixX;
                        _copyFirstY = tileMatrixY;

                        _copyRectangle = new Rectangle(_copyFirstX, _copyFirstY, 1, 1);
                    }

                    if (_copyClickTracker)
                    {
                        if (tileMatrixX != _copySecondX || tileMatrixY != _copySecondY)
                        {
                            _copySecondX = tileMatrixX;
                            _copySecondY = tileMatrixY;
                            
                            _copyRectangle = Utils.RecFromTwoVecs(
                                new Vector2(_copyFirstX, _copyFirstY), 
                                new Vector2(_copySecondX, _copySecondY)
                            );

                            _copyRectangle.Width += 1;
                            _copyRectangle.Height += 1;
                        }
                        
                        if (IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key))
                        {
                            _copyClickTracker = false;
                            
                            // copy tiles

                            var beginX = (int) _copyRectangle.X;
                            var beginY = (int) _copyRectangle.Y;

                            var width = (int)_copyRectangle.Width;
                            var height = (int)_copyRectangle.Height;
                            
                            _copyBuffer = new TileCell[height, width, 2];
                            _copyGeoBuffer = new RunCell[height, width, 2];

                            for (var x = 0; x < width; x++)
                            {
                                for (var y = 0; y < height; y++)
                                {
                                    var mx = beginX + x;
                                    var my = beginY + y;
                                    
                                    if (mx < 0 || mx >= GLOBALS.Level.Width || my < 0 || my >= GLOBALS.Level.Height) continue;

                                    _copyBuffer[y, x, 0] = CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer]);
                                    _copyGeoBuffer[y, x, 0] =
                                        Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer]);
                                    
                                    if (GLOBALS.Layer != 2)
                                    {
                                        _copyBuffer[y, x, 1] = CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1]);
                                        _copyGeoBuffer[y, x, 1] = Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer + 1]);
                                    }
                                }
                            }

                            _prevCopiedRectangle = _copyRectangle;
                            
                            _copyRectangle.X = -1;
                            _copyRectangle.Y = -1;
                            _copyRectangle.Width = 0;
                            _copyRectangle.Height = 0;

                            _placeMode = 0;
                        }
                    }

                }
                    break;

                case 3: // Paste tile with geo
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt) || _shortcuts.AltDraw.Check(ctrl, shift, alt))
                    {
                        List<Gram.ISingleAction<(TileCell, RunCell)>> actions = [];
                        
                        var difX = (int) _prevCopiedRectangle.X - tileMatrixX;
                        var difY = (int) _prevCopiedRectangle.Y - tileMatrixY;
                        
                        for (var x = 0; x < _copyBuffer.GetLength(1); x++)
                        {
                            for (var y = 0; y < _copyBuffer.GetLength(0); y++)
                            {
                                var mx = x + tileMatrixX;
                                var my = y + tileMatrixY;

                                if (mx < 0 || my < 0 || mx >= GLOBALS.Level.Width || my >= GLOBALS.Level.Height) continue;

                                var l1Copy = CopyTileCell(_copyBuffer[y, x, 0]);

                                if (l1Copy.Data is TileBody b)
                                {
                                    var pos = b.HeadPosition;

                                    l1Copy.Data = new TileBody(pos.x - difX, pos.y - difY, GLOBALS.Layer);
                                }
                                
                                actions.Add(new Gram.TileGeoAction(
                                    (mx, my, GLOBALS.Layer), 
                                    (CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer]), Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer])), 
                                    (CopyTileCell(l1Copy), Utils.CopyGeoCell(_copyGeoBuffer[y, x, 0]))));

                                GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer] = l1Copy;
                                GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer] =
                                    Utils.CopyGeoCell(_copyGeoBuffer[y, x, 0]);

                                if (_deepTileCopy && GLOBALS.Layer != 2)
                                {
                                    var l2Copy = CopyTileCell(_copyBuffer[y, x, 1]);
                                    
                                    if (l2Copy.Data is TileBody b2)
                                    {
                                        var pos = b2.HeadPosition;

                                        l2Copy.Data = new TileBody(pos.x - difX, pos.y - difY, GLOBALS.Layer);
                                    }
                                    
                                    actions.Add(new Gram.TileGeoAction(
                                        (mx, my, GLOBALS.Layer + 1), 
                                        (CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1]), Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer + 1])), 
                                        (CopyTileCell(l2Copy), Utils.CopyGeoCell(_copyGeoBuffer[y, x, 1]))));
                                    
                                    GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1] = l2Copy;
                                    GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer + 1] = Utils.CopyGeoCell(_copyGeoBuffer[y, x, 1]);
                                }
                            }
                        }
                        
                        GLOBALS.Gram.Proceed(new Gram.GroupAction<(TileCell, RunCell)>(actions));
                        _shouldRedrawLevel = true;
                    }
                }
                    break;

                case 4: // Paste tile without geo
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt) || _shortcuts.AltDraw.Check(ctrl, shift, alt))
                    {
                        List<Gram.ISingleAction<TileCell>> actions = [];
                        
                        var difX = (int) _prevCopiedRectangle.X - tileMatrixX;
                        var difY = (int) _prevCopiedRectangle.Y - tileMatrixY;
                        
                        for (var x = 0; x < _copyBuffer.GetLength(1); x++)
                        {
                            for (var y = 0; y < _copyBuffer.GetLength(0); y++)
                            {
                                var mx = x + tileMatrixX;
                                var my = y + tileMatrixY;

                                if (mx < 0 || my < 0 || mx >= GLOBALS.Level.Width || my >= GLOBALS.Level.Height) continue;

                                var l1Copy = CopyTileCell(_copyBuffer[y, x, 0]);

                                if (l1Copy.Data is TileBody b)
                                {
                                    var pos = b.HeadPosition;

                                    l1Copy.Data = new TileBody(pos.x - difX, pos.y - difY, GLOBALS.Layer + 1);
                                }
                                
                                actions.Add(new Gram.TileAction(
                                    (mx, my, GLOBALS.Layer), 
                                    CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer]),
                                    CopyTileCell(l1Copy))
                                );

                                GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer] = l1Copy;

                                if (_deepTileCopy && GLOBALS.Layer != 2)
                                {
                                    var l2Copy = CopyTileCell(_copyBuffer[y, x, 1]);
                                    
                                    if (l2Copy.Data is TileBody b2)
                                    {
                                        var pos = b2.HeadPosition;

                                        l2Copy.Data = new TileBody(pos.x - difX, pos.y - difY, GLOBALS.Layer + 1);
                                    }
                                    
                                    actions.Add(new Gram.TileAction(
                                        (mx, my, GLOBALS.Layer + 1), 
                                        CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1]),
                                        CopyTileCell(l2Copy))
                                    );
                                    
                                    GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1] = l2Copy;
                                }
                            }
                        }
                        
                        GLOBALS.Gram.Proceed(new Gram.GroupAction<TileCell>(actions));
                        _shouldRedrawLevel = true;
                    }
                }
                    break;

                case 5: // Auto-Tiling
                {
                    switch (_autoTilerMethod) {
                        case 0: // Micro
                        {
                            if (!_autoTilingCancled && (_shortcuts.Draw.Check(ctrl, shift, alt, true) ||
                                                _shortcuts.AltDraw.Check(ctrl, shift, alt, true)))
                            {
                                var lastCoords = _autoTilerPath.Last?.Value;

                                if (lastCoords is not null && lastCoords.Value.X == tileMatrixX && lastCoords.Value.Y == tileMatrixY) break;

                                _autoTilerPath.AddLast(new Data.Coords(tileMatrixX, tileMatrixY, GLOBALS.Layer));
                            }

                            // if (IsMouseButtonPressed(MouseButton.Right))
                            // {
                            //     _autoTilerPath.Clear();
                            //     _autoTilingCancled = true;
                            // }

                            if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw)))
                            {
                                _autoTilingCancled = false;

                                if (_autoTilerPath.Count > 0)
                                {
                                    ResolveAutoTiler();
                                    _autoTilerPath.Clear();
                                    _shouldRedrawLevel = true;
                                }
                            }
                        }
                        break;

                        case 1: // Macro
                        {
                            if (_shortcuts.CycleAutoTilerMacroAxisPriority.Check(ctrl, shift, alt)) 
                                _autoTilerMacroYAxisFirst = !_autoTilerMacroYAxisFirst;

                            if (IsMouseButtonPressed(MouseButton.Left)) {
                                _autoTilerMacroFirstClick = !_autoTilerMacroFirstClick;
                                if (_autoTilerMacroFirstClick) _autoTilerFirstClickPos = new Data.Coords(tileMatrixX, tileMatrixY);
                                else {
                                    ResolveAutoTiler();
                                    _autoTilerPath.Clear();
                                    
                                    _shouldRedrawLevel = true;
                                }

                            }


                            if (_autoTilerMacroFirstClick) {
                                _autoTilerPath.Clear();
                                var points = Utils.GetTrianglePathPoints(_autoTilerFirstClickPos, new Data.Coords(tileMatrixX, tileMatrixY), _autoTilerMacroYAxisFirst);
                            
                                foreach (var p in points) {
                                    _autoTilerPath.AddLast(p);
                                }

                                _shouldRedrawLevel = true;
                            }
                        }
                        break;
                    }
                }
                    break;
            
                case 6: // Rect
                {

                }
                break;
            }
        }
        
        // handle removing items
        if (canDrawTile || _clickTracker)
        {
            if (_shortcuts.Erase.Check(ctrl, shift, alt, true) && canDrawTile && inMatrixBounds)
            {
                if (_materialBrushRadius == 0) {
                    var cell = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];

                    switch (cell.Data)
                    {
                        case TileHead:
                        case TileBody:
                            if (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY) {
                                var actions = RemoveTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                                        
                                foreach (var action in actions) _tempActions.Add(action);
                            }
                            break;

                        case TileMaterial:
                            if (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY) {
                                var actions = RemoveMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer, _materialBrushRadius);
                                        
                                foreach (var action in actions) _tempActions.Add(action);
                            }
                            break;
                    }
                } else {
                    for (var lx = -_materialBrushRadius; lx < _materialBrushRadius+1; lx++)
                    {
                        var matrixX = tileMatrixX + lx;
                        
                        if (matrixX < 0 || matrixX >= GLOBALS.Level.Width) continue;
                        
                        for (var ly = -_materialBrushRadius; ly < _materialBrushRadius+1; ly++)
                        {
                            var matrixY = tileMatrixY + ly;
                
                            if (matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                            var cell = GLOBALS.Level.TileMatrix[matrixY, matrixX, GLOBALS.Layer];

                            switch (cell.Data)
                            {
                                case TileHead:
                                case TileBody:
                                    if (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY) {
                                        var actions = RemoveTile(matrixX, matrixY, GLOBALS.Layer);
                                                
                                        foreach (var action in actions) _tempActions.Add(action);
                                    }
                                    break;

                                case TileMaterial:
                                    if (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY) {
                                        var actions = RemoveMaterial(matrixX, matrixY, GLOBALS.Layer, 0);
                                                
                                        foreach (var action in actions) _tempActions.Add(action);
                                    }
                                    break;
                            }
                        }
                    }
                }
                            
                _prevPosX = tileMatrixX;
                _prevPosY = tileMatrixY;
                        
                _eraseClickTracker = true;
                _shouldRedrawLevel = true;
            }
        }
        if (IsMouseButtonReleased(_shortcuts.Erase.Button) && _eraseClickTracker)
        {
            GLOBALS.Gram.Proceed(new Gram.GroupAction<TileCell>([.._tempActions]));
            _tempActions.Clear();
                        
            _prevPosX = -1;
            _prevPosY = -1;
                        
            _eraseClickTracker = false;
        }

        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;
            _shouldRedrawLevel = true;
        }

        if (IsMouseButtonReleased(MouseButton.Left))
        {
            _clickTracker = false;
        }

        // change tile category

        if (_shortcuts.MoveToNextCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToNextTileCategory();
            else ToNextMaterialCategory();

            if (_tileSpecDisplayMode) 
                UpdateTileTexturePanel();
            else 
                UpdateTileSpecsPanel();
        }
        else if (_shortcuts.MoveToPreviousCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToPreviousCategory();
            else ToPreviousMaterialCategory();
            
            if (_tileSpecDisplayMode) 
                UpdateTileTexturePanel();
            else 
                UpdateTileSpecsPanel();
        }

        if (_shortcuts.MoveDown.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToNextTileCategory();
                }
                else
                {
                    if (_tileMatches is [])
                    {
                        _tileIndex = ++_tileIndex;

                        if (GLOBALS.Settings.GeneralSettings.CycleMenus)
                            Utils.Cycle(ref _tileIndex, 0, _currentCategoryTiles.Length - 1);                     
                        else 
                            Utils.Restrict(ref _tileIndex, 0, _currentCategoryTiles.Length - 1);
                    }
                    // When searching
                    // Why did I leave this empty?
                    else
                    {
                        
                    }
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToNextMaterialCategory();
                }
                else
                {
                    _materialIndex = ++_materialIndex % GLOBALS.Materials[_materialCategoryIndex].Length;

                    if (
                        _materialIndex % (categoriesPageSize + _materialScrollIndex) == categoriesPageSize + _materialScrollIndex - 1 &&
                        _materialIndex != GLOBALS.Materials[_materialCategoryIndex].Length - 1) _materialScrollIndex++;

                    if (_materialIndex == 0) _materialScrollIndex = 0;
                }
            }
            
            if (_tileSpecDisplayMode) 
                UpdateTileTexturePanel();
            else 
                UpdateTileSpecsPanel();
        }

        if (_shortcuts.MoveUp.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousCategory();
                }
                else
                {
                    _tileIndex--;
                    
                    if (GLOBALS.Settings.GeneralSettings.CycleMenus)
                            Utils.Cycle(ref _tileIndex, 0, _currentCategoryTiles.Length - 1);                     
                        else 
                            Utils.Restrict(ref _tileIndex, 0, _currentCategoryTiles.Length - 1);
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousMaterialCategory();
                }
                else
                {
                    _materialIndex--;
                    if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _materialIndex, 0, GLOBALS.Materials[_materialCategoryIndex].Length - 1);
                    else Utils.Restrict(ref _materialIndex, 0, GLOBALS.Materials[_materialCategoryIndex].Length - 1);
                }
            }
            
            if (_tileSpecDisplayMode) 
                UpdateTileTexturePanel();
            else 
                UpdateTileSpecsPanel();
        }

        if (_shortcuts.TileMaterialSwitch.Check(ctrl, shift, alt))
        {
            _materialTileSwitch = !_materialTileSwitch;
            
            if (_tileSpecDisplayMode) 
                UpdateTileTexturePanel();
            else 
                UpdateTileSpecsPanel();
        }

        if (_shortcuts.HoveredItemInfo.Check(ctrl, shift, alt)) GLOBALS.Settings.TileEditor.HoveredTileInfo = !GLOBALS.Settings.TileEditor.HoveredTileInfo;
        
        if (_shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt))
        {
            _showLayer1Tiles = !_showLayer1Tiles;
            _shouldRedrawLevel = true;
        }
        if (_shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt))
        {
            _showLayer2Tiles = !_showLayer2Tiles;
            _shouldRedrawLevel = true;
        }
        if (_shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt))
        {
            _showLayer3Tiles = !_showLayer3Tiles;
            _shouldRedrawLevel = true;
        }
        if (_shortcuts.ToggleLayer1.Check(ctrl, shift, alt))
        {
            _showTileLayer1 = !_showTileLayer1;
            _shouldRedrawLevel = true;
        }
        if (_shortcuts.ToggleLayer2.Check(ctrl, shift, alt))
        {
            _showTileLayer2 = !_showTileLayer2;
            _shouldRedrawLevel = true;
        }
        if (_shortcuts.ToggleLayer3.Check(ctrl, shift, alt))
        {
            _showTileLayer3 = !_showTileLayer3;
            _shouldRedrawLevel = true;
        }

        if (_shortcuts.TogglePathsView.Check(ctrl, shift, alt))
        {
            _highlightPaths = !_highlightPaths;
            _shouldRedrawLevel = true;
        }

        #endregion
        
        skipShortcuts:

        BeginDrawing();

        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? Color.Black 
            : new Color(170, 170, 170, 255));
        
        if (_shouldRedrawLevel)
        {
            Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams
            {
                DarkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme,
                CurrentLayer = GLOBALS.Layer,
                PropsLayer1 = false,
                PropsLayer2 = false,
                PropsLayer3 = false,
                Water = false,
                TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                Palette = GLOBALS.SelectedPalette,
                Grid = false
            });
            _shouldRedrawLevel = false;
        }

        BeginMode2D(_camera);
        {
            #region Matrix
            
            DrawRectangleLinesEx(new Rectangle(-3, -3, GLOBALS.Level.Width * 20 + 6, GLOBALS.Level.Height * 20 + 6), 3, Color.White);
            
            BeginShaderMode(GLOBALS.Shaders.VFlip);
            SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
            DrawTexture(GLOBALS.Textures.GeneralLevel.Texture, 0, 0, Color.White);
            EndShaderMode();

            // cameras

            if (GLOBALS.Settings.TileEditor.ShowCameras)
            {
                foreach (var cam in GLOBALS.Level.Cameras)
                {
                    DrawRectangleLinesEx(
                        new(cam.Coords.X, cam.Coords.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                        4f,
                        Color.Pink
                    );
                }
            }
            
            if (GLOBALS.Settings.TileEditor.Grid) Printers.DrawGrid(GLOBALS.Scale);
            
            //

            if (_highlightPaths)
            {
                DrawRectangle(
                    0,
                    0,
                    GLOBALS.Level.Width * GLOBALS.Scale,
                    GLOBALS.Level.Height * GLOBALS.Scale,
                    Color.Black with { A = 190 });
            }

            // Draw geo features
            Printers.DrawGeoLayer(0, GLOBALS.Scale, false, Color.White, false, GLOBALS.GeoPathsFilter);

            if (GLOBALS.Settings.GeneralSettings.IndexHint)
            {
                Printers.DrawLevelIndexHintsHollow(
                    tileMatrixX, tileMatrixY, 
                    2, 
                    _materialBrushRadius, 
                    Color.White with { A = 100 }
                );
            }
            
            #endregion

            if (_placeMode == 5)
            {
                foreach (var node in _autoTilerPath)
                {
                    var scaledNode = node * 20;
                 
                    DrawCircle(scaledNode.X + 10, scaledNode.Y + 10, 5, Color.White);
                    DrawCircle(scaledNode.X + 10, scaledNode.Y + 10, 3, Color.Green);
                }
            }

            if (inMatrixBounds)
            {
                switch (_placeMode)
                {
                    case 0: // none
                    {
                        var hoverRect = GetSingleTileRect(tileMatrixX, tileMatrixY, GLOBALS.Layer, GLOBALS.Scale);
                        
                        if (hoverRect is not { Width: 20, Height: 20 } || !_materialTileSwitch) 
                            DrawRectangleLinesEx(
                                hoverRect,
                                2f,
                                Color.White
                            );
                        
                        if (_materialTileSwitch)
                        {
                            Color color = isTileLegal ? _currentCategory.color : Color.Red;
                            Printers.DrawTilePreview(_currentTile, color, (tileMatrixX, tileMatrixY), GLOBALS.Scale);

                            EndShaderMode();
                        }
                        else
                        {
                            if (GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer].Data is TileDefault
                                    or TileMaterial)
                            {
                                DrawRectangleLinesEx(
                                    new Rectangle(
                                        (tileMatrixX - _materialBrushRadius) * GLOBALS.Scale, 
                                        (tileMatrixY - _materialBrushRadius) * GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale),
                                    2f,
                                    Color.White
                                );
                            }
                        }
                    }
                        break;

                    case 1: // copy with geo
                    {
                        if (_copyRectangle is { Width: > 0, Height: > 0 })
                        {
                            var rect = new Rectangle(
                                _copyRectangle.X*GLOBALS.Scale, 
                                _copyRectangle.Y*GLOBALS.Scale, 
                                _copyRectangle.Width*GLOBALS.Scale, 
                                _copyRectangle.Height*GLOBALS.Scale);
                            
                            DrawRectangleLinesEx(
                                rect,
                                2f,
                                Color.Green
                            );
                        }
                        else
                        {
                            DrawRectangleLinesEx(
                                new Rectangle(
                                    tileMatrixX*GLOBALS.Scale, 
                                    tileMatrixY*GLOBALS.Scale, 
                                    GLOBALS.Scale, 
                                    GLOBALS.Scale
                                ),
                                2f,
                                Color.Green
                            );
                        }
                    }
                        break;
                    
                    case 3: // paste with geo
                    {
                        if (_copyBuffer.GetLength(0) > 0 && _copyBuffer.GetLength(1) > 0)
                        {
                            var rect = new Rectangle(
                                tileMatrixX*GLOBALS.Scale, 
                                tileMatrixY*GLOBALS.Scale, 
                                _copyBuffer.GetLength(1)*GLOBALS.Scale,
                                _copyBuffer.GetLength(0)*GLOBALS.Scale
                            );
                            
                            DrawRectangleLinesEx(
                                rect,
                                2f,
                                Color.Yellow
                            );
                        }
                        else
                        {
                            DrawRectangleLinesEx(
                                new Rectangle(
                                    (tileMatrixX - _materialBrushRadius) * GLOBALS.Scale, 
                                    (tileMatrixY - _materialBrushRadius) * GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale),
                                2f,
                                Color.Yellow
                            );
                        }
                    }
                        break;
                    
                    case 4: // paste without geo
                    {
                        if (_copyBuffer.GetLength(0) > 0 && _copyBuffer.GetLength(1) > 0)
                        {
                            var rect = new Rectangle(
                                tileMatrixX*GLOBALS.Scale, 
                                tileMatrixY*GLOBALS.Scale, 
                                _copyBuffer.GetLength(1)*GLOBALS.Scale,
                                _copyBuffer.GetLength(0)*GLOBALS.Scale
                            );
                            
                            DrawRectangleLinesEx(
                                rect,
                                2f,
                                Color.Blue
                            );
                        }
                        else
                        {
                            DrawRectangleLinesEx(
                                new Rectangle(
                                    (tileMatrixX - _materialBrushRadius) * GLOBALS.Scale, 
                                    (tileMatrixY - _materialBrushRadius) * GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale, (_materialBrushRadius*2+1)*GLOBALS.Scale),
                                2f,
                                Color.Blue
                            );
                        }
                    }
                        break;
                }
            }

            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds) DrawText(
                    $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                    tileMatrixX * GLOBALS.Scale + GLOBALS.Scale,
                    tileMatrixY * GLOBALS.Scale - GLOBALS.Scale,
                    15,
                    Color.White
                );
            }
            else
            {
                if (inMatrixBounds)
                {
                    DrawText(
                        $"x: {tileMatrixX}, y: {tileMatrixY}",
                        tileMatrixX * GLOBALS.Scale + GLOBALS.Scale,
                        tileMatrixY * GLOBALS.Scale - GLOBALS.Scale,
                        15,
                        Color.White
                    );
                }
            }
        }
        EndMode2D();

        #region TileEditorUI
        {
            // ImGui

            rlImGui.Begin();

            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Navigation bar
                
            GLOBALS.NavSignal = Printers.ImGui.Nav();
            
            // Menu

            var menuWinOpened = ImGui.Begin("Tiles & Materials", ImGuiWindowFlags.NoFocusOnAppearing);

            var menuPos = ImGui.GetWindowPos();
            var menuWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(tileMouse, new(menuPos.X - 5, menuPos.Y, menuWinSpace.X + 10, menuWinSpace.Y)))
            {
                _isTilesWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isTilesWinDragged = true;
            }
            else
            {
                _isTilesWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isTilesWinDragged) _isTilesWinDragged = false;
            
            if (menuWinOpened)
            {
                if (ImGui.Button(_materialTileSwitch ? "Switch to materials" : "Switch to tiles", ImGui.GetContentRegionAvail() with { Y = 20 }))
                    _materialTileSwitch = !_materialTileSwitch;

                var autoTileSwitch = ImGui.Button(_placeMode == 5 ? "Cancel" : "Use Auto-Tiling", ImGui.GetContentRegionAvail() with { Y = 20 });
            
                if (autoTileSwitch) 
                {
                    _placeMode = _placeMode == 5 ? 0 : 5;
                    _searchText = "";
                }
                
                if (!_materialTileSwitch)
                {
                    if (currentMaterialInit.Item1 == GLOBALS.Level.DefaultMaterial)
                    {
                        ImGui.Text("Current material is the default");
                    }
                    else
                    {
                        if (ImGui.Button("Set to default material"))
                        {
                            GLOBALS.Level.DefaultMaterial = currentMaterialInit.Item1;
                        }
                    }
                }

                var availableSpace = ImGui.GetContentRegionAvail();
                
                var halfWidth = availableSpace.X / 2f - ImGui.GetStyle().ItemSpacing.X / 2f;
                var boxHeight = availableSpace.Y - 30;

                ImGui.SetNextItemWidth(availableSpace.X);

                var textUpdated = ImGui.InputTextWithHint(
                    "##TilesSearch", 
                    "Search..", 
                    ref _searchText, 
                    120, 
                    ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll
                );
                
                _isSearching = ImGui.IsItemActive();
                
                if (textUpdated)
                {
                    _searchTask?.Dispose();
                    _searchTask = Task.Factory.StartNew(Search);
                }
                
                // Reset hovered index
                _hoveredTileCategoryIndex = -1;
                _hoveredTileIndex = -1;
                //

                var isSearching = !string.IsNullOrEmpty(_searchText);

                if (_placeMode == 5) {
                    if (ImGui.BeginListBox("##AutoTilerTemplates", ImGui.GetContentRegionAvail() - new Vector2(0, 70))) {
                        
                        for (var index = 0; index < (_autoTiler?.PathPacks.Count ?? 0); index++) 
                        {
                            var pack = _autoTiler!.PathPacks[index];
                            if (!string.IsNullOrEmpty(_searchText) && !pack.Name.Contains(_searchText)) continue;

                            var packSelected = ImGui.Selectable(pack.Name, _autoTiler.SelectedPathPack == pack);

                            if (packSelected) _autoTiler.SelectedPathPack = pack;
                        }
                        
                        ImGui.EndListBox();
                    }

                    ImGui.SeparatorText("Options");

                    ImGui.Combo("Input Method", ref _autoTilerMethod, "Micro\0Macro");

                    ImGui.Checkbox("Place Geo", ref _autoTilingWithGeo);
                    ImGui.Checkbox("Linear Algorithm", ref _autoTilerLinearAlgorithm);

                } else {

                    if (!isSearching && ImGui.BeginListBox("##Groups", new Vector2(halfWidth, boxHeight)))
                    {
                        if (_materialTileSwitch)
                        {
                            // Thanks to Chromosoze
                            var drawList = ImGui.GetWindowDrawList();
                            var textHeight = ImGui.GetTextLineHeight();
                            //

                            if (string.IsNullOrEmpty(_searchText))
                            {
                                for (var categoryIndex = 0; categoryIndex < (GLOBALS.TileDex?.OrderedCategoryNames.Length ?? 0); categoryIndex++)
                                {
                                    var category = GLOBALS.TileDex?.OrderedCategoryNames[categoryIndex] ?? "";
                                    var color = GLOBALS.TileDex?.GetCategoryColor(category) ?? Color.Black;
                                    Vector4 colorVec = color; 
                                    
                                    // Thanks to Chromosoze
                                    var cursor = ImGui.GetCursorScreenPos();
                                    drawList.AddRectFilled(
                                        p_min: cursor,
                                        p_max: cursor + new Vector2(10f, textHeight),
                                        ImGui.ColorConvertFloat4ToU32((colorVec / 255f))
                                    );

                                    if (_tileCategoryIndex == categoryIndex && 
                                        !string.Equals(category, _currentCategory.name,
                                            StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        _currentCategory = (category, color);
                                        _currentCategoryTiles = GLOBALS.TileDex?.GetTilesOfCategory(category) ?? [];
                                    }
                                    
                                    // TODO: performance issue
                                    var selected = ImGui.Selectable("  "+category, _tileCategoryIndex == categoryIndex);

                                    if (selected)
                                    {
                                        _tileCategoryIndex = categoryIndex;
                                        _tileIndex = 0;

                                        _currentCategory = (category, color);
                                        _currentCategoryTiles = GLOBALS.TileDex?.GetTilesOfCategory(category) ?? [];
                                        
                                        if (_tileSpecDisplayMode) 
                                            UpdateTileTexturePanel();
                                        else 
                                            UpdateTileSpecsPanel();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(_searchText))
                            {
                                for (var category = 0; category < GLOBALS.MaterialCategories.Length; category++)
                                {
                                    var selected = ImGui.Selectable(GLOBALS.MaterialCategories[category], _materialCategoryIndex == category);
                                    if (selected)
                                    {
                                        _materialCategoryIndex = category;
                                        _materialIndex = 0;
                                    }
                                }
                            }
                            // When searching
                            else
                            {
                                for (var c = 0; c < _materialIndices.Length; c++)
                                {
                                    var searchIndex = _materialIndices[c];
                                    
                                    var selected = ImGui.Selectable(GLOBALS.MaterialCategories[searchIndex.category], _materialCategorySearchIndex == c);
                                    if (selected)
                                    {
                                        _materialCategorySearchIndex = c;
                                        if (searchIndex.materials.Length > 0)_materialSearchIndex = 0;
                                    }
                                }
                            }
                        }
                    
                        ImGui.EndListBox();
                    }
                    
                    if (!isSearching) ImGui.SameLine();
                    if (ImGui.BeginListBox("##Tiles", isSearching 
                            ? ImGui.GetContentRegionAvail() with { Y = boxHeight} : new Vector2(halfWidth, boxHeight)))
                    {
                        if (_materialTileSwitch)
                        {
                            // Thanks to Chromosoze
                            var drawList = ImGui.GetWindowDrawList();
                            var textHeight = ImGui.GetTextLineHeight();
                            //
                            
                            if (string.IsNullOrEmpty(_searchText))
                            {
                                for (var tileIndex = 0; tileIndex < _currentCategoryTiles.Length; tileIndex++)
                                {
                                    var currentTile = _currentCategoryTiles[tileIndex];
                                    
                                    var selected = ImGui.Selectable(
                                        currentTile.Name, 
                                        _tileIndex == tileIndex
                                    );

                                    if (_tileIndex == tileIndex && 
                                        _currentTile != currentTile)
                                    {
                                        _currentTile = currentTile;
                                        
                                        if (_tileSpecDisplayMode) 
                                            UpdateTileTexturePanel();
                                        else 
                                            UpdateTileSpecsPanel();
                                    }

                                    var hovered = ImGui.IsItemHovered();

                                    if (hovered)
                                    {
                                        if (_hoveredTile != currentTile)
                                        {
                                            _hoveredTile = currentTile;
                                            if (_tooltip) UpdatePreviewToolTip();
                                        }
                                        
                                        if (_hoveredTileCategoryIndex != _tileCategoryIndex ||
                                            _hoveredTileIndex != tileIndex)
                                        {
                                            _hoveredTileCategoryIndex = _tileCategoryIndex;
                                            _hoveredTileIndex = tileIndex;
                                            
                                            if (_tooltip) UpdatePreviewToolTip();
                                        }
                                        
                                        if (_tooltip && _previewTooltipRT.Raw.Id != 0)
                                        {
                                            _hoveredTileCategoryIndex = _tileCategoryIndex;
                                            _hoveredTileIndex = tileIndex;
                                            
                                            _hoveredTile = currentTile;
                                            
                                            ImGui.BeginTooltip();
                                            rlImGui.ImageRenderTexture(_previewTooltipRT);
                                            ImGui.EndTooltip();
                                        }
                                    }


                                    if (selected)
                                    {
                                        _tileIndex = tileIndex;
                                        _currentTile = currentTile;
                                        
                                        if (_tileSpecDisplayMode) 
                                            UpdateTileTexturePanel();
                                        else 
                                            UpdateTileSpecsPanel();
                                    }
                                }
                            }
                            // When searching
                            else if (_tileMatches.Length > 0)
                            {
                                for (var t = 0; t < _tileMatches.Length; t++)
                                {
                                    var (category, color, tile) = _tileMatches[t];
                                    
                                    Vector4 colorVec = color; 
                                    
                                    // Thanks to Chromosoze
                                    var cursor = ImGui.GetCursorScreenPos();
                                    drawList.AddRectFilled(
                                        p_min: cursor,
                                        p_max: cursor + new Vector2(10f, textHeight),
                                        ImGui.ColorConvertFloat4ToU32((colorVec / 255f))
                                    );
                                    
                                    var selected = ImGui.Selectable(
                                        "  " + tile.Name, 
                                        _tileSearchIndex == t
                                    );

                                    if (_tileSearchIndex == t && tile != _currentTile)
                                    {
                                        _currentTile = tile;
                                        _currentCategory = (category, color);
                                    }

                                    if (selected)
                                    {
                                        _tileSearchIndex = t;

                                        _currentTile = tile;
                                        _currentCategory = (category, color);
                                        
                                        if (_tileSpecDisplayMode) 
                                            UpdateTileTexturePanel();
                                        else 
                                            UpdateTileSpecsPanel();
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Thanks to Chromosoze
                            var drawList = ImGui.GetWindowDrawList();
                            var textHeight = ImGui.GetTextLineHeight();
                            //

                            if (string.IsNullOrEmpty(_searchText))
                            {
                                for (var materialIndex = 0; materialIndex < GLOBALS.Materials[_materialCategoryIndex].Length; materialIndex++)
                                {
                                    ref var material = ref GLOBALS.Materials[_materialCategoryIndex][materialIndex];
                                    
                                    // Thanks to Chromosoze
                                    var cursor = ImGui.GetCursorScreenPos();
                                    drawList.AddRectFilled(
                                        p_min: cursor,
                                        p_max: cursor + new Vector2(10f, textHeight),
                                        ImGui.ColorConvertFloat4ToU32(new Vector4(material.Item2.R / 255f, material.Item2.G / 255f, material.Item2.B / 255, 1f))
                                    );
                                    //
                                    
                                    var selected = ImGui.Selectable(
                                        "  "+material.Item1,
                                        _materialIndex == materialIndex
                                    );

                                    if (selected) _materialIndex = materialIndex;
                                }
                            }
                            // When searching
                            else if (_materialIndices.Length > 0)
                            {
                                for (var m = 0; m < _materialIndices[_materialCategorySearchIndex].materials.Length; m++)
                                {
                                    ref var material =
                                        ref GLOBALS.Materials[_materialIndices[_materialCategorySearchIndex].category][_materialIndices[_materialCategorySearchIndex].materials[m]];
                                    
                                    // Thanks to Chromosoze
                                    var cursor = ImGui.GetCursorScreenPos();
                                    drawList.AddRectFilled(
                                        p_min: cursor,
                                        p_max: cursor + new Vector2(10f, textHeight),
                                        ImGui.ColorConvertFloat4ToU32(new Vector4(material.Item2.R / 255f, material.Item2.G / 255f, material.Item2.B / 255, 1f))
                                    );
                                    //
                                    
                                    var selected = ImGui.Selectable(
                                        "  "+material.Item1,
                                        _materialSearchIndex == m
                                    );

                                    if (selected)
                                    {
                                        _materialSearchIndex = m;

                                        _materialCategoryIndex = _materialIndices[_materialCategorySearchIndex].category;
                                        _materialIndex = _materialIndices[_materialCategorySearchIndex].materials[m];
                                    }
                                }
                            }
                        }
                        ImGui.EndListBox();
                    }
                }
                
                
                
                ImGui.End();
            }
            
            // Tile Specs with ImGui

            var specsWinOpened = ImGui.Begin("Specs");
            
            var specsPos = ImGui.GetWindowPos();
            var specsWinSpace = ImGui.GetWindowSize();
            
            if (CheckCollisionPointRec(GetMousePosition(), new(specsPos.X - 5, specsPos.Y, specsWinSpace.X + 10, specsWinSpace.Y)))
            {
                _isSpecsWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isSpecsWinDragged = true;
            }
            else
            {
                _isSpecsWinHovered = false;
            }
            
            if (IsMouseButtonReleased(MouseButton.Left) && _isSpecsWinDragged) _isSpecsWinDragged = false;
            
            if (specsWinOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();
                
                var displayClicked = ImGui.Button(
                    _tileSpecDisplayMode ? "Texture" : "Geometry", 
                    availableSpace with { Y = 20 }
                );

                if (displayClicked)
                {
                    _tileSpecDisplayMode = !_tileSpecDisplayMode;

                    if (_tileSpecDisplayMode)
                    {
                        UpdateTileTexturePanel();
                    }
                    else
                    {
                        UpdateTileSpecsPanel();
                    }
                }
                
                rlImGui.ImageRenderTextureFit(_tileSpecDisplayMode ? _tileTexturePanelRT : _tileSpecsPanelRT);
                
                // Idk where to put this
                ImGui.End();
            }
            
            // Settings

            var settingsWinOpened = ImGui.Begin("Settings##TilesSettings");
            
            var settingsPos = ImGui.GetWindowPos();
            var settingsWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(tileMouse, new(settingsPos.X - 5, settingsPos.Y-5, settingsWinSpace.X + 10, settingsWinSpace.Y+10)))
            {
                _isSettingsWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isSettingsWinDragged = true;
            }
            else
            {
                _isSettingsWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isSettingsWinDragged) _isSettingsWinDragged = false;
            
            if (settingsWinOpened)
            {
                //

                ImGui.Checkbox("Tooltip", ref _tooltip);
                
                ImGui.Checkbox("Deep Tile Copy", ref _deepTileCopy);

                var hoveredInfo = GLOBALS.Settings.TileEditor.HoveredTileInfo;
                ImGui.Checkbox("Hovered Item Info", ref hoveredInfo);
                if (GLOBALS.Settings.TileEditor.HoveredTileInfo != hoveredInfo) 
                    GLOBALS.Settings.TileEditor.HoveredTileInfo = hoveredInfo;

                var showCameras = GLOBALS.Settings.TileEditor.ShowCameras;
                if (ImGui.Checkbox("Cameras", ref showCameras)) GLOBALS.Settings.TileEditor.ShowCameras = showCameras;

                var grid = GLOBALS.Settings.TileEditor.Grid;
                if (ImGui.Checkbox("Grid", ref grid))
                {
                    GLOBALS.Settings.TileEditor.Grid = grid;
                    _shouldRedrawLevel = true;
                }
                
                ImGui.End();
            }
            
            // Shortcuts window
            
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.TileEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    tileMouse, 
                    shortcutWindowRect with
                    {
                        Y = shortcutWindowRect.Y - 5, Height = shortcutWindowRect.Height + 10,
                        X = shortcutWindowRect.X - 5, Width = shortcutWindowRect.Width + 10
                    }
                );

                if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.Left))
                {
                    _isShortcutsWinDragged = true;
                }
                else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.Left))
                {
                    _isShortcutsWinDragged = false;
                }
            }
            
            rlImGui.End();

            #region LayerIndicator
            
            // layer indicator

            var newLayer = GLOBALS.Layer;

            var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(tileMouse, layer3Rect);

            if (layer3Hovered)
            {
                DrawRectangleRec(layer3Rect, Color.Blue with { A = 100 });

                if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 0;
            }

            DrawRectangleRec(
                layer3Rect,
                GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
            );

            DrawRectangleLines(10, (int)layer3Rect.Y, 40, 40, Color.Gray);

            if (GLOBALS.Layer == 2) DrawText("3", 26, (int)layer3Rect.Y+10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            
            if (GLOBALS.Layer is 1 or 0)
            {
                var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(tileMouse, layer2Rect);

                if (layer2Hovered)
                {
                    DrawRectangleRec(layer2Rect, Color.Blue with { A = 100 });

                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 2;
                }
                
                DrawRectangleRec(
                    layer2Rect,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
                );

                DrawRectangleLines(20, (int)layer2Rect.Y, 40, 40, Color.Gray);

                if (GLOBALS.Layer == 1) DrawText("2", 35, (int)layer2Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            }

            if (GLOBALS.Layer == 0)
            {
                var layer1Hovered = CheckCollisionPointRec(tileMouse, layer1Rect);

                if (layer1Hovered)
                {
                    DrawRectangleRec(layer1Rect, Color.Blue with { A = 100 });
                    if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 1;
                }
                
                DrawRectangleRec(
                    layer1Rect,
                    GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
                );

                DrawRectangleLines(
                    30, (int)layer1Rect.Y, 40, 40, Color.Gray);

                DrawText("1", 48, (int)layer1Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            }

            if (newLayer != GLOBALS.Layer) GLOBALS.Layer = newLayer;
            
            #endregion
            
            // Hovered tile info text

            if (inMatrixBounds && canDrawTile)
            {

                TileCell hoveredTile;

                #if DEBUG
                try
                {
                    hoveredTile = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie,
                        message:
                        $"Failed to fetch hovered tile from {nameof(GLOBALS.Level.TileMatrix)} (LX: {GLOBALS.Level.TileMatrix.GetLength(1)}, LY: {GLOBALS.Level.TileMatrix.GetLength(0)}): x, y, or z ({tileMatrixX}, {tileMatrixY}, {GLOBALS.Layer}) were out of bounds");
                }
                #else
                hoveredTile = GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer];
                #endif

                switch (hoveredTile.Data)
                {
                    case TileDefault:
                    {
                        if (GLOBALS.Font is null)
                        {
                            DrawText(
                                GLOBALS.Level.DefaultMaterial,
                                0,
                                (int)(specsRect.Y + specsRect.Height - 20),
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                GLOBALS.Level.DefaultMaterial,
                                new Vector2(10, specsRect.Y + specsRect.Height - 30),
                                30,
                                1,
                                Color.White
                            );
                        }
                    }
                        break;

                    case TileHead h:
                    {
                        if (GLOBALS.Font is null) {
                            DrawText(
                                h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\"",
                                0,
                                (int)(specsRect.Y + specsRect.Height - 20),
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\"",
                                new Vector2(10, specsRect.Y + specsRect.Height - 30),
                                30,
                                1,
                                Color.White
                            );
                        }
                    }
                        break;

                    case TileBody b:
                    {
                        var (hx, hy, hz) = b.HeadPosition;
                        
                        try
                        {
                            var supposedHead = GLOBALS.Level.TileMatrix[hy-1, hx-1, hz-1];

                            if (GLOBALS.Font is null)
                            {
                                DrawText(
                                    supposedHead.Data is TileHead h
                                        ? h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\""
                                        : "Stray Tile Fragment",
                                    0,
                                    (int)(specsRect.Y + specsRect.Height - 20),
                                    20,
                                    Color.White
                                );
                                
                            }
                            else
                            {
                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    supposedHead.Data is TileHead h
                                        ? h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\""
                                        : "Stray Tile Fragment",
                                    new Vector2(10,
                                        specsRect.Y + specsRect.Height - 30),
                                    30,
                                    1,
                                    Color.White
                                );
                            }
                        
                        }
                        catch (IndexOutOfRangeException)
                        {
                            if (GLOBALS.Font is null) {
                                DrawText("Stray Tile Fragment",
                                    0,
                                    (int) (specsRect.Y + specsRect.Height - 20),
                                    20,
                                    Color.White
                                );
                            }
                            else
                            {
                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    "Stray Tile Fragment",
                                    new Vector2(10,
                                        specsRect.Y + specsRect.Height - 30),
                                    30,
                                    1,
                                    Color.White
                                );
                            }
                        }
                    }
                        break;
                    
                    case TileMaterial m:
                    {
                        if (GLOBALS.Font is null) {
                            DrawText(m.Name,
                                0,
                                (int)(specsRect.Y + specsRect.Height - 20),
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                m.Name,
                                new Vector2(10, specsRect.Y + specsRect.Height - 30),
                                30,
                                1,
                                Color.White
                            );
                        }
                    }
                        break;
                }
            }
        }
        #endregion

        EndDrawing();
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}