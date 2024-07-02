using static Raylib_cs.Raylib;
using rlImGui_cs;
using Leditor.Types;
using Leditor.Data.Tiles;
using System.Numerics;
using ImGuiNET;

namespace Leditor.Pages;

#nullable enable

internal class TileCreatorPage : EditorPage
{
    public override void Dispose()
    {
        if (Disposed) return;

        Disposed = true;

        for (var l = 0; l < 30; l++) _layersRT[l].Dispose();
        _previewRT.Dispose();
        _composedRT.Dispose();
        _newCanvasRT.Dispose();

        UnloadShader(_whiteEraser);
    }

    internal TileCreatorPage()
    {
        _camera = new Camera2D { Zoom = 1 };
        _generalSettings = GLOBALS.Settings.GeneralSettings;

        _layersRT = new RL.Managed.RenderTexture2D[30];
        _hidden = new bool[30];

        for (var l = 0; l < 30; l++) {
            _layersRT[l] = new(0, 0);
        }

        _previewRT = new(0, 0);
        _composedRT = new(0, 0);

        _newCanvasRT = new(20 + 20, 20 + 20);

        BeginTextureMode(_newCanvasRT);
        ClearBackground(Color.White with { A = 0 });

        DrawRectangle(10, 10, 20, 20, _generalSettings.DarkTheme ? Color.White : Color.Black);

        EndTextureMode();

        _whiteEraser = LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = texture(inputTexture, fragTexCoord);

	if (newColor.r == 1.0 && newColor.g == 1.0 && newColor.b == 1.0) discard;

	if (newColor.a == 0.0) {
		discard;
	}

    newColor = vec4(newColor.r * fragColor.r, newColor.g * fragColor.g, newColor.b * fragColor.b, newColor.a * fragColor.a);

	FragColor = newColor;
}");

        Name = "";
        Type = Data.Tiles.TileType.VoxelStruct;

        L1Specs = new int[0, 0];
        L2Specs = new int[0, 0];
        L3Specs = new int[0, 0];

        RepeatL = Enumerable
            .Range(0, _newLayerCount)
            .Select(i => 1)
            .ToList();
        
        Tags = [];

        UpdateNewCanvas();
    }

    ~TileCreatorPage() {
        if (!Disposed) throw new InvalidOperationException("TileCreatorPage was not disposed by consumer.");
    }

    #region Resources

    private RL.Managed.RenderTexture2D[] _layersRT;
    private RL.Managed.RenderTexture2D _previewRT;
    private RL.Managed.RenderTexture2D _composedRT;

    private RL.Managed.RenderTexture2D _newCanvasRT;

    private Shader _whiteEraser;

    #endregion

    #region Fields

    private Camera2D _camera;
    private GeneralSettings _generalSettings;

    private bool _createNew = true;

    private int _newWidth = 1;
    private int _newHeight = 1;
    private int _newBuffer;

    private int _newLayerCount = 1;

    private string _newName = "New Tile";

    private bool[] _hidden;

    #endregion

    #region Properties

    private string Name { get; set; }

    private Data.Tiles.TileType Type { get; set; }

    private int Width { get; set; }
    private int Height { get; set; }

    private int BufferSpace { get; set; }

    private int[,] L1Specs { get; set; }
    private int[,] L2Specs { get; set; }
    private int[,] L3Specs { get; set; }

    private List<int> RepeatL { get; set; }

    private List<string> Tags { get; set; }

    #endregion

    private void ResetNewMenu() {
        _newWidth = 1;
        _newHeight = 1;
        _newBuffer = 0;
        _newLayerCount = 1;
    }

    private void New(int width, int height, int bufferSpace) {
        if (width is 0 || height is 0) return;

        Width = width;
        Height = height;
        BufferSpace = bufferSpace;

        L1Specs = new int[Height, Width];
        L2Specs = new int[Height, Width];
        L3Specs = new int[Height, Width];

        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                L1Specs[y, x] = -1;
                L2Specs[y, x] = -1;
                L3Specs[y, x] = -1;
            }
        }

        RepeatL = Enumerable.Range(0, _newLayerCount).Select(i => 1).ToList();

        Tags = [];

        ResetNewMenu();

        UpdateCanvas();
    }

    private void UpdateNewCanvas() {
        if (_newWidth is 0 || _newHeight is 0) return;

        _newCanvasRT.Dispose();
        _newCanvasRT = new((_newWidth + _newBuffer * 2) * 20 + 20, (_newHeight + _newBuffer * 2) * 20 + 20);

        BeginTextureMode(_newCanvasRT);
        ClearBackground(_generalSettings.DarkTheme ? Color.Black : Color.White);

        for (var x = 0; x < _newWidth + _newBuffer*2; x++) {
            for (var y = 0; y < _newHeight + _newBuffer*2; y++) {
                if (y == 0 || y == _newHeight + _newBuffer*2 ||
                    x == 0 || x == _newWidth + _newBuffer*2) {

                    DrawRectangleLines(10 + x * 20, 10 + y * 20, 20, 20, Color.Green);
                } else {
                    DrawRectangleLines(10 + x * 20, 10 + y * 20, 20, 20, _generalSettings.DarkTheme ? Color.White : Color.Black);
                }
            }
        }
        EndTextureMode();
    }

    private void UpdateCanvas() {
        if (Width is 0 || Height is 0) {
            for (var l = 0; l < 30; l++) {
                _layersRT[l]?.Dispose();
                _layersRT[l] = new(0, 0);
            }

            _previewRT?.Dispose();
            _previewRT = new(0, 0);

            _composedRT?.Dispose();
            _composedRT = new(0, 0);

            return;
        }

        //

        for (var l = 0; l < 30; l++) {
            var newCanvas = new RL.Managed.RenderTexture2D((Width + BufferSpace * 2) * 20, (Height + BufferSpace * 2) * 20);

            BeginTextureMode(newCanvas);
            ClearBackground(Color.White);
            DrawTexture(_layersRT[l].Raw.Texture, 0, 0, Color.White);
            EndTextureMode();

            _layersRT[l].Dispose();
            _layersRT[l] = newCanvas;
        }


        var newPreview = new RL.Managed.RenderTexture2D(Width * 16, Height * 16);

        BeginTextureMode(newPreview);
        DrawTexture(_previewRT!.Raw.Texture, 0, 0, Color.White);
        EndTextureMode();

        _previewRT?.Dispose();
        _previewRT = newPreview;

        _composedRT?.Dispose();
        _composedRT = new(Width * 20, Height * 20);

        BeginTextureMode(_composedRT);
        ClearBackground(Color.White);

        for (var l = 29; l > -1; l--) {
            BeginShaderMode(_whiteEraser);
            SetShaderValueTexture(_whiteEraser, GetShaderLocation(_whiteEraser, "inputTexture"), _layersRT[l].Raw.Texture);
            DrawTexture(_layersRT[l].Raw.Texture, 0, 0, Color.White);
            EndShaderMode();
        }

        EndTextureMode();
    }

    public override void Draw()
    {
        if (_generalSettings.GlobalCamera) _camera = GLOBALS.Camera;

        #region Shortcuts

        // Handle Mouse Drag
        if (IsMouseButtonDown(MouseButton.Middle))
        {
            var delta = GetMouseDelta();
            delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
            _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
        }

        // Handle Zoom
        var wheel = GetMouseWheelMove();
        if (wheel != 0 && true /*isNotWinBusy*/)
        {
            var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
            _camera.Offset = GetMousePosition();
            _camera.Target = mouseWorldPosition;
            _camera.Zoom += wheel * GLOBALS.ZoomIncrement;
            if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
        }

        #endregion

        // BeginDrawing();
        {
            ClearBackground(
                _generalSettings.DarkTheme 
                    ? Color.Black 
                    : new Color(120, 120, 120, 255)
            );

            if (_createNew) {
                rlImGui.Begin();
                {
                    if (ImGui.Begin("New Tile##TileCreatorNewWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove)) {
                        ImGui.SetWindowPos(Vector2.One * 20);
                        ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 40, GetScreenHeight() - 40));

                        ImGui.Columns(2);

                        ImGui.InputText("Name", ref _newName, 256);

                        if (ImGui.InputInt("Width", ref _newWidth)) Utils.Restrict(ref _newWidth, 1);
                        if (ImGui.InputInt("Height", ref _newHeight)) Utils.Restrict(ref _newHeight, 1);
                        if (ImGui.InputInt("Buffer Space", ref _newBuffer)) Utils.Restrict(ref _newBuffer, 0);
                        if (ImGui.InputInt("Layers", ref _newLayerCount)) Utils.Restrict(ref _newLayerCount, 1);

                        ImGui.Spacing();

                        var disableCreate = string.IsNullOrEmpty(_newName);

                        if (disableCreate) ImGui.BeginDisabled();

                        if (ImGui.Button("Create", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                            Name = _newName;
                            New(_newWidth, _newHeight, _newBuffer);
                            
                            _createNew = false;
                        }

                        if (disableCreate) ImGui.EndDisabled();

                        ImGui.NextColumn();

                        rlImGui.ImageRenderTextureFit(_newCanvasRT, false);

                        ImGui.End();
                    }
                }
                rlImGui.End();
            } else {
                #region 2D

                BeginMode2D(_camera);
                {
                    DrawTexture(_composedRT.Raw.Texture, 0, 0, Color.White);
                }
                EndMode2D();

                #endregion

                #region ImGui

                rlImGui.Begin();
                {

                }
                rlImGui.End();

                #endregion
            }

        }
        // EndDrawing();

        if (_generalSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}