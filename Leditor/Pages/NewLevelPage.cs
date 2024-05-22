﻿using static Raylib_cs.Raylib;
using ImGuiNET;
using System.Numerics;

namespace Leditor.Pages;

internal class NewLevelPage : EditorPage
{
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
    
    internal event EventHandler ProjectCreated;
    
    private int _leftPadding = 12;
    private int _rightPadding = 12;
    private int _topPadding = 3;
    private int _bottomPadding = 5;

    private int _matrixWidthValue = GLOBALS.InitialMatrixWidth;
    private int _matrixHeightValue = GLOBALS.InitialMatrixHeight;

    private int _rows = 1;
    private int _columns = 1;

    private bool _simpleFillLayer1 = true;
    private bool _simpleFillLayer2 = true;
    private bool _simpleFillLayer3;

    private bool _createCameras = true;
    
    private bool _advanced;

    public override void Draw()
    {
        BeginDrawing();
        
        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? new Color(100, 100, 100, 255) 
            :  Color.Gray
        );
        
        if (_advanced)
        {
        }
        else
        {
            var dimVisualRect = new Rectangle(0, 0, GLOBALS.Textures.DimensionsVisual.Texture.Width, GLOBALS.Textures.DimensionsVisual.Texture.Height);
        
            var scale = Math.Min(dimVisualRect.Width/ (_columns*dimVisualRect.Width), 
                dimVisualRect.Height/ (_rows*dimVisualRect.Height));
            
            var width = GLOBALS.Textures.DimensionsVisual.Texture.Width * scale;
            var height = GLOBALS.Textures.DimensionsVisual.Texture.Height * scale;
            
            BeginTextureMode(GLOBALS.Textures.DimensionsVisual);
            
            ClearBackground(Color.White);
            
            if (_simpleFillLayer1) DrawRectangleRec(dimVisualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer1);
            if (_simpleFillLayer2) DrawRectangleRec(dimVisualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer2 with { A = 50 });
            if (_simpleFillLayer3) DrawRectangleRec(dimVisualRect, GLOBALS.Settings.GeometryEditor.LayerColors.Layer3 with { A = 50 });
            
            for (var i = 0; i < _rows; i++)
            {
                for (var j = 0; j < _columns; j++)
                {
                    DrawRectangleLinesEx(
                        new Rectangle(
                            (GLOBALS.Textures.DimensionsVisual.Texture.Width - width*_columns)/2f + j * width + 40, 
                            (GLOBALS.Textures.DimensionsVisual.Texture.Height - height*_rows)/2f + i * height + 40, 
                            width - 80, 
                            height - 80
                        ), 
                        4f, Color.White
                    );

                    if (_createCameras)
                    {
                        DrawCircleLines(
                            (int)((GLOBALS.Textures.DimensionsVisual.Texture.Width - width*_columns)/2f + j * width + width/2f), 
                            (int)((GLOBALS.Textures.DimensionsVisual.Texture.Height - height*_rows)/2f + i * height + height/2f),
                            50 * scale, Color.White
                        );
                    }
                }
            }
            
            EndTextureMode();
        }
        
        #region ImGui
        rlImGui_cs.rlImGui.Begin();

        if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _);

        if (ImGui.Begin("Create New Level##NewLevelWindow"))
        {
            if (_advanced)
            {
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, 300);

                var col1Space = ImGui.GetContentRegionAvail();
                
                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Width", ref _matrixWidthValue);
                
                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Height", ref _matrixHeightValue);
                
                Utils.Restrict(ref _matrixWidthValue, 1);
                Utils.Restrict(ref _matrixHeightValue, 1);
                
                ImGui.SeparatorText("Padding");

                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Left", ref _leftPadding);
                
                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Top", ref _topPadding);
                
                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Right", ref _rightPadding);
                
                ImGui.SetNextItemWidth(200);
                ImGui.InputInt("Bottom", ref _bottomPadding);
                
                Utils.Restrict(ref _leftPadding, 0);
                Utils.Restrict(ref _topPadding, 0);
                Utils.Restrict(ref _rightPadding, 0);
                Utils.Restrict(ref _bottomPadding, 0);
                
                ImGui.Spacing();

                if (ImGui.Button("Simple Options", col1Space with { Y = 20 })) _advanced = false;

                if (ImGui.Button("Cancel", col1Space with { Y = 20 }))
                {
                    Logger.Debug("page 6: Cancel button clicked");

                    _leftPadding = GLOBALS.Level.Padding.left;
                    _rightPadding = GLOBALS.Level.Padding.right;
                    _topPadding = GLOBALS.Level.Padding.top;
                    _bottomPadding = GLOBALS.Level.Padding.bottom;

                    _matrixWidthValue = GLOBALS.Level.Width;
                    _matrixHeightValue = GLOBALS.Level.Height;

                    GLOBALS.Page = GLOBALS.PreviousPage;
                }
                if (ImGui.Button("Create", col1Space with { Y = 20 }))
                {
                    Logger.Debug("page 6: Ok button clicked");

                    Logger.Debug("new flag detected; creating a new level");
                    
                    GLOBALS.Level.New(
                        _matrixWidthValue,
                        _matrixHeightValue,
                        (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                        [1, 1, 0]
                    );

                    UnloadRenderTexture(GLOBALS.Textures.LightMap);
                    GLOBALS.Textures.LightMap = LoadRenderTexture((GLOBALS.Level.Width * GLOBALS.Scale) + 300, (GLOBALS.Level.Height * GLOBALS.Scale) + 300);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    ClearBackground(Color.White);
                    EndTextureMode();

                    UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
                    GLOBALS.Textures.GeneralLevel =
                        LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);

                    GLOBALS.ProjectPath = "";
                    GLOBALS.Level.ProjectName = "New Project";

                    GLOBALS.Page = 1;
                    ProjectCreated?.Invoke(this, EventArgs.Empty);
                }
                
                //
                ImGui.NextColumn();
                //
            }
            else
            {
                ImGui.Columns(2);
                ImGui.SetColumnWidth(0, 300);

                var col1Space = ImGui.GetContentRegionAvail();

                ImGui.SetNextItemWidth(100);
                ImGui.InputInt("Rows", ref _rows);
                
                ImGui.SetNextItemWidth(100);
                ImGui.InputInt("Columns", ref _columns);
                
                Utils.Restrict(ref _rows, 1);
                Utils.Restrict(ref _columns, 1);
                
                ImGui.Spacing();

                ImGui.Checkbox("Fill Layer 1", ref _simpleFillLayer1);
                ImGui.Checkbox("Fill Layer 2", ref _simpleFillLayer2);
                ImGui.Checkbox("Fill Layer 3", ref _simpleFillLayer3);
                
                ImGui.Spacing();

                ImGui.Checkbox("Create Cameras", ref _createCameras);
                
                ImGui.Spacing();

                if (ImGui.Button("Advanced Option", col1Space with { Y = 20})) _advanced = true;

                if (ImGui.Button("Cancel", col1Space with { Y = 20}))
                {
                    Logger.Debug("page 6: Cancel button clicked");

                    _leftPadding = GLOBALS.Level.Padding.left;
                    _rightPadding = GLOBALS.Level.Padding.right;
                    _topPadding = GLOBALS.Level.Padding.top;
                    _bottomPadding = GLOBALS.Level.Padding.bottom;

                    _matrixWidthValue = GLOBALS.Level.Width;
                    _matrixHeightValue = GLOBALS.Level.Height;

                    GLOBALS.Page = GLOBALS.PreviousPage;
                }

                if (ImGui.Button("Create", col1Space with { Y = 20}))
                {
                    _advanced = true;
                    
                    GLOBALS.Textures.GeneralLevel =
                        LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                    
                    Logger.Debug("new flag detected; creating a new level");

                    GLOBALS.Level.New(
                        _columns * 52 + 20,
                        _rows * 40 + 3,
                        (6, 3, 6, 5),
                        [_simpleFillLayer1 ? 1 : 0, _simpleFillLayer2 ? 1 : 0, _simpleFillLayer3 ? 1 : 0]
                    );

                    GLOBALS.Level.ProjectName = "New Level";
                    // GLOBALS.ProjectPath = GLOBALS.Paths.ProjectsDirectory;
                    
                    UnloadRenderTexture(GLOBALS.Textures.LightMap);
                    GLOBALS.Textures.LightMap = LoadRenderTexture((GLOBALS.Level.Width * GLOBALS.Scale) + 300, (GLOBALS.Level.Height * GLOBALS.Scale) + 300);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    ClearBackground(Color.White);
                    EndTextureMode();

                    UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
                    GLOBALS.Textures.GeneralLevel =
                        LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);

                    // create cameras

                    if (_createCameras)
                    {
                        for (var i = 0; i < _rows; i++)
                        {
                            for (var j = 0; j < _columns; j++)
                            {
                                if (i == 0 && j == 0) continue;

                                GLOBALS.Level.Cameras.Add(new RenderCamera() { Coords = new Vector2(20f + j*(GLOBALS.EditorCameraWidth - 380), 30f + i*(GLOBALS.EditorCameraHeight - 40)), Quad = new(new(), new(), new(), new()) });
                            }
                        }
                        GLOBALS.CamQuadLocks = new int[_rows*_columns];
                    }
                    else GLOBALS.CamQuadLocks = new int[1];

                    GLOBALS.Page = 1;
                    ProjectCreated?.Invoke(this, EventArgs.Empty);
                }
                
                //
                ImGui.NextColumn();
                //

                rlImGui_cs.rlImGui.ImageRenderTextureFit(GLOBALS.Textures.DimensionsVisual, false);
            }
            
            ImGui.End();   
        }
        
        rlImGui_cs.rlImGui.End();
        #endregion

        EndDrawing();
    }
}