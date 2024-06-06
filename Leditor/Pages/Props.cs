using System.Numerics;
using ImGuiNET;
using Leditor.Data.Tiles;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using RenderTexture2D = Leditor.RL.Managed.RenderTexture2D;
using Leditor.Types;
using System.Drawing.Printing;


namespace Leditor.Pages;

#nullable enable

internal class PropsEditorPage : EditorPage, IContextListener
{
    public override void Dispose()
    {
        if (Disposed) return;
        
        Disposed = true;
        
        _propTooltip.Dispose();
        _propLayerRT.Dispose();
        _propModeIndicatorsRT.Dispose();
    }

    ~PropsEditorPage()
    {
        if (!Disposed) throw new InvalidOperationException("PropsEditorPage was not disposed by consumer");
    }

    private Camera2D _camera;

    private readonly PropsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.PropsEditor;

    private RenderTexture2D _propLayerRT = new(0, 0);

    private bool _shouldRedrawPropLayer = true;

    private void DrawPropLayerRT() {
        BeginTextureMode(_propLayerRT);
        ClearBackground(Color.White with { A = 0 });

        for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
        {
            if (_hidden[p]) continue;
            
            var current = GLOBALS.Level.Props[p];
            
            // Filter based on depth
            if (current.prop.Depth <= -(GLOBALS.Layer+1)*10 || current.prop.Depth > -GLOBALS.Layer*10) continue;

            var (category, index) = current.position;
            var quads = current.prop.Quads;
            
            // origin must be the center
            // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
            
            // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
            Printers.DrawProp(current.type, current.tile, category, index, current.prop, 16, GLOBALS.Settings.GeneralSettings.DrawPropMode, GLOBALS.SelectedPalette);

            // Draw Rope Point
            if (current.type == InitPropType.Rope)
            {
                foreach (var point in current.prop.Extras.RopePoints)
                {
                    DrawCircleV(point, 3f, Color.White);
                }
            }
            
            if (_selected[p])
            {
                // Side Lines
                
                DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                
                // Quad Points

                if (_stretchingProp)
                {
                    if (current.type == InitPropType.Rope)
                    {
                        DrawCircleV(
                            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                new(2f, 2f)), 
                            5f, 
                            Color.Blue
                        );
                        
                        DrawCircleV(
                            Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                new(2f, 2f)), 
                            5f, 
                            Color.Blue
                        );
                        
                        /*DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                        DrawCircleV(quads.TopRight, 2f, Color.Blue);
                        DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                        DrawCircleV(quads.BottomLeft, 2f, Color.Blue);*/
                    }
                    else if (current.type == InitPropType.Long)
                    {
                        var sides = Utils.LongSides(current.prop.Quads);
                        
                        DrawCircleV(sides.left, 5f, Color.Blue);
                        DrawCircleV(sides.top, 5f, Color.Blue);
                        DrawCircleV(sides.right, 5f, Color.Blue);
                        DrawCircleV(sides.bottom, 5f, Color.Blue);
                    }
                    else
                    {
                        DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                        DrawCircleV(quads.TopRight, 5f, Color.Blue);
                        DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                        DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
                    }
                }
                else if (_scalingProps)
                {
                    var center = Utils.QuadsCenter(ref quads);
                    
                    switch (_stretchAxes)
                    {
                        case 1:
                            DrawLineEx(
                                center with { X = -10 }, 
                                center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                2f, 
                                Color.Red
                            );
                            break;
                        case 2:
                            DrawLineEx(
                                center with { Y = -10 },
                                center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                2f,
                                Color.Green
                            );
                            break;
                    }
                }
                
                // Draw Rope Point
                if (current.type == InitPropType.Rope)
                {
                    if (_editingPropPoints)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.Red);
                        }
                    }
                    else
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 2f, Color.Orange);
                        }
                    }

                    if (_ropeMode)
                    {
                        // p is copied as suggested by the code editor..
                        var p1 = p;
                        var model = _models.Single(r => r.index == p1);

                        if (!model.simSwitch)
                        {
                            foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
                        }
                    }
                }
            }
        }

        EndTextureMode();
    }


    private bool _ropeInitialPlacement;
    private int _additionalInitialRopeSegments;

    private bool _longInitialPlacement;
    
    
    private bool _lockedPlacement;

    private int _hoveredCategoryIndex = -1;
    private int _hoveredIndex = -1;
    private int _previousRootCategory;

    private bool _tooltip = true;
    
    private RenderTexture2D _propTooltip = new(0, 0);

    private RenderTexture2D _propModeIndicatorsRT = new(40 * 5, 40);

    private bool _shouldUpdateModeIndicatorsRT = true;

    private void UpdateModeIndicators() {
        var darkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme;

        var color = darkTheme ? Color.White : Color.Black;

        BeginTextureMode(_propModeIndicatorsRT);

        ClearBackground(darkTheme ? Color.Black : Color.White);

        if (_movingProps)           DrawRectangle(  0, 0, 40, 40, Color.Blue);
        if (_rotatingProps)         DrawRectangle( 40, 0, 40, 40, Color.Blue);
        if (_scalingProps)          DrawRectangle( 80, 0, 40, 40, Color.Blue);
        if (_stretchingProp)        DrawRectangle(120, 0, 40, 40, Color.Blue);
        if (_editingPropPoints)     DrawRectangle(160, 0, 40, 40, Color.Blue);

        var texture1 = GLOBALS.Textures.PropEditModes[0];
        var texture2 = GLOBALS.Textures.PropEditModes[1];
        var texture3 = GLOBALS.Textures.PropEditModes[2];
        var texture4 = GLOBALS.Textures.PropEditModes[3];
        var texture5 = GLOBALS.Textures.PropEditModes[4];

        DrawTexturePro(texture1, new Rectangle(0, 0, texture1.Width, texture1.Height), new Rectangle(  0, 0, 40, 40), new Vector2(0, 0), 0, _movingProps       ? Color.White : color);
        DrawTexturePro(texture2, new Rectangle(0, 0, texture2.Width, texture1.Height), new Rectangle( 40, 0, 40, 40), new Vector2(0, 0), 0, _rotatingProps     ? Color.White : color);
        DrawTexturePro(texture3, new Rectangle(0, 0, texture3.Width, texture1.Height), new Rectangle( 80, 0, 40, 40), new Vector2(0, 0), 0, _scalingProps      ? Color.White : color);
        DrawTexturePro(texture4, new Rectangle(0, 0, texture4.Width, texture1.Height), new Rectangle(120, 0, 40, 40), new Vector2(0, 0), 0, _stretchingProp    ? Color.White : color);
        DrawTexturePro(texture5, new Rectangle(0, 0, texture5.Width, texture1.Height), new Rectangle(160, 0, 40, 40), new Vector2(0, 0), 0, _editingPropPoints ? Color.White : color);

        EndTextureMode();
    }

    private void UpdatePropTooltip()
    {
        _propTooltip.Dispose();

        var tint = GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.LightGray : Color.Gray;

        // TODO: This should really be made into an enum
        switch (_menuRootCategoryIndex)
        {
            case 0: // Tile-As-Prop
            {
                if (_hoveredTile is null) return;
                
                var (width, height) = _hoveredTile.Size;
                
                _propTooltip = new RenderTexture2D(20 * width + 20, 20 * height + 20);
                
                BeginTextureMode(_propTooltip);
                ClearBackground(Color.White with { A = 0 });
                    
                Printers.DrawTileAsPropColored(
                    _hoveredTile, 
                    new Vector2(0, 0), 
                    new  Vector2(-10, -10), 
                    tint, 
                    0, 
                    20
                );
                EndTextureMode();
            }
                break;
            
            case 1: // Ropes
                break;
            
            case 2: // Longs
                break;

            case 3: // Others
            {
                var prop = GLOBALS.Props[_hoveredCategoryIndex][_hoveredIndex];
                var texture = GLOBALS.Textures.Props[_hoveredCategoryIndex][_hoveredIndex];

                var (width, height) = Utils.GetPropSize(prop);

                if (width == -1 && height == -1)
                {
                    _propTooltip = new RenderTexture2D(texture.Width, texture.Height);
                }
                else
                {
                    _propTooltip = new RenderTexture2D(width, height);
                }
                
                BeginTextureMode(_propTooltip);
                ClearBackground(Color.White with { A = 0 });

                var quad = (width, height) is (-1, -1) 
                    ? new PropQuad(
                        new Vector2(0, 0),
                        new Vector2(texture.Width, 0),
                        new Vector2(texture.Width, texture.Height),
                        new Vector2(0, texture.Height)
                    )
                    : new PropQuad(
                    new Vector2(0, 0),
                    new Vector2(width, 0),
                    new Vector2(width, height),
                    new Vector2(0, height)
                    );
                
                Printers.DrawProp(prop, texture, quad);
                
                EndTextureMode();
                
            }
                break;
        }
    }


    private RenderTexture2D _tempGeoL = new(0, 0);

    private bool _shouldRedrawLevel = true;

    private void RedrawLevel()
    {
        var lWidth = GLOBALS.Level.Width * 16;
        var lHeight = GLOBALS.Level.Height * 16;

        var paletteTiles = GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette;

        BeginTextureMode(GLOBALS.Textures.GeneralLevel);
        ClearBackground(new(170, 170, 170, 255));
        EndTextureMode();
        
        #region TileEditorLayer3
        if (_showTileLayer3)
        {
            // Draw geos first
            if (paletteTiles) {
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 2, 16, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

                // BeginTextureMode(geoL);
                // ClearBackground(Color.White with { A = 0 });
                
                // Printers.DrawGeoLayer(
                //     2, 
                //     16, 
                //     false, 
                //     Color.Black
                // );
                // EndTextureMode();

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                var shader = GLOBALS.Shaders.Palette;

                BeginShaderMode(shader);

                var textureLoc = GetShaderLocation(shader, "inputTexture");
                var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                var depthLoc = GetShaderLocation(shader, "depth");
                var shadingLoc = GetShaderLocation(shader, "shading");

                SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
                SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

                SetShaderValue(shader, depthLoc, 20, ShaderUniformDataType.Int);
                SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

                DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 2 ? Color.Black : Color.Black with { A = 120 });

                EndShaderMode();

                EndTextureMode();
            } else {
                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                Printers.DrawGeoLayer(
                    2, 
                    16, 
                    false, 
                    GLOBALS.Layer == 2 ? Color.Black : Color.Black with { A = 120 }
                );

                EndTextureMode();
            }


            // then draw the tiles



            if (_showLayer3Tiles)
            {
                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                    // Printers.DrawTileLayer(
                    //     GLOBALS.Layer,
                    //     2, 
                    //     16, 
                    //     false, 
                    //     TileDrawMode.Palette,
                    //     GLOBALS.SelectedPalette!.Value,
                    //     70
                    // );
                    Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 2, 16, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true);

                } else {
                    BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                    Printers.DrawTileLayer(
                        GLOBALS.Layer,
                        2, 
                        16, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255)
                    );

                    EndTextureMode();
                }
            }
            
            // Then draw the props

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 2) {

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);


                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth > -20) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(current.type, current.tile, category, index, current.prop, 16, GLOBALS.Settings.GeneralSettings.DrawPropMode, GLOBALS.SelectedPalette);


                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
                                }
                            }
                        }
                    }
                }

                EndTextureMode();
            }

        }
        #endregion

        #region TileEditorLayer2
        if (_showTileLayer2 && (GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers || GLOBALS.Layer is 0 or 1))
        {
            if (paletteTiles) {
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 1, 16, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

                // BeginTextureMode(geoL);
                // ClearBackground(Color.White with { A = 0 });
                
                // Printers.DrawGeoLayer(
                //     1, 
                //     16, 
                //     false, 
                //     Color.Black
                // );
                // EndTextureMode();

                var shader = GLOBALS.Shaders.Palette;

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                BeginShaderMode(shader);

                var textureLoc = GetShaderLocation(shader, "inputTexture");
                var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                var depthLoc = GetShaderLocation(shader, "depth");
                var shadingLoc = GetShaderLocation(shader, "shading");

                SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
                SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

                SetShaderValue(shader, depthLoc, 10, ShaderUniformDataType.Int);
                SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

                DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 1 ? Color.Black : Color.Black with { A = 140 });

                EndShaderMode();

                EndTextureMode();
            } else {
                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                Printers.DrawGeoLayer(
                    1, 
                    16, 
                    false, 
                    GLOBALS.Layer == 1
                        ? Color.Black 
                        : Color.Black with { A = 140 }
                );

                EndTextureMode();
            }



            // Draw layer 2 tiles

            if (_showLayer2Tiles)
            {
                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                    Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 1, 16, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true);
                    
                    
                    // Printers.DrawTileLayer(
                    //     GLOBALS.Layer,
                    //     1, 
                    //     16, 
                    //     false, 
                    //     TileDrawMode.Palette,
                    //     GLOBALS.SelectedPalette!.Value,
                    //     70
                    // );
                } else {
                    BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                    Printers.DrawTileLayer(
                        GLOBALS.Layer,
                        1, 
                        16, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255)
                    );

                    EndTextureMode();
                }
            }

            BeginTextureMode(GLOBALS.Textures.GeneralLevel);

            
            // then draw the props

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 1) {
                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth > -10 || current.prop.Depth < -19) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(current.type, current.tile, category, index, current.prop, 16, GLOBALS.Settings.GeneralSettings.DrawPropMode, GLOBALS.SelectedPalette);

                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
                                }
                            }
                        }
                    }
                }

                EndTextureMode();
            }

            
        }
        #endregion

        if (GLOBALS.Settings.GeneralSettings.Water && !GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1) {
            BeginTextureMode(GLOBALS.Textures.GeneralLevel);
            DrawRectangle(
                (-1) * 16,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 16,
                (GLOBALS.Level.Width + 2) * 16,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 16,
                new Color(0, 0, 255, (int)GLOBALS.Settings.GeneralSettings.WaterOpacity)
            );
            EndTextureMode();
        }

        #region TileEditorLayer1
        if (_showTileLayer1 && (GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers || GLOBALS.Layer == 0))
        {
            if (paletteTiles) {
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 0, 16, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

                // BeginTextureMode(geoL);
                // ClearBackground(Color.White with { A = 0 });
                
                // Printers.DrawGeoLayer(
                //     0, 
                //     16, 
                //     false, 
                //     Color.Black
                // );
                // EndTextureMode();

                var shader = GLOBALS.Shaders.Palette;

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                BeginShaderMode(shader);

                var textureLoc = GetShaderLocation(shader, "inputTexture");
                var paletteLoc = GetShaderLocation(shader, "paletteTexture");

                var depthLoc = GetShaderLocation(shader, "depth");
                var shadingLoc = GetShaderLocation(shader, "shading");

                SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
                SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

                SetShaderValue(shader, depthLoc, 0, ShaderUniformDataType.Int);
                SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

                DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 0 ? Color.Black : Color.Black with { A = 120 });

                EndShaderMode();

                EndTextureMode();
            } else {
                BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                Printers.DrawGeoLayer(
                    0, 
                    16, 
                    false, 
                    GLOBALS.Layer == 0
                        ? Color.Black 
                        : Color.Black with { A = 120 }
                );

                EndTextureMode();
            }


            // Draw layer 1 tiles

            if (_showLayer1Tiles)
            {
                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                        Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 0, 16, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true);
                    
                    // Printers.DrawTileLayer(
                    //     GLOBALS.Layer,
                    //     0, 
                    //     16, 
                    //     false, 
                    //     TileDrawMode.Palette,
                    //     GLOBALS.SelectedPalette!.Value,
                    //     70
                    // );
                } else {
                    BeginTextureMode(GLOBALS.Textures.GeneralLevel);
                    Printers.DrawTileLayer(
                        GLOBALS.Layer,
                        0, 
                        16, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255)
                    );
                    EndTextureMode();
                }
            }

            // then draw the props

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 0) {

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);


                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.prop.Depth < -9) continue;

                    var (category, index) = current.position;
                    var quads = current.prop.Quads;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(current.type, current.tile, category, index, current.prop, 16, GLOBALS.Settings.GeneralSettings.DrawPropMode, GLOBALS.SelectedPalette);

                    // Draw Rope Point
                    if (current.type == InitPropType.Rope)
                    {
                        foreach (var point in current.prop.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.prop.Quads), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.type == InitPropType.Rope)
                            {
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                DrawCircleV(
                                    Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
                                        new(2f, 2f)), 
                                    5f, 
                                    Color.Blue
                                );
                                
                                /*DrawCircleV(quads.TopLeft, 2f, Color.Blue);
                                DrawCircleV(quads.TopRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 2f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 2f, Color.Blue);*/
                            }
                            else if (current.type == InitPropType.Long)
                            {
                                var sides = Utils.LongSides(current.prop.Quads);
                                
                                DrawCircleV(sides.left, 5f, Color.Blue);
                                DrawCircleV(sides.top, 5f, Color.Blue);
                                DrawCircleV(sides.right, 5f, Color.Blue);
                                DrawCircleV(sides.bottom, 5f, Color.Blue);
                            }
                            else
                            {
                                DrawCircleV(quads.TopLeft, 5f, Color.Blue);
                                DrawCircleV(quads.TopRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomRight, 5f, Color.Blue);
                                DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
                            }
                        }
                        else if (_scalingProps)
                        {
                            var center = Utils.QuadsCenter(ref quads);
                            
                            switch (_stretchAxes)
                            {
                                case 1:
                                    DrawLineEx(
                                        center with { X = -10 }, 
                                        center with { X = GLOBALS.Level.Width*GLOBALS.PreviewScale + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*GLOBALS.PreviewScale + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.type == InitPropType.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.prop.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }

                            if (_ropeMode)
                            {
                                // p is copied as suggested by the code editor..
                                var p1 = p;
                                var model = _models.Single(r => r.index == p1);

                                if (!model.simSwitch)
                                {
                                    foreach (var handle in model.bezierHandles) DrawCircleV(handle, 3f, Color.Green);
                                }
                            }
                        }
                    }
                }

                EndTextureMode();
            }
        
        }
        if (GLOBALS.Settings.GeneralSettings.Water && GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1) {
            BeginTextureMode(GLOBALS.Textures.GeneralLevel);
            DrawRectangle(
                (-1) * 16,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 16,
                (GLOBALS.Level.Width + 2) * 16,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 16,
                new Color(0, 0, 255, (int)GLOBALS.Settings.GeneralSettings.WaterOpacity)
            );
            EndTextureMode();
        }
        #endregion
    }
    
    private bool _showLayer1Tiles = true;
    private bool _showLayer2Tiles = true;
    private bool _showLayer3Tiles = true;

    private bool _showTileLayer1 = true;
    private bool _showTileLayer2 = true;
    private bool _showTileLayer3 = true;

    private int _menuRootCategoryIndex;
    
    private int _propsMenuTilesCategoryIndex;
    private int _propsMenuTilesIndex;

    private int _propsMenuRopesIndex;

    private int _propsMenuLongsIndex;

    private int _propsMenuOthersCategoryIndex;
    private int _propsMenuOthersIndex;

    private int _snapMode;

    private int _quadLock;
    private int _pointLock = -1;
    private int _bezierHandleLock = -1;

    private bool _clickTracker;

    private int _mode;

    private bool _movingProps;
    private bool _rotatingProps;
    private bool _scalingProps;
    private bool _stretchingProp;
    private bool _editingPropPoints;
    private bool _ropeMode;

    private bool _noCollisionPropPlacement;

    private byte _stretchAxes;

    // 0 - 360
    private int _placementRotation;

    private int _placementRotationSteps = 1;

    private int _ropeSimulationFrameCut = 1;
    private int _ropeSimulationFrame;

    //
    private string _tileAsPropSearchText = "";

    private int _tileAsPropSearchCategoryIndex = -1;
    private int _tileAsPropSearchIndex = -1;
    private record TileAsPropSearchResult(
        (string name, int originalIndex)[] Categories,
        (TileDefinition tile, int originalIndex)[][] Tiles
    );

    private TileAsPropSearchResult? _tileAsPropSearchResult = null;

    private void SearchTiles() {
        if (GLOBALS.TileDex is null || string.IsNullOrEmpty(_tileAsPropSearchText)) {
            _tileAsPropSearchResult = null;
            _tileAsPropSearchCategoryIndex = -1;
            return;
        }

        var dex = GLOBALS.TileDex;

        List<(string, int)> categories = [];
        List<(TileDefinition, int)[]> tiles = [];

        for (var c = 0; c < dex.OrderedTilesAsProps.Length; c++) {
            List<(TileDefinition, int)> foundTiles = [];

            for (var t = 0; t < dex.OrderedTilesAsProps[c].Length; t++) {
                var tile = dex.OrderedTilesAsProps[c][t];

                if (tile.Name.Contains(_tileAsPropSearchText, StringComparison.InvariantCultureIgnoreCase)) {
                    foundTiles.Add((tile, t));
                }
            }

            if (foundTiles is not []) {
                categories.Add((dex.OrderedTileAsPropCategories[c], c));
                tiles.Add([..foundTiles]);
            }
        }

        _tileAsPropSearchResult = new([..categories], [..tiles]);

        if (categories.Count > 0) {
            _tileAsPropSearchCategoryIndex = 0;
        } else {
            _tileAsPropSearchCategoryIndex = -1;
        }
    }
    //

    //
    private string _propSearchText = "";

    private int _propSearchCategoryIndex = -1;
    private int _propSearchIndex = -1;
    private record PropSearchResult(
        (string name, int originalIndex)[] Categories,
        (InitPropBase prop, int originalIndex)[][] Props
    );

    private PropSearchResult? _propSearchResult = null;

    private void SearchProps() {
        if (GLOBALS.PropCategories is null or { Length: 0 } || GLOBALS.Props is null or { Length: 0 } || string.IsNullOrEmpty(_propSearchText)) {
            _propSearchResult = null;
            _propSearchCategoryIndex = -1;
            return;
        }

        List<(string, int)> categories = [];
        List<(InitPropBase, int)[]> props = [];

        for (var c = 0; c < GLOBALS.Props.Length; c++) {
            List<(InitPropBase, int)> foundProps = [];
            
            for (var p = 0; p < GLOBALS.Props[c].Length; p++) {
                var prop = GLOBALS.Props[c][p];

                if (prop.Name.Contains(_propSearchText, StringComparison.InvariantCultureIgnoreCase)) {
                    foundProps.Add((prop, p));
                }
            }

            if (foundProps is not []) {
                categories.Add((GLOBALS.PropCategories[c].Item1, c));
                props.Add([..foundProps]);
            }
        }

        _propSearchResult = new([..categories], [..props]);

        if (categories.Count > 0) {
            _propSearchCategoryIndex = 0;
        } else {
            _propSearchCategoryIndex = -1;
        }
    }
    //
    
    private Vector2 _selection1 = new(-100, -100);
    private Rectangle _selection;
    private Rectangle _selectedPropsEncloser;
    private Vector2 _selectedPropsCenter;

    private bool[] _selected = [];
    private bool[] _hidden = [];

    private int[] _selectedCycleIndices = [];
    private int _selectedCycleCursor;

    private readonly (string name, Color color)[] _propCategoriesOnly = GLOBALS.PropCategories[..^2]; // a risky move..

    private TileDefinition? _currentTile;
    private TileDefinition? _hoveredTile;

    private readonly string[] _ropeNames = [..GLOBALS.RopeProps.Select(p => p.Name)];
    private readonly string[] _longNames = [..GLOBALS.LongProps.Select(l => l.Name)];
    private readonly string[] _otherCategoryNames;
    private readonly string[][] _otherNames;

    private BasicPropSettings _copiedPropSettings = new();
    private Vector2[] _copiedRopePoints = [];
    private int _copiedDepth;
    private bool _copiedIsTileAsProp;
    private bool _newlyCopied; // to signify that the copied properties should be used

    private bool _showGrid;

    private int _defaultDepth;
    private int _defaultVariation;
    private int _defaultSeed;

    private void UpdateDefaultDepth()
    {
        _defaultDepth = -GLOBALS.Layer * 10;
    }

    private Vector2? _propsMoveMouseAnchor;
    private Vector2? _propsMoveMousePos;
    private Vector2 _propsMoveMouseDelta = new(0, 0);
    
    private (int index, bool simSwitch, RopeModel model, Vector2[] bezierHandles)[] _models;
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isPropsWinHovered;
    private bool _isPropsWinDragged;

    private bool _isNavbarHovered;

    private bool _isPropsListHovered;

    private PropGram _gram = new(30);

    public PropsEditorPage()
    {
        _camera = new Camera2D { Zoom = 0.8f };

        _otherCategoryNames = [..from c in _propCategoriesOnly select c.name];
        _otherNames = GLOBALS.Props.Select(c => c.Select(p => p.Name).ToArray()).ToArray();

        _gram.Proceed(GLOBALS.Level.Props);
    }

    private void Undo() {
        if (_gram.CurrentAction is null) return;

        _gram.Undo();

        GLOBALS.Level.Props = [.._gram.CurrentAction]; 

        ImportRopeModels();

        _selected = new bool[GLOBALS.Level.Props.Length];
        _hidden = new bool[GLOBALS.Level.Props.Length];

        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
        else {
            _shouldRedrawLevel = true;
        }
    }

    private void Redo() {
        _gram.Redo();

        if (_gram.CurrentAction is null) return;

        GLOBALS.Level.Props = [.._gram.CurrentAction];

        ImportRopeModels();

        _selected = new bool[GLOBALS.Level.Props.Length];
        _hidden = new bool[GLOBALS.Level.Props.Length];

        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
        else {
            _shouldRedrawLevel = true;
        }
    }

    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        var lWidth = GLOBALS.Level.Width * 16;
        var lHeight = GLOBALS.Level.Height * 16;

        ImportRopeModels();
        _selected = new bool[GLOBALS.Level.Props.Length];

        if (lWidth != _propLayerRT.Raw.Texture.Width || lHeight != _propLayerRT.Raw.Texture.Height) {
            _propLayerRT.Dispose();
            _propLayerRT = new RenderTexture2D(lWidth, lHeight);
        }


        if (lWidth != _tempGeoL.Raw.Texture.Width || lHeight != _tempGeoL.Raw.Texture.Height) {
            _tempGeoL.Dispose();
            _tempGeoL = new(lWidth, lHeight);
        }
    }

    public void OnProjectCreated(object? sender, EventArgs e)
    {
        var lWidth = GLOBALS.Level.Width * 16;
        var lHeight = GLOBALS.Level.Height * 16;

        ImportRopeModels();
        _selected = new bool[GLOBALS.Level.Props.Length];

        if (lWidth != _propLayerRT.Raw.Texture.Width || lHeight != _propLayerRT.Raw.Texture.Height) {
            _propLayerRT.Dispose();
            _propLayerRT = new RenderTexture2D(lWidth, lHeight);

        }


        if (lWidth != _tempGeoL.Raw.Texture.Width || lHeight != _tempGeoL.Raw.Texture.Height) {
            _tempGeoL.Dispose();
            _tempGeoL = new(lWidth, lHeight);
        }
    }
    #nullable disable

    public void OnPageUpdated(int previous, int @next) {
        if (@next == 8) {
            _shouldRedrawLevel = true;
            _shouldRedrawPropLayer = true;
            _defaultDepth = -10 * GLOBALS.Layer;
        }
    }

    private void ImportRopeModels()
    {
        List<(int, bool, RopeModel, Vector2[])> models = [];
        
        for (var r = 0; r < GLOBALS.Level.Props.Length; r++)
        {
            var current = GLOBALS.Level.Props[r];
            
            if (current.type != InitPropType.Rope) continue;
            
            var newModel = new RopeModel(current.prop, GLOBALS.RopeProps[current.position.index], current.prop.Extras.RopePoints.Length);
           
            var quad = current.prop.Quads;
            var quadCenter = Utils.QuadsCenter(ref quad);

            models.Add((r, true, newModel, [ quadCenter ]));
        }

        _models = [..models];
    }

    private void UpdateRopeModelSegments()
    {
        List<(int, bool, RopeModel, Vector2[])> models = [];
        
        for (var r = 0; r < _models.Length; r++)
        {
            var current = _models[r];
            
            models.Add((
                current.index, 
                current.simSwitch, 
                new RopeModel(
                    GLOBALS.Level.Props[current.index].prop, 
                    GLOBALS.RopeProps[GLOBALS.Level.Props[current.index].position.index], 
                    GLOBALS.Level.Props[current.index].prop.Extras.RopePoints.Length), 
                current.bezierHandles
            ));
        }

        _models = [..models];
    }

    private void DecrementMenuIndex()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // tiles as props
                if (GLOBALS.TileDex is null) return;
                
                _propsMenuTilesIndex--;
                
                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
                _currentTile =
                    GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                break;
                    
            case 1: // Ropes
                _propsMenuRopesIndex--;
                
                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
                else Utils.Restrict(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
                break;
            
            case 2: // Longs 
                _propsMenuLongsIndex--;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
                else Utils.Restrict(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
                break;
                    
            case 3: // props
                _propsMenuOthersIndex--;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                break;
        }
    }

    private void ToNextInnerCategory()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // Tiles-As-Props
                if (GLOBALS.TileDex is null) return;

                _propsMenuTilesCategoryIndex++;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
                else Utils.Restrict(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
                
                if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuTilesIndex = 0;
                else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
                _currentTile =
                    GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                break;
            
            case 3: // Other
                _propsMenuOthersCategoryIndex++;
                
                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);
                else Utils.Restrict(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);;
                
                if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuOthersIndex = 0;
                else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                break;
        }
    }

    private void ToPreviousInnerCategory()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // Tiles-As-Props
                if (GLOBALS.TileDex is null) return;
                _propsMenuTilesCategoryIndex--;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
                else Utils.Restrict(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);

                if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuTilesIndex = 0;
                else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);

                _currentTile =
                    GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                break;
            
            case 3: // Other
                _propsMenuOthersCategoryIndex--;
                
                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);
                else Utils.Restrict(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);;
                
                if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuOthersIndex = 0;
                else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                break;
        }
    }

    private void IncrementMenuIndex()
    {
        switch (_menuRootCategoryIndex)
        {
            case 0: // Tiles as props
                if (GLOBALS.TileDex is null) return;
                _propsMenuTilesIndex++;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
                _currentTile =
                    GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                break;
            
            case 1: // Ropes
                _propsMenuRopesIndex++;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
                else Utils.Restrict(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
                break;
            
            case 2: // Longs 
                _propsMenuLongsIndex++;

                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
                else Utils.Restrict(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
                break;
            
            case 3: // Props
                _propsMenuOthersIndex++;
                
                if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
                break;
        }
    }

    private Rectangle GetLayerIndicator(int layer) => layer switch {
        0 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
            PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 40,                        50 + 25,                     40, 40 ), 
            PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 70,     50 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 70,     GetScreenHeight() - 90 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 40,                        GetScreenHeight() - 90 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 50 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 90 - 25, 40, 40 ),

            _ => new Rectangle(40, GetScreenHeight() - 80, 40, 40)
        },
        1 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
            PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 30,                        40 + 25,                     40, 40 ), 
            PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 60,     40 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 60,     GetScreenHeight() - 80 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 30,                        GetScreenHeight() - 80 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 40 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 80 - 25, 40, 40 ),

            _ => new Rectangle(30, GetScreenHeight() - 70, 40, 40)
        },
        2 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
            PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 20,                        30 + 25,                     40, 40 ), 
            PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 50,     30 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 50,     GetScreenHeight() - 70 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 20,                        GetScreenHeight() - 70 - 25, 40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 30 + 25,                     40, 40 ),
            PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 70 - 25, 40, 40 ),

            _ => new Rectangle(20, GetScreenHeight() - 60, 40, 40)
        },
        
        _ => new Rectangle(0, 0, 40, 40)
    };

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        if (_currentTile is null) _currentTile = GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
        
        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
            
        if (_selected.Length != GLOBALS.Level.Props.Length)
        {
            _selected = new bool[GLOBALS.Level.Props.Length];
        }

        if (_hidden.Length != GLOBALS.Level.Props.Length)
        {
            _hidden = new bool[GLOBALS.Level.Props.Length];
        }

        var previewScale = GLOBALS.PreviewScale;

        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();
        
        var layer3Rect = GetLayerIndicator(2);
        var layer2Rect = GetLayerIndicator(1);
        var layer1Rect = GetLayerIndicator(0);

        var tileMouse = GetMousePosition();
        var tileMouseWorld = GetScreenToWorld2D(tileMouse, _camera);

        var menuPanelRect = new Rectangle(sWidth - 360, 0, 360, sHeight);

        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / previewScale;
        var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / previewScale;

        var canDrawTile = !_isPropsListHovered && !_isNavbarHovered && 
                        !_isPropsWinHovered && 
                          !_isPropsWinDragged && 
                          !_isShortcutsWinHovered && 
                          !_isShortcutsWinDragged && 
                          !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        // Undo

        if (_shortcuts.Undo.Check(ctrl, shift, alt)) {
            Undo();
        }
        
        // Redo

        if (_shortcuts.Redo.Check(ctrl, shift, alt)) {
            Redo();
        }

        // handle mouse drag
        if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // handle zoom
        var tileWheel = GetMouseWheelMove();
        if (tileWheel != 0 && canDrawTile)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }
        
        // Cycle layer
        if (_shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;

            UpdateDefaultDepth();

            _shouldRedrawLevel = true;
            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
        }

        if (_shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) {
            _showLayer1Tiles = !_showLayer1Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (_shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) {
            _showLayer2Tiles = !_showLayer2Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (_shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) {
            _showLayer3Tiles = !_showLayer3Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        
        // Cycle Mode
        if (_shortcuts.CycleModeRight.Check(ctrl, shift, alt))
        {
            _mode = ++_mode % 2;
        }
        else if (_shortcuts.CycleModeLeft.Check(ctrl, shift, alt))
        {
            _mode--;
            if (_mode < 0) _mode = 1;
        }
        
        if (_shortcuts.ToggleLayer1.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer1 = !_showTileLayer1;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (_shortcuts.ToggleLayer2.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer2 = !_showTileLayer2;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (_shortcuts.ToggleLayer3.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer3 = !_showTileLayer3;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }

        if (_shortcuts.CycleSnapMode.Check(ctrl, shift, alt)) _snapMode = ++_snapMode % 3;

        // Mode-based hotkeys
        switch (_mode)
        {
            case 1: // Place Mode
                if (!canDrawTile) break;

                if (_shortcuts.CycleVariations.Check(ctrl, shift, alt)) {
                    if (_menuRootCategoryIndex == 3 && GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex] is IVariableInit v) _defaultVariation = (_defaultVariation + 1) % v.Variations;
                    else _defaultVariation = 0;
                }

                if (_ropeInitialPlacement) {
                    var (index, _, model, _) = _models.LastOrDefault();
                    
                    if (model is not null && index < GLOBALS.Level.Props.Length) {
                        if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
                            _additionalInitialRopeSegments++;
                        }

                        if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
                            _additionalInitialRopeSegments--;
                            // Utils.Restrict(ref _additionalInitialRopeSegments, 0);
                        }

                        var (_, _, (_, initIndex), foundProp) = GLOBALS.Level.Props[index];

                        var middleLeft = Raymath.Vector2Divide(
                            Raymath.Vector2Add(foundProp.Quads.TopLeft, foundProp.Quads.BottomLeft),
                            new(2f, 2f)
                        );

                        var middleRight = Raymath.Vector2Divide(
                            Raymath.Vector2Add(foundProp.Quads.TopRight, foundProp.Quads.BottomRight),
                            new(2f, 2f)
                        );
                        
                        // Attach vertex
                        {
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(foundProp.Quads.TopLeft, middleLeft));
                        
                            var currentQuads = foundProp.Quads;

                            currentQuads.BottomLeft = Raymath.Vector2Add(
                                middleLeft, 
                                new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                            );
                            
                            currentQuads.TopLeft = Raymath.Vector2Add(
                                middleLeft, 
                                new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                            );
                            
                            currentQuads.BottomRight = Raymath.Vector2Add(
                                tileMouseWorld, 
                                new Vector2(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                            );
                            
                            currentQuads.TopRight = Raymath.Vector2Add(
                                tileMouseWorld, 
                                new Vector2(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                            );

                            foundProp.Quads = currentQuads;
                        }

                        // Adjust segment count
                        var init = GLOBALS.Ropes[initIndex];
                        var endsDistance = (int)Raymath.Vector2Distance(middleLeft, middleRight) / (10 + init.SegmentLength);
                        if (endsDistance > 0 && ++_ropeSimulationFrame % 6 == 0) {
                            var ropePoints = foundProp.Extras.RopePoints;
                            var targetCount = endsDistance + _additionalInitialRopeSegments;

                            Utils.Restrict(ref targetCount, 3);

                            var deficit = targetCount - ropePoints.Length;
                            
                            if (targetCount > ropePoints.Length) {
                                ropePoints = ([..ropePoints, ..Enumerable.Range(0, deficit).Select((i) => tileMouseWorld)]);
                            } else if (targetCount < ropePoints.Length) {
                                ropePoints = ropePoints.Take(targetCount).ToArray();
                            }

                            // foundProp.Extras.RopePoints = ropePoints;
                            model.UpdateSegments(ropePoints);
                        }

                        model?.Update(foundProp.Quads, foundProp.Depth switch
                                {
                                    < -19 => 2,
                                    < -9 => 1,
                                    _ => 0
                                });

                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }
                    }
                }

                if (_longInitialPlacement && GLOBALS.Level.Props is { Length: > 0 }) {
                    var currentQuads = GLOBALS.Level.Props[^1].prop.Quads;
                    var (left, top, right, bottom) = Utils.LongSides(currentQuads);
                            
                    var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                    
                    var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, left));

                    currentQuads.BottomLeft = Raymath.Vector2Add(
                        left, 
                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                    );
                
                    currentQuads.TopLeft = Raymath.Vector2Add(
                        left, 
                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                    );
                
                    currentQuads.BottomRight = Raymath.Vector2Add(
                        tileMouseWorld, 
                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                    );
                
                    currentQuads.TopRight = Raymath.Vector2Add(
                        tileMouseWorld, 
                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                    );

                    GLOBALS.Level.Props[^1].prop.Quads = currentQuads;

                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }
                }

                // Place Prop
                if (_noCollisionPropPlacement)
                {
                    if (!_lockedPlacement && canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt, true) ||
                                        _shortcuts.PlacePropAlt.Check(ctrl, shift, alt, true)))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }
                        
                        var posV = _snapMode switch
                        {
                            1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                            2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                if (_currentTile is null) break;
                                
                                var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                
                                BasicPropSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }

                                var placementQuad = new PropQuad(
                                    new Vector2(posV.X - width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y + height),
                                    new Vector2(posV.X - width, posV.Y + height)
                                );

                                foreach (var prop in GLOBALS.Level.Props)
                                {
                                    var propRec = Utils.EncloseQuads(prop.prop.Quads);
                                    var newPropRec = Utils.EncloseQuads(placementQuad);
                                    
                                    if (prop.prop.Depth == _defaultDepth && CheckCollisionRecs(newPropRec, propRec)) goto skipPlacement;
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        InitPropType.Tile, 
                                        _currentTile,
                                        (-1, -1),
                                        new Prop(
                                            _defaultDepth, 
                                            _currentTile.Name, 
                                            true, 
                                            placementQuad
                                        )
                                        {
                                            Extras = new PropExtras(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;

                            case 1: // Ropes
                            {
                                if (_ropeInitialPlacement) {
                                    _ropeInitialPlacement = false;

                                } else {
                                    _ropeInitialPlacement = true;
                                    _additionalInitialRopeSegments = 0;
                                    
                                    var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
                                    var newQuads = new PropQuad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y)
                                    };

                                    var ropeEnds = Utils.RopeEnds(newQuads);
                                    
                                    PropRopeSettings settings;
                                    Vector2[] ropePoints;

                                    if (_newlyCopied)
                                    {
                                        _newlyCopied = false;

                                        ropePoints = _copiedRopePoints;
                                        settings = (PropRopeSettings)_copiedPropSettings;
                                        _defaultDepth = _copiedDepth;
                                    }
                                    else
                                    {
                                        // ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
                                        ropePoints = [tileMouseWorld];
                                        settings = new(thickness: current.Name is "Wire" or "Zero-G Wire" ? 2 : null);
                                    }

                                        
                                    GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                        (
                                            InitPropType.Rope, 
                                            null,
                                            (-1, _propsMenuRopesIndex), 
                                            new Prop(
                                                _defaultDepth, 
                                                current.Name, 
                                                false, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras(
                                                    settings, 
                                                    ropePoints
                                                )
                                            }
                                        ) 
                                    ];

                                    _selected = new bool[GLOBALS.Level.Props.Length];
                                    _selected[^1] = true;

                                    ImportRopeModels();
                                }
                            }
                                break;

                            case 2: // Long Props
                            {
                                if (_clickTracker) break;
                                _clickTracker = true;

                                if (_longInitialPlacement) {
                                    _longInitialPlacement = false;
                                } else {
                                    _longInitialPlacement = true;

                                    var current = GLOBALS.LongProps[_propsMenuLongsIndex];
                                    ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                                    var height = texture.Height / 2f;
                                    var newQuads = new PropQuad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
                                    };
                                    
                                    PropLongSettings settings;

                                    if (_newlyCopied)
                                    {
                                        _newlyCopied = false;

                                        settings = (PropLongSettings)_copiedPropSettings;
                                        _defaultDepth = _copiedDepth;
                                    }
                                    else
                                    {
                                        settings = new();
                                    }
                                    
                                    GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                        (
                                            InitPropType.Long, 
                                            null,
                                            (-1, _propsMenuLongsIndex), 
                                            new Prop(
                                                _defaultDepth, 
                                                current.Name, 
                                                false, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras(settings, [])
                                            }
                                        ) 
                                    ];

                                    _selected = new bool[GLOBALS.Level.Props.Length];
                                    _selected[^1] = true;
                                }
                            }
                                break;

                            case 3: // Others
                            {
                                var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                
                                var (width, height, settings) = init switch
                                {
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                    InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                    InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                    InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                    InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                    InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                    InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                    InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                    InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
                                    _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
                                };

                                if (settings is PropVariedSoftSettings vs && init is InitVariedSoftProp i && i.Colorize == 1) {
                                    vs.ApplyColor = 1;
                                }

                                if (settings is ICustomDepth cd) {
                                    cd.CustomDepth = init switch
                                    {
                                        InitVariedStandardProp v => v.Repeat.Length,
                                        InitStandardProp s => s.Repeat.Length,
                                        _ => init.Depth
                                    };
                                }

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;
                                
                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        init.Type, 
                                        null,
                                        (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex),
                                        new Prop(
                                            _defaultDepth, 
                                            init.Name, 
                                            false, 
                                            new PropQuad(
                                            new(posV.X - width, posV.Y - height), 
                                            new(posV.X + width, posV.Y - height), 
                                            new(posV.X + width, posV.Y + height), 
                                            new(posV.X - width, posV.Y + height))
                                        )
                                        {
                                            Extras = new PropExtras(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;
                        }
                        
                        // Do not forget to update _selected and _hidden

                        _selected = [.._selected, false];
                        _hidden = [.._hidden, false];
                        
                        skipPlacement:
                        {
                        }
                    }

                    if (IsMouseButtonReleased(_shortcuts.PlaceProp.Button) || IsKeyReleased(_shortcuts.PlacePropAlt.Key)) {
                        _gram.Proceed(GLOBALS.Level.Props);

                        if (_lockedPlacement) {
                            _lockedPlacement = false;
                        }

                        if (_clickTracker) {
                            _clickTracker = false;
                        }
                    }
                }
                else
                {
                    if (canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt) || _shortcuts.PlacePropAlt.Check(ctrl, shift, alt)))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else _shouldRedrawLevel = true;
                        
                        var posV = _snapMode switch
                        {
                            1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                            2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * GLOBALS.PreviewScale / 2;
                                
                                BasicPropSettings settings;

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }
                                else
                                {
                                    settings = new();
                                }

                                var quads = new PropQuad(
                                    new Vector2(posV.X - width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y - height),
                                    new Vector2(posV.X + width, posV.Y + height),
                                    new Vector2(posV.X - width, posV.Y + height)
                                );

                                quads = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        InitPropType.Tile, 
                                        _currentTile,
                                        (-1, -1),
                                        new Prop(
                                            _defaultDepth, 
                                            _currentTile.Name, 
                                            true, 
                                            quads
                                        )
                                        {
                                            Extras = new(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;

                            case 1: // Ropes
                            {
                                if (_ropeInitialPlacement) {
                                    _ropeInitialPlacement = false;

                                } else {
                                    _ropeInitialPlacement = true;
                                    _additionalInitialRopeSegments = 0;
                                    
                                    var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
                                    const float height = 10f;
                                    var newQuads = new PropQuad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
                                    };

                                    var ropeEnds = Utils.RopeEnds(newQuads);
                                    
                                    PropRopeSettings settings;
                                    Vector2[] ropePoints;

                                    if (_newlyCopied)
                                    {
                                        _newlyCopied = false;

                                        ropePoints = _copiedRopePoints;
                                        settings = (PropRopeSettings)_copiedPropSettings;
                                        _defaultDepth = _copiedDepth;
                                    }
                                    else
                                    {
                                        // ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
                                        ropePoints = [tileMouseWorld];
                                        settings = new(thickness: current.Name is "Wire" or "Zero-G Wire" ? 2 : null);
                                    }

                                        
                                    GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                        (
                                            InitPropType.Rope, 
                                            null,
                                            (-1, _propsMenuRopesIndex), 
                                            new Prop(
                                                _defaultDepth, 
                                                current.Name, 
                                                false, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras(
                                                    settings, 
                                                    ropePoints
                                                )
                                            }
                                        ) 
                                    ];

                                    _selected = new bool[GLOBALS.Level.Props.Length];
                                    _selected[^1] = true;

                                    ImportRopeModels();
                                }
                            }
                                break;

                            case 2: // Long Props
                            {
                                if (_longInitialPlacement) {
                                    _longInitialPlacement = false;
                                } else {
                                    _longInitialPlacement = true;

                                    var current = GLOBALS.LongProps[_propsMenuLongsIndex];
                                    ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                                    var height = texture.Height / 2f;
                                    var newQuads = new PropQuad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
                                    };
                                    
                                    PropLongSettings settings;

                                    if (_newlyCopied)
                                    {
                                        _newlyCopied = false;

                                        settings = (PropLongSettings)_copiedPropSettings;
                                        _defaultDepth = _copiedDepth;
                                    }
                                    else
                                    {
                                        settings = new();
                                    }
                                    
                                    GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
                                        (
                                            InitPropType.Long, 
                                            null,
                                            (-1, _propsMenuLongsIndex), 
                                            new Prop(
                                                _defaultDepth, 
                                                current.Name, 
                                                false, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras(settings, [])
                                            }
                                        ) 
                                    ];

                                    _selected = new bool[GLOBALS.Level.Props.Length];
                                    _selected[^1] = true;
                                }
                            }
                                break;

                            case 3: // Others
                            {
                                var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                
                                var (width, height, settings) = init switch
                                {
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                    InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                    InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                    InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                    InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                    InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                    InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                    InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                    InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
                                    _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
                                };

                                if (settings is PropVariedSoftSettings vs && init is InitVariedSoftProp i && i.Colorize == 1) {
                                    vs.ApplyColor = 1;
                                }

                                if (settings is ICustomDepth cd) {
                                    cd.CustomDepth = init switch
                                    {
                                        InitVariedStandardProp v => v.Repeat.Length,
                                        InitStandardProp s => s.Repeat.Length,
                                        _ => init.Depth
                                    };
                                }

                                if (_newlyCopied)
                                {
                                    _newlyCopied = false;

                                    settings = _copiedPropSettings;
                                    _defaultDepth = _copiedDepth;
                                }

                                var quads = new PropQuad(
                                    new(posV.X - width, posV.Y - height),
                                    new(posV.X + width, posV.Y - height),
                                    new(posV.X + width, posV.Y + height),
                                    new(posV.X - width, posV.Y + height));

                                quads = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    (
                                        init.Type, 
                                        null,
                                        (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex),
                                        new Prop(
                                            _defaultDepth, 
                                            init.Name, 
                                            false, 
                                            quads)
                                        {
                                            Extras = new PropExtras(settings, [])
                                        }
                                    )
                                ];
                            }
                                break;
                        }
                        
                        // Do not forget to update _selected and _hidden

                        _selected = [.._selected, false];
                        _hidden = [.._hidden, false];

                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                }
                
                if (_shortcuts.RotateClockwise.Check(ctrl, shift, alt, true)) {
                    _placementRotation += 1;
                }

                if (_shortcuts.RotateCounterClockwise.Check(ctrl, shift, alt, true)) {
                    _placementRotation -= 1;
                }

                if (_shortcuts.FastRotateClockwise.Check(ctrl, shift, alt, true)) {
                    _placementRotation += 2;
                }

                if (_shortcuts.FastRotateCounterClockwise.Check(ctrl, shift, alt, true)) {
                    _placementRotation -= 2;
                }

                // Activate Selection Mode Via Mouse
                if (_shortcuts.SelectProps.Button != _shortcuts.PlaceProp.Button) {
                    if (!_isPropsListHovered && !_isPropsWinHovered && (_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                        _mode = 0;
                    }

                    if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker && !(_isPropsWinHovered || _isPropsWinDragged))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }
                        
                        _clickTracker = false;

                        List<int> selectedI = [];
                    
                        for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                        {
                            var current = GLOBALS.Level.Props[i];
                            var propSelectRect = Utils.EncloseQuads(current.prop.Quads);
                            if (_shortcuts.PropSelectionModifier.Check(ctrl, shift, alt, true))
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = !_selected[i];
                                }
                            }
                            else
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = true;
                                    selectedI.Add(i);
                                }
                                else
                                {
                                    _selected[i] = false;
                                }
                            }
                        }

                        _selectedCycleIndices = [..selectedI];
                        _selectedCycleCursor = -1;
                    }  
                }

                // Cycle categories
                if (_shortcuts.CycleCategoriesRight.Check(ctrl, shift, alt))
                {
                    _menuRootCategoryIndex++;
                    if (_menuRootCategoryIndex > 3) _menuRootCategoryIndex = 0;
                }
                else if (_shortcuts.CycleCategoriesLeft.Check(ctrl, shift, alt))
                {
                    _menuRootCategoryIndex--;
                    if (_menuRootCategoryIndex < 0) _menuRootCategoryIndex = 3;
                }
                
                // Navigate categories menu
                if (_shortcuts.ToNextInnerCategory.Check(ctrl, shift, alt))
                {
                    ToNextInnerCategory();
                }
                else if (_shortcuts.ToPreviousInnerCategory.Check(ctrl, shift, alt))
                {
                    ToPreviousInnerCategory();
                }
                
                if (_shortcuts.NavigateMenuDown.Check(ctrl, shift, alt))
                {
                    IncrementMenuIndex();
                }
                else if (_shortcuts.NavigateMenuUp.Check(ctrl, shift, alt))
                {
                    DecrementMenuIndex();
                }
                
                // Pickup Prop
                if (_shortcuts.PickupProp.Check(ctrl, shift, alt))
                {
                    for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                    {
                        var current = GLOBALS.Level.Props[i];
    
                        if (!CheckCollisionPointRec(tileMouseWorld, Utils.EncloseQuads(current.prop.Quads)) || 
                            current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || 
                            current.prop.Depth > GLOBALS.Layer * -10) 
                            continue;

                        if (current.type == InitPropType.Tile && GLOBALS.TileDex is not null)
                        {
                            for (var c = 0; c < GLOBALS.TileDex.OrderedTilesAsProps.Length; c++)
                            {
                                for (var p = 0; p < GLOBALS.TileDex.OrderedTilesAsProps[c].Length; p++)
                                {
                                    var currentTileAsProp = GLOBALS.TileDex.OrderedTilesAsProps[c][p];

                                    if (currentTileAsProp.Name != current.prop.Name) continue;

                                    _propsMenuTilesCategoryIndex = c;
                                    _propsMenuTilesIndex = p;

                                    _copiedPropSettings = current.prop.Extras.Settings;
                                    _copiedIsTileAsProp = true;
                                }
                            }
                        }
                        else if (current.type == InitPropType.Rope)
                        {
                            _copiedRopePoints = [..current.prop.Extras.RopePoints];
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else if (current.type == InitPropType.Long)
                        {
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else
                        {
                            (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex) = current.position;
                            _copiedPropSettings = current.prop.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }

                        _copiedDepth = current.prop.Depth;
                    }
                }
                break;
            
            case 0: // Select Mode
                if (!canDrawTile) break;
                var anySelected = _selected.Any(s => s);
                var fetchedSelected = GLOBALS.Level.Props
                    .Select((prop, index) => (prop, index))
                    .Where(p => _selected[p.index])
                    .Select(p => p)
                    .ToArray();
                
                if (anySelected)
                {
                    _selectedPropsEncloser = Utils.EncloseProps(fetchedSelected.Select(p => p.prop.prop.Quads));
                    _selectedPropsCenter = new Vector2(
                        _selectedPropsEncloser.X + 
                        _selectedPropsEncloser.Width/2, 
                        _selectedPropsEncloser.Y + 
                        _selectedPropsEncloser.Height/2
                    );

                    if (fetchedSelected[0].prop.type == InitPropType.Rope) {
                        if (_shortcuts.SimulationBeizerSwitch.Check(ctrl, shift, alt)) {
                            
                            // Find the rope model
                            var modelIndex = -1;

                            for (var i = 0; i < _models.Length; i++)
                            {
                                if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
                            }

                            if (modelIndex != -1) {
                                ref var currentModel = ref _models[modelIndex];
                                currentModel.simSwitch = !currentModel.simSwitch;
                            }
                        }

                        if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt)) {
                            fetchedSelected[0].prop.prop.Extras.RopePoints = [..fetchedSelected[0].prop.prop.Extras.RopePoints, new Vector2(0, 0)];

                            ImportRopeModels();
                        }

                        if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt)) {
                            fetchedSelected[0].prop.prop.Extras.RopePoints = fetchedSelected[0].prop.prop.Extras.RopePoints.Take(fetchedSelected[0].prop.prop.Extras.RopePoints.Length - 1).ToArray();

                            ImportRopeModels();
                        }
                    }
                }
                else
                {
                    _selectedPropsEncloser.Width = 0;
                    _selectedPropsEncloser.Height = 0;

                    _selectedPropsCenter.X = 0;
                    _selectedPropsCenter.Y = 0;
                }

                #region RotatePropsKeyboard
                // Rotate Selected (Keyboard)
                if (_shortcuts.RotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = 0.3f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (_shortcuts.RotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = -0.3f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (_shortcuts.FastRotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = 2f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (_shortcuts.FastRotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = -2f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                #endregion

                #region ActivateModes
                // Cycle selected
                if (_shortcuts.CycleSelected.Check(ctrl, shift, alt) && anySelected && _selectedCycleIndices.Length > 0)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    _selectedCycleCursor++;
                    Utils.Cycle(ref _selectedCycleCursor, 0, _selectedCycleIndices.Length - 1);

                    _selected = new bool[GLOBALS.Level.Props.Length];
                    _selected[_selectedCycleIndices[_selectedCycleCursor]] = true;
                    _shouldUpdateModeIndicatorsRT = true;
                }
                // Move
                else if (_shortcuts.ToggleMovingPropsMode.Check(ctrl, shift, alt) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = !_movingProps;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;

                    _propsMoveMousePos = _propsMoveMouseAnchor = GetScreenToWorld2D(GetMousePosition(), _camera);
                    _shouldUpdateModeIndicatorsRT = true;

                    if (!_movingProps) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                }
                // Rotate
                else if (_shortcuts.ToggleRotatingPropsMode.Check(ctrl, shift, alt) && anySelected)
                {
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = !_rotatingProps;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                    _shouldUpdateModeIndicatorsRT = true;

                    if (!_rotatingProps) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                }
                // Scale
                else if (_shortcuts.ToggleScalingPropsMode.Check(ctrl, shift, alt) && anySelected)
                {
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    _scalingProps = !_scalingProps;
                    _stretchAxes = 0;
                    // _ropeMode = false;
                    
                    SetMouseCursor(MouseCursor.ResizeNesw);
                    _shouldUpdateModeIndicatorsRT = true;

                    if (_scalingProps) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                }
                // Hide
                else if (_shortcuts.TogglePropsVisibility.Check(ctrl, shift, alt) && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                    {
                        if (_selected[i]) _hidden[i] = !_hidden[i];
                    }
                    _shouldUpdateModeIndicatorsRT = true;
                }
                // Edit Quads
                else if (_shortcuts.ToggleEditingPropQuadsMode.Check(ctrl, shift, alt) && fetchedSelected.Length == 1)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = !_stretchingProp;
                    _editingPropPoints = false;
                    // _ropeMode = false;
                    _shouldUpdateModeIndicatorsRT = true;

                    if (!_stretchingProp) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                }
                // Delete
                else if (_shortcuts.DeleteSelectedProps.Check(ctrl, shift, alt) && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }
   
                    _scalingProps = false;
                    _movingProps = false;
                    _rotatingProps = false;
                    _stretchingProp = false;
                    _editingPropPoints = false;
                    _ropeMode = false;
                    
                    GLOBALS.Level.Props = _selected
                        .Select((s, i) => (s, i))
                        .Where(v => !v.Item1)
                        .Select(v => GLOBALS.Level.Props[v.Item2])
                        .ToArray();

                    if (_selected.Length != GLOBALS.Level.Props.Length) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }
                    
                    _selected = new bool[GLOBALS.Level.Props.Length]; // Update selected
                    _hidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

                    
                    fetchedSelected = GLOBALS.Level.Props
                        .Select((prop, index) => (prop, index))
                        .Where(p => _selected[p.index])
                        .Select(p => p)
                        .ToArray();
                    
                    ImportRopeModels(); // don't forget to update the list when props list is modified
                    _shouldUpdateModeIndicatorsRT = true;
                }
                // Rope-only actions
                else if (
                    fetchedSelected.Length == 1 &&
                    fetchedSelected[0].prop.type == InitPropType.Rope
                )
                {
                    // Edit Rope Points
                    if (_shortcuts.ToggleRopePointsEditingMode.Check(ctrl, shift, alt))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }

                        
                        _scalingProps = false;
                        _movingProps = false;
                        _rotatingProps = false;
                        _stretchingProp = false;
                        _editingPropPoints = !_editingPropPoints;
                        _ropeMode = false;
                        _shouldUpdateModeIndicatorsRT = true;

                        if (!_editingPropPoints) {
                            _gram.Proceed(GLOBALS.Level.Props);
                        }
                    }
                    // Rope mode
                    else if (_shortcuts.ToggleRopeEditingMode.Check(ctrl, shift, alt))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }

                        // _scalingProps = false;
                        // _movingProps = false;
                        // _rotatingProps = false;
                        // _stretchingProp = false;
                        // _editingPropPoints = false;
                        _ropeMode = !_ropeMode;
                        _shouldUpdateModeIndicatorsRT = true;

                        if (!_ropeMode) {
                            _gram.Proceed(GLOBALS.Level.Props);
                        }
                    }
                    else if (_shortcuts.DuplicateProps.Check(ctrl, shift, alt)) {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }

                        List<(InitPropType, TileDefinition?, (int, int), Prop)> dProps = [];
                    
                        foreach (var (prop, _) in fetchedSelected)
                        {
                            dProps.Add((prop.type, prop.tile, prop.position, new Prop(
                                    prop.prop.Depth,
                                    prop.prop.Name,
                                    prop.prop.IsTile,
                                    new PropQuad(
                                        prop.prop.Quads.TopLeft,
                                        prop.prop.Quads.TopRight,
                                        prop.prop.Quads.BottomRight,
                                        prop.prop.Quads.BottomLeft
                                    ))
                                {
                                    Extras = new PropExtras(
                                        prop.prop.Extras.Settings.Clone(),
                                        [..prop.prop.Extras.RopePoints])
                                })
                            );

                        }
                        
                        GLOBALS.Level.Props = [..GLOBALS.Level.Props, ..dProps];

                        var newSelected = new bool[GLOBALS.Level.Props.Length]; // Update selected
                        var newHidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

                        if (newSelected.Length != _selected.Length) {
                            _gram.Proceed(GLOBALS.Level.Props);
                        }

                        for (var i = 0; i < _selected.Length; i++)
                        {
                            newSelected[i] = _selected[i];
                            newHidden[i] = _hidden[i];
                        }

                        _selected = newSelected;
                        _hidden = newHidden;
                    
                        fetchedSelected = GLOBALS.Level.Props
                            .Select((prop, index) => (prop, index))
                            .Where(p => _selected[p.index])
                            .Select(p => p)
                            .ToArray();
                    }
                }
                // Duplicate
                else if (_shortcuts.DuplicateProps.Check(ctrl, shift, alt) && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    List<(InitPropType, TileDefinition?, (int, int), Prop)> dProps = [];
                    
                    foreach (var (prop, _) in fetchedSelected)
                    {
                        dProps.Add((prop.type, prop.tile, prop.position, new Prop(
                                prop.prop.Depth,
                                prop.prop.Name,
                                prop.prop.IsTile,
                                new PropQuad(
                                    prop.prop.Quads.TopLeft,
                                    prop.prop.Quads.TopRight,
                                    prop.prop.Quads.BottomRight,
                                    prop.prop.Quads.BottomLeft
                                ))
                            {
                                Extras = new PropExtras(
                                    prop.prop.Extras.Settings.Clone(),
                                    [..prop.prop.Extras.RopePoints])
                            })
                        );

                    }
                    
                    GLOBALS.Level.Props = [..GLOBALS.Level.Props, ..dProps];

                    var newSelected = new bool[GLOBALS.Level.Props.Length]; // Update selected
                    var newHidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

                    if (newSelected.Length != _selected.Length) {
                        _gram.Proceed(GLOBALS.Level.Props);
                    }

                    for (var i = 0; i < _selected.Length; i++)
                    {
                        newSelected[i] = _selected[i];
                        newHidden[i] = _hidden[i];
                    }

                    _selected = newSelected;
                    _hidden = newHidden;
                
                    fetchedSelected = GLOBALS.Level.Props
                        .Select((prop, index) => (prop, index))
                        .Where(p => _selected[p.index])
                        .Select(p => p)
                        .ToArray();
                }
                else SetMouseCursor(MouseCursor.Default);
                #endregion

                if (_ropeMode && fetchedSelected.Length == 1)
                {
                    _shouldUpdateModeIndicatorsRT = true;

                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    var foundRopeList = _models.Where(rope => rope.index == fetchedSelected[0].index);

                    if (foundRopeList.Any()) {
                        var (_, simSwitch, model, bezierHandles) = foundRopeList.First();


                        if (simSwitch) // simulate
                        {
                            if (++_ropeSimulationFrame % _ropeSimulationFrameCut == 0)
                            {
                                model.Update(
                                fetchedSelected[0].prop.prop.Quads, 
                                fetchedSelected[0].prop.prop.Depth switch
                                {
                                    < -19 => 2,
                                    < -9 => 1,
                                    _ => 0
                                });
                            }
                        }
                        else // bezier
                        {
                            var ends = Utils.RopeEnds(fetchedSelected[0].prop.prop.Quads);
                            
                            fetchedSelected[0].prop.prop.Extras.RopePoints = Utils.Casteljau(fetchedSelected[0].prop.prop.Extras.RopePoints.Length, [ ends.pA, ..bezierHandles, ends.pB ]);

                            if ((IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) && _bezierHandleLock != -1)
                                _bezierHandleLock = -1;

                            if (IsMouseButtonDown(_shortcuts.SelectProps.Button))
                            {
                                for (var b = 0; b < bezierHandles.Length; b++)
                                {
                                    if (_bezierHandleLock == -1 && CheckCollisionPointCircle(tileMouseWorld, bezierHandles[b], 3f))
                                        _bezierHandleLock = b;

                                    if (_bezierHandleLock == b) bezierHandles[b] = tileMouseWorld;
                                }
                            }

                            
                        }
                    }

                }
                
                // TODO: switch on enums instead
                if (_movingProps && anySelected && _propsMoveMouseAnchor is not null && _propsMoveMousePos is not null)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }
                    
                    // update mouse delta
                    
                    var newMousePos = GetScreenToWorld2D(GetMousePosition(), _camera);
                    
                    _propsMoveMouseDelta = newMousePos - _propsMoveMousePos.Value;

                    Vector2 gridDelta, preciseDelta;

                    // Grid delta
                    {
                        var gridScaled = new Vector2((int)(_propsMoveMousePos.Value.X / 16), (int)(_propsMoveMousePos.Value.Y / 16));
                        var gridScaledBack = gridScaled * 16;
                        
                        var newGridScaled = new Vector2((int)(newMousePos.X / 16), (int)(newMousePos.Y / 16));
                        var newGridScaledBack = newGridScaled * 16;

                        gridDelta = newGridScaledBack - gridScaledBack;
                    }
                    
                    // Precise delta
                    {
                        var gridScaled = new Vector2((int)(_propsMoveMousePos.Value.X / 8), (int)(_propsMoveMousePos.Value.Y / 8));
                        var gridScaledBack = gridScaled * 8;
                        
                        var newGridScaled = new Vector2((int)(newMousePos.X / 8), (int)(newMousePos.Y / 8));
                        var newGridScaledBack = newGridScaled * 8;

                        preciseDelta = newGridScaledBack - gridScaledBack;
                    }

                    _propsMoveMousePos = newMousePos;
                    
                    // Fix delta when level panning
                    
                    if (IsMouseButtonDown(_shortcuts.DragLevel.Button))
                        _propsMoveMouseDelta = new Vector2(0, 0);

                    for (var s = 0; s < _selected.Length; s++)
                    {
                        if (!_selected[s]) continue;
                        
                        var quads = GLOBALS.Level.Props[s].prop.Quads;
                        var center = Utils.QuadsCenter(ref quads);

                        switch (_snapMode)
                        {
                            case 0: // Free
                            {
                                quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, _propsMoveMouseDelta);
                                quads.TopRight = Raymath.Vector2Add(quads.TopRight, _propsMoveMouseDelta);
                                quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, _propsMoveMouseDelta);
                                quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, _propsMoveMouseDelta);
                            }
                                break;

                            case 1: // Grid
                            {
                                quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, gridDelta);
                                quads.TopRight = Raymath.Vector2Add(quads.TopRight, gridDelta);
                                quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, gridDelta);
                                quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, gridDelta);
                            }
                                break;

                            case 2: // Precise
                            {
                                var gridScaled = new Vector2((int)(_propsMoveMousePos.Value.X / 8), (int)(_propsMoveMousePos.Value.Y / 8));
                                var gridScaledBack = gridScaled * 8;

                                var centerDelta = gridScaledBack - center;
                                
                                quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, preciseDelta);
                                quads.TopRight = Raymath.Vector2Add(quads.TopRight, preciseDelta);
                                quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, preciseDelta);
                                quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, preciseDelta);
                            }
                                break;
                        }

                        GLOBALS.Level.Props[s].prop.Quads = quads;

                        if (GLOBALS.Level.Props[s].type == InitPropType.Rope)
                        {
                            if (!_ropeMode)
                            {
                                for (var p = 0; p < GLOBALS.Level.Props[s].prop.Extras.RopePoints.Length; p++)
                                {
                                    GLOBALS.Level.Props[s].prop.Extras.RopePoints[p] = Raymath.Vector2Add(GLOBALS.Level.Props[s].prop.Extras.RopePoints[p], _propsMoveMouseDelta);
                                }
                            }

                            for (var r = 0; r < _models.Length; r++)
                            {
                                if (_models[r].index == s)
                                {
                                    for (var h = 0; h < _models[r].bezierHandles.Length; h++)
                                    {
                                        _models[r].bezierHandles[h] += _propsMoveMouseDelta;
                                    }
                                }
                            }
                        }
                    }

                    if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) {
                        _gram.Proceed(GLOBALS.Level.Props);

                        _movingProps = false;
                    }
                }
                else if (_rotatingProps && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) {
                        _gram.Proceed(GLOBALS.Level.Props);
                        _rotatingProps = false;
                    }

                    var delta = GetMouseDelta();
                    
                    // Collective Rotation
                    
                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].prop.Quads;

                        GLOBALS.Level.Props[p].prop.Quads = Utils.RotatePropQuads(quads, delta.X, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].type == InitPropType.Rope)
                        {
                            Utils.RotatePoints(delta.X, _selectedPropsCenter, GLOBALS.Level.Props[p].prop.Extras.RopePoints);
                        }
                    }
                }
                else if (_scalingProps && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key))
                    {
                        _stretchAxes = 0;
                        _scalingProps = false;
                        SetMouseCursor(MouseCursor.Default);
                    }

                    if (IsKeyPressed(KeyboardKey.X))
                    {
                        _stretchAxes = (byte)(_stretchAxes == 1 ? 0 : 1);
                        SetMouseCursor(MouseCursor.ResizeNesw);
                    }
                    if (IsKeyPressed(KeyboardKey.Y))
                    {
                        _stretchAxes =  (byte)(_stretchAxes == 2 ? 0 : 2);
                        SetMouseCursor(MouseCursor.ResizeNs);
                    }

                    var delta = GetMouseDelta();

                    switch (_stretchAxes)
                    {
                        case 0: // Uniform Scaling
                        {
                            var enclose = Utils.EncloseProps(fetchedSelected.Select(s => s.prop.prop.Quads));
                            var center = Utils.RectangleCenter(ref enclose);

                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                Utils.ScaleQuads(ref quads, center, 1f + delta.X*0.01f);
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;

                        case 1: // X-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                var center = Utils.QuadsCenter(ref quads);
                                
                                Utils.ScaleQuadsX(ref quads, center, 1f + delta.X * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;

                        case 2: // Y-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.prop.Quads;
                                var center = Utils.QuadsCenter(ref quads);

                                Utils.ScaleQuadsY(ref quads, center, 1f - delta.Y * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].prop.Quads = quads;
                            }
                        }
                            break;
                    }
                }
                else if (_stretchingProp && anySelected)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    var currentQuads = fetchedSelected[0].prop.prop.Quads; 
                    
                    var posV = _snapMode switch
                    {
                        1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                        2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                        _ => tileMouseWorld
                    };

                    if (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key))
                    {
                        if (fetchedSelected[0].prop.type == InitPropType.Rope)
                        {
                            var middleLeft = Raymath.Vector2Divide(
                                Raymath.Vector2Add(currentQuads.TopLeft, currentQuads.BottomLeft),
                                new(2f, 2f)
                            );

                            var middleRight = Raymath.Vector2Divide(
                                Raymath.Vector2Add(currentQuads.TopRight, currentQuads.BottomRight),
                                new(2f, 2f)
                            );
                            
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, middleLeft));
                            
                            if (
                                CheckCollisionPointCircle(
                                    posV, middleLeft,
                                    5f
                                ) || _quadLock == 1)
                            {
                                _quadLock = 1;
                                currentQuads.BottomLeft = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.TopLeft = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                currentQuads.BottomRight = Raymath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = Raymath.Vector2Add(
                                    middleRight, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }

                            if (
                                CheckCollisionPointCircle(
                                    tileMouseWorld, middleRight,
                                    5f
                                    ) || _quadLock == 2)
                            {
                                _quadLock = 2;
                                
                                currentQuads.BottomLeft = Raymath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopLeft = Raymath.Vector2Add(
                                    middleLeft, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.BottomRight = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                );
                                
                                currentQuads.TopRight = Raymath.Vector2Add(
                                    posV, 
                                    new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                );
                            }
                        }
                        else if (fetchedSelected[0].prop.type == InitPropType.Long)
                        {
                            var (left, top, right, bottom) = Utils.LongSides(fetchedSelected[0].prop.prop.Quads);
                            
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, left));

                            if (CheckCollisionPointCircle(tileMouseWorld, left, 5f) && _quadLock == 0)
                            {
                                _quadLock = 1;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, right, 5f) && _quadLock == 0)
                            {
                                _quadLock = 2;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, top, 5f) && _quadLock == 0)
                            {
                                _quadLock = 3;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, bottom, 5f) && _quadLock == 0)
                            {
                                _quadLock = 4;
                            }

                            switch (_quadLock)
                            {
                                case 1: // left
                                    currentQuads.BottomLeft = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = Raymath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = Raymath.Vector2Add(
                                        right, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                    break;
                                
                                case 2: // right
                                    currentQuads.BottomLeft = Raymath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopLeft = Raymath.Vector2Add(
                                        left, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.BottomRight = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
                                    );
                                
                                    currentQuads.TopRight = Raymath.Vector2Add(
                                        posV, 
                                        new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
                                    );
                                    break;
                                
                                case 3: // TODO: top
                                    break;
                                
                                case 4: // TODO: bottom
                                    break;
                            }
                        }
                        else
                        {
                            if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopLeft, 5f) && _quadLock == 0)
                            {
                                _quadLock = 1;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopRight, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 2;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomRight, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 3;
                            }
                            else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomLeft, 5f) &&
                                     _quadLock == 0)
                            {
                                _quadLock = 4;
                            }
                            
                            // Check Top-Left
                            if (_quadLock == 1)
                            {
                                currentQuads.TopLeft = posV;
                            }
                            // Check Top-Right
                            else if (_quadLock == 2)
                            {
                                currentQuads.TopRight = posV;
                            }
                            // Check Bottom-Right 
                            else if (_quadLock == 3)
                            {
                                currentQuads.BottomRight = posV;
                            }
                            // Check Bottom-Left
                            else if (_quadLock == 4)
                            {
                                currentQuads.BottomLeft = posV;
                            }
                        }
                        
                        GLOBALS.Level.Props[fetchedSelected[0].index].prop.Quads = currentQuads;
                    }
                    else if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _quadLock != 0) _quadLock = 0;
                }
                else if (_editingPropPoints && fetchedSelected.Length == 1)
                {
                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    
                    var points = fetchedSelected[0].prop.prop.Extras.RopePoints;

                    if ((IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) && _pointLock != -1) _pointLock = -1;
                    
                    if (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key))
                    {
                        // Check Collision of Each Point

                        for (var p = 0; p < points.Length; p++)
                        {
                            if (CheckCollisionPointCircle(
                                    tileMouseWorld, 
                                    new Vector2(
                                        points[p].X, 
                                        points[p].Y
                                        ), 
                                    3f) || 
                                _pointLock == p
                            )
                            {
                                _pointLock = p;
                                points[p] = tileMouseWorld;
                            }
                        }
                    }
                }
                else
                {
                    if (anySelected)
                    {
                        if (_shortcuts.DeepenSelectedProps.Check(ctrl, shift, alt))
                        {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else {
                                _shouldRedrawLevel = true;
                            }

                            
                            foreach (var selected in fetchedSelected)
                            {
                                selected.prop.prop.Depth--;

                                if (selected.prop.prop.Depth < -29) selected.prop.prop.Depth = 29;
                            }
                        }
                        else if (_shortcuts.UndeepenSelectedProps.Check(ctrl, shift, alt))
                        {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else {
                                _shouldRedrawLevel = true;
                            }

                            
                            foreach (var selected in fetchedSelected)
                            {
                                selected.prop.prop.Depth++;

                                if (selected.prop.prop.Depth > 0) selected.prop.prop.Depth = 0;
                            }
                        }
                    
                        if (_shortcuts.CycleVariations.Check(ctrl, shift, alt)) {
                            foreach (var selected in fetchedSelected) {
                                if (selected.prop.prop.Extras.Settings is IVariable vs) {
                                    var (category, index) = selected.prop.position;

                                    var variations = (GLOBALS.Props[category][index] as IVariableInit).Variations;
                                
                                    vs.Variation = (vs.Variation += 1) % variations;
                                }
                            }

                            _shouldRedrawPropLayer = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                        }
                    }
                    
                    if (_bezierHandleLock == -1 && (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key)) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                    }

                    if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker && !(_isPropsWinHovered || _isPropsWinDragged))
                    {
                        // _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        else {
                            _shouldRedrawLevel = true;
                        }
                        
                        _clickTracker = false;

                        List<int> selectedI = [];
                    
                        var selectCount = 0;
                        var lastSelected = -1;

                        for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                        {
                            var current = GLOBALS.Level.Props[i];
                            var propSelectRect = Utils.EncloseQuads(current.prop.Quads);
                            
                            if (IsKeyDown(_shortcuts.PropSelectionModifier.Key))
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = !_selected[i];
                                }
                            }
                            else
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.prop.Depth <= (GLOBALS.Layer + 1) * -10 || current.prop.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = true;
                                    selectedI.Add(i);
                                }
                                else
                                {
                                    _selected[i] = false;
                                }
                            }

                            if (_selected[i]) {
                                selectCount++;
                                lastSelected = i;
                            }
                        }

                        // if (selectCount == 1 && GLOBALS.Level.Props[lastSelected].type == InitPropType.Rope) {
                        //     _ropeMode = true;
                        // } else {
                        //     _ropeMode = false;
                        // }

                        if (selectCount == 0) {
                            _shouldUpdateModeIndicatorsRT = true;
                        }

                        _selectedCycleIndices = [..selectedI];
                        _selectedCycleCursor = -1;
                    }   
                    else if (!_isPropsListHovered && !_isPropsWinHovered && canDrawTile && _shortcuts.PlaceProp.Button != _shortcuts.SelectProps.Button && (_shortcuts.PlaceProp.Check(ctrl, shift, alt, true) ||_shortcuts.PlacePropAlt.Check(ctrl, shift, alt, true))) {
                        _mode = 1;
                        if (_noCollisionPropPlacement) _lockedPlacement = true;
                    }
                }
                
                break;
        }

        #region TileEditorDrawing
        BeginDrawing();

        if (_shouldRedrawLevel)
        {
            RedrawLevel();
            _shouldRedrawLevel = false;
        }

        if (_shouldRedrawPropLayer) {
            DrawPropLayerRT();

            _shouldRedrawPropLayer = false;
        }

        if (_shouldUpdateModeIndicatorsRT) {
            UpdateModeIndicators();
            _shouldUpdateModeIndicatorsRT = false;
        }

        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? Color.Black 
            : new Color(170, 170, 170, 255));

        BeginMode2D(_camera);
        {
            DrawRectangleLinesEx(new Rectangle(-3, -3, GLOBALS.Level.Width * 16 + 6, GLOBALS.Level.Height * 16 + 6), 3, Color.White);
            
            BeginShaderMode(GLOBALS.Shaders.VFlip);
            SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
            DrawTexturePro(GLOBALS.Textures.GeneralLevel.Texture, 
                new Rectangle(0, 0, GLOBALS.Level.Width * 16, GLOBALS.Level.Height * 16), 
                new Rectangle(0, 0, GLOBALS.Level.Width * 16, GLOBALS.Level.Height * 16), 
                new Vector2(0, 0), 
                0, 
                Color.White);
            
            
            EndShaderMode();

            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
                BeginShaderMode(GLOBALS.Shaders.VFlip);
                SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), _propLayerRT.Raw.Texture);
                DrawTexture(_propLayerRT.Raw.Texture, 0, 0, Color.White);
                EndShaderMode();
            }

            
            // Grid

            if (_showGrid)
            {
                Printers.DrawGrid(16);
            }
            
            if (GLOBALS.Settings.GeneralSettings.DarkTheme)
            {
                DrawRectangleLines(0, 0, GLOBALS.Level.Width*16, GLOBALS.Level.Height*16, Color.White);
            }
            
            // Draw the enclosing rectangle for selected props
            // DEBUG: DrawRectangleLinesEx(_selectedPropsEncloser, 3f, Color.White);

            switch (_mode)
            {
                case 1: // Place Mode
                    switch (_menuRootCategoryIndex)
                    {
                        case 0: // Current Tile-As-Prop
                            {
                                switch (_snapMode)
                                {
                                    case 0: // free
                                        if (_currentTile is not null) Printers.DrawTileAsProp(
                                            _currentTile,
                                            tileMouseWorld,
                                            _placementRotation * _placementRotationSteps
                                        );
                                        break;

                                    case 1: // grid snap
                                    {
                                        var posV = new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale;
                                        
                                        Printers.DrawTileAsProp(
                                            _currentTile,
                                            posV,
                                            _placementRotation * _placementRotationSteps
                                        );
                                    }
                                        break;
                                    
                                    case 2: // precise grid snap
                                    {
                                        var posV = new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f;
                                        
                                        Printers.DrawTileAsProp(
                                            _currentTile,
                                            posV,
                                            _placementRotation * _placementRotationSteps
                                        );
                                    }
                                        break;
                                }
                            }
                            break;
                        
                        case 1: // Current Rope
                            DrawCircleV(tileMouseWorld, 3f, Color.Blue);
                            break;

                        case 2: // Current Long Prop
                        {
                            var prop = GLOBALS.LongProps[_propsMenuLongsIndex];
                            var texture = GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                            var height = texture.Height / 2f;

                            var posV = _snapMode switch
                            {
                                1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                                2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                                _ => tileMouseWorld,
                            };
                            
                            Printers.DrawProp(
                                new PropLongSettings(), 
                                prop, 
                                texture, 
                                new PropQuad
                                {
                                    TopLeft = new(posV.X - 100, posV.Y - height),
                                    BottomLeft = new(posV.X - 100, posV.Y + height),
                                    TopRight = new(posV.X + 100, posV.Y - height),
                                    BottomRight = new(posV.X + 100, posV.Y + height)
                                }, 
                                0
                            );
                        }
                            break;

                        case 3: // Current Prop
                        {
                            // Since I've already seperated regular props from everything else, this can be
                            // consideColor.Red outdated
                            var prop = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            
                            var (width, height, settings) = prop switch
                            {
                                InitVariedStandardProp variedStandard => (variedStandard.Size.x * GLOBALS.PreviewScale / 2f, variedStandard.Size.y * GLOBALS.PreviewScale / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                InitStandardProp standard => (standard.Size.x * GLOBALS.PreviewScale / 2f, standard.Size.y * GLOBALS.PreviewScale / 2f, new BasicPropSettings()),
                                InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
                                InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
                                InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
                                InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
                                InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
                                InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
                                InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
                                InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                        
                                _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
                            };
                            
                            var posV = _snapMode switch
                            {
                                1 => new Vector2(tileMatrixX, tileMatrixY) * GLOBALS.PreviewScale,
                                2 => new Vector2((int)(tileMouseWorld.X / 8f), (int)(tileMouseWorld.Y / 8f)) * 8f,
                                _ => tileMouseWorld,
                            };
                            
                            Printers.DrawProp(settings, prop, texture, new PropQuad(
                                new Vector2(posV.X - width, posV.Y - height), 
                                new Vector2(posV.X + width, posV.Y - height), 
                                new Vector2(posV.X + width, posV.Y + height), 
                                new Vector2(posV.X - width, posV.Y + height)),
                                0,
                                _placementRotation * _placementRotationSteps
                            );
                        }
                            break;
                    }
                    break;
                
                case 0: // Select Mode
                    
                    // // TODO: tweak selection cancellation
                    // if ((_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && _clickTracker)
                    // {
                    //     var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
                    //     var diff = Raymath.Vector2Subtract(mouse, _selection1);
                    //     var position = (diff.X > 0, diff.Y > 0) switch
                    //     {
                    //         (true, true) => _selection1,
                    //         (true, false) => new Vector2(_selection1.X, mouse.Y),
                    //         (false, true) => new Vector2(mouse.X, _selection1.Y),
                    //         (false, false) => mouse
                    //     };

                    //     _selection = new Rectangle(
                    //         position.X, 
                    //         position.Y, 
                    //         Math.Abs(diff.X), 
                    //         Math.Abs(diff.Y)
                    //     );
                        
                    //     DrawRectangleRec(_selection, new Color(0, 0, 255, 90));
                        
                    //     DrawRectangleLinesEx(
                    //         _selection,
                    //         2f,
                    //         Color.Blue
                    //     );
                    // }
                    break;
            }

            // TODO: tweak selection cancellation
            if ((IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key)) && _clickTracker)
            {
                var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
                var diff = Raymath.Vector2Subtract(mouse, _selection1);
                var position = (diff.X > 0, diff.Y > 0) switch
                {
                    (true, true) => _selection1,
                    (true, false) => new Vector2(_selection1.X, mouse.Y),
                    (false, true) => new Vector2(mouse.X, _selection1.Y),
                    (false, false) => mouse
                };

                _selection = new Rectangle(
                    position.X, 
                    position.Y, 
                    Math.Abs(diff.X), 
                    Math.Abs(diff.Y)
                );
                
                DrawRectangleRec(_selection, new Color(0, 0, 255, 90));
                
                DrawRectangleLinesEx(
                    _selection,
                    2f,
                    Color.Blue
                );
            }

        }
        EndMode2D();

        #region TileEditorUI

        {
            // Selected Props
            var fetchedSelected = GLOBALS.Level.Props
                .Select((prop, index) => (prop, index))
                .Where(p => _selected[p.index])
                .Select(p => p)
                .ToArray();

            // Coordinates

            if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
            {
                if (inMatrixBounds)
                    DrawText(
                        $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
                        (int)tileMouse.X + previewScale,
                        (int)tileMouse.Y + previewScale,
                        15,
                        Color.White
                    );
            }
            else
            {
                if (inMatrixBounds)
                    DrawText(
                        $"x: {tileMatrixX}, y: {tileMatrixY}",
                        (int)tileMouse.X + previewScale,
                        (int)tileMouse.Y + previewScale,
                        15,
                        Color.White
                    );
            }

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

            if (GLOBALS.Layer == 2) DrawText("3", (int)layer3Rect.X + 15, (int)layer3Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);

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

            if (newLayer != GLOBALS.Layer)
            {
                GLOBALS.Layer = newLayer;
                UpdateDefaultDepth();
                _shouldRedrawLevel = true;
                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
            }

            // Update prop depth render texture
            if (fetchedSelected.Length == 1 || 
                (fetchedSelected.Length > 1 && 
                                                Utils.AllEqual(fetchedSelected.Select(f => f.prop.prop.Depth),
                    fetchedSelected[0].prop.prop.Depth)))
            {
                BeginTextureMode(GLOBALS.Textures.PropDepth);
                ClearBackground(Color.Green);
                Printers.DrawDepthIndicator(fetchedSelected[0].prop);
                EndTextureMode();
            }

            // // Edit Mode Indicators
            // if (_mode == 0) {
            //      var moveTexture = GLOBALS.Textures.PropEditModes[0];
            //      var rotateTexture = GLOBALS.Textures.PropEditModes[1];
            //      var scaleTexture = GLOBALS.Textures.PropEditModes[2];
            //      var warpTexture = GLOBALS.Textures.PropEditModes[3];
            //      var editPointsTexture = GLOBALS.Textures.PropEditModes[4];

            //      var moveRect = new Rectangle(135, sHeight - 50, 40, 40);
            //      var rotateRect = new Rectangle(180, sHeight - 50, 40, 40);
            //      var scaleRect = new Rectangle(225, sHeight - 50, 40, 40);
            //      var warpRect = new Rectangle(270, sHeight - 50, 40, 40);
            //      var editPointsRect = new Rectangle(315, sHeight - 50, 40, 40);

            //      var rectColor = GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White;

            //      DrawRectangleRec(moveRect, rectColor);
            //      DrawRectangleRec(rotateRect, rectColor);
            //      DrawRectangleRec(scaleRect, rectColor);
            //      DrawRectangleRec(warpRect, rectColor);
            //      DrawRectangleRec(editPointsRect, rectColor);
                 
            //      if (_movingProps) DrawRectangleRec(moveRect, Color.Blue);
            //      if (_rotatingProps) DrawRectangleRec(rotateRect, Color.Blue);
            //      if (_scalingProps) DrawRectangleRec(scaleRect, Color.Blue);
            //      if (_stretchingProp) DrawRectangleRec(warpRect, Color.Blue);
            //      if (_editingPropPoints) DrawRectangleRec(editPointsRect, Color.Blue);

            //      DrawTexturePro(
            //          moveTexture,
            //          new Rectangle(0, 0, moveTexture.Width, moveTexture.Height),
            //          moveRect,
            //          new Vector2(0, 0),
            //          0,
            //          GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _movingProps ? Color.White : Color.Black);

            //      DrawTexturePro(
            //          rotateTexture,
            //          new Rectangle(0, 0, rotateTexture.Width, rotateTexture.Height),
            //          rotateRect,
            //          new Vector2(0, 0),
            //          0,
            //          GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _rotatingProps ? Color.White : Color.Black);

            //      DrawTexturePro(
            //          scaleTexture,
            //          new Rectangle(0, 0, scaleTexture.Width, scaleTexture.Height),
            //          scaleRect,
            //          new Vector2(0, 0),
            //          0,
            //          GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _scalingProps ? Color.White : Color.Black);

            //      DrawTexturePro(
            //          warpTexture,
            //          new Rectangle(0, 0, warpTexture.Width, warpTexture.Height),
            //          warpRect,
            //          new Vector2(0, 0),
            //          0,
            //          GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _stretchingProp ? Color.White : Color.Black);

            //      DrawTexturePro(
            //          editPointsTexture,
            //          new Rectangle(0, 0, editPointsTexture.Width, editPointsTexture.Height),
            //          editPointsRect,
            //          new Vector2(0, 0),
            //          0,
            //          GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : _editingPropPoints ? Color.White : Color.Black);
            // }
            //

            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
            // Navigation bar
                
            if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);

            var menuOpened = ImGui.Begin("Props Menu##PropsPlacementPanel");
            
            var menuPos = ImGui.GetWindowPos();
            var menuWinSpace = ImGui.GetWindowSize();

            if (CheckCollisionPointRec(tileMouse, new(menuPos.X - 5, menuPos.Y, menuWinSpace.X + 10, menuWinSpace.Y)))
            {
                _isPropsWinHovered = true;

                if (IsMouseButtonDown(MouseButton.Left)) _isPropsWinDragged = true;
            }
            else
            {
                _isPropsWinHovered = false;
            }

            if (IsMouseButtonReleased(MouseButton.Left) && _isPropsWinDragged) _isPropsWinDragged = false;
            
            if (menuOpened)
            {
                var availableSpace = ImGui.GetContentRegionAvail();

                var halfWidth = availableSpace.X / 2f;
                var halfSize = new Vector2(halfWidth, 20);
                
                ImGui.Spacing();

                if (ImGui.Button($"Grid: {_showGrid}",
                        availableSpace with { X = availableSpace.X / 2, Y = 20 }))
                {
                    _showGrid = !_showGrid;
                    _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                }
                        
                ImGui.SameLine();
                
                var precisionSelected = ImGui.Button(
                    $"Precision: {_snapMode switch { 0 => "Free", 1 => "Grid", 2 => "Precise", _ => "?" }}",
                    availableSpace with { X = availableSpace.X / 2, Y = 20 });

                if (precisionSelected) _snapMode = ++_snapMode % 3;
                
                ImGui.Spacing();
                
                {
                    if (ImGui.Button($"{(_noCollisionPropPlacement? "Continuous Placement" : "Single Placement")}", availableSpace with { Y = 20 }))
                        _noCollisionPropPlacement = !_noCollisionPropPlacement;
                    
                    ImGui.Spacing();
                    
                    ImGui.SeparatorText("Categories");

                    var quarterSpace = availableSpace with { X = availableSpace.X / 4f, Y = 20 };

                    var tilesSelected = ImGui.Selectable("Tiles", _menuRootCategoryIndex == 0, ImGuiSelectableFlags.None, quarterSpace);
                    ImGui.SameLine();
                    var ropesSelected = ImGui.Selectable("Ropes", _menuRootCategoryIndex == 1, ImGuiSelectableFlags.None, quarterSpace);
                    ImGui.SameLine();
                    var longsSelected = ImGui.Selectable("Longs", _menuRootCategoryIndex == 2, ImGuiSelectableFlags.None, quarterSpace);
                    ImGui.SameLine();
                    var othersSelected = ImGui.Selectable("Others", _menuRootCategoryIndex == 3, ImGuiSelectableFlags.None, quarterSpace);

                    if (tilesSelected) _menuRootCategoryIndex = 0;
                    if (ropesSelected) _menuRootCategoryIndex = 1;
                    if (longsSelected) _menuRootCategoryIndex = 2;
                    if (othersSelected) _menuRootCategoryIndex = 3;

                    var listSize = new Vector2(halfWidth, availableSpace.Y - 340);
                    
                    switch (_menuRootCategoryIndex)
                    {
                        case 0: // Tiles-As-Props
                        {
                            if (GLOBALS.TileDex is null) break;

                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                            var textChanged = ImGui.InputTextWithHint(
                                "##TileAsPropSearch", 
                                "Search tiles..", 
                                ref _tileAsPropSearchText, 
                                100, 
                                ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll
                            );

                            if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                                ImGui.SetItemDefaultFocus();
                                ImGui.SetKeyboardFocusHere(-1);
                            }
                            
                            if (textChanged) {
                                SearchTiles();
                            }

                            if (ImGui.BeginListBox("##TileCategories", listSize))
                            {
                                // Not searching
                                if (_tileAsPropSearchResult is null) {
                                    for (var index = 0; index < GLOBALS.TileDex.OrderedTileAsPropCategories.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(GLOBALS.TileDex.OrderedTileAsPropCategories[index],
                                            index == _propsMenuTilesCategoryIndex);
                                        
                                        if (selected)
                                        {
                                            _propsMenuTilesCategoryIndex = index;
                                            Utils.Restrict(
                                                ref _propsMenuTilesIndex, 
                                                0, 
                                                GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length-1);
                                            _currentTile =
                                                GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][
                                                    _propsMenuTilesIndex];
                                        }
                                    }
                                }
                                // Searching
                                else {
                                    for (var c = 0; c < _tileAsPropSearchResult.Categories.Length; c++) {
                                        var (name, originalIndex) = _tileAsPropSearchResult.Categories[c];

                                        var selected = ImGui.Selectable(name, _tileAsPropSearchCategoryIndex == c);

                                        if (selected) {
                                            _tileAsPropSearchCategoryIndex = c;
                                            _tileAsPropSearchIndex = -1;

                                            _propsMenuTilesCategoryIndex = originalIndex;
                                            Utils.Restrict(
                                                ref _propsMenuTilesIndex, 
                                                0, 
                                                GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length-1);
                                            _currentTile =
                                                GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][
                                                    _propsMenuTilesIndex];
                                        }
                                    }
                                }
                                ImGui.EndListBox();
                            }
                            
                            ImGui.SameLine();

                            if (ImGui.BeginListBox("##Tiles", listSize))
                            {
                                // Not searching
                                if (_tileAsPropSearchResult is null) {
                                    for (var index = 0; index < GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length; index++)
                                    {
                                        var currentTilep =
                                            GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][index];
                                        
                                        var selected = ImGui.Selectable(
                                            currentTilep.Name, 
                                            index == _propsMenuTilesIndex
                                        );
                                        
                                        if (ImGui.IsItemHovered())
                                        {
                                            if (_hoveredCategoryIndex != _propsMenuTilesCategoryIndex ||
                                                _hoveredIndex != index ||
                                                _previousRootCategory != _menuRootCategoryIndex)
                                            {
                                                _hoveredCategoryIndex = _propsMenuTilesCategoryIndex;
                                                _hoveredIndex = index;
                                                _previousRootCategory = _menuRootCategoryIndex;

                                                _hoveredTile =
                                                    GLOBALS.TileDex.OrderedTilesAsProps[_hoveredCategoryIndex][
                                                        _hoveredIndex];
                                                
                                                UpdatePropTooltip();
                                            }
                                            
                                            ImGui.BeginTooltip();
                                            rlImGui.ImageRenderTexture(_propTooltip);
                                            ImGui.EndTooltip();
                                        }
                                        
                                        if (selected)
                                        {
                                            _mode = 1;
                                            _propsMenuTilesIndex = index;
                                            _currentTile = currentTilep;
                                        }
                                    }
                                }
                                // Searching
                                else {
                                    for (var t = 0; t < _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex].Length; t++) {
                                        var (tile, originalIndex) = _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex][t];

                                        var selected = ImGui.Selectable(tile.Name, _tileAsPropSearchIndex == t);

                                        if (selected) {
                                            _mode = 1;
                                            _tileAsPropSearchIndex = t;
                                            _propsMenuTilesIndex = originalIndex;
                                            _currentTile = tile;
                                        }

                                        if (ImGui.IsItemHovered())
                                        {
                                            if (_hoveredCategoryIndex != _propsMenuTilesCategoryIndex ||
                                                _hoveredIndex != originalIndex ||
                                                _previousRootCategory != _menuRootCategoryIndex)
                                            {
                                                _hoveredCategoryIndex = _propsMenuTilesCategoryIndex;
                                                _hoveredIndex = originalIndex;
                                                _previousRootCategory = _menuRootCategoryIndex;

                                                _hoveredTile = tile;
                                                
                                                UpdatePropTooltip();
                                            }
                                            
                                            ImGui.BeginTooltip();
                                            rlImGui.ImageRenderTexture(_propTooltip);
                                            ImGui.EndTooltip();
                                        }
                                    }
                                }

                                ImGui.EndListBox();
                            }
                        }
                            break;
                        case 1: // Ropes
                        {
                            if (ImGui.BeginListBox("##Ropes", listSize))
                            {
                                for (var index = 0; index < _ropeNames.Length; index++)
                                {
                                    var selected = ImGui.Selectable(_ropeNames[index], index == _propsMenuRopesIndex);
                                    
                                    if (selected) {
                                        _mode = 1;
                                        _propsMenuRopesIndex = index;
                                    }
                                }
                                ImGui.EndListBox();
                            }
                        }
                            break;
                        case 2: // Longs
                        {
                            if (ImGui.BeginListBox("##Longs", listSize))
                            {
                                for (var index = 0; index < _longNames.Length; index++)
                                {
                                    var selected = ImGui.Selectable(_longNames[index], index == _propsMenuLongsIndex);
                                    
                                    if (selected) {
                                        _mode = 1;
                                        _propsMenuLongsIndex = index;
                                    }
                                }
                                ImGui.EndListBox();
                            }
                        }
                            break;
                        case 3: // Others
                        {
                            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                            var textChanged = ImGui.InputTextWithHint(
                                "##OtherPropsSearch", 
                                "Search props..", 
                                ref _propSearchText, 
                                100, 
                                ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll
                            );

                            if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                                ImGui.SetItemDefaultFocus();
                                ImGui.SetKeyboardFocusHere(-1);
                            }

                            if (textChanged) {
                                SearchProps();
                            }

                            if (ImGui.BeginListBox("##OtherPropCategories", listSize))
                            {
                                // Not searching 
                                if (_propSearchResult is null) {
                                    for (var index = 0; index < _otherCategoryNames.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(_otherCategoryNames[index],
                                            index == _propsMenuOthersCategoryIndex);
                                        
                                        if (selected)
                                        {
                                            _propsMenuOthersCategoryIndex = index;
                                            Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
                                        }
                                    }
                                } else {
                                    for (var c = 0; c < _propSearchResult.Categories.Length; c++) {
                                        var (categoryName, originalIndex) = _propSearchResult.Categories[c];

                                        var selected = ImGui.Selectable(categoryName, _propSearchCategoryIndex == c);

                                        if (selected) {
                                            _propSearchCategoryIndex = c;
                                            _propsMenuOthersCategoryIndex = originalIndex;
                                            Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
                                        }
                                    }
                                }
                                ImGui.EndListBox();
                            }
                            
                            ImGui.SameLine();

                            if (ImGui.BeginListBox("##OtherProps", listSize))
                            {
                                // Not searching
                                if (_propSearchResult is null) {
                                    var array = _otherNames[_propsMenuOthersCategoryIndex];

                                    for (var index = 0; index < array.Length; index++)
                                    {
                                        var selected = ImGui.Selectable(array[index], index == _propsMenuOthersIndex);
                                        
                                        if (ImGui.IsItemHovered())
                                        {
                                            if (_hoveredCategoryIndex != _propsMenuOthersCategoryIndex ||
                                                _hoveredIndex != index ||
                                                _previousRootCategory != _menuRootCategoryIndex)
                                            {
                                                _hoveredCategoryIndex = _propsMenuOthersCategoryIndex;
                                                _hoveredIndex = index;
                                                _previousRootCategory = _menuRootCategoryIndex;
                                                
                                                UpdatePropTooltip();
                                            }

                                            ImGui.BeginTooltip();
                                            rlImGui.ImageRenderTexture(_propTooltip);
                                            ImGui.EndTooltip();
                                        }
                                        
                                        if (selected) {
                                            _mode = 1;
                                            _propsMenuOthersIndex = index;
                                        }
                                    }
                                } else {
                                    for (var p = 0; p < _propSearchResult.Props[_propSearchCategoryIndex].Length; p++) {
                                        var (prop, originalIndex) = _propSearchResult.Props[_propSearchCategoryIndex][p];

                                        var selected = ImGui.Selectable(prop.Name, _propSearchIndex == p);

                                        if (selected) {
                                            _propSearchIndex = p;
                                            _mode = 1;
                                            _propsMenuOthersIndex = originalIndex;
                                        }

                                        if (ImGui.IsItemHovered())
                                        {
                                            if (_hoveredCategoryIndex != _propsMenuOthersCategoryIndex ||
                                                _hoveredIndex != originalIndex ||
                                                _previousRootCategory != _menuRootCategoryIndex)
                                            {
                                                _hoveredCategoryIndex = _propsMenuOthersCategoryIndex;
                                                _hoveredIndex = originalIndex;
                                                _previousRootCategory = _menuRootCategoryIndex;
                                                
                                                UpdatePropTooltip();
                                            }

                                            ImGui.BeginTooltip();
                                            rlImGui.ImageRenderTexture(_propTooltip);
                                            ImGui.EndTooltip();
                                        }
                                    }
                                }
                                ImGui.EndListBox();
                            }
                        }
                            break;
                    }

                    ImGui.SeparatorText("Placement Options");
                    
                    // Seed

                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Seed", ref _defaultSeed);
                    
                    // Rotation
                    
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Rotation", ref _placementRotation))
                    {
                        // _shouldRedrawLevel = true;
                        _shouldRedrawPropLayer = true;
                    }
                    
                    ImGui.SetNextItemWidth(100);
                    ImGui.SliderInt("Rotation Steps", ref _placementRotationSteps, 1, 10);
            
                    // Depth
            
                    var currentTile = GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
                    var currentRope = GLOBALS.RopeProps[_propsMenuRopesIndex];
                    var currentLong = GLOBALS.LongProps[_propsMenuLongsIndex];
                    var currentOther = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
            
                    var depth = _defaultDepth;
                        
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Depth", ref depth))
                    {
                        _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    }

                    Utils.Restrict(ref depth, -29, 0);

                    _defaultDepth = depth;

                    var propDepthTo = _menuRootCategoryIndex switch
                    {
                        0 => Utils.GetPropDepth(currentTile),
                        1 => Utils.GetPropDepth(currentRope),
                        2 => Utils.GetPropDepth(currentLong),
                        3 => Utils.GetPropDepth(currentOther),
                        _ => 0
                    };
            
                    ImGui.Text($"From {_defaultDepth} to {_defaultDepth - propDepthTo}");
            
                    // Variation

                    if (_menuRootCategoryIndex == 3 && currentOther is IVariableInit v)
                    {
                        var variations = v.Variations;

                        if (variations > 1) {
                            var variation = _defaultVariation;
                    
                            ImGui.SetNextItemWidth(100);
                            if (ImGui.InputInt("Variation", ref variation))
                            {
                                // _shouldRedrawLevel = true;
                                _shouldRedrawPropLayer = true;
                            }

                            Utils.Restrict(ref variation, 0, variations-1);

                            _defaultVariation = variation;
                        } else {
                            _defaultVariation = 0;
                        }
                    }
                    
                    // Misc
                    
                    ImGui.SeparatorText("Misc");

                    ImGui.Checkbox("Tooltip", ref _tooltip);
                }

                ImGui.End();
            }

            //

            var listOpened = ImGui.Begin("Props List##PropsListWindow");
            
            var listPos = ImGui.GetWindowPos();
            var listSpace = ImGui.GetWindowSize();

            _isPropsListHovered = CheckCollisionPointRec(tileMouse, new(listPos.X - 5, listPos.Y, listSpace.X + 10, listSpace.Y));

            if (listOpened)
            {
                ImGui.SeparatorText("Placed Props");
                
                if (ImGui.Button("Select All", ImGui.GetContentRegionAvail() with { Y = 20 }))
                {
                    _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
                    for (var i = 0; i < _selected.Length; i++) _selected[i] = true;
                }

                if (ImGui.BeginListBox("Props", ImGui.GetContentRegionAvail() with { Y = ImGui.GetContentRegionAvail().Y - 350 }))
                {
                    for (var index = 0; index < GLOBALS.Level.Props.Length; index++)
                    {
                        ref var currentProp = ref GLOBALS.Level.Props[index];
                        
                        var selected = ImGui.Selectable(
                            $"{index}. {currentProp.prop.Name}{(_hidden[index] ? " [hidden]" : "")}", 
                            _selected[index]);
                        
                        if (selected)
                        {
                            _mode = 0;

                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            
                            if (IsKeyDown(KeyboardKey.LeftControl))
                            {
                                _selected[index] = !_selected[index];
                            }
                            else if (IsKeyDown(KeyboardKey.LeftShift))
                            {
                                var otherSelected = Array.IndexOf(_selected, true);
                                
                                if (otherSelected == -1) _selected = _selected.Select((p, i) => i == index).ToArray();

                                var first = Math.Min(otherSelected, index);
                                var second = Math.Max(otherSelected, index);

                                for (var i = 0; i < _selected.Length; i++)
                                {
                                    _selected[i] = i >= first && i <= second;
                                }
                            }
                            else
                            {
                                _selected = _selected.Select((p, i) => i == index).ToArray();
                            }
                        }
                    }
                    
                    ImGui.EndListBox();
                }

                var hideSelected = ImGui.Button("Hide Selected", ImGui.GetContentRegionAvail() with { Y = 20 });

                if (hideSelected)
                {
                    _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
                    for (var i = 0; i < _hidden.Length; i++)
                    {
                        if (_selected[i]) _hidden[i] = !_hidden[i];
                    }
                }

                var deleteSelected = ImGui.Button("Delete Selected", ImGui.GetContentRegionAvail() with { Y = 20 });

                if (deleteSelected)
                {
                    _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
                    GLOBALS.Level.Props = _selected
                        .Select((s, i) => (s, i))
                        .Where(v => !v.s)
                        .Select(v => GLOBALS.Level.Props[v.i])
                        .ToArray();

                    _selected = new bool [GLOBALS.Level.Props.Length];
                    _hidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden


                    fetchedSelected = GLOBALS.Level.Props
                        .Select((prop, index) => (prop, index))
                        .Where(p => _selected[p.index])
                        .Select(p => p)
                        .ToArray();

                    ImportRopeModels();
                }

                //

                rlImGui.ImageRenderTexture(_propModeIndicatorsRT);

                //
                
                ImGui.SeparatorText("Selected Prop Options");
                
                if (fetchedSelected.Length == 1)
                {
                    var (selectedProp, _) = fetchedSelected[0];
                    
                    // Render Order

                    var renderOrder = selectedProp.prop.Extras.Settings.RenderOrder;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Render Order", ref renderOrder))
                        selectedProp.prop.Extras.Settings.RenderOrder = renderOrder;
            
                    // Seed

                    var seed = selectedProp.prop.Extras.Settings.Seed;
            
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Seed", ref seed);

                    selectedProp.prop.Extras.Settings.Seed = seed;
            
                    // Depth
            
                    ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

                    var depth = selectedProp.prop.Depth;
            
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Depth", ref depth))
                    {
                        _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    }
            
                    Utils.Restrict(ref depth, -29, 0);
            
                    selectedProp.prop.Depth = depth;
            
                    // Variation

                    if (selectedProp.prop.Extras.Settings is IVariable v)
                    {
                        var init = GLOBALS.Props[selectedProp.position.category][selectedProp.position.index];
                        var variations = (init as IVariableInit).Variations;

                        if (variations > 1) {
                            ImGui.SetNextItemWidth(100);
                            var variation = v.Variation;
                            if (ImGui.InputInt("Variation", ref variation))
                            {
                                // _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                else _shouldRedrawLevel = true;
                                
                                Utils.Restrict(ref variation, 0, variations -1);

                                v.Variation = variation;
                            }
                        } else {
                            v.Variation = 0;
                        }
                    }
                    
                    // Colored

                    if (selectedProp.type == InitPropType.VariedSoft && 
                        selectedProp.prop.Extras.Settings is PropVariedSoftSettings vs &&
                        ((InitVariedSoftProp) GLOBALS.Props[selectedProp.position.category][selectedProp.position.index]).Colorize != 0)
                    {
                        var applyColor = vs.ApplyColor is 1;

                        if (ImGui.Checkbox("Apply Color", ref applyColor))
                        {
                            vs.ApplyColor = applyColor ? 1 : 0;
                        }
                    }

                    // Custom Depth

                    if (selectedProp.prop.Extras.Settings is ICustomDepth cd) {
                        ImGui.SetNextItemWidth(100);

                        var customDepth = cd.CustomDepth;

                        if (ImGui.InputInt("Custom Depth", ref customDepth)) {
                            cd.CustomDepth = customDepth;
                        }
                    }
                    
                    // Rope
                    
                    if (fetchedSelected.Length == 1 && fetchedSelected[0].prop.type == InitPropType.Rope)
                    {
                        ImGui.SeparatorText("Rope Options");
                        
                        //

                        if (fetchedSelected[0].prop.prop.Name == "Zero-G Tube")
                        {
                            var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.prop.Extras.Settings);
                            var applyColor = ropeSettings.ApplyColor is 1;

                            if (ImGui.Checkbox("Apply Color", ref applyColor))
                            {
                                ropeSettings.ApplyColor = applyColor ? 1 : 0;
                            }
                        }
                        else if (fetchedSelected[0].prop.prop.Name is "Wire" or "Zero-G Wire")
                        {
                            var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.prop.Extras.Settings);
                            var thickness = ropeSettings.Thickness ?? 2f;

                            ImGui.SetNextItemWidth(100);
                            var thicknessUpdated = ImGui.InputFloat("Thickness", ref thickness, 0.5f, 1f);
                            if (thicknessUpdated)
                            {
                                ropeSettings.Thickness = thickness;
                            }
                        }
                        
                        var modelIndex = -1;

                        for (var i = 0; i < _models.Length; i++)
                        {
                            if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
                        }

                        if (modelIndex == -1)
                        {
#if DEBUG
                            Logger.Fatal(
                                $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
                            throw new Exception(
                                message:
                                $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
#else
                    goto ropeNotFound;
#endif
                        }

                        ref var currentModel = ref _models[modelIndex];

                        var oldSegmentCount = GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints.Length;
                        var segmentCount = oldSegmentCount;
                        
                        var switchSimSelected = ImGui.Button(currentModel.simSwitch ? "Simulation" : "Bezier Path");

                        if (switchSimSelected)
                        {
                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            currentModel.simSwitch = !currentModel.simSwitch;
                        }

                        ImGui.SetNextItemWidth(100);

                        if (ImGui.InputInt("Segment Count", ref segmentCount))
                        {
                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        }

                        // Update segment count if needed

                        if (segmentCount < 1) segmentCount = 1;

                        if (segmentCount > oldSegmentCount)
                        {
                            GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints =
                            [
                                ..GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints, new Vector2()
                            ];
                        }
                        else if (segmentCount < oldSegmentCount)
                        {
                            GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints =
                                GLOBALS.Level.Props[currentModel.index].prop.Extras.RopePoints[..^1];
                        }

                        if (segmentCount != oldSegmentCount)
                        {
                            UpdateRopeModelSegments();
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else _shouldRedrawLevel = true;
                        }
                        
                        //

                        if (ImGui.Checkbox("Simulate Rope", ref _ropeMode))
                        {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else _shouldRedrawLevel = true;
                        }

                        if (currentModel.simSwitch) // Simulation mode
                        {
                            var cycleFpsSelected = ImGui.Button($"{60 / _ropeSimulationFrameCut} FPS");

                            if (cycleFpsSelected)
                            {
                                _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;
                                // _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                else _shouldRedrawLevel = true;
                            }

                            var release = (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings)
                                .Release;

                            var releaseClicked = ImGui.Button(release switch
                            {
                                PropRopeRelease.Left => "Release Left",
                                PropRopeRelease.None => "Release None",
                                PropRopeRelease.Right => "Release Right",
                                _ => "Error"
                            });

                            if (releaseClicked)
                            {
                                // _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                else _shouldRedrawLevel = true;
                                
                                release = (PropRopeRelease)((int)release + 1);
                                if ((int)release > 2) release = 0;

                                (fetchedSelected[0].prop.prop.Extras.Settings as PropRopeSettings).Release =
                                    release;
                            }
                        }
                        else // Bezier mode
                        {
                            var oldHandlePointNumber = currentModel.bezierHandles.Length;
                            var handlePointNumber = oldHandlePointNumber;

                            ImGui.SetNextItemWidth(100);
                            if (ImGui.InputInt("Control Points", ref handlePointNumber))
                            {
                                _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;

                                Utils.Restrict(ref handlePointNumber, 1);
                            }

                            var quads = GLOBALS.Level.Props[currentModel.index].prop.Quads;
                            var center = Utils.QuadsCenter(ref quads);

                            if (handlePointNumber > oldHandlePointNumber)
                            {
                                currentModel.bezierHandles = [..currentModel.bezierHandles, center];
                            }
                            else if (handlePointNumber < oldHandlePointNumber)
                            {
                                currentModel.bezierHandles = currentModel.bezierHandles[..^1];
                            }
                        }

                        ropeNotFound:
                        {
                        }
                    }
                }
                else if (fetchedSelected.Length > 1 && 
                            Utils.AllEqual(fetchedSelected.Select(f => f.prop.prop.Depth),
                                fetchedSelected[0].prop.prop.Depth))
                {
                    ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

                    var depth = fetchedSelected[0].prop.prop.Depth;
            
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Depth", ref depth))
                    {
                        _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    }
            
                    Utils.Restrict(ref depth, -29, 0);
            
                    foreach (var selected in fetchedSelected)
                        selected.prop.prop.Depth = depth;
                }
                
            }

            // Shortcuts window
            if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
            {
                var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.PropsEditor);

                _isShortcutsWinHovered = CheckCollisionPointRec(
                    tileMouse, 
                    shortcutWindowRect with
                    {
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
        
        }
        #endregion

        EndDrawing();
        #endregion
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
