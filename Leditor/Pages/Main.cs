using Leditor.Types;
using Leditor.Data.Tiles;
using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Leditor.Data.Props.Legacy;
using rlImGui_cs;
using Leditor.Renderer;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

internal class MainPage : EditorPage, IContextListener
{
    public override void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
        
        // _explorer.Dispose();
    }

    // private readonly FileX _explorer = new();
    
    internal event EventHandler? ProjectLoaded;

    private Camera2D _camera = new() { Zoom = 0.5f };

    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private Task<string>? _saveFileDialog;
    private Task<SaveProjectResult>? _saveResult;

    private bool _askForPath;
    private bool _failedToSave;
    private bool _undefinedTilesAlert;

    // Only overriden when _undefinedTilesAlert is set to true.
    private bool _missingTileDef;
    private bool _missingTileText;
    private bool _missingMatDef;

    private bool _isGuiLocked;

    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private int _fileDialogMode; // 0 - save, 1 - load

    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;

    private bool _showWater = true;
    private bool _showTiles = true;
    private bool _showProps = true;
    
    private bool _shouldRedrawLevel = true;

    private bool _isVisualsWinHovered;
    private bool _isVisualsWinDragged;

    private bool _isNavbarHovered;

    private bool _isRecentlyWinHovered;

    private int _selectedPaletteIndex;

    private bool _isInputBusy;

    private bool _isLevelsWinHovered;
    
    private DrizzleRenderWindow? _renderWindow;

    private bool _renderAfterSave;

    public void OnLevelSelected(int previous, int next)
    {
        _shouldRedrawLevel = true;
    }

    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        if (e is LevelLoadedEventArgs l)
        {
            _undefinedTilesAlert = l.UndefinedTiles || l.MissingTileTextures || l.UndefinedMaterials;
            _missingMatDef = l.UndefinedMaterials;
            _missingTileDef = l.UndefinedTiles;
            _missingTileText = l.MissingTileTextures;
        }

        _shouldRedrawLevel = true;
    }

    public void OnProjectCreated(object? sender, EventArgs e)
    {
        _shouldRedrawLevel = true;
    }

    public void OnPageUpdated(int previous, int @next) {
        if (next == 1) _shouldRedrawLevel = true;
    }

    public void OnGlobalResourcesUpdated()
    {
        _shouldRedrawLevel = true;
    }
    
    private async Task<SaveProjectResult> SaveProjectAsync(string path)
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Leditor.Serialization.Exporters.ExportAsync(GLOBALS.Level);

            // export light map
            // var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);
            //
            // unsafe
            // {
            //     ImageFlipVertical(&image);
            // }
            //
            // var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
            // var name = Path.GetFileNameWithoutExtension(path);
            //         
            // ExportImage(image, Path.Combine(parent, name+".png"));
            //
            // UnloadImage(image);

            var str = await strTask;
            
            Logger.Debug($"Saving to {GLOBALS.ProjectPath}");
            await File.WriteAllTextAsync(path, str);

            result = new(true);
        }
        catch (Exception e)
        {
            result = new(false, e);
        }

        return result;
    }
       
    private TileCheckResult CheckTileIntegrity(in LoadFileResult res)
    {
        HashSet<string> missingDefs = [];
        HashSet<string> missingTextures = [];
        HashSet<string> missingMaterials = [];
        
        for (int y = 0; y < res.Height; y++)
        {
            for (int x = 0; x < res.Width; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var cell = res.TileMatrix![y, x, z];

                    if (cell.Type is TileCellType.Head)
                    {
                        if (cell.TileDefinition is null) {
                            missingDefs.Add(cell.UndefinedName!);    
                        }
                        else if (cell.TileDefinition.Texture.Id == 0) {
                            missingTextures.Add(cell.TileDefinition!.Name);
                        }
                    }
                    else if (cell.Type is TileCellType.Material)
                    {
                        var materialName = cell.MaterialDefinition?.Name ?? cell.UndefinedName!;

                        if (!GLOBALS.MaterialColors.ContainsKey(materialName))
                        {
                            Logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
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

    // TODO: Fix
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


    public override void Draw()
    {
        var mouse = GetMousePosition();

        var width = GetScreenWidth();
        var height = GetScreenHeight();


        var isWinBusy = _isRecentlyWinHovered || _isNavbarHovered || 
                _isVisualsWinHovered || 
                _isVisualsWinDragged || 
                _isShortcutsWinHovered || 
                _isShortcutsWinDragged || 
                _isLevelsWinHovered;

        GLOBALS.LockNavigation = _isInputBusy;

        var ctrl = IsKeyDown(KeyboardKey.LeftControl) || IsKeyDown(KeyboardKey.RightControl);
        var shift = IsKeyDown(KeyboardKey.LeftShift) || IsKeyDown(KeyboardKey.RightShift);
        var alt = IsKeyDown(KeyboardKey.LeftAlt) || IsKeyDown(KeyboardKey.RightAlt);

        if (!_isGuiLocked && !isWinBusy)
        {
            // handle zoom
            var mainPageWheel = GetMouseWheelMove();
            if (mainPageWheel != 0)
            {
                var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                _camera.Offset = GetMousePosition();
                _camera.Target = mouseWorldPosition;
                _camera.Zoom += mainPageWheel * GLOBALS.ZoomIncrement;
                if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
            }

            // handle mouse drag
            if (IsMouseButtonDown(MouseButton.Middle))
            {
                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }
        }
        

        // BeginDrawing();
        {
            if (_isGuiLocked)
            {
                ClearBackground(Color.Black);

                switch (_fileDialogMode)
                {
                    case 0:
                    {
                        if (_askForPath)
                        {
                            if (_saveFileDialog is null)
                            {
                                if (_saveResult!.IsCompleted)
                                {
                                    GLOBALS.Page = 1;
                                    _isGuiLocked = false;
                                    GLOBALS.LockNavigation = false;
                                }
                            }
                            else
                            {
                                if (!_saveFileDialog.IsCompleted)
                                {
                                    // EndDrawing();
                                    return;
                                }
                                if (string.IsNullOrEmpty(_saveFileDialog.Result))
                                {
                                    _isGuiLocked = false;
                                    GLOBALS.LockNavigation = false;
                                    // EndDrawing();
                                    return;
                                }

                                var path = _saveFileDialog.Result;

                                if (_saveResult is null)
                                {
                                    _saveResult = SaveProjectAsync(path);
                                    // EndDrawing();
                                    return;
                                }
                                if (!_saveResult.IsCompleted)
                                {
                                    // EndDrawing();
                                    return;
                                }

                                var result = _saveResult.Result;

                                if (!result.Success)
                                {
                                    _failedToSave = true;
                                    _isGuiLocked = false;
                                    GLOBALS.LockNavigation = false;
                                    // EndDrawing();
                                    #if DEBUG
                                    if (result.Exception is not null) Logger.Error($"Failed to save project: {result.Exception}");
                                    #endif
                                    _saveResult = null;
                                    _saveFileDialog = null;
                                    return;
                                }
                                
                                // export light map
                                {
                                    BeginTextureMode(GLOBALS.Textures.LightMap);
                                    DrawRectangle(0, 0, 1, 1, Color.Black);
                                    DrawRectangle(GLOBALS.Textures.LightMap.Texture.Width - 1, GLOBALS.Textures.LightMap.Texture.Height - 1, 1, 1, Color.Black);
                                    EndTextureMode();
                                    
                                    var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);

                                    unsafe
                                    {
                                        ImageFlipVertical(&image);
                                    }

                                    var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                                    var name = Path.GetFileNameWithoutExtension(path);

                                    ExportImage(image, Path.Combine(parent, name + ".png"));

                                    UnloadImage(image);
                                }

                                {
                                    var parent = Directory.GetParent(_saveFileDialog.Result)?.FullName;

                                    GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                                    GLOBALS.Level.ProjectName =
                                        Path.GetFileNameWithoutExtension(_saveFileDialog.Result);

                                    _saveFileDialog = null;
                                    _saveResult = null;
                                    _isGuiLocked = false;
                                    GLOBALS.LockNavigation = false;

                                    // Export level image to cache
                                    if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"))) {
                                        Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));
                                    }

                                    using var levelImg = Printers.GenerateLevelReviewImage();

                                    ExportImage(levelImg, Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", GLOBALS.Level.ProjectName+".png"));

                                    //

                                    // EndDrawing();
                                }
                            }
                        }
                        else
                        {
                            var path = Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName + ".txt");
                            
                            if (_saveResult is null)
                            {
                                _saveResult = SaveProjectAsync(path);
                                // EndDrawing();
                                return;
                            }
                            if (!_saveResult.IsCompleted)
                            {
                                // EndDrawing();
                                return;
                            }

                            var result = _saveResult.Result;

                            if (!result.Success)
                            {
                                _failedToSave = true;
                                _isGuiLocked = false;
                                GLOBALS.LockNavigation = false;
                                // EndDrawing();
                                #if DEBUG
                                if (result.Exception is not null) Logger.Error($"Failed to save project: {result.Exception}");
                                #endif
                                _saveResult = null;
                                _saveFileDialog = null;
                                return;
                            }
                            
                            // export light map
                            {
                                var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.Texture);

                                unsafe
                                {
                                    ImageFlipVertical(&image);
                                }

                                var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                                var name = Path.GetFileNameWithoutExtension(path);

                                ExportImage(image, Path.Combine(parent, name + ".png"));

                                // Export level image to cache
                                if (!Directory.Exists(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"))) {
                                    Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews"));
                                }

                                using var levelImg = Printers.GenerateLevelReviewImage();

                                ExportImage(levelImg, Path.Combine(GLOBALS.Paths.CacheDirectory, "levelpreviews", GLOBALS.Level.ProjectName+".png"));

                                //

                                UnloadImage(image);
                            }
                            
                            _saveFileDialog = null;
                            _saveResult = null;
                            _isGuiLocked = false;
                            GLOBALS.LockNavigation = false;
                            // EndDrawing();
                        }
                    }
                        break;

                    case 1:
                    {
                        if (_openFileDialog!.IsCompleted != true)
                        {
                            DrawText("Please wait..", GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                            // EndDrawing();
                            return;
                        }
                        if (string.IsNullOrEmpty(_openFileDialog.Result))
                        {
                            _openFileDialog = null;
                            _isGuiLocked = false;
                            GLOBALS.LockNavigation = false;
                            // EndDrawing();
                            return;
                        }
                        if (_loadFileTask is null)
                        {
                            _loadFileTask = Utils.LoadProjectAsync(_openFileDialog.Result);
                            // EndDrawing();
                            return;
                        }
                        if (!_loadFileTask.IsCompleted)
                        {
                            DrawText("Loading. Please wait..", (GetScreenWidth() - MeasureText("Loading. Please wait..", 30))/2, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                            // EndDrawing();
                            return;
                        }

                        var result = _loadFileTask.Result;

                        if (!result.Success)
                        {
                            _loadFileTask = null;
                            _openFileDialog = null;
                            _isGuiLocked = false;
                            GLOBALS.LockNavigation = false;
                            // EndDrawing();
                            return;
                        }
                        
                        Utils.AppendRecentProjectPath(_openFileDialog.Result);
                        
                        // Validate if tiles are defined in Init.txt
                        if (GLOBALS.TileCheck is null)
                        {
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
                        
                        if (!GLOBALS.TileCheck.IsCompleted || !GLOBALS.PropCheck.IsCompleted)
                        {
                            DrawText("Validating..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                            // EndDrawing();
                            return;
                        }
                        
                        // Tile check failure
                        if (GLOBALS.TileCheck.Result.MissingTileDefinitions.Count > 0 || GLOBALS.TileCheck.Result.MissingTileTextures.Count > 0 || GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Count > 0)
                        {
                            if (!GLOBALS.Settings.TileEditor.AllowUndefinedTiles)
                            {
                                GLOBALS.Page = 13;
                                _isGuiLocked = false;
                                GLOBALS.LockNavigation = false;
                                
                                // EndDrawing();
                                return;
                            }

                            Logger.Error($"{GLOBALS.TileCheck.Result.MissingTileDefinitions.Count} tile definistions missing");
                            Logger.Error($"{GLOBALS.TileCheck.Result.MissingTileTextures.Count} tile textures missing");
                            Logger.Error($"{GLOBALS.TileCheck.Result.MissingMaterialDefinitions.Count} materials missing");

                            foreach (var missingDef in GLOBALS.TileCheck.Result.MissingTileDefinitions) Logger.Error($"Missing tile definition: {missingDef}");
                            foreach (var missingTexture in GLOBALS.TileCheck.Result.MissingTileTextures) Logger.Error($"Missing tile texture: {missingTexture}");
                            foreach (var missingMaterial in GLOBALS.TileCheck.Result.MissingMaterialDefinitions) Logger.Error($"Missing material definition: {missingMaterial}");
                    
                        }

                        // Prop check failure
                        if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                        {
                            GLOBALS.Page = 19;
                            _isGuiLocked = false;
                            GLOBALS.LockNavigation = false;
                            
                            // EndDrawing();
                            return;
                        }

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
                        
                        #if DEBUG
                        Logger.Debug($"Adjusting {nameof(GLOBALS.CamQuadLocks)}");
                        #endif

                        GLOBALS.CamQuadLocks = new int[result.Cameras.Count];
                        
                        #if DEBUG
                        Logger.Debug($"Updating project name");
                        #endif

                        GLOBALS.Level.ProjectName = result.Name;
                        GLOBALS.Page = 1;

                        _undefinedTilesAlert = GLOBALS.TileCheck?.Result.MissingTileDefinitions.Count > 0 || GLOBALS.TileCheck?.Result.MissingTileTextures.Count > 0 || GLOBALS.TileCheck?.Result.MissingMaterialDefinitions.Count > 0;
                        
                        GLOBALS.Textures.GeneralLevel =
                            LoadRenderTexture(GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20);

                        ProjectLoaded?.Invoke(this, new LevelLoadedEventArgs(GLOBALS.TileCheck?.Result.MissingTileDefinitions.Count > 0, GLOBALS.TileCheck?.Result.MissingTileTextures.Count > 0, GLOBALS.TileCheck?.Result.MissingMaterialDefinitions.Count > 0));
                        
                        GLOBALS.TileCheck = null;
                        GLOBALS.PropCheck = null;
                        _loadFileTask = null;
                        
                        #if DEBUG
                        Logger.Debug($"Invoking {nameof(ProjectLoaded)} event");
                        #endif
                        
                        var parent = Directory.GetParent(_openFileDialog.Result)?.FullName;
                            
                        GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                        GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_openFileDialog.Result);
                        _shouldRedrawLevel = true;

                        _isGuiLocked = false;
                        GLOBALS.LockNavigation = false;
                    }
                        break;
                    
                    default:
                        _isGuiLocked = false;
                        GLOBALS.LockNavigation = false;
                        break;
                }
                
                DrawText(
                    "Please wait..", 
                    (int)(width - MeasureText("Please wait..", 50))/2, 
                    100, 
                    50, 
                    Color.White
                );

                
            }
            else
            {
                ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                    ? Color.Black 
                    : new(170, 170, 170, 255));

                if (_shouldRedrawLevel)
                {
                    Printers.DrawLevelIntoBuffer(GLOBALS.Textures.GeneralLevel, new Printers.DrawLevelParams
                    {
                        CurrentLayer = 0,
                        Water = GLOBALS.Settings.GeneralSettings.Water,
                        WaterOpacity = GLOBALS.Settings.GeneralSettings.WaterOpacity,
                        WaterAtFront = GLOBALS.Level.WaterAtFront,
                        TileDrawMode = GLOBALS.Settings.GeneralSettings.DrawTileMode,
                        PropDrawMode = GLOBALS.Settings.GeneralSettings.DrawPropMode,
                        TilesLayer1 = _showTiles,
                        TilesLayer2 = _showTiles,
                        TilesLayer3 = _showTiles,
                        PropsLayer1 = _showProps,
                        PropsLayer2 = _showProps,
                        PropsLayer3 = _showProps,
                        HighLayerContrast = GLOBALS.Settings.GeneralSettings.HighLayerContrast,
                        CurrentLayerAtFront = GLOBALS.Settings.GeneralSettings.CurrentLayerAtFront,
                        Palette = GLOBALS.SelectedPalette,
                        CropTilePrevious = GLOBALS.Settings.GeneralSettings.CropTilePreviews,
                        VisibleStrayTileFragments = false,
                        MaterialWhiteSpace = GLOBALS.Settings.GeneralSettings.MaterialWhiteSpace,
                        PropOpacity = (byte)GLOBALS.Settings.GeneralSettings.PropOpacity,
                    });
                    _shouldRedrawLevel = false;
                }
                BeginMode2D(_camera);
                {
                    
                    DrawRectangleLinesEx(new Rectangle(-3, 37, GLOBALS.Level.Width * 20 + 6, GLOBALS.Level.Height * 20 + 6), 3, Color.White);
            
                    BeginShaderMode(GLOBALS.Shaders.VFlip);
                    SetShaderValueTexture(GLOBALS.Shaders.VFlip, GetShaderLocation(GLOBALS.Shaders.VFlip, "inputTexture"), GLOBALS.Textures.GeneralLevel.Texture);
                    DrawTexture(GLOBALS.Textures.GeneralLevel.Texture, 
                        0, 
                        40,
                        Color.White);
                    EndShaderMode();
                }
                EndMode2D();
                
                // Menu

                _isInputBusy = false;
                
                rlImGui.Begin();
                
                ImGui.DockSpaceOverViewport(ImGui.GetWindowDockID(), ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
                
                // Navigation bar
                
                if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);
                
                // render window

                if (_renderWindow is not null)
                {
                    // True == window is closed
                    if (_renderWindow.DrawWindow(Logger))
                    {
                        _renderWindow.Dispose();
                        _renderWindow = null;
                        
                        // Freeing as much memory as possible
                        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
                        GC.WaitForFullGCComplete();
                    }
                }
                
                //
                
                if (_failedToSave)
                {
                    if (ImGui.Begin("Error##ProjectSaveFail"))
                    {
                        
                        ImGui.Text("Failed to save your project");

                        var okSelected = ImGui.Button("Ok");

                        if (okSelected) _failedToSave = false;
                        
                        ImGui.End();
                    }
                }

                if (ImGui.Begin("Options##MainMenu"))
                {
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    ImGui.Text($"{GLOBALS.Level.ProjectName}");
                    
                    ImGui.SameLine();

                    var helpSelected = ImGui.Button("?");
                    if (helpSelected) GLOBALS.Page = 9;
                    
                    ImGui.Separator();

                    // Seed
                    
                    var seed = GLOBALS.Level.Seed;
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Seed", ref seed);
                    if (GLOBALS.Level.Seed != seed) GLOBALS.Level.Seed = seed;

                    _isInputBusy = _isInputBusy || ImGui.IsItemActive();

                    // Water Level
                    
                    var waterLevel = GLOBALS.Level.WaterLevel;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.InputInt("Water Level", ref waterLevel))
                    {
                        _shouldRedrawLevel = true;
                        if (waterLevel < -1) waterLevel = -1;
                        GLOBALS.Level.WaterLevel = waterLevel;
                    }

                    _isInputBusy = _isInputBusy || ImGui.IsItemActive();
                    
                    // Water Position
                    
                    var waterInFront = GLOBALS.Level.WaterAtFront;
                    if (ImGui.Checkbox("Water In Front", ref waterInFront))
                    {
                        _shouldRedrawLevel = true;
                        GLOBALS.Level.WaterAtFront = waterInFront;
                    }

                    _isInputBusy = _isInputBusy || ImGui.IsItemActive();
                    
                    ImGui.Separator();
                    
                    // Light Mode

                    var lightMode = GLOBALS.Level.LightMode;
                    var terrainMode = GLOBALS.Level.DefaultTerrain;
                    
                    ImGui.Checkbox("Light Mode", ref lightMode);
                    ImGui.Checkbox("Terrain Medium", ref terrainMode);

                    if (lightMode != GLOBALS.Level.LightMode) GLOBALS.Level.LightMode = lightMode;
                    if (terrainMode != GLOBALS.Level.DefaultTerrain) GLOBALS.Level.DefaultTerrain = terrainMode;
                    
                    // Buttons

                    var saveSelected = ImGui.Button("Save", availableSpace with { Y = 20 });
                    var saveAsSelected = ImGui.Button("Save as..", availableSpace with { Y = 20 });
                    var loadSelected = ImGui.Button("Load Project..", availableSpace with { Y = 20 });
                    var newSelected = ImGui.Button("New Project", availableSpace with { Y = 20 });

                    var renderSelected = ImGui.Button("RENDER", availableSpace with { Y = 20 });
                    
                    if (renderSelected || _renderAfterSave)
                    {
                        if (_renderAfterSave) {

                            if (File.Exists(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName + ".txt"))) {
                                Logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                                _renderWindow = new DrizzleRenderWindow();
                                _renderAfterSave = false;
                            } else {
                                ImGui.OpenPopup("Project Not Rendered!");

                                if (ImGui.BeginPopupModal("Project Not Rendered!")) {
                                    ImGui.Text("Render failed because the project was not saved.");

                                    ImGui.Spacing();
                                    
                                    if (ImGui.Button("Ok", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                                        ImGui.CloseCurrentPopup();
                                        _renderAfterSave = false;
                                    }
                                    
                                    ImGui.End();
                                }
                            }


                        } else {
                            if (GLOBALS.Settings.GeneralSettings.AutoSaveBeforeRendering) {
                                
                                if (GLOBALS.NewlyCreated) GLOBALS.Page = 12;
                                else  {
                                    GLOBALS.LockNavigation = true;
                                    _fileDialogMode = 0;
                                    _isGuiLocked = true;
                                    _askForPath = false;
                                }
                                
                                _renderAfterSave = true;
                            } else {
                                Logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                                _renderWindow = new DrizzleRenderWindow();
                            }
                        }

                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Requires saving the project to render the new changes");
                        ImGui.EndTooltip();
                    }
                    
                    if (saveSelected)
                    {
                        if (GLOBALS.NewlyCreated)
                        {
                            GLOBALS.Page = 12;
                        }
                        else
                        {
                            _fileDialogMode = 0;
                            _askForPath = false;
                            _isGuiLocked = true;
                            GLOBALS.LockNavigation = true;
                        }

                    }

                    if (saveAsSelected)
                    {
                        // _fileDialogMode = 0;
                        // _askForPath = true;
                        // _saveFileDialog = Utils.SetFilePathAsync();
                        // _isGuiLocked = true;
                        // GLOBALS.LockNavigation = true;

                        GLOBALS.Page = 12;
                    }
                    
                    if (loadSelected)
                    {
                        // _fileDialogMode = 1;
                        // _openFileDialog = Utils.GetFilePathAsync();
                        // _isGuiLocked = true;
                        // GLOBALS.LockNavigation = true;

                        GLOBALS.Page = 0;
                    }
                    
                    if (newSelected)
                    {
                        GLOBALS.Page = 11;
                    }
                    ImGui.End();
                }

                var recentlyOpened = ImGui.Begin("Recently Opened Projects##MainRecentProjects");
                
                var recentlyPos = ImGui.GetWindowPos();
                var recentlySpace = ImGui.GetWindowSize();

                _isRecentlyWinHovered = CheckCollisionPointRec(GetMousePosition(), new(recentlyPos.X - 5, recentlyPos.Y, recentlySpace.X + 10, recentlySpace.Y));

                if (ImGui.Begin("Recently Opened Projects##MainRecentProjects"))
                {
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    if (ImGui.BeginListBox("##RecentProjectsList", availableSpace))
                    {
                        var counter = 0;
                        
                        foreach (var (path, name) in GLOBALS.RecentProjects)
                        {
                            counter++;
                            
                            var selected = ImGui.Selectable($"{counter}. {name}");

                            if (selected)
                            {
                                // TODO: Load Project Protocol

                                GLOBALS.LockNavigation = true;
                                _fileDialogMode = 1;
                                _openFileDialog = Task.FromResult(path);
                                _isGuiLocked = true;
                            }
                        }
                        ImGui.EndListBox();
                    }
                    ImGui.End();
                }

                if (_undefinedTilesAlert)
                {
                    if (ImGui.Begin("Alert##UndefinedTilesAlert", 
                            ImGuiWindowFlags.Modal | 
                            ImGuiWindowFlags.NoCollapse))
                    {
                        var winSize = ImGui.GetWindowSize();
                        var screenSize = new Vector2(width, height);
                        
                        ImGui.SetWindowPos((screenSize - winSize)/2);
                        if (_missingTileDef) ImGui.Text("This  project contains undefined tiles.");
                        if (_missingTileText) ImGui.Text("Textures are still loading. Be ware that if you forgot to include your custom tile textures, they will not show in the level.");
                        if (_missingMatDef) ImGui.Text("This  project contains undefined materials.");

                        ImGui.Spacing();
                        
                        var neverShow = GLOBALS.Settings.GeneralSettings.NeverShowMissingTileTexturesAlertAgain;
                        if (ImGui.Checkbox("Don't Show Again", ref neverShow)) {
                            GLOBALS.Settings.GeneralSettings.NeverShowMissingTileTexturesAlertAgain = neverShow;
                        }
                        
                        var okSelected = ImGui.Button("Ok", ImGui.GetContentRegionAvail() with { Y = 20 });
                        
                        if (okSelected) {
                            _undefinedTilesAlert = false;
                            GLOBALS.Settings.GeneralSettings.NeverShowMissingTileTexturesAlertAgain = neverShow;
                        }
                        
                        ImGui.End();
                    }
                }
                
                //
                
                // Settings Window

                if (ImGui.Begin("Settings##MainSettings"))
                {
                    var availableSpace = ImGui.GetContentRegionAvail();
                    
                    // Dark Theme
                    
                    var darkTheme = GLOBALS.Settings.GeneralSettings.DarkTheme;
                    ImGui.Checkbox("Dark Theme", ref darkTheme);
                    if (darkTheme != GLOBALS.Settings.GeneralSettings.DarkTheme)
                        GLOBALS.Settings.GeneralSettings.DarkTheme = darkTheme;
                    
                    // Shortcuts Window
                    
                    var showShortcutsWindow = GLOBALS.Settings.GeneralSettings.ShortcutWindow;
                    ImGui.Checkbox("Shortcuts Window", ref showShortcutsWindow);
                    if (showShortcutsWindow != GLOBALS.Settings.GeneralSettings.ShortcutWindow) 
                        GLOBALS.Settings.GeneralSettings.ShortcutWindow = showShortcutsWindow;
                    
                    ImGui.SeparatorText("Visibility");

                    if (ImGui.Checkbox("Water", ref _showWater))
                    {
                        _shouldRedrawLevel = true;
                    }
                    

                    // Save Settings
                    
                    if (ImGui.Button("Save Changes", availableSpace with { Y = 20 }))
                    {
                        try
                        {
                            var text = JsonSerializer.Serialize(GLOBALS.Settings, GLOBALS.JsonSerializerOptions);
                            File.WriteAllText(GLOBALS.Paths.SettingsPath, text);
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Failed to save changes: {e}");
                        }
                    }

                    ImGui.End();
                }

                var visualsWinOpened = ImGui.Begin("Visuals##MainTextureVisualsWindow");

                var visualsWinPos = ImGui.GetWindowPos();
                var visualsWinSize = ImGui.GetWindowSize();

                //
                if (CheckCollisionPointRec(mouse, new(visualsWinPos.X - 5, visualsWinPos.Y, visualsWinSize.X + 10, visualsWinSize.Y)))
                {
                    _isVisualsWinHovered = true;

                    if (IsMouseButtonDown(MouseButton.Left)) _isVisualsWinDragged = true;
                }
                else
                {
                    _isVisualsWinHovered = false;
                }

                if (IsMouseButtonReleased(MouseButton.Left) && _isVisualsWinDragged) _isVisualsWinDragged = false;
                //

                if (visualsWinOpened) {
                    if (ImGui.Button("Take a screeshot", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        if (!Directory.Exists(GLOBALS.Paths.ScreenshotsDirectory))
                            Directory.CreateDirectory(GLOBALS.Paths.ScreenshotsDirectory);
                        
                        var img = LoadImageFromTexture(GLOBALS.Textures.GeneralLevel.Texture);

                        ImageFlipVertical(ref img);

                        ExportImage(img, Path.Combine(GLOBALS.Paths.ScreenshotsDirectory, $"screenshot-{(DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss"))}.png"));

                        UnloadImage(img);
                    }

                    //
                    var highContrast = GLOBALS.Settings.GeneralSettings.HighLayerContrast;

                    if (ImGui.Checkbox("Non-current layer is transparent", ref highContrast))
                    {
                        GLOBALS.Settings.GeneralSettings.HighLayerContrast = highContrast;
                        _shouldRedrawLevel = true;
                    }

                    var visiblePreceeding = GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers;

                    if (ImGui.Checkbox("Visible Preceding Unfocused Layers", ref visiblePreceeding)) {
                        _shouldRedrawLevel = true;
                        GLOBALS.Settings.GeneralSettings.VisiblePrecedingUnfocusedLayers = visiblePreceeding;
                    }

                    // Water
                    var water = GLOBALS.Settings.GeneralSettings.Water;
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.Checkbox("Water", ref water)) {
                        GLOBALS.Settings.GeneralSettings.Water = water;
                        _shouldRedrawLevel = true;
                    }

                    var waterOpacity = (int) GLOBALS.Settings.GeneralSettings.WaterOpacity;
                    if (ImGui.InputInt("Water Opacity", ref waterOpacity)) {
                        Utils.Restrict(ref waterOpacity, 0, 255);
                        GLOBALS.Settings.GeneralSettings.WaterOpacity = (byte) waterOpacity;
                        _shouldRedrawLevel = true;
                    }

                    _isInputBusy = _isInputBusy || ImGui.IsItemActive();
                    //

                    var layerAboveAll = GLOBALS.Settings.GeneralSettings.CurrentLayerAtFront;
                    if (ImGui.Checkbox("Current Layer Above All", ref layerAboveAll)) {
                        GLOBALS.Settings.GeneralSettings.CurrentLayerAtFront = layerAboveAll;
                        _shouldRedrawLevel = true;
                    }

                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Preview) ImGui.BeginDisabled();

                    var cropTilePreviews = GLOBALS.Settings.GeneralSettings.CropTilePreviews;

                    if (ImGui.Checkbox("Crop Tile Previews", ref cropTilePreviews)) {
                        GLOBALS.Settings.GeneralSettings.CropTilePreviews = cropTilePreviews;
                        _shouldRedrawLevel = true;
                    }

                    if (ImGui.IsItemHovered()) {
                        ImGui.BeginTooltip();

                        ImGui.Text("Tiles that take up more than one layer will only render the parts that intersect with their layers");

                        ImGui.EndTooltip();
                    }

                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode != TileDrawMode.Preview) ImGui.EndDisabled();


                    var tileVisual = (int)GLOBALS.Settings.GeneralSettings.DrawTileMode;
                    var propVisual = (int)GLOBALS.Settings.GeneralSettings.DrawPropMode;

                    if (ImGui.Checkbox("Show Tiles", ref _showTiles)) _shouldRedrawLevel = true;

                    if (ImGui.Combo("Tiles", ref tileVisual, GLOBALS.Textures.Palettes.Length > 0 ? "Preview\0Tinted Texture\0Palette" : "Preview\0Tinted Texture")) {
                        _shouldRedrawLevel = true;
                        GLOBALS.Settings.GeneralSettings.DrawTileMode = (TileDrawMode)tileVisual;
                    }

                    ImGui.Spacing();

                    if (ImGui.Checkbox("Show Props", ref _showProps)) _shouldRedrawLevel = true;
                    
                    if (!_showProps) ImGui.BeginDisabled();
                    var propOpacity = (int)GLOBALS.Settings.GeneralSettings.PropOpacity;
                    if (ImGui.SliderInt("Prop Opacity", ref propOpacity, 1, 255))
                    {
                        GLOBALS.Settings.GeneralSettings.PropOpacity = (byte) propOpacity;
                        _shouldRedrawLevel = true;
                    }
                    if (!_showProps) ImGui.EndDisabled();

                    if (ImGui.Combo("Props", ref propVisual, GLOBALS.Textures.Palettes.Length > 0 ?  "Untinted Texture\0Tinted Texture\0Palette" : "Untinted Texture\0Tinted Texture")) {
                        _shouldRedrawLevel = true;
                        GLOBALS.Settings.GeneralSettings.DrawPropMode = (PropDrawMode)propVisual;
                    }

                    if (GLOBALS.Settings.GeneralSettings.DrawTileMode == TileDrawMode.Palette || 
                        GLOBALS.Settings.GeneralSettings.DrawPropMode == PropDrawMode.Palette) {
                        
                        ImGui.SeparatorText("Palettes");

                        if (GLOBALS.Textures.Palettes.Length > 0) {
                            if (ImGui.BeginListBox("##Visuals", ImGui.GetContentRegionAvail())) {
                                
                                for (var index = 0; index < GLOBALS.Textures.Palettes.Length; index++) {
                                    var selected = ImGui.Selectable(GLOBALS.Textures.PaletteNames[index], index == _selectedPaletteIndex);
                                    
                                    if (selected) {
                                        _selectedPaletteIndex = index;
                                        GLOBALS.SelectedPalette = GLOBALS.Textures.Palettes[index];
                                        _shouldRedrawLevel = true;
                                    }

                                    if (ImGui.IsItemHovered()) {
                                        ImGui.BeginTooltip();

                                        rlImGui.ImageSize(GLOBALS.Textures.Palettes[index], new Vector2(320, 160));
                                        
                                        ImGui.EndTooltip();
                                    }
                                }

                                ImGui.EndListBox();
                            }
                        } else {
                            ImGui.Text("No palettes loaded");
                        }

                    }

                    
                    ImGui.End();
                }
                
                // Shortcuts Window
                if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
                {
                    var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.GlobalShortcuts);

                    _isShortcutsWinHovered = CheckCollisionPointRec(
                        GetMousePosition(), 
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
                
                // Levels window

                _isLevelsWinHovered = Printers.ImGui.LevelsWindow();
                
                rlImGui.End();
            }
        }
        // EndDrawing();
    }
}
