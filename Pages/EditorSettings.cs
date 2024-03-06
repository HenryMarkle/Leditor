using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;

namespace Leditor;

public class SettingsPage : IPage
{
    private readonly Serilog.Core.Logger _logger;

    private int _categoryScrollIndex;
    private int _activeCategory;

    private int _shortcutCategoryScrollIndex;
    private int _shortcutActiveCategory;

    private Vector2 _shortcutsOldGeoScrollPanelScroll;
    private Vector2 _shortcutsNewGeoScrollPanelScroll;
    private Vector2 _shortcutsTileScrollPanelScroll;
    private Vector2 _shortcutsCameraScrollPanelScroll;
    private Vector2 _shortcutsLightScrollPanelScroll;
    private Vector2 _shortcutsEffectsScrollPanelScroll;
    private Vector2 _shortcutsPropsScrollPanelScroll;
    private Vector2 _shortcutsGlobalScrollPanelScroll;

    private bool _assigningShortcut;

    #nullable enable
    private KeyboardShortcut? _shortcutToAssign;
    private MouseShortcut? _mouseShortcutToAssign;
    #nullable disable
    
    private byte _colorPickerLayer = 2;

    private string[] _shortcutCategories = [
        "Global",
        "Old Geometry Editor",
        "New Geometry Editor",
        "Tile Editor",
        "Camera Editor",
        "Light Editor",
        "Effects Editor",
        "Props Editor"
    ];
   

    public SettingsPage(Serilog.Core.Logger logger)
    {
        _logger = logger;
    }
    
    public void Draw()
    {
        GLOBALS.PreviousPage = 9;
        
        var width = GetScreenWidth();
        var height = GetScreenHeight();
        
        var shortcutsScrollPanelRect = new Rectangle(420, 91, width - 460, height - 130);
        
        var panelRect = new Rectangle(20, 20, width - 40, height - 40);
        var categoryRect = new Rectangle(30, 60, 200, height - 200);
        var contentRect = new Rectangle(231, 60, width - 50, height - 90);

        var subPanelX = (int)(categoryRect.x + categoryRect.width + 10);
        var subPanelY = (int)(categoryRect.y + 60);

        #region Shortcuts
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
        
        if (!_assigningShortcut)
        {
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage.Check(ctrl, shift, alt)) GLOBALS.Page = 1;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 2;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 3;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 4;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 5;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.NewFlag = false;
                GLOBALS.Page = 6;
                _logger.Debug("go from GLOBALS.Page 2 to GLOBALS.Page 6");
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 7;
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt)) GLOBALS.Page = 8;
            // if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt)) GLOBALS.Page = 9;
        }
        #endregion
        
        BeginDrawing();
        
        ClearBackground(GRAY);

        rlImGui.Begin();

        if (ImGui.Begin("Settings##EditorSettings", ImGuiWindowFlags.NoCollapse))
        {
            ImGui.Columns(2);
            ImGui.SetColumnWidth(0, 200);

            var col1Space = ImGui.GetContentRegionAvail();

            if (ImGui.BeginListBox("##SettingsCategories", col1Space with { Y = col1Space.Y - 60 }))
            {
                for (var category = 0; category < _shortcutCategories.Length; category++)
                {
                    var selected = ImGui.Selectable(_shortcutCategories[category], category == _shortcutActiveCategory);

                    if (selected && !_assigningShortcut) _shortcutActiveCategory = category;
                }
                ImGui.EndListBox();
            }

            var resetSelected = ImGui.Button("Reset Shortcuts", col1Space with { Y = 20 });
            var saveSelected = ImGui.Button("Save Shortcuts", col1Space with { Y = 20 });

            if (resetSelected)
            {
                _assigningShortcut = false;
                _shortcutToAssign = null;
                _mouseShortcutToAssign = null;
               
                GLOBALS.Settings = new Settings(
                    new GeneralSettings(),
                    new Shortcuts(
                        new GlobalShortcuts(),
                        new GeoShortcuts(),
                        new ExperimentalGeoShortcuts(),
                        new TileShortcuts(),
                        new CameraShortcuts(),
                        new LightShortcuts(),
                        new EffectsShortcuts(),
                        new PropsShortcuts()
                    ),
                    new Misc(splashScreen:false, tileImageScansPerFrame: 100),
                    new GeoEditor(
                        new LayerColors(
                            layer1: new ConColor(0, 0, 0, 255),
                            layer2: new ConColor(0, 255, 0, 50),
                            layer3: new ConColor(255, 0, 0, 50)
                        ),
                        new ConColor(0, 0, 255, 70)
                    ),
                    new TileEditor(),
                    new LightEditor(background: new ConColor(66, 108, 245, 255)),
                    new EffectsSettings(
                        effectColorLight:GREEN,
                        effectColorDark:new(214, 187, 9, 255)),
                    new PropEditor(),
                    new Experimental()
                );
            }

            if (saveSelected)
            {
                _assigningShortcut = false;
                _shortcutToAssign = null;
                _mouseShortcutToAssign = null;
               
                try
                {
                    File.WriteAllText(
                        GLOBALS.Paths.SettingsPath, 
                        JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions)
                    );
                }
                catch (Exception e)
                {
                    _logger.Error($"Failed to save settings: {e}");
                }
            }
            
            //
            ImGui.NextColumn();
            //

            var col2Space = ImGui.GetContentRegionAvail();

            if (ImGui.BeginChild("##ShortcutsPanel", col2Space))
            {
                switch (_shortcutActiveCategory)
                {
                    case 0: // global
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.GlobalShortcuts = new GlobalShortcuts();
                        }
                        
                        ImGui.SeparatorText("Navigation");
                        
                        var assignToMainPage = ImGui.Button($"Main Page: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage}");
                        var assignToGeometryEditor = ImGui.Button($"Geometry: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor}"); 
                        var assignToTileEditor = ImGui.Button($"Tiles: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor}"); 
                        var assignToCamerasEditor = ImGui.Button($"Cameras: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor}");
                        var assignToLightEditor = ImGui.Button($"Light: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor}"); 
                        var assignToDimensionsEditor = ImGui.Button($"Dimensions: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor}");
                        var assignToEffectsEditor = ImGui.Button($"Effects: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor}");
                        var assignToPropsEditor = ImGui.Button($"Props: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor}");
                        var assignToSettings = ImGui.Button($"Shortcuts: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage}");

                        ImGui.SeparatorText("Quick Actions");
                        
                        var assignSave = ImGui.Button($"Save: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSave}");
                        var assignSaveAs = ImGui.Button($"Save As:{GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSaveAs}");
                        var assignRender = ImGui.Button($"Render: {GLOBALS.Settings.Shortcuts.GlobalShortcuts.Render}");

                        if (assignToMainPage) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage;
                        if (assignToGeometryEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor;
                        if (assignToTileEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor;
                        if (assignToCamerasEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor;
                        if (assignToLightEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor;
                        if (assignToDimensionsEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor;
                        if (assignToEffectsEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor;
                        if (assignToPropsEditor) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor;
                        if (assignToSettings) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage;
                        if (assignSave) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSave;
                        if (assignSaveAs) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSaveAs;
                        if (assignRender) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GlobalShortcuts.Render;
                    }
                        break;

                    case 1: // old geometry
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.GeoEditor = new GeoShortcuts();
                        }
                        
                        ImGui.SeparatorText("Tools Menu");
                        
                        var assignToLeftGeo = ImGui.Button($"To Left Geo: {GLOBALS.Settings.Shortcuts.GeoEditor.ToLeftGeo}");
                        var assignToTopGeo = ImGui.Button($"To Top Geo: {GLOBALS.Settings.Shortcuts.GeoEditor.ToTopGeo}");
                        var assignToRightGeo = ImGui.Button($"To Right Geo: {GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo}");
                        var assignToBottomGeo = ImGui.Button($"To Bottom Geo: {GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo}");

                        if (assignToLeftGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToLeftGeo;
                        if (assignToTopGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToTopGeo;
                        if (assignToRightGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo;
                        if (assignToBottomGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo;
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.GeoEditor.Draw}");
                        var assignPan = ImGui.Button($"Drag: {GLOBALS.Settings.Shortcuts.GeoEditor.DragLevel}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.DragLevel;
                        
                        ImGui.Separator();
                        
                        var assignCycleLayers = ImGui.Button($"Cycle Layers: {GLOBALS.Settings.Shortcuts.GeoEditor.CycleLayers}");
                        var assignToggleGrid = ImGui.Button($"Toggle Grid: {GLOBALS.Settings.Shortcuts.GeoEditor.ToggleGrid}"); 
                        var assignToggleCameras = ImGui.Button($"Show/Hide Cameras: {GLOBALS.Settings.Shortcuts.GeoEditor.ShowCameras}"); 
                        var assignUndo = ImGui.Button($"Undo: {GLOBALS.Settings.Shortcuts.GeoEditor.Undo}");
                        var assignRedo = ImGui.Button($"Redo: {GLOBALS.Settings.Shortcuts.GeoEditor.Redo}"); 
                        var assignAltDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.GeoEditor.AltDraw}");
                        var assignAltPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.GeoEditor.AltDrag}");

                        if (assignCycleLayers) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.CycleLayers;
                        if (assignToggleGrid) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToggleGrid;
                        if (assignToggleCameras) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ShowCameras;
                        if (assignUndo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Undo;
                        if (assignRedo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Redo;
                        if (assignAltDraw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.AltDraw;
                        if (assignAltPan) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.AltDrag;
                    }
                        break;

                    case 2: // new geometry
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts = new ExperimentalGeoShortcuts();
                        }
                        
                        ImGui.SeparatorText("Tools Menu");
                        
                        var assignToLeftGeo = ImGui.Button($"To Left Geo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToLeftGeo}");
                        var assignToTopGeo = ImGui.Button($"To Top Geo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToTopGeo}");
                        var assignToRightGeo = ImGui.Button($"To Right Geo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToRightGeo}");
                        var assignToBottomGeo = ImGui.Button($"To Bottom Geo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToBottomGeo}");

                        if (assignToLeftGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToLeftGeo;
                        if (assignToTopGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToTopGeo;
                        if (assignToRightGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToRightGeo;
                        if (assignToBottomGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToBottomGeo;
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Draw}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.DragLevel}");
                        var assignErase = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Erase}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Erase;
                        
                        ImGui.Separator();
                        
                        var assignCycleLayers = ImGui.Button($"Cycle Layers: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.CycleLayers}");
                        var assignToggleGrid = ImGui.Button($"Show/Hide Grid: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleGrid}"); 
                        var assignToggleCameras = ImGui.Button($"Show/Hide Cameras: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ShowCameras}"); 
                        var assignUndo = ImGui.Button($"Undo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Undo}");
                        var assignRedo = ImGui.Button($"Redo: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Redo}"); 
                        var assignAltDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDraw}");
                        var assignAltPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDragLevel}");
                        var assignToggleMultiselect = ImGui.Button($"Toggle Multi-Select: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMultiSelect}");
                        var assignEraseEverything = ImGui.Button($"Erase Everything (hold){GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.EraseEverything}");
                        var assignToggleTileVisibility = ImGui.Button($"Show/Hide Tiles: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleTileVisibility}");
                        var assignToggleMemoryLoadMode = ImGui.Button($"Toggle Copy: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryLoadMode}");
                        var assignToggleMemoryDumpMode = ImGui.Button($"Toggle Paste{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryDumbMode}");
                        var assignAltErase = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltErase}");

                        if (assignCycleLayers) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.CycleLayers;
                        if (assignToggleGrid) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleGrid;
                        if (assignToggleCameras) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ShowCameras;
                        if (assignUndo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Undo;
                        if (assignRedo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Redo;
                        if (assignAltDraw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDraw;
                        if (assignAltPan) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDragLevel;
                        if (assignToggleMultiselect) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMultiSelect;
                        if (assignEraseEverything) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.EraseEverything;
                        if (assignToggleTileVisibility) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleTileVisibility;
                        if (assignToggleMemoryLoadMode) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryLoadMode;
                        if (assignToggleMemoryDumpMode) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryDumbMode;
                        if (assignAltErase) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltErase;
                    }
                        break;

                    case 3: // tiles
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.TileEditor = new TileShortcuts();
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 180), 
                            "Tiles & Materials Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 30), "Move to left");
                        GuiLabel(new Rectangle(440, 185, 100, 30), "Move to right");
                        GuiLabel(new Rectangle(440, 220, 100, 30), "Move to top");
                        GuiLabel(new Rectangle(440, 255, 100, 30), "Move to bottom");
                        
                        var assignFocusOnCategory = GuiButton(new Rectangle(550, 150, 200, 30), $"{GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileCategoryMenu}");
                        var assignFocusOnTiles =GuiButton(new Rectangle(550, 185, 200, 30), $"{GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileMenu}");
                        var assignMoveUp = GuiButton(new Rectangle(550, 220, 200, 30), $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveUp}");
                        var assignMoveDown = GuiButton(new Rectangle(550, 255, 200, 30), $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveDown}");

                        if (assignFocusOnCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileCategoryMenu;
                        if (assignFocusOnTiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileMenu;
                        if (assignMoveUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveUp;
                        if (assignMoveDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveDown;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.TileEditor.Draw}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.TileEditor.DragLevel}");
                        var assignErase = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.TileEditor.Erase}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Erase;
                        
                        //
                        ImGui.SeparatorText("Keyboard Shortcuts");
                        //
                        
                        var assignCycleLayers = ImGui.Button($"Cycle Layers: {GLOBALS.Settings.Shortcuts.TileEditor.CycleLayers}");
                        var assignPickupItem = ImGui.Button($"Pickup Item: {GLOBALS.Settings.Shortcuts.TileEditor.PickupItem}"); 
                        var assignForcePlaceTileWithGeo = ImGui.Button($"Place Tile With Geo: {GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithGeo}"); 
                        var assignUndo = ImGui.Button($"Undo: {GLOBALS.Settings.Shortcuts.TileEditor.Undo}");
                        var assignRedo = ImGui.Button($"Redo: {GLOBALS.Settings.Shortcuts.TileEditor.Redo}"); 
                        var assignAltDraw = ImGui.Button($"Draw: {GLOBALS.Settings.Shortcuts.TileEditor.AltDraw}");
                        var assignAltPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.TileEditor.AltDragLevel}");
                        var assignForcePlaceTileWithoutGeo = ImGui.Button($"Place Without Geo{GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithoutGeo}");
                        var assignMoveToNextCategory = ImGui.Button($"To Next Category: {GLOBALS.Settings.Shortcuts.TileEditor.MoveToNextCategory}");
                        var assignTogglePathsView = ImGui.Button($"Show/Hide Paths: {GLOBALS.Settings.Shortcuts.TileEditor.TogglePathsView}");
                        var assignTileMaterialSwitch = ImGui.Button($"Materials/Tiles Switch: {GLOBALS.Settings.Shortcuts.TileEditor.TileMaterialSwitch}");
                        var assignToggleHoveredItemInfo = ImGui.Button($"Hovered Item Info: {GLOBALS.Settings.Shortcuts.TileEditor.HoveredItemInfo}");
                        var assignToggleLayer1 = ImGui.Button($"Show/Hide Layer 1: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1}");
                        var assignToggleLayer2 = ImGui.Button($"Show/Hide Layer 2: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2}");
                        var assignToggleLayer3 = ImGui.Button($"Show/Hide Layer 3: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3}");
                        var assignToggleLayer1Tiles = ImGui.Button($"Show/Hide Layer 1 Tiles: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1Tiles}");
                        var assignToggleLayer2Tiles = ImGui.Button($"Show/Hide Layer 2 Tiles: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2Tiles}");
                        var assignToggleLayer3Tiles = ImGui.Button($"Show/Hide Layer 3 Tiles: {GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3Tiles}");
                        var assignMoveToPreviousCategory = ImGui.Button($"To Previous Category: {GLOBALS.Settings.Shortcuts.TileEditor.MoveToPreviousCategory}");

                        if (assignCycleLayers) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.CycleLayers;
                        if (assignPickupItem) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.PickupItem;
                        if (assignForcePlaceTileWithGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithGeo;
                        if (assignUndo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Undo;
                        if (assignRedo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Redo;
                        if (assignAltDraw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.AltDraw;
                        if (assignAltPan) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.AltDragLevel;
                        if (assignForcePlaceTileWithoutGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithoutGeo;
                        if (assignTileMaterialSwitch) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.TileMaterialSwitch;
                        if (assignToggleHoveredItemInfo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.HoveredItemInfo;
                        if (assignToggleLayer1) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1;
                        if (assignToggleLayer2) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2;
                        if (assignToggleLayer3) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3;
                        if (assignToggleLayer1Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1Tiles;
                        if (assignToggleLayer2Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2Tiles;
                        if (assignToggleLayer3Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3Tiles;
                        if (assignMoveToNextCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveToNextCategory;
                        if (assignMoveToPreviousCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveToPreviousCategory;
                        if (assignTogglePathsView) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.TogglePathsView;
                    }
                        break;

                    case 4: // cameras
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.CameraEditor = new CameraShortcuts();
                        }
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignGrabCamera = ImGui.Button($"Grab Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.GrabCamera}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.CameraEditor.DragLevel}");
                        var assignCreateAndDeleteCamera = ImGui.Button($"Create/Delete Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCamera}");
                        var assignManipulateCamera = ImGui.Button($"Manipulate Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera}");

                        if (assignGrabCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.GrabCamera;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DragLevel;
                        if (assignCreateAndDeleteCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCamera;
                        if (assignManipulateCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera;
                        
                        //
                        ImGui.SeparatorText("Keyboard Shortcuts");
                        
                        var assignManipulateCameraKeyboard = ImGui.Button($"Manipulate Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt}");
                        var assignPanKeyboard = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.CameraEditor.DragLevelAlt}"); 
                        var assignGrabCameraKeyboard = ImGui.Button($"Grab Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.GrabCameraAlt}");
                        var assignCreateCamera = ImGui.Button($"Create Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.CreateCamera}"); 
                        var assignDeleteCamera = ImGui.Button($"Delete Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.DeleteCamera}");
                        var assignCreateAndDeleteCameraKeyboard = ImGui.Button($"Create/Delete Camera: {GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCameraAlt}");

                        if (assignManipulateCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt;
                        if (assignPanKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DragLevelAlt;
                        if (assignGrabCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.GrabCameraAlt;
                        if (assignCreateCamera) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateCamera;
                        if (assignDeleteCamera) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DeleteCamera;
                        if (assignCreateAndDeleteCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCameraAlt;
                    }
                        break;

                    case 5: // light
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.LightEditor = new LightShortcuts();
                        }
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        

                        var assignPaint = ImGui.Button($"Paint: {GLOBALS.Settings.Shortcuts.LightEditor.Paint}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.LightEditor.DragLevel}");
                        var assignErase = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.LightEditor.Erase}");

                        if (assignPaint) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.Paint;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.Erase;
                        
                        //
                        ImGui.SeparatorText("Keyboard Shortcuts");
                        
                        var assignPaintKeyboard = ImGui.Button($"Paint: {GLOBALS.Settings.Shortcuts.LightEditor.PaintAlt}");
                        var assignPanKeyboard = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.LightEditor.DragLevelAlt}");
                        var assignEraseKeyboard = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.LightEditor.EraseAlt}");
                        var assignIncreaseFlatness = ImGui.Button($"Increase Flatness: {GLOBALS.Settings.Shortcuts.LightEditor.IncreaseFlatness}");
                        var assignDecreaseFlatness = ImGui.Button($"Decrease Flatness: {GLOBALS.Settings.Shortcuts.LightEditor.DecreaseFlatness}"); 
                        var assignIncreaseAngle = ImGui.Button($"Increase Angle: {GLOBALS.Settings.Shortcuts.LightEditor.IncreaseAngle}"); 
                        var assignDecreaseAngle = ImGui.Button($"Decrease Angle: {GLOBALS.Settings.Shortcuts.LightEditor.DecreaseAngle}");
                        var assignRotateBrushCcw = ImGui.Button($"Rotate Brush Counter-Clockwise: {GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushCounterClockwise}"); 
                        var assignRotateBrushCw = ImGui.Button($"Rotate Brush Clockwise: {GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushClockwise}");
                        var assignStretchBrushVertically = ImGui.Button($"Stretch Brush Vertically: {GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushVertically}");
                        var assignStretchBrushHorizontally = ImGui.Button($"Stretch Brush Horizontally: {GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushHorizontally}");
                        var assignSqueezeBrushVertically = ImGui.Button($"Squeeze Brush Vertically: {GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushVertically}");
                        var assignSqueezeBrushHorizontally = ImGui.Button($"Squeeze Brush Horizontally: {GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushHorizontally}");
                        
                        var assignToggleTileVisibility = ImGui.Button($"Show/Hide Tiles: {GLOBALS.Settings.Shortcuts.LightEditor.ToggleTileVisibility}");
                        var assignToggleTileTextures = ImGui.Button($"Tile Display: {GLOBALS.Settings.Shortcuts.LightEditor.ToggleTilePreview}");
                        var assignToggleRenderedTiles = ImGui.Button($"Tint Tiles: {GLOBALS.Settings.Shortcuts.LightEditor.ToggleTintedTileTextures}");
                        var assignFastRotateBrushCcw = ImGui.Button($"Fast Rotate Brush Counter-Clockwise{GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushCounterClockwise}");
                        var assignFastRotateBrushCw = ImGui.Button($"Fast Rotate Brush Clockwise: {GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushClockwise}");
                        var assignFastStretchBrushVertically = ImGui.Button($"Fast Stretch Brush Vertically: {GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushVertically}");
                        var assignFastStretchBrushHorizontally = ImGui.Button($"Fast Stretch Brush Horizontally: {GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushHorizontally}");
                        var assignFastSqueezeBrushVertically = ImGui.Button($"Fast Squeeze Brush Vertically{GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushVertically}");
                        var assignFastSqueezeBrushHorizontally = ImGui.Button($"Fast Squeeze Brush Horizontally: {GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushHorizontally}");

                        if (assignPaintKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.PaintAlt;
                        if (assignPanKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.DragLevelAlt;
                        if (assignEraseKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.EraseAlt;
                        if (assignIncreaseFlatness) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.IncreaseFlatness;
                        if (assignDecreaseFlatness) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.DecreaseFlatness;
                        if (assignIncreaseAngle) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.IncreaseAngle;
                        if (assignDecreaseAngle) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.DecreaseAngle;
                        if (assignRotateBrushCcw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushCounterClockwise;
                        if (assignRotateBrushCw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushClockwise;
                        if (assignStretchBrushVertically) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushVertically;
                        if (assignStretchBrushHorizontally) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushHorizontally;
                        if (assignSqueezeBrushVertically) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushVertically;
                        if (assignSqueezeBrushHorizontally) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushHorizontally;
                        if (assignToggleTileVisibility) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.ToggleTileVisibility;
                        if (assignToggleTileTextures) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.ToggleTilePreview;
                        if (assignToggleRenderedTiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.ToggleTintedTileTextures;
                        if (assignFastRotateBrushCcw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushCounterClockwise;
                        if (assignFastRotateBrushCw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushClockwise;
                        if (assignFastStretchBrushVertically) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushVertically;
                        if (assignFastStretchBrushHorizontally) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushHorizontally;
                        if (assignFastSqueezeBrushVertically) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushVertically;
                        if (assignFastSqueezeBrushHorizontally) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushHorizontally;
                    }
                        break;

                    case 6: // effects
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.EffectsEditor = new EffectsShortcuts();
                        }
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignPaint = ImGui.Button($"Paint: {GLOBALS.Settings.Shortcuts.EffectsEditor.Paint}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevel}");
                        var assignErase = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.EffectsEditor.Erase}");

                        if (assignPaint) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.Paint;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.Erase;
                        
                        //
                        ImGui.SeparatorText("Keyboard Shortcuts");
                        
                        var assignPaintKeyboard = ImGui.Button($"Paint: {GLOBALS.Settings.Shortcuts.EffectsEditor.PaintAlt}");
                        var assignPanKeyboard = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevelAlt}");
                        var assignEraseKeyboard = ImGui.Button($"Erase: {GLOBALS.Settings.Shortcuts.EffectsEditor.EraseAlt}");
                        
                        
                        var assignShiftAppliedEffectUp = ImGui.Button($"Shift Effect Up: {GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectUp}");
                        var assignShiftAppliedEffectDown = ImGui.Button($"Shift Effect Down: {GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectDown}"); 
                        
                        var assignCycleAppliedEffectUp = ImGui.Button($"To Top Effect: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectUp}"); 
                        var assignCycleAppliedEffectDown = ImGui.Button($"To Bottom Effect: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectDown}");
                        var assignDeleteAppliedEffect = ImGui.Button($"Delete Effect: {GLOBALS.Settings.Shortcuts.EffectsEditor.DeleteAppliedEffect}"); 
                        var assignCycleEffectOptionsUp = ImGui.Button($"To Top Option: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsUp}");
                        var assignCycleEffectOptionsDown = ImGui.Button($"Top Bottom Option: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsDown}");
                        var assignToRightOptionChoice = ImGui.Button($"Next Option Choice: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesRight}");
                        var assignToLeftOptionChoice = ImGui.Button($"Previous Option Choice: {GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesLeft}");
                        
                        var assignStrongBrush = ImGui.Button($"String Brush: {GLOBALS.Settings.Shortcuts.EffectsEditor.StrongBrush}");

                        if (assignPaintKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.PaintAlt;
                        if (assignPanKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevelAlt;
                        if (assignEraseKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.EraseAlt;
                        
                        if (assignShiftAppliedEffectUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectUp;
                        if (assignShiftAppliedEffectDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectDown;
                        if (assignCycleAppliedEffectUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectUp;
                        if (assignCycleAppliedEffectDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectDown;
                        if (assignDeleteAppliedEffect) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.DeleteAppliedEffect;
                        if (assignCycleEffectOptionsUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsUp;
                        if (assignCycleEffectOptionsDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsDown;
                        if (assignToRightOptionChoice) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesRight;
                        if (assignToLeftOptionChoice) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesLeft;
                        if (assignStrongBrush) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.StrongBrush;
                    }
                        break;

                    case 7: // props
                    {
                        if (resetSelected)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.PropsEditor = new PropsShortcuts();
                        }
                        
                        ImGui.SeparatorText("Mouse Shortcuts");
                        
                        var assignPlaceProp = ImGui.Button($"Place Prop: {GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp}");
                        var assignPan = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.PropsEditor.DragLevel}");
                        var assignSelectProps = ImGui.Button($"Select Props: {GLOBALS.Settings.Shortcuts.PropsEditor.SelectProps}");

                        if (assignPlaceProp) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.DragLevel;
                        if (assignSelectProps) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.SelectProps;
                        
                        //
                        
                        var assignCycleLayers = ImGui.Button($"Cycle Layers: {GLOBALS.Settings.Shortcuts.PropsEditor.CycleLayers}");
                        var assignCycleSnapMode = ImGui.Button($"Cycle Snap Mode: {GLOBALS.Settings.Shortcuts.PropsEditor.CycleSnapMode}"); 
                        var assignToggleLayer1 = ImGui.Button($"Show/Hide Layer 1: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1}");
                        var assignToggleLayer2 = ImGui.Button($"Show/Hide Layer 2: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2}");
                        var assignToggleLayer3 = ImGui.Button($"Show/Hide Layer 3: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3}");
                        var assignToggleLayer1Tiles = ImGui.Button($"Show/Hide Layer 1 Tiles: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1Tiles}");
                        var assignToggleLayer2Tiles = ImGui.Button($"Show/Hide Layer 2 Tiles: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2Tiles}");
                        var assignToggleLayer3Tiles = ImGui.Button($"Show/Hide Layer 3 Tiles: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3Tiles}");
                        
                        ImGui.SeparatorText("Keyboard Shortcuts");
                        
                        var assignPlacePropAlt = ImGui.Button($"Place Prop: {GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp}");
                        var assignPanAlt = ImGui.Button($"Pan: {GLOBALS.Settings.Shortcuts.PropsEditor.EscapeSpinnerControl}");
                        var assignSelectPropsKeyboard = ImGui.Button($"Select Props: {GLOBALS.Settings.Shortcuts.PropsEditor.SelectPropsAlt}");
                        var assignToggleNoCollisionPlacement = ImGui.Button($"Toggle No-Collision Placement: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleNoCollisionPropPlacement}");
                        
                        ImGui.SeparatorText("Selected Props Actions");
                        
                        var assignToggleMovingProps = ImGui.Button($"Move: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleMovingPropsMode}");
                        var assignToggleRotatingProps = ImGui.Button($"Rotate: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRotatingPropsMode}");
                        var assignToggleScaling = ImGui.Button($"Scale: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleScalingPropsMode}");
                        var assignToggleVisibility = ImGui.Button($"Show/Hide: {GLOBALS.Settings.Shortcuts.PropsEditor.TogglePropsVisibility}");
                        var assignToggleWarp = ImGui.Button($"Warp: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleEditingPropQuadsMode}");
                        var assignDeleteProps = ImGui.Button($"Delete: {GLOBALS.Settings.Shortcuts.PropsEditor.DeleteSelectedProps}");
                        var assignEditRopePoints = ImGui.Button($"Edit Rope Points: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopePointsEditingMode}");
                        var assignToggleRopeEditing = ImGui.Button($"Rope Simulation: {GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopeEditingMode}");
                        
                        if (assignCycleLayers) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleLayers;
                        if (assignCycleSnapMode) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleSnapMode;
                        if (assignToggleLayer1) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1;
                        if (assignToggleLayer2) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2;
                        if (assignToggleLayer3) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3;
                        if (assignToggleLayer1Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1Tiles;
                        if (assignToggleLayer2Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2Tiles;
                        if (assignToggleLayer3Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3Tiles;
                        
                        if (assignPlacePropAlt) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.PlacePropAlt;
                        if (assignPanAlt) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.DragLevelAlt;
                        if (assignSelectPropsKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.SelectPropsAlt;
                        if (assignToggleNoCollisionPlacement) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleNoCollisionPropPlacement;
                        if (assignToggleMovingProps) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleMovingPropsMode;
                        if (assignToggleRotatingProps) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRotatingPropsMode;
                        if (assignToggleScaling) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleScalingPropsMode;
                        if (assignToggleVisibility) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.TogglePropsVisibility;
                        if (assignToggleWarp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleEditingPropQuadsMode;
                        if (assignDeleteProps) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.DeleteSelectedProps;
                        if (assignEditRopePoints) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopePointsEditingMode;
                        if (assignToggleRopeEditing) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopeEditingMode;
                    }
                        break;
                }

                ImGui.EndChild();
            }
            
            ImGui.End();
        }
        
        rlImGui.End();
        
        if (_shortcutToAssign is not null || _mouseShortcutToAssign is not null) _assigningShortcut = true;

        if (_shortcutToAssign is not null)
        {
            var key = GetKeyPressed();

            if (key == 256)
            {
                if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
                {
                    _shortcutToAssign.Key = KeyboardKey.KEY_NULL;
                }
                
                _assigningShortcut = false;
                _shortcutToAssign = null;
            }

            if (key != 0 && key != 340 && key != 341 && key != 342 && key != 256 && key != 4)
            {
                _shortcutToAssign.Key = (KeyboardKey)key;
                _shortcutToAssign.Shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
                _shortcutToAssign.Ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
                _shortcutToAssign.Alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
                
                _assigningShortcut = false;
                _shortcutToAssign = null;
            }
            
        }
        else if (_mouseShortcutToAssign is not null)
        {
            var key = GetKeyPressed();
            var button = -1;

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) button = 0;
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_MIDDLE)) button = 2;
            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) button = 1;

            if (key == 256)
            {
                _assigningShortcut = false;
                _mouseShortcutToAssign = null;
            }

            if (button != -1 && key != 340 && key != 341 && key != 342 && key != 256)
            {
                _mouseShortcutToAssign.Button = (MouseButton)button;
                _mouseShortcutToAssign.Shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
                _mouseShortcutToAssign.Ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
                _mouseShortcutToAssign.Alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);
                
                _assigningShortcut = false;
                _mouseShortcutToAssign = null;
            }
            
        }
        
        EndDrawing();
    }
}