using static Raylib_cs.Raylib;
using ImGuiNET;
using Leditor.Data.Tiles;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using SixLabors.ImageSharp.Memory;

namespace Leditor.Pages;

#nullable enable

internal class TileViewerPage : EditorPage {
    public override void Dispose()
    {
        if (Disposed) return;

        Disposed = true;

        _previewTooltipRT.Dispose();
        _mainCanvasRT.Dispose();
        _tileSpecsPanelRT.Dispose();
    }

    internal TileViewerPage() {
        
    }

    #region Resources
    private RL.Managed.RenderTexture2D _previewTooltipRT = new(0, 0);
    private RL.Managed.RenderTexture2D _mainCanvasRT = new(0, 0);
    private RL.Managed.RenderTexture2D _tileSpecsPanelRT = new(0, 0);
    #endregion

    #region Fields
    private Camera2D _camera = new() { Zoom = 1 };

    private string _searchText = "";
    private bool _isSearchContorlActive;

    private record SearchResult(
        (string name, int originalIndex)[] Categories,
        (TileDefinition tile, int originalIndex)[][] Tiles
    );

    private SearchResult? _searchResult;


    private TileDefinition? _hoveredTile;

    private TileDefinition[]? _currentCategoryTiles;
    private TileDefinition? _currentTile;

    private enum Coloring { Untinted, Tinted, Palette, Custom }
    private Coloring _coloring = Coloring.Tinted;

    private Vector3 _customColor = new(170, 170, 170);


    private int _categoryIndex = -1;
    private int _tileIndex = -1;

    private int _hoveredIndex = -1;

    private int _searchCategoryInedx = -1;
    private int _searchTileIndex = -1;


    private bool _isNavbarHovered;
    private bool _isTilesWinHovered;
    private bool _isSpecsWinHovered;
    private bool _isPropertiesWinHovered;
    private bool _isToolsWinHovered;
    private bool _isColorsWinHovered;

    private bool _clickLock;

    private bool _shouldRedrawTile;
    private bool _shouldRedrawTooltip;
    private bool _shouldRedrawSpecs;

    private bool _initialSetup;

    private int _selectedPaletteIndex;

    private int _tileDepth;

    private bool _showTileHead;

    #endregion

    #region Methods

    private void InitialSetup() {
        if (_initialSetup) return;

        if ((GLOBALS.TileDex?.OrderedCategoryNames.Length ?? 0) > 0) {
            _categoryIndex = 0;

            _ = GLOBALS.TileDex!.TryGetTilesOfCategory(GLOBALS.TileDex!.OrderedCategoryNames[0], out _currentCategoryTiles);
        }

        _initialSetup = true;
    }

    private void SearchTextUpdated() {
        _searchResult = null;

        if (GLOBALS.TileDex is null) return;
        if (string.IsNullOrEmpty(_searchText)) return;


        List<(string, int)> categories = [];
        List<(TileDefinition, int)[]> tiles = [];

        for (var c = 0; c < GLOBALS.TileDex.OrderedCategoryNames.Length; c++) {
            var currentCategory = GLOBALS.TileDex.OrderedCategoryNames[c];
            var currentTiles = GLOBALS.TileDex.GetTilesOfCategory(currentCategory);
            
            List<(TileDefinition, int)> categoryTiles = [];

            for (var t = 0; t < currentTiles.Length; t++) {
                if (currentTiles[t].Name.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase)) {
                    categoryTiles.Add((currentTiles[t], t));
                }
            }

            if (categoryTiles is not []) {
                categories.Add((currentCategory, c));
                tiles.Add([..categoryTiles]);
            }
        }

        _searchResult = new SearchResult([..categories], [..tiles]);
    }

    private void DrawTile() {
        if (_currentTile is null) return;

        var (width, height) = _currentTile.Size;
        var buffer = _currentTile.BufferTiles;

        var bufferedWidth = width + buffer * 2;
        var bufferedHeight = height + buffer * 2;

        var scaledWidth = bufferedWidth* 20;
        var scaledHeight = bufferedHeight * 20;

        _mainCanvasRT.Dispose();
        _mainCanvasRT = new RL.Managed.RenderTexture2D(scaledWidth + 8, scaledHeight + 8);

        BeginTextureMode(_mainCanvasRT);

        ClearBackground(Color.Gray);

        // Background

        DrawRectangleLinesEx(new Rectangle(0, 0, scaledWidth + 8, scaledHeight + 8), 4f, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);

        for (var w = 0; w < bufferedWidth; w++) {
            for (var h = 0; h < bufferedHeight; h++) {
                DrawRectangle(4 + w * 20, 4 + h * 20, 20, 20, (w + h) % 2 == 0 ? Color.Gray : Color.LightGray);
            }
        }

        // Tile

        var origin = new Vector2(4, 4);

        var quad = new PropQuad(
            origin,
            origin + new Vector2(scaledWidth, 0),
            origin + new Vector2(scaledWidth, scaledHeight),
            origin + new Vector2(0, scaledHeight)
        );

        switch (_coloring) {
            case Coloring.Untinted:
            Printers.DrawTileAsProp(_currentTile, quad);
            break;

            case Coloring.Tinted:
            {
                var color = GLOBALS.TileDex!.GetTileColor(_currentTile);
                Printers.DrawTileAsPropColored(_currentTile, quad, color);
            }
            break;

            case Coloring.Palette:
            Printers.DrawTileWithPalette(_currentTile, GLOBALS.Textures.Palettes[_selectedPaletteIndex], quad, _tileDepth);
            break;

            case Coloring.Custom:
            Printers.DrawTileAsPropColored(_currentTile, quad, new Color((int)(_customColor.X * 255), (int)(_customColor.Y * 255), (int)(_customColor.Z * 255), 255));
            break;

            default:
            Printers.DrawTileAsPropColored(_currentTile, quad, Color.SkyBlue);
            break;
        }

        if (_showTileHead) {
            var headPos = new Vector2(6, 6) + (Vector2.One * buffer + Utils.GetTileHeadOrigin(_currentTile)) * 20;

            var categoryColor = GLOBALS.TileDex!.GetTileColor(_currentTile);

            var color = _coloring switch {
                Coloring.Tinted => new Color(255 - categoryColor.R, 255 - categoryColor.G, 255 - categoryColor.B, 255),
                Coloring.Custom => new Color(255 - (int)(_customColor.X)*255, 255 - (int)(_customColor.Y)*255, 255 - (int)(_customColor.Z)*255, 255),
                _ => Color.White
            };

            DrawRectangleLinesEx(new Rectangle(headPos.X - 3, headPos.Y - 3, 23, 23), 3, color);
        }

        EndTextureMode();
    }

    private void DrawTooltip() {
        if (_hoveredTile is null) return;

        _previewTooltipRT.Dispose();

        if (GLOBALS.Settings.TileEditor.UseTexturesInTooltip) {
            var (width, height) = _hoveredTile.Size;
            var buffer = _hoveredTile.BufferTiles * 2;

            var totalWidth = (width + buffer) * 20;
            var totalHeight = (height + buffer) * 20;

            _previewTooltipRT = new RL.Managed.RenderTexture2D(totalWidth, totalHeight);
            
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
            _previewTooltipRT = new RL.Managed.RenderTexture2D(
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
    }

    private void DrawSpecs() {
        if (_currentTile is null) return;

        var (width, height) = _currentTile.Size;
        
        const int scale = 20;
        
        _tileSpecsPanelRT.Dispose();

        _tileSpecsPanelRT = new RL.Managed.RenderTexture2D(scale*width + 10, scale*height + 10);
        
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
    #endregion

    //

    public override void Draw()
    {
        if (!_initialSetup) InitialSetup();

        var settings = GLOBALS.Settings.GeneralSettings;

        var isWinBusy = _isNavbarHovered || 
            _isTilesWinHovered || 
            _isSearchContorlActive ||
            _isSpecsWinHovered ||
            _isPropertiesWinHovered ||
            _isToolsWinHovered ||
            _isColorsWinHovered;

        if (!isWinBusy || _clickLock) {
            
            // Drag
            if (IsMouseButtonDown(MouseButton.Middle))
            {
                _clickLock = true;
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsMouseButtonReleased(MouseButton.Middle)) _clickLock = false;

            // Zoom
            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                _camera.Offset = GetMousePosition();
                _camera.Target = mouseWorldPosition;
                _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
                if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
            }
        }

        #region Drawing
        BeginDrawing();
        {
            ClearBackground(settings.DarkTheme ? Color.Black : Color.Gray);

            if (_shouldRedrawTile) {

                DrawTile();
                _shouldRedrawTile = false;
            }

            if (_shouldRedrawTooltip) {
                DrawTooltip();
                _shouldRedrawTooltip = false;
            }

            if (_shouldRedrawSpecs) {
                DrawSpecs();

                _shouldRedrawSpecs = false;
            }

            BeginMode2D(_camera);
            {
                var shader = GLOBALS.Shaders.VFlip;

                BeginShaderMode(shader);
                
                SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _mainCanvasRT.Raw.Texture);

                DrawTexture(_mainCanvasRT.Raw.Texture, 0, 20, Color.White);
                EndShaderMode();
            }
            EndMode2D();

            #region ImGui
            rlImGui_cs.rlImGui.Begin();
            {
                ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                // Navigation bar
                
                if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);

                var tilesWinOpened = ImGui.Begin("Tiles##TileViewTilesWindow");

                var tilesWinPos = ImGui.GetWindowPos();
                var tilesWinSpace = ImGui.GetWindowSize();

                _isTilesWinHovered = CheckCollisionPointRec(GetMousePosition(), new(tilesWinPos.X - 5, tilesWinPos.Y, tilesWinSpace.X + 10, tilesWinSpace.Y));

                if (tilesWinOpened) {
                    if (GLOBALS.TileDex is not null) {

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        var textUpdated = ImGui.InputTextWithHint("##Search", "Search..", ref _searchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll);

                        _isSearchContorlActive = ImGui.IsItemActive();

                        var availableSpace = ImGui.GetContentRegionAvail();

                        if (ImGui.BeginListBox("##Categories", availableSpace with { X = availableSpace.X/2f })) {
                            var drawList = ImGui.GetWindowDrawList();
                            var textHeight = ImGui.GetTextLineHeight();

                            if (_searchResult is null) {
                                for (var c = 0; c < GLOBALS.TileDex.OrderedCategoryNames.Length; c++) {
                                    var categoryName = GLOBALS.TileDex.OrderedCategoryNames[c];
                                    var color = GLOBALS.TileDex.GetCategoryColor(categoryName);
                                    Vector4 colorVec = color; 

                                    var cursor = ImGui.GetCursorScreenPos();
                                    drawList.AddRectFilled(
                                        p_min: cursor,
                                        p_max: cursor + new Vector2(10f, textHeight),
                                        ImGui.ColorConvertFloat4ToU32((colorVec / 255f))
                                    );

                                    var selected = ImGui.Selectable($"  {categoryName}", _categoryIndex == c);

                                    if (selected) {
                                        _categoryIndex = c;

                                        var tilesFound = GLOBALS.TileDex.TryGetTilesOfCategory(categoryName, out _currentCategoryTiles);

                                        if (tilesFound) {
                                            _tileIndex = -1;
                                            _currentTile = null;
                                        }
                                    }
                                }
                            } else {
                                for (var c = 0; c < _searchResult.Categories.Length; c++) {
                                    var selected = ImGui.Selectable(_searchResult.Categories[c].name, _searchCategoryInedx == c);

                                    if (selected) {
                                        _searchCategoryInedx = c;
                                        _categoryIndex = _searchResult.Categories[c].originalIndex;
                                        _searchTileIndex = -1;
                                        _tileIndex = -1;
                                    }
                                }
                            }


                            ImGui.EndListBox();
                        }

                        ImGui.SameLine();

                        if (ImGui.BeginListBox("##Tiles", availableSpace with { X = availableSpace.X/2f })) {
                            if (_currentCategoryTiles is not null) {

                                if (_searchResult is null) {
                                    for (var t = 0; t < _currentCategoryTiles.Length; t++) {
                                        var tile = _currentCategoryTiles[t];

                                        var selected = ImGui.Selectable(tile.Name, _tileIndex == t);

                                        if (ImGui.IsItemHovered()) {
                                            if (_hoveredIndex != t) _shouldRedrawTooltip = true;
                                            _hoveredIndex = t;
                                            
                                            _hoveredTile = _currentCategoryTiles[t];

                                            ImGui.BeginTooltip();
                                            if (_hoveredTile.Texture.Id == 0) {
                                                ImGui.Text("Texture not found");
                                            } else {
                                                rlImGui_cs.rlImGui.ImageRenderTexture(_previewTooltipRT);
                                            }
                                            ImGui.EndTooltip();
                                        }

                                        if (selected) {
                                            _shouldRedrawTile = true;
                                            _shouldRedrawSpecs = true;
                                            _currentTile = tile;
                                            _tileIndex = t;
                                        }
                                    }
                                } else {
                                    if (_searchCategoryInedx >= 0 && _searchCategoryInedx < _searchResult.Tiles.Length) {
                                        for (var t = 0; t < _searchResult.Tiles[_searchCategoryInedx].Length; t++) {
                                            var selected = ImGui.Selectable(_searchResult.Tiles[_searchCategoryInedx][t].tile.Name, _searchTileIndex == t);

                                            if (selected) {
                                                _searchTileIndex = t;
                                                _tileIndex = _searchResult.Tiles[_searchCategoryInedx][t].originalIndex;
                                                _currentTile = _searchResult.Tiles[_searchCategoryInedx][t].tile;
                                                _shouldRedrawTile = true;
                                            }
                                        }
                                    }

                                }
                                
                            }

                            ImGui.EndListBox();
                        }

                        
                        if (textUpdated) {
                            SearchTextUpdated();
                        }
                    }

                    ImGui.End();
                }

                // Properties

                var propertiesWinOpened = ImGui.Begin("Properties##TileViewerPropertiesWindow");

                var propertiesWinPos = ImGui.GetWindowPos();
                var propertiesWinSpace = ImGui.GetWindowSize();

                _isPropertiesWinHovered = CheckCollisionPointRec(GetMousePosition(), new(propertiesWinPos.X - 5, propertiesWinPos.Y, propertiesWinSpace.X + 10, propertiesWinSpace.Y));

                if (propertiesWinOpened) {
                    if (_currentTile is not null ) {

                        var bufferSpace = _currentTile.BufferTiles;


                        if (ImGui.BeginTable("##PropertiesTable", 2, ImGuiTableFlags.RowBg)) {
                            ImGui.TableSetupColumn("Property");
                            ImGui.TableSetupColumn("Value");

                            ImGui.TableHeadersRow();

                            // Name

                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Name");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.Name}");

                            // Type

                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Type");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.Type}");

                            // Layers

                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Layers");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.Repeat.Sum() + 1}");
                            
                            // Buffer Space

                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Buffer Space");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.BufferTiles}");

                            ImGui.EndTable();
                        }

                        ImGui.SeparatorText("Dimensions");


                        if (ImGui.BeginTable("##SpecsTable", 3, ImGuiTableFlags.RowBg)) {
                            ImGui.TableSetupColumn("Dimension");
                            ImGui.TableSetupColumn("Width");
                            ImGui.TableSetupColumn("Height");

                            ImGui.TableHeadersRow();

                            // Raw

                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("Raw");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.Size.Width}");

                            ImGui.TableSetColumnIndex(2);
                            ImGui.Text($"{_currentTile.Size.Height}");
                            
                            // With Buffer Space
                            
                            ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text("With Buffers");

                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text($"{_currentTile.Size.Width + bufferSpace * 2}");

                            ImGui.TableSetColumnIndex(2);
                            ImGui.Text($"{_currentTile.Size.Height + bufferSpace * 2}");
                            
                            ImGui.EndTable();
                        }
                    }
                    ImGui.End();
                }

                // Tools

                var toolsWinOpened = ImGui.Begin("Tools##TilesViewerToolsWindow");

                var toolsWinPos = ImGui.GetWindowPos();
                var toolsWinSpace = ImGui.GetWindowSize();

                _isToolsWinHovered = CheckCollisionPointRec(GetMousePosition(), new(toolsWinPos.X - 5, toolsWinPos.Y, toolsWinSpace.X + 10, toolsWinSpace.Y));
                
                if (toolsWinOpened) {
                    if (_currentTile is not null) {
                        if (ImGui.Checkbox("Head", ref _showTileHead)) {
                            _shouldRedrawTile = true;
                        }
                    }
                    
                    ImGui.End();
                }

                // Colors

                var colorsWinOpened = ImGui.Begin("Colors##TilesViewerColorsWindow");

                var colorsWinPos = ImGui.GetWindowPos();
                var colorsWinSpace = ImGui.GetWindowSize();

                _isToolsWinHovered = CheckCollisionPointRec(GetMousePosition(), new(colorsWinPos.X - 5, colorsWinPos.Y, colorsWinSpace.X + 10, colorsWinSpace.Y));
                
                if (colorsWinOpened) {
                    if (_currentTile is not null) {
                        ImGui.SeparatorText("Colors");

                        if (ImGui.RadioButton("Untinted", _coloring == Coloring.Untinted)) {
                            _coloring = Coloring.Untinted;
                            _shouldRedrawTile = true;
                        }

                        if (ImGui.RadioButton("Category Tint", _coloring == Coloring.Tinted)) {
                            _coloring = Coloring.Tinted;
                            _shouldRedrawTile = true;
                        }


                        if (ImGui.RadioButton("Palette", _coloring == Coloring.Palette)) {
                            _coloring = Coloring.Palette;
                            _shouldRedrawTile = true;
                        }

                        if (ImGui.BeginListBox("##Palettes", new Vector2(ImGui.GetContentRegionAvail().X, 100))) {

                            // var drawList = ImGui.GetWindowDrawList();
                            
                            for (var p = 0; p < GLOBALS.Textures.Palettes.Length; p++) {
                                

                                // var cursor = ImGui.GetCursorScreenPos();
                                // drawList.AddImage(
                                //     (nint)GLOBALS.Textures.Palettes[p].Id,
                                //     p_min: cursor,
                                //     p_max: cursor + new Vector2(320, 160)
                                // );


                                var selected = ImGui.Selectable($"{p}", p == _selectedPaletteIndex);

                                if (ImGui.IsItemHovered()) {
                                    ImGui.BeginTooltip();
                                    rlImGui_cs.rlImGui.ImageSize(GLOBALS.Textures.Palettes[p], new Vector2(320, 160));
                                    ImGui.EndTooltip();
                                }
                                
                                if (selected) {
                                    _coloring = Coloring.Palette;
                                    _selectedPaletteIndex = p;
                                    _shouldRedrawTile = true;
                                }
                            }
                            
                            ImGui.EndListBox();
                        }

                        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                        if (ImGui.SliderInt("Depth", ref _tileDepth, 0, 29)) {
                            _coloring = Coloring.Palette;
                            _shouldRedrawTile = true;
                        }


                        ImGui.Spacing();


                        if (ImGui.RadioButton("Custom", _coloring == Coloring.Custom)) {
                            _coloring = Coloring.Custom;
                            _shouldRedrawTile = true;
                        }

                        
                        ImGui.Spacing();


                        if (ImGui.ColorPicker3("##RGB", ref _customColor)) {
                            _coloring = Coloring.Custom;
                            _shouldRedrawTile = true;
                        }
                    }
                    
                    ImGui.End();
                }

                // Specs
            
                var specsWinOpened = ImGui.Begin("Specs##TileViewerSpecsWindow");

                var specsWinPos = ImGui.GetWindowPos();
                var specsWinSpace = ImGui.GetWindowSize();

                _isSpecsWinHovered = CheckCollisionPointRec(GetMousePosition(), new(specsWinPos.X - 5, specsWinPos.Y, specsWinSpace.X + 10, specsWinSpace.Y));
                
                if (specsWinOpened) {
                    rlImGui_cs.rlImGui.ImageRenderTextureFit(_tileSpecsPanelRT);
                    
                    ImGui.End();
                }
            }
            rlImGui_cs.rlImGui.End();
            #endregion
        }
        EndDrawing();
        #endregion
    }
}