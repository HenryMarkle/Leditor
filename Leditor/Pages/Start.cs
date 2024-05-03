using System.Numerics;
using System.Text;
using ImGuiNET;
using Leditor.Pages;
using Pidgin;
using rlImGui_cs;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

internal class StartPage : EditorPage
{
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        _levelPreviewRT.Dispose();
        _upTexture.Dispose();
        _homeTexture.Dispose();
        _folderTexture.Dispose();
        _fileTexture.Dispose();

        _upBlackTexture.Dispose();
        _homeBlackTexture.Dispose();
        _folderBlackTexture.Dispose();
        _fileBlackTexture.Dispose();
    }

    internal event EventHandler? ProjectLoaded;

    private bool _uiLocked;

    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private TileCheckResult CheckTileIntegrity(in LoadFileResult res)
    {
        var result = TileCheckResult.Ok;
        
        for (int y = 0; y < res.Height; y++)
        {
            for (int x = 0; x < res.Width; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var cell = res.TileMatrix![y, x, z];

                    if (cell.Data is TileHead h)
                    {
                        if (h.Definition is null) result = TileCheckResult.Missing;
                        else if (h.Definition.Texture.Id == 0) result = TileCheckResult.MissingTexture;
                    }
                    else if (cell.Type == TileType.Material)
                    {
                        var materialName = ((TileMaterial)cell.Data).Name;

                        if (!GLOBALS.MaterialColors.ContainsKey(materialName))
                        {
                            Logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
                            result = TileCheckResult.MissingMaterial;
                        }
                    }

                    skip:
                    { }
                }
            }
        }

        Logger.Debug("tile check passed");

        return result;
    }

    private PropCheckResult CheckPropIntegrity(in LoadFileResult res)
    {
        var result = PropCheckResult.Ok;
        
        for (var p = 0; p < res.PropsArray!.Length; p++)
        {
            var prop = res.PropsArray[p];
            
            // Check for texture
            
            try
            {
                _ = prop.type switch
                {
                    InitPropType.Long => GLOBALS.Textures.LongProps[prop.position.index],
                    InitPropType.Rope => GLOBALS.Textures.RopeProps[prop.position.index],
                    InitPropType.Tile => prop.tile?.Texture ?? throw new NullReferenceException(),
                    _ => GLOBALS.Textures.Props[prop.position.category][prop.position.index]
                };

                // No IndexOutOfRangeException exception was thrown - Success
            }
            catch
            {
                var path = prop.type == InitPropType.Tile
                    ? Path.Combine(GLOBALS.Paths.TilesAssetsDirectory, prop.prop.Name+".png")
                    : Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, prop.prop.Name + ".png");
                
                Logger.Error($"prop texture \"{path}\"");
                result = PropCheckResult.MissingTexture;
            }
        }
        
        return result;
    }
    
    private RL.Managed.RenderTexture2D _levelPreviewRT;
    private RL.Managed.Texture2D _upTexture;
    private RL.Managed.Texture2D _homeTexture;
    private RL.Managed.Texture2D _folderTexture;
    private RL.Managed.Texture2D _fileTexture;
    
    private RL.Managed.Texture2D _upBlackTexture;
    private RL.Managed.Texture2D _homeBlackTexture;
    private RL.Managed.Texture2D _folderBlackTexture;
    private RL.Managed.Texture2D _fileBlackTexture;

    private bool _shouldRedrawLevelReview = true;

    //
    private string _currentDir;
    private (string path, string name, bool isDir)[] _dirEntries;

    private int _currentIndex = -1;

    private void NavigateToDir(string path) {
        if (!Directory.Exists(path)) return;

        _currentDir = path;
        _dirEntries = Directory
            .GetFileSystemEntries(path)
            .Select(e => {
                var attrs = File.GetAttributes(e);

                return (e, (attrs & FileAttributes.Directory) == FileAttributes.Directory);
            })
            .Where(e => e.Item2 || e.Item1.EndsWith(".txt"))
            .Select(e => {
                return (e.Item1, Path.GetFileNameWithoutExtension(e.Item1), e.Item2);
            })
            .ToArray();
    }

    private void NavigateUp() {
        var parentDir = Directory.GetParent(_currentDir);

        if (parentDir is null or { Exists: false }) return;

        NavigateToDir(parentDir.FullName);
    }

    // TODO: Optimize
    private void LoadLevelPreview(string path) {
        var name = Path.GetFileNameWithoutExtension(path);

        if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews")))
            Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));

        var pngPath = Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", name+".png");

        if (File.Exists(pngPath)) {
            using RL.Managed.Texture2D texture = new(LoadTexture(pngPath));
        
            _levelPreviewRT.Dispose();
            _levelPreviewRT = new(texture.Raw.Width, texture.Raw.Height);

            BeginTextureMode(_levelPreviewRT);
            ClearBackground(Color.White);

            DrawTexture(texture, 0, 0, Color.White);
            EndTextureMode();
        } else {
            RunCell[,,] matrix;

            try {
                matrix = Utils.GetGeometryMatrixFromFile(path);
            } catch {
                return;
            }
            
            using RL.Managed.Image image = Printers.GenerateLevelReviewImage(matrix);
            ExportImage(image, pngPath);
            
            using RL.Managed.Texture2D texture = new(image);
        
            _levelPreviewRT.Dispose();
            _levelPreviewRT = new(texture.Raw.Width, texture.Raw.Height);

            BeginTextureMode(_levelPreviewRT);
            ClearBackground(Color.White);

            DrawTexture(texture, 0, 0, Color.White);
            EndTextureMode();
        }
    }
    //

    internal StartPage()
    {
        _currentDir = GLOBALS.Paths.ProjectsDirectory;
        _dirEntries = Directory
            .GetFileSystemEntries(_currentDir)
            .Select(e => {
                var attrs = File.GetAttributes(e);

                return (e, (attrs & FileAttributes.Directory) == FileAttributes.Directory);
            })
            .Where(e => e.Item2 || e.Item1.EndsWith(".txt"))
            .Select(e => {
                return (e.Item1, Path.GetFileNameWithoutExtension(e.Item1), e.Item2);
            })
            .ToArray();

        _levelPreviewRT = new(0, 0);
        
        _upTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon.png"));
        _homeTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon.png"));
        _folderTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon.png"));
        _fileTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon.png"));
    
        _upBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon black.png"));
        _homeBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon black.png"));
        _folderBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon black.png"));
        _fileBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon black.png"));
    

    }

    ~StartPage() {
        if (!Disposed) throw new InvalidOperationException("StartPage was not disposed by consumer");
    }

    public override void Draw()
    {
        if (!_uiLocked) {
            if (_dirEntries.Length > 0 && (IsKeyPressed(KeyboardKey.Down))) {
                _currentIndex++;
                _shouldRedrawLevelReview = true;

                Utils.Restrict(ref _currentIndex, 0, _dirEntries.Length - 1);
            }

            if (_dirEntries.Length > 0 && (IsKeyPressed(KeyboardKey.Up))) {
                _currentIndex--;
                _shouldRedrawLevelReview = true;

                Utils.Restrict(ref _currentIndex, 0, _dirEntries.Length - 1);
            }

            if (IsKeyPressed(KeyboardKey.Left)) {
                NavigateUp();
                _shouldRedrawLevelReview = true;
            }

            if (_dirEntries.Length > 0 && _currentIndex > -1 && _currentIndex < _dirEntries.Length && IsKeyPressed(KeyboardKey.Right) && _dirEntries[_currentIndex].isDir) {
                NavigateToDir(_dirEntries[_currentIndex].path);
                _shouldRedrawLevelReview = true;
            }

            if (_dirEntries.Length > 0 && IsKeyPressed(KeyboardKey.Enter)) {
                if (_dirEntries[_currentIndex].isDir) {
                    NavigateToDir(_dirEntries[_currentIndex].path);
                    _shouldRedrawLevelReview = true;
                } else {
                    _uiLocked = true;
                }
            } 
        }

        BeginDrawing();
        {
            if (_uiLocked)
            {
                ClearBackground(Color.Black);

                if (_dirEntries[_currentIndex].isDir || !_dirEntries[_currentIndex].path.EndsWith(".txt")) {
                    _uiLocked = false;
                    EndDrawing();
                    return;
                } 
                
                if (_loadFileTask is null)
                {
                    _loadFileTask = Utils.LoadProjectAsync(_dirEntries[_currentIndex].path);
                    EndDrawing();
                    return;
                }
                if (!_loadFileTask.IsCompleted)
                {
                    DrawText("Loading. Please wait..", (GetScreenWidth() - MeasureText("Loading. Please wait..", 30))/2, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                    EndDrawing();
                    return;
                }

                var result = _loadFileTask.Result;

                if (!result.Success)
                {
                    _loadFileTask = null;
                    _openFileDialog = null;
                    _uiLocked = false;
                    EndDrawing();
                    return;
                }
                
                // Validate if tiles are defined in Init.txt
                if (GLOBALS.TileCheck is null)
                {
                    GLOBALS.TileCheck = Task.Factory.StartNew(() => CheckTileIntegrity(result));

                    EndDrawing();
                    return;
                }
                
                // Validate if props have textures
                if (GLOBALS.PropCheck is null)
                {
                    GLOBALS.PropCheck = Task.Factory.StartNew(() => CheckPropIntegrity(result));
                    
                    EndDrawing();
                    return;
                }

                if (!GLOBALS.TileCheck.IsCompleted) {
                    DrawText("Validating Tiles..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    EndDrawing();
                    return;
                }
                
                if (!GLOBALS.PropCheck.IsCompleted)
                {
                    DrawText("Validating Props..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    EndDrawing();
                    return;
                }

                // Tile check failure
                if (GLOBALS.TileCheck.Result != TileCheckResult.Ok && !GLOBALS.Settings.TileEditor.AllowUndefinedTiles)
                {
                    GLOBALS.Page = 13;
                    _uiLocked = false;
                    
                    EndDrawing();
                    return;
                }
                

                // Prop check failure
                if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                {
                    GLOBALS.Page = 19;
                    _uiLocked = false;
                    
                    EndDrawing();
                    return;
                }
                
                Utils.AppendRecentProjectPath(_dirEntries[_currentIndex].path);
                
                Logger.Debug("Globals.Level.Import()");
                
                GLOBALS.Level.Import(
                    result.Width, 
                    result.Height,
                    (result.BufferTiles.Left, result.BufferTiles.Top, result.BufferTiles.Right, result.BufferTiles.Bottom),
                    result.GeoMatrix!,
                    result.TileMatrix!,
                    result.MaterialColorMatrix!,
                    result.Effects,
                    result.Cameras,
                    result.PropsArray!,
                    result.LightSettings,
                    result.LightMode,
                    result.DefaultTerrain,
                    result.Seed,
                    result.WaterLevel,
                    result.WaterInFront,
                    result.DefaultMaterial,
                    result.Name
                );
                
                #if DEBUG
                Logger.Debug($"Adjusting {nameof(GLOBALS.CamQuadLocks)}");
                #endif

                GLOBALS.CamQuadLocks = new int[result.Cameras.Count];

                #if DEBUG
                Logger.Debug($"Importing lightmap texture");
                #endif
                
                var lightMapTexture = LoadTextureFromImage(result.LightMapImage);

                UnloadRenderTexture(GLOBALS.Textures.LightMap);
                
                GLOBALS.Textures.LightMap = LoadRenderTexture(
                    GLOBALS.Level.Width * GLOBALS.Scale + 300, 
                    GLOBALS.Level.Height * GLOBALS.Scale + 300
                );

                BeginTextureMode(GLOBALS.Textures.LightMap);
                DrawTextureRec(
                    lightMapTexture,
                    new(0, 0, lightMapTexture.Width, lightMapTexture.Height),
                    new(0, 0),
                    new(255, 255, 255, 255)
                );
                
                EndTextureMode();

                UnloadImage(result.LightMapImage);

                UnloadTexture(lightMapTexture);
                
                #if DEBUG
                Logger.Debug($"Updating project name");
                #endif

                GLOBALS.Level.ProjectName = result.Name;
                GLOBALS.Page = 1;
                
                GLOBALS.Textures.GeneralLevel =
                    LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                
                ProjectLoaded?.Invoke(this, new LevelLoadedEventArgs(GLOBALS.TileCheck?.Result is TileCheckResult.Missing or TileCheckResult.MissingTexture));

                GLOBALS.TileCheck = null;
                GLOBALS.PropCheck = null;
                _loadFileTask = null;
                
                #if DEBUG
                Logger.Debug($"Invoking {nameof(ProjectLoaded)} event");
                #endif
                
                var parent = Directory.GetParent(_dirEntries[_currentIndex].path)?.FullName;
                    
                GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_dirEntries[_currentIndex].path);

                _uiLocked = false;
            }
            else
            {
                if (GLOBALS.Settings.GeneralSettings.DarkTheme) ClearBackground(new Color(100, 100, 100, 255));
                else ClearBackground(new Color(170, 170, 170, 255));

                if (_currentIndex > -1 && _currentIndex < _dirEntries.Length && !_dirEntries[_currentIndex].isDir) {
                    if (_shouldRedrawLevelReview) {
                        _shouldRedrawLevelReview = false;

                        LoadLevelPreview(_dirEntries[_currentIndex].path);
                    }
                }

                #region ImGui
                rlImGui.Begin();

                var winSize = new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80);


                if (ImGui.Begin("Start##StartupWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking)) {
                    ImGui.SetWindowSize(winSize);
                    ImGui.SetWindowPos(new(40, 40));
                    
                    ImGui.Columns(2);

                    var createClicked = ImGui.Button("Create", ImGui.GetContentRegionAvail() with { Y = 20 });

                    ImGui.Separator();

                    var upALevelClicked = rlImGui.ImageButtonSize("##Up", GLOBALS.Settings.GeneralSettings.DarkTheme ? _upTexture : _upBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    var homeClicked = rlImGui.ImageButtonSize("##Home", GLOBALS.Settings.GeneralSettings.DarkTheme ? _homeTexture : _homeBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

                    var pathUpdated = ImGui.InputText("##FilePathBuffer", ref _currentDir, 260, ImGuiInputTextFlags.AutoSelectAll);

                    var listAvailSpace = ImGui.GetContentRegionAvail();

                    if (ImGui.BeginListBox("##StartPageFileExplorerList", listAvailSpace with { Y = listAvailSpace.Y - 60 })) {
                        
                        for (var i = 0; i < _dirEntries.Length; i++) {

                            if (_dirEntries[i].isDir) rlImGui.ImageSize(GLOBALS.Settings.GeneralSettings.DarkTheme ? _folderTexture : _folderBlackTexture, new(23, 23));
                            else rlImGui.ImageSize(GLOBALS.Settings.GeneralSettings.DarkTheme ? _fileTexture : _fileBlackTexture, new(23, 23));
                            ImGui.SameLine();
                            var selected = ImGui.Selectable(_dirEntries[i].name, _currentIndex == i, ImGuiSelectableFlags.None, ImGui.GetContentRegionAvail() with { Y = 20 });

                            if (selected) {
                                if (_currentIndex == i) {
                                    if (_dirEntries[i].isDir) NavigateToDir(_dirEntries[i].path);
                                    else {
                                        _uiLocked = true;
                                    }
                                } else {
                                    _currentIndex = i;
                                    _shouldRedrawLevelReview = true;
                                }
                            }
                        }
                        
                        ImGui.EndListBox();
                    }

                    if (createClicked) {
                        GLOBALS.Page = 11;
                    }

                    if (pathUpdated) {
                        if (!Directory.Exists(_currentDir)) {
                            _dirEntries = [];
                        } else {
                            NavigateToDir(_currentDir);
                            GLOBALS.ProjectPath = _currentDir;
                        }
                    }

                    var loadableLevel = _currentIndex >= 0 && 
                        _currentIndex < _dirEntries.Length && 
                        !_dirEntries[_currentIndex].isDir;

                    if (!loadableLevel) ImGui.BeginDisabled();

                    var openLevelClicked = ImGui.Button("Open Level", ImGui.GetContentRegionAvail() with { Y = 20 });
                    
                    if (!loadableLevel) ImGui.EndDisabled();
                    
                    var cancelClicked = ImGui.Button("Cancel", ImGui.GetContentRegionAvail() with { Y = 20 });

                    if (openLevelClicked) {
                        _uiLocked = true;
                            
                    }

                    if (cancelClicked) GLOBALS.Page = GLOBALS.PreviousPage;

                    //

                    if (homeClicked) {
                        NavigateToDir(GLOBALS.Paths.ProjectsDirectory);
                    }

                    if (upALevelClicked) {
                        NavigateUp();
                    }

                    ImGui.NextColumn();

                    rlImGui.ImageRenderTextureFit(_levelPreviewRT, false);

                    ImGui.End();
                }

                rlImGui.End();
                #endregion
            }
        }
        EndDrawing();
    }
}