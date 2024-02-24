using System.Numerics;
using System.Text.Json;
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

    private readonly Color _geoBackgroundColor = new Color(120, 120, 120, 255);

    private readonly byte[] _title = "Settings"u8.ToArray();
    private readonly byte[] _geoColorPickerTitle = "Color Picker"u8.ToArray();
    private readonly byte[] _shortcutCategoryScrollPanelTitle = "Panel"u8.ToArray();

    private readonly InitTile[] _previewTiles = [
        new InitTile { Size = (6, 10), Repeat = [1,1,1,1,1,1,1,1,2], BufferTiles = 1, Type = InitTileType.VoxelStruct },
        new InitTile { Size = (4, 4), Repeat = [1, 8, 1], BufferTiles = 1, Type = InitTileType.VoxelStruct },
        new InitTile { Size = (22, 10), Repeat = [1,1,1,1,1,1,1,1,1,1], BufferTiles = 0, Type = InitTileType.VoxelStruct },
        new InitTile { Size = (3, 3), Repeat = [1, 1, 1, 1, 1, 1, 1, 1, 2], BufferTiles = 1, Type = InitTileType.VoxelStruct },
        new InitTile { Size = (3, 3), Repeat = [1], BufferTiles = 1, Type = InitTileType.VoxelStructRockType },
        new InitTile { Size = (3, 3), Repeat = [1,8,1,10], BufferTiles = 1, Type = InitTileType.VoxelStruct }
    ];

    private readonly Color[] _previewTileColors = [
        new Color(255, 0, 255, 255),
        new Color(180, 255, 255, 255),
        new Color(0, 50, 255, 255),
        new Color(255, 0, 255, 255),
        new Color(210, 180, 180, 255),
        new Color(255, 160, 255, 255)
    ];

    private readonly Texture[] _previewTileTextures;

    public SettingsPage(Serilog.Core.Logger logger, Texture[] previewTextures)
    {
        _logger = logger;
        _previewTileTextures = previewTextures;
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

        unsafe
        {
           fixed (byte* t = _title)
           {
               GuiPanel(panelRect, (sbyte*)t);
           }

           fixed (int* scrollIndex = &_categoryScrollIndex)
           { 
                var newActiveCategory = GuiListView(
                   categoryRect, 
                   "General;Geometry Editor;Tile Editor;Camera Editor;Light Editor;Effects Editor;Props Editor;Miscellaneous;Shortcuts", 
                   scrollIndex,
                   _activeCategory
                );

                if (!_assigningShortcut) _activeCategory = newActiveCategory;
           }

           var saveSettings = GuiButton(new Rectangle(categoryRect.X, height - 125, 200, 40), "Save Settings");
           var resetSettings = GuiButton(new Rectangle(categoryRect.X, height - 80, 200, 40), "Reset Settings");


           if (saveSettings)
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
           
           if (resetSettings)
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
                   new PropEditor(),
                   new Experimental()
               );
           }
        }

        switch (_activeCategory)
        {
            case 0: // General
            {
                GLOBALS.Settings.GeneralSettings.DefaultFont = GuiCheckBox(
                    new Rectangle(subPanelX, categoryRect.Y, 20, 20), 
                    "Default Font", 
                    GLOBALS.Settings.GeneralSettings.DefaultFont
                );

                GLOBALS.Settings.GeneralSettings.GlobalCamera = GuiCheckBox(
                    new Rectangle(subPanelX, categoryRect.Y + 25, 20, 20),
                    "Global Camera",
                    GLOBALS.Settings.GeneralSettings.GlobalCamera
                );

                GLOBALS.Settings.GeneralSettings.ShortcutWindow = GuiCheckBox(
                    new Rectangle(subPanelX, categoryRect.Y + 50, 20, 20),
                    "Shortcuts Window",
                    GLOBALS.Settings.GeneralSettings.ShortcutWindow);
            }
                break;
            case 1: // Geometry Editor
                DrawText(
                    "Layer Colors", 
                    subPanelX, 
                    categoryRect.y, 
                    30, 
                    BLACK
                );
                
                DrawRectangleV(
                    new(subPanelX, subPanelY),
                    new(300, 300), 
                    _geoBackgroundColor
                );
                
                DrawRectangleV(
                    new(subPanelX + 20, subPanelY + 120),
                    new(150, 150),
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1
                );
                
                DrawRectangleV(
                    new(subPanelX + 70, subPanelY + 70),
                    new(150, 150),
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2
                );
                
                DrawRectangleV(
                    new(subPanelX + 120, subPanelY + 20),
                    new(150, 150),
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer3
                );

                unsafe
                {
                    fixed (byte* title = _geoColorPickerTitle)
                    {
                        var newColor = GuiColorPicker(
                            new Rectangle(subPanelX + 320, subPanelY, 300, 300), 
                            (sbyte*)title,
                            _colorPickerLayer switch
                            {
                                0 => GLOBALS.Settings.GeometryEditor.LayerColors.Layer1,
                                1 => GLOBALS.Settings.GeometryEditor.LayerColors.Layer2,
                                2 => GLOBALS.Settings.GeometryEditor.LayerColors.Layer3,
                                _ => throw new Exception($"Invalid _colorPickerLayer value \"{_colorPickerLayer}\"")
                            }
                        );

                        switch (_colorPickerLayer)
                        {
                            case 0: 
                                GLOBALS.Settings.GeometryEditor.LayerColors.Layer1 = newColor;
                                break;
                            case 1:
                                GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 = newColor;
                                break;
                            case 2:
                                GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 = newColor;
                                break;
                        }
                    }
                }

                if(GuiCheckBox(
                    new(subPanelX + 680, subPanelY, 20, 20), 
                    "Layer 3", 
                    _colorPickerLayer == 2
                )) _colorPickerLayer = 2;
                
                if (GuiCheckBox(
                    new(subPanelX + 680, subPanelY + 25, 20, 20), 
                    "Layer 2", 
                    _colorPickerLayer == 1
                )) _colorPickerLayer = 1;
                
                if (GuiCheckBox(
                    new(subPanelX + 680, subPanelY + 50, 20, 20), 
                    "Layer 1", 
                    _colorPickerLayer == 0
                )) _colorPickerLayer = 0;

                if (GuiButton(new(subPanelX + 680, subPanelY + 100, 100, 50), "Reset"))
                {
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer1 = new();
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 = new(G: 255, A: 50);
                    GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 = new(R: 255, A: 50);
                }
                
                DrawText("Tools", subPanelX, subPanelY + 330, 30, BLACK);

                GLOBALS.Settings.GeometryEditor.LegacyGeoTools = GuiCheckBox(
                    new(subPanelX, subPanelY + 380, 20, 20), 
                    "Legacy tools",
                    GLOBALS.Settings.GeometryEditor.LegacyGeoTools
                );

                GLOBALS.Settings.GeometryEditor.AllowOutboundsPlacement = GuiCheckBox(
                    new (subPanelX, subPanelY + 410, 20, 20),
                    "Allow placing out of bounds",
                    GLOBALS.Settings.GeometryEditor.AllowOutboundsPlacement
                );

                DrawText("Experimental", subPanelX, subPanelY + 500, 30, BLACK);

                GLOBALS.Settings.Experimental.NewGeometryEditor = GuiCheckBox(
                    new(subPanelX, subPanelY + 550, 20, 20), 
                    "New Interface",
                    GLOBALS.Settings.Experimental.NewGeometryEditor
                );
                
                break;
            case 2: // Tile Editor
                DrawText("Colors", subPanelX, subPanelY, 30, BLACK);

                DrawRectangle(
                    subPanelX,
                    subPanelY + 50,
                    (int)panelRect.width - subPanelX - 5,
                    300,
                    GRAY
                );

                const float scaleConst = 0.4f;

                #region ExampleTiles

                if (GLOBALS.Settings.TileEditor.UseTextures)
                {
                    {
                        // Draw the first example tile
                        ref var init1 = ref _previewTiles[0];

                        var center1 = new Vector2(
                            subPanelX + 100,
                            subPanelY + 200
                        );

                        var layerHeight1 = (init1.Size.Item2 + (init1.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth1 = (init1.Size.Item1 + (init1.BufferTiles * 2)) * GLOBALS.Scale;

                        var width1 = scaleConst * textureCutWidth1;
                        var height1 = scaleConst * layerHeight1;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[0],
                                ref _previewTiles[0],
                                ref center1,
                                [
                                    new(width1, -height1),
                                    new(-width1, -height1),
                                    new(-width1, height1),
                                    new(width1, height1),
                                    new(width1, -height1)
                                ],
                                _previewTileColors[0]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[0],
                                ref _previewTiles[0],
                                ref center1,
                                [
                                    new(width1, -height1),
                                    new(-width1, -height1),
                                    new(-width1, height1),
                                    new(width1, height1),
                                    new(width1, -height1)
                                ]
                            );
                        }

                        
                    }

                    {
                        // Draw the second example tile
                        ref var init2 = ref _previewTiles[1];

                        var center2 = new Vector2(
                            subPanelX + 230,
                            subPanelY + 200
                        );

                        var layerHeight2 = (init2.Size.Item2 + (init2.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth2 = (init2.Size.Item1 + (init2.BufferTiles * 2)) * GLOBALS.Scale;

                        var width2 = scaleConst * textureCutWidth2;
                        var height2 = scaleConst * layerHeight2;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[1],
                                ref _previewTiles[1],
                                ref center2,
                                [
                                    new(width2, -height2),
                                    new(-width2, -height2),
                                    new(-width2, height2),
                                    new(width2, height2),
                                    new(width2, -height2)
                                ],
                                _previewTileColors[1]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[1],
                                ref _previewTiles[1],
                                ref center2,
                                [
                                    new(width2, -height2),
                                    new(-width2, -height2),
                                    new(-width2, height2),
                                    new(width2, height2),
                                    new(width2, -height2)
                                ]
                            );
                        }

                        
                    }

                    {
                        // Draw the third example tile
                        ref var init3 = ref _previewTiles[2];

                        var center3 = new Vector2(
                            subPanelX + 500,
                            subPanelY + 200
                        );

                        var layerHeight3 = (init3.Size.Item2 + (init3.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth3 = (init3.Size.Item1 + (init3.BufferTiles * 2)) * GLOBALS.Scale;

                        var width3 = scaleConst * textureCutWidth3;
                        var height3 = scaleConst * layerHeight3;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[2],
                                ref _previewTiles[2],
                                ref center3,
                                [
                                    new(width3, -height3),
                                    new(-width3, -height3),
                                    new(-width3, height3),
                                    new(width3, height3),
                                    new(width3, -height3)
                                ],
                                _previewTileColors[2]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[2],
                                ref _previewTiles[2],
                                ref center3,
                                [
                                    new(width3, -height3),
                                    new(-width3, -height3),
                                    new(-width3, height3),
                                    new(width3, height3),
                                    new(width3, -height3)
                                ]
                            );
                        }

                        
                    }

                    {
                        // Draw the fourth example tile
                        ref var init4 = ref _previewTiles[3];

                        var center4 = new Vector2(
                            subPanelX + 750,
                            subPanelY + 200
                        );

                        var layerHeight4 = (init4.Size.Item2 + (init4.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth4 = (init4.Size.Item1 + (init4.BufferTiles * 2)) * GLOBALS.Scale;

                        var width4 = scaleConst * textureCutWidth4;
                        var height4 = scaleConst * layerHeight4;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[3],
                                ref _previewTiles[3],
                                ref center4,
                                [
                                    new(width4, -height4),
                                    new(-width4, -height4),
                                    new(-width4, height4),
                                    new(width4, height4),
                                    new(width4, -height4)
                                ],
                                _previewTileColors[3]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[3],
                                ref _previewTiles[3],
                                ref center4,
                                [
                                    new(width4, -height4),
                                    new(-width4, -height4),
                                    new(-width4, height4),
                                    new(width4, height4),
                                    new(width4, -height4)
                                ]
                            );
                        }
                    }

                    {
                        // Draw the fifth example tile

                        ref var init5 = ref _previewTiles[4];

                        var center5 = new Vector2(
                            subPanelX + 850,
                            subPanelY + 200
                        );

                        var layerHeight5 = (init5.Size.Item2 + (init5.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth5 = (init5.Size.Item1 + (init5.BufferTiles * 2)) * GLOBALS.Scale;

                        var width5 = scaleConst * textureCutWidth5;
                        var height5 = scaleConst * layerHeight5;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[4],
                                ref init5,
                                ref center5,
                                [
                                    new(width5, -height5),
                                    new(-width5, -height5),
                                    new(-width5, height5),
                                    new(width5, height5),
                                    new(width5, -height5)
                                ],
                                _previewTileColors[4]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[4],
                                ref init5,
                                ref center5,
                                [
                                    new(width5, -height5),
                                    new(-width5, -height5),
                                    new(-width5, height5),
                                    new(width5, height5),
                                    new(width5, -height5)
                                ]
                            );
                        }
                    }

                    {
                        // Draw the sixth example tile
                        ref var init6 = ref _previewTiles[5];

                        var center6 = new Vector2(
                            subPanelX + 950,
                            subPanelY + 200
                        );

                        var layerHeight6 = (init6.Size.Item2 + (init6.BufferTiles * 2)) * GLOBALS.Scale;
                        var textureCutWidth6 = (init6.Size.Item1 + (init6.BufferTiles * 2)) * GLOBALS.Scale;

                        var width6 = scaleConst * textureCutWidth6;
                        var height6 = scaleConst * layerHeight6;

                        if (GLOBALS.Settings.TileEditor.TintedTiles)
                        {
                            Printers.DrawTileAsPropColored(
                                ref _previewTileTextures[5],
                                ref init6,
                                ref center6,
                                [
                                    new(width6, -height6),
                                    new(-width6, -height6),
                                    new(-width6, height6),
                                    new(width6, height6),
                                    new(width6, -height6)
                                ],
                                _previewTileColors[5]
                            );
                        }
                        else
                        {
                            Printers.DrawTileAsProp(
                                ref _previewTileTextures[5],
                                ref init6,
                                ref center6,
                                [
                                    new(width6, -height6),
                                    new(-width6, -height6),
                                    new(-width6, height6),
                                    new(width6, height6),
                                    new(width6, -height6)
                                ]
                            );
                        }
                    }
                }
                else
                {
                    Printers.DrawTilePreview(
                        ref _previewTiles[0], 
                        ref _previewTileTextures[0], 
                        ref _previewTileColors[0], 
                        (21, 19)
                    );
                    
                    Printers.DrawTilePreview(
                        ref _previewTiles[1], 
                        ref _previewTileTextures[1], 
                        ref _previewTileColors[1], 
                        (28, 19)
                    );
                    
                    Printers.DrawTilePreview(
                        ref _previewTiles[2], 
                        ref _previewTileTextures[2], 
                        ref _previewTileColors[2], 
                        (45, 19)
                    );
                    
                    Printers.DrawTilePreview(
                        ref _previewTiles[3], 
                        ref _previewTileTextures[3], 
                        ref _previewTileColors[3], 
                        (61, 20)
                    );
                    
                    Printers.DrawTilePreview(
                        ref _previewTiles[4], 
                        ref _previewTileTextures[4], 
                        ref _previewTileColors[4], 
                        (67, 20)
                    );
                    
                    Printers.DrawTilePreview(
                        ref _previewTiles[5], 
                        ref _previewTileTextures[5], 
                        ref _previewTileColors[5], 
                        (74, 20)
                    );
                }

                #endregion

                GLOBALS.Settings.TileEditor.UseTextures = GuiToggle(
                    new(subPanelX, subPanelY + 400, 200, 50), 
                    GLOBALS.Settings.TileEditor.UseTextures 
                        ? "Texture" 
                        : "Preview", 
                    GLOBALS.Settings.TileEditor.UseTextures
                );
                
                GLOBALS.Settings.TileEditor.TintedTiles = GuiToggle(
                    new(subPanelX + 250, subPanelY + 400, 200, 50), 
                    GLOBALS.Settings.TileEditor.TintedTiles 
                        ? "Tinted" 
                        : "Original", 
                    GLOBALS.Settings.TileEditor.TintedTiles
                );

                GLOBALS.Settings.TileEditor.AllowUndefinedTiles = GuiCheckBox(
                    new Rectangle(subPanelX, subPanelY + 460, 20, 20),
                    "Allow Undefined Tiles",
                    GLOBALS.Settings.TileEditor.AllowUndefinedTiles
                );
                
                break;
            case 3: // Cameras Editor
                break;
            case 4: // Light Editor
                break;
            case 5: // Effects Editor
                break;
            case 6: // Props Editor
                break;
            case 7: // Miscellaneous
                break;
            case 8: // Shortcuts
            {
                GuiLine(new Rectangle(250, 60, 160, 30), "Editor Shortcuts");
                
                unsafe
                {
                    fixed (int* scrollIndex = &_shortcutCategoryScrollIndex)
                    {
                        var newShortcutActiveCategory = GuiListView(
                            new Rectangle(250, 91, 160, height - 200),
                            "Global;Old Geometry Editor;New Geometry Editor;Tile Editor;Cameras Editor;Light Editor;Effects Editor;Props Editor",
                            scrollIndex,
                            _shortcutActiveCategory
                        );

                        if (!_assigningShortcut) _shortcutActiveCategory = newShortcutActiveCategory;
                    }
                }

                var resetShortcuts = GuiButton(new Rectangle(250, height - 80, 160, 40), "Reset Shortcuts");

                switch (_shortcutActiveCategory)
                {
                    case 0: // Global
                    {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.GlobalShortcuts = new GlobalShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsGlobalScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        
                        GuiLabel(new Rectangle(430, 150, 100, 40), "Main Screen");
                        var assignToMainPage = GuiButton(new Rectangle(540, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToMainPage}");
                        
                        GuiLabel(new Rectangle(430, 195, 100, 40), "Geometry Editor");
                        var assignToGeometryEditor = GuiButton(new Rectangle(540, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor}"); 
                        
                        GuiLabel(new Rectangle(430, 240, 100, 40), "Tiles Editor");
                        var assignToTileEditor = GuiButton(new Rectangle(540, 240, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor}"); 
                        
                        GuiLabel(new Rectangle(430, 285, 100, 40), "Cameras Editor");
                        var assignToCamerasEditor = GuiButton(new Rectangle(540, 285, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor}");
                        
                        GuiLabel(new Rectangle(430, 330, 100, 40), "Light Editor");
                        var assignToLightEditor = GuiButton(new Rectangle(540, 330, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor}"); 
                        
                        GuiLabel(new Rectangle(430, 375, 100, 40), "Dimensions Editor");
                        var assignToDimensionsEditor = GuiButton(new Rectangle(540, 375, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor}");
                        
                        GuiLabel(new Rectangle(430, 420, 100, 40), "Effects Editor");
                        var assignToEffectsEditor = GuiButton(new Rectangle(540, 420, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor}");
                        
                        GuiLabel(new Rectangle(430, 465, 100, 40), "Props Editor");
                        var assignToPropsEditor = GuiButton(new Rectangle(540, 465, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor}");
                        
                        
                        GuiLabel(new Rectangle(430, 510, 100, 40), "Settings");
                        var assignToSettings = GuiButton(new Rectangle(540, 510, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage}");
                        


                        GuiLabel(new Rectangle(430, 555, 100, 40), "Save");
                        var assignSave = GuiButton(new Rectangle(540, 555, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSave}");

                        GuiLabel(new Rectangle(430, 600, 100, 40), "Save As");
                        var assignSaveAs = GuiButton(new Rectangle(540, 600, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.QuickSaveAs}");

                        GuiLabel(new Rectangle(430, 645, 100, 40), "Render");
                        var assignRender = GuiButton(new Rectangle(540, 645, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GlobalShortcuts.Render}");

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
                    
                    case 1: // Old Geometry
                    {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.GeoEditor = new GeoShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsOldGeoScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 210), 
                            "Tools Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 40), "Move to left");
                        GuiLabel(new Rectangle(440, 195, 100, 40), "Move to top");
                        GuiLabel(new Rectangle(440, 240, 100, 40), "Move to right");
                        GuiLabel(new Rectangle(440, 285, 100, 40), "Move to bottom");
                        
                        var assignToLeftGeo = GuiButton(new Rectangle(550, 150, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToLeftGeo}");
                        var assignToTopGeo =GuiButton(new Rectangle(550, 195, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToTopGeo}");
                        var assignToRightGeo = GuiButton(new Rectangle(550, 240, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo}");
                        var assignToBottomGeo = GuiButton(new Rectangle(550, 285, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo}");

                        if (assignToLeftGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToLeftGeo;
                        if (assignToTopGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToTopGeo;
                        if (assignToRightGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo;
                        if (assignToBottomGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 210), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 40), "Draw");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 195, 100, 40), "Level Pan");

                        var assignDraw = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.Draw}");
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.DragLevel}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.DragLevel;
                        
                        //
                        
                        GuiLabel(new Rectangle(430, 350, 100, 40), "Cycle Layers");
                        var assignCycleLayers = GuiButton(new Rectangle(540, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.CycleLayers}");
                        
                        GuiLabel(new Rectangle(430, 395, 100, 40), "Toggle Grid");
                        var assignToggleGrid = GuiButton(new Rectangle(540, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToggleGrid}"); 
                        
                        GuiLabel(new Rectangle(430, 440, 100, 40), "Show/Hide Cameras");
                        var assignToggleCameras = GuiButton(new Rectangle(540, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.ShowCameras}"); 
                        
                        GuiLabel(new Rectangle(430, 485, 100, 40), "Undo");
                        var assignUndo = GuiButton(new Rectangle(540, 485, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.Undo}");
                        
                        GuiLabel(new Rectangle(430, 530, 100, 40), "Redo");
                        var assignRedo = GuiButton(new Rectangle(540, 530, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.Redo}"); 
                        
                        GuiLabel(new Rectangle(430, 575, 100, 40), "Draw");
                        var assignAltDraw = GuiButton(new Rectangle(540, 575, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.AltDraw}");
                        
                        GuiLabel(new Rectangle(430, 620, 100, 40), "Level Pan");
                        var assignAltPan = GuiButton(new Rectangle(540, 620, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.AltDrag}");

                        if (assignCycleLayers) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.CycleLayers;
                        if (assignToggleGrid) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToggleGrid;
                        if (assignToggleCameras) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ShowCameras;
                        if (assignUndo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Undo;
                        if (assignRedo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.Redo;
                        if (assignAltDraw) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.AltDraw;
                        if (assignAltPan) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.AltDrag;
                    }
                        break;
                    case 2: // New Geometry
                    {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts = new ExperimentalGeoShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsNewGeoScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 210), 
                            "Tools Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 40), "Move to left");
                        GuiLabel(new Rectangle(440, 195, 100, 40), "Move to top");
                        GuiLabel(new Rectangle(440, 240, 100, 40), "Move to right");
                        GuiLabel(new Rectangle(440, 285, 100, 40), "Move to bottom");
                        
                        var assignToLeftGeo = GuiButton(new Rectangle(550, 150, 200, 40), $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToLeftGeo}");
                        var assignToTopGeo =GuiButton(new Rectangle(550, 195, 200, 40), $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToTopGeo}");
                        var assignToRightGeo = GuiButton(new Rectangle(550, 240, 200, 40), $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToRightGeo}");
                        var assignToBottomGeo = GuiButton(new Rectangle(550, 285, 200, 40), $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToBottomGeo}");

                        if (assignToLeftGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToLeftGeo;
                        if (assignToTopGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToTopGeo;
                        if (assignToRightGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToRightGeo;
                        if (assignToBottomGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToBottomGeo;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 210), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 40), "Draw");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 195, 100, 40), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 240, 100, 40), "Erase");

                        var assignDraw = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Draw}");
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.DragLevel}");
                        var assignErase = GuiButton(new Rectangle(mouseShortcutsOffset+110, 240, 200, 40), 
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Erase}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Erase;
                        
                        //
                        
                        GuiLabel(new Rectangle(430, 350, 100, 40), "Cycle Layers");
                        var assignCycleLayers = GuiButton(new Rectangle(540, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.CycleLayers}");
                        
                        GuiLabel(new Rectangle(430, 395, 100, 40), "Toggle Grid");
                        var assignToggleGrid = GuiButton(new Rectangle(540, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleGrid}"); 
                        
                        GuiLabel(new Rectangle(430, 440, 100, 40), "Show/Hide Cameras");
                        var assignToggleCameras = GuiButton(new Rectangle(540, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ShowCameras}"); 
                        
                        GuiLabel(new Rectangle(430, 485, 100, 40), "Undo");
                        var assignUndo = GuiButton(new Rectangle(540, 485, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Undo}");
                        
                        GuiLabel(new Rectangle(430, 530, 100, 40), "Redo");
                        var assignRedo = GuiButton(new Rectangle(540, 530, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.Redo}"); 
                        
                        GuiLabel(new Rectangle(430, 575, 100, 40), "Draw");
                        var assignAltDraw = GuiButton(new Rectangle(540, 575, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDraw}");
                        
                        GuiLabel(new Rectangle(430, 620, 100, 40), "Level Pan");
                        var assignAltPan = GuiButton(new Rectangle(540, 620, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltDragLevel}");
                        
                        GuiLabel(new Rectangle(430, 665, 100, 40), "Toggle Multi-select");
                        var assignToggleMultiselect = GuiButton(new Rectangle(540, 665, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMultiSelect}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 350, 100, 40), "Erase Everything");
                        var assignEraseEverything = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.EraseEverything}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 395, 100, 40), "Show/Hide Tiles");
                        var assignToggleTileVisibility = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleTileVisibility}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 440, 100, 40), "Toggle Memory Load Mode");
                        var assignToggleMemoryLoadMode = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryLoadMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 485, 100, 40), "Toggle Memory Dump Mode");
                        var assignToggleMemoryDumpMode = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 485, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.ToggleMemoryDumbMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 530, 100, 40), "Erase");
                        var assignAltErase = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 530, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.ExperimentalGeoShortcuts.AltErase}");

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
                    case 3: // Tiles Editor
                        {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.TileEditor = new TileShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsTileScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 210), 
                            "Tiles Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 40), "Move to left");
                        GuiLabel(new Rectangle(440, 195, 100, 40), "Move to top");
                        GuiLabel(new Rectangle(440, 240, 100, 40), "Move to right");
                        GuiLabel(new Rectangle(440, 285, 100, 40), "Move to bottom");
                        
                        var assignFocusOnCategory = GuiButton(new Rectangle(550, 150, 200, 40), $"{GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileCategoryMenu}");
                        var assignFocusOnTiles =GuiButton(new Rectangle(550, 195, 200, 40), $"{GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileMenu}");
                        var assignMoveUp = GuiButton(new Rectangle(550, 240, 200, 40), $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveUp}");
                        var assignMoveDown = GuiButton(new Rectangle(550, 285, 200, 40), $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveDown}");

                        if (assignFocusOnCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileCategoryMenu;
                        if (assignFocusOnTiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.FocusOnTileMenu;
                        if (assignMoveUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveUp;
                        if (assignMoveDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveDown;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 210), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 40), "Draw");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 195, 100, 40), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 240, 100, 40), "Erase");

                        var assignDraw = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.Draw}");
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.DragLevel}");
                        var assignErase = GuiButton(new Rectangle(mouseShortcutsOffset+110, 240, 200, 40), 
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.Erase}");

                        if (assignDraw) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Draw;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.Erase;
                        
                        //
                        
                        GuiLabel(new Rectangle(430, 350, 100, 40), "Cycle Layers");
                        var assignCycleLayers = GuiButton(new Rectangle(610, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.CycleLayers}");
                        
                        GuiLabel(new Rectangle(430, 395, 100, 40), "Pickup Item");
                        var assignPickupItem = GuiButton(new Rectangle(610, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.PickupItem}"); 
                        
                        GuiLabel(new Rectangle(430, 440, 100, 40), "Force-Place Tile With Geo");
                        var assignForcePlaceTileWithGeo = GuiButton(new Rectangle(610, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithGeo}"); 
                        
                        GuiLabel(new Rectangle(430, 485, 100, 40), "Undo");
                        var assignUndo = GuiButton(new Rectangle(610, 485, 200, 40),
                            $"");
                        
                        GuiLabel(new Rectangle(430, 530, 100, 40), "Redo");
                        var assignRedo = GuiButton(new Rectangle(610, 530, 200, 40),
                            $""); 
                        
                        GuiLabel(new Rectangle(430, 575, 100, 40), "Draw");
                        var assignAltDraw = GuiButton(new Rectangle(610, 575, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.AltDraw}");
                        
                        GuiLabel(new Rectangle(430, 620, 100, 40), "Level Pan");
                        var assignAltPan = GuiButton(new Rectangle(610, 620, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.AltDragLevel}");
                        
                        GuiLabel(new Rectangle(430, 665, 100, 40), "Force-Place Tile Without Geo");
                        var assignForcePlaceTileWithoutGeo = GuiButton(new Rectangle(610, 665, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ForcePlaceTileWithoutGeo}");
                        
                        GuiLabel(new Rectangle(430, 710, 100, 40), "Move to Next Category");
                        var assignMoveToNextCategory = GuiButton(new Rectangle(610, 710, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveToNextCategory}");
                        
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 350, 100, 40), "Tile/Material Switch");
                        var assignTileMaterialSwitch = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.TileMaterialSwitch}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 395, 100, 40), "Toggle Hovered Item Info");
                        var assignToggleHoveredItemInfo = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.HoveredItemInfo}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 440, 100, 40), "Show/Hide Layer 1");
                        var assignToggleLayer1 = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 485, 100, 40), "Show/Hide Layer 2");
                        var assignToggleLayer2 = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 485, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 530, 100, 40), "Show/Hide Layer 3");
                        var assignToggleLayer3 = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 530, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 575, 100, 40), "Show/Hide Layer 1 Tiles");
                        var assignToggleLayer1Tiles = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 575, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer1Tiles}");
                            
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 620, 100, 40), "Show/Hide Layer 2 Tiles");
                        var assignToggleLayer2Tiles = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 620, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2Tiles}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 665, 100, 40), "Show/Hide Layer 3 Tiles");
                        var assignToggleLayer3Tiles = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 665, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3Tiles}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 710, 100, 40), "Move to Previous Category");
                        var assignMoveToPreviousCategory = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 710, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.TileEditor.MoveToPreviousCategory}");

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
                        if (assignToggleLayer3Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer2Tiles;
                        if (assignToggleLayer3Tiles) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.ToggleLayer3Tiles;
                        if (assignMoveToNextCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveToNextCategory;
                        if (assignMoveToPreviousCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.TileEditor.MoveToPreviousCategory;
                    }
                        break;
                    case 4: // Cameras Editor
                        {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.CameraEditor = new CameraShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsCameraScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 210), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 40), "Grab Camera");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 195, 100, 40), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 240, 100, 40), "Create/Delete Camera");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 285, 100, 40), "Manipulate Camera");

                        var assignGrabCamera = GuiButton(new Rectangle(mouseShortcutsOffset+140, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.GrabCamera}");
                        
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+140, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.DragLevel}");
                        
                        var assignCreateAndDeleteCamera = GuiButton(new Rectangle(mouseShortcutsOffset+140, 240, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCamera}");
                        
                        var assignManipulateCamera = GuiButton(new Rectangle(mouseShortcutsOffset+140, 285, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera}");

                        if (assignGrabCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.GrabCamera;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DragLevel;
                        if (assignCreateAndDeleteCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCamera;
                        if (assignManipulateCamera) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCamera;
                        
                        //
                        
                        GuiLabel(new Rectangle(430, 350, 100, 40), "Manipulate Camera");
                        var assignManipulateCameraKeyboard = GuiButton(new Rectangle(560, 350, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt}");
                        
                        GuiLabel(new Rectangle(430, 395, 100, 40), "Level Pan");
                        var assignPanKeyboard = GuiButton(new Rectangle(560, 395, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.DragLevelAlt}"); 
                        
                        /*GuiLabel(new Rectangle(430, 440, 100, 40), "Show/Hide Cameras");
                        var assignToggleCameras = GuiButton(new Rectangle(540, 440, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.GeoEditor.ShowCameras}"); */
                        
                        GuiLabel(new Rectangle(430, 485, 100, 40), "Grab Camera");
                        var assignGrabCameraKeyboard = GuiButton(new Rectangle(560, 485, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.GrabCameraAlt}");
                        
                        GuiLabel(new Rectangle(430, 530, 100, 40), "Create Camera");
                        var assignCreateCamera = GuiButton(new Rectangle(560, 530, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.CreateCamera}"); 
                        
                        GuiLabel(new Rectangle(430, 575, 100, 40), "Delete Camera");
                        var assignDeleteCamera = GuiButton(new Rectangle(560, 575, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.DeleteCamera}");
                        
                        GuiLabel(new Rectangle(430, 620, 100, 40), "Create/Delete Camera");
                        var assignCreateAndDeleteCameraKeyboard = GuiButton(new Rectangle(560, 620, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCameraAlt}");

                        if (assignManipulateCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.ManipulateCameraAlt;
                        if (assignPanKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DragLevelAlt;
                        if (assignGrabCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.GrabCameraAlt;
                        if (assignCreateCamera) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateCamera;
                        if (assignDeleteCamera) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.DeleteCamera;
                        if (assignCreateAndDeleteCameraKeyboard) _shortcutToAssign = GLOBALS.Settings.Shortcuts.CameraEditor.CreateAndDeleteCameraAlt;
                    }
                        break;
                    case 5: // Light Editor
                        {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.LightEditor = new LightShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsLightScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 120), 
                            "Brush Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 40), "Next Brush");
                        GuiLabel(new Rectangle(440, 195, 100, 40), "Previous Brush");
                        // GuiLabel(new Rectangle(440, 240, 100, 40), "Move to right");
                        // GuiLabel(new Rectangle(440, 285, 100, 40), "Move to bottom");
                        
                        var assignNextBrush = GuiButton(new Rectangle(550, 150, 200, 40), $"{GLOBALS.Settings.Shortcuts.LightEditor.NextBrush}");
                        var assignPreviousBrush = GuiButton(new Rectangle(550, 195, 200, 40), $"{GLOBALS.Settings.Shortcuts.LightEditor.PreviousBrush}");
                        // var assignToRightGeo = GuiButton(new Rectangle(550, 240, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo}");
                        // var assignToBottomGeo = GuiButton(new Rectangle(550, 285, 200, 40), $"{GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo}");

                        if (assignNextBrush) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.NextBrush;
                        if (assignPreviousBrush) _shortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.PreviousBrush;
                        // if (assignToRightGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToRightGeo;
                        // if (assignToBottomGeo) _shortcutToAssign = GLOBALS.Settings.Shortcuts.GeoEditor.ToBottomGeo;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 210), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 40), "Paint");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 195, 100, 40), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 240, 100, 40), "Erase");

                        var assignPaint = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.Paint}");
                        
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 195, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.DragLevel}");
                        
                        var assignErase = GuiButton(new Rectangle(mouseShortcutsOffset+110, 240, 200, 40),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.Erase}");

                        if (assignPaint) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.Paint;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.LightEditor.Erase;
                        
                        //
                        
                        GuiLabel(new Rectangle(430, 265, 100, 30), "Paint");
                        var assignPaintKeyboard = GuiButton(new Rectangle(580, 265, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.PaintAlt}");
                        
                        GuiLabel(new Rectangle(430, 300, 100, 30), "Level Pan");
                        var assignPanKeyboard = GuiButton(new Rectangle(580, 300, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.DragLevelAlt}");
                        
                        GuiLabel(new Rectangle(430, 335, 100, 30), "Erase");
                        var assignEraseKeyboard = GuiButton(new Rectangle(580, 335, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.EraseAlt}");
                        
                        GuiLabel(new Rectangle(430, 370, 100, 30), "Increase Flatness");
                        var assignIncreaseFlatness = GuiButton(new Rectangle(580, 370, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.IncreaseFlatness}");
                        
                        GuiLabel(new Rectangle(430, 405, 100, 30), "Decrease Flatness");
                        var assignDecreaseFlatness = GuiButton(new Rectangle(580, 405, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.DecreaseFlatness}"); 
                        
                        GuiLabel(new Rectangle(430, 440, 100, 30), "Increase Angle");
                        var assignIncreaseAngle = GuiButton(new Rectangle(580, 440, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.IncreaseAngle}"); 
                        
                        GuiLabel(new Rectangle(430, 475, 100, 30), "Decrease Angle");
                        var assignDecreaseAngle = GuiButton(new Rectangle(580, 475, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.DecreaseAngle}");
                        
                        GuiLabel(new Rectangle(430, 510, 100, 30), "Rotate Brush CCW");
                        var assignRotateBrushCcw = GuiButton(new Rectangle(580, 510, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushCounterClockwise}"); 
                        
                        GuiLabel(new Rectangle(430, 545, 100, 30), "Rotate Brush CW");
                        var assignRotateBrushCw = GuiButton(new Rectangle(580, 545, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.RotateBrushClockwise}");
                        
                        GuiLabel(new Rectangle(430, 580, 100, 30), "Stretch Brush Vertically");
                        var assignStretchBrushVertically = GuiButton(new Rectangle(580, 580, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushVertically}");
                        
                        GuiLabel(new Rectangle(430, 615, 100, 30), "Stretch Brush Horizontally");
                        var assignStretchBrushHorizontally = GuiButton(new Rectangle(580, 615, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.StretchBrushHorizontally}");
                        
                        GuiLabel(new Rectangle(430, 650, 100, 30), "Squeeze Brush Vertically");
                        var assignSqueezeBrushVertically = GuiButton(new Rectangle(580, 650, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushVertically}");
                        
                        GuiLabel(new Rectangle(430, 685, 100, 30), "Squeeze Brush Horizontally");
                        var assignSqueezeBrushHorizontally = GuiButton(new Rectangle(580, 685, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.SqueezeBrushHorizontally}");
                        
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 405, 100, 30), "Show/Hide Tiles");
                        var assignToggleTileVisibility = GuiButton(new Rectangle(mouseShortcutsOffset+190, 405, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.ToggleTileVisibility}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 440, 100, 30), "Use Tile Textures");
                        var assignToggleTileTextures = GuiButton(new Rectangle(mouseShortcutsOffset+190, 440, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.ToggleTilePreview}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 475, 100, 30), "Toggle Tinted Tile Textures");
                        var assignToggleRenderedTiles = GuiButton(new Rectangle(mouseShortcutsOffset+190, 475, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.ToggleTintedTileTextures}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 510, 100, 30), "Fast Rotate Brush CCW");
                        var assignFastRotateBrushCcw = GuiButton(new Rectangle(mouseShortcutsOffset+190, 510, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushCounterClockwise}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 545, 100, 30), "Fast Rotate Brush CW");
                        var assignFastRotateBrushCw = GuiButton(new Rectangle(mouseShortcutsOffset+190, 545, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastRotateBrushClockwise}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 580, 100, 30), "Fast Stretch Brush Vertically");
                        var assignFastStretchBrushVertically = GuiButton(new Rectangle(mouseShortcutsOffset+190, 580, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushVertically}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 615, 100, 30), "Fast Stretch Brush Horizontally");
                        var assignFastStretchBrushHorizontally = GuiButton(new Rectangle(mouseShortcutsOffset+190, 615, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastStretchBrushHorizontally}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 650, 100, 30), "Fast Squeeze Brush Vertically");
                        var assignFastSqueezeBrushVertically = GuiButton(new Rectangle(mouseShortcutsOffset+190, 650, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushVertically}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 685, 100, 30), "Fast Squeeze Brush Horizontally");
                        var assignFastSqueezeBrushHorizontally = GuiButton(new Rectangle(mouseShortcutsOffset+190, 685, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.LightEditor.FastSqueezeBrushHorizontally}");

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
                    case 6: // Effects Editor
                        {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.EffectsEditor = new EffectsShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsEffectsScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 220), 
                            "New Effect Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 30), "Category Navigation");
                        GuiLabel(new Rectangle(440, 185, 100, 30), "Move Up");
                        GuiLabel(new Rectangle(440, 220, 100, 30), "Move Down");
                        GuiLabel(new Rectangle(440, 255, 100, 30), "Accept");
                        GuiLabel(new Rectangle(440, 290, 100, 30), "Alt Accept");
                        
                        var assignNewEffectCategoryNavigation = GuiButton(new Rectangle(550, 150, 200, 30), "SHIFT");
                        var assignMoveUp = GuiButton(new Rectangle(550, 185, 200, 30), $"{GLOBALS.Settings.Shortcuts.EffectsEditor.MoveUpInNewEffectMenu}");
                        var assignMoveDown = GuiButton(new Rectangle(550, 220, 200, 30), $"{GLOBALS.Settings.Shortcuts.EffectsEditor.MoveDownInNewEffectMenu}");
                        var assignAcceptNewEffect = GuiButton(new Rectangle(550, 255, 200, 30), $"{GLOBALS.Settings.Shortcuts.EffectsEditor.AcceptNewEffect}");
                        var assignAcceptNewEffectAlt = GuiButton(new Rectangle(550, 290, 200, 30), $"{GLOBALS.Settings.Shortcuts.EffectsEditor.AcceptNewEffectAlt}");

                        // if (assignNewEffectCategoryNavigation) GLOBALS.Settings.Shortcuts.EffectsEditor.NewEffectMenuCategoryNavigation;
                        if (assignMoveUp) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.MoveUpInNewEffectMenu;
                        if (assignMoveDown) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.MoveDownInNewEffectMenu;
                        if (assignAcceptNewEffect) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.AcceptNewEffect;
                        if (assignAcceptNewEffectAlt) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.AcceptNewEffectAlt;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 170), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 30), "Paint");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 185, 100, 30), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 220, 100, 30), "Erase");

                        var assignPaint = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.Paint}");
                        
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 185, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevel}");
                        
                        var assignErase = GuiButton(new Rectangle(mouseShortcutsOffset+110, 220, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.Erase}");

                        if (assignPaint) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.Paint;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevel;
                        if (assignErase) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.Erase;
                        
                        //
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 370, 100, 30), "Paint");
                        var assignPaintKeyboard = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 370, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.PaintAlt}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 405, 100, 30), "Level Pan");
                        var assignPanKeyboard = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 405, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.DragLevelAlt}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 440, 100, 30), "Erase");
                        var assignEraseKeyboard = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 440, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.EraseAlt}");
                        
                        
                        GuiLabel(new Rectangle(430, 370, 100, 30), "Shift Applied Effect Up");
                        var assignShiftAppliedEffectUp = GuiButton(new Rectangle(580, 370, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectUp}");
                        
                        GuiLabel(new Rectangle(430, 405, 100, 30), "Shift Applied Effect Down");
                        var assignShiftAppliedEffectDown = GuiButton(new Rectangle(580, 405, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.ShiftAppliedEffectDown}"); 
                        
                        GuiLabel(new Rectangle(430, 440, 100, 30), "Cycle Applied Effects Up");
                        var assignCycleAppliedEffectUp = GuiButton(new Rectangle(580, 440, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectUp}"); 
                        
                        GuiLabel(new Rectangle(430, 475, 100, 30), "Cycle Applied Effects Down");
                        var assignCycleAppliedEffectDown = GuiButton(new Rectangle(580, 475, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleAppliedEffectDown}");
                        
                        GuiLabel(new Rectangle(430, 510, 100, 30), "Delete Applied Effect");
                        var assignDeleteAppliedEffect = GuiButton(new Rectangle(580, 510, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.DeleteAppliedEffect}"); 
                        
                        GuiLabel(new Rectangle(430, 545, 100, 30), "Cycle Effect Options Up");
                        var assignCycleEffectOptionsUp = GuiButton(new Rectangle(580, 545, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsUp}");
                        
                        GuiLabel(new Rectangle(430, 580, 100, 30), "Cycle Effect Options Down");
                        var assignCycleEffectOptionsDown = GuiButton(new Rectangle(580, 580, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionsDown}");
                        
                        GuiLabel(new Rectangle(430, 615, 100, 30), "To Right Option Choice");
                        var assignToRightOptionChoice = GuiButton(new Rectangle(580, 615, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesRight}");
                        
                        GuiLabel(new Rectangle(430, 650, 100, 30), "To Left Option Choice");
                        var assignToLeftOptionChoice = GuiButton(new Rectangle(580, 650, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.CycleEffectOptionChoicesLeft}");
                        
                        GuiLabel(new Rectangle(430, 685, 100, 30), "Show/Hide Options");
                        var assignToggleOptionsVisibility = GuiButton(new Rectangle(580, 685, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.EffectsEditor.ToggleOptionsVisibility}");
                        
                        

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
                        if (assignToggleOptionsVisibility) _shortcutToAssign = GLOBALS.Settings.Shortcuts.EffectsEditor.ToggleOptionsVisibility;
                    }
                        break;
                    case 7: // Props
                    {
                        if (resetShortcuts)
                        {
                            _assigningShortcut = false;
                            _shortcutToAssign = null;
                            _mouseShortcutToAssign = null;

                            GLOBALS.Settings.Shortcuts.PropsEditor = new PropsShortcuts();
                        }
                        
                        unsafe
                        {
                            fixed (byte* pl = _shortcutCategoryScrollPanelTitle)
                            {
                                fixed (Vector2* sv = &_shortcutsPropsScrollPanelScroll)
                                {
                                    GuiScrollPanel(
                                        shortcutsScrollPanelRect, 
                                        (sbyte*)pl, 
                                        new Rectangle(), 
                                        sv
                                    );
                                }
                            }
                        }
                        
                        GuiGroupBox(
                            new Rectangle(430, 130, shortcutsScrollPanelRect.width/2f - 10, 100), 
                            "Mode"
                        );
                        
                        GuiLabel(new Rectangle(440, 150, 100, 30), "Cycle Mode Right");
                        GuiLabel(new Rectangle(440, 185, 100, 30), "Cycle Mode Left");
                        
                        var assignCycleModeRight = GuiButton(new Rectangle(550, 150, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleModeRight}");
                        var assignCycleModeLeft = GuiButton(new Rectangle(550, 185, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleModeRight}");

                        if (assignCycleModeRight) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleModeRight;
                        if (assignCycleModeLeft) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleModeLeft;
                        
                        //
                        
                        GuiGroupBox(
                            new Rectangle(430, 250, shortcutsScrollPanelRect.width/2f - 10, 90), 
                            "Menu"
                        );
                        
                        GuiLabel(new Rectangle(440, 260, 100, 30), "Cycle Categories Right");
                        GuiLabel(new Rectangle(440, 295, 100, 30), "Cycle Categories Left");

                        var assignCycleCategoriesRight = GuiButton(new Rectangle(570, 260, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleCategoriesRight}");
                        var assignCycleCategoriesLeft = GuiButton(new Rectangle(570, 295, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleCategoriesLeft}");

                        if (assignCycleCategoriesRight) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleCategoriesRight;
                        if (assignCycleCategoriesLeft) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.CycleCategoriesLeft;
                        
                        //
                        
                        GuiGroupBox(
                            new Rectangle(430, 350, shortcutsScrollPanelRect.width/2f - 10, 90), 
                            "Subcategories"
                        );
                        
                        GuiLabel(new Rectangle(440, 360, 100, 30), "Focus on Category");
                        GuiLabel(new Rectangle(440, 395, 100, 30), "Focus on List");

                        var assignFocusOnCategory = GuiButton(new Rectangle(550, 360, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.InnerCategoryFocusLeft}");
                        var assignFocusOnList = GuiButton(new Rectangle(550, 395, 200, 30), $"{GLOBALS.Settings.Shortcuts.PropsEditor.InnerCategoryFocusRight}");

                        if (assignFocusOnCategory) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.InnerCategoryFocusLeft;
                        if (assignFocusOnList) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.InnerCategoryFocusRight;
                        
                        //

                        var mouseShortcutsOffset = 430 + shortcutsScrollPanelRect.width / 2f;
                        
                        GuiGroupBox(
                            new Rectangle(mouseShortcutsOffset, 130, shortcutsScrollPanelRect.width/2f - 20, 140), 
                            "Mouse Shortcuts"
                        );
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 150, 100, 30), "Place Prop");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 185, 100, 30), "Level Pan");
                        GuiLabel(new Rectangle(mouseShortcutsOffset + 10, 220, 100, 30), "Select Props");

                        var assignPlaceProp = GuiButton(new Rectangle(mouseShortcutsOffset+110, 150, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp}");
                        
                        var assignPan = GuiButton(new Rectangle(mouseShortcutsOffset+110, 185, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.DragLevel}");
                        
                        var assignSelectProps = GuiButton(new Rectangle(mouseShortcutsOffset+110, 220, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.SelectProps}");

                        if (assignPlaceProp) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp;
                        if (assignPan) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.DragLevel;
                        if (assignSelectProps) _mouseShortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.SelectProps;
                        
                        //
                        
                        
                        GuiLabel(new Rectangle(430, 475, 100, 30), "Cycle Layers");
                        var assignCycleLayers = GuiButton(new Rectangle(580, 475, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleLayers}");
                        
                        GuiLabel(new Rectangle(430, 510, 100, 30), "Cycle Snap Mode");
                        var assignCycleSnapMode = GuiButton(new Rectangle(580, 510, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.CycleSnapMode}"); 
                        
                        GuiLabel(new Rectangle(430, 545, 100, 30), "Show/Hide Layer 1");
                        var assignToggleLayer1 = GuiButton(new Rectangle(580, 545, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1}");
                        
                        GuiLabel(new Rectangle(430, 580, 100, 30), "Show/Hide Layer 2");
                        var assignToggleLayer2 = GuiButton(new Rectangle(580, 580, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2}");
                        
                        GuiLabel(new Rectangle(430, 615, 100, 30), "Show/Hide Layer 3");
                        var assignToggleLayer3 = GuiButton(new Rectangle(580, 615, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3}");
                        
                        GuiLabel(new Rectangle(430, 650, 100, 30), "Show/Hide Layer 1 Tiles");
                        var assignToggleLayer1Tiles = GuiButton(new Rectangle(580, 650, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer1Tiles}");
                        
                        GuiLabel(new Rectangle(430, 685, 100, 30), "Show/Hide Layer 2 Tiles");
                        var assignToggleLayer2Tiles = GuiButton(new Rectangle(580, 685, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer2Tiles}");
                        
                        GuiLabel(new Rectangle(430, 720, 100, 30), "Show/Hide Layer 3 Tiles");
                        var assignToggleLayer3Tiles = GuiButton(new Rectangle(580, 720, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleLayer3Tiles}");
                        
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 280, 100, 30), "Place Prop");
                        var assignPlacePropAlt = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 280, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.PlaceProp}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 315, 100, 30), "Level Pan");
                        var assignPanAlt = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 315, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.EscapeSpinnerControl}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 350, 100, 30), "Select Props");
                        var assignSelectPropsKeyboard = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 350, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.SelectPropsAlt}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 385, 100, 30), "Escape Spinner");
                        var assignEscapeSpinner = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 385, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.EscapeSpinnerControl}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 420, 100, 30), "Toggle No-collision Placement");
                        var assignToggleNoCollisionPlacement = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 420, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleNoCollisionPropPlacement}");
                         
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 455, 100, 30), "Toggle Moving Props");
                        var assignToggleMovingProps = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 455, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleMovingPropsMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 490, 100, 30), "Toggle Rotating Props");
                        var assignToggleRotatingProps = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 490, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRotatingPropsMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 525, 100, 30), "Toggle Scaling Props");
                        var assignToggleScaling = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 525, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleScalingPropsMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 560, 100, 30), "Toggle Props Visibility");
                        var assignToggleVisibility = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 560, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.TogglePropsVisibility}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 595, 100, 30), "Toggle Warp Props");
                        var assignToggleWarp = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 595, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleEditingPropQuadsMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 630, 100, 30), "Delete Props");
                        var assignDeleteProps = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 630, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.DeleteSelectedProps}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 665, 100, 30), "Toggle Rope Points Editing");
                        var assignEditRopePoints = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 665, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopePointsEditingMode}");
                        
                        GuiLabel(new Rectangle(mouseShortcutsOffset, 700, 100, 30), "Toggle Rope Editing");
                        var assignToggleRopeEditing = GuiButton(new Rectangle(mouseShortcutsOffset + 190, 700, 200, 30),
                            $"{GLOBALS.Settings.Shortcuts.PropsEditor.ToggleRopeEditingMode}");
                        

                        if (assignEscapeSpinner) _shortcutToAssign = GLOBALS.Settings.Shortcuts.PropsEditor.EscapeSpinnerControl;
                        
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
            }
                break;
        }
        
        EndDrawing();
    }
}