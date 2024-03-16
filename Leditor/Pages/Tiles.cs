using System.Numerics;
using System.Text;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor;

internal class TileEditorPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    readonly Serilog.Core.Logger _logger = logger;
    Camera2D _camera = camera ?? new() { Zoom = 1.0f };

    private readonly GlobalShortcuts _gShortcuts = GLOBALS.Settings.Shortcuts.GlobalShortcuts;
    private readonly TileShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.TileEditor;
    
    private bool _materialTileSwitch;
    private int _tileCategoryIndex;
    private int _tileIndex;
    private int _materialCategoryIndex;
    private int _materialIndex;
    private bool _clickTracker;
    private bool _tileCategoryFocus = true;
    private int _tileCategoryScrollIndex;
    private int _tileScrollIndex;
    private int _materialCategoryScrollIndex;
    private int _materialScrollIndex;

    private bool _highlightPaths;

    private bool _drawClickTracker;
    private bool _eraseClickTracker;
    private bool _copyClickTracker;

    private bool _deepTileCopy = true;

    private int _prevPosX = -1;
    private int _prevPosY = -1;

    private int _copyFirstX = -1;
    private int _copyFirstY = -1;

    private int _copySecondX = -1;
    private int _copySecondY = -1;

    private bool _showLayer1Tiles = true;
    private bool _showLayer2Tiles = true;
    private bool _showLayer3Tiles = true;

    private bool _showTileLayer1 = true;
    private bool _showTileLayer2 = true;
    private bool _showTileLayer3 = true;

    /// <summary>
    /// 0 - none
    /// 1 - with geo
    /// 2 - without geo
    /// 3 - paste with geo
    /// 4 - paste without geo
    /// </summary>
    private int _copyMode;

    private Rectangle _prevCopiedRectangle;
    private Rectangle _copyRectangle;
    private TileCell[,,] _copyBuffer = new TileCell[0,0,0];
    private RunCell[,,] _copyGeoBuffer = new RunCell[0, 0, 0];

    private readonly string[] _tileCategoryNames = [..GLOBALS.TileCategories.Select(t => t.Item1)];
    private readonly string[] _materialCategoryNames = [..GLOBALS.MaterialCategories];

    private bool _tileSpecDisplayMode;
    
    private int _tilePanelWidth = 400;
    private int _materialBrushRadius;

    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isTilesWinHovered;
    private bool _isTilesWinDragged;
    
    private bool _isSpecsWinHovered;
    private bool _isSpecsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;
    
    private bool _isSettingsWinHovered;
    private bool _isSettingsWinDragged;

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
                if (h.CategoryPostition is (-1, -1, _)) return emptyRect;
                
                var data = h;
                var tileInit = GLOBALS.Tiles[data.CategoryPostition.Item1][data.CategoryPostition.Item2];
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
                if (supposedHead.Data is TileHead { CategoryPostition: (-1, -1, _) } or not TileHead)
                    return emptyRect;

                var headTile = (TileHead)supposedHead.Data;
            
                var tileInit = GLOBALS.Tiles[headTile.CategoryPostition.Item1][headTile.CategoryPostition.Item2];
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

    /// Has no regard to matrix bounds and gets unscaled coords of a matrix
    private static TileCell[,,] GetSingleTile(int x, int y, int z)
    {
        ref var cell = ref GLOBALS.Level.TileMatrix[y, x, z];

        TileCell[,,] array;
        var emptyArray = new TileCell[0, 0, 0];

        switch (cell.Data)
        {
            case TileHead h:
            {
                // If tile is undefined
                if (h.CategoryPostition is (-1, -1, _)) return emptyArray;
                
                var data = h;
                var tileInit = GLOBALS.Tiles[data.CategoryPostition.Item1][data.CategoryPostition.Item2];
                var (width, height) = tileInit.Size;

                var isThick = tileInit.Specs2.Length > 0 && z != 2;

                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new(x, y), head);

                if (isThick)
                {
                    array = new TileCell[height, width, 2];

                    for (var my = 0; my < height; my++)
                    {
                        for (var mx = 0; mx < width; mx++)
                        {
                            var matrixX = mx + (int)start.X;
                            var matrixY = my + (int)start.Y;

                            array[my, mx, 0] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x];
                            array[my, mx, 1] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x + 1];
                        }
                    }
                }
                else
                {
                    array = new TileCell[height, width, 1];
                    
                    for (var my = 0; my < height; my++)
                    {
                        for (var mx = 0; mx < width; mx++)
                        {
                            var matrixX = mx + (int)start.X;
                            var matrixY = my + (int)start.Y;

                            array[my, mx, 0] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x];
                        }
                    }
                }

                return array;
            }

            case TileBody b:
            {
                var (headX, headY, headZ) = b.HeadPosition;

                // This is done because Lingo is 1-based index
                var supposedHead = GLOBALS.Level.TileMatrix[headY - 1, headX - 1, headZ - 1];

                // if the head was not found, only delete the given tile body
                if (supposedHead.Data is TileHead { CategoryPostition: (-1, -1, _) } or not TileHead)
                    return emptyArray;

                var headTile = (TileHead)supposedHead.Data;
            
                var tileInit = GLOBALS.Tiles[headTile.CategoryPostition.Item1][headTile.CategoryPostition.Item2];
                var (width, height) = tileInit.Size;
                
                // get the "middle" point of the tile
                var head = Utils.GetTileHeadOrigin(tileInit);

                // the top-left of the tile
                var start = Raymath.Vector2Subtract(new(headX, headY), Raymath.Vector2AddValue(head, 1));

                var isThick = tileInit.Specs2.Length > 0 && z != 2;
                
                if (isThick)
                {
                    array = new TileCell[height, width, 2];

                    for (var my = 0; my < height; my++)
                    {
                        for (var mx = 0; mx < width; mx++)
                        {
                            var matrixX = mx + (int)start.X;
                            var matrixY = my + (int)start.Y;

                            array[my, mx, 0] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x];
                            array[my, mx, 1] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x + 1];
                        }
                    }
                }
                else
                {
                    array = new TileCell[height, width, 1];
                    
                    for (var my = 0; my < height; my++)
                    {
                        for (var mx = 0; mx < width; mx++)
                        {
                            var matrixX = mx + (int)start.X;
                            var matrixY = my + (int)start.Y;

                            array[my, mx, 0] = GLOBALS.Level.TileMatrix[matrixY, matrixX, x];
                        }
                    }
                }

                return array;
            }
            
            default: return emptyArray;
        }
    }

    private void ToNextTileCategory(int pageSize)
    {
        _tileCategoryIndex = ++_tileCategoryIndex % GLOBALS.TileCategories.Length;

        if (_tileCategoryIndex % (pageSize + _tileCategoryScrollIndex) == pageSize + _tileCategoryScrollIndex - 1
            && _tileCategoryIndex != GLOBALS.TileCategories.Length - 1)
            _tileCategoryScrollIndex++;

        if (_tileCategoryIndex == 0)
        {
            _tileCategoryScrollIndex = 0;
        }

        _tileIndex = 0;
    }

    private void ToPreviousCategory(int pageSize)
    {
        _tileCategoryIndex--;

        if (_tileCategoryIndex < 0)
        {
            _tileCategoryIndex = GLOBALS.Tiles.Length - 1;
        }

        if (_tileCategoryIndex == (_tileCategoryScrollIndex + 1) && _tileCategoryIndex != 1) _tileCategoryScrollIndex--;
        if (_tileCategoryIndex == GLOBALS.Tiles.Length - 1) _tileCategoryScrollIndex += Math.Abs(GLOBALS.Tiles.Length - pageSize);
        _tileIndex = 0;
    }

    private void ToNextMaterialCategory(int pageSize)
    {
        _materialCategoryIndex = ++_materialCategoryIndex % GLOBALS.MaterialCategories.Length;

        if (_materialCategoryIndex % (pageSize + _materialCategoryScrollIndex) == pageSize + _materialCategoryScrollIndex - 1
            && _materialCategoryIndex != GLOBALS.MaterialCategories.Length - 1)
            _materialCategoryScrollIndex++;

        if (_materialCategoryIndex == 0)
        {
            _materialCategoryScrollIndex = 0;
        }

        _materialIndex = 0;
    }
    
    private void ToPreviousMaterialCategory(int pageSize)
    {
        _materialCategoryIndex--;

        if (_materialCategoryIndex < 0)
        {
            _materialCategoryIndex = GLOBALS.MaterialCategories.Length - 1;

            if (_materialCategoryIndex == (_materialCategoryScrollIndex + 1) && _materialCategoryIndex != 1) _materialCategoryScrollIndex--;
            if (_materialCategoryScrollIndex == GLOBALS.MaterialCategories.Length - 1) _materialCategoryScrollIndex = Math.Abs(GLOBALS.MaterialCategories.Length - pageSize);

            _materialIndex = 0;
        }
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
        var cell = GLOBALS.Level.TileMatrix[my, mx, mz];

        List<Gram.ISingleMatrixAction<TileCell>> actions = [];

        if (cell.Data is TileHead h)
        {
            // tile is undefined
            if (h.CategoryPostition is (-1, -1, _))
            {
                var oldCell = GLOBALS.Level.TileMatrix[my, mx, mz];
                var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                
                GLOBALS.Level.TileMatrix[my, mx, mz] = newCell;
                
                return [ new Gram.TileAction((mx, my, mz), oldCell, newCell) ];
            }
            
            var data = h;
            var tileInit = GLOBALS.Tiles[data.CategoryPostition.Item1][data.CategoryPostition.Item2];
            var (width, height) = tileInit.Size;

            var isThick = tileInit.Specs2.Length > 0;

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

                    if (matrixX < 0 || matrixX >= GLOBALS.Level.Width || matrixY < 0 || matrixY >=GLOBALS.Level.Height) continue;

                    var oldCell = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz];
                    var newCell = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                    
                    actions.Add(new Gram.TileAction((matrixX, matrixY, mz), oldCell, newCell));

                    GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = newCell;
                    
                    if (isThick && mz != 2)
                    {
                        var oldCell2 = GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1];
                        var newCell2 = new TileCell
                            { Type = TileType.Default, Data = new TileDefault() };
                        
                        actions.Add(new Gram.TileAction((matrixX, matrixY, mz + 1), oldCell2, newCell2));
                        
                        GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = newCell2;
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
            if (supposedHead.Data is TileHead { CategoryPostition: (-1, -1, _) } or not TileHead)
            {
                GLOBALS.Level.TileMatrix[my, mx, mz] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                return [];
            }

            var headTile = (TileHead)supposedHead.Data;
            
            var tileInit = GLOBALS.Tiles[headTile.CategoryPostition.Item1][headTile.CategoryPostition.Item2];
            var (width, height) = tileInit.Size;

            var isThick = tileInit.Specs2.Length > 0;

            // get the "middle" point of the tile
            var head = Utils.GetTileHeadOrigin(tileInit);

            // the top-left of the tile
            var start = Raymath.Vector2Subtract(new(headX, headY), Raymath.Vector2AddValue(head, 1));

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var matrixX = x + (int)start.X;
                    var matrixY = y + (int)start.Y;
                    
                    if (matrixX < 0 || matrixX >= GLOBALS.Level.Width || matrixY < 0 || matrixY >= GLOBALS.Level.Height) continue;

                    GLOBALS.Level.TileMatrix[matrixY, matrixX, mz] = new TileCell { Type = TileType.Default, Data = new TileDefault() };
                    if (isThick)
                    {
                        if (headZ - 1 == mz)
                        {
                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz + 1] = new TileCell
                                { Type = TileType.Default, Data = new TileDefault() };
                        } 
                        else if (headZ - 1 == mz - 1)
                        {
                            GLOBALS.Level.TileMatrix[matrixY, matrixX, mz - 1] = new TileCell
                                { Type = TileType.Default, Data = new TileDefault() };
                        }
                    }
                }
            }
        }

        return actions;
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithGeo(
        in InitTile init,
        int tileCategoryIndex,
        int tileIndex,
        (int x, int y, int z) matrixPosition
    )
    {
        var (mx, my, mz) = matrixPosition;
        var (width, height) = init.Size;
        var specs = init.Specs;
        var specs2 = init.Specs2;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(new(mx, my), head);
        
        // First remove tile heads in the way

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

                ref var cell = ref GLOBALS.Level.TileMatrix[matrixY, matrixX, mz];

                if (cell.Data is TileHead) RemoveTile(matrixX, matrixY, mz);
            }
        }
        
        List<Gram.ISingleMatrixAction<TileCell>> actions = [];

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

                    var spec = specs[specsIndex];
                    var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                    
                    // If it's the tile head
                    if (x == (int)head.X && y == (int)head.Y)
                    {
                        // Place the head of the tile at matrixPosition
                        var newHead = new TileCell
                        {
                            Type = TileType.TileHead,
                            Data = new TileHead(tileCategoryIndex, tileIndex, init.Name)
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
                }
            }
        }

        return actions;
    }
    
    private static List<Gram.ISingleMatrixAction<TileCell>> ForcePlaceTileWithoutGeo(
        in InitTile init,
        int tileCategoryIndex,
        int tileIndex,
        (int x, int y, int z) matrixPosition
    )
    {
        var (mx, my, mz) = matrixPosition;
        var (width, height) = init.Size;
        var specs = init.Specs;
        var specs2 = init.Specs2;

        // get the "middle" point of the tile
        var head = Utils.GetTileHeadOrigin(init);

        // the top-left of the tile
        var start = Raymath.Vector2Subtract(new(mx, my), head);
        
        // First remove tile heads in the way

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

                ref var cell = ref GLOBALS.Level.TileMatrix[matrixY, matrixX, mz];

                if (cell.Data is TileHead) RemoveTile(matrixX, matrixY, mz);
            }
        }
        
        // Record the action

        var newTileHead = new TileCell
        {
            Type = TileType.TileHead,
            Data = new TileHead(tileCategoryIndex, tileIndex, init.Name)
        };
        
        List<Gram.ISingleMatrixAction<TileCell>> actions =
        [
            new Gram.TileAction(
                matrixPosition, 
                CopyTileCell(GLOBALS.Level.TileMatrix[my, mx, mz]),
                CopyTileCell(newTileHead))
        ];

        // Place the head of the tile at matrixPosition
        GLOBALS.Level.TileMatrix[my, mx, mz] = newTileHead;

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

                    var spec = specs[specsIndex];
                    var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                    
                    // If it's the tile head
                    if (x == (int)head.X && y == (int)head.Y)
                    {
                        // Place the head of the tile at matrixPosition
                        GLOBALS.Level.TileMatrix[my, mx, mz] = new TileCell
                        {
                            Type = TileType.TileHead,
                            Data = new TileHead(tileCategoryIndex, tileIndex, init.Name)
                        };
                    }
                    
                    if (spec != -1)
                    {
                        // GLOBALS.Level.GeoMatrix[matrixY, matrixX, mz].Geo = spec;
                        
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
                        // GLOBALS.Level.GeoMatrix[matrixY, matrixX, mz + 1].Geo = spec2;
                        
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
            TileHead h => new TileHead(h.CategoryPostition.Category, h.CategoryPostition.Index, h.CategoryPostition.Name),
            TileBody b => new TileBody(b.HeadPosition.x, b.HeadPosition.y, b.HeadPosition.z),
                                            
            _ => throw new Exception("Invalid tile data")
        }
    };

    public void Draw()
    {
        GLOBALS.Page = 3;

        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var teWidth = GetScreenWidth();
        var teHeight = GetScreenHeight();

        var tileMouseWorld = GetScreenToWorld2D(GetMousePosition(), _camera);
        var tileMouse = GetMousePosition();
        
        var tilePanelRect = new Rectangle(teWidth - _tilePanelWidth, 0, _tilePanelWidth, teHeight);
        var panelMenuHeight = tilePanelRect.Height - 270;
        var leftPanelSideStart = new Vector2(teWidth - _tilePanelWidth, 0);
        var leftPanelSideEnd = new Vector2(teWidth - _tilePanelWidth, teHeight);
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

        var canDrawTile = !_isSettingsWinHovered && 
                          !_isSettingsWinDragged && 
                          !_isSpecsWinHovered &&
                          !_isSpecsWinDragged &&
                          !_isTilesWinHovered &&
                          !_isTilesWinDragged &&
                          !_isShortcutsWinHovered &&
                          !_isShortcutsWinDragged &&
                          !_isNavigationWinHovered &&
                          !_isNavigationWinDragged &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));
        
        var categoriesPageSize = (int)panelMenuHeight / 26;

        // TODO: fetch init only when menu indices change
        var currentTileInit = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex];
        var currentMaterialInit = GLOBALS.Materials[_materialCategoryIndex][_materialIndex];


        #region TileEditorShortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
        
        if (_gShortcuts.ToMainPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 1");
            #endif
            GLOBALS.Page = 1;
        }
        if (_gShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 2");
            #endif
            GLOBALS.Page = 2;
        }
        // if (_gShortcuts.ToTileEditor.Check(ctrl, shift)) GLOBALS.Page = 3;
        if (_gShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 4");
            #endif
            GLOBALS.Page = 4;
        }
        if (_gShortcuts.ToLightEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 5");
            #endif
            GLOBALS.Page = 5;
        }

        if (_gShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 6");
            #endif
            GLOBALS.ResizeFlag = true; 
            GLOBALS.NewFlag = false; 
            GLOBALS.Page = 6;
        }
        if (_gShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 7");
            #endif
            GLOBALS.Page = 7;
        }
        if (_gShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 8");
            #endif
            GLOBALS.Page = 8;
        }
        if (_gShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
        {
            #if DEBUG
            _logger.Debug($"Going to page 9");
            #endif
            GLOBALS.Page = 9;
        }
        
        // Undo

        if (_shortcuts.Undo.Check(ctrl, shift, alt)) Undo();
        
        // Redo
        
        if (_shortcuts.Redo.Check(ctrl, shift, alt)) Redo();
        
        //
        
        // Copy Shortcuts

        if (_shortcuts.CopyTiles.Check(ctrl, shift, alt)) _copyMode = _copyMode == 1 ? 0 : 1;
        
        if (_shortcuts.PasteTilesWithGeo.Check(ctrl, shift, alt)) _copyMode = _copyMode == 3 ? 0 : 3;
        if (_shortcuts.PasteTilesWithoutGeo.Check(ctrl, shift, alt)) _copyMode = _copyMode == 4 ? 0 : 4;
        
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
                    var pickedTile = Utils.PickupTile(tileMatrixX, tileMatrixY, GLOBALS.Layer);
                    (_tileCategoryIndex, _tileIndex) = pickedTile ?? (_tileCategoryIndex, _tileIndex);
                    _materialTileSwitch = pickedTile is not null;
                    break;
            }
        }

        var isTileLegal = Utils.IsTileLegal(ref currentTileInit, new(tileMatrixX, tileMatrixY));
        
        // handle mouse drag and brush resize

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
            
            // handle zoom
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                if (IsKeyDown(_shortcuts.ResizeMaterialBrush.Key))
                {
                    _materialBrushRadius += tileWheel > 0 ? 1 : -1;
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

        // handle placing/removing tiles

        if (canDrawTile)
        {
            switch (_copyMode)
            {
                case 0: // None
                {
                    if (_shortcuts.Draw.Check(ctrl, shift, alt, true) && inMatrixBounds)
                    {
                        // _clickTracker = true;
                        if (_materialTileSwitch)
                        {
                            if (_shortcuts.ForcePlaceTileWithGeo.Check(ctrl, shift, alt, true))
                            {
                                if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                                {
                                    var actions = ForcePlaceTileWithGeo(
                                        currentTileInit, 
                                        _tileCategoryIndex, 
                                        _tileIndex, 
                                        (tileMatrixX, tileMatrixY, 
                                            GLOBALS.Layer
                                        )
                                    );
                                    
                                    foreach (var action in actions) _tempActions.Add(action);
                                }
                            } 
                            else if (_shortcuts.ForcePlaceTileWithoutGeo.Check(ctrl, shift, alt, true))
                            {
                                if (!_drawClickTracker || _prevPosX != tileMatrixX || _prevPosY != tileMatrixY)
                                {
                                    var actions = ForcePlaceTileWithoutGeo(
                                        currentTileInit, 
                                        _tileCategoryIndex, 
                                        _tileIndex, 
                                        (tileMatrixX, tileMatrixY, 
                                            GLOBALS.Layer
                                        )
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
                                            currentTileInit,
                                            _tileCategoryIndex,
                                            _tileIndex,
                                            (tileMatrixX, tileMatrixY,
                                                GLOBALS.Layer
                                            )
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
                    }
                    if ((IsMouseButtonReleased(_shortcuts.Draw.Button) || IsKeyReleased(_shortcuts.AltDraw.Key)) && _drawClickTracker)
                    {
                        GLOBALS.Gram.Proceed(new Gram.GroupAction<TileCell>([.._tempActions]));
                        _tempActions.Clear();
                        
                        _prevPosX = -1;
                        _prevPosY = -1;
                        
                        _drawClickTracker = false;
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

                            _copyMode = 0;
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

                                if (mx >= GLOBALS.Level.Width || my >= GLOBALS.Level.Height) continue;

                                var l1Copy = CopyTileCell(_copyBuffer[y, x, 0]);

                                if (l1Copy.Data is TileBody b)
                                {
                                    var pos = b.HeadPosition;

                                    l1Copy.Data = new TileBody(pos.x - difX, pos.y - difY, pos.z);
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

                                        l2Copy.Data = new TileBody(pos.x - difX, pos.y - difY, pos.z);
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

                                if (mx >= GLOBALS.Level.Width || my >= GLOBALS.Level.Height) continue;

                                var l1Copy = CopyTileCell(_copyBuffer[y, x, 0]);

                                if (l1Copy.Data is TileBody b)
                                {
                                    var pos = b.HeadPosition;

                                    l1Copy.Data = new TileBody(pos.x - difX, pos.y - difY, pos.z);
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

                                        l2Copy.Data = new TileBody(pos.x - difX, pos.y - difY, pos.z);
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
                            
                _prevPosX = tileMatrixX;
                _prevPosY = tileMatrixY;
                        
                _eraseClickTracker = true;
            }
        }
        if (IsMouseButtonReleased(_shortcuts.Erase.Button) && _eraseClickTracker)
        {
            _tempActions.Clear();
                        
            _prevPosX = -1;
            _prevPosY = -1;
                        
            _eraseClickTracker = false;
        }

        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;
        }

        // handle resizing tile panel
        // TODO: remove this feature
        if (((tileMouse.X <= leftPanelSideStart.X + 5 && tileMouse.X >= leftPanelSideStart.X - 5 && tileMouse.Y >= leftPanelSideStart.Y && tileMouse.Y <= leftPanelSideEnd.Y) || _clickTracker) &&
        IsMouseButtonDown(MouseButton.Left))
        {
            _clickTracker = true;

            var delta = GetMouseDelta();

            if (_tilePanelWidth - (int)delta.X >= 400 && _tilePanelWidth - (int)delta.X <= 700)
            {
                _tilePanelWidth -= (int)delta.X;
            }
        }

        if (IsMouseButtonReleased(MouseButton.Left))
        {
            _clickTracker = false;
        }

        // change focus

        if (_shortcuts.FocusOnTileMenu.Check(ctrl, shift, alt))
        {
            _tileCategoryFocus = false;
        }

        if (_shortcuts.FocusOnTileCategoryMenu.Check(ctrl, shift, alt))
        {
            _tileCategoryFocus = true;
        }

        // change tile category

        if (_shortcuts.MoveToNextCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToNextTileCategory(categoriesPageSize);
            else ToNextMaterialCategory(categoriesPageSize);
        }
        else if (_shortcuts.MoveToPreviousCategory.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch) ToPreviousCategory(categoriesPageSize);
            else ToPreviousMaterialCategory(categoriesPageSize);
        }

        if (_shortcuts.MoveDown.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToNextTileCategory(categoriesPageSize);
                }
                else
                {
                    _tileIndex = ++_tileIndex % GLOBALS.Tiles[_tileCategoryIndex].Length;
                    if (
                        _tileIndex % (categoriesPageSize + _tileScrollIndex) == categoriesPageSize + _tileScrollIndex - 1 &&
                        _tileIndex != GLOBALS.Tiles[_tileCategoryIndex].Length - 1) _tileScrollIndex++;

                    if (_tileIndex == 0) _tileScrollIndex = 0;
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToNextMaterialCategory(categoriesPageSize);
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
        }

        if (_shortcuts.MoveUp.Check(ctrl, shift, alt))
        {
            if (_materialTileSwitch)
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousCategory(categoriesPageSize);
                }
                else
                {
                    if (_tileIndex == (_tileScrollIndex) && _tileIndex != 1) _tileScrollIndex--;

                    _tileIndex--;
                    if (_tileIndex < 0) _tileIndex = GLOBALS.Tiles[_tileCategoryIndex].Length - 1;

                    if (_tileIndex == GLOBALS.Tiles[_tileCategoryIndex].Length - 1) _tileScrollIndex += GLOBALS.Tiles[_tileCategoryIndex].Length - categoriesPageSize;
                }
            }
            else
            {
                if (_tileCategoryFocus)
                {
                    ToPreviousMaterialCategory(categoriesPageSize);
                }
                else
                {
                    if (_materialIndex == (_materialScrollIndex) && _materialIndex != 1) _materialScrollIndex--;
                    _materialIndex--;
                    if (_materialIndex < 0) _materialIndex = GLOBALS.Materials[_materialCategoryIndex].Length - 1;
                    if (_materialIndex == GLOBALS.Materials[_materialCategoryIndex].Length - 1) _materialScrollIndex += GLOBALS.Materials[_materialCategoryIndex].Length - categoriesPageSize;
                }
            }
        }

        if (_shortcuts.TileMaterialSwitch.Check(ctrl, shift, alt)) _materialTileSwitch = !_materialTileSwitch;

        if (_shortcuts.HoveredItemInfo.Check(ctrl, shift, alt)) GLOBALS.Settings.TileEditor.HoveredTileInfo = !GLOBALS.Settings.TileEditor.HoveredTileInfo;

        if (_shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) _showLayer1Tiles = !_showLayer1Tiles;
        if (_shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) _showLayer2Tiles = !_showLayer2Tiles;
        if (_shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) _showLayer3Tiles = !_showLayer3Tiles;
        if (_shortcuts.ToggleLayer1.Check(ctrl, shift, alt)) _showTileLayer1 = !_showTileLayer1;
        if (_shortcuts.ToggleLayer2.Check(ctrl, shift, alt)) _showTileLayer2 = !_showTileLayer2;
        if (_shortcuts.ToggleLayer3.Check(ctrl, shift, alt)) _showTileLayer3 = !_showTileLayer3;

        if (_shortcuts.TogglePathsView.Check(ctrl, shift, alt)) _highlightPaths = !_highlightPaths;

        var currentTilePreviewColor = GLOBALS.TileCategories[_tileCategoryIndex].Item2;
        var currentTileTexture = GLOBALS.Textures.Tiles[_tileCategoryIndex][_tileIndex];

        #endregion

        BeginDrawing();

        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? Color.Black 
            : new Color(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            #region Matrix
            
            DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale,
                GLOBALS.Settings.GeneralSettings.DarkTheme
                    ? new Color(100, 100, 100, 255)
                    : Color.White);

            #region TileEditorLayer3
            // Draw geos first
            if (_showTileLayer3)
            {
                if (GLOBALS.Layer == 2) DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, Color.Gray with { A = 120 });
                Printers.DrawGeoLayer(
                    2, 
                    GLOBALS.Scale, 
                    false, 
                    Color.Black
                );

                // then draw the tiles

                if (_showLayer3Tiles)
                {
                    Printers.DrawTileLayer(
                        2, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles
                    );
                }
            }
            #endregion

            #region TileEditorLayer2
            if (_showTileLayer2)
            {
                if (GLOBALS.Layer != 2) DrawRectangle(
                    0, 
                    0, 
                    GLOBALS.Level.Width * GLOBALS.Scale, 
                    GLOBALS.Level.Height * GLOBALS.Scale, 
                    Color.Gray with { A = 130 });

                Printers.DrawGeoLayer(
                    1, 
                    GLOBALS.Scale, 
                    false, 
                    GLOBALS.Layer < 2
                        ? Color.Black 
                        : Color.Black with { A = 80 }
                );

                // Draw layer 2 tiles

                if (_showLayer2Tiles)
                {
                    Printers.DrawTileLayer(
                        1, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer < 2 ? 255 : 80)
                    );
                }
            }
            #endregion

            #region TileEditorLayer1
            if (_showTileLayer1)
            {
                if (GLOBALS.Layer != 1 && GLOBALS.Layer!= 2) 
                    DrawRectangle(
                        0, 
                    0, 
                        GLOBALS.Level.Width * GLOBALS.Scale, 
                        GLOBALS.Level.Height * GLOBALS.Scale, 
                        Color.Gray with { A = 130 }
                    );

                Printers.DrawGeoLayer(
                    0, 
                    GLOBALS.Scale, 
                    false, 
                    GLOBALS.Layer == 0
                        ? Color.Black 
                        : Color.Black with { A = 80 }
                );

                // Draw layer 1 tiles

                if (_showLayer1Tiles)
                {
                    Printers.DrawTileLayer(
                        0, 
                        GLOBALS.Scale, 
                        false, 
                        !GLOBALS.Settings.TileEditor.UseTextures,
                        GLOBALS.Settings.TileEditor.TintedTiles,
                        (byte)(GLOBALS.Layer == 0 ? 255 : 80)
                    );
                }
            }
            #endregion
            
            // Dark Theme

            if (GLOBALS.Settings.GeneralSettings.DarkTheme)
            {
                DrawRectangleLines(0, 0, GLOBALS.Level.Width*GLOBALS.Scale, GLOBALS.Level.Height*GLOBALS.Scale, Color.White);
            }
            
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

            Printers.DrawGeoLayer(0, GLOBALS.Scale, false, Color.White, false, GLOBALS.GeoPathsFilter);
            
            #endregion

            if (inMatrixBounds)
            {
                switch (_copyMode)
                {
                    case 0: // none
                    {
                        var hoverRect = GetSingleTileRect(tileMatrixX, tileMatrixY, GLOBALS.Layer, GLOBALS.Scale);
                        
                        DrawRectangleLinesEx(
                            hoverRect,
                            2f,
                            Color.White
                        );
                        
                        if (_materialTileSwitch)
                        {
                            var color = isTileLegal ? currentTilePreviewColor : Color.Red;
                            Printers.DrawTilePreview(ref currentTileInit, ref currentTileTexture, ref color, (tileMatrixX, tileMatrixY), GLOBALS.Scale);

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
            
            BeginTextureMode(GLOBALS.Textures.TileSpecs);
            {
                ClearBackground(Color.Gray);

                if (_tileSpecDisplayMode)
                {
                    ref var texture = ref GLOBALS.Textures.Tiles[_tileCategoryIndex][_tileIndex];
                    ref var color = ref GLOBALS.TileCategories[_tileCategoryIndex].Item2;
                    
                    var newWholeScale = Math.Min(200 / currentTileInit.Size.Item1, 200 /
                        currentTileInit.Size.Item2);
                    
                    Printers.DrawTileAsPropColored(
                        ref texture, 
                        ref currentTileInit, 
                        new PropQuads(
                            new Vector2(0,0), 
                            new Vector2(currentTileInit.Size.Item1*newWholeScale, 0), 
                            new Vector2(currentTileInit.Size.Item1*newWholeScale, currentTileInit.Size.Item2*newWholeScale), 
                            new Vector2(0, currentTileInit.Size.Item2*newWholeScale)),
                        color);
                }
                else
                {
                    var (tileWidth, tileHeight) = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Size;

                    var newWholeScale = Math.Min(200 / tileWidth * 20, 200 / tileHeight * 20);
                    var newCellScale = newWholeScale / 20;

                    int[] specs;
                    int[] specs2;
                    
                    try
                    {
                        specs = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs;
                        specs2 = GLOBALS.Tiles[_tileCategoryIndex][_tileIndex].Specs2;
                    }
                    catch (IndexOutOfRangeException ie)
                    {
                        _logger.Fatal($"Failed to fetch tile specs(2) from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} or {_tileIndex} were out of bounds");
                        throw new IndexOutOfRangeException(innerException: ie,
                            message:
                            $"Failed to fetch tile specs(2) from {nameof(GLOBALS.Tiles)} (L:{GLOBALS.Tiles.Length}): {nameof(_tileCategoryIndex)} or {_tileIndex} were out of bounds");
                    }
                    
                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            var specsIndex = (x * tileHeight) + y;
                            var spec = specs[specsIndex];
                            var spec2 = specs2.Length > 0 ? specs2[specsIndex] : -1;
                            var specOrigin = new Vector2(
                                (specsRect.Width - newCellScale * tileWidth) / 2f + x * newCellScale, 
                                y * newCellScale
                            );

                            if (spec is >= 0 and < 9 and not 8)
                            {
                                Printers.DrawTileSpec(
                                    spec,
                                    specOrigin,
                                    newCellScale,
                                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1
                                );
                                
                                Printers.DrawTileSpec(
                                    spec2,
                                    specOrigin,
                                    newCellScale,
                                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 with { A = 100 } // this can be optimized
                                );
                            }
                        }
                    }

                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            DrawRectangleLinesEx(
                                new(
                                    (specsRect.Width - newCellScale * tileWidth) / 2f + x * newCellScale,
                                    y * newCellScale,
                                    newCellScale,
                                    newCellScale
                                ),
                                Math.Max(tileWidth, tileHeight) switch
                                {
                                    > 25 => 0.3f,
                                    > 10 => 0.5f,
                                    _ => 1f
                                },
                                new(255, 255, 255, 255)
                            );
                        }
                    }
                }
                
            }
            EndTextureMode();

            rlImGui.Begin();

            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Navigation
            
            var navWindowRect = Printers.ImGui.NavigationWindow();

            _isNavigationWinHovered = CheckCollisionPointRec(GetMousePosition(), navWindowRect with
            {
                X = navWindowRect.X - 5, Width = navWindowRect.Width + 10
            });
                
            if (_isNavigationWinHovered && IsMouseButtonDown(MouseButton.Left))
            {
                _isNavigationWinDragged = true;
            }
            else if (_isNavigationWinDragged && IsMouseButtonReleased(MouseButton.Left))
            {
                _isNavigationWinDragged = false;
            }
            
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
                if (ImGui.Button(_materialTileSwitch ? "Switch to materials" : "Switch to tiles"))
                    _materialTileSwitch = !_materialTileSwitch;

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
                
                var halfWidth = ImGui.GetContentRegionAvail().X / 2f - ImGui.GetStyle().ItemSpacing.X / 2f;
                var boxHeight = ImGui.GetContentRegionAvail().Y;
                
                if (ImGui.BeginListBox("##Groups", new Vector2(halfWidth, boxHeight)))
                {
                    if (_materialTileSwitch)
                    {
                        for (var categoryIndex = 0; categoryIndex < _tileCategoryNames.Length; categoryIndex++)
                        {
                            ref var category = ref GLOBALS.TileCategories[categoryIndex];
                            
                            
                            var selected = ImGui.Selectable(
                                category.Item1, 
                                _tileCategoryIndex == categoryIndex);
                            
                            if (selected)
                            {
                                _tileCategoryIndex = categoryIndex;
                                _tileIndex = 0;
                            }
                        }
                    }
                    else
                    {
                        for (var category = 0; category < _materialCategoryNames.Length; category++)
                        {
                            var selected = ImGui.Selectable(_materialCategoryNames[category], _materialCategoryIndex == category);
                            if (selected)
                            {
                                _materialCategoryIndex = category;
                                _materialIndex = 0;
                            }
                        }
                    }
                
                    ImGui.EndListBox();
                }
                
                ImGui.SameLine();
                if (ImGui.BeginListBox("##Tiles", new Vector2(halfWidth, boxHeight)))
                {
                    if (_materialTileSwitch)
                    {
                        for (var tile = 0; tile < GLOBALS.Tiles[_tileCategoryIndex].Length; tile++)
                        {
                            var selected = ImGui.Selectable(
                                GLOBALS.Tiles[_tileCategoryIndex][tile].Name, 
                                _tileIndex == tile
                            );

                            if (selected) _tileIndex = tile;
                        }
                    }
                    else
                    {
                        for (var materialIndex = 0; materialIndex < GLOBALS.Materials[_materialCategoryIndex].Length; materialIndex++)
                        {
                            ref var material = ref GLOBALS.Materials[_materialCategoryIndex][materialIndex];
                            
                            var selected = ImGui.Selectable(
                                material.Item1,
                                _materialIndex == materialIndex
                            );

                            if (selected) _materialIndex = materialIndex;
                        }
                    }
                    ImGui.EndListBox();
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
                var displayClicked = ImGui.Button(_tileSpecDisplayMode ? "Texture" : "Geometry");

                if (displayClicked) _tileSpecDisplayMode = !_tileSpecDisplayMode;
                
                // rlImGui.ImageRenderTexture(GLOBALS.Textures.TileSpecs);
                rlImGui.ImageRenderTextureFit(GLOBALS.Textures.TileSpecs);
                
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
                var texture = GLOBALS.Settings.TileEditor.UseTextures;
                var tinted = GLOBALS.Settings.TileEditor.TintedTiles;
                
                var cycleDisplaySelected = ImGui.Button($"Textures: {(texture, tinted) switch { (false, false) => "Preview", (true, false) => "Raw Texture", (true, true) => "Tinted Texture", _ => "Preview" }}");

                if (cycleDisplaySelected)
                {
                    (texture, tinted) = (texture, tinted) switch
                    {
                        (false, false) => (true, false),
                        (true, false) => (true, true),
                        _ => (false, false)
                    };
                }

                GLOBALS.Settings.TileEditor.UseTextures = texture;
                GLOBALS.Settings.TileEditor.TintedTiles = tinted;
                
                //
                
                ImGui.Checkbox("Deep Tile Copy", ref _deepTileCopy);

                var hoveredInfo = GLOBALS.Settings.TileEditor.HoveredTileInfo;
                ImGui.Checkbox("Hovered Item Info", ref hoveredInfo);
                if (GLOBALS.Settings.TileEditor.HoveredTileInfo != hoveredInfo) 
                    GLOBALS.Settings.TileEditor.HoveredTileInfo = hoveredInfo;
                
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
                                h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name,
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
                                h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name,
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
                                        ? (h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name)
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
                                        ? (h.CategoryPostition is (-1, -1, _) ? $"Undefined Tile \"{h.CategoryPostition.Name}\"" : h.CategoryPostition.Name)
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