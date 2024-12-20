using System.Numerics;
using ImGuiNET;
using rlImGui_cs;
using static Raylib_cs.Raylib;

using Leditor.Data.Geometry;
using Leditor.Data.Tiles;
using Leditor.Data.Props.Legacy;

namespace Leditor.Pages;


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
        _refreshTexture.Dispose();

        _upBlackTexture.Dispose();
        _homeBlackTexture.Dispose();
        _folderBlackTexture.Dispose();
        _fileBlackTexture.Dispose();
        _refreshBlackTexture.Dispose();
    }

    private bool _loadFailed;
    private Exception? _loadException;


    internal event EventHandler? ProjectLoaded;

    private bool _uiLocked;

    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private TileCheckResult CheckTileIntegrity(in LoadFileResult res)
    {
        var result = TileCheckResultEnum.Ok;
        HashSet<string> missingDefs = [];
        HashSet<string> missingTextures = [];
        HashSet<string> missingMaterials = [];
        
        for (int y = 0; y < res.TileMatrix.GetLength(0); y++)
        {
            for (int x = 0; x < res.TileMatrix.GetLength(1); x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var cell = res.TileMatrix![y, x, z];

                    if (cell.Type is TileCellType.Head)
                    {
                        if (cell.TileDefinition is null) {
                            result = TileCheckResultEnum.Missing;
                            missingDefs.Add(cell.UndefinedName ?? "NULL");    
                        }
                        else if (cell.TileDefinition.Texture.Id == 0) {
                            result = TileCheckResultEnum.MissingTexture;
                            missingTextures.Add(cell.TileDefinition.Name);
                        }
                    }
                    else if (cell.Type == TileCellType.Material)
                    {
                        var materialName = cell.MaterialDefinition?.Name ?? cell.UndefinedName ?? "NULL";

                        if (!GLOBALS.MaterialColors.ContainsKey(materialName))
                        {
                            Logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
                            result = TileCheckResultEnum.MissingMaterial;
                            missingMaterials.Add(materialName);
                        }
                    }

                    skip:
                    { }
                }
            }
        }

        Logger.Debug("tile check passed");

        return new TileCheckResult {
            MissingMaterialDefinitions = missingMaterials,
            MissingTileDefinitions = missingDefs,
            MissingTileTextures = missingTextures
        };
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
                _ = prop.Type switch
                {
                    InitPropType_Legacy.Long => GLOBALS.Textures.LongProps[prop.Position.index],
                    InitPropType_Legacy.Rope => GLOBALS.Textures.RopeProps[prop.Position.index],
                    InitPropType_Legacy.Tile => prop.Tile?.Texture ?? throw new NullReferenceException(),
                    _ => GLOBALS.Textures.Props[prop.Position.category][prop.Position.index]
                };

                // No IndexOutOfRangeException exception was thrown - Success
            }
            catch
            {
                var path = prop.Type == InitPropType_Legacy.Tile
                    ? Path.Combine(GLOBALS.Paths.TilesAssetsDirectory, prop.Name+".png")
                    : Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, prop.Name + ".png");
                
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
    private RL.Managed.Texture2D _refreshTexture;
    
    private RL.Managed.Texture2D _upBlackTexture;
    private RL.Managed.Texture2D _homeBlackTexture;
    private RL.Managed.Texture2D _folderBlackTexture;
    private RL.Managed.Texture2D _fileBlackTexture;
    private RL.Managed.Texture2D _refreshBlackTexture;


    private bool _shouldRedrawLevelReview = true;

    //
    private string _currentDir;
    private (string path, string name, bool isDir)[] _dirEntries;

    private int _currentIndex = -1;

    private void NavigateToDir(string path) {
        if (!Directory.Exists(path)) return;

        _currentDir = path;

        var entries = Directory
            .GetFileSystemEntries(path)
            .Select(e => {
                var attrs = File.GetAttributes(e);

                return (e, (attrs & FileAttributes.Directory) == FileAttributes.Directory);
            })
            .Where(e => e.Item2 || e.Item1.EndsWith(".txt"))
            .Select(e => {
                return (e.Item1, Path.GetFileNameWithoutExtension(e.Item1), e.Item2);
            });

        var folders = entries.Where(e => e.Item3).OrderBy(e => e.Item2, StringComparer.OrdinalIgnoreCase);
        var files = entries.Where(e => !e.Item3).OrderBy(e => e.Item2, StringComparer.OrdinalIgnoreCase);

        _dirEntries = [..folders, ..files];
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
            Geo[,,] matrix;

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
            .OrderBy(e => !e.Item3)
            .ToArray();

        _levelPreviewRT = new(0, 0);
        
        _upTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon.png"));
        _homeTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon.png"));
        _folderTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon.png"));
        _fileTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon.png"));
        _refreshTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "rotate icon.png"));
    
        _upBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "up icon black.png"));
        _homeBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "home icon black.png"));
        _folderBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "folder icon black.png"));
        _fileBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "file icon black.png"));
        _refreshBlackTexture = new(Path.Combine(GLOBALS.Paths.UiAssetsDirectory, "rotate icon black.png"));
    }

    ~StartPage() {
        if (!Disposed) throw new InvalidOperationException("StartPage was not disposed by consumer");
    }

    private bool _columnWidthSetOnce;
    private bool _isPathBufferActive;

    private bool _isInputBusy;

    public override void Draw()
    {
        GLOBALS.LockNavigation = _isInputBusy;

        if (!_uiLocked) {
            _isInputBusy = false;

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

        // BeginDrawing();
        {
            if (_uiLocked)
            {
                ClearBackground(Color.Black);

                if (_dirEntries[_currentIndex].isDir || !_dirEntries[_currentIndex].path.EndsWith(".txt")) {
                    _uiLocked = false;
                    // EndDrawing();
                    return;
                } 
                
                if (_loadFileTask is null)
                {
                    _loadFileTask = Utils.LoadProjectAsync(_dirEntries[_currentIndex].path);
                    // EndDrawing();
                    return;
                }
                if (!_loadFileTask.IsCompleted)
                {
                    DrawText("Loading. Please wait..", (GetScreenWidth() - MeasureText("Loading. Please wait..", 30))/2, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                    // EndDrawing();
                    return;
                }

                if (_loadFileTask.IsFaulted) {
                    _loadFailed = true;
                    _loadException = _loadFileTask.Exception;

                    if (_loadException is not null) Logger.Error(_loadException, "Failed to load level");

                    _loadFileTask = null;
                    _openFileDialog = null;
                    _uiLocked = false;
                    // EndDrawing();
                    return;
                }

                var result = _loadFileTask.Result;

                if (!result.Success)
                {
                    _loadFailed = true;

                    _loadFileTask = null;
                    _openFileDialog = null;
                    _uiLocked = false;
                    // EndDrawing();
                    return;
                }
                
                // Validate if tiles are defined in Init.txt
                if (GLOBALS.TileCheck is null)
                {
                    // var res = Task.Factory.StartNew(() => CheckTileIntegrity(result));
                    GLOBALS.TileCheck = Task.Factory.StartNew(() => CheckTileIntegrity(result));

                    // EndDrawing();
                    return;
                }
                
                // Validate if props have textures
                if (GLOBALS.PropCheck is null)
                {
                    GLOBALS.PropCheck = Task.Factory.StartNew(() => CheckPropIntegrity(result));
                    
                    // EndDrawing();
                    return;
                }

                if (!GLOBALS.TileCheck.IsCompleted) {
                    DrawText("Validating Tiles..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    // EndDrawing();
                    return;
                }
                
                if (!GLOBALS.PropCheck.IsCompleted)
                {
                    DrawText("Validating Props..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    // EndDrawing();
                    return;
                }

                // Tile check failure
                if (GLOBALS.TileCheck.Result.MissingTileDefinitions.Count > 0 || 
                        GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Count > 0 || 
                        GLOBALS.TileCheck.Result.MissingTileTextures.Count > 0)
                {
                    if (!GLOBALS.Settings.TileEditor.AllowUndefinedTiles) {
                        GLOBALS.Page = 13;
                        _uiLocked = false;
                        // EndDrawing();
                        return;
                    }


                    Logger.Error($"{GLOBALS.TileCheck.Result.MissingTileDefinitions.Count} tile definistions missing");
                    Logger.Error($"{GLOBALS.TileCheck.Result.MissingTileTextures.Count} tile textures missing");
                    Logger.Error($"{GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Count} materials missing");

                    foreach (var missingDef in GLOBALS.TileCheck.Result.MissingTileDefinitions) {
                        Logger.Error($"Missing tile definition: {missingDef}");
                        // GLOBALS.TileCheck.Result.MissingTileDefinitions.Remove(missingDef);
                    }

                    foreach (var missingTexture in GLOBALS.TileCheck.Result.MissingTileTextures) {
                        Logger.Error($"Missing tile texture: {missingTexture}");
                        // GLOBALS.TileCheck.Result.MissingTileTextures.Remove(missingTexture);
                    }

                    foreach (var missingMaterial in GLOBALS.TileCheck.Result.MissingMaterialDefinitions) {
                        Logger.Error($"Missing material definition: {missingMaterial}");
                        // GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Remove(missingMaterial);
                    }
                }
                

                // Prop check failure
                if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                {
                    GLOBALS.Page = 19;
                    _uiLocked = false;
                    
                    // EndDrawing();
                    return;
                }
                
                if (result.PropsLoadException is not null) {
                    Logger.Error($"Failed to load props: {result.PropsLoadException}");

                    GLOBALS.PropCheck = Task.FromResult(PropCheckResult.NotOk);
                }

                Utils.AppendRecentProjectPath(_dirEntries[_currentIndex].path);
                
                if (GLOBALS.AppendNewLevels)
                {
                    Data.LevelState newLevel = new(
                        10, 
                        10, 
                        (12, 6, 12, 5), 
                        result.DefaultMaterial
                    );
                    
                    newLevel.Import(
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
                        result.LightMapImage,
                        result.DefaultMaterial,
                        result.Name
                    );
                    
                    GLOBALS.Levels.Add(newLevel);
                    GLOBALS.Textures.LightMaps.Add(LoadRenderTexture(
                        GLOBALS.Level.Width * GLOBALS.Scale + 300, 
                        GLOBALS.Level.Height * GLOBALS.Scale + 300
                    ));
                    GLOBALS.SelectedLevel++;
                    
                    var lightMapTexture = LoadTextureFromImage(result.LightMapImage);

                    BeginTextureMode(GLOBALS.Textures.LightMap);
                    DrawTextureRec(
                        lightMapTexture,
                        new(0, 0, lightMapTexture.Width, lightMapTexture.Height),
                        new(0, 0),
                        new(255, 255, 255, 255)
                    );
                
                    EndTextureMode();

                    // UnloadImage(result.LightMapImage);

                    UnloadTexture(lightMapTexture);
                }
                else
                {
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
                        result.LightMapImage,
                        result.DefaultMaterial,
                        result.Name
                    );

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

                    // UnloadImage(result.LightMapImage);

                    UnloadTexture(lightMapTexture);
                }
                
                // GLOBALS.Level.Import(
                //     result.Width, 
                //     result.Height,
                //     (result.BufferTiles.Left, result.BufferTiles.Top, result.BufferTiles.Right, result.BufferTiles.Bottom),
                //     result.GeoMatrix!,
                //     result.TileMatrix!,
                //     result.MaterialColorMatrix!,
                //     result.Effects,
                //     result.Cameras,
                //     result.PropsArray!,
                //     result.LightSettings,
                //     result.LightMode,
                //     result.DefaultTerrain,
                //     result.Seed,
                //     result.WaterLevel,
                //     result.WaterInFront,
                //     result.LightMapImage,
                //     result.DefaultMaterial,
                //     result.Name
                // );
                
                #if DEBUG
                Logger.Debug($"Adjusting {nameof(GLOBALS.CamQuadLocks)}");
                #endif

                GLOBALS.CamQuadLocks = new int[result.Cameras.Count];
                
                #if DEBUG
                Logger.Debug($"Updating project name");
                #endif

                GLOBALS.Level.ProjectName = result.Name;
                GLOBALS.Page = 1;
                
                GLOBALS.Textures.GeneralLevel =
                    LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);
                
                ProjectLoaded?.Invoke(this, new LevelLoadedEventArgs(GLOBALS.TileCheck?.Result.MissingTileDefinitions.Count > 0, GLOBALS.TileCheck?.Result.MissingTileTextures.Count > 0, GLOBALS.TileCheck?.Result.MissingMaterialDefinitions.Count > 0));

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
                GLOBALS.NewlyCreated = false;
            }
            else
            {
                _isInputBusy = false;

                if (GLOBALS.Settings.GeneralSettings.DarkTheme) ClearBackground(new Color(100, 100, 100, 255));
                else ClearBackground(new Color(170, 170, 170, 255));

                if (_currentIndex > -1 && _currentIndex < _dirEntries.Length && !_dirEntries[_currentIndex].isDir) {
                    if (_shouldRedrawLevelReview) {
                        _shouldRedrawLevelReview = false;

                        LoadLevelPreview(_dirEntries[_currentIndex].path);
                    }
                }

                if (!_isPathBufferActive && IsKeyPressed(KeyboardKey.N)) {
                    GLOBALS.Page = 11;
                }

                #region ImGui
                rlImGui.Begin();

                var winSize = new Vector2(GetScreenWidth() - 80, GetScreenHeight() - 80);

                if (ImGui.Begin("Start##StartupWindow", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking)) {
                    ImGui.SetWindowSize(winSize);
                    ImGui.SetWindowPos(new(40, 40));
                    
                    ImGui.Columns(2);

                    if (!_columnWidthSetOnce) {
                        ImGui.SetColumnWidth(0, 300);
                        _columnWidthSetOnce = true;
                    }

                    var createClicked = ImGui.Button("Create", ImGui.GetContentRegionAvail() with { Y = 20 });

                    if (ImGui.IsItemHovered()) {
                        ImGui.BeginTooltip();
                        ImGui.Text("Press N to create a new level");
                        ImGui.EndTooltip();
                    }

                    ImGui.Separator();

                    var upALevelClicked = rlImGui.ImageButtonSize("##Up", GLOBALS.Settings.GeneralSettings.DarkTheme ? _upTexture : _upBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    var homeClicked = rlImGui.ImageButtonSize("##Home", GLOBALS.Settings.GeneralSettings.DarkTheme ? _homeTexture : _homeBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    var refreshClicked = rlImGui.ImageButtonSize("##Refresh", GLOBALS.Settings.GeneralSettings.DarkTheme ? _refreshTexture : _refreshBlackTexture, new(20, 20));
                    ImGui.SameLine();
                    
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

                    var pathUpdated = ImGui.InputText("##FilePathBuffer", ref _currentDir, 260, ImGuiInputTextFlags.AutoSelectAll);

                    _isInputBusy = _isPathBufferActive = ImGui.IsItemActive();
                    
                    var listAvailSpace = ImGui.GetContentRegionAvail();

                    if (ImGui.BeginListBox("##StartPageFileExplorerList", listAvailSpace with { Y = listAvailSpace.Y - 260 })) {
                        
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

                    ImGui.Spacing();

                    var appendLevel = GLOBALS.AppendNewLevels;
                    if (ImGui.Checkbox("New Tab", ref appendLevel))
                    {
                        GLOBALS.AppendNewLevels = appendLevel;
                    }

                    ImGui.SeparatorText("Recently Opened Levels");

                    if (ImGui.BeginListBox("##RecentlyOpenedList", listAvailSpace with { Y = 180 })) {
                        foreach (var (path, name) in GLOBALS.RecentProjects) {
                            var selected = ImGui.Selectable(name);

                            if (selected) {
                                var parent = Directory.GetParent(path)?.FullName;

                                if (parent is not null) {
                                    NavigateToDir(parent);

                                    for (var i = 0; i < _dirEntries.Length; i++) {
                                        if (_dirEntries[i].name == name) {
                                            _currentIndex = i;
                                            _shouldRedrawLevelReview = true;
                                        }
                                    }
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
                        // GLOBALS.TileCheck = null;
                        // GLOBALS.PropCheck = null;
                        _uiLocked = true;
                            
                    }

                    if (cancelClicked) GLOBALS.Page = GLOBALS.PreviousPage;

                    //

                    if (homeClicked) {
                        NavigateToDir(GLOBALS.Paths.ProjectsDirectory);
                    }

                    if (refreshClicked) {
                        NavigateToDir(_currentDir);
                    }

                    if (upALevelClicked) {
                        NavigateUp();
                    }

                    ImGui.NextColumn();

                    rlImGui.ImageRenderTextureFit(_levelPreviewRT, false);

                    ImGui.End();
                }

                if (_loadFailed) {
                    ImGui.OpenPopup("Load Failed");
                }

                if (ImGui.BeginPopupModal("Load Failed")) {

                    ImGui.Text("Failed to load the selected level. This might be due to data corruption or invalid level data.");

                    if (ImGui.Button("Ok")) {
                        _loadFailed = false;
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }

                rlImGui.End();
                #endregion
            }
        }
        // EndDrawing();
    }
}