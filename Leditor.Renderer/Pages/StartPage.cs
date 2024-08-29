namespace Leditor.Renderer.Pages;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;

using Leditor.Serialization;
using Leditor.Data.Geometry;

public sealed class StartPage
{
    public Serilog.ILogger? Logger { get; init; }
    public Context Context { get; init; }

    private Task<Geo[,,]>? _geoLoadTask;
    private RenderTexture2D _previewRT;

    private int _x, _y;

    private bool _disposed;

    public void OnTermination(object? sender, EventArgs e)
    {
    }

    public StartPage(Context context)
    {
        Context = context;

        Context.TerminationSignal += OnTermination;
    }

    ~StartPage()
    {
        Context.TerminationSignal -= OnTermination;
    }

    private void PreparePreview()
    {
        _geoLoadTask = Task.Run(() => GeoImporter.GetGeoMatrixAsync(ProjectFiles[CurrentProject], Logger));

        if (_previewRT.Id != 0)
        {
            UnloadRenderTexture(_previewRT);
            _previewRT.Id = 0;
        }

        _x = _y = -1;
    }

    private void DrawPreview()
    {
        if (_geoLoadTask is not { IsCompletedSuccessfully: true }) return;

        var matrix = _geoLoadTask.Result;

        if (_x is -1 || _y is -1)
        {
            _previewRT = LoadRenderTexture(matrix.GetLength(1) * 4, matrix.GetLength(0) * 4);

            BeginTextureMode(_previewRT);
            ClearBackground(new Color(170, 170, 170, 255));
            EndTextureMode();

            _x = _y = 0;
        }

        if (_x >= matrix.GetLength(1))
        {
            _x = 0;
            _y++;
        }

        if (_y >= matrix.GetLength(0)) return;

        var cell1 = matrix[_y, _x, 0];
        var cell2 = matrix[_y, _x, 1];
        var cell3 = matrix[_y, _x, 2];

        BeginTextureMode(_previewRT);

        if (cell3.Type == GeoType.Solid)
        {
            DrawRectangle(_x * 4, _y * 4, 4, 4, new Color(120, 120, 120, 255));
        }

        if (cell2.Type == GeoType.Solid)
        {
            DrawRectangle(_x * 4, _y * 4, 4, 4, new Color(80, 80, 80, 255));
        }

        if (cell1.Type == GeoType.Solid)
        {
            DrawRectangle(_x * 4, _y * 4, 4, 4, Color.Black);
        }

        EndTextureMode();

        _x++;
    }

    private string _projects = "";

    private int _currentProject;

    public int CurrentProject
    {
        get => _currentProject;
        set => _currentProject = value;
    }

    public string[] ProjectFiles { get; private set; } = [];
    public string[] ProjectNames { get; private set; } = [];

    private void ListFiles(string folder)
    {
        ProjectFiles = Directory.GetFiles(folder).Where(f => f.EndsWith(".txt")).ToArray();
        ProjectNames = ProjectFiles.Select(f => Path.GetFileNameWithoutExtension(f)!).ToArray();
        CurrentProject = 0;
        PreparePreview();
    }

    public required string ProjectsFolder
    { 
        get => _projects; 
        set
        {
            if (!Directory.Exists(value)) throw new DirectoryNotFoundException(value);

            _projects = value;
            ListFiles(value);
        } 
    }

    private int _column1Width;

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        UnloadRenderTexture(_previewRT);
    }

    public void Draw()
    {
        for (var i = 0; i < 120; i++) DrawPreview();

        ClearBackground(Color.DarkGray);

        if (IsKeyPressed(KeyboardKey.Enter))
        {
            if (!Context.IsLoadingLevel)
            {
                Logger?.Information("Loading level \"{Name}\"", ProjectFiles[CurrentProject]);
                Context.LoadLevelFromFile(ProjectFiles[CurrentProject]);
            }

            // Serialization.Level.Importers.LoadProjectAsync(
            //     ProjectFiles[CurrentProject], 
            //     Context.Registry.Tiles.Names, 
            //     Context.Registry.Props.Names,
            //     Context.Registry.Materials.Names
            // ).Wait();
        }

        if (IsKeyPressed(KeyboardKey.Down))
        {
            CurrentProject++;
            CurrentProject %= ProjectFiles.Length;

            PreparePreview();
        }
        else if (IsKeyPressed(KeyboardKey.Up))
        {
            CurrentProject--;
            CurrentProject =  Utils.Restrict(CurrentProject, 0, ProjectFiles.Length);

            PreparePreview();
        }

        rlImGui.Begin();
        {
            if (ImGui.Begin("Projects", 
                ImGuiWindowFlags.NoCollapse | 
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

                if (ImGui.Button("Refresh"))
                {
                    ListFiles(ProjectsFolder);
                    PreparePreview();
                }

                var listSpace = ImGui.GetContentRegionAvail();

                if (ImGui.BeginListBox("##ProjectsList", listSpace with { Y = listSpace.Y - 30 }))
                {
                    for (var p = 0; p < ProjectNames.Length; p++)
                    {
                        if (ImGui.Selectable(ProjectNames[p], p == CurrentProject))
                        {
                            if (CurrentProject != p) PreparePreview();
                            CurrentProject = p;
                        }
                    }


                    ImGui.EndListBox();
                }

                var outbounds = CurrentProject < 0 || CurrentProject >= ProjectNames.Length;

                if (outbounds) ImGui.BeginDisabled(); 

                if (ImGui.Button("Load", ImGui.GetContentRegionAvail() with { Y = 20 }))
                {
                    Logger?.Information("Loading level \"{Name}\"", ProjectFiles[CurrentProject]);
                    Context.LoadLevelFromFile(ProjectFiles[CurrentProject]);
                }

                if (outbounds) ImGui.EndDisabled();

                ImGui.NextColumn();

                rlImGui.ImageRenderTextureFit(_previewRT, false);

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}