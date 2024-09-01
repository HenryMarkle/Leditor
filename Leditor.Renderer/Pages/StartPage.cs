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

    private bool _loadFailedMessage;

    public void OnLevelLoadFail(object? sender, EventArgs e)
    {
        _loadFailedMessage = true;
    }

    public StartPage(Context context)
    {
        Context = context;

        Context.LevelLoadingFailed += OnLevelLoadFail;
    }

    ~StartPage()
    {
        Context.LevelLoadingFailed -= OnLevelLoadFail;
    }

    private void PreparePreview()
    {
        _geoLoadTask = Task.Run(() => GeoImporter.GetGeoMatrixAsync(ProjectEntries[CurrentEntry], Logger));

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

    private int _currentEntry;

    public int CurrentEntry
    {
        get => _currentEntry;
        set => _currentEntry = value;
    }

    public bool[] IsDirectory { get; private set; } = [];
    public string[] ProjectEntries { get; private set; } = [];
    public string[] ProjectNames { get; private set; } = [];

    private void ListFiles(string folder)
    {
        ProjectEntries = Directory.GetFileSystemEntries(folder).Where(e => e.EndsWith(".txt") || Directory.Exists(e)).ToArray();
        ProjectNames = ProjectEntries.Select(f => Path.GetFileNameWithoutExtension(f)!).ToArray();
        IsDirectory = ProjectEntries.Select(Directory.Exists).ToArray();
        CurrentEntry = 0;
        if (ProjectEntries.Length > 0 && !IsDirectory[CurrentEntry]) PreparePreview();
    }

    public required string ProjectsFolder
    { 
        get => _projects; 
        set
        {
            if (!Directory.Exists(value)) throw new DirectoryNotFoundException(value);

            ListFiles(value);

            for (var p = 0; p < ProjectEntries.Length; p++)
            {
                if (ProjectEntries[p] == _projects)
                {
                    CurrentEntry = p;
                    break;
                }
            }

            _projects = value;
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
            if (IsDirectory[CurrentEntry])
            {
                ProjectsFolder = ProjectEntries[CurrentEntry];
            }
            else
            {
                if (!Context.IsLoadingLevel)
                {
                    Logger?.Information("Loading level \"{Name}\"", ProjectEntries[CurrentEntry]);
                    Context.LoadLevelFromFile(ProjectEntries[CurrentEntry]);
                }
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
            CurrentEntry++;
            CurrentEntry %= ProjectEntries.Length;

            if (ProjectEntries.Length > 0 && !IsDirectory[CurrentEntry]) PreparePreview();
        }
        else if (IsKeyPressed(KeyboardKey.Up))
        {
            CurrentEntry--;
            CurrentEntry =  Utils.Cycle(CurrentEntry, 0, ProjectEntries.Length - 1);

            if (ProjectEntries.Length > 0 && !IsDirectory[CurrentEntry]) PreparePreview();
        }
        else if (IsKeyPressed(KeyboardKey.Left))
        {
            ProjectsFolder = Directory.GetParent(ProjectsFolder)!.FullName;
        }
        else if (IsKeyPressed(KeyboardKey.Right) && ProjectEntries.Length > 0 && IsDirectory[CurrentEntry])
        {
            ProjectsFolder = ProjectEntries[CurrentEntry];
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
                    if (ProjectEntries.Length > 0 && !IsDirectory[CurrentEntry]) PreparePreview();
                }

                var listSpace = ImGui.GetContentRegionAvail();

                if (Context?.IsLoadingLevel ?? false) ImGui.BeginDisabled();

                if (ImGui.BeginListBox("##ProjectsList", listSpace with { Y = listSpace.Y - 30 }))
                {
                    for (var p = 0; p < ProjectNames.Length; p++)
                    {
                        if (Context?.Textures is not null)
                        {
                            if (IsDirectory[p]) rlImGui.ImageSize(Context.Textures.Folder, new(23, 23));
                            else rlImGui.ImageSize(Context.Textures.File, new(23, 23));

                            ImGui.SameLine();
                        }
                        
                        if (ImGui.Selectable(ProjectNames[p], p == CurrentEntry, ImGuiSelectableFlags.None, ImGui.GetContentRegionAvail() with { Y = 23 }))
                        {
                            if (CurrentEntry == p)
                            {
                                if (IsDirectory[p]) ProjectsFolder = ProjectEntries[p];
                                else
                                {
                                    Logger?.Information("Loading level \"{Name}\"", ProjectNames[CurrentEntry]);
                                    Context?.LoadLevelFromFile(ProjectEntries[CurrentEntry]);
                                }
                            }
                            else
                            {
                                CurrentEntry = p;
                                if (!IsDirectory[p]) PreparePreview();
                            }
                        }
                    }


                    ImGui.EndListBox();
                }

                if (Context?.IsLoadingLevel ?? false) ImGui.EndDisabled();

                var outbounds = CurrentEntry < 0 || CurrentEntry >= ProjectNames.Length;

                if (outbounds) ImGui.BeginDisabled(); 

                if (ImGui.Button("Load", ImGui.GetContentRegionAvail() with { Y = 20 }))
                {
                    Logger?.Information("Loading level \"{Name}\"", ProjectEntries[CurrentEntry]);
                    Context?.LoadLevelFromFile(ProjectEntries[CurrentEntry]);
                }

                if (outbounds) ImGui.EndDisabled();

                ImGui.NextColumn();

                rlImGui.ImageRenderTextureFit(_previewRT, false);

                if (_loadFailedMessage) ImGui.OpenPopup("Load Failed");

                if (ImGui.BeginPopupModal("Load Failed"))
                {
                    ImGui.Text("Failed to load the level.");
                    if (ImGui.Button("Ok"))
                    {
                        _loadFailedMessage = false;
                        ImGui.CloseCurrentPopup();
                    }
                    ImGui.EndPopup();
                }

                ImGui.End();
            }
        }
        rlImGui.End();
    }
}