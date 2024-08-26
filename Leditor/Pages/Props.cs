using System.Numerics;
using ImGuiNET;
using Leditor.Data.Tiles;
using Leditor.Data.Props.Legacy;
using rlImGui_cs;
using static Raylib_cs.Raylib;
using RenderTexture2D = Leditor.RL.Managed.RenderTexture2D;
using Leditor.Types;
using System.Drawing.Printing;


namespace Leditor.Pages;

#nullable enable

// internal class PropsEditorPage : EditorPage, IContextListener
// {
//     public override void Dispose()
//     {
//         if (Disposed) return;
        
//         Disposed = true;
        
//         _propTooltip.Dispose();
//         _propLayerRT.Dispose();
//         _propModeIndicatorsRT.Dispose();
//     }

//     ~PropsEditorPage()
//     {
//         if (!Disposed) throw new InvalidOperationException("PropsEditorPage was not disposed by consumer");
//     }

//     private Camera2D _camera;

//     private readonly PropsShortcuts _shortcuts = GLOBALS.Settings.Shortcuts.PropsEditor;

//     private RenderTexture2D _propLayerRT = new(0, 0);

//     private bool _shouldRedrawPropLayer = true;

//     private void DrawPropLayerRT() {
//         BeginTextureMode(_propLayerRT);
//         ClearBackground(Color.White with { A = 0 });

//         for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//         {
//             if (_hidden[p]) continue;
            
//             var current = GLOBALS.Level.Props[p];
            
//             // Filter based on depth
//             if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (current.Depth <= -(GLOBALS.Layer+1)*10 || current.Depth > -GLOBALS.Layer*10)) continue;

//             var (category, index) = current.Position;
//             var quads = current.Quad;
            
//             // origin must be the center
//             // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
            
//             // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
//             Printers.DrawProp(
//                 category, 
//                 index, 
//                 current, 
//                 20, 
//                 GLOBALS.Settings.GeneralSettings.DrawPropMode, 
//                 GLOBALS.SelectedPalette,
//                 GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
//             );

//             // Draw Rope Point
//             if (current.Type == InitPropType.Rope)
//             {
//                 foreach (var point in current.Extras.RopePoints)
//                 {
//                     DrawCircleV(point, 3f, Color.White);
//                 }
//             }
            
//             if (_selected[p])
//             {
//                 // Side Lines
                
//                 DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                
//                 // Quad Points

//                 if (_stretchingProp)
//                 {
//                     if (current.Type == InitPropType.Rope)
//                     {
//                         DrawCircleV(
//                             Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
//                                 new(2f, 2f)), 
//                             5f, 
//                             Color.Blue
//                         );
                        
//                         DrawCircleV(
//                             Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
//                                 new(2f, 2f)), 
//                             5f, 
//                             Color.Blue
//                         );
                        
//                         /*DrawCircleV(quads.TopLeft, 2f, Color.Blue);
//                         DrawCircleV(quads.TopRight, 2f, Color.Blue);
//                         DrawCircleV(quads.BottomRight, 2f, Color.Blue);
//                         DrawCircleV(quads.BottomLeft, 2f, Color.Blue);*/
//                     }
//                     else if (current.Type == InitPropType.Long)
//                     {
//                         var sides = Utils.LongSides(current.Quad);
                        
//                         DrawCircleV(sides.left, 5f, Color.Blue);
//                         DrawCircleV(sides.top, 5f, Color.Blue);
//                         DrawCircleV(sides.right, 5f, Color.Blue);
//                         DrawCircleV(sides.bottom, 5f, Color.Blue);
//                     }
//                     else
//                     {
//                         DrawCircleV(quads.TopLeft, 5f, Color.Blue);
//                         DrawCircleV(quads.TopRight, 5f, Color.Blue);
//                         DrawCircleV(quads.BottomRight, 5f, Color.Blue);
//                         DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
//                     }
//                 }
//                 else if (_scalingProps)
//                 {
//                     var center = Utils.QuadsCenter(ref quads);
                    
//                     switch (_stretchAxes)
//                     {
//                         case 1:
//                             DrawLineEx(
//                                 center with { X = -10 }, 
//                                 center with { X = GLOBALS.Level.Width*20 + 10 }, 
//                                 2f, 
//                                 Color.Red
//                             );
//                             break;
//                         case 2:
//                             DrawLineEx(
//                                 center with { Y = -10 },
//                                 center with { Y = GLOBALS.Level.Height*20 + 10 },
//                                 2f,
//                                 Color.Green
//                             );
//                             break;
//                     }
//                 }
                
//                 // Draw Rope Point
//                 if (current.Type == InitPropType.Rope)
//                 {
//                     if (_editingPropPoints)
//                     {
//                         foreach (var point in current.Extras.RopePoints)
//                         {
//                             DrawCircleV(point, 3f, Color.Red);
//                         }
//                     }
//                     else
//                     {
//                         foreach (var point in current.Extras.RopePoints)
//                         {
//                             DrawCircleV(point, 2f, Color.Orange);
//                         }
//                     }

//                     var p1 = p;
//                     var model = _models.SingleOrDefault(r => r.index == p1);

//                     if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
//                     {
//                         foreach (var handle in model.model.BezierHandles) {
//                             DrawCircleV(handle, 6f, Color.White);
//                             DrawCircleV(handle, 4f, Color.Green);
//                         }
//                     }
//                 }
//             }
//         }

//         EndTextureMode();
//     }


//     private bool _ropeInitialPlacement;
//     private int _additionalInitialRopeSegments;

//     private bool _longInitialPlacement;
    
    
//     private bool _lockedPlacement;

//     private int _hoveredCategoryIndex = -1;
//     private int _hoveredIndex = -1;
//     private int _previousRootCategory;

//     private bool _tooltip = true;
    
//     private RenderTexture2D _propTooltip = new(0, 0);

//     private RenderTexture2D _propModeIndicatorsRT = new(40 * 5, 40);

//     private bool _shouldUpdateModeIndicatorsRT = true;

//     private void UpdateModeIndicators() {
//         var darkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme;

//         var color = darkTheme ? Color.White : Color.Black;

//         BeginTextureMode(_propModeIndicatorsRT);

//         ClearBackground(darkTheme ? Color.Black : Color.White);

//         if (_movingProps)           DrawRectangle(  0, 0, 40, 40, Color.Blue);
//         if (_rotatingProps)         DrawRectangle( 40, 0, 40, 40, Color.Blue);
//         if (_scalingProps)          DrawRectangle( 80, 0, 40, 40, Color.Blue);
//         if (_stretchingProp)        DrawRectangle(120, 0, 40, 40, Color.Blue);
//         if (_editingPropPoints)     DrawRectangle(160, 0, 40, 40, Color.Blue);

//         var texture1 = GLOBALS.Textures.PropEditModes[0];
//         var texture2 = GLOBALS.Textures.PropEditModes[1];
//         var texture3 = GLOBALS.Textures.PropEditModes[2];
//         var texture4 = GLOBALS.Textures.PropEditModes[3];
//         var texture5 = GLOBALS.Textures.PropEditModes[4];

//         DrawTexturePro(texture1, new Rectangle(0, 0, texture1.Width, texture1.Height), new Rectangle(  0, 0, 40, 40), new Vector2(0, 0), 0, _movingProps       ? Color.White : color);
//         DrawTexturePro(texture2, new Rectangle(0, 0, texture2.Width, texture1.Height), new Rectangle( 40, 0, 40, 40), new Vector2(0, 0), 0, _rotatingProps     ? Color.White : color);
//         DrawTexturePro(texture3, new Rectangle(0, 0, texture3.Width, texture1.Height), new Rectangle( 80, 0, 40, 40), new Vector2(0, 0), 0, _scalingProps      ? Color.White : color);
//         DrawTexturePro(texture4, new Rectangle(0, 0, texture4.Width, texture1.Height), new Rectangle(120, 0, 40, 40), new Vector2(0, 0), 0, _stretchingProp    ? Color.White : color);
//         DrawTexturePro(texture5, new Rectangle(0, 0, texture5.Width, texture1.Height), new Rectangle(160, 0, 40, 40), new Vector2(0, 0), 0, _editingPropPoints ? Color.White : color);

//         EndTextureMode();
//     }

//     private void UpdatePropTooltip()
//     {
//         _propTooltip.Dispose();

//         var tint = GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.LightGray : Color.Gray;

//         // TODO: This should really be made into an enum
//         switch (_menuRootCategoryIndex)
//         {
//             case 0: // Tile-As-Prop
//             {
//                 if (_hoveredTile is null) return;
                
//                 var (width, height) = _hoveredTile.Size;
                
//                 _propTooltip = new RenderTexture2D(20 * width + 20, 20 * height + 20);
                
//                 BeginTextureMode(_propTooltip);
//                 ClearBackground(Color.White with { A = 0 });
                    
//                 Printers.DrawTileAsPropColored(
//                     _hoveredTile, 
//                     new Vector2(0, 0), 
//                     new  Vector2(-10, -10), 
//                     tint, 
//                     0, 
//                     20
//                 );
//                 EndTextureMode();
//             }
//                 break;
            
//             case 1: // Ropes
//                 break;
            
//             case 2: // Longs
//                 break;

//             case 3: // Others
//             {
//                 var prop = GLOBALS.Props[_hoveredCategoryIndex][_hoveredIndex];
//                 var texture = GLOBALS.Textures.Props[_hoveredCategoryIndex][_hoveredIndex];

//                 var (width, height) = Utils.GetPropSize(prop);

//                 if (width == -1 && height == -1)
//                 {
//                     _propTooltip = new RenderTexture2D(texture.Width, texture.Height);
//                 }
//                 else
//                 {
//                     _propTooltip = new RenderTexture2D(width, height);
//                 }
                
//                 BeginTextureMode(_propTooltip);
//                 ClearBackground(Color.White with { A = 0 });

//                 var quad = (width, height) is (-1, -1) 
//                     ? new Data.Quad(
//                         new Vector2(0, 0),
//                         new Vector2(texture.Width, 0),
//                         new Vector2(texture.Width, texture.Height),
//                         new Vector2(0, texture.Height)
//                     )
//                     : new Data.Quad(
//                     new Vector2(0, 0),
//                     new Vector2(width, 0),
//                     new Vector2(width, height),
//                     new Vector2(0, height)
//                     );
                
//                 Printers.DrawProp(prop, texture, quad);
                
//                 EndTextureMode();
                
//             }
//                 break;
//         }
//     }


//     private RenderTexture2D _tempGeoL = new(0, 0);

//     private bool _shouldRedrawLevel = true;

//     private void RedrawLevel()
//     {
//         var lWidth = GLOBALS.Level.Width * 20;
//         var lHeight = GLOBALS.Level.Height * 20;

//         var paletteTiles = GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette;

//         BeginTextureMode(GLOBALS.Textures.GeneralLevel);
//         ClearBackground(new(170, 170, 170, 255));
//         EndTextureMode();
        
//         #region TileEditorLayer3
//         if (_showTileLayer3)
//         {
//             // Draw geos first
//             if (paletteTiles) {
//                 Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 2, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

//                 // BeginTextureMode(geoL);
//                 // ClearBackground(Color.White with { A = 0 });
                
//                 // Printers.DrawGeoLayer(
//                 //     2, 
//                 //     16, 
//                 //     false, 
//                 //     Color.Black
//                 // );
//                 // EndTextureMode();

//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 var shader = GLOBALS.Shaders.Palette;

//                 BeginShaderMode(shader);

//                 var textureLoc = GetShaderLocation(shader, "inputTexture");
//                 var paletteLoc = GetShaderLocation(shader, "paletteTexture");

//                 var depthLoc = GetShaderLocation(shader, "depth");
//                 var shadingLoc = GetShaderLocation(shader, "shading");

//                 SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
//                 SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

//                 SetShaderValue(shader, depthLoc, 20, ShaderUniformDataType.Int);
//                 SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

//                 DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 2 ? Color.Black : Color.Black with { A = 120 });

//                 EndShaderMode();

//                 EndTextureMode();
//             } else {
//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 Printers.DrawGeoLayer(
//                     2, 
//                     20, 
//                     false, 
//                     GLOBALS.Layer == 2 ? Color.Black : Color.Black with { A = 120 }
//                 );

//                 EndTextureMode();
//             }


//             // then draw the tiles



//             if (_showLayer3Tiles)
//             {
//                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
//                     // Printers.DrawTileLayer(
//                     //     GLOBALS.Layer,
//                     //     2, 
//                     //     16, 
//                     //     false, 
//                     //     TileDrawMode.Palette,
//                     //     GLOBALS.SelectedPalette!.Value,
//                     //     70
//                     // );
//                     Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 2, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);

//                 } else {
//                     BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                     Printers.DrawTileLayer(
//                         GLOBALS.Layer,
//                         2, 
//                         20, 
//                         false, 
//                         GLOBALS.Settings.GeneralSettings.DrawTileMode,
//                         (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
//                         visibleStrays: false,
//                         materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
//                     );

//                     EndTextureMode();
//                 }
//             }
            
//             // Then draw the props

//             if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 2)) {

//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);


//                 for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                 {
//                     if (_hidden[p]) continue;
                    
//                     var current = GLOBALS.Level.Props[p];
                    
//                     // Filter based on depth
//                     if (current.Depth > -20) continue;

//                     var (category, index) = current.Position;
//                     var quads = current.Quad;
                    
//                     // origin must be the center
//                     // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
//                     // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
//                     Printers.DrawProp(
//                         category, 
//                         index, 
//                         current, 
//                         20, 
//                         GLOBALS.Settings.GeneralSettings.DrawPropMode, 
//                         GLOBALS.SelectedPalette,
//                         GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
//                     );

//                     // Draw Rope Point
//                     if (current.Type == InitPropType.Rope)
//                     {
//                         foreach (var point in current.Extras.RopePoints)
//                         {
//                             DrawCircleV(point, 3f, Color.White);
//                         }
//                     }
                    
//                     if (_selected[p])
//                     {
//                         // Side Lines
                        
//                         DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
//                         // Quad Points

//                         if (_stretchingProp)
//                         {
//                             if (current.Type == InitPropType.Rope)
//                             {
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 DrawCircleV(quads.TopLeft, 2f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
//                             }
//                             else
//                             {
//                                 DrawCircleV(quads.TopLeft, 5f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
//                             }
//                         }
//                         else if (_scalingProps)
//                         {
//                             var center = Utils.QuadsCenter(ref quads);
                            
//                             switch (_stretchAxes)
//                             {
//                                 case 1:
//                                     DrawLineEx(
//                                         center with { X = -10 }, 
//                                         center with { X = GLOBALS.Level.Width*20 + 10 }, 
//                                         2f, 
//                                         Color.Red
//                                     );
//                                     break;
//                                 case 2:
//                                     DrawLineEx(
//                                         center with { Y = -10 },
//                                         center with { Y = GLOBALS.Level.Height*20 + 10 },
//                                         2f,
//                                         Color.Green
//                                     );
//                                     break;
//                             }
//                         }
                        
//                         // Draw Rope Point
//                         if (current.Type == InitPropType.Rope)
//                         {
//                             if (_editingPropPoints)
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 3f, Color.Red);
//                                 }
//                             }
//                             else
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 2f, Color.Orange);
//                                 }
//                             }
                            
//                             var p1 = p;
//                             var model = _models.SingleOrDefault(r => r.index == p1);

//                             if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
//                             {
//                                 foreach (var handle in model.model.BezierHandles) {
//                                     DrawCircleV(handle, 6f, Color.White);
//                                     DrawCircleV(handle, 4f, Color.Green);
//                                 }
//                             }
//                         }
//                     }
//                 }

//                 EndTextureMode();
//             }

//         }
//         #endregion

//         #region TileEditorLayer2
//         if (_showTileLayer2 && (GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers || GLOBALS.Layer is 0 or 1))
//         {
//             if (paletteTiles) {
//                 Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 1, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

//                 // BeginTextureMode(geoL);
//                 // ClearBackground(Color.White with { A = 0 });
                
//                 // Printers.DrawGeoLayer(
//                 //     1, 
//                 //     16, 
//                 //     false, 
//                 //     Color.Black
//                 // );
//                 // EndTextureMode();

//                 var shader = GLOBALS.Shaders.Palette;

//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 BeginShaderMode(shader);

//                 var textureLoc = GetShaderLocation(shader, "inputTexture");
//                 var paletteLoc = GetShaderLocation(shader, "paletteTexture");

//                 var depthLoc = GetShaderLocation(shader, "depth");
//                 var shadingLoc = GetShaderLocation(shader, "shading");

//                 SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
//                 SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

//                 SetShaderValue(shader, depthLoc, 10, ShaderUniformDataType.Int);
//                 SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

//                 DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 1 ? Color.Black : Color.Black with { A = 140 });

//                 EndShaderMode();

//                 EndTextureMode();
//             } else {
//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 Printers.DrawGeoLayer(
//                     1, 
//                     20, 
//                     false, 
//                     GLOBALS.Layer == 1
//                         ? Color.Black 
//                         : Color.Black with { A = 140 }
//                 );

//                 EndTextureMode();
//             }



//             // Draw layer 2 tiles

//             if (_showLayer2Tiles)
//             {
//                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
//                     Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 1, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);
                    
                    
//                     // Printers.DrawTileLayer(
//                     //     GLOBALS.Layer,
//                     //     1, 
//                     //     16, 
//                     //     false, 
//                     //     TileDrawMode.Palette,
//                     //     GLOBALS.SelectedPalette!.Value,
//                     //     70
//                     // );
//                 } else {
//                     BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                     Printers.DrawTileLayer(
//                         GLOBALS.Layer,
//                         1, 
//                         20, 
//                         false, 
//                         GLOBALS.Settings.GeneralSettings.DrawTileMode,
//                         (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
//                         visibleStrays: false,
//                         materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
//                     );

//                     EndTextureMode();
//                 }
//             }

//             BeginTextureMode(GLOBALS.Textures.GeneralLevel);

            
//             // then draw the props

//             if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 1)) {
//                 for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                 {
//                     if (_hidden[p]) continue;
                    
//                     var current = GLOBALS.Level.Props[p];
                    
//                     // Filter based on depth
//                     if (current.Depth > -10 || current.Depth < -19) continue;

//                     var (category, index) = current.Position;
//                     var quads = current.Quad;
                    
//                     // origin must be the center
//                     // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
//                     // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
//                     Printers.DrawProp(
//                         category, 
//                         index, 
//                         current, 
//                         20, 
//                         GLOBALS.Settings.GeneralSettings.DrawPropMode, 
//                         GLOBALS.SelectedPalette,
//                         GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
//                     );

//                     // Draw Rope Point
//                     if (current.Type == InitPropType.Rope)
//                     {
//                         foreach (var point in current.Extras.RopePoints)
//                         {
//                             DrawCircleV(point, 3f, Color.White);
//                         }
//                     }
                    
//                     if (_selected[p])
//                     {
//                         // Side Lines
                        
//                         DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
//                         // Quad Points

//                         if (_stretchingProp)
//                         {
//                             if (current.Type == InitPropType.Rope)
//                             {
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 DrawCircleV(quads.TopLeft, 2f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 2f, Color.Blue);
//                             }
//                             else
//                             {
//                                 DrawCircleV(quads.TopLeft, 5f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
//                             }
//                         }
//                         else if (_scalingProps)
//                         {
//                             var center = Utils.QuadsCenter(ref quads);
                            
//                             switch (_stretchAxes)
//                             {
//                                 case 1:
//                                     DrawLineEx(
//                                         center with { X = -10 }, 
//                                         center with { X = GLOBALS.Level.Width*20 + 10 }, 
//                                         2f, 
//                                         Color.Red
//                                     );
//                                     break;
//                                 case 2:
//                                     DrawLineEx(
//                                         center with { Y = -10 },
//                                         center with { Y = GLOBALS.Level.Height*20 + 10 },
//                                         2f,
//                                         Color.Green
//                                     );
//                                     break;
//                             }
//                         }
                        
//                         // Draw Rope Point
//                         if (current.Type == InitPropType.Rope)
//                         {
//                             if (_editingPropPoints)
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 3f, Color.Red);
//                                 }
//                             }
//                             else
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 2f, Color.Orange);
//                                 }
//                             }
                            
//                             var p1 = p;
//                             var model = _models.SingleOrDefault(r => r.index == p1);

//                             if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
//                             {
//                                 foreach (var handle in model.model.BezierHandles) {
//                                     DrawCircleV(handle, 6f, Color.White);
//                                     DrawCircleV(handle, 4f, Color.Green);
//                                 }
//                             }
//                         }
//                     }
//                 }

//                 EndTextureMode();
//             }

            
//         }
//         #endregion

//         if (GLOBALS.Settings.GeneralSettings.Water && !GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1) {
//             BeginTextureMode(GLOBALS.Textures.GeneralLevel);
//             DrawRectangle(
//                 (-1) * 20,
//                 (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 20,
//                 (GLOBALS.Level.Width + 2) * 20,
//                 (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 20,
//                 new Color(0, 0, 255, (int)GLOBALS.Settings.GeneralSettings.WaterOpacity)
//             );
//             EndTextureMode();
//         }

//         #region TileEditorLayer1
//         if (_showTileLayer1 && (GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers || GLOBALS.Layer == 0))
//         {
//             if (paletteTiles) {
//                 Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 0, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

//                 // BeginTextureMode(geoL);
//                 // ClearBackground(Color.White with { A = 0 });
                
//                 // Printers.DrawGeoLayer(
//                 //     0, 
//                 //     16, 
//                 //     false, 
//                 //     Color.Black
//                 // );
//                 // EndTextureMode();

//                 var shader = GLOBALS.Shaders.Palette;

//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 BeginShaderMode(shader);

//                 var textureLoc = GetShaderLocation(shader, "inputTexture");
//                 var paletteLoc = GetShaderLocation(shader, "paletteTexture");

//                 var depthLoc = GetShaderLocation(shader, "depth");
//                 var shadingLoc = GetShaderLocation(shader, "shading");

//                 SetShaderValueTexture(shader, textureLoc, _tempGeoL.Raw.Texture);
//                 SetShaderValueTexture(shader, paletteLoc, GLOBALS.SelectedPalette!.Value);

//                 SetShaderValue(shader, depthLoc, 0, ShaderUniformDataType.Int);
//                 SetShaderValue(shader, shadingLoc, 1, ShaderUniformDataType.Int);

//                 DrawTexture(_tempGeoL.Raw.Texture, 0, 0, GLOBALS.Layer == 0 ? Color.Black : Color.Black with { A = 120 });

//                 EndShaderMode();

//                 EndTextureMode();
//             } else {
//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);

//                 Printers.DrawGeoLayer(
//                     0, 
//                     20, 
//                     false, 
//                     GLOBALS.Layer == 0
//                         ? Color.Black 
//                         : Color.Black with { A = 120 }
//                 );

//                 EndTextureMode();
//             }


//             // Draw layer 1 tiles

//             if (_showLayer1Tiles)
//             {
//                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
//                         Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 0, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);
                    
//                     // Printers.DrawTileLayer(
//                     //     GLOBALS.Layer,
//                     //     0, 
//                     //     16, 
//                     //     false, 
//                     //     TileDrawMode.Palette,
//                     //     GLOBALS.SelectedPalette!.Value,
//                     //     70
//                     // );
//                 } else {
//                     BeginTextureMode(GLOBALS.Textures.GeneralLevel);
//                     Printers.DrawTileLayer(
//                         GLOBALS.Layer,
//                         0, 
//                         20, 
//                         false, 
//                         GLOBALS.Settings.GeneralSettings.DrawTileMode,
//                         (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
//                         visibleStrays: false,
//                         materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
//                     );
//                     EndTextureMode();
//                 }
//             }

//             // then draw the props

//             if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 0) {

//                 BeginTextureMode(GLOBALS.Textures.GeneralLevel);


//                 for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                 {
//                     if (_hidden[p]) continue;
                    
//                     var current = GLOBALS.Level.Props[p];
                    
//                     // Filter based on depth
//                     if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (current.Depth < -9)) continue;

//                     var (category, index) = current.Position;
//                     var quads = current.Quad;
                    
//                     // origin must be the center
//                     // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
//                     // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
//                     Printers.DrawProp(
//                         category, 
//                         index, 
//                         current, 
//                         20, 
//                         GLOBALS.Settings.GeneralSettings.DrawPropMode, 
//                         GLOBALS.SelectedPalette,
//                         GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
//                     );

//                     // Draw Rope Point
//                     if (current.Type == InitPropType.Rope)
//                     {
//                         foreach (var point in current.Extras.RopePoints)
//                         {
//                             DrawCircleV(point, 3f, Color.White);
//                         }
//                     }
                    
//                     if (_selected[p])
//                     {
//                         // Side Lines
                        
//                         DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
//                         // Quad Points

//                         if (_stretchingProp)
//                         {
//                             if (current.Type == InitPropType.Rope)
//                             {
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopLeft, quads.BottomLeft), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 DrawCircleV(
//                                     Raymath.Vector2Divide(Raymath.Vector2Add(quads.TopRight, quads.BottomRight), 
//                                         new(2f, 2f)), 
//                                     5f, 
//                                     Color.Blue
//                                 );
                                
//                                 /*DrawCircleV(quads.TopLeft, 2f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 2f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 2f, Color.Blue);*/
//                             }
//                             else if (current.Type == InitPropType.Long)
//                             {
//                                 var sides = Utils.LongSides(current.Quad);
                                
//                                 DrawCircleV(sides.left, 5f, Color.Blue);
//                                 DrawCircleV(sides.top, 5f, Color.Blue);
//                                 DrawCircleV(sides.right, 5f, Color.Blue);
//                                 DrawCircleV(sides.bottom, 5f, Color.Blue);
//                             }
//                             else
//                             {
//                                 DrawCircleV(quads.TopLeft, 5f, Color.Blue);
//                                 DrawCircleV(quads.TopRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomRight, 5f, Color.Blue);
//                                 DrawCircleV(quads.BottomLeft, 5f, Color.Blue);
//                             }
//                         }
//                         else if (_scalingProps)
//                         {
//                             var center = Utils.QuadsCenter(ref quads);
                            
//                             switch (_stretchAxes)
//                             {
//                                 case 1:
//                                     DrawLineEx(
//                                         center with { X = -10 }, 
//                                         center with { X = GLOBALS.Level.Width*20 + 10 }, 
//                                         2f, 
//                                         Color.Red
//                                     );
//                                     break;
//                                 case 2:
//                                     DrawLineEx(
//                                         center with { Y = -10 },
//                                         center with { Y = GLOBALS.Level.Height*20 + 10 },
//                                         2f,
//                                         Color.Green
//                                     );
//                                     break;
//                             }
//                         }
                        
//                         // Draw Rope Point
//                         if (current.Type == InitPropType.Rope)
//                         {
//                             if (_editingPropPoints)
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 3f, Color.Red);
//                                 }
//                             }
//                             else
//                             {
//                                 foreach (var point in current.Extras.RopePoints)
//                                 {
//                                     DrawCircleV(point, 2f, Color.Orange);
//                                 }
//                             }

//                             var p1 = p;
//                             var model = _models.SingleOrDefault(r => r.index == p1);

//                             if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
//                             {
//                                 foreach (var handle in model.model.BezierHandles) {
//                                     DrawCircleV(handle, 6f, Color.White);
//                                     DrawCircleV(handle, 4f, Color.Green);
//                                 }
//                             }
//                         }
//                     }
//                 }

//                 EndTextureMode();
//             }
        
//         }
//         if (GLOBALS.Settings.GeneralSettings.Water && GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1) {
//             BeginTextureMode(GLOBALS.Textures.GeneralLevel);
//             DrawRectangle(
//                 (-1) * 20,
//                 (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 20,
//                 (GLOBALS.Level.Width + 2) * 20,
//                 (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 20,
//                 new Color(0, 0, 255, (int)GLOBALS.Settings.GeneralSettings.WaterOpacity)
//             );
//             EndTextureMode();
//         }
//         #endregion
//     }
    
//     private bool _showLayer1Tiles = true;
//     private bool _showLayer2Tiles = true;
//     private bool _showLayer3Tiles = true;

//     private bool _showTileLayer1 = true;
//     private bool _showTileLayer2 = true;
//     private bool _showTileLayer3 = true;

//     private int _menuRootCategoryIndex;
    
//     private int _propsMenuTilesCategoryIndex;
//     private int _propsMenuTilesIndex;

//     private int _propsMenuRopesIndex;

//     private int _propsMenuLongsIndex;

//     private int _propsMenuOthersCategoryIndex;
//     private int _propsMenuOthersIndex;

//     private int _snapMode;

//     private int _quadLock;
//     private int _pointLock = -1;
//     private int _bezierHandleLock = -1;

//     private bool _clickTracker;

//     private int _mode;

//     private bool _movingProps;
//     private bool _rotatingProps;
//     private bool _scalingProps;
//     private bool _stretchingProp;
//     private bool _editingPropPoints;
//     // private bool _ropeMode;

//     // private bool _ropeModelGravity = true;

//     private bool _noCollisionPropPlacement;

//     private byte _stretchAxes;

//     // 0 - 360
//     private int _placementRotation;

//     private bool _vFlipPlacement;
//     private bool _hFlipPlacement;

//     private int _placementRotationSteps = 1;

//     private int _ropeSimulationFrameCut = 1;
//     private int _ropeSimulationFrame;

//     //
//     private string _tileAsPropSearchText = "";

//     private int _tileAsPropSearchCategoryIndex = -1;
//     private int _tileAsPropSearchIndex = -1;
//     private record TileAsPropSearchResult(
//         (string name, int originalIndex)[] Categories,
//         (TileDefinition tile, int originalIndex)[][] Tiles
//     );

//     private TileAsPropSearchResult? _tileAsPropSearchResult = null;

//     private bool _isTileSearchActive; 
//     private bool _isPropSearchActive; 

//     private void SearchTiles() {
//         if (GLOBALS.TileDex is null || string.IsNullOrEmpty(_tileAsPropSearchText)) {
//             _tileAsPropSearchResult = null;
//             _tileAsPropSearchCategoryIndex = -1;
//             return;
//         }

//         var dex = GLOBALS.TileDex;

//         List<(string, int)> categories = [];
//         List<(TileDefinition, int)[]> tiles = [];

//         for (var c = 0; c < dex.OrderedTilesAsProps.Length; c++) {
//             List<(TileDefinition, int)> foundTiles = [];

//             for (var t = 0; t < dex.OrderedTilesAsProps[c].Length; t++) {
//                 var tile = dex.OrderedTilesAsProps[c][t];

//                 if (tile.Name.Contains(_tileAsPropSearchText, StringComparison.InvariantCultureIgnoreCase)) {
//                     foundTiles.Add((tile, t));
//                 }
//             }

//             if (foundTiles is not []) {
//                 categories.Add((dex.OrderedTileAsPropCategories[c], c));
//                 tiles.Add([..foundTiles]);
//             }
//         }

//         _tileAsPropSearchResult = new([..categories], [..tiles]);

//         if (categories.Count > 0) {
//             _tileAsPropSearchCategoryIndex = 0;
//         } else {
//             _tileAsPropSearchCategoryIndex = -1;
//         }
//     }
//     //

//     //
//     private string _propSearchText = "";

//     private int _propSearchCategoryIndex = -1;
//     private int _propSearchIndex = -1;
//     private record PropSearchResult(
//         (string name, int originalIndex, Color color)[] Categories,
//         (InitPropBase prop, int originalIndex)[][] Props
//     );

//     private PropSearchResult? _propSearchResult = null;

//     private void SearchProps() {
//         if (GLOBALS.PropCategories is null or { Length: 0 } || GLOBALS.Props is null or { Length: 0 } || string.IsNullOrEmpty(_propSearchText)) {
//             _propSearchResult = null;
//             _propSearchCategoryIndex = -1;
//             return;
//         }

//         List<(string, int, Color)> categories = [];
//         List<(InitPropBase, int)[]> props = [];

//         for (var c = 0; c < GLOBALS.Props.Length; c++) {
//             List<(InitPropBase, int)> foundProps = [];
            
//             for (var p = 0; p < GLOBALS.Props[c].Length; p++) {
//                 var prop = GLOBALS.Props[c][p];

//                 if (prop.Name.Contains(_propSearchText, StringComparison.InvariantCultureIgnoreCase)) {
//                     foundProps.Add((prop, p));
//                 }
//             }

//             if (foundProps is not []) {
//                 categories.Add((GLOBALS.PropCategories[c].Item1, c, GLOBALS.PropCategories[c].Item2));
//                 props.Add([..foundProps]);
//             }
//         }

//         _propSearchResult = new([..categories], [..props]);

//         if (categories.Count > 0) {
//             _propSearchCategoryIndex = 0;
//         } else {
//             _propSearchCategoryIndex = -1;
//         }
//     }
//     //
    
//     private Vector2 _selection1 = new(-100, -100);
//     private Rectangle _selection;
//     private Rectangle _selectedPropsEncloser;
//     private Vector2 _selectedPropsCenter;

//     private bool[] _selected = [];
//     private bool[] _hidden = [];

//     private int[] _selectedCycleIndices = [];
//     private int _selectedCycleCursor;

//     private readonly (string name, Color color)[] _propCategoriesOnly = GLOBALS.PropCategories[..^2]; // a risky move..

//     private TileDefinition? _currentTile;
//     private TileDefinition? _hoveredTile;

//     private readonly string[] _ropeNames = [..GLOBALS.RopeProps.Select(p => p.Name)];
//     private readonly string[] _longNames = [..GLOBALS.LongProps.Select(l => l.Name)];
//     private readonly string[] _otherCategoryNames;
//     private readonly string[][] _otherNames;

//     private BasicPropSettings _copiedPropSettings = new();
//     private Vector2[] _copiedRopePoints = [];
//     private int _copiedDepth;
//     private bool _copiedIsTileAsProp;
//     private bool _newlyCopied; // to signify that the copied properties should be used

//     private bool _showGrid;

//     private int _defaultDepth;
//     private int _defaultVariation;
//     private int _defaultSeed;
//     private Vector2 _defaultStretch = Vector2.Zero;

//     private void UpdateDefaultDepth()
//     {
//         _defaultDepth = -GLOBALS.Layer * 10;
//     }

//     private Vector2? _propsMoveMouseAnchor;
//     private Vector2? _propsMoveMousePos;
//     private Vector2 _propsMoveMouseDelta = new(0, 0);
    
//     private (int index, bool simulate, RopeModel model)[] _models = [];
    
//     private bool _isShortcutsWinHovered;
//     private bool _isShortcutsWinDragged;
    
//     private bool _isPropsWinHovered;
//     private bool _isPropsWinDragged;

//     private bool _isNavbarHovered;

//     private bool _isPropsListHovered;

//     private PropGram _gram = new(30);

//     public PropsEditorPage()
//     {
//         _camera = new Camera2D { Zoom = 0.8f };

//         _otherCategoryNames = [..from c in _propCategoriesOnly select c.name];
//         _otherNames = GLOBALS.Props.Select(c => c.Select(p => p.Name).ToArray()).ToArray();

//         _gram.Proceed(GLOBALS.Level.Props);
//     }

//     private void Undo() {
//         if (_gram.CurrentAction is null) return;

//         _gram.Undo();

//         GLOBALS.Level.Props = [.._gram.CurrentAction]; 

//         ImportRopeModels();

//         _selected = new bool[GLOBALS.Level.Props.Length];
//         _hidden = new bool[GLOBALS.Level.Props.Length];

//         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//         else {
//             _shouldRedrawLevel = true;
//         }
//     }

//     private void Redo() {
//         _gram.Redo();

//         if (_gram.CurrentAction is null) return;

//         GLOBALS.Level.Props = [.._gram.CurrentAction];

//         ImportRopeModels();

//         _selected = new bool[GLOBALS.Level.Props.Length];
//         _hidden = new bool[GLOBALS.Level.Props.Length];

//         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//         else {
//             _shouldRedrawLevel = true;
//         }
//     }

//     public void OnGlobalResourcesUpdated()
//     {
//         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//         else {
//             _shouldRedrawLevel = true;
//         }
//     }

//     public void OnProjectLoaded(object? sender, EventArgs e)
//     {
//         var lWidth = GLOBALS.Level.Width * 20;
//         var lHeight = GLOBALS.Level.Height * 20;

//         ImportRopeModels();
//         _selected = new bool[GLOBALS.Level.Props.Length];

//         if (lWidth != _propLayerRT.Raw.Texture.Width || lHeight != _propLayerRT.Raw.Texture.Height) {
//             _propLayerRT.Dispose();
//             _propLayerRT = new RenderTexture2D(lWidth, lHeight);
//         }


//         if (lWidth != _tempGeoL.Raw.Texture.Width || lHeight != _tempGeoL.Raw.Texture.Height) {
//             _tempGeoL.Dispose();
//             _tempGeoL = new(lWidth, lHeight);
//         }
//     }

//     public void OnProjectCreated(object? sender, EventArgs e)
//     {
//         var lWidth = GLOBALS.Level.Width * 20;
//         var lHeight = GLOBALS.Level.Height * 20;

//         ImportRopeModels();
//         _selected = new bool[GLOBALS.Level.Props.Length];

//         if (lWidth != _propLayerRT.Raw.Texture.Width || lHeight != _propLayerRT.Raw.Texture.Height) {
//             _propLayerRT.Dispose();
//             _propLayerRT = new RenderTexture2D(lWidth, lHeight);

//         }


//         if (lWidth != _tempGeoL.Raw.Texture.Width || lHeight != _tempGeoL.Raw.Texture.Height) {
//             _tempGeoL.Dispose();
//             _tempGeoL = new(lWidth, lHeight);
//         }
//     }

//     public void OnPageUpdated(int previous, int @next) {
//         if (@next == 8) {
//             _shouldRedrawLevel = true;
//             _shouldRedrawPropLayer = true;
//             _defaultDepth = -10 * GLOBALS.Layer;
//         }
//     }

//     private void ImportRopeModels()
//     {
//         if (GLOBALS.Level.Props.Length == 0) {
//             _models = [];
//             return;
//         }

//         var ropes = GLOBALS.Level.Props
//             .Select((rope, index) => (rope, index))
//             .Where(ropeInfo => ropeInfo.rope.Type == InitPropType.Rope);

//         if (!ropes.Any()) {
//             _models = [];
//             return;
//         }

//         if (_models.Length == 0) {
//             List<(int, bool, RopeModel)> models = [];
        
//             for (var r = 0; r < GLOBALS.Level.Props.Length; r++)
//             {
//                 var current = GLOBALS.Level.Props[r];
                
//                 if (current.Type != InitPropType.Rope) continue;
                
//                 var newModel = new RopeModel(current, GLOBALS.RopeProps[current.Position.index]);
            
//                 // var quad = current.prop.Quads;
//                 // var quadCenter = Utils.QuadsCenter(ref quad);

//                 models.Add((r, false, newModel));
//             }

//             _models = [..models];
//         } else {
//             var models = ropes.Select(ropeInfo => {
//                 var foundModel = _models.SingleOrDefault(m => m.index == ropeInfo.index);

//                 if (foundModel.model != null) return foundModel;

//                 var newModel = new RopeModel(ropeInfo.rope, GLOBALS.RopeProps[ropeInfo.rope.Position.index]);

//                 // var quad = ropeInfo.rope.prop.Quads;
//                 // var quadCenter = Utils.QuadsCenter(ref quad);

//                 return (ropeInfo.index, false, newModel);
//             });

//             _models = [..models];
//         }
//     }

//     private void UpdateRopeModelSegments()
//     {
//         List<(int, bool, RopeModel)> models = [];
        
//         for (var r = 0; r < _models.Length; r++)
//         {
//             var current = _models[r];
            
//             models.Add((
//                 current.index, 
//                 false, 
//                 new RopeModel(
//                     GLOBALS.Level.Props[current.index], 
//                     GLOBALS.RopeProps[GLOBALS.Level.Props[current.index].Position.index])
//                     {
//                         BezierHandles = current.model.BezierHandles
//                     }
//             ));
//         }

//         _models = [..models];
//     }

//     private void DecrementMenuIndex()
//     {
//         switch (_menuRootCategoryIndex)
//         {
//             case 0: // tiles as props
//                 if (GLOBALS.TileDex is null) return;
                
//                 _propsMenuTilesIndex--;
                
//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
//                 else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
//                 _currentTile =
//                     GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
//                 break;
                    
//             case 1: // Ropes
//                 _propsMenuRopesIndex--;
                
//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
//                 else Utils.Restrict(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
//                 break;
            
//             case 2: // Longs 
//                 _propsMenuLongsIndex--;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
//                 else Utils.Restrict(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
//                 break;
                    
//             case 3: // props
//                 _propsMenuOthersIndex--;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 break;
//         }
//     }

//     private void ToNextInnerCategory()
//     {
//         switch (_menuRootCategoryIndex)
//         {
//             case 0: // Tiles-As-Props
//                 if (GLOBALS.TileDex is null) return;

//                 _propsMenuTilesCategoryIndex++;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
//                 else Utils.Restrict(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
                
//                 if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuTilesIndex = 0;
//                 else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
//                 _currentTile =
//                     GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
//                 break;
            
//             case 3: // Other
//                 _propsMenuOthersCategoryIndex++;
                
//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);
//                 else Utils.Restrict(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);;
                
//                 if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuOthersIndex = 0;
//                 else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 break;
//         }
//     }

//     private void ToPreviousInnerCategory()
//     {
//         switch (_menuRootCategoryIndex)
//         {
//             case 0: // Tiles-As-Props
//                 if (GLOBALS.TileDex is null) return;
//                 _propsMenuTilesCategoryIndex--;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);
//                 else Utils.Restrict(ref _propsMenuTilesCategoryIndex, 0, GLOBALS.TileDex.OrderedTileAsPropCategories.Length - 1);

//                 if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuTilesIndex = 0;
//                 else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);

//                 _currentTile =
//                     GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
//                 break;
            
//             case 3: // Other
//                 _propsMenuOthersCategoryIndex--;
                
//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);
//                 else Utils.Restrict(ref _propsMenuOthersCategoryIndex, 0, GLOBALS.Props.Length - 1);;
                
//                 if (GLOBALS.Settings.GeneralSettings.ChangingCategoriesResetsIndex) _propsMenuOthersIndex = 0;
//                 else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 break;
//         }
//     }

//     private void IncrementMenuIndex()
//     {
//         switch (_menuRootCategoryIndex)
//         {
//             case 0: // Tiles as props
//                 if (GLOBALS.TileDex is null) return;
//                 _propsMenuTilesIndex++;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
//                 else Utils.Restrict(ref _propsMenuTilesIndex, 0, GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length - 1);
                
//                 _currentTile =
//                     GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
//                 break;
            
//             case 1: // Ropes
//                 _propsMenuRopesIndex++;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
//                 else Utils.Restrict(ref _propsMenuRopesIndex, 0, GLOBALS.RopeProps.Length - 1);
//                 break;
            
//             case 2: // Longs 
//                 _propsMenuLongsIndex++;

//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
//                 else Utils.Restrict(ref _propsMenuLongsIndex, 0, GLOBALS.LongProps.Length - 1);
//                 break;
            
//             case 3: // Props
//                 _propsMenuOthersIndex++;
                
//                 if (GLOBALS.Settings.GeneralSettings.CycleMenus) Utils.Cycle(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 else Utils.Restrict(ref _propsMenuOthersIndex, 0, GLOBALS.Props[_propsMenuOthersCategoryIndex].Length - 1);
//                 break;
//         }
//     }

//     private Rectangle GetLayerIndicator(int layer) => layer switch {
//         0 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
//             PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 40,                        50 + 25,                     40, 40 ), 
//             PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 70,     50 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 70,     GetScreenHeight() - 90 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 40,                        GetScreenHeight() - 90 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 50 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 90 - 25, 40, 40 ),

//             _ => new Rectangle(40, GetScreenHeight() - 80, 40, 40)
//         },
//         1 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
//             PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 30,                        40 + 25,                     40, 40 ), 
//             PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 60,     40 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 60,     GetScreenHeight() - 80 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 30,                        GetScreenHeight() - 80 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 40 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 80 - 25, 40, 40 ),

//             _ => new Rectangle(30, GetScreenHeight() - 70, 40, 40)
//         },
//         2 => GLOBALS.Settings.PropEditor.LayerIndicatorPosition switch { 
//             PropEditor.ScreenRelativePosition.TopLeft       => new Rectangle( 20,                        30 + 25,                     40, 40 ), 
//             PropEditor.ScreenRelativePosition.TopRight      => new Rectangle( GetScreenWidth() - 50,     30 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomRight   => new Rectangle( GetScreenWidth() - 50,     GetScreenHeight() - 70 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.BottomLeft    => new Rectangle( 20,                        GetScreenHeight() - 70 - 25, 40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleTop     => new Rectangle((GetScreenWidth() - 40)/2f, 30 + 25,                     40, 40 ),
//             PropEditor.ScreenRelativePosition.MiddleBottom  => new Rectangle((GetScreenWidth() - 40)/2f, GetScreenHeight() - 70 - 25, 40, 40 ),

//             _ => new Rectangle(20, GetScreenHeight() - 60, 40, 40)
//         },
        
//         _ => new Rectangle(0, 0, 40, 40)
//     };

//     public override void Draw()
//     {
//         if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

//         _currentTile ??= GLOBALS.TileDex?.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
        
//         var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
//         var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
//         var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);
            
//         if (_selected.Length != GLOBALS.Level.Props.Length)
//         {
//             _selected = new bool[GLOBALS.Level.Props.Length];
//         }

//         if (_hidden.Length != GLOBALS.Level.Props.Length)
//         {
//             _hidden = new bool[GLOBALS.Level.Props.Length];
//         }

//         var previewScale = 20;

//         var sWidth = GetScreenWidth();
//         var sHeight = GetScreenHeight();
        
//         var layer3Rect = GetLayerIndicator(2);
//         var layer2Rect = GetLayerIndicator(1);
//         var layer1Rect = GetLayerIndicator(0);

//         var tileMouse = GetMousePosition();
//         var tileMouseWorld = GetScreenToWorld2D(tileMouse, _camera);

//         var menuPanelRect = new Rectangle(sWidth - 360, 0, 360, sHeight);

//         //                        v this was done to avoid rounding errors
//         var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / 20;
//         var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / 20;

//         var canDrawTile = !_isPropsListHovered && !_isNavbarHovered && 
//                         !_isPropsWinHovered && 
//                           !_isPropsWinDragged && 
//                           !_isShortcutsWinHovered && 
//                           !_isShortcutsWinDragged && 
//                           !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
//                           !CheckCollisionPointRec(tileMouse, layer3Rect) &&
//                           (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
//                           (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));

//         var isSearchBusy = _isTileSearchActive || _isPropSearchActive;

//         var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

//         GLOBALS.LockNavigation = _isTileSearchActive || _isPropSearchActive;

//         // Move level with keyboard

//         if (_shortcuts.MoveViewLeft.Check(ctrl, shift, alt)) {
//             _camera.Target.X -= GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.MoveViewTop.Check(ctrl, shift, alt)) {
//             _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.MoveViewRight.Check(ctrl, shift, alt)) {
//             _camera.Target.X += GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } if (_shortcuts.MoveViewBottom.Check(ctrl, shift, alt)) {
//             _camera.Target.Y += GLOBALS.Settings.GeneralSettings.KeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         }

//         else if (_shortcuts.FastMoveViewLeft.Check(ctrl, shift, alt)) {
//             _camera.Target.X -= GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.FastMoveViewTop.Check(ctrl, shift, alt)) {
//             _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.FastMoveViewRight.Check(ctrl, shift, alt)) {
//             _camera.Target.X += GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } if (_shortcuts.FastMoveViewBottom.Check(ctrl, shift, alt)) {
//             _camera.Target.Y += GLOBALS.Settings.GeneralSettings.FastKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         }

//         else if (_shortcuts.ReallyFastMoveViewLeft.Check(ctrl, shift, alt)) {
//             _camera.Target.X -= GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20) - 120) _camera.Target.X = (GLOBALS.Level.Width * 20) - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20) - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.ReallyFastMoveViewTop.Check(ctrl, shift, alt)) {
//             _camera.Target.Y -= GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } else if (_shortcuts.ReallyFastMoveViewRight.Check(ctrl, shift, alt)) {
//             _camera.Target.X += GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         } if (_shortcuts.ReallyFastMoveViewBottom.Check(ctrl, shift, alt)) {
//             _camera.Target.Y += GLOBALS.Settings.GeneralSettings.ReallyKeyboardMovementSteps * 20;

//             if (_camera.Target.X < -80) _camera.Target.X = -80;
//             if (_camera.Target.X > (GLOBALS.Level.Width * 20)  - 120) _camera.Target.X = (GLOBALS.Level.Width * 20)  - 120;
            
//             if (_camera.Target.Y < -80) _camera.Target.Y = -80;
//             if (_camera.Target.Y > (GLOBALS.Level.Height * 20)  - 120) _camera.Target.Y = (GLOBALS.Level.Height * 20)  - 120;
//         }

//         // Undo

//         if (!isSearchBusy && _shortcuts.Undo.Check(ctrl, shift, alt)) {
//             Undo();
//         }
        
//         // Redo

//         if (!isSearchBusy && _shortcuts.Redo.Check(ctrl, shift, alt)) {
//             Redo();
//         }

//         // handle mouse drag
//         if (_shortcuts.DragLevel.Check(ctrl, shift, alt, true) || _shortcuts.DragLevelAlt.Check(ctrl, shift, alt, true))
//         {
//             var delta = GetMouseDelta();
//             delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
//             _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
//         }

//         // handle zoom
//         var tileWheel = GetMouseWheelMove();
//         if (tileWheel != 0 && canDrawTile)
//         {
//             var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
//             _camera.Offset = GetMousePosition();
//             _camera.Target = mouseWorldPosition;
//             _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
//             if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
//         }
        
//         // Cycle layer
//         if (!isSearchBusy && _shortcuts.CycleLayers.Check(ctrl, shift, alt))
//         {
//             GLOBALS.Layer++;

//             if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;

//             UpdateDefaultDepth();

//             _shouldRedrawLevel = true;
//             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//         }

//         if (!isSearchBusy && _shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) {
//             _showLayer1Tiles = !_showLayer1Tiles;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }
//         if (!isSearchBusy && _shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) {
//             _showLayer2Tiles = !_showLayer2Tiles;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }
//         if (!isSearchBusy && _shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) {
//             _showLayer3Tiles = !_showLayer3Tiles;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }
        
//         // Cycle Mode
//         if (!isSearchBusy && _shortcuts.CycleModeRight.Check(ctrl, shift, alt))
//         {
//             _mode = ++_mode % 2;
//         }
//         else if (!isSearchBusy && _shortcuts.CycleModeLeft.Check(ctrl, shift, alt))
//         {
//             _mode--;
//             if (_mode < 0) _mode = 1;
//         }
        
//         if (!isSearchBusy && _shortcuts.ToggleLayer1.Check(ctrl, shift, alt) && !_scalingProps) {
//             _showTileLayer1 = !_showTileLayer1;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }
//         if (!isSearchBusy && _shortcuts.ToggleLayer2.Check(ctrl, shift, alt) && !_scalingProps) {
//             _showTileLayer2 = !_showTileLayer2;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }
//         if (!isSearchBusy && _shortcuts.ToggleLayer3.Check(ctrl, shift, alt) && !_scalingProps) {
//             _showTileLayer3 = !_showTileLayer3;
//             _shouldRedrawLevel = true;
//             // _shouldRedrawPropLayer = true;
//         }

//         if (!isSearchBusy && _shortcuts.CycleSnapMode.Check(ctrl, shift, alt)) _snapMode = ++_snapMode % 3;

//         #region Placement & Selection Shortcuts
//         // Mode-based hotkeys
//         switch (_mode)
//         {
//             case 1: // Place Mode
//                 if (!canDrawTile) break;

//                 if (_shortcuts.CycleVariations.Check(ctrl, shift, alt)) {
//                     if (_menuRootCategoryIndex == 3 && GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex] is IVariableInit v) _defaultVariation = (_defaultVariation + 1) % v.Variations;
//                     else _defaultVariation = 0;
//                 }

//                 if (_ropeInitialPlacement) {
//                     var (index, _, model) = _models.LastOrDefault();
                    
//                     if (model is not null && index < GLOBALS.Level.Props.Length) {
//                         if (_shortcuts.ToggleRopeGravity.Check(ctrl, shift, alt)) {
//                             model.Gravity = !model.Gravity;
//                         }

//                         if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
//                             _additionalInitialRopeSegments++;
//                         }

//                         if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
//                             _additionalInitialRopeSegments--;
//                             // Utils.Restrict(ref _additionalInitialRopeSegments, 0);
//                         }

//                         var current = GLOBALS.Level.Props[index];

//                         var middleLeft = Raymath.Vector2Divide(
//                             Raymath.Vector2Add(current.Quad.TopLeft, current.Quad.BottomLeft),
//                             new(2f, 2f)
//                         );

//                         var middleRight = Raymath.Vector2Divide(
//                             Raymath.Vector2Add(current.Quad.TopRight, current.Quad.BottomRight),
//                             new(2f, 2f)
//                         );
                        
//                         // Attach vertex
//                         {
//                             var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
//                             var r = Raymath.Vector2Length(Raymath.Vector2Subtract(current.Quad.TopLeft, middleLeft));
                        
//                             var currentQuads = current.Quad;

//                             currentQuads.BottomLeft = Raymath.Vector2Add(
//                                 middleLeft, 
//                                 new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                             );
                            
//                             currentQuads.TopLeft = Raymath.Vector2Add(
//                                 middleLeft, 
//                                 new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                             );
                            
//                             currentQuads.BottomRight = Raymath.Vector2Add(
//                                 tileMouseWorld, 
//                                 new Vector2(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                             );
                            
//                             currentQuads.TopRight = Raymath.Vector2Add(
//                                 tileMouseWorld, 
//                                 new Vector2(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                             );

//                             current.Quad = currentQuads;
//                         }

//                         // Adjust segment count
//                         var init = GLOBALS.Ropes[current.Position.index];
//                         var endsDistance = (int)Raymath.Vector2Distance(middleLeft, middleRight) / (10 + init.SegmentLength);
//                         if (endsDistance > 0 && ++_ropeSimulationFrame % 6 == 0) {
//                             var ropePoints = current.Extras.RopePoints;
//                             var targetCount = endsDistance + _additionalInitialRopeSegments;

//                             Utils.Restrict(ref targetCount, 3);

//                             var deficit = targetCount - ropePoints.Length;
                            
//                             if (targetCount > ropePoints.Length) {
//                                 ropePoints = ([..ropePoints, ..Enumerable.Range(0, deficit).Select((i) => tileMouseWorld)]);
//                             } else if (targetCount < ropePoints.Length) {
//                                 ropePoints = ropePoints.Take(targetCount).ToArray();
//                             }

//                             // foundProp.Extras.RopePoints = ropePoints;
//                             model.UpdateSegments(ropePoints);
//                         }

//                         model?.Update(current.Quad, current.Depth switch
//                         {
//                             < -19 => 2,
//                             < -9 => 1,
//                             _ => 0
//                         });

//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }
//                     }
//                 }

//                 if (_longInitialPlacement && GLOBALS.Level.Props is { Length: > 0 }) {
//                     var currentQuads = GLOBALS.Level.Props[^1].Quad;
//                     var (left, top, right, bottom) = Utils.LongSides(currentQuads);
                            
//                     var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                    
//                     var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, left));

//                     currentQuads.BottomLeft = Raymath.Vector2Add(
//                         left, 
//                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                     );
                
//                     currentQuads.TopLeft = Raymath.Vector2Add(
//                         left, 
//                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                     );
                
//                     currentQuads.BottomRight = Raymath.Vector2Add(
//                         tileMouseWorld, 
//                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                     );
                
//                     currentQuads.TopRight = Raymath.Vector2Add(
//                         tileMouseWorld, 
//                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                     );

//                     GLOBALS.Level.Props[^1].Quad = currentQuads;

//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }
//                 }

//                 // Place Prop
//                 if (_noCollisionPropPlacement)
//                 {
//                     if (!_lockedPlacement && canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt, true) ||
//                                         _shortcuts.PlacePropAlt.Check(ctrl, shift, alt, true)))
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }
                        
//                         var posV = _snapMode switch
//                         {
//                             1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
//                             2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
//                             _ => tileMouseWorld
//                         };

//                         switch (_menuRootCategoryIndex)
//                         {
//                             case 0: // Tiles as props
//                             {
//                                 if (_currentTile is null) break;
                                
//                                 // var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
//                                 // var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;

//                                 var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
//                                 var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;
                                
//                                 BasicPropSettings settings;

//                                 if (_newlyCopied)
//                                 {
//                                     _newlyCopied = false;

//                                     settings = _copiedPropSettings;
//                                     _defaultDepth = _copiedDepth;
//                                 }
//                                 else
//                                 {
//                                     settings = new();
//                                 }

//                                 var placementQuad = new Data.Quad(
//                                     new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
//                                     new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
//                                 );

//                                 if (_vFlipPlacement) Utils.VFlipQuad(ref placementQuad);
//                                 if (_hFlipPlacement) Utils.HFlipQuad(ref placementQuad);

//                                 foreach (var prop in GLOBALS.Level.Props)
//                                 {
//                                     var propRec = Utils.EncloseQuads(prop.Quad);
//                                     var newPropRec = Utils.EncloseQuads(placementQuad);
                                    
//                                     if (prop.Depth == _defaultDepth && CheckCollisionRecs(newPropRec, propRec)) goto skipPlacement;
//                                 }

//                                 var rotatedQuad = Utils.RotatePropQuads(placementQuad, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
//                                 GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
//                                         new Prop(
//                                             _defaultDepth, 
//                                             _currentTile.Name, 
//                                             true, 
//                                             rotatedQuad
//                                         )
//                                         {
//                                             Extras = new PropExtras(settings, []),
//                                             Rotation = _placementRotation * _placementRotationSteps,
//                                             OriginalQuad = placementQuad,
//                                             Type = InitPropType.Tile,
//                                             Tile = _currentTile,
//                                             Position = (-1, -1)
//                                         }
//                                 ];
//                             }
//                                 break;

//                             case 1: // Ropes
//                             {
//                                 if (_clickTracker) break;
//                                 _clickTracker = true;

//                                 if (_ropeInitialPlacement) {
//                                     _ropeInitialPlacement = false;

//                                 } else {
//                                     _ropeInitialPlacement = true;
//                                     _additionalInitialRopeSegments = 0;
                                    
//                                     var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
//                                     var newQuads = new Data.Quad
//                                     {
//                                         TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y),
//                                         BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y),
//                                         TopRight = new(tileMouseWorld.X, tileMouseWorld.Y),
//                                         BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y)
//                                     };

//                                     var ropeEnds = Utils.RopeEnds(newQuads);
                                    
//                                     PropRopeSettings settings;
//                                     Vector2[] ropePoints;

//                                     if (_newlyCopied)
//                                     {
//                                         _newlyCopied = false;

//                                         ropePoints = _copiedRopePoints;
//                                         settings = (PropRopeSettings)_copiedPropSettings;
//                                         _defaultDepth = _copiedDepth;
//                                     }
//                                     else
//                                     {
//                                         // ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
//                                         ropePoints = [tileMouseWorld];
//                                         settings = new(thickness: current.Name is "Wire" or "Zero-G Wire" ? 2 : null);
//                                     }

                                        
//                                     GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
//                                             new Prop(
//                                                 _defaultDepth, 
//                                                 current.Name, 
//                                                 false, 
//                                                 newQuads
//                                             )
//                                             {
//                                                 Extras = new PropExtras(
//                                                     settings, 
//                                                     ropePoints
//                                                 ),
//                                                 Type = InitPropType.Rope,
//                                                 Tile = null,
//                                                 Position = (-1, _propsMenuRopesIndex)
//                                             }
//                                     ];

//                                     _selected = new bool[GLOBALS.Level.Props.Length];
//                                     _selected[^1] = true;

//                                     ImportRopeModels();
//                                 }
//                             }
//                                 break;

//                             case 2: // Long Props
//                             {
//                                 if (_clickTracker) break;
//                                 _clickTracker = true;

//                                 if (_longInitialPlacement) {
//                                     _longInitialPlacement = false;
//                                 } else {
//                                     _longInitialPlacement = true;

//                                     var current = GLOBALS.LongProps[_propsMenuLongsIndex];
//                                     ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
//                                     var height = texture.Height / 2f;
//                                     var newQuads = new Data.Quad
//                                     {
//                                         TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
//                                         TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
//                                     };

//                                     if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
//                                     if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);
                                    
//                                     PropLongSettings settings;

//                                     if (_newlyCopied)
//                                     {
//                                         _newlyCopied = false;

//                                         settings = (PropLongSettings)_copiedPropSettings;
//                                         _defaultDepth = _copiedDepth;
//                                     }
//                                     else
//                                     {
//                                         settings = new();
//                                     }
                                    
//                                     GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
//                                             new Prop(
//                                                 _defaultDepth, 
//                                                 current.Name, 
//                                                 false, 
//                                                 newQuads
//                                             )
//                                             {
//                                                 Extras = new PropExtras(settings, []),
//                                                 Type = InitPropType.Long,
//                                                 Tile = null,
//                                                 Position = (-1, _propsMenuLongsIndex)
//                                             }
//                                     ];

//                                     _selected = new bool[GLOBALS.Level.Props.Length];
//                                     _selected[^1] = true;
//                                 }
//                             }
//                                 break;

//                             case 3: // Others
//                             {
//                                 var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
//                                 var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                
//                                 var (width, height, settings) = init switch
//                                 {
//                                     InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
//                                     InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
//                                     InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
//                                     InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
//                                     InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
//                                     InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
//                                     InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
//                                     InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
//                                     InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
//                                     InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
//                                     _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
//                                 };

//                                 if (settings is PropVariedSoftSettings vs && init is InitVariedSoftProp i && i.Colorize == 1) {
//                                     vs.ApplyColor = 1;
//                                 }

//                                 if (settings is ICustomDepth cd) {
//                                     cd.CustomDepth = init switch
//                                     {
//                                         InitVariedStandardProp v => v.Repeat.Length,
//                                         InitStandardProp s => s.Repeat.Length,
//                                         _ => init.Depth
//                                     };
//                                 }

//                                 if (_newlyCopied)
//                                 {
//                                     _newlyCopied = false;
                                
//                                     settings = _copiedPropSettings;
//                                     _defaultDepth = _copiedDepth;
//                                 }

//                                 var newQuads = new Data.Quad(
//                                     new(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
//                                     new(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
//                                     new(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y), 
//                                     new(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
//                                 );

//                                 if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
//                                 if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);

//                                 var rotatedQuad = Utils.RotatePropQuads(newQuads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
//                                 GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
//                                         new Prop(
//                                             _defaultDepth, 
//                                             init.Name, 
//                                             false, 
//                                             rotatedQuad
//                                         )
//                                         {
//                                             Extras = new PropExtras(settings, []),
//                                             Rotation = _placementRotation * _placementRotationSteps,
//                                             OriginalQuad = newQuads,
//                                             Type = init.Type,
//                                             Tile = null,
//                                             Position = (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex)
//                                         }
//                                 ];
//                             }
//                                 break;
//                         }
                        
//                         if (GLOBALS.Level.Props.Length > 0 && 
//                             GLOBALS.Level.Props[^1].Type != InitPropType.Rope) _ropeInitialPlacement = false;

//                         // Do not forget to update _selected and _hidden

//                         _selected = [.._selected, false];
//                         _hidden = [.._hidden, false];
                        
//                         skipPlacement:
//                         {
//                         }
//                     }

//                     if (IsMouseButtonReleased(_shortcuts.PlaceProp.Button) || IsKeyReleased(_shortcuts.PlacePropAlt.Key)) {
//                         _gram.Proceed(GLOBALS.Level.Props);

//                         if (_lockedPlacement) {
//                             _lockedPlacement = false;
//                         }

//                         if (_clickTracker) {
//                             _clickTracker = false;
//                         }
//                     }
//                 }
//                 else
//                 {
//                     if (canDrawTile && (_shortcuts.PlaceProp.Check(ctrl, shift, alt) || _shortcuts.PlacePropAlt.Check(ctrl, shift, alt)))
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else _shouldRedrawLevel = true;
                        
//                         var posV = _snapMode switch
//                         {
//                             1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
//                             2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
//                             _ => tileMouseWorld
//                         };

//                         switch (_menuRootCategoryIndex)
//                         {
//                             case 0: // Tiles as props
//                             {
//                                 var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
//                                 var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;
                                
//                                 BasicPropSettings settings;

//                                 if (_newlyCopied)
//                                 {
//                                     _newlyCopied = false;

//                                     settings = _copiedPropSettings;
//                                     _defaultDepth = _copiedDepth;
//                                 }
//                                 else
//                                 {
//                                     settings = new();
//                                 }

//                                 var quads = new Data.Quad(
//                                     new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
//                                     new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
//                                 );

//                                 if (_vFlipPlacement) Utils.VFlipQuad(ref quads);
//                                 if (_hFlipPlacement) Utils.HFlipQuad(ref quads);

//                                 var rotatedQuads = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
//                                 GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    
//                                         new Prop(
//                                             _defaultDepth, 
//                                             _currentTile.Name, 
//                                             true, 
//                                             rotatedQuads
//                                         )
//                                         {
//                                             Extras = new(settings, []),
//                                             Rotation = _placementRotation * _placementRotationSteps,
//                                             OriginalQuad = quads,
//                                             Type = InitPropType.Tile,
//                                             Tile = _currentTile,
//                                             Position = (-1, -1)
//                                         }
                                    
//                                 ];
//                             }
//                                 break;

//                             case 1: // Ropes
//                             {
//                                 if (_ropeInitialPlacement) {
//                                     _ropeInitialPlacement = false;

//                                 } else {
//                                     _ropeInitialPlacement = true;
//                                     _additionalInitialRopeSegments = 0;
                                    
//                                     var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
//                                     const float height = 10f;
//                                     var newQuads = new Data.Quad
//                                     {
//                                         TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
//                                         TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
//                                     };

//                                     var ropeEnds = Utils.RopeEnds(newQuads);
                                    
//                                     PropRopeSettings settings;
//                                     Vector2[] ropePoints;

//                                     if (_newlyCopied)
//                                     {
//                                         _newlyCopied = false;

//                                         ropePoints = _copiedRopePoints;
//                                         settings = (PropRopeSettings)_copiedPropSettings;
//                                         _defaultDepth = _copiedDepth;
//                                     }
//                                     else
//                                     {
//                                         // ropePoints = Utils.GenerateRopePoints(ropeEnds.pA, ropeEnds.pB, 30);
//                                         ropePoints = [tileMouseWorld];
//                                         settings = new(thickness: current.Name is "Wire" or "Zero-G Wire" ? 2 : null);
//                                     }

                                        
//                                     GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
//                                             new Prop(
//                                                 _defaultDepth, 
//                                                 current.Name, 
//                                                 false, 
//                                                 newQuads
//                                             )
//                                             {
//                                                 Extras = new PropExtras(
//                                                     settings, 
//                                                     ropePoints
//                                                 ),
//                                                 Type = InitPropType.Rope,
//                                                 Tile = null,
//                                                 Position = (-1, _propsMenuRopesIndex)
//                                             }
//                                     ];

//                                     _selected = new bool[GLOBALS.Level.Props.Length];
//                                     _selected[^1] = true;

//                                     ImportRopeModels();
//                                 }
//                             }
//                                 break;

//                             case 2: // Long Props
//                             {
//                                 if (_longInitialPlacement) {
//                                     _longInitialPlacement = false;
//                                 } else {
//                                     _longInitialPlacement = true;

//                                     var current = GLOBALS.LongProps[_propsMenuLongsIndex];
//                                     ref var texture = ref GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
//                                     var height = texture.Height / 2f;
//                                     var newQuads = new Data.Quad
//                                     {
//                                         TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
//                                         TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
//                                         BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
//                                     };

//                                     if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
//                                     if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);
                                    
//                                     PropLongSettings settings;

//                                     if (_newlyCopied)
//                                     {
//                                         _newlyCopied = false;

//                                         settings = (PropLongSettings)_copiedPropSettings;
//                                         _defaultDepth = _copiedDepth;
//                                     }
//                                     else
//                                     {
//                                         settings = new();
//                                     }
                                    
//                                     GLOBALS.Level.Props = [..GLOBALS.Level.Props, 
//                                             new Prop(
//                                                 _defaultDepth, 
//                                                 current.Name, 
//                                                 false, 
//                                                 newQuads
//                                             )
//                                             {
//                                                 Extras = new PropExtras(settings, []),
//                                                 Rotation = _placementRotation,
//                                                 Type = InitPropType.Long,
//                                                 Tile = null,
//                                                 Position = (-1, _propsMenuLongsIndex)
//                                             }
//                                     ];

//                                     _selected = new bool[GLOBALS.Level.Props.Length];
//                                     _selected[^1] = true;
//                                 }
//                             }
//                                 break;

//                             case 3: // Others
//                             {
//                                 var init = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
//                                 var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                                
//                                 var (width, height, settings) = init switch
//                                 {
//                                     InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
//                                     InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
//                                     InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
//                                     InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
//                                     InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
//                                     InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
//                                     InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
//                                     InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
//                                     InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
//                                     InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                    
//                                     _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
//                                 };

//                                 if (settings is PropVariedSoftSettings vs && init is InitVariedSoftProp i && i.Colorize == 1) {
//                                     vs.ApplyColor = 1;
//                                 }

//                                 if (settings is ICustomDepth cd) {
//                                     cd.CustomDepth = init switch
//                                     {
//                                         InitVariedStandardProp v => v.Repeat.Length,
//                                         InitStandardProp s => s.Repeat.Length,
//                                         _ => init.Depth
//                                     };
//                                 }

//                                 if (_newlyCopied)
//                                 {
//                                     _newlyCopied = false;

//                                     settings = _copiedPropSettings;
//                                     _defaultDepth = _copiedDepth;
//                                 }

//                                 var quads = new Data.Quad(
//                                     new(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
//                                     new(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
//                                     new(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
//                                 );

//                                 if (_vFlipPlacement) Utils.VFlipQuad(ref quads);
//                                 if (_hFlipPlacement) Utils.HFlipQuad(ref quads);

//                                 var rotatedQuad = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
//                                 GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
//                                         new Prop(
//                                             _defaultDepth, 
//                                             init.Name, 
//                                             false, 
//                                             rotatedQuad)
//                                         {
//                                             Extras = new PropExtras(settings, []),
//                                             Rotation = _placementRotation * _placementRotationSteps,
//                                             OriginalQuad = quads,
//                                             Type = init.Type,
//                                             Tile = null,
//                                             Position = (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex)
//                                         }
//                                 ];
//                             }
//                                 break;
//                         }

//                         if (GLOBALS.Level.Props.Length > 0 && 
//                             GLOBALS.Level.Props[^1].Type != InitPropType.Rope) _ropeInitialPlacement = false;
                        
//                         // Do not forget to update _selected and _hidden

//                         _selected = [.._selected, false];
//                         _hidden = [.._hidden, false];

//                         _gram.Proceed(GLOBALS.Level.Props);
//                     }
//                 }
                
//                 if (_shortcuts.RotateClockwise.Check(ctrl, shift, alt, true)) {
//                     _placementRotation += 1;
//                 }

//                 if (_shortcuts.RotateCounterClockwise.Check(ctrl, shift, alt, true)) {
//                     _placementRotation -= 1;
//                 }

//                 if (_shortcuts.FastRotateClockwise.Check(ctrl, shift, alt, true)) {
//                     _placementRotation += 2;
//                 }

//                 if (_shortcuts.FastRotateCounterClockwise.Check(ctrl, shift, alt, true)) {
//                     _placementRotation -= 2;
//                 }

//                 if (_shortcuts.ResetPlacementRotation.Check(ctrl, shift, alt)) {
//                     _placementRotation = 0;
//                 }

//                 // Flipping Burgers

//                 if (_shortcuts.VerticalFlipPlacement.Check(ctrl, shift, alt)) {
//                     _vFlipPlacement = !_vFlipPlacement;
//                 }

//                 if (_shortcuts.HorizontalFlipPlacement.Check(ctrl, shift, alt)) {
//                     _hFlipPlacement = !_hFlipPlacement;
//                 }

//                 // 90 Degree Rotation

//                 if (_shortcuts.RotateRightAnglePlacement.Check(ctrl, shift, alt)) {
//                     _placementRotation += 90;

//                     Utils.Cycle(ref _placementRotation, 0, 360);
//                 }

//                 // Continuous Placement
//                 if (_shortcuts.ToggleNoCollisionPropPlacement.Check(ctrl, shift, alt)) {
//                     _noCollisionPropPlacement = !_noCollisionPropPlacement;
//                 }

//                 // Activate Selection Mode Via Mouse
//                 if (_shortcuts.SelectProps.Button != _shortcuts.PlaceProp.Button) {
//                     if (!_isPropsListHovered && !_isPropsWinHovered && (_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && !_clickTracker && canDrawTile)
//                     {
//                         _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
//                         _clickTracker = true;
//                         _mode = 0;
//                         _ropeInitialPlacement = false;
//                     }

//                     if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker && !(_isPropsWinHovered || _isPropsWinDragged))
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }
                        
//                         _clickTracker = false;

//                         List<int> selectedI = [];
                    
//                         for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
//                         {
//                             var current = GLOBALS.Level.Props[i];
//                             var propSelectRect = Utils.EncloseQuads(current.Quad);
//                             if (_shortcuts.PropSelectionModifier.Check(ctrl, shift, alt, true))
//                             {
//                                 if (CheckCollisionRecs(propSelectRect, _selection) && !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10))
//                                 {
//                                     _selected[i] = !_selected[i];
//                                 }
//                             }
//                             else
//                             {
//                                 if (CheckCollisionRecs(propSelectRect, _selection) && !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10))
//                                 {
//                                     _selected[i] = true;
//                                     selectedI.Add(i);
//                                 }
//                                 else
//                                 {
//                                     _selected[i] = false;
//                                 }
//                             }
//                         }

//                         _selectedCycleIndices = [..selectedI];
//                         _selectedCycleCursor = -1;
//                     }  
//                 }

//                 // Cycle categories
//                 if (_shortcuts.CycleCategoriesRight.Check(ctrl, shift, alt))
//                 {
//                     _menuRootCategoryIndex++;
//                     if (_menuRootCategoryIndex > 3) _menuRootCategoryIndex = 0;
//                 }
//                 else if (_shortcuts.CycleCategoriesLeft.Check(ctrl, shift, alt))
//                 {
//                     _menuRootCategoryIndex--;
//                     if (_menuRootCategoryIndex < 0) _menuRootCategoryIndex = 3;
//                 }
                
//                 // Navigate categories menu
//                 if (_shortcuts.ToNextInnerCategory.Check(ctrl, shift, alt))
//                 {
//                     ToNextInnerCategory();
//                 }
//                 else if (_shortcuts.ToPreviousInnerCategory.Check(ctrl, shift, alt))
//                 {
//                     ToPreviousInnerCategory();
//                 }
                
//                 if (_shortcuts.NavigateMenuDown.Check(ctrl, shift, alt))
//                 {
//                     IncrementMenuIndex();
//                 }
//                 else if (_shortcuts.NavigateMenuUp.Check(ctrl, shift, alt))
//                 {
//                     DecrementMenuIndex();
//                 }

//                 // Stretch Placement

//                 if (_shortcuts.StretchPlacementHorizontal.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.X += 1;
//                 } else if (_shortcuts.StretchPlacementVertical.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.Y += 1;
//                 } else if (_shortcuts.SqueezePlacementHorizontal.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.X -= 1;
//                 } else if (_shortcuts.SqueezePlacementVertical.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.Y -= 1;
//                 } else if (_shortcuts.FastStretchPlacementHorizontal.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.X += 4;
//                 } else if (_shortcuts.FastStretchPlacementVertical.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.Y += 4;
//                 } else if (_shortcuts.FastSqueezePlacementHorizontal.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.X -= 4;
//                 } else if (_shortcuts.FastSqueezePlacementVertical.Check(ctrl, shift, alt, true)) {
//                     _defaultStretch.Y -= 4;
//                 }

//                 if (_shortcuts.ResetPlacementStretch.Check(ctrl, shift, alt)) {
//                     _defaultStretch = Vector2.Zero;
//                 }
                
//                 // Pickup Prop
//                 if (_shortcuts.PickupProp.Check(ctrl, shift, alt))
//                 {
//                     for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
//                     {
//                         var current = GLOBALS.Level.Props[i];
    
//                         if (!CheckCollisionPointRec(tileMouseWorld, Utils.EncloseQuads(current.Quad)) || 
//                             ((current.Depth <= (GLOBALS.Layer + 1) * -10 || 
//                             current.Depth > GLOBALS.Layer * -10) && !GLOBALS.Settings.PropEditor.CrossLayerSelection)) 
//                             continue;

//                         if (current.Type == InitPropType.Tile && GLOBALS.TileDex is not null)
//                         {
//                             for (var c = 0; c < GLOBALS.TileDex.OrderedTilesAsProps.Length; c++)
//                             {
//                                 for (var p = 0; p < GLOBALS.TileDex.OrderedTilesAsProps[c].Length; p++)
//                                 {
//                                     var currentTileAsProp = GLOBALS.TileDex.OrderedTilesAsProps[c][p];

//                                     if (currentTileAsProp.Name != current.Name) continue;

//                                     _propsMenuTilesCategoryIndex = c;
//                                     _propsMenuTilesIndex = p;

//                                     _copiedPropSettings = current.Extras.Settings;
//                                     _copiedIsTileAsProp = true;
//                                 }
//                             }
//                         }
//                         else if (current.Type == InitPropType.Rope)
//                         {
//                             _copiedRopePoints = [..current.Extras.RopePoints];
//                             _copiedPropSettings = current.Extras.Settings;
//                             _copiedIsTileAsProp = false;
//                         }
//                         else if (current.Type == InitPropType.Long)
//                         {
//                             _copiedPropSettings = current.Extras.Settings;
//                             _copiedIsTileAsProp = false;
//                         }
//                         else
//                         {
//                             (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex) = current.Position;
//                             _copiedPropSettings = current.Extras.Settings;
//                             _copiedIsTileAsProp = false;
//                         }

//                         _copiedDepth = current.Depth;
//                     }
//                 }
//                 break;
            
//             case 0: // Select Mode
//                 if (!canDrawTile) break;
//                 var anySelected = _selected.Any(s => s);
//                 var fetchedSelected = GLOBALS.Level.Props
//                     .Select((prop, index) => (prop, index))
//                     .Where(p => _selected[p.index])
//                     .Select(p => p)
//                     .ToArray();
                
//                 if (anySelected)
//                 {
//                     _selectedPropsEncloser = Utils.EncloseProps(fetchedSelected.Select(p => p.prop.Quad));
//                     _selectedPropsCenter = new Vector2(
//                         _selectedPropsEncloser.X + 
//                         _selectedPropsEncloser.Width/2, 
//                         _selectedPropsEncloser.Y + 
//                         _selectedPropsEncloser.Height/2
//                     );

//                     if (fetchedSelected[0].prop.Type == InitPropType.Rope) {
//                         if (_shortcuts.ToggleRopeGravity.Check(ctrl, shift, alt)) {
//                             for (var i = 0; i < _models.Length; i++)
//                             {
//                                 if (_models[i].index == fetchedSelected[0].index) {
//                                     _models[i].model.Gravity = !_models[i].model.Gravity;
//                                     break;
//                                 }
//                             }
//                         }
                        
//                         if (_shortcuts.SimulationBeizerSwitch.Check(ctrl, shift, alt)) {
                            
//                             // Find the rope model
//                             var modelIndex = -1;

//                             for (var i = 0; i < _models.Length; i++)
//                             {
//                                 if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
//                             }

//                             if (modelIndex != -1) {
//                                 ref var currentModel = ref _models[modelIndex];
//                                 currentModel.model.EditType = (RopeModel.EditTypeEnum)(((int)currentModel.model.EditType + 1) % 2);
//                             }
//                         }

//                         var foundRope = _models.SingleOrDefault(rope => rope.index == fetchedSelected[0].index);

//                         if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt)) {
//                             foundRope.model.UpdateSegments([..fetchedSelected[0].prop.Extras.RopePoints, new Vector2(0, 0)]);

//                             // ImportRopeModels();
//                         }

//                         if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt)) {
//                             foundRope.model.UpdateSegments(fetchedSelected[0].prop.Extras.RopePoints.Take(fetchedSelected[0].prop.Extras.RopePoints.Length - 1).ToArray());

//                             // ImportRopeModels();
//                         }

//                     }
                
//                     if (!isSearchBusy && _shortcuts.ResetQuadVertices.Check(ctrl, shift, alt)) {
//                         for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                         {
//                             if (!_selected[p]) continue;
                            
//                             GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(GLOBALS.Level.Props[p].OriginalQuad, GLOBALS.Level.Props[p].Rotation);
//                         }

//                         _shouldRedrawPropLayer = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                     }
//                 }
//                 else
//                 {
//                     _selectedPropsEncloser.Width = 0;
//                     _selectedPropsEncloser.Height = 0;

//                     _selectedPropsCenter.X = 0;
//                     _selectedPropsCenter.Y = 0;
//                 }

//                 #region RotatePropsKeyboard
//                 // Rotate Selected (Keyboard)
//                 if (!isSearchBusy && _shortcuts.RotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
//                     const float degree = 0.3f;

//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation += degree;
//                     }

//                     _shouldRedrawPropLayer = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                 }

//                 if (!isSearchBusy && _shortcuts.RotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
//                     const float degree = -0.3f;

//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation += degree;
//                     }

//                     _shouldRedrawPropLayer = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                 }

//                 if (!isSearchBusy && _shortcuts.FastRotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
//                     const float degree = 2f;

//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation += degree;
//                     }

//                     _shouldRedrawPropLayer = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                 }

//                 if (!isSearchBusy && _shortcuts.FastRotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
//                     const float degree = -2f;

//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation += degree;
//                     }

//                     _shouldRedrawPropLayer = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                 }

//                 if (!isSearchBusy && _shortcuts.ResetPlacementRotation.Check(ctrl, shift, alt) && anySelected) {
//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, -GLOBALS.Level.Props[p].Rotation, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(-_placementRotation, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation = 0;
//                     }

//                     _shouldRedrawPropLayer = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                 }
//                 #endregion

//                 #region ActivateModes
//                 if (!isSearchBusy) {
//                     // Cycle selected
//                     if (_shortcuts.CycleSelected.Check(ctrl, shift, alt) && anySelected && _selectedCycleIndices.Length > 0)
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }

                        
//                         _selectedCycleCursor++;
//                         Utils.Cycle(ref _selectedCycleCursor, 0, _selectedCycleIndices.Length - 1);

//                         _selected = new bool[GLOBALS.Level.Props.Length];
//                         _selected[_selectedCycleIndices[_selectedCycleCursor]] = true;
//                         _shouldUpdateModeIndicatorsRT = true;
//                     }
//                     // Move
//                     else if (_shortcuts.ToggleMovingPropsMode.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         _scalingProps = false;
//                         _movingProps = !_movingProps;
//                         _rotatingProps = false;
//                         _stretchingProp = false;
//                         _editingPropPoints = false;
//                         // _ropeMode = false;

//                         _propsMoveMousePos = _propsMoveMouseAnchor = GetScreenToWorld2D(GetMousePosition(), _camera);
//                         _shouldUpdateModeIndicatorsRT = true;

//                         if (!_movingProps) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }
//                     }
//                     // Rotate
//                     else if (_shortcuts.ToggleRotatingPropsMode.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         _scalingProps = false;
//                         _movingProps = false;
//                         _rotatingProps = !_rotatingProps;
//                         _stretchingProp = false;
//                         _editingPropPoints = false;
//                         // _ropeMode = false;
//                         _shouldUpdateModeIndicatorsRT = true;

//                         if (!_rotatingProps) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }
//                     }
//                     // Scale
//                     else if (_shortcuts.ToggleScalingPropsMode.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         _movingProps = false;
//                         _rotatingProps = false;
//                         _stretchingProp = false;
//                         _editingPropPoints = false;
//                         _scalingProps = !_scalingProps;
//                         _stretchAxes = 0;
//                         // _ropeMode = false;
                        
//                         SetMouseCursor(MouseCursor.ResizeNesw);
//                         _shouldUpdateModeIndicatorsRT = true;

//                         if (_scalingProps) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }
//                     }
//                     // Hide
//                     else if (_shortcuts.TogglePropsVisibility.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }

                        
//                         for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
//                         {
//                             if (_selected[i]) _hidden[i] = !_hidden[i];
//                         }
//                         _shouldUpdateModeIndicatorsRT = true;
//                     }
//                     // Edit Quads
//                     else if (_shortcuts.ToggleEditingPropQuadsMode.Check(ctrl, shift, alt) && fetchedSelected.Length == 1)
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }

                        
//                         _scalingProps = false;
//                         _movingProps = false;
//                         _rotatingProps = false;
//                         _stretchingProp = !_stretchingProp;
//                         _editingPropPoints = false;
//                         // _ropeMode = false;
//                         _shouldUpdateModeIndicatorsRT = true;

//                         if (!_stretchingProp) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }
//                     }
//                     // Delete
//                     else if (_shortcuts.DeleteSelectedProps.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }
    
//                         _scalingProps = false;
//                         _movingProps = false;
//                         _rotatingProps = false;
//                         _stretchingProp = false;
//                         _editingPropPoints = false;
//                         // _ropeMode = false;
                        
//                         GLOBALS.Level.Props = _selected
//                             .Select((s, i) => (s, i))
//                             .Where(v => !v.Item1)
//                             .Select(v => GLOBALS.Level.Props[v.Item2])
//                             .ToArray();

//                         if (_selected.Length != GLOBALS.Level.Props.Length) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }
                        
//                         _selected = new bool[GLOBALS.Level.Props.Length]; // Update selected
//                         _hidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

                        
//                         fetchedSelected = GLOBALS.Level.Props
//                             .Select((prop, index) => (prop, index))
//                             .Where(p => _selected[p.index])
//                             .Select(p => p)
//                             .ToArray();
                        
//                         ImportRopeModels(); // don't forget to update the list when props list is modified
//                         _shouldUpdateModeIndicatorsRT = true;
//                     }
//                     // Rope-only actions
//                     else if (
//                         fetchedSelected.Length == 1 &&
//                         fetchedSelected[0].prop.Type == InitPropType.Rope
//                     )
//                     {
//                         // Edit Rope Points
//                         if (_shortcuts.ToggleRopePointsEditingMode.Check(ctrl, shift, alt))
//                         {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else {
//                                 _shouldRedrawLevel = true;
//                             }

                            
//                             _scalingProps = false;
//                             _movingProps = false;
//                             _rotatingProps = false;
//                             _stretchingProp = false;
//                             _editingPropPoints = !_editingPropPoints;
//                             // _ropeMode = false;
//                             _shouldUpdateModeIndicatorsRT = true;

//                             if (!_editingPropPoints) {
//                                 _gram.Proceed(GLOBALS.Level.Props);
//                             }

//                             _bezierHandleLock = -1;
//                         }
//                         // Rope mode
//                         else if (_shortcuts.ToggleRopeEditingMode.Check(ctrl, shift, alt))
//                         {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else {
//                                 _shouldRedrawLevel = true;
//                             }

//                             _bezierHandleLock = -1;

//                             // _scalingProps = false;
//                             // _movingProps = false;
//                             // _rotatingProps = false;
//                             // _stretchingProp = false;
//                             // _editingPropPoints = false;
//                             // _ropeMode = !_ropeMode;

//                             if (fetchedSelected.Length == 1 && _models.Length > 0) {
//                                 var found = Array.FindIndex(_models, (m) => m.index == fetchedSelected[0].index);

//                                 if (found != -1) { 
//                                     _models[found].simulate = !_models[found].simulate;
//                                     if (!_models[found].simulate) {
//                                         _gram.Proceed(GLOBALS.Level.Props);
//                                         _bezierHandleLock = -1; // Must be set!
//                                     };
//                                 }
//                             }
//                             _shouldUpdateModeIndicatorsRT = true;
//                         }
//                         else if (_shortcuts.DuplicateProps.Check(ctrl, shift, alt)) {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else {
//                                 _shouldRedrawLevel = true;
//                             }

//                             _bezierHandleLock = -1;

//                             List<Prop> dProps = [];
                        
//                             foreach (var (prop, _) in fetchedSelected)
//                             {
//                                 dProps.Add(new Prop(
//                                         prop.Depth,
//                                         prop.Name,
//                                         prop.IsTile,
//                                         new Data.Quad(
//                                             prop.Quad.TopLeft,
//                                             prop.Quad.TopRight,
//                                             prop.Quad.BottomRight,
//                                             prop.Quad.BottomLeft
//                                         ))
//                                     {
//                                         Extras = new PropExtras(
//                                             prop.Extras.Settings.Clone(),
//                                             [..prop.Extras.RopePoints]),

//                                             Type = prop.Type,
//                                             Tile = prop.Tile,
//                                             Position = prop.Position
//                                     }
//                                 );

//                             }
                            
//                             GLOBALS.Level.Props = [..GLOBALS.Level.Props, ..dProps];

//                             var newSelected = new bool[GLOBALS.Level.Props.Length]; // Update selected
//                             var newHidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

//                             if (newSelected.Length != _selected.Length) {
//                                 _gram.Proceed(GLOBALS.Level.Props);
//                             }

//                             for (var i = 0; i < _selected.Length; i++)
//                             {
//                                 newSelected[i] = _selected[i];
//                                 newHidden[i] = _hidden[i];
//                             }

//                             _selected = newSelected;
//                             _hidden = newHidden;
                        
//                             fetchedSelected = GLOBALS.Level.Props
//                                 .Select((prop, index) => (prop, index))
//                                 .Where(p => _selected[p.index])
//                                 .Select(p => p)
//                                 .ToArray();
//                         }
//                     }
//                     // Duplicate
//                     else if (_shortcuts.DuplicateProps.Check(ctrl, shift, alt) && anySelected)
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }

                        
//                         List<Prop> dProps = [];
                        
//                         foreach (var (prop, _) in fetchedSelected)
//                         {
//                             dProps.Add(new Prop(
//                                     prop.Depth,
//                                     prop.Name,
//                                     prop.IsTile,
//                                     new Data.Quad(
//                                         prop.Quad.TopLeft,
//                                         prop.Quad.TopRight,
//                                         prop.Quad.BottomRight,
//                                         prop.Quad.BottomLeft
//                                     ))
//                                 {
//                                     Extras = new PropExtras(
//                                         prop.Extras.Settings.Clone(),
//                                         [..prop.Extras.RopePoints]),
//                                     Type = prop.Type,
//                                     Tile = prop.Tile,
//                                     Position = prop.Position
//                                 }
//                             );

//                         }
                        
//                         GLOBALS.Level.Props = [..GLOBALS.Level.Props, ..dProps];

//                         var newSelected = new bool[GLOBALS.Level.Props.Length]; // Update selected
//                         var newHidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden

//                         if (newSelected.Length != _selected.Length) {
//                             _gram.Proceed(GLOBALS.Level.Props);
//                         }

//                         for (var i = 0; i < _selected.Length; i++)
//                         {
//                             newSelected[i] = _selected[i];
//                             newHidden[i] = _hidden[i];
//                         }

//                         _selected = newSelected;
//                         _hidden = newHidden;
                    
//                         fetchedSelected = GLOBALS.Level.Props
//                             .Select((prop, index) => (prop, index))
//                             .Where(p => _selected[p.index])
//                             .Select(p => p)
//                             .ToArray();
//                     }
//                     else SetMouseCursor(MouseCursor.Default);
//                 }
//                 #endregion

//                 // Simulate ropes
//                 if (_models.Length > 0)
//                 {
//                     _shouldUpdateModeIndicatorsRT = true;

//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }

//                     for (var i = 0; i < _models.Length; i++) {
//                         ref var current = ref _models[i];

//                         if (current.simulate && ++_ropeSimulationFrame % _ropeSimulationFrameCut == 0) {
//                             #if DEBUG
//                             if (current.index < 0 || 
//                             current.index >= GLOBALS.Level.Props.Length || 
//                             GLOBALS.Level.Props[current.index].Type != InitPropType.Rope) {
//                                 throw new Exception("model's reference index was invalid or out of bounds");
//                             }
//                             #endif
                            
//                             var prop = GLOBALS.Level.Props[current.index]; // potential exception

//                             if (current.model.EditType == RopeModel.EditTypeEnum.Simulation) {
//                                 current.model.Update(
//                                     prop.Quad,
//                                     prop.Depth switch {
//                                         < -19 => 2,
//                                         < -9 => 1,
//                                         _ => 0
//                                     }
//                                 );
//                             } else if (current.model.EditType == RopeModel.EditTypeEnum.BezierPaths) {
//                                 var (pA, pB) = Utils.RopeEnds(fetchedSelected[0].prop.Quad);
                                
//                                 fetchedSelected[0].prop.Extras.RopePoints = Utils.Casteljau(fetchedSelected[0].prop.Extras.RopePoints.Length, [ pA, ..current.model.BezierHandles, pB ]);

//                                 if ((IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) && _bezierHandleLock != -1)
//                                     _bezierHandleLock = -1;

//                                 if (IsMouseButtonDown(_shortcuts.SelectProps.Button))
//                                 {
//                                     for (var b = 0; b < current.model.BezierHandles.Length; b++)
//                                     {
//                                         if (_bezierHandleLock == -1 && CheckCollisionPointCircle(tileMouseWorld, current.model.BezierHandles[b], 5f))
//                                             _bezierHandleLock = b;

//                                         if (_bezierHandleLock == b) current.model.BezierHandles[b] = tileMouseWorld;
//                                     }
//                                 }
//                             }
//                         }
//                     }
//                 }
                
//                 // TODO: switch on enums instead
//                 if (_movingProps && anySelected && _propsMoveMouseAnchor is not null && _propsMoveMousePos is not null)
//                 {
//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }
                    
//                     // update mouse delta
                    
//                     var newMousePos = GetScreenToWorld2D(GetMousePosition(), _camera);
                    
//                     _propsMoveMouseDelta = newMousePos - _propsMoveMousePos.Value;

//                     // Vector2 gridDelta, preciseDelta;

//                     var deltaFactor = _snapMode switch {
//                         1 => 20,
//                         2 => 10,
//                         _ => 1
//                     };

//                     var gridScaled = new Vector2((int)(_propsMoveMousePos.Value.X / deltaFactor), (int)(_propsMoveMousePos.Value.Y / deltaFactor));
//                     var gridScaledBack = gridScaled * deltaFactor;
                    
//                     var newGridScaled = new Vector2((int)(newMousePos.X / deltaFactor), (int)(newMousePos.Y / deltaFactor));
//                     var newGridScaledBack = newGridScaled * deltaFactor;

//                     var delta = newGridScaledBack - gridScaledBack;

//                     _propsMoveMousePos = newMousePos;
                    
//                     // Fix delta when level panning
                    
//                     if (IsMouseButtonDown(_shortcuts.DragLevel.Button))
//                         _propsMoveMouseDelta = new Vector2(0, 0);

//                     for (var s = 0; s < _selected.Length; s++)
//                     {
//                         if (!_selected[s]) continue;
                        
//                         var quads = GLOBALS.Level.Props[s].Quad;
//                         var center = Utils.QuadsCenter(ref quads);

//                         Vector2 deltaToAdd = _snapMode switch {
//                             1 or 2 => delta,

//                             _ => _propsMoveMouseDelta
//                         };

//                         quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, deltaToAdd);
//                         quads.TopRight = Raymath.Vector2Add(quads.TopRight, deltaToAdd);
//                         quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, deltaToAdd);
//                         quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, deltaToAdd);

//                         GLOBALS.Level.Props[s].Quad = quads;

//                         if (GLOBALS.Level.Props[s].Type == InitPropType.Rope)
//                         {
//                             for (var r = 0; r < _models.Length; r++)
//                             {
//                                 var (propIndex, simulated, model) = _models[r];

//                                 if (propIndex == s)
//                                 {
//                                     var segments = GLOBALS.Level.Props[s].Extras.RopePoints;
//                                     if (!simulated) {
//                                         for (var p = 0; p < segments.Length; p++)
//                                         {
//                                             segments[p] = segments[p] + deltaToAdd;
//                                         }

//                                         GLOBALS.Level.Props[propIndex].Extras.RopePoints = segments;
//                                     }

//                                     for (var h = 0; h < _models[r].model.BezierHandles.Length; h++)
//                                     {
//                                         _models[r].model.BezierHandles[h] += deltaToAdd;
//                                     }
//                                 }
//                             }
//                         }
//                     }

//                     if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) {
//                         _gram.Proceed(GLOBALS.Level.Props);

//                         _movingProps = false;
//                     }
//                 }
//                 else if (_rotatingProps && anySelected)
//                 {
//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }

                    
//                     if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) {
//                         _gram.Proceed(GLOBALS.Level.Props);
//                         _rotatingProps = false;
//                     }

//                     var delta = GetMouseDelta();
                    
//                     // Collective Rotation
                    
//                     for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
//                     {
//                         if (!_selected[p]) continue;
                        
//                         var quads = GLOBALS.Level.Props[p].Quad;

//                         GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, delta.X, _selectedPropsCenter);

//                         if (GLOBALS.Level.Props[p].Type == InitPropType.Rope)
//                         {
//                             Utils.RotatePoints(delta.X, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
//                         }

//                         GLOBALS.Level.Props[p].Rotation += delta.X;
//                     }
//                 }
//                 else if (_scalingProps && anySelected)
//                 {
//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }

                    
//                     if (IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key))
//                     {
//                         _stretchAxes = 0;
//                         _scalingProps = false;
//                         SetMouseCursor(MouseCursor.Default);
//                     }

//                     if (IsKeyPressed(KeyboardKey.X))
//                     {
//                         _stretchAxes = (byte)(_stretchAxes == 1 ? 0 : 1);
//                         SetMouseCursor(MouseCursor.ResizeNesw);
//                     }
//                     if (IsKeyPressed(KeyboardKey.Y))
//                     {
//                         _stretchAxes =  (byte)(_stretchAxes == 2 ? 0 : 2);
//                         SetMouseCursor(MouseCursor.ResizeNs);
//                     }

//                     var delta = GetMouseDelta();

//                     switch (_stretchAxes)
//                     {
//                         case 0: // Uniform Scaling
//                         {
//                             var enclose = Utils.EncloseProps(fetchedSelected.Select(s => s.prop.Quad));
//                             var center = Utils.RectangleCenter(ref enclose);

//                             foreach (var selected in fetchedSelected)
//                             {
//                                 var quads = selected.prop.Quad;
//                                 Utils.ScaleQuads(ref quads, center, 1f + delta.X*0.01f);
//                                 GLOBALS.Level.Props[selected.index].Quad = quads;
//                             }
//                         }
//                             break;

//                         case 1: // X-axes Scaling
//                         {
//                             foreach (var selected in fetchedSelected)
//                             {
//                                 var quads = selected.prop.Quad;
//                                 var center = Utils.QuadsCenter(ref quads);
                                
//                                 Utils.ScaleQuadsX(ref quads, center, 1f + delta.X * 0.01f);
                                
//                                 GLOBALS.Level.Props[selected.index].Quad = quads;
//                             }
//                         }
//                             break;

//                         case 2: // Y-axes Scaling
//                         {
//                             foreach (var selected in fetchedSelected)
//                             {
//                                 var quads = selected.prop.Quad;
//                                 var center = Utils.QuadsCenter(ref quads);

//                                 Utils.ScaleQuadsY(ref quads, center, 1f - delta.Y * 0.01f);
                                
//                                 GLOBALS.Level.Props[selected.index].Quad = quads;
//                             }
//                         }
//                             break;
//                     }
//                 }
//                 else if (_stretchingProp && anySelected)
//                 {
//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }

                    
//                     var currentQuads = fetchedSelected[0].prop.Quad; 
                    
//                     var posV = _snapMode switch
//                     {
//                         1 => new Vector2((int)(tileMouseWorld.X / 20f), (int)(tileMouseWorld.Y / 20f)) * 20f,
//                         2 => new Vector2((int)(tileMouseWorld.X / 10f), (int)(tileMouseWorld.Y / 10f)) * 10f,
//                         _ => tileMouseWorld
//                     };

//                     if (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key))
//                     {
//                         if (fetchedSelected[0].prop.Type == InitPropType.Rope)
//                         {
//                             var middleLeft = Raymath.Vector2Divide(
//                                 Raymath.Vector2Add(currentQuads.TopLeft, currentQuads.BottomLeft),
//                                 new(2f, 2f)
//                             );

//                             var middleRight = Raymath.Vector2Divide(
//                                 Raymath.Vector2Add(currentQuads.TopRight, currentQuads.BottomRight),
//                                 new(2f, 2f)
//                             );
                            
//                             var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
//                             var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, middleLeft));
                            
//                             if (
//                                 CheckCollisionPointCircle(
//                                     posV, middleLeft,
//                                     5f
//                                 ) || _quadLock == 1)
//                             {
//                                 _quadLock = 1;
//                                 currentQuads.BottomLeft = Raymath.Vector2Add(
//                                     posV, 
//                                     new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                     );
                                
//                                 currentQuads.TopLeft = Raymath.Vector2Add(
//                                     posV, 
//                                     new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                     );
                                
//                                 currentQuads.BottomRight = Raymath.Vector2Add(
//                                     middleRight, 
//                                     new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                 );
                                
//                                 currentQuads.TopRight = Raymath.Vector2Add(
//                                     middleRight, 
//                                     new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                 );
//                             }

//                             if (
//                                 CheckCollisionPointCircle(
//                                     tileMouseWorld, middleRight,
//                                     5f
//                                     ) || _quadLock == 2)
//                             {
//                                 _quadLock = 2;
                                
//                                 currentQuads.BottomLeft = Raymath.Vector2Add(
//                                     middleLeft, 
//                                     new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                 );
                                
//                                 currentQuads.TopLeft = Raymath.Vector2Add(
//                                     middleLeft, 
//                                     new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                 );
                                
//                                 currentQuads.BottomRight = Raymath.Vector2Add(
//                                     posV, 
//                                     new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                 );
                                
//                                 currentQuads.TopRight = Raymath.Vector2Add(
//                                     posV, 
//                                     new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                 );
//                             }
//                         }
//                         else if (fetchedSelected[0].prop.Type == InitPropType.Long)
//                         {
//                             var (left, top, right, bottom) = Utils.LongSides(fetchedSelected[0].prop.Quad);
                            
//                             var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(left, right), new(1.0f, 0.0f));
                            
//                             var r = Raymath.Vector2Length(Raymath.Vector2Subtract(currentQuads.TopLeft, left));

//                             if (CheckCollisionPointCircle(tileMouseWorld, left, 5f) && _quadLock == 0)
//                             {
//                                 _quadLock = 1;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, right, 5f) && _quadLock == 0)
//                             {
//                                 _quadLock = 2;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, top, 5f) && _quadLock == 0)
//                             {
//                                 _quadLock = 3;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, bottom, 5f) && _quadLock == 0)
//                             {
//                                 _quadLock = 4;
//                             }

//                             switch (_quadLock)
//                             {
//                                 case 1: // left
//                                     currentQuads.BottomLeft = Raymath.Vector2Add(
//                                         posV, 
//                                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.TopLeft = Raymath.Vector2Add(
//                                         posV, 
//                                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.BottomRight = Raymath.Vector2Add(
//                                         right, 
//                                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.TopRight = Raymath.Vector2Add(
//                                         right, 
//                                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                     );
//                                     break;
                                
//                                 case 2: // right
//                                     currentQuads.BottomLeft = Raymath.Vector2Add(
//                                         left, 
//                                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.TopLeft = Raymath.Vector2Add(
//                                         left, 
//                                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.BottomRight = Raymath.Vector2Add(
//                                         posV, 
//                                         new(r * (float) Math.Cos(-beta - float.DegreesToRadians(90)), r * (float) Math.Sin(-beta - float.DegreesToRadians(90)))
//                                     );
                                
//                                     currentQuads.TopRight = Raymath.Vector2Add(
//                                         posV, 
//                                         new(r * (float) Math.Cos(-beta + float.DegreesToRadians(90)), r * (float) Math.Sin(-beta + float.DegreesToRadians(90)))
//                                     );
//                                     break;
                                
//                                 case 3: // TODO: top
//                                     break;
                                
//                                 case 4: // TODO: bottom
//                                     break;
//                             }
//                         }
//                         else
//                         {
//                             if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopLeft, 5f) && _quadLock == 0)
//                             {
//                                 _quadLock = 1;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.TopRight, 5f) &&
//                                      _quadLock == 0)
//                             {
//                                 _quadLock = 2;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomRight, 5f) &&
//                                      _quadLock == 0)
//                             {
//                                 _quadLock = 3;
//                             }
//                             else if (CheckCollisionPointCircle(tileMouseWorld, currentQuads.BottomLeft, 5f) &&
//                                      _quadLock == 0)
//                             {
//                                 _quadLock = 4;
//                             }
                            
//                             // Check Top-Left
//                             if (_quadLock == 1)
//                             {
//                                 currentQuads.TopLeft = posV;
//                             }
//                             // Check Top-Right
//                             else if (_quadLock == 2)
//                             {
//                                 currentQuads.TopRight = posV;
//                             }
//                             // Check Bottom-Right 
//                             else if (_quadLock == 3)
//                             {
//                                 currentQuads.BottomRight = posV;
//                             }
//                             // Check Bottom-Left
//                             else if (_quadLock == 4)
//                             {
//                                 currentQuads.BottomLeft = posV;
//                             }
//                         }
                        
//                         GLOBALS.Level.Props[fetchedSelected[0].index].Quad = currentQuads;
//                     }
//                     else if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _quadLock != 0) _quadLock = 0;
//                 }
//                 else if (_editingPropPoints && fetchedSelected.Length == 1)
//                 {
//                     // _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     else {
//                         _shouldRedrawLevel = true;
//                     }

                    
//                     var points = fetchedSelected[0].prop.Extras.RopePoints;

//                     if ((IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) && _pointLock != -1) _pointLock = -1;
                    
//                     if (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key))
//                     {
//                         // Check Collision of Each Point

//                         for (var p = 0; p < points.Length; p++)
//                         {
//                             if (CheckCollisionPointCircle(
//                                     tileMouseWorld, 
//                                     new Vector2(
//                                         points[p].X, 
//                                         points[p].Y
//                                         ), 
//                                     3f) || 
//                                 _pointLock == p
//                             )
//                             {
//                                 _pointLock = p;
//                                 points[p] = tileMouseWorld;
//                             }
//                         }
//                     }
//                 }
//                 else
//                 {
//                     if (anySelected)
//                     {
//                         if (_shortcuts.DeepenSelectedProps.Check(ctrl, shift, alt))
//                         {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else {
//                                 _shouldRedrawLevel = true;
//                             }

                            
//                             foreach (var selected in fetchedSelected)
//                             {
//                                 selected.prop.Depth--;

//                                 if (selected.prop.Depth < -29) selected.prop.Depth = 29;
//                             }
//                         }
//                         else if (_shortcuts.UndeepenSelectedProps.Check(ctrl, shift, alt))
//                         {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else {
//                                 _shouldRedrawLevel = true;
//                             }

                            
//                             foreach (var selected in fetchedSelected)
//                             {
//                                 selected.prop.Depth++;

//                                 if (selected.prop.Depth > 0) selected.prop.Depth = 0;
//                             }
//                         }
                    
//                         if (_shortcuts.CycleVariations.Check(ctrl, shift, alt)) {
//                             foreach (var selected in fetchedSelected) {
//                                 if (selected.prop.Extras.Settings is IVariable vs) {
//                                     var (category, index) = selected.prop.Position;

//                                     var variations = (GLOBALS.Props[category][index] as IVariableInit)?.Variations ?? 1;
                                
//                                     vs.Variation = (vs.Variation += 1) % variations;
//                                 }
//                             }

//                             _shouldRedrawPropLayer = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
//                         }
//                     }
                    
//                     if (_bezierHandleLock == -1 && (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key)) && !_clickTracker && canDrawTile)
//                     {
//                         _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
//                         _clickTracker = true;
//                     }


//                     if ((IsMouseButtonReleased(_shortcuts.SelectProps.Button) || IsKeyReleased(_shortcuts.SelectPropsAlt.Key)) && _clickTracker && !(_isPropsWinHovered || _isPropsWinDragged))
//                     {
//                         // _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         else {
//                             _shouldRedrawLevel = true;
//                         }
                        
//                         _clickTracker = false;

//                         List<int> selectedI = [];
                    
//                         var selectCount = 0;
//                         var lastSelected = -1;

//                         for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
//                         {
//                             var current = GLOBALS.Level.Props[i];
//                             var propSelectRect = Utils.EncloseQuads(current.Quad);
                            
//                             if (IsKeyDown(_shortcuts.PropSelectionModifier.Key))
//                             {
//                                 if (CheckCollisionRecs(propSelectRect, _selection) && (GLOBALS.Settings.PropEditor.CrossLayerSelection || !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10)))
//                                 {
//                                     _selected[i] = !_selected[i];
//                                 }
//                             }
//                             else
//                             {
//                                 if (CheckCollisionRecs(propSelectRect, _selection) && (GLOBALS.Settings.PropEditor.CrossLayerSelection || !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10)))
//                                 {
//                                     _selected[i] = true;
//                                     selectedI.Add(i);
//                                 }
//                                 else
//                                 {
//                                     _selected[i] = false;
//                                 }
//                             }

//                             if (_selected[i]) {
//                                 selectCount++;
//                                 lastSelected = i;
//                             }
//                         }

//                         // Auto-simulate ropes
//                         if (GLOBALS.Settings.PropEditor.SimulateSelectedRopes) {
//                             for (var i = 0; i < _models.Length; i++) {
//                                 _models[i].simulate = _selected[_models[i].index];
//                             }
//                         }

//                         if (selectCount == 0) {
//                             _shouldUpdateModeIndicatorsRT = true;
//                         }

//                         _selectedCycleIndices = [..selectedI];
//                         _selectedCycleCursor = -1;
//                     }   
//                     else if (!_isPropsListHovered && !_isPropsWinHovered && canDrawTile && _shortcuts.PlaceProp.Button != _shortcuts.SelectProps.Button && (_shortcuts.PlaceProp.Check(ctrl, shift, alt, true) ||_shortcuts.PlacePropAlt.Check(ctrl, shift, alt, true))) {
//                         _mode = 1;
//                         if (_noCollisionPropPlacement) _lockedPlacement = true;
//                     }
//                 }
                
//                 break;
//         }
//         #endregion

//         #region TileEditorDrawing
//         // BeginDrawing();

//         if (_shouldRedrawLevel)
//         {
//             RedrawLevel();
//             _shouldRedrawLevel = false;
//         }

//         if (_shouldRedrawPropLayer) {
//             DrawPropLayerRT();

//             _shouldRedrawPropLayer = false;
//         }

//         if (_shouldUpdateModeIndicatorsRT) {
//             UpdateModeIndicators();
//             _shouldUpdateModeIndicatorsRT = false;
//         }

//         ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
//             ? Color.Black 
//             : new Color(170, 170, 170, 255));

//         BeginMode2D(_camera);
//         {
//             DrawRectangleLinesEx(new Rectangle(-3, -3, GLOBALS.Level.Width * 20 + 6, GLOBALS.Level.Height * 20 + 6), 3, Color.White);
            
//             BeginShaderMode(GLOBALS.Shaders.VFlip);
//             SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
//             DrawTexturePro(GLOBALS.Textures.GeneralLevel.Texture, 
//                 new Rectangle(0, 0, GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20), 
//                 new Rectangle(0, 0, GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20), 
//                 new Vector2(0, 0), 
//                 0, 
//                 Color.White);
            
            
//             EndShaderMode();

//             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) {
//                 BeginShaderMode(GLOBALS.Shaders.VFlip);
//                 SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), _propLayerRT.Raw.Texture);
//                 DrawTexture(_propLayerRT.Raw.Texture, 0, 0, Color.White);
//                 EndShaderMode();
//             }

            
//             // Grid

//             if (_showGrid)
//             {
//                 Printers.DrawGrid(20);
//             }
            
//             if (GLOBALS.Settings.GeneralSettings.DarkTheme)
//             {
//                 DrawRectangleLines(0, 0, GLOBALS.Level.Width*20, GLOBALS.Level.Height*20, Color.White);
//             }
            
//             if (GLOBALS.Settings.PropEditor.Cameras) {
//                 var counter = 0;

//                 foreach (var cam in GLOBALS.Level.Cameras)
//                 {
//                     var critRect = Utils.CameraCriticalRectangle(cam.Coords);

//                     DrawRectangleLinesEx(
//                         GLOBALS.Settings.PropEditor.CamerasInnerBoundries 
//                             ? critRect with { X = critRect.X, Y = critRect.Y, Width = critRect.Width, Height = critRect.Height }
//                             : new(cam.Coords.X, cam.Coords.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
//                         4f,
//                         GLOBALS.Settings.GeneralSettings.ColorfulCameras 
//                             ? GLOBALS.CamColors[counter] 
//                             : Color.Pink
//                     );

//                     counter++;
//                     Utils.Cycle(ref counter, 0, GLOBALS.CamColors.Length - 1);
//                 }
//             }

//             switch (_mode)
//             {
//                 case 1: // Place Mode
//                     switch (_menuRootCategoryIndex)
//                     {
//                         case 0: // Current Tile-As-Prop
//                             {
//                                 switch (_snapMode)
//                                 {
//                                     case 0: // free
//                                     {
//                                         var offset = new Vector2(
//                                             _currentTile.Size.Width + _currentTile.BufferTiles*2, 
//                                             _currentTile.Size.Height + _currentTile.BufferTiles*2
//                                         ) * 20;

//                                         offset /= 2;

//                                         var propQuad = new Data.Quad(
//                                             tileMouseWorld - offset - _defaultStretch,
//                                             tileMouseWorld + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
//                                             tileMouseWorld + offset + _defaultStretch,
//                                             tileMouseWorld + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
//                                         );

//                                         if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
//                                         if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

//                                         propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);

//                                         Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
//                                     }
//                                         break;

//                                     case 1: // grid snap
//                                     {
//                                         var posV = new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20f;

//                                         var offset = new Vector2(
//                                             _currentTile.Size.Width + _currentTile.BufferTiles*2, 
//                                             _currentTile.Size.Height + _currentTile.BufferTiles*2
//                                         ) * 20;

//                                         offset /= 2;

//                                         var propQuad = new Data.Quad(
//                                             posV - offset - _defaultStretch,
//                                             posV + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
//                                             posV + offset + _defaultStretch,
//                                             posV + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
//                                         );

//                                         if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
//                                         if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

//                                         propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);
                                        
//                                         // Printers.DrawTileAsProp(
//                                         //     _currentTile,
//                                         //     posV,
//                                         //     _placementRotation * _placementRotationSteps
//                                         // );

//                                         Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
//                                     }
//                                         break;
                                    
//                                     case 2: // precise grid snap
//                                     {
//                                         var posV = new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10;
                                        
//                                         var offset = new Vector2(
//                                             _currentTile.Size.Width + _currentTile.BufferTiles*2, 
//                                             _currentTile.Size.Height + _currentTile.BufferTiles*2
//                                         ) * 20;

//                                         offset /= 2;

//                                         var propQuad = new Data.Quad(
//                                             posV - offset - _defaultStretch,
//                                             posV + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
//                                             posV + offset + _defaultStretch,
//                                             posV + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
//                                         );

//                                         if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
//                                         if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

//                                         propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);
                                        
//                                         // Printers.DrawTileAsProp(
//                                         //     _currentTile,
//                                         //     posV,
//                                         //     _placementRotation * _placementRotationSteps
//                                         // );

//                                         Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
//                                     }
//                                         break;
//                                 }
//                             }
//                             break;
                        
//                         case 1: // Current Rope
//                             DrawCircleV(tileMouseWorld, 3f, Color.Blue);
//                             break;

//                         case 2: // Current Long Prop
//                         if (!_longInitialPlacement) {
//                             var prop = GLOBALS.LongProps[_propsMenuLongsIndex];
//                             var texture = GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
//                             var height = texture.Height / 2f;

//                             var posV = _snapMode switch
//                             {
//                                 1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
//                                 2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
//                                 _ => tileMouseWorld,
//                             };
                            
//                             var offset = new Vector2(
//                                 _currentTile.Size.Width + _currentTile.BufferTiles*2, 
//                                 _currentTile.Size.Height + _currentTile.BufferTiles*2
//                             ) * 20;

//                             offset /= 2;

//                             var propQuad = new Data.Quad(
//                                 posV - offset,
//                                 posV + offset with { Y = -offset.Y },
//                                 posV + offset,
//                                 posV + offset with { X = -offset.X }
//                             );

//                             if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
//                             if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

//                             propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);

//                             Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
//                         }
//                             break;

//                         case 3: // Current Prop
//                         {
//                             // Since I've already seperated regular props from everything else, this can be
//                             // consideColor.Red outdated
//                             var prop = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
//                             var texture = GLOBALS.Textures.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
                            
//                             var (width, height, settings) = prop switch
//                             {
//                                 InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
//                                 InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
//                                 InitVariedSoftProp variedSoft => (variedSoft.SizeInPixels.x  / 2f, variedSoft.SizeInPixels.y / 2f, new PropVariedSoftSettings(variation:_defaultVariation)),
//                                 InitSoftProp => (texture.Width  / 2f, texture.Height  / 2f, new PropSoftSettings()),
//                                 InitVariedDecalProp variedDecal => (variedDecal.SizeInPixels.x  / 2f, variedDecal.SizeInPixels.y / 2f, new PropVariedDecalSettings(variation:_defaultVariation)),
//                                 InitSimpleDecalProp => (texture.Width / 2f, texture.Height / 2f, new PropSimpleDecalSettings()), 
//                                 InitSoftEffectProp => (texture.Width / 2f, texture.Height / 2f, new PropSoftEffectSettings()), 
//                                 InitAntimatterProp => (texture.Width / 2f, texture.Height / 2f, new PropAntimatterSettings()),
//                                 InitLongProp => (texture.Width / 2f, texture.Height / 2f, new PropLongSettings()), 
//                                 InitRopeProp => (texture.Width / 2f, texture.Height / 2f, new PropRopeSettings()),
                                        
//                                 _ => (texture.Width / 2f, texture.Height / 2f, new BasicPropSettings())
//                             };
                            
//                             var posV = _snapMode switch
//                             {
//                                 1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
//                                 2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
//                                 _ => tileMouseWorld,
//                             };

//                             var quad = new Data.Quad(
//                                 new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
//                                 new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
//                                 new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y), 
//                                 new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y));

//                             if (_vFlipPlacement) Utils.VFlipQuad(ref quad);
//                             if (_hFlipPlacement) Utils.HFlipQuad(ref quad);
                            
//                             Printers.DrawProp(settings, prop, texture, quad,
//                                 0,
//                                 _placementRotation * _placementRotationSteps
//                             );
//                         }
//                             break;
//                     }
//                     break;
                
//                 case 0: // Select Mode
                    
//                     // // TODO: tweak selection cancellation
//                     // if ((_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && _clickTracker)
//                     // {
//                     //     var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
//                     //     var diff = Raymath.Vector2Subtract(mouse, _selection1);
//                     //     var position = (diff.X > 0, diff.Y > 0) switch
//                     //     {
//                     //         (true, true) => _selection1,
//                     //         (true, false) => new Vector2(_selection1.X, mouse.Y),
//                     //         (false, true) => new Vector2(mouse.X, _selection1.Y),
//                     //         (false, false) => mouse
//                     //     };

//                     //     _selection = new Rectangle(
//                     //         position.X, 
//                     //         position.Y, 
//                     //         Math.Abs(diff.X), 
//                     //         Math.Abs(diff.Y)
//                     //     );
                        
//                     //     DrawRectangleRec(_selection, new Color(0, 0, 255, 90));
                        
//                     //     DrawRectangleLinesEx(
//                     //         _selection,
//                     //         2f,
//                     //         Color.Blue
//                     //     );
//                     // }
//                     break;
//             }

//             // TODO: tweak selection cancellation
//             if ((IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key)) && _clickTracker)
//             {
//                 var mouse = GetScreenToWorld2D(GetMousePosition(), _camera);
//                 var diff = Raymath.Vector2Subtract(mouse, _selection1);
//                 var position = (diff.X > 0, diff.Y > 0) switch
//                 {
//                     (true, true) => _selection1,
//                     (true, false) => new Vector2(_selection1.X, mouse.Y),
//                     (false, true) => new Vector2(mouse.X, _selection1.Y),
//                     (false, false) => mouse
//                 };

//                 _selection = new Rectangle(
//                     position.X, 
//                     position.Y, 
//                     Math.Abs(diff.X), 
//                     Math.Abs(diff.Y)
//                 );
                
//                 DrawRectangleRec(_selection, new Color(0, 0, 255, 90));
                
//                 DrawRectangleLinesEx(
//                     _selection,
//                     2f,
//                     Color.Blue
//                 );
//             }

//         }
//         EndMode2D();

//         #region TileEditorUI

//         {
//             // Selected Props
//             var fetchedSelected = GLOBALS.Level.Props
//                 .Select((prop, index) => (prop, index))
//                 .Where(p => _selected[p.index])
//                 .Select(p => p)
//                 .ToArray();

//             // Coordinates

//             if (GLOBALS.Settings.TileEditor.HoveredTileInfo && canDrawTile)
//             {
//                 if (inMatrixBounds)
//                     DrawText(
//                         $"x: {tileMatrixX}, y: {tileMatrixY}\n{GLOBALS.Level.TileMatrix[tileMatrixY, tileMatrixX, GLOBALS.Layer]}",
//                         (int)tileMouse.X + previewScale,
//                         (int)tileMouse.Y + previewScale,
//                         15,
//                         Color.White
//                     );
//             }
//             else
//             {
//                 if (inMatrixBounds)
//                     DrawText(
//                         $"x: {tileMatrixX}, y: {tileMatrixY}",
//                         (int)tileMouse.X + previewScale,
//                         (int)tileMouse.Y + previewScale,
//                         15,
//                         Color.White
//                     );
//             }

//             // layer indicator

//             var newLayer = GLOBALS.Layer;

//             var layer3Hovered = GLOBALS.Layer == 2 && CheckCollisionPointRec(tileMouse, layer3Rect);

//             if (layer3Hovered)
//             {
//                 DrawRectangleRec(layer3Rect, Color.Blue with { A = 100 });

//                 if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 0;
//             }

//             DrawRectangleRec(
//                 layer3Rect,
//                 GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
//             );

//             DrawRectangleLines((int)layer3Rect.X, (int)layer3Rect.Y, 40, 40, Color.Gray);

//             if (GLOBALS.Layer == 2) DrawText("3", (int)layer3Rect.X + 15, (int)layer3Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);

//             if (GLOBALS.Layer is 1 or 0)
//             {
//                 var layer2Hovered = GLOBALS.Layer == 1 && CheckCollisionPointRec(tileMouse, layer2Rect);

//                 if (layer2Hovered)
//                 {
//                     DrawRectangleRec(layer2Rect, Color.Blue with { A = 100 });

//                     if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 2;
//                 }

//                 DrawRectangleRec(
//                     layer2Rect,
//                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
//                 );

//                 DrawRectangleLines((int)layer2Rect.X, (int)layer2Rect.Y, 40, 40, Color.Gray);

//                 if (GLOBALS.Layer == 1) DrawText("2", (int)layer2Rect.X + 15, (int)layer2Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
//             }

//             if (GLOBALS.Layer == 0)
//             {
//                 var layer1Hovered = CheckCollisionPointRec(tileMouse, layer1Rect);

//                 if (layer1Hovered)
//                 {
//                     DrawRectangleRec(layer1Rect, Color.Blue with { A = 100 });
//                     if (IsMouseButtonPressed(MouseButton.Left)) newLayer = 1;
//                 }

//                 DrawRectangleRec(
//                     layer1Rect,
//                     GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.Black with { A = 100 } : Color.White
//                 );

//                 DrawRectangleLines(
//                     (int)layer1Rect.X, (int)layer1Rect.Y, 40, 40, Color.Gray);

//                 DrawText("1", (int)layer1Rect.X + 15, (int)layer1Rect.Y + 10, 22, GLOBALS.Settings.GeneralSettings.DarkTheme ? Color.White : Color.Black);
//             }

//             if (newLayer != GLOBALS.Layer)
//             {
//                 GLOBALS.Layer = newLayer;
//                 UpdateDefaultDepth();
//                 _shouldRedrawLevel = true;
//                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//             }

//             // Update prop depth render texture
//             if (fetchedSelected.Length == 1 || 
//                 (fetchedSelected.Length > 1 && 
//                                                 Utils.AllEqual(fetchedSelected.Select(f => f.prop.Depth),
//                     fetchedSelected[0].prop.Depth)))
//             {
//                 BeginTextureMode(GLOBALS.Textures.PropDepth);
//                 ClearBackground(Color.Green);
//                 Printers.DrawDepthIndicator(fetchedSelected[0].prop);
//                 EndTextureMode();
//             }

//             rlImGui.Begin();
            
//             ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
//             // Navigation bar
                
//             if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);

//             var menuOpened = ImGui.Begin("Props Menu##PropsPlacementPanel");
            
//             var menuPos = ImGui.GetWindowPos();
//             var menuWinSpace = ImGui.GetWindowSize();

//             if (CheckCollisionPointRec(tileMouse, new(menuPos.X - 5, menuPos.Y, menuWinSpace.X + 10, menuWinSpace.Y)))
//             {
//                 _isPropsWinHovered = true;

//                 if (IsMouseButtonDown(MouseButton.Left)) _isPropsWinDragged = true;
//             }
//             else
//             {
//                 _isPropsWinHovered = false;
//             }

//             if (IsMouseButtonReleased(MouseButton.Left) && _isPropsWinDragged) _isPropsWinDragged = false;
            
//             if (menuOpened)
//             {
//                 var availableSpace = ImGui.GetContentRegionAvail();

//                 var halfWidth = availableSpace.X / 2f;
//                 var halfSize = new Vector2(halfWidth, 20);
                
//                 ImGui.Spacing();

//                 if (ImGui.Button($"Grid: {_showGrid}",
//                         availableSpace with { X = availableSpace.X / 2, Y = 20 }))
//                 {
//                     _showGrid = !_showGrid;
//                     _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                 }
                        
//                 ImGui.SameLine();
                
//                 var precisionSelected = ImGui.Button(
//                     $"Precision: {_snapMode switch { 0 => "Free", 1 => "Grid", 2 => "Precise", _ => "?" }}",
//                     availableSpace with { X = availableSpace.X / 2, Y = 20 });

//                 if (precisionSelected) _snapMode = ++_snapMode % 3;
                
//                 ImGui.Spacing();
                
//                 {
//                     if (ImGui.Button($"{(_noCollisionPropPlacement? "Continuous Placement" : "Single Placement")}", availableSpace with { Y = 20 }))
//                         _noCollisionPropPlacement = !_noCollisionPropPlacement;
                    
//                     ImGui.Spacing();
                    
//                     ImGui.SeparatorText("Categories");

//                     var quarterSpace = availableSpace with { X = availableSpace.X / 4f, Y = 20 };

//                     var tilesSelected = ImGui.Selectable("Tiles", _menuRootCategoryIndex == 0, ImGuiSelectableFlags.None, quarterSpace);
//                     ImGui.SameLine();
//                     var ropesSelected = ImGui.Selectable("Ropes", _menuRootCategoryIndex == 1, ImGuiSelectableFlags.None, quarterSpace);
//                     ImGui.SameLine();
//                     var longsSelected = ImGui.Selectable("Longs", _menuRootCategoryIndex == 2, ImGuiSelectableFlags.None, quarterSpace);
//                     ImGui.SameLine();
//                     var othersSelected = ImGui.Selectable("Others", _menuRootCategoryIndex == 3, ImGuiSelectableFlags.None, quarterSpace);

//                     if (tilesSelected) _menuRootCategoryIndex = 0;
//                     if (ropesSelected) _menuRootCategoryIndex = 1;
//                     if (longsSelected) _menuRootCategoryIndex = 2;
//                     if (othersSelected) _menuRootCategoryIndex = 3;

//                     var listSize = new Vector2(halfWidth, availableSpace.Y - 340);
                    
//                     switch (_menuRootCategoryIndex)
//                     {
//                         case 0: // Tiles-As-Props
//                         {
//                             if (GLOBALS.TileDex is null) break;

//                             ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
//                             var textChanged = ImGui.InputTextWithHint(
//                                 "##TileAsPropSearch", 
//                                 "Search tiles..", 
//                                 ref _tileAsPropSearchText, 
//                                 100, 
//                                 ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll
//                             );

//                             _isTileSearchActive = ImGui.IsItemActive();

//                             if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
//                                 ImGui.SetItemDefaultFocus();
//                                 ImGui.SetKeyboardFocusHere(-1);
//                             }
                            
//                             if (textChanged) {
//                                 SearchTiles();
//                             }

//                             if (ImGui.BeginListBox("##TileCategories", listSize))
//                             {
//                                 var drawList = ImGui.GetWindowDrawList();
//                                 var textHeight = ImGui.GetTextLineHeight();

//                                 // Not searching
//                                 if (_tileAsPropSearchResult is null) {
//                                     for (var index = 0; index < GLOBALS.TileDex.OrderedTileAsPropCategories.Length; index++)
//                                     {
//                                         var color = GLOBALS.TileDex?.GetCategoryColor(GLOBALS.TileDex.OrderedTileAsPropCategories[index]) ?? Vector4.Zero;

//                                         Vector4 colorVec = color;

//                                         var cursor = ImGui.GetCursorScreenPos();
//                                         drawList.AddRectFilled(
//                                             p_min: cursor,
//                                             p_max: cursor + new Vector2(10f, textHeight),
//                                             ImGui.ColorConvertFloat4ToU32(colorVec / 255f)
//                                         );

//                                         var selected = ImGui.Selectable("  " + GLOBALS.TileDex.OrderedTileAsPropCategories[index],
//                                             index == _propsMenuTilesCategoryIndex);
                                        
//                                         if (selected)
//                                         {
//                                             _propsMenuTilesCategoryIndex = index;
//                                             Utils.Restrict(
//                                                 ref _propsMenuTilesIndex, 
//                                                 0, 
//                                                 GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length-1);
//                                             _currentTile =
//                                                 GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][
//                                                     _propsMenuTilesIndex];
//                                         }
//                                     }
//                                 }
//                                 // Searching
//                                 else {
//                                     for (var c = 0; c < _tileAsPropSearchResult.Categories.Length; c++) {
//                                         var (name, originalIndex) = _tileAsPropSearchResult.Categories[c];

//                                         var color = GLOBALS.TileDex?.GetCategoryColor(GLOBALS.TileDex.OrderedTileAsPropCategories[originalIndex]) ?? Vector4.Zero;

//                                         Vector4 colorVec = color;

//                                         var cursor = ImGui.GetCursorScreenPos();
//                                         drawList.AddRectFilled(
//                                             p_min: cursor,
//                                             p_max: cursor + new Vector2(10f, textHeight),
//                                             ImGui.ColorConvertFloat4ToU32(colorVec / 255f)
//                                         );

//                                         var selected = ImGui.Selectable("  " + name, _tileAsPropSearchCategoryIndex == c);

//                                         if (selected) {
//                                             _tileAsPropSearchCategoryIndex = c;
//                                             _tileAsPropSearchIndex = -1;

//                                             _propsMenuTilesCategoryIndex = originalIndex;
//                                             Utils.Restrict(
//                                                 ref _propsMenuTilesIndex, 
//                                                 0, 
//                                                 GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length-1);
//                                             _currentTile =
//                                                 GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][
//                                                     _propsMenuTilesIndex];
//                                         }
//                                     }
//                                 }
//                                 ImGui.EndListBox();
//                             }
                            
//                             ImGui.SameLine();

//                             if (ImGui.BeginListBox("##Tiles", listSize))
//                             {
//                                 // Not searching
//                                 if (_tileAsPropSearchResult is null) {
//                                     for (var index = 0; index < GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex].Length; index++)
//                                     {
//                                         var currentTilep =
//                                             GLOBALS.TileDex.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][index];
                                        
//                                         var selected = ImGui.Selectable(
//                                             currentTilep.Name, 
//                                             index == _propsMenuTilesIndex
//                                         );
                                        
//                                         if (ImGui.IsItemHovered())
//                                         {
//                                             if (_hoveredCategoryIndex != _propsMenuTilesCategoryIndex ||
//                                                 _hoveredIndex != index ||
//                                                 _previousRootCategory != _menuRootCategoryIndex)
//                                             {
//                                                 _hoveredCategoryIndex = _propsMenuTilesCategoryIndex;
//                                                 _hoveredIndex = index;
//                                                 _previousRootCategory = _menuRootCategoryIndex;

//                                                 _hoveredTile =
//                                                     GLOBALS.TileDex.OrderedTilesAsProps[_hoveredCategoryIndex][
//                                                         _hoveredIndex];
                                                
//                                                 UpdatePropTooltip();
//                                             }
                                            
//                                             ImGui.BeginTooltip();
//                                             rlImGui.ImageRenderTexture(_propTooltip);
//                                             ImGui.EndTooltip();
//                                         }
                                        
//                                         if (selected)
//                                         {
//                                             _mode = 1;
//                                             _propsMenuTilesIndex = index;
//                                             _currentTile = currentTilep;
//                                         }
//                                     }
//                                 }
//                                 // Searching
//                                 else if (_tileAsPropSearchResult.Tiles.Length > 0) {
//                                     for (var t = 0; t < _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex].Length; t++) {
//                                         var (tile, originalIndex) = _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex][t];

//                                         var selected = ImGui.Selectable(tile.Name, _tileAsPropSearchIndex == t);

//                                         if (selected) {
//                                             _mode = 1;
//                                             _tileAsPropSearchIndex = t;
//                                             _propsMenuTilesIndex = originalIndex;
//                                             _propsMenuTilesCategoryIndex = _tileAsPropSearchResult.Categories[_tileAsPropSearchCategoryIndex].originalIndex;
//                                             _currentTile = tile;
//                                         }

//                                         if (ImGui.IsItemHovered())
//                                         {
//                                             if (_hoveredCategoryIndex != _propsMenuTilesCategoryIndex ||
//                                                 _hoveredIndex != originalIndex ||
//                                                 _previousRootCategory != _menuRootCategoryIndex)
//                                             {
//                                                 _hoveredCategoryIndex = _propsMenuTilesCategoryIndex;
//                                                 _hoveredIndex = originalIndex;
//                                                 _previousRootCategory = _menuRootCategoryIndex;

//                                                 _hoveredTile = tile;
                                                
//                                                 UpdatePropTooltip();
//                                             }
                                            
//                                             ImGui.BeginTooltip();
//                                             rlImGui.ImageRenderTexture(_propTooltip);
//                                             ImGui.EndTooltip();
//                                         }
//                                     }
//                                 }

//                                 ImGui.EndListBox();
//                             }
//                         }
//                             break;
//                         case 1: // Ropes
//                         {
//                             if (ImGui.BeginListBox("##Ropes", listSize))
//                             {
//                                 for (var index = 0; index < _ropeNames.Length; index++)
//                                 {
//                                     var selected = ImGui.Selectable(_ropeNames[index], index == _propsMenuRopesIndex);
                                    
//                                     if (selected) {
//                                         _mode = 1;
//                                         _propsMenuRopesIndex = index;
//                                     }
//                                 }
//                                 ImGui.EndListBox();
//                             }
//                         }
//                             break;
//                         case 2: // Longs
//                         {
//                             if (ImGui.BeginListBox("##Longs", listSize))
//                             {
//                                 for (var index = 0; index < _longNames.Length; index++)
//                                 {
//                                     var selected = ImGui.Selectable(_longNames[index], index == _propsMenuLongsIndex);
                                    
//                                     if (selected) {
//                                         _mode = 1;
//                                         _propsMenuLongsIndex = index;
//                                     }
//                                 }
//                                 ImGui.EndListBox();
//                             }
//                         }
//                             break;
//                         case 3: // Others
//                         {
//                             ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
//                             var textChanged = ImGui.InputTextWithHint(
//                                 "##OtherPropsSearch", 
//                                 "Search props..", 
//                                 ref _propSearchText, 
//                                 100, 
//                                 ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EscapeClearsAll
//                             );

//                             _isPropSearchActive = ImGui.IsItemActive();

//                             if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
//                                 ImGui.SetItemDefaultFocus();
//                                 ImGui.SetKeyboardFocusHere(-1);
//                             }

//                             if (textChanged) {
//                                 SearchProps();
//                             }

//                             if (ImGui.BeginListBox("##OtherPropCategories", listSize))
//                             {
//                                 var drawList = ImGui.GetWindowDrawList();
//                                 var textHeight = ImGui.GetTextLineHeight();

//                                 // Not searching 
//                                 if (_propSearchResult is null) {
//                                     for (var index = 0; index < _otherCategoryNames.Length; index++)
//                                     {
//                                         var color = GLOBALS.PropCategories[index].Item2;
//                                         var colorVec = new Vector4(color.R/255f, color.G/255f, color.B/255f, 1.0f);

//                                         var cursor = ImGui.GetCursorScreenPos();
                                        
//                                         drawList.AddRectFilled(
//                                             p_min: cursor,
//                                             p_max: cursor + new Vector2(10f, textHeight),
//                                             ImGui.ColorConvertFloat4ToU32(colorVec)
//                                         );

//                                         var selected = ImGui.Selectable("  " + _otherCategoryNames[index],
//                                             index == _propsMenuOthersCategoryIndex);
                                        
//                                         if (selected)
//                                         {
//                                             _propsMenuOthersCategoryIndex = index;
//                                             Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
//                                         }
//                                     }
//                                 } else {
//                                     for (var c = 0; c < _propSearchResult.Categories.Length; c++) {
//                                         var (categoryName, originalIndex, color) = _propSearchResult.Categories[c];

//                                         var colorVec = new Vector4(color.R/255f, color.G/255f, color.B/255f, 1.0f);

//                                         var cursor = ImGui.GetCursorScreenPos();
//                                         drawList.AddRectFilled(
//                                             p_min: cursor,
//                                             p_max: cursor + new Vector2(10f, textHeight),
//                                             ImGui.ColorConvertFloat4ToU32(colorVec)
//                                         );

//                                         var selected = ImGui.Selectable("  " + categoryName, _propSearchCategoryIndex == c);

//                                         if (selected) {
//                                             _propSearchCategoryIndex = c;
//                                             _propsMenuOthersCategoryIndex = originalIndex;
//                                             Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
//                                         }
//                                     }
//                                 }
//                                 ImGui.EndListBox();
//                             }
                            
//                             ImGui.SameLine();

//                             if (ImGui.BeginListBox("##OtherProps", listSize))
//                             {
//                                 // Not searching
//                                 if (_propSearchResult is null) {
//                                     var array = _otherNames[_propsMenuOthersCategoryIndex];

//                                     for (var index = 0; index < array.Length; index++)
//                                     {
//                                         var selected = ImGui.Selectable(array[index], index == _propsMenuOthersIndex);
                                        
//                                         if (ImGui.IsItemHovered())
//                                         {
//                                             if (_hoveredCategoryIndex != _propsMenuOthersCategoryIndex ||
//                                                 _hoveredIndex != index ||
//                                                 _previousRootCategory != _menuRootCategoryIndex)
//                                             {
//                                                 _hoveredCategoryIndex = _propsMenuOthersCategoryIndex;
//                                                 _hoveredIndex = index;
//                                                 _previousRootCategory = _menuRootCategoryIndex;
                                                
//                                                 UpdatePropTooltip();
//                                             }

//                                             ImGui.BeginTooltip();
//                                             rlImGui.ImageRenderTexture(_propTooltip);
//                                             ImGui.EndTooltip();
//                                         }
                                        
//                                         if (selected) {
//                                             _mode = 1;
//                                             _propsMenuOthersIndex = index;
//                                         }
//                                     }
//                                 } else if (_propSearchResult.Props.Length > 0) {
//                                     for (var p = 0; p < _propSearchResult.Props[_propSearchCategoryIndex].Length; p++) {
//                                         var (prop, originalIndex) = _propSearchResult.Props[_propSearchCategoryIndex][p];

//                                         var selected = ImGui.Selectable(prop.Name, _propSearchIndex == p);

//                                         if (selected) {
//                                             _propSearchIndex = p;
//                                             _mode = 1;
//                                             _propsMenuOthersIndex = originalIndex;
//                                             _propsMenuOthersCategoryIndex = _propSearchResult.Categories[_propSearchCategoryIndex].originalIndex;
//                                         }

//                                         if (ImGui.IsItemHovered())
//                                         {
//                                             if (_hoveredCategoryIndex != _propsMenuOthersCategoryIndex ||
//                                                 _hoveredIndex != originalIndex ||
//                                                 _previousRootCategory != _menuRootCategoryIndex)
//                                             {
//                                                 _hoveredCategoryIndex = _propsMenuOthersCategoryIndex;
//                                                 _hoveredIndex = originalIndex;
//                                                 _previousRootCategory = _menuRootCategoryIndex;
                                                
//                                                 UpdatePropTooltip();
//                                             }

//                                             ImGui.BeginTooltip();
//                                             rlImGui.ImageRenderTexture(_propTooltip);
//                                             ImGui.EndTooltip();
//                                         }
//                                     }
//                                 }
//                                 ImGui.EndListBox();
//                             }
//                         }
//                             break;
//                     }

//                     ImGui.SeparatorText("Placement Options");
                    
//                     // Seed

//                     ImGui.SetNextItemWidth(100);
//                     ImGui.InputInt("Seed", ref _defaultSeed);
                    
//                     // Rotation
                    
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Rotation", ref _placementRotation))
//                     {
//                         // _shouldRedrawLevel = true;
//                         _shouldRedrawPropLayer = true;
//                     }
                    
//                     ImGui.SetNextItemWidth(100);
//                     ImGui.SliderInt("Rotation Steps", ref _placementRotationSteps, 1, 10);
            
//                     // Depth
            
//                     var currentTile = GLOBALS.TileDex?.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
//                     var currentRope = GLOBALS.RopeProps[_propsMenuRopesIndex];
//                     var currentLong = GLOBALS.LongProps[_propsMenuLongsIndex];
//                     var currentOther = GLOBALS.Props[_propsMenuOthersCategoryIndex][_propsMenuOthersIndex];
            
//                     var depth = _defaultDepth;
                        
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Depth", ref depth))
//                     {
//                         _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     }

//                     Utils.Restrict(ref depth, -29, 0);

//                     _defaultDepth = depth;

//                     var propDepthTo = _menuRootCategoryIndex switch
//                     {
//                         0 => Utils.GetPropDepth(currentTile),
//                         1 => Utils.GetPropDepth(currentRope),
//                         2 => Utils.GetPropDepth(currentLong),
//                         3 => Utils.GetPropDepth(currentOther),
//                         _ => 0
//                     };
            
//                     ImGui.Text($"From {_defaultDepth} to {_defaultDepth - propDepthTo}");

//                     // Stretch

//                     ImGui.SetNextItemWidth(100);
//                     ImGui.InputFloat("Vertical Stretch", ref _defaultStretch.Y);
                    
//                     ImGui.SetNextItemWidth(100);
//                     ImGui.InputFloat("Horizontal Stretch", ref _defaultStretch.X);
            
//                     // Variation

//                     if (_menuRootCategoryIndex == 3 && currentOther is IVariableInit v)
//                     {
//                         var variations = v.Variations;

//                         if (variations > 1) {
//                             var variation = _defaultVariation;
                    
//                             ImGui.SetNextItemWidth(100);
//                             if (ImGui.InputInt("Variation", ref variation))
//                             {
//                                 // _shouldRedrawLevel = true;
//                                 _shouldRedrawPropLayer = true;
//                             }

//                             Utils.Restrict(ref variation, 0, variations-1);

//                             _defaultVariation = variation;
//                         } else {
//                             _defaultVariation = 0;
//                         }
//                     }
                    
//                     // Misc
                    
//                     ImGui.SeparatorText("Misc");

//                     ImGui.Checkbox("Tooltip", ref _tooltip);
//                 }

//                 ImGui.End();
//             }

//             //

//             var listOpened = ImGui.Begin("Props List##PropsListWindow");
            
//             var listPos = ImGui.GetWindowPos();
//             var listSpace = ImGui.GetWindowSize();

//             _isPropsListHovered = CheckCollisionPointRec(tileMouse, new(listPos.X - 5, listPos.Y, listSpace.X + 10, listSpace.Y));

//             if (listOpened)
//             {
//                 ImGui.SeparatorText("Placed Props");
                
//                 if (ImGui.Button("Select All", ImGui.GetContentRegionAvail() with { Y = 20 }))
//                 {
//                     _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
//                     for (var i = 0; i < _selected.Length; i++) _selected[i] = true;
//                 }

//                 if (ImGui.BeginListBox("Props", ImGui.GetContentRegionAvail() with { Y = ImGui.GetContentRegionAvail().Y - 350 }))
//                 {
//                     for (var index = 0; index < GLOBALS.Level.Props.Length; index++)
//                     {
//                         ref var currentProp = ref GLOBALS.Level.Props[index];
                        
//                         var selected = ImGui.Selectable(
//                             $"{index}. {currentProp.Name}{(_hidden[index] ? " [hidden]" : "")}", 
//                             _selected[index]);
                        
//                         if (selected)
//                         {
//                             _mode = 0;

//                             _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            
//                             if (IsKeyDown(KeyboardKey.LeftControl))
//                             {
//                                 _selected[index] = !_selected[index];
//                             }
//                             else if (IsKeyDown(KeyboardKey.LeftShift))
//                             {
//                                 var otherSelected = Array.IndexOf(_selected, true);
                                
//                                 if (otherSelected == -1) _selected = _selected.Select((p, i) => i == index).ToArray();

//                                 var first = Math.Min(otherSelected, index);
//                                 var second = Math.Max(otherSelected, index);

//                                 for (var i = 0; i < _selected.Length; i++)
//                                 {
//                                     _selected[i] = i >= first && i <= second;
//                                 }
//                             }
//                             else
//                             {
//                                 _selected = _selected.Select((p, i) => i == index).ToArray();
//                             }
//                         }
//                     }
                    
//                     ImGui.EndListBox();
//                 }

//                 var hideSelected = ImGui.Button("Hide Selected", ImGui.GetContentRegionAvail() with { Y = 20 });

//                 if (hideSelected)
//                 {
//                     _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
//                     for (var i = 0; i < _hidden.Length; i++)
//                     {
//                         if (_selected[i]) _hidden[i] = !_hidden[i];
//                     }
//                 }

//                 var deleteSelected = ImGui.Button("Delete Selected", ImGui.GetContentRegionAvail() with { Y = 20 });

//                 if (deleteSelected)
//                 {
//                     _shouldRedrawLevel = true;
//                     if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    
//                     GLOBALS.Level.Props = _selected
//                         .Select((s, i) => (s, i))
//                         .Where(v => !v.s)
//                         .Select(v => GLOBALS.Level.Props[v.i])
//                         .ToArray();

//                     _selected = new bool [GLOBALS.Level.Props.Length];
//                     _hidden = new bool[GLOBALS.Level.Props.Length]; // Update hidden


//                     fetchedSelected = GLOBALS.Level.Props
//                         .Select((prop, index) => (prop, index))
//                         .Where(p => _selected[p.index])
//                         .Select(p => p)
//                         .ToArray();

//                     ImportRopeModels();
//                 }

//                 //

//                 rlImGui.ImageRenderTexture(_propModeIndicatorsRT);

//                 //
                
//                 ImGui.SeparatorText("Selected Prop Options");
                
//                 if (fetchedSelected.Length == 1)
//                 {
//                     var (selectedProp, _) = fetchedSelected[0];

//                     // Render Time

//                     var renderTime = selectedProp.Extras.Settings.RenderTime;
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Render Time", ref renderTime)) {
//                         selectedProp.Extras.Settings.RenderTime = renderTime;
//                     }
                    
//                     // Render Order

//                     var renderOrder = selectedProp.Extras.Settings.RenderOrder;
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Render Order", ref renderOrder))
//                         selectedProp.Extras.Settings.RenderOrder = renderOrder;
            
//                     // Seed

//                     var seed = selectedProp.Extras.Settings.Seed;
            
//                     ImGui.SetNextItemWidth(100);
//                     ImGui.InputInt("Seed", ref seed);

//                     selectedProp.Extras.Settings.Seed = seed;
            
//                     // Depth
            
//                     ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

//                     var depth = selectedProp.Depth;
            
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Depth", ref depth))
//                     {
//                         _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     }
            
//                     Utils.Restrict(ref depth, -29, 0);
            
//                     selectedProp.Depth = depth;
            
//                     // Variation

//                     if (selectedProp.Extras.Settings is IVariable v)
//                     {
//                         var init = GLOBALS.Props[selectedProp.Position.category][selectedProp.Position.index];
//                         var variations = (init as IVariableInit)?.Variations ?? 1;

//                         if (variations > 1) {
//                             ImGui.SetNextItemWidth(100);
//                             var variation = v.Variation;
//                             if (ImGui.InputInt("Variation", ref variation))
//                             {
//                                 // _shouldRedrawLevel = true;
//                                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                                 else _shouldRedrawLevel = true;
                                
//                                 Utils.Restrict(ref variation, 0, variations -1);

//                                 v.Variation = variation;
//                             }
//                         } else {
//                             v.Variation = 0;
//                         }
//                     }
                    
//                     // Colored

//                     if (selectedProp.Type == InitPropType.VariedSoft && 
//                         selectedProp.Extras.Settings is PropVariedSoftSettings vs &&
//                         ((InitVariedSoftProp) GLOBALS.Props[selectedProp.Position.category][selectedProp.Position.index]).Colorize != 0)
//                     {
//                         var applyColor = vs.ApplyColor is 1;

//                         if (ImGui.Checkbox("Apply Color", ref applyColor))
//                         {
//                             vs.ApplyColor = applyColor ? 1 : 0;
//                         }
//                     }

//                     // Custom Depth

//                     if (selectedProp.Extras.Settings is ICustomDepth cd) {
//                         ImGui.SetNextItemWidth(100);

//                         var customDepth = cd.CustomDepth;

//                         if (ImGui.InputInt("Custom Depth", ref customDepth)) {
//                             cd.CustomDepth = customDepth;
//                         }
//                     }
                    
//                     // Rope
                    
//                     if (fetchedSelected.Length == 1 && fetchedSelected[0].prop.Type == InitPropType.Rope)
//                     {
//                         ImGui.SeparatorText("Rope Options");
                        
//                         //

//                         if (fetchedSelected[0].prop.Name == "Zero-G Tube")
//                         {
//                             var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.Extras.Settings);
//                             var applyColor = ropeSettings.ApplyColor is 1;

//                             if (ImGui.Checkbox("Apply Color", ref applyColor))
//                             {
//                                 ropeSettings.ApplyColor = applyColor ? 1 : 0;
//                             }
//                         }
//                         else if (fetchedSelected[0].prop.Name is "Wire" or "Zero-G Wire")
//                         {
//                             var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.Extras.Settings);
//                             var thickness = ropeSettings.Thickness ?? 2f;

//                             ImGui.SetNextItemWidth(100);
//                             var thicknessUpdated = ImGui.InputFloat("Thickness", ref thickness, 0.5f, 1f);
//                             if (thicknessUpdated)
//                             {
//                                 ropeSettings.Thickness = thickness;
//                             }
//                         }
                        
//                         var modelIndex = -1;

//                         for (var i = 0; i < _models.Length; i++)
//                         {
//                             if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
//                         }

//                         if (modelIndex == -1)
//                         {
// #if DEBUG
//                             Logger.Fatal(
//                                 $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
//                             throw new Exception(
//                                 message:
//                                 $"failed to fetch selected rope from {nameof(_models)}: no element with index [{fetchedSelected[0].index}] was found");
// #else
//                     goto ropeNotFound;
// #endif
//                         }

//                         var (modelOgIndex, simulate, model) = _models[modelIndex];

//                         var oldSegmentCount = GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints.Length;
//                         var segmentCount = oldSegmentCount;
                        
//                         var switchSimSelected = ImGui.Button(model.EditType switch { RopeModel.EditTypeEnum.BezierPaths => "Bezier Paths", RopeModel.EditTypeEnum.Simulation => "Simulation", _ => "Unknown" });

//                         if (switchSimSelected)
//                         {
//                             _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            
//                             model.EditType = model.EditType switch {
//                                 RopeModel.EditTypeEnum.BezierPaths => RopeModel.EditTypeEnum.Simulation,
//                                 RopeModel.EditTypeEnum.Simulation => RopeModel.EditTypeEnum.BezierPaths,
//                                 _ => RopeModel.EditTypeEnum.Simulation
//                             };

//                             _bezierHandleLock = -1;
//                         }

//                         ImGui.SetNextItemWidth(100);

//                         if (ImGui.InputInt("Segment Count", ref segmentCount))
//                         {
//                             _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         }

//                         // Update segment count if needed

//                         if (segmentCount < 1) segmentCount = 1;

//                         if (segmentCount > oldSegmentCount)
//                         {
//                             model.UpdateSegments([
//                                 ..GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints, new Vector2()
//                             ]);
//                         }
//                         else if (segmentCount < oldSegmentCount)
//                         {
//                             model.UpdateSegments(GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints[..^1]);
//                         }

//                         if (segmentCount != oldSegmentCount)
//                         {
//                             UpdateRopeModelSegments();
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else _shouldRedrawLevel = true;
//                         }
                        
//                         //

//                         if (ImGui.Checkbox("Simulate Rope", ref simulate))
//                         {
//                             // _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                             else _shouldRedrawLevel = true;

//                             _bezierHandleLock = -1;
//                         }


//                         if (model.EditType == RopeModel.EditTypeEnum.Simulation) // Simulation mode
//                         {
//                             var grav = model.Gravity;
//                             if (ImGui.Checkbox("Gravity", ref grav)) {
//                                 model.Gravity = grav;
//                             }
                            
//                             var cycleFpsSelected = ImGui.Button($"{60 / _ropeSimulationFrameCut} FPS");

//                             if (cycleFpsSelected)
//                             {
//                                 _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;
//                                 // _shouldRedrawLevel = true;
//                                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                                 else _shouldRedrawLevel = true;
//                             }

//                             var release = (fetchedSelected[0].prop.Extras.Settings as PropRopeSettings)?.Release ?? PropRopeRelease.None;

//                             var releaseClicked = ImGui.Button(release switch
//                             {
//                                 PropRopeRelease.Left => "Release Left",
//                                 PropRopeRelease.None => "Release None",
//                                 PropRopeRelease.Right => "Release Right",
//                                 _ => "Error"
//                             });

//                             if (releaseClicked)
//                             {
//                                 // _shouldRedrawLevel = true;
//                                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                                 else _shouldRedrawLevel = true;
                                
//                                 release = (PropRopeRelease)((int)release + 1);
//                                 if ((int)release > 2) release = 0;

//                                 (fetchedSelected[0].prop.Extras.Settings as PropRopeSettings)!.Release =release;
//                             }
//                         }
//                         else // Bezier mode
//                         {
//                             var oldHandlePointNumber = model.BezierHandles.Length;
//                             var handlePointNumber = oldHandlePointNumber;

//                             Utils.Restrict(ref handlePointNumber, 1);
                            
//                             ImGui.SetNextItemWidth(100);
//                             ImGui.InputInt("Control Points", ref handlePointNumber);


//                             if (handlePointNumber != oldHandlePointNumber) {
//                                 var quads = GLOBALS.Level.Props[modelOgIndex].Quad;
//                                 var center = Utils.QuadsCenter(ref quads);
        
//                                 Utils.Restrict(ref handlePointNumber, 1);

//                                 _shouldRedrawLevel = true;
//                                 if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                
//                                 if (handlePointNumber > oldHandlePointNumber)
//                                 {
//                                     model.BezierHandles = [..model.BezierHandles, center];
//                                 }
//                                 else if (handlePointNumber < oldHandlePointNumber)
//                                 {
//                                     model.BezierHandles = model.BezierHandles[..^1];
//                                 }
//                             }

//                         }

//                         _models[modelIndex] = (modelOgIndex, simulate, model);

//                         ropeNotFound:
//                         {
//                         }
//                     }
//                 }
//                 else if (fetchedSelected.Length > 1 && 
//                             Utils.AllEqual(fetchedSelected.Select(f => f.prop.Depth),
//                                 fetchedSelected[0].prop.Depth))
//                 {
//                     ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

//                     var depth = fetchedSelected[0].prop.Depth;
            
//                     ImGui.SetNextItemWidth(100);
//                     if (ImGui.InputInt("Depth", ref depth))
//                     {
//                         _shouldRedrawLevel = true;
//                         if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                     }
            
//                     Utils.Restrict(ref depth, -29, 0);
            
//                     foreach (var selected in fetchedSelected)
//                         selected.prop.Depth = depth;
//                 } else if (fetchedSelected.Length > 0) {
//                     if (ImGui.Button("Reset Quads", ImGui.GetContentRegionAvail() with { Y = 20 })) {
//                         foreach (var prop in GLOBALS.Level.Props) {
//                             prop.Quad = Utils.RotatePropQuads(prop.OriginalQuad, prop.Rotation);

//                             _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         }
//                     }

//                     if (ImGui.Button("Reset Rotation", ImGui.GetContentRegionAvail() with { Y = 20 })) {
//                         foreach (var prop in GLOBALS.Level.Props) {
//                             prop.Quad = Utils.RotatePropQuads(prop.Quad, -prop.Rotation);
//                             prop.Rotation = 0;

//                             _shouldRedrawLevel = true;
//                             if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
//                         }
//                     }
//                 }
                
//             }

//             // Shortcuts window
//             if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
//             {
//                 var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.PropsEditor);

//                 _isShortcutsWinHovered = CheckCollisionPointRec(
//                     tileMouse, 
//                     shortcutWindowRect with
//                     {
//                         X = shortcutWindowRect.X - 5, Width = shortcutWindowRect.Width + 10
//                     }
//                 );

//                 if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.Left))
//                 {
//                     _isShortcutsWinDragged = true;
//                 }
//                 else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.Left))
//                 {
//                     _isShortcutsWinDragged = false;
//                 }
//             }
            
//             rlImGui.End();
        
//         }
//         #endregion

//         // EndDrawing();
//         #endregion

//         // F3

//         Printers.Debug.EnqueueF3(new(GLOBALS.Level.Width) { Name = "LW", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.Level.Height) { Name = "LH" });

//         Printers.Debug.EnqueueF3(new(_defaultDepth) { Name = "PSL", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_placementRotation) { Name = "PR", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_defaultStretch.X) { Name = "HS", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_defaultStretch.Y) { Name = "VS" });
        
//         Printers.Debug.EnqueueF3(null);
        
//         Printers.Debug.EnqueueF3(new(_showTileLayer1) { Name = "Layer 1", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_showLayer1Tiles) { Name = "Tiles" });

//         Printers.Debug.EnqueueF3(new(_showTileLayer2) { Name = "Layer 2", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_showLayer2Tiles) { Name = "Tiles" });

//         Printers.Debug.EnqueueF3(new(_showTileLayer3) { Name = "Layer 3", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_showLayer3Tiles) { Name = "Tiles" });
        
//         Printers.Debug.EnqueueF3(null);
        
//         Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.Cameras) { Name = "Cameras", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.CamerasInnerBoundries) { Name = "Inner" });

//         Printers.Debug.EnqueueF3(null);

//         Printers.Debug.EnqueueF3(new("Coordinates: ") { SameLine = true });
//         Printers.Debug.EnqueueF3(new(tileMouseWorld) { SameLine = true });
        
//         Printers.Debug.EnqueueF3(new($"MX: {tileMatrixX} / MY: {tileMatrixY}"));
        
//         Printers.Debug.EnqueueF3(new(canDrawTile) { Name = "canDrawTile" });
//         Printers.Debug.EnqueueF3(new(inMatrixBounds) { Name = "inMatrixBounds" });

//         Printers.Debug.EnqueueF3(null);
        
//         Printers.Debug.EnqueueF3(new(_mode switch { 0 => "Selection", _ => "Placement" }) { Name = "Mode", SameLine = _mode == 0 });
        
//         if (_mode == 0) {
//             Printers.Debug.EnqueueF3(new(_movingProps) { Name = "M", SameLine = true });
//             Printers.Debug.EnqueueF3(new(_rotatingProps) { Name = "R", SameLine = true });
//             Printers.Debug.EnqueueF3(new(_scalingProps) { Name = "S", SameLine = true });
//             Printers.Debug.EnqueueF3(new(_stretchingProp) { Name = "V", SameLine = true });
//             Printers.Debug.EnqueueF3(new(_editingPropPoints) { Name = "P" });
//         }

//         Printers.Debug.EnqueueF3(new(GLOBALS.Props.Length) { Name = "Prop Categories", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.Props.Select(c => c.Length).Sum()) { Name = "Props", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.TileDex?.OrderedTileAsPropCategories.Length ?? 0) { Name = "Tile Categories", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.TileDex?.OrderedTilesAsProps.Select(c => c.Length).Sum() ?? 0) { Name = "Tiles", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.RopeProps.Length) { Name = "Ropes", SameLine = true });
//         Printers.Debug.EnqueueF3(new(GLOBALS.LongProps.Length) { Name = "Longs" });
        
//         Printers.Debug.EnqueueF3(new(_currentTile?.Name ?? "NULL") { Name = "Current Tile" });

//         Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.CrossLayerSelection) { Name = "CLS", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_additionalInitialRopeSegments) { Name = "ARS", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_editingPropPoints) { Name = "EPP", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_lockedPlacement) { Name = "LP", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_models.Length) { Name = "RM", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_newlyCopied) { Name = "NC" });
        
//         Printers.Debug.EnqueueF3(new(_isTileSearchActive) { Name = "TS", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_isPropSearchActive) { Name = "PS", SameLine = true });
        
//         Printers.Debug.EnqueueF3(new(_propsMenuTilesCategoryIndex) { Name = "TCI", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_propsMenuOthersCategoryIndex) { Name = "OCI" });

//         Printers.Debug.EnqueueF3(new(_propsMenuTilesIndex) { Name = "TI", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_propsMenuOthersIndex) { Name = "OI", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_propsMenuRopesIndex) { Name = "RI", SameLine = true });
//         Printers.Debug.EnqueueF3(new(_propsMenuLongsIndex) { Name = "LI" });
        
//         if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
//     }
// }
// using System.Numerics;
// using ImGuiNET;
// using Leditor.Data.Tiles;
// using rlImGui_cs;
// using static Raylib_cs.Raylib;
// using RenderTexture2D = Leditor.RL.Managed.RenderTexture2D;
// using Leditor.Types;
// using System.Drawing.Printing;


// namespace Leditor.Pages;

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
            if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (current.Depth <= -(GLOBALS.Layer+1)*10 || current.Depth > -GLOBALS.Layer*10)) continue;

            var (category, index) = current.Position;
            var quads = current.Quad;
            
            // origin must be the center
            // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
            
            // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
            Printers.DrawProp(
                category, 
                index, 
                current, 
                20, 
                GLOBALS.Settings.GeneralSettings.DrawPropMode, 
                GLOBALS.SelectedPalette,
                GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
            );

            // Draw Rope Point
            if (current.Type == InitPropType_Legacy.Rope)
            {
                foreach (var point in current.Extras.RopePoints)
                {
                    DrawCircleV(point, 3f, Color.White);
                }
            }
            
            if (_selected[p])
            {
                // Side Lines
                
                DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                
                // Quad Points

                if (_stretchingProp)
                {
                    if (current.Type == InitPropType_Legacy.Rope)
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
                    else if (current.Type == InitPropType_Legacy.Long)
                    {
                        var sides = Utils.LongSides(current.Quad);
                        
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
                                center with { X = GLOBALS.Level.Width*20 + 10 }, 
                                2f, 
                                Color.Red
                            );
                            break;
                        case 2:
                            DrawLineEx(
                                center with { Y = -10 },
                                center with { Y = GLOBALS.Level.Height*20 + 10 },
                                2f,
                                Color.Green
                            );
                            break;
                    }
                }
                
                // Draw Rope Point
                if (current.Type == InitPropType_Legacy.Rope)
                {
                    if (_editingPropPoints)
                    {
                        foreach (var point in current.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.Red);
                        }
                    }
                    else
                    {
                        foreach (var point in current.Extras.RopePoints)
                        {
                            DrawCircleV(point, 2f, Color.Orange);
                        }
                    }

                    var p1 = p;
                    var model = _models.SingleOrDefault(r => r.index == p1);

                    if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
                    {
                        foreach (var handle in model.model.BezierHandles) {
                            DrawCircleV(handle, 6f, Color.White);
                            DrawCircleV(handle, 4f, Color.Green);
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
                    ? new Data.Quad(
                        new Vector2(0, 0),
                        new Vector2(texture.Width, 0),
                        new Vector2(texture.Width, texture.Height),
                        new Vector2(0, texture.Height)
                    )
                    : new Data.Quad(
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
        var lWidth = GLOBALS.Level.Width * 20;
        var lHeight = GLOBALS.Level.Height * 20;

        var paletteTiles = GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette;

        BeginTextureMode(GLOBALS.Textures.GeneralLevel);
        ClearBackground(new(170, 170, 170, 255));
        EndTextureMode();
        
        #region TileEditorLayer3
        if (_showTileLayer3)
        {
            // Draw geos first
            if (paletteTiles) {
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 2, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

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
                    20, 
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
                    Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 2, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);

                } else {
                    BeginTextureMode(GLOBALS.Textures.GeneralLevel);

                    Printers.DrawTileLayer(
                        GLOBALS.Layer,
                        2, 
                        20, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
                        visibleStrays: false,
                        materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
                    );

                    EndTextureMode();
                }
            }
            
            // Then draw the props

            if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 2)) {

                BeginTextureMode(GLOBALS.Textures.GeneralLevel);


                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.Depth > -20) continue;

                    var (category, index) = current.Position;
                    var quads = current.Quad;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(
                        category, 
                        index, 
                        current, 
                        20, 
                        GLOBALS.Settings.GeneralSettings.DrawPropMode, 
                        GLOBALS.SelectedPalette,
                        GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
                    );

                    // Draw Rope Point
                    if (current.Type == InitPropType_Legacy.Rope)
                    {
                        foreach (var point in current.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.Type == InitPropType_Legacy.Rope)
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
                                        center with { X = GLOBALS.Level.Width*20 + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*20 + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.Type == InitPropType_Legacy.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            var p1 = p;
                            var model = _models.SingleOrDefault(r => r.index == p1);

                            if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
                            {
                                foreach (var handle in model.model.BezierHandles) {
                                    DrawCircleV(handle, 6f, Color.White);
                                    DrawCircleV(handle, 4f, Color.Green);
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
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 1, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

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
                    20, 
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
                    Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 1, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);
                    
                    
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
                        20, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
                        visibleStrays: false,
                        materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
                    );

                    EndTextureMode();
                }
            }

            BeginTextureMode(GLOBALS.Textures.GeneralLevel);

            
            // then draw the props

            if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette || GLOBALS.Layer != 1)) {
                for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                {
                    if (_hidden[p]) continue;
                    
                    var current = GLOBALS.Level.Props[p];
                    
                    // Filter based on depth
                    if (current.Depth > -10 || current.Depth < -19) continue;

                    var (category, index) = current.Position;
                    var quads = current.Quad;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(
                        category, 
                        index, 
                        current, 
                        20, 
                        GLOBALS.Settings.GeneralSettings.DrawPropMode, 
                        GLOBALS.SelectedPalette,
                        GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
                    );

                    // Draw Rope Point
                    if (current.Type == InitPropType_Legacy.Rope)
                    {
                        foreach (var point in current.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.Type == InitPropType_Legacy.Rope)
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
                                        center with { X = GLOBALS.Level.Width*20 + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*20 + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.Type == InitPropType_Legacy.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }
                            
                            var p1 = p;
                            var model = _models.SingleOrDefault(r => r.index == p1);

                            if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
                            {
                                foreach (var handle in model.model.BezierHandles) {
                                    DrawCircleV(handle, 6f, Color.White);
                                    DrawCircleV(handle, 4f, Color.Green);
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
                (-1) * 20,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 20,
                (GLOBALS.Level.Width + 2) * 20,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 20,
                new Color(0, 0, 255, (int)GLOBALS.Settings.GeneralSettings.WaterOpacity)
            );
            EndTextureMode();
        }

        #region TileEditorLayer1
        if (_showTileLayer1 && (GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers || GLOBALS.Layer == 0))
        {
            if (paletteTiles) {
                Printers.DrawGeoLayerWithMaterialsIntoBuffer(_tempGeoL, 0, 20, GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette);

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
                    20, 
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
                        Printers.DrawTileLayerWithPaletteIntoBuffer(GLOBALS.Textures.GeneralLevel, GLOBALS.Layer, 0, 20, GLOBALS.SelectedPalette!.Value, (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255), false, true, false);
                    
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
                        20, 
                        false, 
                        GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        (byte)(GLOBALS.Settings.GeneralSettings.HighLayerContrast ? 70 : 255),
                        visibleStrays: false,
                        materialColorSpace: GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace
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
                    if (!GLOBALS.Settings.PropEditor.CrossLayerSelection && (current.Depth < -9)) continue;

                    var (category, index) = current.Position;
                    var quads = current.Quad;
                    
                    // origin must be the center
                    // var origin = new Vector2(tl.X + (tr.X - tl.X)/2f, tl.Y + (bl.Y - tl.Y)/2f);
                    
                    // Printers.DrawProp(current.type, current.tile, category, index, current.prop, GLOBALS.Settings.PropEditor.TintedTextures);
                    Printers.DrawProp(
                        category, 
                        index, 
                        current, 
                        20, 
                        GLOBALS.Settings.GeneralSettings.DrawPropMode, 
                        GLOBALS.SelectedPalette,
                        GLOBALS.Settings.GeneralSettings.InverseBilinearInterpolation
                    );

                    // Draw Rope Point
                    if (current.Type == InitPropType_Legacy.Rope)
                    {
                        foreach (var point in current.Extras.RopePoints)
                        {
                            DrawCircleV(point, 3f, Color.White);
                        }
                    }
                    
                    if (_selected[p])
                    {
                        // Side Lines
                        
                        DrawRectangleLinesEx(Utils.EncloseQuads(current.Quad), 1.2f, Color.Blue);
                        
                        // Quad Points

                        if (_stretchingProp)
                        {
                            if (current.Type == InitPropType_Legacy.Rope)
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
                            else if (current.Type == InitPropType_Legacy.Long)
                            {
                                var sides = Utils.LongSides(current.Quad);
                                
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
                                        center with { X = GLOBALS.Level.Width*20 + 10 }, 
                                        2f, 
                                        Color.Red
                                    );
                                    break;
                                case 2:
                                    DrawLineEx(
                                        center with { Y = -10 },
                                        center with { Y = GLOBALS.Level.Height*20 + 10 },
                                        2f,
                                        Color.Green
                                    );
                                    break;
                            }
                        }
                        
                        // Draw Rope Point
                        if (current.Type == InitPropType_Legacy.Rope)
                        {
                            if (_editingPropPoints)
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 3f, Color.Red);
                                }
                            }
                            else
                            {
                                foreach (var point in current.Extras.RopePoints)
                                {
                                    DrawCircleV(point, 2f, Color.Orange);
                                }
                            }

                            var p1 = p;
                            var model = _models.SingleOrDefault(r => r.index == p1);

                            if (model.model?.EditType is RopeModel.EditTypeEnum.BezierPaths && model.simulate)
                            {
                                foreach (var handle in model.model.BezierHandles) {
                                    DrawCircleV(handle, 6f, Color.White);
                                    DrawCircleV(handle, 4f, Color.Green);
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
                (-1) * 20,
                (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel - GLOBALS.Level.Padding.bottom) * 20,
                (GLOBALS.Level.Width + 2) * 20,
                (GLOBALS.Level.WaterLevel + GLOBALS.Level.Padding.bottom) * 20,
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
    // private bool _ropeMode;

    // private bool _ropeModelGravity = true;

    private bool _noCollisionPropPlacement;

    private byte _stretchAxes;

    // 0 - 360
    private int _placementRotation;

    private bool _vFlipPlacement;
    private bool _hFlipPlacement;

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

    private bool _isTileSearchActive; 
    private bool _isPropSearchActive; 

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
        (string name, int originalIndex, Color color)[] Categories,
        (InitPropBase prop, int originalIndex)[][] Props
    );

    private PropSearchResult? _propSearchResult = null;

    private void SearchProps() {
        if (GLOBALS.PropCategories is null or { Length: 0 } || GLOBALS.Props is null or { Length: 0 } || string.IsNullOrEmpty(_propSearchText)) {
            _propSearchResult = null;
            _propSearchCategoryIndex = -1;
            return;
        }

        List<(string, int, Color)> categories = [];
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
                categories.Add((GLOBALS.PropCategories[c].Item1, c, GLOBALS.PropCategories[c].Item2));
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
    private Vector2 _defaultStretch = Vector2.Zero;

    private void UpdateDefaultDepth()
    {
        _defaultDepth = -GLOBALS.Layer * 10;
    }

    private Vector2? _propsMoveMouseAnchor;
    private Vector2? _propsMoveMousePos;
    private Vector2 _propsMoveMouseDelta = new(0, 0);
    
    private (int index, bool simulate, RopeModel model)[] _models = [];
    
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

    public void OnGlobalResourcesUpdated()
    {
        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
        else {
            _shouldRedrawLevel = true;
        }
    }

    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        var lWidth = GLOBALS.Level.Width * 20;
        var lHeight = GLOBALS.Level.Height * 20;

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
        var lWidth = GLOBALS.Level.Width * 20;
        var lHeight = GLOBALS.Level.Height * 20;

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

    public void OnPageUpdated(int previous, int @next) {
        if (@next == 8) {
            _shouldRedrawLevel = true;
            _shouldRedrawPropLayer = true;
            _defaultDepth = -10 * GLOBALS.Layer;
        }
    }

    private void ImportRopeModels()
    {
        if (GLOBALS.Level.Props.Length == 0) {
            _models = [];
            return;
        }

        var ropes = GLOBALS.Level.Props
            .Select((rope, index) => (rope, index))
            .Where(ropeInfo => ropeInfo.rope.Type == InitPropType_Legacy.Rope);

        if (!ropes.Any()) {
            _models = [];
            return;
        }

        if (_models.Length == 0) {
            List<(int, bool, RopeModel)> models = [];
        
            for (var r = 0; r < GLOBALS.Level.Props.Length; r++)
            {
                var current = GLOBALS.Level.Props[r];
                
                if (current.Type != InitPropType_Legacy.Rope) continue;
                
                var newModel = new RopeModel(current, GLOBALS.RopeProps[current.Position.index]);
            
                // var quad = current.prop.Quads;
                // var quadCenter = Utils.QuadsCenter(ref quad);

                models.Add((r, false, newModel));
            }

            _models = [..models];
        } else {
            var models = ropes.Select(ropeInfo => {
                var foundModel = _models.SingleOrDefault(m => m.index == ropeInfo.index);

                if (foundModel.model != null) return foundModel;

                var newModel = new RopeModel(ropeInfo.rope, GLOBALS.RopeProps[ropeInfo.rope.Position.index]);

                // var quad = ropeInfo.rope.prop.Quads;
                // var quadCenter = Utils.QuadsCenter(ref quad);

                return (ropeInfo.index, false, newModel);
            });

            _models = [..models];
        }
    }

    private void UpdateRopeModelSegments()
    {
        List<(int, bool, RopeModel)> models = [];
        
        for (var r = 0; r < _models.Length; r++)
        {
            var current = _models[r];
            
            models.Add((
                current.index, 
                false, 
                new RopeModel(
                    GLOBALS.Level.Props[current.index], 
                    GLOBALS.RopeProps[GLOBALS.Level.Props[current.index].Position.index])
                    {
                        BezierHandles = current.model.BezierHandles
                    }
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

        _currentTile ??= GLOBALS.TileDex?.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
        
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

        var previewScale = 20;

        var sWidth = GetScreenWidth();
        var sHeight = GetScreenHeight();
        
        var layer3Rect = GetLayerIndicator(2);
        var layer2Rect = GetLayerIndicator(1);
        var layer1Rect = GetLayerIndicator(0);

        var tileMouse = GetMousePosition();
        var tileMouseWorld = GetScreenToWorld2D(tileMouse, _camera);

        var menuPanelRect = new Rectangle(sWidth - 360, 0, 360, sHeight);

        //                        v this was done to avoid rounding errors
        var tileMatrixY = tileMouseWorld.Y < 0 ? -1 : (int)tileMouseWorld.Y / 20;
        var tileMatrixX = tileMouseWorld.X < 0 ? -1 : (int)tileMouseWorld.X / 20;

        var canDrawTile = !_isPropsListHovered && !_isNavbarHovered && 
                        !_isPropsWinHovered && 
                          !_isPropsWinDragged && 
                          !_isShortcutsWinHovered && 
                          !_isShortcutsWinDragged && 
                          !CheckCollisionPointRec(tileMouse, menuPanelRect) &&
                          !CheckCollisionPointRec(tileMouse, layer3Rect) &&
                          (GLOBALS.Layer != 1 || !CheckCollisionPointRec(tileMouse, layer2Rect)) &&
                          (GLOBALS.Layer != 0 || !CheckCollisionPointRec(tileMouse, layer1Rect));

        var isSearchBusy = _isTileSearchActive || _isPropSearchActive;

        var inMatrixBounds = tileMatrixX >= 0 && tileMatrixX < GLOBALS.Level.Width && tileMatrixY >= 0 && tileMatrixY < GLOBALS.Level.Height;

        GLOBALS.LockNavigation = _isTileSearchActive || _isPropSearchActive;

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

        // Undo

        if (!isSearchBusy && _shortcuts.Undo.Check(ctrl, shift, alt)) {
            Undo();
        }
        
        // Redo

        if (!isSearchBusy && _shortcuts.Redo.Check(ctrl, shift, alt)) {
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
        if (!isSearchBusy && _shortcuts.CycleLayers.Check(ctrl, shift, alt))
        {
            GLOBALS.Layer++;

            if (GLOBALS.Layer > 2) GLOBALS.Layer = 0;

            UpdateDefaultDepth();

            _shouldRedrawLevel = true;
            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
        }

        if (!isSearchBusy && _shortcuts.ToggleLayer1Tiles.Check(ctrl, shift, alt)) {
            _showLayer1Tiles = !_showLayer1Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (!isSearchBusy && _shortcuts.ToggleLayer2Tiles.Check(ctrl, shift, alt)) {
            _showLayer2Tiles = !_showLayer2Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (!isSearchBusy && _shortcuts.ToggleLayer3Tiles.Check(ctrl, shift, alt)) {
            _showLayer3Tiles = !_showLayer3Tiles;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        
        // Cycle Mode
        if (!isSearchBusy && _shortcuts.CycleModeRight.Check(ctrl, shift, alt))
        {
            _mode = ++_mode % 2;
        }
        else if (!isSearchBusy && _shortcuts.CycleModeLeft.Check(ctrl, shift, alt))
        {
            _mode--;
            if (_mode < 0) _mode = 1;
        }
        
        if (!isSearchBusy && _shortcuts.ToggleLayer1.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer1 = !_showTileLayer1;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (!isSearchBusy && _shortcuts.ToggleLayer2.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer2 = !_showTileLayer2;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }
        if (!isSearchBusy && _shortcuts.ToggleLayer3.Check(ctrl, shift, alt) && !_scalingProps) {
            _showTileLayer3 = !_showTileLayer3;
            _shouldRedrawLevel = true;
            // _shouldRedrawPropLayer = true;
        }

        if (!isSearchBusy && _shortcuts.CycleSnapMode.Check(ctrl, shift, alt)) _snapMode = ++_snapMode % 3;

        #region Placement & Selection Shortcuts
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
                    var (index, _, model) = _models.LastOrDefault();
                    
                    if (model is not null && index < GLOBALS.Level.Props.Length) {
                        if (_shortcuts.ToggleRopeGravity.Check(ctrl, shift, alt)) {
                            model.Gravity = !model.Gravity;
                        }

                        if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
                            _additionalInitialRopeSegments++;
                        }

                        if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt, true)) {
                            _additionalInitialRopeSegments--;
                            // Utils.Restrict(ref _additionalInitialRopeSegments, 0);
                        }

                        var current = GLOBALS.Level.Props[index];

                        var middleLeft = Raymath.Vector2Divide(
                            Raymath.Vector2Add(current.Quad.TopLeft, current.Quad.BottomLeft),
                            new(2f, 2f)
                        );

                        var middleRight = Raymath.Vector2Divide(
                            Raymath.Vector2Add(current.Quad.TopRight, current.Quad.BottomRight),
                            new(2f, 2f)
                        );
                        
                        // Attach vertex
                        {
                            var beta = Raymath.Vector2Angle(Raymath.Vector2Subtract(middleLeft, middleRight), new(1.0f, 0.0f));
                            
                            var r = Raymath.Vector2Length(Raymath.Vector2Subtract(current.Quad.TopLeft, middleLeft));
                        
                            var currentQuads = current.Quad;

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

                            current.Quad = currentQuads;
                        }

                        // Adjust segment count
                        var init = GLOBALS.Ropes[current.Position.index];
                        var endsDistance = (int)Raymath.Vector2Distance(middleLeft, middleRight) / (10 + init.SegmentLength);
                        if (endsDistance > 0 && ++_ropeSimulationFrame % 6 == 0) {
                            var ropePoints = current.Extras.RopePoints;
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

                        model?.Update(current.Quad, current.Depth switch
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
                    var currentQuads = GLOBALS.Level.Props[^1].Quad;
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

                    GLOBALS.Level.Props[^1].Quad = currentQuads;

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
                            1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
                            2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                if (_currentTile is null) break;
                                
                                // var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
                                // var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;

                                var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
                                var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;
                                
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

                                var placementQuad = new Data.Quad(
                                    new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
                                    new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
                                );

                                if (_vFlipPlacement) Utils.VFlipQuad(ref placementQuad);
                                if (_hFlipPlacement) Utils.HFlipQuad(ref placementQuad);

                                foreach (var prop in GLOBALS.Level.Props)
                                {
                                    var propRec = Utils.EncloseQuads(prop.Quad);
                                    var newPropRec = Utils.EncloseQuads(placementQuad);
                                    
                                    if (prop.Depth == _defaultDepth && CheckCollisionRecs(newPropRec, propRec)) goto skipPlacement;
                                }

                                var rotatedQuad = Utils.RotatePropQuads(placementQuad, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                        new Prop_Legacy(
                                            _defaultDepth, 
                                            _currentTile.Name, 
                                            rotatedQuad
                                        )
                                        {
                                            Extras = new PropExtras_Legacy(settings, []),
                                            Rotation = _placementRotation * _placementRotationSteps,
                                            OriginalQuad = placementQuad,
                                            Type = InitPropType_Legacy.Tile,
                                            Tile = _currentTile,
                                            Position = (-1, -1)
                                        }
                                ];
                            }
                                break;

                            case 1: // Ropes
                            {
                                if (_clickTracker) break;
                                _clickTracker = true;

                                if (_ropeInitialPlacement) {
                                    _ropeInitialPlacement = false;

                                } else {
                                    _ropeInitialPlacement = true;
                                    _additionalInitialRopeSegments = 0;
                                    
                                    var current = GLOBALS.RopeProps[_propsMenuRopesIndex];
                                    var newQuads = new Data.Quad
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
                                            new Prop_Legacy(
                                                _defaultDepth, 
                                                current.Name, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras_Legacy(
                                                    settings, 
                                                    ropePoints
                                                ),
                                                Type = InitPropType_Legacy.Rope,
                                                Tile = null,
                                                Position = (-1, _propsMenuRopesIndex)
                                            }
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
                                    var newQuads = new Data.Quad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
                                    };

                                    if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
                                    if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);
                                    
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
                                            new Prop_Legacy(
                                                _defaultDepth, 
                                                current.Name, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras_Legacy(settings, []),
                                                Type = InitPropType_Legacy.Long,
                                                Tile = null,
                                                Position = (-1, _propsMenuLongsIndex)
                                            }
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
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
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

                                var newQuads = new Data.Quad(
                                    new(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
                                    new(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
                                    new(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y), 
                                    new(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
                                );

                                if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
                                if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);

                                var rotatedQuad = Utils.RotatePropQuads(newQuads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                        new Prop_Legacy(
                                            _defaultDepth, 
                                            init.Name, 
                                            rotatedQuad
                                        )
                                        {
                                            Extras = new PropExtras_Legacy(settings, []),
                                            Rotation = _placementRotation * _placementRotationSteps,
                                            OriginalQuad = newQuads,
                                            Type = init.Type,
                                            Tile = null,
                                            Position = (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex)
                                        }
                                ];
                            }
                                break;
                        }
                        
                        if (GLOBALS.Level.Props.Length > 0 && 
                            GLOBALS.Level.Props[^1].Type != InitPropType_Legacy.Rope) _ropeInitialPlacement = false;

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
                            1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
                            2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
                            _ => tileMouseWorld
                        };

                        switch (_menuRootCategoryIndex)
                        {
                            case 0: // Tiles as props
                            {
                                var width = (float)(_currentTile.Size.Item1 + _currentTile.BufferTiles*2) * 20 / 2;
                                var height = (float)(_currentTile.Size.Item2 + _currentTile.BufferTiles*2) * 20 / 2;
                                
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

                                var quads = new Data.Quad(
                                    new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
                                    new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
                                );

                                if (_vFlipPlacement) Utils.VFlipQuad(ref quads);
                                if (_hFlipPlacement) Utils.HFlipQuad(ref quads);

                                var rotatedQuads = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                    
                                        new Prop_Legacy(
                                            _defaultDepth, 
                                            _currentTile.Name, 
                                            rotatedQuads
                                        )
                                        {
                                            Extras = new(settings, []),
                                            Rotation = _placementRotation * _placementRotationSteps,
                                            OriginalQuad = quads,
                                            Type = InitPropType_Legacy.Tile,
                                            Tile = _currentTile,
                                            Position = (-1, -1)
                                        }
                                    
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
                                    var newQuads = new Data.Quad
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
                                            new Prop_Legacy(
                                                _defaultDepth, 
                                                current.Name, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras_Legacy(
                                                    settings, 
                                                    ropePoints
                                                ),
                                                Type = InitPropType_Legacy.Rope,
                                                Tile = null,
                                                Position = (-1, _propsMenuRopesIndex)
                                            }
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
                                    var newQuads = new Data.Quad
                                    {
                                        TopLeft = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomLeft = new(tileMouseWorld.X, tileMouseWorld.Y + height),
                                        TopRight = new(tileMouseWorld.X, tileMouseWorld.Y - height),
                                        BottomRight = new(tileMouseWorld.X, tileMouseWorld.Y + height)
                                    };

                                    if (_vFlipPlacement) Utils.VFlipQuad(ref newQuads);
                                    if (_hFlipPlacement) Utils.HFlipQuad(ref newQuads);
                                    
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
                                            new Prop_Legacy(
                                                _defaultDepth, 
                                                current.Name, 
                                                newQuads
                                            )
                                            {
                                                Extras = new PropExtras_Legacy(settings, []),
                                                Rotation = _placementRotation,
                                                Type = InitPropType_Legacy.Long,
                                                Tile = null,
                                                Position = (-1, _propsMenuLongsIndex)
                                            }
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
                                    InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                    InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
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

                                var quads = new Data.Quad(
                                    new(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y),
                                    new(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y),
                                    new(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y)
                                );

                                if (_vFlipPlacement) Utils.VFlipQuad(ref quads);
                                if (_hFlipPlacement) Utils.HFlipQuad(ref quads);

                                var rotatedQuad = Utils.RotatePropQuads(quads, _placementRotation * _placementRotationSteps, tileMouseWorld);
                                
                                GLOBALS.Level.Props = [ .. GLOBALS.Level.Props,
                                        new Prop_Legacy(
                                            _defaultDepth, 
                                            init.Name, 
                                            rotatedQuad)
                                        {
                                            Extras = new PropExtras_Legacy(settings, []),
                                            Rotation = _placementRotation * _placementRotationSteps,
                                            OriginalQuad = quads,
                                            Type = init.Type,
                                            Tile = null,
                                            Position = (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex)
                                        }
                                ];
                            }
                                break;
                        }

                        if (GLOBALS.Level.Props.Length > 0 && 
                            GLOBALS.Level.Props[^1].Type != InitPropType_Legacy.Rope) _ropeInitialPlacement = false;
                        
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

                if (_shortcuts.ResetPlacementRotation.Check(ctrl, shift, alt)) {
                    _placementRotation = 0;
                }

                // Flipping Burgers

                if (_shortcuts.VerticalFlipPlacement.Check(ctrl, shift, alt)) {
                    _vFlipPlacement = !_vFlipPlacement;
                }

                if (_shortcuts.HorizontalFlipPlacement.Check(ctrl, shift, alt)) {
                    _hFlipPlacement = !_hFlipPlacement;
                }

                // 90 Degree Rotation

                if (_shortcuts.RotateRightAnglePlacement.Check(ctrl, shift, alt)) {
                    _placementRotation += 90;

                    Utils.Cycle(ref _placementRotation, 0, 360);
                }

                // Continuous Placement
                if (_shortcuts.ToggleNoCollisionPropPlacement.Check(ctrl, shift, alt)) {
                    _noCollisionPropPlacement = !_noCollisionPropPlacement;
                }

                // Activate Selection Mode Via Mouse
                if (_shortcuts.SelectProps.Button != _shortcuts.PlaceProp.Button) {
                    if (!_isPropsListHovered && !_isPropsWinHovered && (_shortcuts.SelectProps.Check(ctrl, shift, alt, true) || _shortcuts.SelectPropsAlt.Check(ctrl, shift, alt, true)) && !_clickTracker && canDrawTile)
                    {
                        _selection1 = GetScreenToWorld2D(GetMousePosition(), _camera);
                        _clickTracker = true;
                        _mode = 0;
                        _ropeInitialPlacement = false;
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
                            var propSelectRect = Utils.EncloseQuads(current.Quad);
                            if (_shortcuts.PropSelectionModifier.Check(ctrl, shift, alt, true))
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10))
                                {
                                    _selected[i] = !_selected[i];
                                }
                            }
                            else
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10))
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

                // Stretch Placement

                if (_shortcuts.StretchPlacementHorizontal.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.X += 1;
                } else if (_shortcuts.StretchPlacementVertical.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.Y += 1;
                } else if (_shortcuts.SqueezePlacementHorizontal.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.X -= 1;
                } else if (_shortcuts.SqueezePlacementVertical.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.Y -= 1;
                } else if (_shortcuts.FastStretchPlacementHorizontal.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.X += 4;
                } else if (_shortcuts.FastStretchPlacementVertical.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.Y += 4;
                } else if (_shortcuts.FastSqueezePlacementHorizontal.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.X -= 4;
                } else if (_shortcuts.FastSqueezePlacementVertical.Check(ctrl, shift, alt, true)) {
                    _defaultStretch.Y -= 4;
                }

                if (_shortcuts.ResetPlacementStretch.Check(ctrl, shift, alt)) {
                    _defaultStretch = Vector2.Zero;
                }
                
                // Pickup Prop
                if (_shortcuts.PickupProp.Check(ctrl, shift, alt))
                {
                    for (var i = 0; i < GLOBALS.Level.Props.Length; i++)
                    {
                        var current = GLOBALS.Level.Props[i];
    
                        if (!CheckCollisionPointRec(tileMouseWorld, Utils.EncloseQuads(current.Quad)) || 
                            ((current.Depth <= (GLOBALS.Layer + 1) * -10 || 
                            current.Depth > GLOBALS.Layer * -10) && !GLOBALS.Settings.PropEditor.CrossLayerSelection)) 
                            continue;

                        if (current.Type == InitPropType_Legacy.Tile && GLOBALS.TileDex is not null)
                        {
                            for (var c = 0; c < GLOBALS.TileDex.OrderedTilesAsProps.Length; c++)
                            {
                                for (var p = 0; p < GLOBALS.TileDex.OrderedTilesAsProps[c].Length; p++)
                                {
                                    var currentTileAsProp = GLOBALS.TileDex.OrderedTilesAsProps[c][p];

                                    if (currentTileAsProp.Name != current.Name) continue;

                                    _propsMenuTilesCategoryIndex = c;
                                    _propsMenuTilesIndex = p;

                                    _copiedPropSettings = current.Extras.Settings;
                                    _copiedIsTileAsProp = true;
                                }
                            }
                        }
                        else if (current.Type == InitPropType_Legacy.Rope)
                        {
                            _copiedRopePoints = [..current.Extras.RopePoints];
                            _copiedPropSettings = current.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else if (current.Type == InitPropType_Legacy.Long)
                        {
                            _copiedPropSettings = current.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }
                        else
                        {
                            (_propsMenuOthersCategoryIndex, _propsMenuOthersIndex) = current.Position;
                            _copiedPropSettings = current.Extras.Settings;
                            _copiedIsTileAsProp = false;
                        }

                        _copiedDepth = current.Depth;
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
                    _selectedPropsEncloser = Utils.EncloseProps(fetchedSelected.Select(p => p.prop.Quad));
                    _selectedPropsCenter = new Vector2(
                        _selectedPropsEncloser.X + 
                        _selectedPropsEncloser.Width/2, 
                        _selectedPropsEncloser.Y + 
                        _selectedPropsEncloser.Height/2
                    );

                    if (fetchedSelected[0].prop.Type == InitPropType_Legacy.Rope) {
                        if (_shortcuts.ToggleRopeGravity.Check(ctrl, shift, alt)) {
                            for (var i = 0; i < _models.Length; i++)
                            {
                                if (_models[i].index == fetchedSelected[0].index) {
                                    _models[i].model.Gravity = !_models[i].model.Gravity;
                                    break;
                                }
                            }
                        }
                        
                        if (_shortcuts.SimulationBeizerSwitch.Check(ctrl, shift, alt)) {
                            
                            // Find the rope model
                            var modelIndex = -1;

                            for (var i = 0; i < _models.Length; i++)
                            {
                                if (_models[i].index == fetchedSelected[0].index) modelIndex = i;
                            }

                            if (modelIndex != -1) {
                                ref var currentModel = ref _models[modelIndex];
                                currentModel.model.EditType = (RopeModel.EditTypeEnum)(((int)currentModel.model.EditType + 1) % 2);
                            }
                        }

                        var foundRope = _models.SingleOrDefault(rope => rope.index == fetchedSelected[0].index);

                        if (_shortcuts.IncrementRopSegmentCount.Check(ctrl, shift, alt)) {
                            foundRope.model.UpdateSegments([..fetchedSelected[0].prop.Extras.RopePoints, new Vector2(0, 0)]);

                            // ImportRopeModels();
                        }

                        if (_shortcuts.DecrementRopSegmentCount.Check(ctrl, shift, alt)) {
                            foundRope.model.UpdateSegments(fetchedSelected[0].prop.Extras.RopePoints.Take(fetchedSelected[0].prop.Extras.RopePoints.Length - 1).ToArray());

                            // ImportRopeModels();
                        }

                    }
                
                    if (!isSearchBusy && _shortcuts.ResetQuadVertices.Check(ctrl, shift, alt)) {
                        for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                        {
                            if (!_selected[p]) continue;
                            
                            GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(GLOBALS.Level.Props[p].OriginalQuad, GLOBALS.Level.Props[p].Rotation);
                        }

                        _shouldRedrawPropLayer = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
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
                if (!isSearchBusy && _shortcuts.RotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = 0.3f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation += degree;
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (!isSearchBusy && _shortcuts.RotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = -0.3f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation += degree;
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (!isSearchBusy && _shortcuts.FastRotateClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = 2f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation += degree;
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (!isSearchBusy && _shortcuts.FastRotateCounterClockwise.Check(ctrl, shift, alt, true) && anySelected) {
                    const float degree = -2f;

                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, degree, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(degree, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation += degree;
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }

                if (!isSearchBusy && _shortcuts.ResetPlacementRotation.Check(ctrl, shift, alt) && anySelected) {
                    for (var p = 0; p < GLOBALS.Level.Props.Length; p++)
                    {
                        if (!_selected[p]) continue;
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, -GLOBALS.Level.Props[p].Rotation, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(-_placementRotation, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation = 0;
                    }

                    _shouldRedrawPropLayer = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Palette) _shouldRedrawLevel = true;
                }
                #endregion

                #region ActivateModes
                if (!isSearchBusy) {
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
                        // _ropeMode = false;
                        
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
                        fetchedSelected[0].prop.Type == InitPropType_Legacy.Rope
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
                            // _ropeMode = false;
                            _shouldUpdateModeIndicatorsRT = true;

                            if (!_editingPropPoints) {
                                _gram.Proceed(GLOBALS.Level.Props);
                            }

                            _bezierHandleLock = -1;
                        }
                        // Rope mode
                        else if (_shortcuts.ToggleRopeEditingMode.Check(ctrl, shift, alt))
                        {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else {
                                _shouldRedrawLevel = true;
                            }

                            _bezierHandleLock = -1;

                            // _scalingProps = false;
                            // _movingProps = false;
                            // _rotatingProps = false;
                            // _stretchingProp = false;
                            // _editingPropPoints = false;
                            // _ropeMode = !_ropeMode;

                            if (fetchedSelected.Length == 1 && _models.Length > 0) {
                                var found = Array.FindIndex(_models, (m) => m.index == fetchedSelected[0].index);

                                if (found != -1) { 
                                    _models[found].simulate = !_models[found].simulate;
                                    if (!_models[found].simulate) {
                                        _gram.Proceed(GLOBALS.Level.Props);
                                        _bezierHandleLock = -1; // Must be set!
                                    };
                                }
                            }
                            _shouldUpdateModeIndicatorsRT = true;
                        }
                        else if (_shortcuts.DuplicateProps.Check(ctrl, shift, alt)) {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else {
                                _shouldRedrawLevel = true;
                            }

                            _bezierHandleLock = -1;

                            List<Prop_Legacy> dProps = [];
                        
                            foreach (var (prop, _) in fetchedSelected)
                            {
                                dProps.Add(new Prop_Legacy(
                                        prop.Depth,
                                        prop.Name,
                                        new Data.Quad(
                                            prop.Quad.TopLeft,
                                            prop.Quad.TopRight,
                                            prop.Quad.BottomRight,
                                            prop.Quad.BottomLeft
                                        ))
                                    {
                                        Extras = new PropExtras_Legacy(
                                            prop.Extras.Settings.Clone(),
                                            [..prop.Extras.RopePoints]),

                                            Type = prop.Type,
                                            Tile = prop.Tile,
                                            Position = prop.Position
                                    }
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

                        
                        List<Prop_Legacy> dProps = [];
                        
                        foreach (var (prop, _) in fetchedSelected)
                        {
                            dProps.Add(new Prop_Legacy(
                                    prop.Depth,
                                    prop.Name,
                                    new Data.Quad(
                                        prop.Quad.TopLeft,
                                        prop.Quad.TopRight,
                                        prop.Quad.BottomRight,
                                        prop.Quad.BottomLeft
                                    ))
                                {
                                    Extras = new PropExtras_Legacy(
                                        prop.Extras.Settings.Clone(),
                                        [..prop.Extras.RopePoints]),
                                    Type = prop.Type,
                                    Tile = prop.Tile,
                                    Position = prop.Position
                                }
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
                }
                #endregion

                // Simulate ropes
                if (_models.Length > 0)
                {
                    _shouldUpdateModeIndicatorsRT = true;

                    // _shouldRedrawLevel = true;
                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    else {
                        _shouldRedrawLevel = true;
                    }

                    for (var i = 0; i < _models.Length; i++) {
                        ref var current = ref _models[i];

                        if (current.simulate && ++_ropeSimulationFrame % _ropeSimulationFrameCut == 0) {
                            #if DEBUG
                            if (current.index < 0 || 
                            current.index >= GLOBALS.Level.Props.Length || 
                            GLOBALS.Level.Props[current.index].Type != InitPropType_Legacy.Rope) {
                                throw new Exception("model's reference index was invalid or out of bounds");
                            }
                            #endif
                            
                            var prop = GLOBALS.Level.Props[current.index]; // potential exception

                            if (current.model.EditType == RopeModel.EditTypeEnum.Simulation) {
                                current.model.Update(
                                    prop.Quad,
                                    prop.Depth switch {
                                        < -19 => 2,
                                        < -9 => 1,
                                        _ => 0
                                    }
                                );
                            } else if (current.model.EditType == RopeModel.EditTypeEnum.BezierPaths) {
                                var (pA, pB) = Utils.RopeEnds(fetchedSelected[0].prop.Quad);
                                
                                fetchedSelected[0].prop.Extras.RopePoints = Utils.Casteljau(fetchedSelected[0].prop.Extras.RopePoints.Length, [ pA, ..current.model.BezierHandles, pB ]);

                                if ((IsMouseButtonPressed(_shortcuts.SelectProps.Button) || IsKeyPressed(_shortcuts.SelectPropsAlt.Key)) && _bezierHandleLock != -1)
                                    _bezierHandleLock = -1;

                                if (IsMouseButtonDown(_shortcuts.SelectProps.Button))
                                {
                                    for (var b = 0; b < current.model.BezierHandles.Length; b++)
                                    {
                                        if (_bezierHandleLock == -1 && CheckCollisionPointCircle(tileMouseWorld, current.model.BezierHandles[b], 5f))
                                            _bezierHandleLock = b;

                                        if (_bezierHandleLock == b) current.model.BezierHandles[b] = tileMouseWorld;
                                    }
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

                    // Vector2 gridDelta, preciseDelta;

                    var deltaFactor = _snapMode switch {
                        1 => 20,
                        2 => 10,
                        _ => 1
                    };

                    var gridScaled = new Vector2((int)(_propsMoveMousePos.Value.X / deltaFactor), (int)(_propsMoveMousePos.Value.Y / deltaFactor));
                    var gridScaledBack = gridScaled * deltaFactor;
                    
                    var newGridScaled = new Vector2((int)(newMousePos.X / deltaFactor), (int)(newMousePos.Y / deltaFactor));
                    var newGridScaledBack = newGridScaled * deltaFactor;

                    var delta = newGridScaledBack - gridScaledBack;

                    _propsMoveMousePos = newMousePos;
                    
                    // Fix delta when level panning
                    
                    if (IsMouseButtonDown(_shortcuts.DragLevel.Button))
                        _propsMoveMouseDelta = new Vector2(0, 0);

                    for (var s = 0; s < _selected.Length; s++)
                    {
                        if (!_selected[s]) continue;
                        
                        var quads = GLOBALS.Level.Props[s].Quad;
                        var center = Utils.QuadsCenter(ref quads);

                        Vector2 deltaToAdd = _snapMode switch {
                            1 or 2 => delta,

                            _ => _propsMoveMouseDelta
                        };

                        quads.TopLeft = Raymath.Vector2Add(quads.TopLeft, deltaToAdd);
                        quads.TopRight = Raymath.Vector2Add(quads.TopRight, deltaToAdd);
                        quads.BottomRight = Raymath.Vector2Add(quads.BottomRight, deltaToAdd);
                        quads.BottomLeft = Raymath.Vector2Add(quads.BottomLeft, deltaToAdd);

                        GLOBALS.Level.Props[s].Quad = quads;

                        if (GLOBALS.Level.Props[s].Type == InitPropType_Legacy.Rope)
                        {
                            for (var r = 0; r < _models.Length; r++)
                            {
                                var (propIndex, simulated, model) = _models[r];

                                if (propIndex == s)
                                {
                                    var segments = GLOBALS.Level.Props[s].Extras.RopePoints;
                                    if (!simulated) {
                                        for (var p = 0; p < segments.Length; p++)
                                        {
                                            segments[p] = segments[p] + deltaToAdd;
                                        }

                                        GLOBALS.Level.Props[propIndex].Extras.RopePoints = segments;
                                    }

                                    for (var h = 0; h < _models[r].model.BezierHandles.Length; h++)
                                    {
                                        _models[r].model.BezierHandles[h] += deltaToAdd;
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
                        
                        var quads = GLOBALS.Level.Props[p].Quad;

                        GLOBALS.Level.Props[p].Quad = Utils.RotatePropQuads(quads, delta.X, _selectedPropsCenter);

                        if (GLOBALS.Level.Props[p].Type == InitPropType_Legacy.Rope)
                        {
                            Utils.RotatePoints(delta.X, _selectedPropsCenter, GLOBALS.Level.Props[p].Extras.RopePoints);
                        }

                        GLOBALS.Level.Props[p].Rotation += delta.X;
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
                            var enclose = Utils.EncloseProps(fetchedSelected.Select(s => s.prop.Quad));
                            var center = Utils.RectangleCenter(ref enclose);

                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.Quad;
                                Utils.ScaleQuads(ref quads, center, 1f + delta.X*0.01f);
                                GLOBALS.Level.Props[selected.index].Quad = quads;
                            }
                        }
                            break;

                        case 1: // X-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.Quad;
                                var center = Utils.QuadsCenter(ref quads);
                                
                                Utils.ScaleQuadsX(ref quads, center, 1f + delta.X * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].Quad = quads;
                            }
                        }
                            break;

                        case 2: // Y-axes Scaling
                        {
                            foreach (var selected in fetchedSelected)
                            {
                                var quads = selected.prop.Quad;
                                var center = Utils.QuadsCenter(ref quads);

                                Utils.ScaleQuadsY(ref quads, center, 1f - delta.Y * 0.01f);
                                
                                GLOBALS.Level.Props[selected.index].Quad = quads;
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

                    
                    var currentQuads = fetchedSelected[0].prop.Quad; 
                    
                    var posV = _snapMode switch
                    {
                        1 => new Vector2((int)(tileMouseWorld.X / 20f), (int)(tileMouseWorld.Y / 20f)) * 20f,
                        2 => new Vector2((int)(tileMouseWorld.X / 10f), (int)(tileMouseWorld.Y / 10f)) * 10f,
                        _ => tileMouseWorld
                    };

                    if (IsMouseButtonDown(_shortcuts.SelectProps.Button) || IsKeyDown(_shortcuts.SelectPropsAlt.Key))
                    {
                        if (fetchedSelected[0].prop.Type == InitPropType_Legacy.Rope)
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
                        else if (fetchedSelected[0].prop.Type == InitPropType_Legacy.Long)
                        {
                            var (left, top, right, bottom) = Utils.LongSides(fetchedSelected[0].prop.Quad);
                            
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
                        
                        GLOBALS.Level.Props[fetchedSelected[0].index].Quad = currentQuads;
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

                    
                    var points = fetchedSelected[0].prop.Extras.RopePoints;

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
                                selected.prop.Depth--;

                                if (selected.prop.Depth < -29) selected.prop.Depth = 29;
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
                                selected.prop.Depth++;

                                if (selected.prop.Depth > 0) selected.prop.Depth = 0;
                            }
                        }
                    
                        if (_shortcuts.CycleVariations.Check(ctrl, shift, alt)) {
                            foreach (var selected in fetchedSelected) {
                                if (selected.prop.Extras.Settings is IVariable vs) {
                                    var (category, index) = selected.prop.Position;

                                    var variations = (GLOBALS.Props[category][index] as IVariableInit)?.Variations ?? 1;
                                
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
                            var propSelectRect = Utils.EncloseQuads(current.Quad);
                            
                            if (IsKeyDown(_shortcuts.PropSelectionModifier.Key))
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && (GLOBALS.Settings.PropEditor.CrossLayerSelection || !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10)))
                                {
                                    _selected[i] = !_selected[i];
                                }
                            }
                            else
                            {
                                if (CheckCollisionRecs(propSelectRect, _selection) && (GLOBALS.Settings.PropEditor.CrossLayerSelection || !(current.Depth <= (GLOBALS.Layer + 1) * -10 || current.Depth > GLOBALS.Layer * -10)))
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

                        // Auto-simulate ropes
                        if (GLOBALS.Settings.PropEditor.SimulateSelectedRopes) {
                            for (var i = 0; i < _models.Length; i++) {
                                _models[i].simulate = _selected[_models[i].index];
                            }
                        }

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
        #endregion

        #region TileEditorDrawing
        // BeginDrawing();

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
            DrawRectangleLinesEx(new Rectangle(-3, -3, GLOBALS.Level.Width * 20 + 6, GLOBALS.Level.Height * 20 + 6), 3, Color.White);
            
            BeginShaderMode(GLOBALS.Shaders.VFlip);
            SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
            DrawTexturePro(GLOBALS.Textures.GeneralLevel.Texture, 
                new Rectangle(0, 0, GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20), 
                new Rectangle(0, 0, GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20), 
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
                Printers.DrawGrid(20);
            }
            
            if (GLOBALS.Settings.GeneralSettings.DarkTheme)
            {
                DrawRectangleLines(0, 0, GLOBALS.Level.Width*20, GLOBALS.Level.Height*20, Color.White);
            }
            
            if (GLOBALS.Settings.PropEditor.Cameras) {
                var counter = 0;

                foreach (var cam in GLOBALS.Level.Cameras)
                {
                    var critRect = Utils.CameraCriticalRectangle(cam.Coords);

                    DrawRectangleLinesEx(
                        GLOBALS.Settings.PropEditor.CamerasInnerBoundries 
                            ? critRect with { X = critRect.X, Y = critRect.Y, Width = critRect.Width, Height = critRect.Height }
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
                                    {
                                        var offset = new Vector2(
                                            _currentTile.Size.Width + _currentTile.BufferTiles*2, 
                                            _currentTile.Size.Height + _currentTile.BufferTiles*2
                                        ) * 20;

                                        offset /= 2;

                                        var propQuad = new Data.Quad(
                                            tileMouseWorld - offset - _defaultStretch,
                                            tileMouseWorld + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
                                            tileMouseWorld + offset + _defaultStretch,
                                            tileMouseWorld + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
                                        );

                                        if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
                                        if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

                                        propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);

                                        Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
                                    }
                                        break;

                                    case 1: // grid snap
                                    {
                                        var posV = new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20f;

                                        var offset = new Vector2(
                                            _currentTile.Size.Width + _currentTile.BufferTiles*2, 
                                            _currentTile.Size.Height + _currentTile.BufferTiles*2
                                        ) * 20;

                                        offset /= 2;

                                        var propQuad = new Data.Quad(
                                            posV - offset - _defaultStretch,
                                            posV + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
                                            posV + offset + _defaultStretch,
                                            posV + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
                                        );

                                        if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
                                        if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

                                        propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);
                                        
                                        // Printers.DrawTileAsProp(
                                        //     _currentTile,
                                        //     posV,
                                        //     _placementRotation * _placementRotationSteps
                                        // );

                                        Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
                                    }
                                        break;
                                    
                                    case 2: // precise grid snap
                                    {
                                        var posV = new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10;
                                        
                                        var offset = new Vector2(
                                            _currentTile.Size.Width + _currentTile.BufferTiles*2, 
                                            _currentTile.Size.Height + _currentTile.BufferTiles*2
                                        ) * 20;

                                        offset /= 2;

                                        var propQuad = new Data.Quad(
                                            posV - offset - _defaultStretch,
                                            posV + offset with { Y = -offset.Y } + _defaultStretch with { Y = -_defaultStretch.Y },
                                            posV + offset + _defaultStretch,
                                            posV + offset with { X = -offset.X } + _defaultStretch with { X = -_defaultStretch.X }
                                        );

                                        if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
                                        if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

                                        propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);
                                        
                                        // Printers.DrawTileAsProp(
                                        //     _currentTile,
                                        //     posV,
                                        //     _placementRotation * _placementRotationSteps
                                        // );

                                        Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
                                    }
                                        break;
                                }
                            }
                            break;
                        
                        case 1: // Current Rope
                            DrawCircleV(tileMouseWorld, 3f, Color.Blue);
                            break;

                        case 2: // Current Long Prop
                        if (!_longInitialPlacement) {
                            var prop = GLOBALS.LongProps[_propsMenuLongsIndex];
                            var texture = GLOBALS.Textures.LongProps[_propsMenuLongsIndex];
                            var height = texture.Height / 2f;

                            var posV = _snapMode switch
                            {
                                1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
                                2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
                                _ => tileMouseWorld,
                            };
                            
                            var offset = new Vector2(
                                _currentTile.Size.Width + _currentTile.BufferTiles*2, 
                                _currentTile.Size.Height + _currentTile.BufferTiles*2
                            ) * 20;

                            offset /= 2;

                            var propQuad = new Data.Quad(
                                posV - offset,
                                posV + offset with { Y = -offset.Y },
                                posV + offset,
                                posV + offset with { X = -offset.X }
                            );

                            if (_vFlipPlacement) Utils.VFlipQuad(ref propQuad);
                            if (_hFlipPlacement) Utils.HFlipQuad(ref propQuad);

                            propQuad = Utils.RotatePropQuads(propQuad, _placementRotation * _placementRotationSteps);

                            Printers.DrawTileAsProp(_currentTile, propQuad, 0, 255);
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
                                InitVariedStandardProp variedStandard => (variedStandard.Size.x * 20 / 2f, variedStandard.Size.y * 20 / 2f, new PropVariedSettings(variation:_defaultVariation)),
                                InitStandardProp standard => (standard.Size.x * 20 / 2f, standard.Size.y * 20 / 2f, new BasicPropSettings()),
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
                                1 => new Vector2((int)(tileMouseWorld.X / 20), (int)(tileMouseWorld.Y / 20)) * 20,
                                2 => new Vector2((int)(tileMouseWorld.X / 10), (int)(tileMouseWorld.Y / 10)) * 10,
                                _ => tileMouseWorld,
                            };

                            var quad = new Data.Quad(
                                new Vector2(posV.X - width - _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
                                new Vector2(posV.X + width + _defaultStretch.X, posV.Y - height - _defaultStretch.Y), 
                                new Vector2(posV.X + width + _defaultStretch.X, posV.Y + height + _defaultStretch.Y), 
                                new Vector2(posV.X - width - _defaultStretch.X, posV.Y + height + _defaultStretch.Y));

                            if (_vFlipPlacement) Utils.VFlipQuad(ref quad);
                            if (_hFlipPlacement) Utils.HFlipQuad(ref quad);
                            
                            Printers.DrawProp(settings, prop, texture, quad,
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
                                                Utils.AllEqual(fetchedSelected.Select(f => f.prop.Depth),
                    fetchedSelected[0].prop.Depth)))
            {
                BeginTextureMode(GLOBALS.Textures.PropDepth);
                ClearBackground(Color.Green);
                Printers.DrawDepthIndicator(fetchedSelected[0].prop);
                EndTextureMode();
            }

            rlImGui.Begin();
            
            ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            
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

                            _isTileSearchActive = ImGui.IsItemActive();

                            if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                                ImGui.SetItemDefaultFocus();
                                ImGui.SetKeyboardFocusHere(-1);
                            }
                            
                            if (textChanged) {
                                SearchTiles();
                            }

                            if (ImGui.BeginListBox("##TileCategories", listSize))
                            {
                                var drawList = ImGui.GetWindowDrawList();
                                var textHeight = ImGui.GetTextLineHeight();

                                // Not searching
                                if (_tileAsPropSearchResult is null) {
                                    for (var index = 0; index < GLOBALS.TileDex.OrderedTileAsPropCategories.Length; index++)
                                    {
                                        var color = GLOBALS.TileDex?.GetCategoryColor(GLOBALS.TileDex.OrderedTileAsPropCategories[index]) ?? Vector4.Zero;

                                        Vector4 colorVec = color;

                                        var cursor = ImGui.GetCursorScreenPos();
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(colorVec / 255f)
                                        );

                                        var selected = ImGui.Selectable("  " + GLOBALS.TileDex.OrderedTileAsPropCategories[index],
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

                                        var color = GLOBALS.TileDex?.GetCategoryColor(GLOBALS.TileDex.OrderedTileAsPropCategories[originalIndex]) ?? Vector4.Zero;

                                        Vector4 colorVec = color;

                                        var cursor = ImGui.GetCursorScreenPos();
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(colorVec / 255f)
                                        );

                                        var selected = ImGui.Selectable("  " + name, _tileAsPropSearchCategoryIndex == c);

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
                                else if (_tileAsPropSearchResult.Tiles.Length > 0) {
                                    for (var t = 0; t < _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex].Length; t++) {
                                        var (tile, originalIndex) = _tileAsPropSearchResult.Tiles[_tileAsPropSearchCategoryIndex][t];

                                        var selected = ImGui.Selectable(tile.Name, _tileAsPropSearchIndex == t);

                                        if (selected) {
                                            _mode = 1;
                                            _tileAsPropSearchIndex = t;
                                            _propsMenuTilesIndex = originalIndex;
                                            _propsMenuTilesCategoryIndex = _tileAsPropSearchResult.Categories[_tileAsPropSearchCategoryIndex].originalIndex;
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

                            _isPropSearchActive = ImGui.IsItemActive();

                            if (_shortcuts.ActivateSearch.Check(ctrl, shift, alt)) {
                                ImGui.SetItemDefaultFocus();
                                ImGui.SetKeyboardFocusHere(-1);
                            }

                            if (textChanged) {
                                SearchProps();
                            }

                            if (ImGui.BeginListBox("##OtherPropCategories", listSize))
                            {
                                var drawList = ImGui.GetWindowDrawList();
                                var textHeight = ImGui.GetTextLineHeight();

                                // Not searching 
                                if (_propSearchResult is null) {
                                    for (var index = 0; index < _otherCategoryNames.Length; index++)
                                    {
                                        var color = GLOBALS.PropCategories[index].Item2;
                                        var colorVec = new Vector4(color.R/255f, color.G/255f, color.B/255f, 1.0f);

                                        var cursor = ImGui.GetCursorScreenPos();
                                        
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(colorVec)
                                        );

                                        var selected = ImGui.Selectable("  " + _otherCategoryNames[index],
                                            index == _propsMenuOthersCategoryIndex);
                                        
                                        if (selected)
                                        {
                                            _propsMenuOthersCategoryIndex = index;
                                            Utils.Restrict(ref _propsMenuOthersIndex, 0, _otherNames[_propsMenuOthersCategoryIndex].Length-1);
                                        }
                                    }
                                } else {
                                    for (var c = 0; c < _propSearchResult.Categories.Length; c++) {
                                        var (categoryName, originalIndex, color) = _propSearchResult.Categories[c];

                                        var colorVec = new Vector4(color.R/255f, color.G/255f, color.B/255f, 1.0f);

                                        var cursor = ImGui.GetCursorScreenPos();
                                        drawList.AddRectFilled(
                                            p_min: cursor,
                                            p_max: cursor + new Vector2(10f, textHeight),
                                            ImGui.ColorConvertFloat4ToU32(colorVec)
                                        );

                                        var selected = ImGui.Selectable("  " + categoryName, _propSearchCategoryIndex == c);

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
                                } else if (_propSearchResult.Props.Length > 0) {
                                    for (var p = 0; p < _propSearchResult.Props[_propSearchCategoryIndex].Length; p++) {
                                        var (prop, originalIndex) = _propSearchResult.Props[_propSearchCategoryIndex][p];

                                        var selected = ImGui.Selectable(prop.Name, _propSearchIndex == p);

                                        if (selected) {
                                            _propSearchIndex = p;
                                            _mode = 1;
                                            _propsMenuOthersIndex = originalIndex;
                                            _propsMenuOthersCategoryIndex = _propSearchResult.Categories[_propSearchCategoryIndex].originalIndex;
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
            
                    var currentTile = GLOBALS.TileDex?.OrderedTilesAsProps[_propsMenuTilesCategoryIndex][_propsMenuTilesIndex];
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

                    // Stretch

                    ImGui.SetNextItemWidth(100);
                    ImGui.InputFloat("Vertical Stretch", ref _defaultStretch.Y);
                    
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputFloat("Horizontal Stretch", ref _defaultStretch.X);
            
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
                            $"{index}. {currentProp.Name}{(_hidden[index] ? " [hidden]" : "")}", 
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

                    // Render Time

                    var renderTime = selectedProp.Extras.Settings.RenderTime;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Render Time", ref renderTime)) {
                        selectedProp.Extras.Settings.RenderTime = renderTime;
                    }
                    
                    // Render Order

                    var renderOrder = selectedProp.Extras.Settings.RenderOrder;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Render Order", ref renderOrder))
                        selectedProp.Extras.Settings.RenderOrder = renderOrder;
            
                    // Seed

                    var seed = selectedProp.Extras.Settings.Seed;
            
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Seed", ref seed);

                    selectedProp.Extras.Settings.Seed = seed;
            
                    // Depth
            
                    ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

                    var depth = selectedProp.Depth;
            
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Depth", ref depth))
                    {
                        _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    }
            
                    Utils.Restrict(ref depth, -29, 0);
            
                    selectedProp.Depth = depth;
            
                    // Variation

                    if (selectedProp.Extras.Settings is IVariable v)
                    {
                        var init = GLOBALS.Props[selectedProp.Position.category][selectedProp.Position.index];
                        var variations = (init as IVariableInit)?.Variations ?? 1;

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

                    if (selectedProp.Type == InitPropType_Legacy.VariedSoft && 
                        selectedProp.Extras.Settings is PropVariedSoftSettings vs &&
                        ((InitVariedSoftProp) GLOBALS.Props[selectedProp.Position.category][selectedProp.Position.index]).Colorize != 0)
                    {
                        var applyColor = vs.ApplyColor is 1;

                        if (ImGui.Checkbox("Apply Color", ref applyColor))
                        {
                            vs.ApplyColor = applyColor ? 1 : 0;
                        }
                    }

                    // Custom Depth

                    if (selectedProp.Extras.Settings is ICustomDepth cd) {
                        ImGui.SetNextItemWidth(100);

                        var customDepth = cd.CustomDepth;

                        if (ImGui.InputInt("Custom Depth", ref customDepth)) {
                            cd.CustomDepth = customDepth;
                        }
                    }
                    
                    // Rope
                    
                    if (fetchedSelected.Length == 1 && fetchedSelected[0].prop.Type == InitPropType_Legacy.Rope)
                    {
                        ImGui.SeparatorText("Rope Options");
                        
                        //

                        if (fetchedSelected[0].prop.Name == "Zero-G Tube")
                        {
                            var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.Extras.Settings);
                            var applyColor = ropeSettings.ApplyColor is 1;

                            if (ImGui.Checkbox("Apply Color", ref applyColor))
                            {
                                ropeSettings.ApplyColor = applyColor ? 1 : 0;
                            }
                        }
                        else if (fetchedSelected[0].prop.Name is "Wire" or "Zero-G Wire")
                        {
                            var ropeSettings = ((PropRopeSettings)fetchedSelected[0].prop.Extras.Settings);
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

                        var (modelOgIndex, simulate, model) = _models[modelIndex];

                        var oldSegmentCount = GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints.Length;
                        var segmentCount = oldSegmentCount;
                        
                        var switchSimSelected = ImGui.Button(model.EditType switch { RopeModel.EditTypeEnum.BezierPaths => "Bezier Paths", RopeModel.EditTypeEnum.Simulation => "Simulation", _ => "Unknown" });

                        if (switchSimSelected)
                        {
                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            
                            model.EditType = model.EditType switch {
                                RopeModel.EditTypeEnum.BezierPaths => RopeModel.EditTypeEnum.Simulation,
                                RopeModel.EditTypeEnum.Simulation => RopeModel.EditTypeEnum.BezierPaths,
                                _ => RopeModel.EditTypeEnum.Simulation
                            };

                            _bezierHandleLock = -1;
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
                            model.UpdateSegments([
                                ..GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints, new Vector2()
                            ]);
                        }
                        else if (segmentCount < oldSegmentCount)
                        {
                            model.UpdateSegments(GLOBALS.Level.Props[modelOgIndex].Extras.RopePoints[..^1]);
                        }

                        if (segmentCount != oldSegmentCount)
                        {
                            UpdateRopeModelSegments();
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else _shouldRedrawLevel = true;
                        }
                        
                        //

                        if (ImGui.Checkbox("Simulate Rope", ref simulate))
                        {
                            // _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                            else _shouldRedrawLevel = true;

                            _bezierHandleLock = -1;
                        }


                        if (model.EditType == RopeModel.EditTypeEnum.Simulation) // Simulation mode
                        {
                            var grav = model.Gravity;
                            if (ImGui.Checkbox("Gravity", ref grav)) {
                                model.Gravity = grav;
                            }
                            
                            var cycleFpsSelected = ImGui.Button($"{60 / _ropeSimulationFrameCut} FPS");

                            if (cycleFpsSelected)
                            {
                                _ropeSimulationFrameCut = ++_ropeSimulationFrameCut % 3 + 1;
                                // _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                else _shouldRedrawLevel = true;
                            }

                            var release = (fetchedSelected[0].prop.Extras.Settings as PropRopeSettings)?.Release ?? PropRopeRelease.None;

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

                                (fetchedSelected[0].prop.Extras.Settings as PropRopeSettings)!.Release =release;
                            }
                        }
                        else // Bezier mode
                        {
                            var oldHandlePointNumber = model.BezierHandles.Length;
                            var handlePointNumber = oldHandlePointNumber;

                            Utils.Restrict(ref handlePointNumber, 1);
                            
                            ImGui.SetNextItemWidth(100);
                            ImGui.InputInt("Control Points", ref handlePointNumber);


                            if (handlePointNumber != oldHandlePointNumber) {
                                var quads = GLOBALS.Level.Props[modelOgIndex].Quad;
                                var center = Utils.QuadsCenter(ref quads);
        
                                Utils.Restrict(ref handlePointNumber, 1);

                                _shouldRedrawLevel = true;
                                if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                                
                                if (handlePointNumber > oldHandlePointNumber)
                                {
                                    model.BezierHandles = [..model.BezierHandles, center];
                                }
                                else if (handlePointNumber < oldHandlePointNumber)
                                {
                                    model.BezierHandles = model.BezierHandles[..^1];
                                }
                            }

                        }

                        _models[modelIndex] = (modelOgIndex, simulate, model);

                        ropeNotFound:
                        {
                        }
                    }
                }
                else if (fetchedSelected.Length > 1 && 
                            Utils.AllEqual(fetchedSelected.Select(f => f.prop.Depth),
                                fetchedSelected[0].prop.Depth))
                {
                    ImGui.Image(new IntPtr(GLOBALS.Textures.PropDepth.Texture.Id), new Vector2(290, 20));

                    var depth = fetchedSelected[0].prop.Depth;
            
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Depth", ref depth))
                    {
                        _shouldRedrawLevel = true;
                        if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                    }
            
                    Utils.Restrict(ref depth, -29, 0);
            
                    foreach (var selected in fetchedSelected)
                        selected.prop.Depth = depth;
                } else if (fetchedSelected.Length > 0) {
                    if (ImGui.Button("Reset Quads", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        foreach (var prop in GLOBALS.Level.Props) {
                            prop.Quad = Utils.RotatePropQuads(prop.OriginalQuad, prop.Rotation);

                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        }
                    }

                    if (ImGui.Button("Reset Rotation", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        foreach (var prop in GLOBALS.Level.Props) {
                            prop.Quad = Utils.RotatePropQuads(prop.Quad, -prop.Rotation);
                            prop.Rotation = 0;

                            _shouldRedrawLevel = true;
                            if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette) _shouldRedrawPropLayer = true;
                        }
                    }
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

        // EndDrawing();
        #endregion

        // F3

        Printers.Debug.EnqueueF3(new(GLOBALS.Level.Width) { Name = "LW", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.Level.Height) { Name = "LH" });

        Printers.Debug.EnqueueF3(new(_defaultDepth) { Name = "PSL", SameLine = true });
        Printers.Debug.EnqueueF3(new(_placementRotation) { Name = "PR", SameLine = true });
        Printers.Debug.EnqueueF3(new(_defaultStretch.X) { Name = "HS", SameLine = true });
        Printers.Debug.EnqueueF3(new(_defaultStretch.Y) { Name = "VS" });
        
        Printers.Debug.EnqueueF3(null);
        
        Printers.Debug.EnqueueF3(new(_showTileLayer1) { Name = "Layer 1", SameLine = true });
        Printers.Debug.EnqueueF3(new(_showLayer1Tiles) { Name = "Tiles" });

        Printers.Debug.EnqueueF3(new(_showTileLayer2) { Name = "Layer 2", SameLine = true });
        Printers.Debug.EnqueueF3(new(_showLayer2Tiles) { Name = "Tiles" });

        Printers.Debug.EnqueueF3(new(_showTileLayer3) { Name = "Layer 3", SameLine = true });
        Printers.Debug.EnqueueF3(new(_showLayer3Tiles) { Name = "Tiles" });
        
        Printers.Debug.EnqueueF3(null);
        
        Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.Cameras) { Name = "Cameras", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.CamerasInnerBoundries) { Name = "Inner" });

        Printers.Debug.EnqueueF3(null);

        Printers.Debug.EnqueueF3(new("Coordinates: ") { SameLine = true });
        Printers.Debug.EnqueueF3(new(tileMouseWorld) { SameLine = true });
        
        Printers.Debug.EnqueueF3(new($"MX: {tileMatrixX} / MY: {tileMatrixY}"));
        
        Printers.Debug.EnqueueF3(new(canDrawTile) { Name = "canDrawTile" });
        Printers.Debug.EnqueueF3(new(inMatrixBounds) { Name = "inMatrixBounds" });

        Printers.Debug.EnqueueF3(null);
        
        Printers.Debug.EnqueueF3(new(_mode switch { 0 => "Selection", _ => "Placement" }) { Name = "Mode", SameLine = _mode == 0 });
        
        if (_mode == 0) {
            Printers.Debug.EnqueueF3(new(_movingProps) { Name = "M", SameLine = true });
            Printers.Debug.EnqueueF3(new(_rotatingProps) { Name = "R", SameLine = true });
            Printers.Debug.EnqueueF3(new(_scalingProps) { Name = "S", SameLine = true });
            Printers.Debug.EnqueueF3(new(_stretchingProp) { Name = "V", SameLine = true });
            Printers.Debug.EnqueueF3(new(_editingPropPoints) { Name = "P" });
        }

        Printers.Debug.EnqueueF3(new(GLOBALS.Props.Length) { Name = "Prop Categories", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.Props.Select(c => c.Length).Sum()) { Name = "Props", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.TileDex?.OrderedTileAsPropCategories.Length ?? 0) { Name = "Tile Categories", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.TileDex?.OrderedTilesAsProps.Select(c => c.Length).Sum() ?? 0) { Name = "Tiles", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.RopeProps.Length) { Name = "Ropes", SameLine = true });
        Printers.Debug.EnqueueF3(new(GLOBALS.LongProps.Length) { Name = "Longs" });
        
        Printers.Debug.EnqueueF3(new(_currentTile?.Name ?? "NULL") { Name = "Current Tile" });

        Printers.Debug.EnqueueF3(new(GLOBALS.Settings.PropEditor.CrossLayerSelection) { Name = "CLS", SameLine = true });
        Printers.Debug.EnqueueF3(new(_additionalInitialRopeSegments) { Name = "ARS", SameLine = true });
        Printers.Debug.EnqueueF3(new(_editingPropPoints) { Name = "EPP", SameLine = true });
        Printers.Debug.EnqueueF3(new(_lockedPlacement) { Name = "LP", SameLine = true });
        Printers.Debug.EnqueueF3(new(_models.Length) { Name = "RM", SameLine = true });
        Printers.Debug.EnqueueF3(new(_newlyCopied) { Name = "NC" });
        
        Printers.Debug.EnqueueF3(new(_isTileSearchActive) { Name = "TS", SameLine = true });
        Printers.Debug.EnqueueF3(new(_isPropSearchActive) { Name = "PS", SameLine = true });
        
        Printers.Debug.EnqueueF3(new(_propsMenuTilesCategoryIndex) { Name = "TCI", SameLine = true });
        Printers.Debug.EnqueueF3(new(_propsMenuOthersCategoryIndex) { Name = "OCI" });

        Printers.Debug.EnqueueF3(new(_propsMenuTilesIndex) { Name = "TI", SameLine = true });
        Printers.Debug.EnqueueF3(new(_propsMenuOthersIndex) { Name = "OI", SameLine = true });
        Printers.Debug.EnqueueF3(new(_propsMenuRopesIndex) { Name = "RI", SameLine = true });
        Printers.Debug.EnqueueF3(new(_propsMenuLongsIndex) { Name = "LI" });
        
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}
