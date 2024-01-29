using System.Numerics;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;

namespace Leditor;

public class SettingsPage : IPage
{
    private readonly Serilog.Core.Logger _logger;

    private int _categoryScrollIndex;
    private int _activeCategory;

    private byte _colorPickerLayer = 2;

    private readonly Color _geoBackgroundColor = new Color(120, 120, 120, 255);

    private readonly byte[] _title = "Settings"u8.ToArray();
    private readonly byte[] _geoColorPickerTitle = "Color Picker"u8.ToArray();

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
        
        var panelRect = new Rectangle(20, 20, width - 40, height - 40);
        var categoryRect = new Rectangle(30, 60, 200, height - 90);

        var subPanelX = (int)(categoryRect.x + categoryRect.width + 10);
        var subPanelY = (int)(categoryRect.y + 60);

        #region Shortcuts

        {
            if (IsKeyPressed(KeyboardKey.KEY_ONE)) GLOBALS.Page = 1;
            if (IsKeyPressed(KeyboardKey.KEY_TWO)) GLOBALS.Page = 2;
            if (IsKeyPressed(KeyboardKey.KEY_THREE)) GLOBALS.Page = 3;
            if (IsKeyPressed(KeyboardKey.KEY_FOUR)) GLOBALS.Page = 4;
            if (IsKeyPressed(KeyboardKey.KEY_FIVE)) GLOBALS.Page = 5;
            if (IsKeyPressed(KeyboardKey.KEY_SIX))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.Page = 6;
            }

            if (IsKeyPressed(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 8;
            //if (IsKeyPressed(KeyboardKey.KEY_SEVEN)) GLOBALS.Page = 9;
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
                _activeCategory = GuiListView(
                   categoryRect, 
                   "Geometry Editor;Tile Editor;Miscellaneous;Shortcuts", 
                   scrollIndex,
                   _activeCategory
                );
           }
        }

        switch (_activeCategory)
        {
            case 0: // Geometry Editor
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
            case 1: // Tile Editor
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
                
                break;
        }
        
        EndDrawing();
    }
}