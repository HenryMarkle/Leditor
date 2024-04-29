using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

internal class DimensionsEditorPage : EditorPage, IContextListener
{
    public override void Dispose()
    {
        Disposed = true;
    }
    
    internal event EventHandler ProjectCreated;
    
    private int _matrixWidthValue = GLOBALS.InitialMatrixWidth;
    private int _matrixHeightValue = GLOBALS.InitialMatrixHeight;
    
    private int _leftPadding = 12;
    private int _rightPadding = 12;
    private int _topPadding = 3;
    private int _bottomPadding = 5;
    private int _editControl;

    private int _rows = 1;
    private int _columns = 1;

    private bool _simpleFillLayer1 = true;
    private bool _simpleFillLayer2 = true;
    private bool _simpleFillLayer3;

    private bool _createCameras = true;

    private bool _fillLayer1;
    private bool _fillLayer2;
    private bool _fillLayer3;

    private bool _advanced;

    /// <summary>
    /// Only call in drawing mode
    /// </summary>
    static void ResizeLightMap(int newWidth, int newHeight)
    {
        var scale = GLOBALS.Scale;
        var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);
        UnloadRenderTexture(GLOBALS.Textures.LightMap);
        var texture = LoadTextureFromImage(image);
        UnloadImage(image);

        GLOBALS.Textures.LightMap = LoadRenderTexture((newWidth * scale) + 300, (newHeight) * scale + 300);

        BeginTextureMode(GLOBALS.Textures.LightMap);
        ClearBackground(new(255, 255, 255, 255));
        DrawTexture(texture, 0, 0, new(255, 255, 255, 255));
        EndTextureMode();

        UnloadTexture(texture);
    }

    #nullable enable
    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        _matrixWidthValue = GLOBALS.Level.Width;
        _matrixHeightValue = GLOBALS.Level.Height;
    }

    public void OnProjectCreated(object? sender, EventArgs e)
    {
        
    }
    #nullable disable

    public void OnPageUpdated(int previous, int @next) {
        
    }

    public override void Draw()
    {
        BeginDrawing();

        ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
            ? new Color(100, 100, 100, 255) 
            :  Color.Gray
        );

        rlImGui.Begin();
        
        // Navigation bar
                
        GLOBALS.NavSignal = Printers.ImGui.Nav();

        if (ImGui.Begin("Resize##LevelDimensions", ImGuiWindowFlags.NoCollapse))
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
            
            var isBecomingLarger = _matrixWidthValue > GLOBALS.Level.Width || _matrixHeightValue > GLOBALS.Level.Height;
            
            if (isBecomingLarger)
            {
                ImGui.SeparatorText("Fill Extra Space");
                        
                ImGui.Checkbox("Fill Layer 1", ref _fillLayer1);
                ImGui.Checkbox("Fill Layer 2", ref _fillLayer2);
                ImGui.Checkbox("Fill Layer 3", ref _fillLayer3);
                        
                ImGui.Spacing();
            }

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
            
            if (ImGui.Button("Resize", col1Space with { Y = 20 }))
            {
                Logger.Debug("page 6: Ok button clicked");

                Logger.Debug("resize flag detected");

                if (
                    GLOBALS.Level.Height != _matrixHeightValue ||
                    GLOBALS.Level.Width != _matrixWidthValue)
                {
                    Logger.Debug("dimensions don't match; resizing");

                    Logger.Debug("resizing geometry matrix");
                    
                    // I know this can be simplified, but I'm keeping it in case 
                    // it becomes useful in the future
                    if (isBecomingLarger)
                    {
                        GLOBALS.Level.Resize(
                            _matrixWidthValue,
                            _matrixHeightValue,
                            (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                            [
                                _fillLayer1 ? 1 : 0,
                                _fillLayer2 ? 1 : 0,
                                _fillLayer3 ? 1 : 0
                            ]
                        );
                    }
                    else
                    {
                        GLOBALS.Level.Resize(
                            _matrixWidthValue,
                            _matrixHeightValue,
                            (_leftPadding, _topPadding, _rightPadding, _bottomPadding),
                            [ 0, 0, 0 ]
                        );
                    }


                    Logger.Debug("resizing light map");

                    ResizeLightMap(_matrixWidthValue, _matrixHeightValue);
                    
                    UnloadRenderTexture(GLOBALS.Textures.GeneralLevel);
                    GLOBALS.Textures.GeneralLevel =
                        LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                }
                
                GLOBALS.Level.Padding = (_leftPadding, _topPadding, _rightPadding, _bottomPadding);

                GLOBALS.Page = 1;
            }
            
            ImGui.End();
        }
        
        rlImGui.End();

        EndDrawing();
    }
}
