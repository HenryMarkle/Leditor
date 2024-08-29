namespace Leditor.Renderer.Pages;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Data.Geometry;

public class MainPage
{
    public Serilog.ILogger? Logger { get; init; }
    public Context Context { get; init; }

    private int _column1Width;

    private int _x, _y;
    private int _cam;
    private bool _water, _previewDone;

    private bool _resetPreview;

    private bool _disposed;

    private RenderTexture2D _previewRT;

    private void ResetDraw()
    {
        _x = _y = _cam = 0;
        _water = _previewDone = false;

        if (_previewRT.Id != 0)
        {
            UnloadRenderTexture(_previewRT);
            _previewRT.Id = 0;    
        }

        if (Context.Level is null) return;

        _previewRT = LoadRenderTexture(Context.Level.Width * 20, Context.Level.Height * 20);

        BeginTextureMode(_previewRT);
        ClearBackground(new Color(170, 170, 170, 255));
        EndTextureMode();
    }

    private void OnLevelLoaded(object? sender, EventArgs e)
    {
        _resetPreview = true;
    }

    private void OnPageChanged(int previous, int next)
    {
        if (next == 1) _column1Width = 0;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        UnloadRenderTexture(_previewRT);
    }

    public MainPage(Context context)
    {
        Context = context;

        context.LevelLoaded += OnLevelLoaded;
        context.PageChanged += OnPageChanged;
    }

    ~MainPage()
    {
        Context.LevelLoaded -= OnLevelLoaded;
        Context.PageChanged -= OnPageChanged;
    }

    private void DrawPreview()
    {
        if (Context.Level is null || _previewDone) return;

        if (_x >= Context.Level.Width)
        {
            _x = 0;
            _y++;
        }

        BeginTextureMode(_previewRT);

        if (_y < Context.Level.Height)
        {
            ref var cell1 = ref Context.Level.GeoMatrix[_y, _x, 0];
            ref var cell2 = ref Context.Level.GeoMatrix[_y, _x, 1];
            ref var cell3 = ref Context.Level.GeoMatrix[_y, _x, 2];
        
            if (cell3.Type == GeoType.Solid) DrawRectangle(_x * 20, _y * 20, 20, 20, new Color(120, 120, 120, 255));
            if (cell2.Type == GeoType.Solid) DrawRectangle(_x * 20, _y * 20, 20, 20, new Color(80, 80, 80, 255));
            if (cell1.Type == GeoType.Solid) DrawRectangle(_x * 20, _y * 20, 20, 20, Color.Black);

            if (cell1[GeoFeature.HorizontalPole])
            {
                DrawLineEx(new Vector2(_x * 20, _y * 20 + 10), new Vector2(_x * 20 + 20, _y * 20 + 10), 4, Color.Black);
            }

            if (cell1[GeoFeature.VerticalPole])
            {
                DrawLineEx(new Vector2(_x * 20 + 10, _y * 20), new Vector2(_x * 20 + 10, _y * 20 + 20), 4, Color.Black);
            }

            _x++;
        }
        else if (!_water)
        {
            DrawRectangle(
                -20,
                (Context.Level.Height - Context.Level.WaterLevel - Context.Level.Padding.bottom) * 20,
                (Context.Level.Width + 2) * 20,
                (Context.Level.WaterLevel + Context.Level.Padding.bottom) * 20,
                new Color(0, 0, 255, 70)
            );

            _water = true;
        }
        else if (_cam < Context.Level.Cameras.Count)
        {
            var camera = Context.Level.Cameras[_cam];

            DrawRectangleLinesEx(new(camera.Coords, new Vector2(1300, 800)), 2, Color.Green);
            
            _cam++;
        }
        else if (!_previewDone)
        {
            _previewDone = true;
        }

        EndTextureMode();

    }

    public void Draw()
    {
        if (_resetPreview)
        {
            ResetDraw();
            _resetPreview = false;
        }

        if (IsKeyDown(KeyboardKey.LeftControl) && IsKeyPressed(KeyboardKey.O))
        {
            Context.Page = 0;
        }

        ClearBackground(Color.DarkGray);

        for (var i = 0; i < 120; i++) DrawPreview();

        rlImGui.Begin();
        {
            ImGui.BeginMainMenuBar();
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Open..", "CTRL + O")) Context.Page = 0;

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Render"))
                {
                    if (Context.Level?.Cameras.Count == 0) ImGui.BeginDisabled();

                    if (ImGui.MenuItem("Render all cameras", "CTRL + SHIFT + R"))
                    {
                        // RENDER
                        if (Context.Engine is not null && Context.Level is not null)
                        {
                            Context.Engine.Load(Context.Level);
                            Context.Page = 3;
                        }
                    }

                    if (Context.Level?.Cameras.Count == 0) ImGui.EndDisabled();

                    ImGui.EndMenu();
                }
            }
            ImGui.EndMainMenuBar();

            if (ImGui.Begin($"{Context.Level?.ProjectName ?? "NULL"}##LevelWindow", ImGuiWindowFlags.NoCollapse | 
                ImGuiWindowFlags.NoResize | 
                ImGuiWindowFlags.NoMove))
            {
                ImGui.SetWindowSize(new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80));
                ImGui.SetWindowPos(new Vector2(40, 40));

                ImGui.Columns(2);

                if (_column1Width == 0)
                {
                    _column1Width = (int) ImGui.GetContentRegionAvail().Y / 3;
                    ImGui.SetColumnWidth(0, _column1Width);
                }

                // Table

                if (ImGui.BeginTable("##LevelInfo", 2, ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Value");

                    ImGui.TableHeadersRow();

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Width");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{Context.Level?.Width ?? 0}");

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Height");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{Context.Level?.Height ?? 0}");
                    
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Cameras");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{Context.Level?.Cameras.Count ?? 0}");

                    ImGui.EndTable();
                }

                ImGui.NextColumn();

                rlImGui.ImageRenderTextureFit(_previewRT, false);

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}