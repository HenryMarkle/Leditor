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
    private int _materialScrollIndex;

    private bool _highlightPaths;

    private bool _drawClickTracker;
    private bool _eraseClickTracker;
    private bool _copyClickTracker;

    private bool _deepTileCopy = true;
    private bool _copyMaterials = true;

    private int _prevPosX = -1;
    private int _prevPosY = -1;

    private int _copyFirstX = -1;
    private int _copyFirstY = -1;

    private int _copySecondX = -1;
    private int _copySecondY = -1;

    private bool _boxClickTracker;

    private int _boxPlacementFirstX = -1;
    private int _boxPlacementFirstY = -1;
    private int _boxPlacementSecondX = -1;
    private int _boxPlacementSecondY = -1;

    private Rectangle _boxRectangle;

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
    private enum AutoTilerMode { Normal, Copy, PasteWithGeo, PasteWithoutGeo, Path, Box, Rect }

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
    private AutoTilerMode _placeMode;

    private Rectangle _prevCopiedRectangle;
    private Rectangle _copyRectangle;

    private Color[,,] _copyMaterialColorBuffer = new Color[0, 0, 0];
    private TileCell[,,] _copyBuffer = new TileCell[0,0,0];
    private GeoCell[,,] _copyGeoBuffer = new GeoCell[0, 0, 0];

    private (string category, Data.Color color, TileDefinition tile)[] _tileMatches = [];
    private (int category, int[] materials)[] _materialIndices = [];

    private RenderTexture2D _previewTooltipRT = new(0, 0);
    private RenderTexture2D _tileSpecsPanelRT = new(0, 0);
    private RenderTexture2D _tileTexturePanelRT = new(0, 0);

    private readonly Dictionary<string, RL.Managed.Texture2D> _materialPreviews = [];
    
    private bool _shouldRedrawLevel = true;

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

    private bool _isTexturesWinHovered;

    private bool _isNavbarHovered;

    public static void EraseStrayFragments(TileCell[,,] matrix) {
        for (var y = 0; y < matrix.GetLength(0); y++) {
            for (var x = 0; x < matrix.GetLength(1); x++) {
                for (var z = 0; z < 3; z++) {
                    var cell = matrix[y, x, z];

                    if (cell.Type != TileType.TileBody || cell.Data is not TileBody) continue;

                    var (hx, hy, hz) = ((TileBody)cell.Data).HeadPosition;

                    if (hx < 1 || 
                        hx > matrix.GetLength(1) ||
                        hy < 1 ||
                        hy > matrix.GetLength(0) ||
                        hz < 1 || 
                        hz > 3 ||
                        matrix[hy - 1, hx - 1, hz - 1].Data is not TileHead) {
                            matrix[y, x, z] = new TileCell();
                    }
                }
            }
        }
    }

    public static bool IsTileLegal(in TileDefinition? init, Vector2 point)
    {
        if (init is null) return false;
        
        var (width, height) = init.Size;
        var specs = init.Specs;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(point, head);

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
                    var tileCell = GLOBALS.Level.TileMatrix[matrixY, matrixX, GLOBALS.Layer];
                    var geoCell = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer];
                    var specsIndex = (x * height) + y;


                    var spec = specs[y, x, 0];
                    var spec2 = specs[y, x, 1];
                    var spec3 = specs[y, x, 2];

                    var isLegal = spec == -1 ||
                                  (geoCell.Geo == spec && tileCell.Type is TileType.Default or TileType.Material);

                    if (tileCell.Type is TileType.Material && !GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials) isLegal = false;

                    if (GLOBALS.Layer != 2)
                    {
                        var tileCellNextLayer = GLOBALS.Level.TileMatrix[matrixY, matrixX, GLOBALS.Layer + 1];
                        var geoCellNextLayer = GLOBALS.Level.GeoMatrix[matrixY, matrixX, GLOBALS.Layer + 1];

                        isLegal = isLegal && (spec2 == -1 || (geoCellNextLayer.Geo == spec2 &&
                                                     tileCellNextLayer.Type is TileType.Default or TileType.Material));

                        if (tileCellNextLayer.Type is TileType.Material && !GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials) isLegal = false;
                    }

                    if (GLOBALS.Layer == 0)
                    {
                        var tileCellNextLayer = GLOBALS.Level.TileMatrix[matrixY, matrixX, 2];
                        var geoCellNextLayer = GLOBALS.Level.GeoMatrix[matrixY, matrixX, 2];
                        
                        isLegal = isLegal && (spec3 == -1 || (geoCellNextLayer.Geo == spec3 &&
                                                              tileCellNextLayer.Type is TileType.Default or TileType.Material));

                        if (tileCellNextLayer.Type is TileType.Material && !GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials) isLegal = false;
                    }

                    if (!isLegal) return false;
                }
                else return false;
            }
        }

        return true;
    }

    public void ResolveAutoTiler() {
        var resolvedTiles = _autoTilerLinearAlgorithm 
            ? _autoTiler?.ResolvePathLinear(_autoTilerPath) 
            : _autoTiler?.ResolvePath(_autoTilerPath);

        if (resolvedTiles == null) return;

        List<TileGram.ISingleMatrixAction<TileCell>> combinedActions = [];

        foreach (var resolved in resolvedTiles)
        {
            if (!Utils.InBounds(resolved.Coords, GLOBALS.Level.TileMatrix) || resolved.Tile is null) continue;

            var actions = _autoTilingWithGeo 
                ? ForcePlaceTileWithGeo(resolved.Tile, resolved.Coords) 
                : ForcePlaceTileWithoutGeo(resolved.Tile, resolved.Coords);

            combinedActions = [..combinedActions, ..actions];
            
        }    
            
        GLOBALS.Gram.Proceed(new TileGram.GroupAction<TileCell>([..combinedActions]));
    }
    
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        
        _tileSpecsPanelRT.Dispose();
        _tileTexturePanelRT.Dispose();
        _previewTooltipRT.Dispose();

        foreach (var (_, texture) in _materialPreviews) texture.Dispose();
    }

    ~TileEditorPage()
    {
        if (!Disposed) throw new InvalidOperationException("Page not disposed by consumer");
    }

    internal TileEditorPage() {
        // var materialTextureFiles = Directory.GetFiles(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "material previews"))
        //     .Where(f => f.EndsWith(".png"));

        // foreach (var file in materialTextureFiles) _materialPreviews[Path.GetFileNameWithoutExtension(file)] = new(file);
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

    private readonly List<TileGram.ISingleMatrixAction<TileCell>> _tempActions = [];

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
        
        if (GLOBALS.Settings.GeneralSettings.CycleMenus) _tileCategoryIndex %= GLOBALS.TileDex!.OrderedCategoryNames.Length;
        else Utils.Restrict(ref _tileCategoryIndex, 0, GLOBALS.TileDex!.OrderedCategoryNames.Length-1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _tileIndex = 0;
        else Utils.Restrict(ref _tileIndex, 0, _currentCategoryTiles.Length-1);
    }

    private void ToPreviousCategory()
    {
        _tileCategoryIndex--;

        if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _tileCategoryIndex, 0, GLOBALS.TileDex!.OrderedCategoryNames.Length - 1);
        else Utils.Restrict(ref _tileCategoryIndex, 0, GLOBALS.TileDex!.OrderedCategoryNames.Length - 1);
        
        if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _tileIndex = 0;
        else Utils.Restrict(ref _tileIndex, 0, _currentCategoryTiles.Length-1);
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

        void UndoThis(TileGram.IAction action)
        {
            switch (action)
            {
                case TileGram.TileAction tileAction:
                {
                    if (!IsCoordsInBounds(tileAction.Position)) return;

                    var (x, y, z) = tileAction.Position;

                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileAction.Old);

                    if (tileAction.Old.Data is TileMaterial m) 
                        GLOBALS.Level.MaterialColors[y, x, z] = GLOBALS.MaterialColors[m.Name];
                    
                }
                    break;

                case TileGram.TileGeoAction tileGeoAction:
                {
                    if (!IsCoordsInBounds(tileGeoAction.Position)) return;

                    var (x, y, z) = tileGeoAction.Position;
                    
                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileGeoAction.Old.Item1);
                    GLOBALS.Level.GeoMatrix[y, x, z] = Utils.CopyGeoCell(tileGeoAction.Old.Item2);
                }
                    break;
                
                case TileGram.GroupAction<TileCell> groupAction:
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

        void UndoThis(TileGram.IAction action)
        {
            switch (action)
            {
                case TileGram.TileAction tileAction:
                {
                    if (!IsCoordsInBounds(tileAction.Position)) return;

                    var (x, y, z) = tileAction.Position;

                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileAction.New);

                    if (tileAction.Old.Data is TileMaterial m) 
                        GLOBALS.Level.MaterialColors[y, x, z] = GLOBALS.MaterialColors[m.Name];
                    
                }
                    break;

                case TileGram.TileGeoAction tileGeoAction:
                {
                    if (!IsCoordsInBounds(tileGeoAction.Position)) return;
                    
                    var (x, y, z) = tileGeoAction.Position;
                    
                    GLOBALS.Level.TileMatrix[y, x, z] = CopyTileCell(tileGeoAction.New.Item1);
                    GLOBALS.Level.GeoMatrix[y, x, z] = Utils.CopyGeoCell(tileGeoAction.New.Item2);
                }
                    break;
                
                case TileGram.GroupAction<TileCell> groupAction:
                    foreach (var a in groupAction.Actions) UndoThis(a);
                    break;
            }
        }
    }
    
    private static List<TileGram.ISingleMatrixAction<TileCell>> RemoveMaterial(int x, int y, int z, int radius)
    {
        List<TileGram.ISingleMatrixAction<TileCell>> actions = [];
        
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
                
                actions.Add(new TileGram.TileAction((lx, ly, x), cell, newCell));
                
                GLOBALS.Level.TileMatrix[matrixY, matrixX, z] = newCell;
            }
        }
        
        return actions;
    }
    
    private static List<TileGram.ISingleMatrixAction<TileCell>> PlaceMaterial((string name, Color color) material, (int x, int y, int z) position, int radius)
    {
        var (x, y, z) = position;

        List<TileGram.ISingleMatrixAction<TileCell>> actions = [];

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
                if (cell.Type == TileType.Material && !GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials) continue;

                var newCell = new TileCell { Type = TileType.Material, Data = new TileMaterial(material.name) }; // waste of space, ik
                actions.Add(new TileGram.TileAction((matrixX, matrixY, z), cell, newCell));
                
                GLOBALS.Level.TileMatrix[matrixY, matrixX, z] = newCell;
                GLOBALS.Level.MaterialColors[matrixY, matrixX, z] = material.color;
            }
        }
        
        return actions;
    }

    private static List<TileGram.ISingleMatrixAction<TileCell>> RemoveTile(int mx, int my, int mz)
    {
        while (true)
        {
            var cell = GLOBALS.Level.TileMatrix[my, mx, mz];

            List<TileGram.ISingleMatrixAction<TileCell>> actions = [];

            if (cell.Data is TileHead h)
            {
                // tile is undefined
                if (h.Definition is null)
                {
                    var oldCell = GLOBALS.Level.TileMatrix[my, mx, mz];
                    var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                    GLOBALS.Level.TileMatrix[my, mx, mz] = newCell;

                    return [new TileGram.TileAction((mx, my, mz), oldCell, newCell)];
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

                        if (spec != -1 || GLOBALS.Level.TileMatrix[matrixY, matrixX, mz].Data is TileHead)
                        {
                            var oldCell = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz];
                            var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new TileGram.TileAction((matrixX, matrixY, mz), oldCell, newCell));

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                        }

                        if (mz != 2 && spec2 != -1)
                        {
                            var oldCell2 = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1];
                            var newCell2 = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new TileGram.TileAction((matrixX, matrixY, mz + 1), oldCell2, newCell2));

                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newCell2;
                        }

                        if (mz == 0 && spec3 != -1)
                        {
                            var oldCell3 = GLOBALS.Level.TileMatrix[matrixY, matrixX, 2];
                            var newCell3 = new TileCell { Type = TileType.Default, Data = new TileDefault() };

                            actions.Add(new TileGram.TileAction((matrixX, matrixY, 2), oldCell3, newCell3));

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
    
    private static List<TileGram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithGeo(
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
        
        List<TileGram.ISingleMatrixAction<TileCell>> actions = [];
        
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
                        
                        actions.Add(new TileGram.TileAction(matrixPosition, CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, mz]), CopyTileCell(newHead)));
                        
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
                            
                            actions.Add(new TileGram.TileAction((matrixX, matrixY, mz),
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
                        
                        actions.Add(new TileGram.TileAction((matrixX, matrixY, mz+1),
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
                        
                        actions.Add(new TileGram.TileAction((matrixX, matrixY, 2),
                            CopyTileCell(GLOBALS.Level.TileMatrix[matrixY, matrixX, 2]), CopyTileCell(newerCell)));
                        
                        GLOBALS.Level.TileMatrix[matrixY, matrixX, 2] = newerCell;
                    }
                }
            }
        }

        return actions;
    }
    
    private static List<TileGram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithoutGeo(
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
        
        List<TileGram.ISingleMatrixAction<TileCell>> actions = [];
        
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
                        
                        actions.Add(new TileGram.TileAction(matrixPosition, CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, mz]), CopyTileCell(newHead)));
                        
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
                            
                            actions.Add(new TileGram.TileAction(
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
                        actions.Add(new TileGram.TileAction(
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
        

        if (GLOBALS.Settings.TileEditor.UseTexturesInTooltip) {
            var (width, height) = _hoveredTile.Size;
            var buffer = _hoveredTile.BufferTiles * 2;

            var totalWidth = (width + buffer) * 20;
            var totalHeight = (height + buffer) * 20;

            if (totalWidth != _previewTooltipRT.Raw.Texture.Width || totalHeight != _previewTooltipRT.Raw.Texture.Height) {
                _previewTooltipRT.Dispose();
                _previewTooltipRT = new RenderTexture2D(totalWidth, totalHeight);
            }

            
            BeginTextureMode(_previewTooltipRT);
            ClearBackground(Color.White with { A = 0 });
                
            Printers.DrawTileAsPropColored(
                _hoveredTile, 
                new PropQuad(
                    new Vector2(0, 0),
                    new Vector2(totalWidth, 0),
                    new Vector2(totalWidth, totalHeight),
                    new Vector2(0, totalHeight)
                ),
                Color.LightGray
            );
            EndTextureMode();
        } else {
            var totalWidth = _hoveredTile.Size.Width * 16;
            var totalHeight = _hoveredTile.Size.Height * 16;

            if (totalWidth != _previewTooltipRT.Raw.Texture.Width || totalHeight != _previewTooltipRT.Raw.Texture.Height) {
                _previewTooltipRT.Dispose();
                _previewTooltipRT = new RenderTexture2D(totalWidth, totalHeight);
            }

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

                if (spec is >= 0 and < 9 and not 8)
                {
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
        var buffer = _currentTile.BufferTiles * 2;

        var totalWidth = (width + buffer) * 20;
        var totalHeight = (height + buffer) * 20;

        _tileTexturePanelRT.Dispose();
        _tileTexturePanelRT = new RenderTexture2D(
            totalWidth, 
            totalHeight);
                    
        BeginTextureMode(_tileTexturePanelRT);
        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 0 } : Color.Gray);

        Printers.DrawTileAsPropColored(
            _currentTile, 
            new PropQuad(
                new Vector2(0, 0),
                new Vector2(totalWidth, 0),
                new Vector2(totalWidth, totalHeight),
                new Vector2(0, totalHeight)
            ), 
            color
        );

        EndTextureMode();
    }

    public void OnPageUpdated(int previous, int @next) {
        if (@next == 3) {
            _shouldRedrawLevel = true;
            UpdateTileTexturePanel();
            UpdateTileSpecsPanel();
        }
    }

    private Rectangle GetLayerIndicator(int layer) => layer switch {
        0 => GLOBALS.Settings.TileEditor.LayerIndicatorPosition switch { 
            TileEditorSettings.ScreenRelativePosition.TopLeft       => new Rectangle( 40,                        50 + 25,                     40, 40 ), 
            TileEditorSettings.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 70,     50 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 70,     GetScreenHeight() - 90 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomLeft    => new Rectangle( 40,                        GetScreenHeight() - 90 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 50 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 90 - 25, 40, 40 ),

            _ => new Rectangle(40, GetScreenHeight() - 80, 40, 40)
        },
        1 => GLOBALS.Settings.TileEditor.LayerIndicatorPosition switch { 
            TileEditorSettings.ScreenRelativePosition.TopLeft       => new Rectangle( 30,                        40 + 25,                     40, 40 ), 
            TileEditorSettings.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 60,     40 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 60,     GetScreenHeight() - 80 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomLeft    => new Rectangle( 30,                        GetScreenHeight() - 80 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 40 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 80 - 25, 40, 40 ),

            _ => new Rectangle(30, GetScreenHeight() - 70, 40, 40)
        },
        2 => GLOBALS.Settings.TileEditor.LayerIndicatorPosition switch { 
            TileEditorSettings.ScreenRelativePosition.TopLeft       => new Rectangle( 20,                        30 + 25,                     40, 40 ), 
            TileEditorSettings.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 50,     30 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 50,     GetScreenHeight() - 70 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.BottomLeft    => new Rectangle( 20,                        GetScreenHeight() - 70 - 25, 40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 30 + 25,                     40, 40 ),
            TileEditorSettings.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 70 - 25, 40, 40 ),

            _ => new Rectangle(20, GetScreenHeight() - 60, 40, 40)
        },
        
        _ => new Rectangle(0, 0, 40, 40)
    };

    private Vector2 GetLabelPosition(int length, int height) => GLOBALS.Settings.TileEditor.LayerIndicatorPosition switch {
        TileEditorSettings.ScreenRelativePosition.TopLeft           => new Vector2( 20,                                                         20 ), 
        TileEditorSettings.ScreenRelativePosition.TopRight          => new Vector2( GetScreenWidth() - length,                                  20 ),
        TileEditorSettings.ScreenRelativePosition.BottomRight       => new Vector2( GetScreenWidth() - length,     GetScreenHeight() - height - 10 ),
        TileEditorSettings.ScreenRelativePosition.BottomLeft        => new Vector2( 20,                            GetScreenHeight() - height - 10 ),
        TileEditorSettings.ScreenRelativePosition.MiddleTop         => new Vector2((GetScreenWidth() - length)/2f,                              20 ),
        TileEditorSettings.ScreenRelativePosition.MiddleBottom      => new Vector2((GetScreenWidth() - length)/2f, GetScreenHeight() - height - 10 ),

        _ => new Vector2( 20, GetScreenHeight() - height - 10 )
    };

    public override void Draw()
    {
        if (_autoTiler is null)
        {
            _autoTiler = new AutoTiler(
                GLOBALS.Settings.TileEditor.AutoTilerPathPacks ?? [], 
                GLOBALS.Settings.TileEditor.AutoTilerBoxPacks ?? []
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
        
        var layer3Rect = GetLayerIndicator(2);
        var layer2Rect = GetLayerIndicator(1);
        var layer1Rect = GetLayerIndicator(0);
        
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

        var canDrawTile = !_isNavbarHovered &&
                          !_isSettingsWinHovered && 
                          !_isSettingsWinDragged && 
                          !_isSpecsWinHovered &&
                          !_isSpecsWinDragged &&
                          !_isTilesWinHovered &&
                          !_isTilesWinDragged &&
                          !_isShortcutsWinHovered &&
                          !_isShortcutsWinDragged &&
                          !_isTexturesWinHovered &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));
        
        var categoriesPageSize = (int)panelMenuHeight / 26;

        var currentMaterialInit = GLOBALS.Materials[_materialCategoryIndex][_materialIndex];

        var isTileLegal = IsTileLegal(_currentTile, new Vector2(tileMatrixX, tileMatrixY));

        #region TileEditorShortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        if (canDrawTile || _clickTracker)
        {
            // Drag
            if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true))
            {
                _clickTracker = true;
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsMouseButtonReleased(_shortcuts.DragLevel.Button)) _clickTracker = false;


            // Zoom
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
        
        // Resizing Brush With Keyboard

        if (canDrawTile) {
            if (_shortcuts.EnlargeBrush.Check(ctrl, shift, alt)) {
                _materialBrushRadius += 1;
                Utils.Restrict(ref _materialBrushRadius, 0);
            }
            else if (_shortcuts.ShrinkBrush.Check(ctrl, shift, alt)) {
                _materialBrushRadius -= 1;
                Utils.Restrict(ref _materialBrushRadius, 0);
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

        if (_shortcuts.CopyTiles.Check(ctrl, shift, alt)) _placeMode = _placeMode == AutoTilerMode.Copy ? AutoTilerMode.Normal : AutoTilerMode.Copy;
        
        if (_shortcuts.PasteTilesWithGeo.Check(ctrl, shift, alt)) _placeMode = _placeMode == AutoTilerMode.PasteWithGeo ? AutoTilerMode.Normal : AutoTilerMode.PasteWithGeo;
        if (_shortcuts.PasteTilesWithoutGeo.Check(ctrl, shift, alt)) _placeMode = _placeMode == AutoTilerMode.PasteWithoutGeo ? AutoTilerMode.Normal : AutoTilerMode.PasteWithoutGeo;
        
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

            UpdateTileTexturePanel();
            UpdateTileSpecsPanel();
        }

        
        // handle mouse drag

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
                case AutoTilerMode.Normal: // None
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
                                    _shouldRedrawLevel = true;
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
                                    _shouldRedrawLevel = true;
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
                                        _shouldRedrawLevel = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                            {
                                var actions = PlaceMaterial(currentMaterialInit, (tileMatrixX, tileMatrixY, GLOBALS.Layer), _materialBrushRadius);

                                _tempActions.AddRange(actions);
                                _shouldRedrawLevel = true;
                            }
                        }

                        _prevPosX = tileMatrixX;
                        _prevPosY = tileMatrixY;
                        
                        _drawClickTracker = true;
                    }
                    if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) && _drawClickTracker)
                    {
                        GLOBALS.Gram.Proceed(new TileGram.GroupAction<TileCell>([.._tempActions]));
                        _tempActions.Clear();
                        
                        _prevPosX = -1;
                        _prevPosY = -1;
                        
                        _drawClickTracker = false;
                        // _shouldRedrawLevel = true;
                    }
                }
                    break;

                case AutoTilerMode.Copy: // Copy tile with geo
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
                            
                            _copyMaterialColorBuffer = new Color[height, width, 2];
                            _copyBuffer = new TileCell[height, width, 2];
                            _copyGeoBuffer = new GeoCell[height, width, 2];

                            for (var x = 0; x < width; x++)
                            {
                                for (var y = 0; y < height; y++)
                                {
                                    var mx = beginX + x;
                                    var my = beginY + y;
                                    
                                    if (mx < 0 || mx >= GLOBALS.Level.Width || my < 0 || my >= GLOBALS.Level.Height) continue;

                                    _copyMaterialColorBuffer[y, x, 0] = GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer];
                                    _copyBuffer[y, x, 0] = CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer]);
                                    _copyGeoBuffer[y, x, 0] =
                                        Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer]);
                                    
                                    if (GLOBALS.Layer != 2)
                                    {
                                        _copyMaterialColorBuffer[y, x, 1] = GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer + 1];
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

                case AutoTilerMode.PasteWithGeo: // Paste tile with geo
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt) || _shortcuts.AltDraw.Check(ctrl, shift, alt))
                    {
                        List<TileGram.ISingleAction<(TileCell, GeoCell)>> actions = [];
                        
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
                                } else if (_copyMaterials && l1Copy.Data is TileMaterial m) {
                                    GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer] = _copyMaterialColorBuffer[y, x, 0];
                                }
                                
                                actions.Add(new TileGram.TileGeoAction(
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

                                        l2Copy.Data = new TileBody(pos.x - difX, pos.y - difY, GLOBALS.Layer + 1);
                                    } else if (_copyMaterials && l1Copy.Data is TileMaterial m) {
                                        GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer + 1] = _copyMaterialColorBuffer[y, x, 1];
                                    }
                                    
                                    actions.Add(new TileGram.TileGeoAction(
                                        (mx, my, GLOBALS.Layer + 1), 
                                        (CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1]), Utils.CopyGeoCell(GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer + 1])), 
                                        (CopyTileCell(l2Copy), Utils.CopyGeoCell(_copyGeoBuffer[y, x, 1]))));
                                    
                                    GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1] = l2Copy;
                                    GLOBALS.Level.GeoMatrix[my, mx, GLOBALS.Layer + 1] = Utils.CopyGeoCell(_copyGeoBuffer[y, x, 1]);
                                }
                            }
                        }
                        
                        GLOBALS.Gram.Proceed(new TileGram.GroupAction<(TileCell, GeoCell)>(actions));
                        _shouldRedrawLevel = true;
                    }
                }
                    break;

                case AutoTilerMode.PasteWithoutGeo: // Paste tile without geo
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt) || _shortcuts.AltDraw.Check(ctrl, shift, alt))
                    {
                        List<TileGram.ISingleAction<TileCell>> actions = [];
                        
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
                                } else if (_copyMaterials && l1Copy.Data is TileMaterial m) {
                                    GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer] = _copyMaterialColorBuffer[y, x, 0];
                                }
                                
                                actions.Add(new TileGram.TileAction(
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
                                    } else if (_copyMaterials && l1Copy.Data is TileMaterial m) {
                                        GLOBALS.Level.MaterialColors[my, mx, GLOBALS.Layer + 1] = _copyMaterialColorBuffer[y, x, 1];
                                    }
                                    
                                    actions.Add(new TileGram.TileAction(
                                        (mx, my, GLOBALS.Layer + 1), 
                                        CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1]),
                                        CopyTileCell(l2Copy))
                                    );
                                    
                                    GLOBALS.Level.TileMatrix[my, mx, GLOBALS.Layer + 1] = l2Copy;
                                }
                            }
                        }
                        
                        GLOBALS.Gram.Proceed(new TileGram.GroupAction<TileCell>(actions));
                        _shouldRedrawLevel = true;
                    }
                }
                    break;

                case AutoTilerMode.Path: // Auto-Tiling
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
                                if (_autoTilerMacroFirstClick) _autoTilerFirstClickPos = new Data.Coords(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                                else {
                                    ResolveAutoTiler();
                                    _autoTilerPath.Clear();
                                    
                                    _shouldRedrawLevel = true;
                                }

                            }


                            if (_autoTilerMacroFirstClick) {
                                _autoTilerPath.Clear();
                                var points = Utils.GetTrianglePathPoints(_autoTilerFirstClickPos, new Data.Coords(tileMatrixX, tileMatrixY, GLOBALS.Layer), _autoTilerMacroYAxisFirst);
                            
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
            
                case AutoTilerMode.Box: // Box
                {
                    if ((_shortcuts.Draw.Check(ctrl, shift, alt, true) ||
                         _shortcuts.AltDraw.Check(ctrl, shift, alt, true)) && !_boxClickTracker)
                    {
                        _boxClickTracker = true;
                        
                        _boxPlacementFirstX = tileMatrixX;
                        _boxPlacementFirstY = tileMatrixY;

                        _boxRectangle = new Rectangle(_copyFirstX, _copyFirstY, 1, 1);
                    }

                    if (!_boxClickTracker) break;
                    
                    if (tileMatrixX != _boxPlacementSecondX || tileMatrixY != _boxPlacementSecondY)
                    {
                        _boxPlacementSecondX = tileMatrixX;
                        _boxPlacementSecondY = tileMatrixY;
                        
                        _boxRectangle = Utils.RecFromTwoVecs(
                            new Vector2(_boxPlacementFirstX, _boxPlacementFirstY), 
                            new Vector2(_boxPlacementSecondX, _boxPlacementSecondY)
                        );

                        _boxRectangle.Width += 1;
                        _boxRectangle.Height += 1;
                    }

                    if (IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key))
                    {
                        _boxClickTracker = false;

                        var box = _autoTiler?.GenerateBox((int)_boxRectangle.Width, (int)_boxRectangle.Height);
                    
                        if (box is null) break;

                        List<TileGram.ISingleMatrixAction<TileCell>> actions = [];

                        for (var y = 0; y < box.GetLength(0); y ++) {
                            for (var x = 0; x < box.GetLength(1); x++) {
                                var sy = (int)_boxRectangle.Y + y;
                                var sx = (int)_boxRectangle.X + x;

                                if (sy < 0 || sy >= GLOBALS.Level.Height || sx < 0 || sx >= GLOBALS.Level.Width) continue;

                                var cell = box[y, x, 0];

                                if (cell.Data is TileBody b) {
                                    var (bx, by, _) = b.HeadPosition;

                                    b.HeadPosition = ((int)_boxRectangle.X + bx, (int)_boxRectangle.Y + by, 1);

                                    cell.Data = b;
                                }

                                var oldCell = GLOBALS.Level.TileMatrix[sy, sx, 0];
                                
                                var action = new TileGram.TileAction(new(x, y, 0), CopyTileCell(oldCell), CopyTileCell(cell));
                                actions.Add(action);

                                GLOBALS.Level.TileMatrix[sy, sx, 0] = cell;
                            }
                        }

                        GLOBALS.Gram.Proceed(new TileGram.GroupAction<TileCell>(actions));

                        _boxRectangle.X = -1;
                        _boxRectangle.Y = -1;
                        _boxRectangle.Width = 0;
                        _boxRectangle.Height = 0;

                        _shouldRedrawLevel = true;
                    }
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
                            if ((GLOBALS.Settings.TileEditor.OriginalDeletionBehavior || GLOBALS.Settings.TileEditor.UnifiedDeletion || _materialTileSwitch) && (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)) {
                                var actions = RemoveTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                                        
                                foreach (var action in actions) _tempActions.Add(action);
                                _shouldRedrawLevel = true;
                            }
                            break;

                        case TileMaterial:
                            if ((GLOBALS.Settings.TileEditor.OriginalDeletionBehavior || GLOBALS.Settings.TileEditor.UnifiedDeletion || !_materialTileSwitch) && (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)) {
                                var actions = RemoveMaterial(tileMatrixX, tileMatrixY, GLOBALS.Layer, _materialBrushRadius);
                                        
                                foreach (var action in actions) _tempActions.Add(action);
                                _shouldRedrawLevel = true;
                            }
                            break;
                    }
                } else if (!GLOBALS.Settings.TileEditor.OriginalDeletionBehavior || !GLOBALS.Settings.TileEditor.ExactHoverDeletion || GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer].Data is not TileDefault) { 
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
                                        if (GLOBALS.Settings.TileEditor.OriginalDeletionBehavior) {
                                            if (matrixX == tileMatrixX && matrixY == tileMatrixY) {
                                                var actions = RemoveTile(matrixX, matrixY, GLOBALS.Layer);
                                                    
                                                foreach (var action in actions) _tempActions.Add(action);
                                                _shouldRedrawLevel = true;
                                            }
                                        } else if (GLOBALS.Settings.TileEditor.UnifiedDeletion || _materialTileSwitch) {
                                            var actions = RemoveTile(matrixX, matrixY, GLOBALS.Layer);
                                                
                                            foreach (var action in actions) _tempActions.Add(action);
                                            _shouldRedrawLevel = true;
                                        }
                                    }
                                    break;

                                case TileMaterial:
                                    if ((GLOBALS.Settings.TileEditor.UnifiedDeletion || !_materialTileSwitch) && (!_eraseClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)) {
                                        var actions = RemoveMaterial(matrixX, matrixY, GLOBALS.Layer, 0);
                                                
                                        foreach (var action in actions) _tempActions.Add(action);
                                        _shouldRedrawLevel = true;
                                    }
                                    break;
                            }
                        }
                    }
                }
                            
                _prevPosX = tileMatrixX;
                _prevPosY = tileMatrixY;
                        
                _eraseClickTracker = true;
            }
        }
        if (IsMouseButtonReleased(_shortcuts.Erase.Button) && _eraseClickTracker)
        {
            GLOBALS.Gram.Proceed(new TileGram.GroupAction<TileCell>([.._tempActions]));
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

            UpdateTileTexturePanel();
            UpdateTileSpecsPanel();
        }
        else if (_shortcuts.MoveToPreviousCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToPreviousCategory();
            else ToPreviousMaterialCategory();
            
            UpdateTileTexturePanel();
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
            
            UpdateTileTexturePanel();
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
            
            UpdateTileTexturePanel();
            UpdateTileSpecsPanel();
        }

        if (_shortcuts.TileMaterialSwitch.Check(ctrl, shift, alt))
        {
            _materialTileSwitch = !_materialTileSwitch;
            
            UpdateTileTexturePanel();
            UpdateTileSpecsPanel();
        }

        if (_shortcuts.HoveredItemInfo.Check(ctrl, shift, alt)) GLOBALS.Settings.TileEditor.HoveredTileInfo = !GLOBALS.Settings.TileEditor.HoveredTileInfo;
        
        if (_shortcuts.SetMaterialDefault.Check(ctrl, shift, alt) && !_materialTileSwitch) {
            GLOBALS.Level.DefaultMaterial = currentMaterialInit.Item1;
        }

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

        if (_shortcuts.ShowCameras.Check(ctrl, shift, alt)) {
            GLOBALS.Settings.TileEditor.ShowCameras = !GLOBALS.Settings.TileEditor.ShowCameras;
        }

        if (_shortcuts.ToggleIndexHint.Check(ctrl, shift, alt)) {
            GLOBALS.Settings.TileEditor.IndexHint = !GLOBALS.Settings.TileEditor.IndexHint;
        }

        if (!_isSearching) {
            // Move level with keyboard

            if (_shortcuts.MoveViewLeft.Check(ctrl, shift, alt)) {
                _camera.Target.X -= GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.MoveViewTop.Check(ctrl, shift, alt)) {
                _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.MoveViewRight.Check(ctrl, shift, alt)) {
                _camera.Target.X += GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } if (_shortcuts.MoveViewBottom.Check(ctrl, shift, alt)) {
                _camera.Target.Y += GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            }

            else if (_shortcuts.FastMoveViewLeft.Check(ctrl, shift, alt)) {
                _camera.Target.X -= GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.FastMoveViewTop.Check(ctrl, shift, alt)) {
                _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.FastMoveViewRight.Check(ctrl, shift, alt)) {
                _camera.Target.X += GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } if (_shortcuts.FastMoveViewBottom.Check(ctrl, shift, alt)) {
                _camera.Target.Y += GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            }

            else if (_shortcuts.ReallyFastMoveViewLeft.Check(ctrl, shift, alt)) {
                _camera.Target.X -= GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.ReallyFastMoveViewTop.Check(ctrl, shift, alt)) {
                _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } else if (_shortcuts.ReallyFastMoveViewRight.Check(ctrl, shift, alt)) {
                _camera.Target.X += GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            } if (_shortcuts.ReallyFastMoveViewBottom.Check(ctrl, shift, alt)) {
                _camera.Target.Y += GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

                if (_camera.Target.X < -80) _camera.Target.X = -80;
                if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
                
                if (_camera.Target.Y < -80) _camera.Target.Y = -80;
                if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
            }
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
                TilesLayer1 = _showLayer1Tiles,
                TilesLayer2 = _showLayer2Tiles,
                TilesLayer3 = _showLayer3Tiles,
                PropsLayer1 = false,
                PropsLayer2 = false,
                PropsLayer3 = false,
                Water = GLOBALS.Settings.GeneralSettings.Water,
                WaterOpacity = GLOBALS.Settings.GeneralSettings.WaterOpacity,
                TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                Palette = GLOBALS.SelectedPalette,
                Grid = false,
                VisiblePreceedingUnfocusedLayers = GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers,
                CropTilePrevious = GLOBALS.Settings.GeneralSettings.CropTilePreviews,
                VisibleStrayTileFragments = GLOBALS.Settings.TileEditor.ShowStrayTileFragments,
                UnifiedTileColor = GLOBALS.Settings.TileEditor.UnifiedPreviewColor ? Color.White : null
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
                var counter = 0;
                foreach (var cam in GLOBALS.Level.Cameras)
                {
                    DrawRectangleLinesEx(
                        GLOBALS.Settings.TileEditor.CameraInnerBoundires 
                            ? Utils.CameraCriticalRectangle(cam.Coords) 
                            : new(cam.Coords.X, cam.Coords.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                        4f,
                        GLOBALS.Settings.GeneralSettings.ColorfulCameras 
                            ? GLOBALS.CamColors[counter] 
                            : Color.Pink
                    );

                    counter++;
                    Utils.Cycle(ref counter, 0, GLOBALS.CamColors.Length - 1);
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

            if (GLOBALS.Settings.TileEditor.IndexHint)
            {
                Printers.DrawLevelIndexHintsHollow(
                    tileMatrixX, tileMatrixY, 
                    2, 
                    _materialBrushRadius, 
                    Color.White with { A = 100 }
                );
            }
            
            #endregion

            if (_placeMode == AutoTilerMode.Path)
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
                    case AutoTilerMode.Normal: // none
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
                            // Draw current specs
                            if (GLOBALS.Settings.TileEditor.DrawCurrentSpecs) {
                                var headOrigin = Utils.GetTileHeadOrigin(_currentTile!);
                                
                                Printers.DrawTileSpecs(_currentTile?.Specs, new Vector2(tileMatrixX, tileMatrixY) - headOrigin, GLOBALS.Scale);
                            }
                            
                            // Draw current tile
                            if (GLOBALS.Settings.TileEditor.DrawCurrentTile) {
                                Color color = isTileLegal 
                                    ? (GLOBALS.Settings.TileEditor.UnifiedInlinePreviewColor ? Color.Green : _currentCategory.color) 
                                    : Color.Red;
                                
                                Printers.DrawTilePreview(_currentTile, color, (tileMatrixX, tileMatrixY), GLOBALS.Scale);

                                // Where's BeginShaderMode()??????
                                EndShaderMode();
                            }
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

                    case AutoTilerMode.Copy: // copy with geo
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
                    
                    case AutoTilerMode.PasteWithGeo: // paste with geo
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
                    
                    case AutoTilerMode.PasteWithoutGeo: // paste without geo
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
                
                    case AutoTilerMode.Box: // auto box
                    {
                        if (_boxRectangle is { Width: > 0, Height: > 0 })
                        {
                            var rect = new Rectangle(
                                _boxRectangle.X*GLOBALS.Scale, 
                                _boxRectangle.Y*GLOBALS.Scale, 
                                _boxRectangle.Width*GLOBALS.Scale, 
                                _boxRectangle.Height*GLOBALS.Scale);
                            
                            DrawRectangleLinesEx(
                                rect,
                                2f,
                                Color.Purple
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
                                Color.Purple
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
                
            if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);
            
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
                var placeMode = (int)_placeMode;
                ImGui.SetNextItemWidth(200);
                if (ImGui.Combo("Placement Mode", ref placeMode, "Normal\0Copy\0Paste With Geo\0Paste Without Geo\0Auto Pipes\0Auto Box")) {
                    _placeMode = (AutoTilerMode)placeMode;
                }

                switch (_placeMode) {
                    case AutoTilerMode.Path:
                    {
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

                        if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                            ImGui.SetItemDefaultFocus();
                            ImGui.SetKeyboardFocusHere(-1);
                        }
                        
                        _isSearching = ImGui.IsItemActive();
                        
                        if (textUpdated)
                        {
                            _searchTask?.Dispose();
                            _searchTask = Task.Factory.StartNew(Search);
                        }

                        if (ImGui.BeginListBox("##AutoTilerTemplates", ImGui.GetContentRegionAvail() - new Vector2(0, 90))) {
                            if (_autoTiler is not null) {
                                for (var index = 0; index < _autoTiler.PathPacks.Count; index++) 
                                {
                                    var pack = _autoTiler.PathPacks[index];
                                    if (!string.IsNullOrEmpty(_searchText) && !pack.Name.Contains(_searchText)) continue;

                                    var packSelected = ImGui.Selectable(pack.Name, _autoTiler.SelectedPathPack == pack);

                                    if (packSelected) _autoTiler.SelectedPathPack = pack;
                                }
                            }
                            
                            ImGui.EndListBox();
                        }

                        ImGui.SeparatorText("Options");

                        ImGui.Combo("Input Method", ref _autoTilerMethod, "Micro\0Macro");

                        ImGui.Checkbox("Place Geo", ref _autoTilingWithGeo);
                        ImGui.Checkbox("Linear Algorithm", ref _autoTilerLinearAlgorithm);
                    }
                    break;

                    case AutoTilerMode.Box:
                    {
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

                        if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                            ImGui.SetItemDefaultFocus();
                            ImGui.SetKeyboardFocusHere(-1);
                        }
                        
                        _isSearching = ImGui.IsItemActive();
                        
                        if (textUpdated)
                        {
                            _searchTask?.Dispose();
                            _searchTask = Task.Factory.StartNew(Search);
                        }
                        
                        if (ImGui.BeginListBox("##AutoBoxTemplates", ImGui.GetContentRegionAvail() - new Vector2(0, 90))) {

                            if (_autoTiler is not null) {
                                for (var index = 0; index < _autoTiler.BoxPacks.Count; index++) 
                                {
                                    var pack = _autoTiler.BoxPacks[index];
                                    if (!string.IsNullOrEmpty(_searchText) && !pack.Name.Contains(_searchText)) continue;

                                    var packSelected = ImGui.Selectable(pack.Name, _autoTiler.SelectedBoxPack == pack);

                                    if (packSelected) _autoTiler.SelectedBoxPack = pack;
                                }
                            }
                        
                            
                            ImGui.EndListBox();
                        }
                    }
                    break;
                    default:
                    {
                        if (ImGui.Button(_materialTileSwitch ? "Switch to materials" : "Switch to tiles", ImGui.GetContentRegionAvail() with { Y = 20 }))
                            _materialTileSwitch = !_materialTileSwitch;

                        // Default Material
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

                        if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                            ImGui.SetItemDefaultFocus();
                            ImGui.SetKeyboardFocusHere(-1);
                        }
                        
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
                                            
                                            UpdateTileTexturePanel();
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
                                            
                                            UpdateTileTexturePanel();
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
                                            
                                            UpdateTileTexturePanel();
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
                                            
                                            UpdateTileTexturePanel();
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
                                            ImGui.ColorConvertFloat4ToU32(new Vector4(material.Item2.R / 255f, material.Item2.G / 255f, material.Item2.B / 255f, 1f))
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
                    break;
                }

                ImGui.End();
            }

            // Tile Textures

            var texturesWinOpened = ImGui.Begin("Textures##TileEditorTileTexturesWin");

            var texturesWinPos = ImGui.GetWindowPos();
            var texturesWinSpace = ImGui.GetWindowSize();

            _isTexturesWinHovered = CheckCollisionPointRec(GetMousePosition(), new(texturesWinPos.X - 5, texturesWinPos.Y, texturesWinSpace.X + 10, texturesWinSpace.Y));

            if (texturesWinOpened) {
                
                rlImGui.ImageRenderTextureFit(_tileTexturePanelRT);
                
                ImGui.End();
            }
            
            // Tile Specs

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
                rlImGui.ImageRenderTextureFit(_tileSpecsPanelRT);
                
                // Idk where to put this
                ImGui.End();
            }
            
            // Settings
            #region Settings Window

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
                var liPos = (int)GLOBALS.Settings.TileEditor.LayerIndicatorPosition;
                ImGui.SetNextItemWidth(100);
                var liPosChanged = ImGui.Combo("Layer Incdicator Position", ref liPos, "Top Left\0Top Right\0Bottom Left\0Bottom Right\0Middle Bottom\0Middle Top\0");

                if (liPosChanged) {
                    GLOBALS.Settings.TileEditor.LayerIndicatorPosition = (TileEditorSettings.ScreenRelativePosition)liPos;
                }

                ImGui.SeparatorText("Tooling");

                var overridableMaterials = GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials;
                if (ImGui.Checkbox("Overridable Materials", ref overridableMaterials)) {
                    GLOBALS.Settings.TileEditor.ImplicitOverrideMaterials = overridableMaterials;
                }

                var ogDelete = GLOBALS.Settings.TileEditor.OriginalDeletionBehavior;
                if (ImGui.Checkbox("Original Deletion Bahvior", ref ogDelete)) {
                    GLOBALS.Settings.TileEditor.OriginalDeletionBehavior = ogDelete;
                }

                if (ogDelete) ImGui.BeginDisabled();

                var unifiedDeletion = GLOBALS.Settings.TileEditor.UnifiedDeletion;
                if (ImGui.Checkbox("Unified Deletion", ref unifiedDeletion)) {
                   GLOBALS.Settings.TileEditor.UnifiedDeletion = unifiedDeletion;
                }

                var exactHoverDeletion = GLOBALS.Settings.TileEditor.ExactHoverDeletion;
                if (ImGui.Checkbox("Exact hover deletion", ref exactHoverDeletion)) {
                    GLOBALS.Settings.TileEditor.ExactHoverDeletion = exactHoverDeletion;
                }

                if (ogDelete) ImGui.EndDisabled();

                var eraseStrays = ImGui.Button("Erase Stray Tile Fragments", ImGui.GetContentRegionAvail() with { Y = 20 });

                if (eraseStrays) {
                    ImGui.OpenPopup("ConfirmEraseStrayTileFragments");
                }

                if (ImGui.BeginPopup("ConfirmEraseStrayTileFragments")) {
                    ImGui.Text("Are you sure?");

                    if (ImGui.Button("Confirm")) {
                        EraseStrayFragments(GLOBALS.Level.TileMatrix);
                        _shouldRedrawLevel = true;
                        ImGui.CloseCurrentPopup();
                    }

                    if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
                    
                    ImGui.EndPopup();
                }

                //

                ImGui.Checkbox("Tooltip", ref _tooltip);

                if (!_tooltip) ImGui.BeginDisabled();

                var useTextures = GLOBALS.Settings.TileEditor.UseTexturesInTooltip;
                if (ImGui.Checkbox("Use textures in tooltip", ref useTextures)) {
                    GLOBALS.Settings.TileEditor.UseTexturesInTooltip = useTextures;
                    UpdatePreviewToolTip();
                }
                if (!_tooltip) ImGui.EndDisabled();

                var drawCurrentTile = GLOBALS.Settings.TileEditor.DrawCurrentTile;
                if (ImGui.Checkbox("Current Tile", ref drawCurrentTile)) {
                    GLOBALS.Settings.TileEditor.DrawCurrentTile = drawCurrentTile;
                }

                var drawCurrentSpecs = GLOBALS.Settings.TileEditor.DrawCurrentSpecs;
                if (ImGui.Checkbox("Current Specs", ref drawCurrentSpecs)) {
                    GLOBALS.Settings.TileEditor.DrawCurrentSpecs = drawCurrentSpecs;
                }
                
                ImGui.Checkbox("Deep Tile Copy", ref _deepTileCopy);
                ImGui.Checkbox("Copy Materials Too", ref _copyMaterials);

                var hoveredInfo = GLOBALS.Settings.TileEditor.HoveredTileInfo;
                ImGui.Checkbox("Hovered Item Info", ref hoveredInfo);
                if (GLOBALS.Settings.TileEditor.HoveredTileInfo != hoveredInfo) 
                    GLOBALS.Settings.TileEditor.HoveredTileInfo = hoveredInfo;

                var showCameras = GLOBALS.Settings.TileEditor.ShowCameras;
                if (ImGui.Checkbox("Cameras", ref showCameras)) GLOBALS.Settings.TileEditor.ShowCameras = showCameras;

                if (!showCameras) ImGui.BeginDisabled();

                var innerCamBounds = GLOBALS.Settings.TileEditor.CameraInnerBoundires;
                if (ImGui.Checkbox("Camera's Inner Boundries", ref innerCamBounds)) {
                    GLOBALS.Settings.TileEditor.CameraInnerBoundires = innerCamBounds;
                }

                if (!showCameras) ImGui.EndDisabled();

                var grid = GLOBALS.Settings.TileEditor.Grid;
                if (ImGui.Checkbox("Grid", ref grid))
                {
                    GLOBALS.Settings.TileEditor.Grid = grid;
                    _shouldRedrawLevel = true;
                }

                var ruler = GLOBALS.Settings.TileEditor.IndexHint;
                if (ImGui.Checkbox("Ruler", ref ruler)) {
                    GLOBALS.Settings.TileEditor.IndexHint = ruler;
                }

                var showStrays = GLOBALS.Settings.TileEditor.ShowStrayTileFragments;
                if (ImGui.Checkbox("Stray Tile Fragments", ref showStrays)) {
                    GLOBALS.Settings.TileEditor.ShowStrayTileFragments = showStrays;
                    _shouldRedrawLevel = true;
                }

                var unifiedInlineColor = GLOBALS.Settings.TileEditor.UnifiedInlinePreviewColor;
                if (ImGui.Checkbox("Unified Placement Preview Color", ref unifiedInlineColor)) {
                    GLOBALS.Settings.TileEditor.UnifiedInlinePreviewColor = unifiedInlineColor;
                }

                var unifiedColor = GLOBALS.Settings.TileEditor.UnifiedPreviewColor;
                if (ImGui.Checkbox("Unified Preview Color", ref unifiedColor)) {
                    GLOBALS.Settings.TileEditor.UnifiedPreviewColor = unifiedColor;
                    _shouldRedrawLevel = true;
                }
                
                ImGui.End();
            }
            
            #endregion
            
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

            DrawRectangleLines((int)layer3Rect.X, (int)layer3Rect.Y, 40, 40, Color.Gray);

            if (GLOBALS.Layer == 2) DrawText("3", (int)layer3Rect.X + 15, (int)layer3Rect.Y+10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            
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

                DrawRectangleLines((int)layer2Rect.X, (int)layer2Rect.Y, 40, 40, Color.Gray);

                if (GLOBALS.Layer == 1) DrawText("2", (int)layer2Rect.X + 15, (int)layer2Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
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
                    (int)layer1Rect.X, (int)layer1Rect.Y, 40, 40, Color.Gray);

                DrawText("1", (int)layer1Rect.X + 15, (int)layer1Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
            }

            if (newLayer != GLOBALS.Layer) {
                GLOBALS.Layer = newLayer;
                _shouldRedrawLevel = true;
            }
            
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
                            var labelPos = GetLabelPosition(MeasureText(GLOBALS.Level.DefaultMaterial, 20), 20);
                            
                            DrawText(
                                GLOBALS.Level.DefaultMaterial,
                                (int)labelPos.X,
                                (int)labelPos.Y,
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            var labelPos = GetLabelPosition(MeasureText(GLOBALS.Level.DefaultMaterial, 30), 30);

                            DrawTextEx(
                                GLOBALS.Font.Value,
                                GLOBALS.Level.DefaultMaterial,
                                labelPos,
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
                            var text = h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\"";

                            var labelPos = GetLabelPosition(MeasureText(text, 20), 20);

                            DrawText(
                                text,
                                (int)labelPos.X,
                                (int)labelPos.Y,
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            var text = h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\"";

                            var labelPos = GetLabelPosition(MeasureText(text, 30), 30);

                            DrawTextEx(
                                GLOBALS.Font.Value,
                                text,
                                labelPos,
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
                                var text = supposedHead.Data is TileHead h
                                        ? h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\""
                                        : "Stray Tile Fragment";

                                var labelPos = GetLabelPosition(MeasureText(text, 20), 20);

                                DrawText(
                                    text,
                                    (int)labelPos.X,
                                    (int)labelPos.Y,
                                    20,
                                    Color.White
                                );
                                
                            }
                            else
                            {
                                var text = supposedHead.Data is TileHead h
                                        ? h.Definition?.Name ?? $"Undefined Tile \"{h.Name}\""
                                        : "Stray Tile Fragment";

                                var labelPos = GetLabelPosition(MeasureText(text, 30), 30);

                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    text,
                                    labelPos,
                                    30,
                                    1,
                                    Color.White
                                );
                            }
                        
                        }
                        catch (IndexOutOfRangeException)
                        {
                            var text = "Stray Tile Fragment";

                            if (GLOBALS.Font is null) {
                                var labelPos = GetLabelPosition(MeasureText(text, 20), 20);

                                DrawText(text,
                                    (int)labelPos.X,
                                    (int)labelPos.Y,
                                    20,
                                    Color.White
                                );
                            }
                            else
                            {
                                var labelPos = GetLabelPosition(MeasureText(text, 30), 30);

                                DrawTextEx(
                                    GLOBALS.Font.Value,
                                    text,
                                    labelPos,
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
                        var text = m.Name;

                        if (GLOBALS.Font is null) {

                            var labelPos = GetLabelPosition(MeasureText(text, 20), 20);

                            DrawText(m.Name,
                                (int)labelPos.X,
                                (int)labelPos.Y,
                                20,
                                Color.White
                            );
                        }
                        else
                        {
                            var labelPos = GetLabelPosition(MeasureText(text, 30), 30);
                            
                            DrawTextEx(
                                GLOBALS.Font.Value,
                                text,
                                labelPos,
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