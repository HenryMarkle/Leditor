using System.Diagnostics;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Leditor.Lingo.Drizzle;
using Pidgin;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;

namespace Leditor;

#nullable enable

internal class MainPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    private readonly Serilog.Core.Logger _logger = logger;
    
    internal event EventHandler? ProjectLoaded;

    private Camera2D _camera = camera ?? new Camera2D { zoom = 0.5f };

    private readonly byte[] _previewPanelBytes = "Level Options"u8.ToArray();
    
    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private Task<string>? _saveFileDialog;
    private Task<SaveProjectResult>? _saveResult;

    private bool _askForPath;
    private bool _failedToSave;
    private bool _undefinedTilesAlert;

    private int _spinnerLock;
    
    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private int _fileDialogMode; // 0 - save, 1 - load

    private System.Diagnostics.Process _renderProcess = new();
    
    private bool _isShortcutsWinHovered;
    private bool _isShortcutsWinDragged;
    
    private bool _isNavigationWinHovered;
    private bool _isNavigationWinDragged;

    public void OnLevelLoadedFromStart(object? sender, EventArgs e)
    {
        if (e is LevelLoadedEventArgs l)
        {
            _undefinedTilesAlert = l.UndefinedTiles;
        }
    }
    
    private async Task<SaveProjectResult> SaveProjectAsync(string path)
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Leditor.Lingo.Exporters.ExportAsync(GLOBALS.Level);

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
            
            _logger.Debug($"Saving to {GLOBALS.ProjectPath}");
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
        for (int y = 0; y < res.Height; y++)
        {
            for (int x = 0; x < res.Width; x++)
            {
                for (int z = 0; z < 3; z++)
                {
                    var cell = res.TileMatrix![y, x, z];

                    if (cell.Type == TileType.TileHead)
                    {
                        var (category, position, name) = ((TileHead)cell.Data).CategoryPostition;

                        // code readability could be optimized using System.Linq

                        for (var c = 0; c < GLOBALS.Tiles.Length; c++)
                        {
                            for (var i = 0; i < GLOBALS.Tiles[c].Length; i++)
                            {
                                if (GLOBALS.Tiles[c][i].Name == name)
                                {
                                    res.TileMatrix![y, x, z].Data.CategoryPostition = (c, i, name);

                                    try
                                    {
                                        _ = GLOBALS.Textures.Tiles[c][i];
                                    }
                                    catch
                                    {
                                        _logger.Warning($"missing tile texture detected: matrix index: ({x}, {y}, {z}); category {category}, position: {position}, name: \"{name}\"");
                                        return TileCheckResult.MissingTexture;
                                    }

                                    goto skip;
                                }
                            }
                        }

                        var data = (TileHead)cell.Data;
                        
                        data.CategoryPostition = (-1, -1, name);

                        res.TileMatrix![y, x, z] = cell with { Data = data };
                        
                        // Tile not found
                        return TileCheckResult.Missing;
                    }
                    else if (cell.Type == TileType.Material)
                    {
                        var materialName = ((TileMaterial)cell.Data).Name;

                        if (!GLOBALS.MaterialColors.ContainsKey(materialName))
                        {
                            _logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
                            return TileCheckResult.MissingMaterial;
                        }
                    }

                    skip:
                    { }
                }
            }
        }

        _logger.Debug("tile check passed");

        return TileCheckResult.Ok;
    }

    private PropCheckResult CheckPropIntegrity(in LoadFileResult res)
    {
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
                    InitPropType.Tile => GLOBALS.Textures.Tiles[prop.position.category][prop.position.index],
                    _ => GLOBALS.Textures.Props[prop.position.category][prop.position.index]
                };

                // No IndexOutOfRangeException exception was thrown - Success
            }
            catch
            {
                var path = prop.type == InitPropType.Tile
                    ? Path.Combine(GLOBALS.Paths.TilesAssetsDirectory, prop.prop.Name+".png")
                    : Path.Combine(GLOBALS.Paths.PropsAssetsDirectory, prop.prop.Name + ".png");
                
                _logger.Error($"prop texture \"{path}\"");
                return PropCheckResult.MissingTexture;
            }
        }
        
        return PropCheckResult.Ok;
    }
    
    private async Task<LoadFileResult> LoadProjectAsync(string filePath)
    {
        try
        {
            var text = (await File.ReadAllTextAsync(filePath)).ReplaceLineEndings().Split(Environment.NewLine);

            var lightMapFileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".png");

            if (!File.Exists(lightMapFileName)) return new();

            var lightMap = Raylib.LoadImage(lightMapFileName);

            if (text.Length < 7) return new LoadFileResult();

            var objTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[0]));
            var tilesObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[1]));
            var terrainObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[4]));
            var obj2Task = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[5]));
            var effObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[2]));
            var lightObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[3]));
            var camsObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[6]));
            var waterObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[7]));
            var propsObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[8]));

            var obj = await objTask;
            var tilesObj = await tilesObjTask;
            var terrainModeObj = await terrainObjTask;
            var obj2 = await obj2Task;
            var effObj = await effObjTask;
            var lightObj = await lightObjTask;
            var camsObj = await camsObjTask;
            var waterObj = await waterObjTask;
            var propsObj = await propsObjTask;

            var mtx = Lingo.Importers.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
            var tlMtx = Lingo.Importers.GetTileMatrix(tilesObj, out _, out _);
            var defaultMaterial = Lingo.Importers.GetDefaultMaterial(tilesObj);
            var buffers = Lingo.Importers.GetBufferTiles(obj2);
            var terrain = Lingo.Importers.GetTerrainMedium(terrainModeObj);
            var lightMode = Lingo.Importers.GetLightMode(obj2);
            var seed = Lingo.Importers.GetSeed(obj2);
            var waterData = Lingo.Importers.GetWaterData(waterObj);
            var effects = Lingo.Importers.GetEffects(effObj, givenWidth, givenHeight);
            var cams = Lingo.Importers.GetCameras(camsObj);
            
            // TODO: catch PropNotFoundException
            var props = Lingo.Importers.GetProps(propsObj);
            var lightSettings = Lingo.Importers.GetLightSettings(lightObj);

            // map material colors

            var materialColors = Utils.NewMaterialColorMatrix(givenWidth, givenHeight, new(0, 0, 0, 255));

            for (int y = 0; y < givenHeight; y++)
            {
                for (int x = 0; x < givenWidth; x++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        var cell = tlMtx[y, x, z];

                        if (cell.Type != TileType.Material) continue;

                        var materialName = ((TileMaterial)cell.Data).Name;

                        if (GLOBALS.MaterialColors.TryGetValue(materialName, out Color color)) materialColors[y, x, z] = color;
                    }
                }
            }

            //

            return new LoadFileResult
            {
                Success = true,
                Seed = seed,
                WaterLevel = waterData.waterLevel,
                WaterInFront = waterData.waterInFront,
                Width = givenWidth,
                Height = givenHeight,
                DefaultMaterial = defaultMaterial,
                BufferTiles = buffers,
                GeoMatrix = mtx,
                TileMatrix = tlMtx,
                LightMode = lightMode,
                DefaultTerrain = terrain,
                MaterialColorMatrix = materialColors,
                Effects = effects,
                LightMapImage = lightMap,
                Cameras = cams,
                PropsArray = props.ToArray(),
                LightSettings = lightSettings,
                Name = Path.GetFileNameWithoutExtension(filePath)
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new();
        }
    }

    public void Draw()
    {
        GLOBALS.PreviousPage = 1;

        var width = GetScreenWidth();
        var height = GetScreenHeight();
        
        var ctrl = IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL);
        var shift = IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT);
        var alt = IsKeyDown(KeyboardKey.KEY_LEFT_ALT);

        if (!RayGui.GuiIsLocked() && _spinnerLock == 0)
        {
            
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToGeometryEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 2;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToTileEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 3;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToCameraEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 4;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToLightEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 5;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToDimensionsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.NewFlag = false;
                GLOBALS.Page = 6;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToEffectsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 7;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToPropsEditor.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 8;
            }
            if (GLOBALS.Settings.Shortcuts.GlobalShortcuts.ToSettingsPage.Check(ctrl, shift, alt))
            {
                GLOBALS.Page = 9;
            }

            // handle zoom
            var mainPageWheel = GetMouseWheelMove();
            if (mainPageWheel != 0)
            {
                var mouseWorldPosition = Raylib.GetScreenToWorld2D(Raylib.GetMousePosition(), _camera);
                _camera.offset = Raylib.GetMousePosition();
                _camera.target = mouseWorldPosition;
                _camera.zoom += mainPageWheel * GLOBALS.ZoomIncrement;
                if (_camera.zoom < GLOBALS.ZoomIncrement) _camera.zoom = GLOBALS.ZoomIncrement;
            }

            // handle mouse drag
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_MIDDLE))
            {
                var delta = GetMouseDelta();
                delta = RayMath.Vector2Scale(delta, -1.0f / _camera.zoom);
                _camera.target = RayMath.Vector2Add(_camera.target, delta);
            }
        }
        

        BeginDrawing();
        {
            if (RayGui.GuiIsLocked())
            {
                ClearBackground(BLACK);

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
                                    RayGui.GuiUnlock();
                                }
                            }
                            else
                            {
                                if (!_saveFileDialog.IsCompleted)
                                {
                                    EndDrawing();
                                    return;
                                }
                                if (string.IsNullOrEmpty(_saveFileDialog.Result))
                                {
                                    RayGui.GuiUnlock();
                                    EndDrawing();
                                    return;
                                }

                                var path = _saveFileDialog.Result;

                                if (_saveResult is null)
                                {
                                    _saveResult = SaveProjectAsync(path);
                                    EndDrawing();
                                    return;
                                }
                                if (!_saveResult.IsCompleted)
                                {
                                    EndDrawing();
                                    return;
                                }

                                var result = _saveResult.Result;

                                if (!result.Success)
                                {
                                    _failedToSave = true;
                                    RayGui.GuiUnlock();
                                    EndDrawing();
                                    #if DEBUG
                                    if (result.Exception is not null) _logger.Error($"Failed to save project: {result.Exception}");
                                    #endif
                                    _saveResult = null;
                                    _saveFileDialog = null;
                                    return;
                                }
                                
                                // export light map
                                {
                                    var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);

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
                                    RayGui.GuiUnlock();
                                    EndDrawing();
                                }
                            }
                        }
                        else
                        {
                            var path = Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName + ".txt");
                            
                            if (_saveResult is null)
                            {
                                _saveResult = SaveProjectAsync(path);
                                EndDrawing();
                                return;
                            }
                            if (!_saveResult.IsCompleted)
                            {
                                EndDrawing();
                                return;
                            }

                            var result = _saveResult.Result;

                            if (!result.Success)
                            {
                                _failedToSave = true;
                                RayGui.GuiUnlock();
                                EndDrawing();
                                #if DEBUG
                                if (result.Exception is not null) _logger.Error($"Failed to save project: {result.Exception}");
                                #endif
                                _saveResult = null;
                                _saveFileDialog = null;
                                return;
                            }
                            
                            // export light map
                            {
                                var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);

                                unsafe
                                {
                                    ImageFlipVertical(&image);
                                }

                                var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
                                var name = Path.GetFileNameWithoutExtension(path);

                                ExportImage(image, Path.Combine(parent, name + ".png"));

                                UnloadImage(image);
                            }
                            
                            _saveFileDialog = null;
                            _saveResult = null;
                            RayGui.GuiUnlock();
                            EndDrawing();
                        }
                    }
                        break;

                    case 1:
                    {
                        if (_openFileDialog!.IsCompleted != true)
                        {
                            DrawText("Please wait..", GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                            EndDrawing();
                            return;
                        }
                        if (string.IsNullOrEmpty(_openFileDialog.Result))
                        {
                            _openFileDialog = null;
                            RayGui.GuiUnlock();
                            EndDrawing();
                            return;
                        }
                        if (_loadFileTask is null)
                        {
                            _loadFileTask = LoadProjectAsync(_openFileDialog.Result);
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
                            RayGui.GuiUnlock();
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
                        
                        if (!GLOBALS.TileCheck.IsCompleted || !GLOBALS.PropCheck.IsCompleted)
                        {
                            DrawText("Validating..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                            EndDrawing();
                            return;
                        }
                        
                        // Tile check failure
                        if (GLOBALS.TileCheck.Result != TileCheckResult.Ok)
                        {
                            if (GLOBALS.TileCheck.Result == TileCheckResult.Missing && GLOBALS.Settings.TileEditor.AllowUndefinedTiles)
                            {
                            }
                            else
                            {
                                GLOBALS.Page = 13;
                                RayGui.GuiUnlock();
                                
                                EndDrawing();
                                return;
                            }
                        }

                        // Prop check failure
                        if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                        {
                            GLOBALS.Page = 19;
                            RayGui.GuiUnlock();
                            
                            EndDrawing();
                            return;
                        }
                        
                        _logger.Debug("Globals.Level.Import()");
                        
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
                        _logger.Debug($"Adjusting {nameof(GLOBALS.CamQuadLocks)}");
                        #endif

                        GLOBALS.CamQuadLocks = new int[result.Cameras.Count];

                        #if DEBUG
                        _logger.Debug($"Importing lightmap texture");
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
                            new(0, 0, lightMapTexture.width, lightMapTexture.height),
                            new(0, 0),
                            new(255, 255, 255, 255)
                        );
                        
                        EndTextureMode();

                        UnloadImage(result.LightMapImage);

                        UnloadTexture(lightMapTexture);
                        
                        #if DEBUG
                        _logger.Debug($"Updating project name");
                        #endif

                        GLOBALS.Level.ProjectName = result.Name;
                        GLOBALS.Page = 1;

                        var undefinedTiles = GLOBALS.TileCheck.Result == TileCheckResult.Missing;
                        _undefinedTilesAlert = undefinedTiles;

                        ProjectLoaded?.Invoke(this, new LevelLoadedEventArgs(undefinedTiles));
                        
                        GLOBALS.TileCheck = null;
                        GLOBALS.PropCheck = null;
                        _loadFileTask = null;
                        
                        #if DEBUG
                        _logger.Debug($"Invoking {nameof(ProjectLoaded)} event");
                        #endif
                        
                        var parent = Directory.GetParent(_openFileDialog.Result)?.FullName;
                            
                        GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                        GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_openFileDialog.Result);
                        
                        RayGui.GuiUnlock();
                    }
                        break;
                    
                    default:
                        RayGui.GuiUnlock();
                        break;
                }
                
                DrawText(
                    "Please wait..", 
                    (width - MeasureText("Please wait..", 50))/2f, 
                    100, 
                    50, 
                    WHITE
                );

                
            }
            else
            {
                ClearBackground(GLOBALS.Settings.GeneralSettings.DarkTheme 
                    ? BLACK 
                    : new(170, 170, 170, 255));

                BeginMode2D(_camera);
                {
                    DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale,
                        GLOBALS.Settings.GeneralSettings.DarkTheme
                            ? new Color(50, 50, 50, 255)
                            : WHITE);

                    Printers.DrawGeoLayer(2, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(150, 150, 150, 255) : BLACK with { a = 150 });
                    Printers.DrawGeoLayer(1, GLOBALS.Scale, false, GLOBALS.Settings.GeneralSettings.DarkTheme ? new Color(100, 100, 100, 255) : BLACK with { a = 150 });
                    
                    if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                    {
                        DrawRectangle(
                            (-1) * GLOBALS.Scale,
                            (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                            (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                            GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                            GLOBALS.Settings.GeneralSettings.DarkTheme 
                                ? GLOBALS.DarkThemeWaterColor 
                                : GLOBALS.LightThemeWaterColor
                        );
                    }
                    
                    Printers.DrawGeoLayer(0, GLOBALS.Scale, false, BLACK);

                    if (GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                    {
                        DrawRectangle(
                            (-1) * GLOBALS.Scale,
                            (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                            (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                            GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                            GLOBALS.Settings.GeneralSettings.DarkTheme 
                                ? GLOBALS.DarkThemeWaterColor 
                                : GLOBALS.LightThemeWaterColor
                        );
                    }
                    
                    DrawRectangleLines(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, WHITE);
                }
                EndMode2D();

                if (_failedToSave)
                {
                    rlImGui.Begin();

                    if (ImGui.Begin("Error##ProjectSaveFail"))
                    {
                        
                        ImGui.Text("Failed to save your project");

                        var okSelected = ImGui.Button("Ok");

                        if (okSelected) _failedToSave = false;
                        
                        ImGui.End();
                    }
                    
                    rlImGui.End();
                }
                
                // Menu
                
                rlImGui.Begin();

                if (ImGui.Begin("Options##MainMenu"))
                {
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

                    // Water Level
                    
                    var waterLevel = GLOBALS.Level.WaterLevel;
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputInt("Water Level", ref waterLevel);
                    if (waterLevel < -1) waterLevel = -1;
                    if (GLOBALS.Level.WaterLevel != waterLevel) GLOBALS.Level.WaterLevel = waterLevel;
                    
                    // Water Position
                    
                    var waterInFront = GLOBALS.Level.WaterAtFront;
                    ImGui.Checkbox("Water In Front", ref waterInFront);
                    if (GLOBALS.Level.WaterAtFront != waterInFront) GLOBALS.Level.WaterAtFront = waterInFront;
                    
                    ImGui.Separator();
                    
                    // Buttons

                    var saveSelected = ImGui.Button("Save");
                    var saveAsSelected = ImGui.Button("Save as..");
                    var loadSelected = ImGui.Button("Load Project..");
                    var newSelected = ImGui.Button("New Project");

                    if (GLOBALS.RendererExists)
                    {
                        var renderSelected = ImGui.Button("RENDER");
                        
                        if (renderSelected)
                        {
                            _logger.Debug($"Rendering level \"{GLOBALS.Level.ProjectName}\"");

                            var projectPath = Path.Combine(GLOBALS.ProjectPath, $"{GLOBALS.Level.ProjectName}.txt");
                            var arguments = $"render \"{projectPath}\"";

                            try
                            {
                                _renderProcess.Kill();
                                    
                                _renderProcess.Dispose();
                            } catch (Exception e) {
                                _logger.Error($"Unable to kill render process: {e}");  
                            }
                                
                            _renderProcess = new Process
                            {
                                StartInfo =
                                {
                                    FileName = Path.Combine(GLOBALS.Paths.RendererDirectory, "Drizzle.ConsoleApp.exe"),
                                    WorkingDirectory = GLOBALS.Paths.ExecutableDirectory,
                                    Arguments = arguments
                                }
                            };

                            _renderProcess.Start();
                        }
                    }
                    
                    if (saveSelected)
                    {
                        _fileDialogMode = 0;
                        if (string.IsNullOrEmpty(GLOBALS.ProjectPath))
                        {
                            _askForPath = true;
                            _saveFileDialog = Utils.SetFilePathAsync();
                        }
                        else
                        {
                            _askForPath = false;
                        }
                        RayGui.GuiLock();
                    }

                    if (saveAsSelected)
                    {
                        _fileDialogMode = 0;
                        _askForPath = true;
                        _saveFileDialog = Utils.SetFilePathAsync();
                        RayGui.GuiLock();
                    }
                    
                    if (loadSelected)
                    {
                        _fileDialogMode = 1;
                        _openFileDialog = Utils.GetFilePathAsync();
                        RayGui.GuiLock();
                    }
                    
                    if (newSelected)
                    {
                        GLOBALS.NewFlag = true;
                        GLOBALS.Page = 6;
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
                        ImGui.Text("This  project contains undefined tiles.");
                        
                        var okSelected = ImGui.Button("Ok");
                        
                        if (okSelected) _undefinedTilesAlert = false;
                        
                        ImGui.End();
                    }
                }
                
                // Shortcuts window
                if (GLOBALS.Settings.GeneralSettings.ShortcutWindow)
                {
                    var shortcutWindowRect = Printers.ImGui.ShortcutsWindow(GLOBALS.Settings.Shortcuts.GlobalShortcuts);

                    _isShortcutsWinHovered = CheckCollisionPointRec(
                        GetMousePosition(), 
                        shortcutWindowRect with
                        {
                            X = shortcutWindowRect.X - 5, width = shortcutWindowRect.width + 10
                        }
                    );

                    if (_isShortcutsWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _isShortcutsWinDragged = true;
                    }
                    else if (_isShortcutsWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                    {
                        _isShortcutsWinDragged = false;
                    }
                }

                var navWindowRect = Printers.ImGui.NavigationWindow();

                _isNavigationWinHovered = CheckCollisionPointRec(GetMousePosition(), navWindowRect with
                {
                    X = navWindowRect.X - 5, width = navWindowRect.width + 10
                });
                
                if (_isNavigationWinHovered && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isNavigationWinDragged = true;
                }
                else if (_isNavigationWinDragged && IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT))
                {
                    _isNavigationWinDragged = false;
                }
                
                rlImGui.End();
                
            }
            
        }
        EndDrawing();
    }
}
