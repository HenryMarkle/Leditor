using System.Numerics;
using System.Text;
using Pidgin;
using static Raylib_CsLo.Raylib;

namespace Leditor;

#nullable enable

internal class MainPage(Serilog.Core.Logger logger, Camera2D? camera = null) : IPage
{
    readonly Serilog.Core.Logger _logger = logger;
    
    internal event EventHandler? ProjectLoaded;

    Camera2D _camera = camera ?? new() { zoom = 0.5f };

    readonly byte[] previewPanelBytes = Encoding.ASCII.GetBytes("Level Options");
    
    private record struct SaveProjectResult(bool Success, Exception? Exception = null);

    private Task<string>? _saveFileDialog;
    private Task<SaveProjectResult>? _saveResult;

    private bool _askForPath;
    private bool _failedToSave;
    
    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
    private int _fileDialogMode; // 0 - save, 1 - load
    
    private async Task<SaveProjectResult> SaveProjectAsync(string path)
    {
        SaveProjectResult result;
        
        try
        {
            var strTask = Leditor.Lingo.Exporters.ExportAsync(GLOBALS.Level);

            // export light map
            var image = LoadImageFromTexture(GLOBALS.Textures.LightMap.texture);

            unsafe
            {
                ImageFlipVertical(&image);
            }
            
            var parent = Directory.GetParent(path)?.FullName ?? GLOBALS.ProjectPath;
            var name = Path.GetFileNameWithoutExtension(path);
                    
            ExportImage(image, Path.Combine(parent, name+".png"));
            
            UnloadImage(image);

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

                        // code readibility could be optimized using System.Linq

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
            var obj2Task = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[5]));
            var effObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[2]));
            var lightObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[3]));
            var camsObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[6]));
            var propsObjTask = Task.Factory.StartNew(() => Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[8]));

            await Task.WhenAll([objTask, tilesObjTask, obj2Task, effObjTask, lightObjTask, camsObjTask, propsObjTask]);
            
            var obj = await objTask;
            var tilesObj = await tilesObjTask;
            var obj2 = await obj2Task;
            var effObj = await effObjTask;
            var lightObj = await lightObjTask;
            var camsObj = await camsObjTask;
            var propsObj = await propsObjTask;

            var mtx = Lingo.Tools.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
            var tlMtx = Lingo.Tools.GetTileMatrix(tilesObj, out _, out _);
            var buffers = Lingo.Tools.GetBufferTiles(obj2);
            var effects = Lingo.Tools.GetEffects(effObj, givenWidth, givenHeight);
            var cams = Lingo.Tools.GetCameras(camsObj);
            
            // TODO: catch PropNotFoundException
            var props = Lingo.Tools.GetProps(propsObj);
            var lightSettings = Lingo.Tools.GetLightSettings(lightObj);

            // map material colors

            Color[,,] materialColors = Utils.NewMaterialColorMatrix(givenWidth, givenHeight, new(0, 0, 0, 255));

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

            return new()
            {
                Success = true,
                Width = givenWidth,
                Height = givenHeight,
                BufferTiles = buffers,
                GeoMatrix = mtx,
                TileMatrix = tlMtx,
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

        if (!RayGui.GuiIsLocked())
        {
            
            if (IsKeyReleased(KeyboardKey.KEY_TWO))
            {
                GLOBALS.Page = 2;
            }
            if (IsKeyReleased(KeyboardKey.KEY_THREE))
            {
                GLOBALS.Page = 3;
            }
            if (IsKeyReleased(KeyboardKey.KEY_FOUR))
            {
                GLOBALS.Page = 4;
            }
            if (IsKeyReleased(KeyboardKey.KEY_FIVE))
            {
                GLOBALS.Page = 5;
            }
            if (IsKeyReleased(KeyboardKey.KEY_SIX))
            {
                GLOBALS.ResizeFlag = true;
                GLOBALS.Page = 6;
            }
            if (IsKeyReleased(KeyboardKey.KEY_SEVEN))
            {
                GLOBALS.Page = 7;
            }
            if (IsKeyReleased(KeyboardKey.KEY_EIGHT))
            {
                GLOBALS.Page = 8;
            }
            if (IsKeyReleased(KeyboardKey.KEY_NINE))
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
                                
                                var parent = Directory.GetParent(_saveFileDialog.Result)?.FullName;
                                
                                GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                                GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_saveFileDialog.Result);

                                _saveFileDialog = null;
                                _saveResult = null;
                                RayGui.GuiUnlock();
                                EndDrawing();
                            }
                        }
                        else
                        {
                            if (_saveResult is null)
                            {
                                _saveResult = SaveProjectAsync(Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt"));
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
                            GLOBALS.Page = 13;
                            RayGui.GuiUnlock();
                            
                            EndDrawing();
                            return;
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
                            projectName: result.Name
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

                        GLOBALS.TileCheck = null;
                        GLOBALS.PropCheck = null;
                        _loadFileTask = null;
                        
                        #if DEBUG
                        _logger.Debug($"Invoking {nameof(ProjectLoaded)} event");
                        #endif
                        
                        var parent = Directory.GetParent(_openFileDialog.Result)?.FullName;
                            
                        GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                        GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_openFileDialog.Result);
                        
                        ProjectLoaded?.Invoke(this, EventArgs.Empty);
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
                ClearBackground(new(170, 170, 170, 255));

                BeginMode2D(_camera);
                {
                    DrawRectangle(0, 0, GLOBALS.Level.Width * GLOBALS.Scale, GLOBALS.Level.Height * GLOBALS.Scale, new(255, 255, 255, 255));

                    Printers.DrawGeoLayer(2, GLOBALS.Scale, false, BLACK with { a = 150 });
                    Printers.DrawGeoLayer(1, GLOBALS.Scale, false, BLACK with { a = 150 });
                    
                    if (!GLOBALS.Level.WaterAtFront && GLOBALS.Level.WaterLevel != -1)
                    {
                        DrawRectangle(
                            (-1) * GLOBALS.Scale,
                            (GLOBALS.Level.Height - GLOBALS.Level.WaterLevel) * GLOBALS.Scale,
                            (GLOBALS.Level.Width + 2) * GLOBALS.Scale,
                            GLOBALS.Level.WaterLevel * GLOBALS.Scale,
                            new(0, 0, 255, 110)
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
                            new(0, 0, 255, 110)
                        );
                    }
                }
                EndMode2D();

                unsafe
                {
                    fixed (byte* pt = previewPanelBytes)
                    {
                        RayGui.GuiPanel(
                            new(
                                GetScreenWidth() - 400,
                                50,
                                380,
                                GetScreenHeight() - 100
                            ),
                            (sbyte*)pt
                        );

                    }
                }

                DrawText(
                    GLOBALS.Level.ProjectName,
                    GetScreenWidth() - 350,
                    100,
                    30,
                    new(0, 0, 0, 255)
                );

                var helpPressed = RayGui.GuiButton(new(
                    GetScreenWidth() - 80,
                    100,
                    40,
                    40
                ),
                "?"
                );

                if (helpPressed) GLOBALS.Page = 9;

                Raylib.DrawText("Seed", Raylib.GetScreenWidth() - 380, 205, 11, new(0, 0, 0, 255));

                GLOBALS.Level.Seed = (int)Math.Round(Raylib_CsLo.RayGui.GuiSlider(
                    new(
                        Raylib.GetScreenWidth() - 290,
                        200,
                        200,
                        20
                    ),
                    "0",
                    "400",
                    GLOBALS.Level.Seed,
                    0,
                    400
                ));

                GLOBALS.Level.LightMode = RayGui.GuiCheckBox(new(
                    Raylib.GetScreenWidth() - 380,
                    250,
                    20,
                    20
                ),
                "Light Mode",
                GLOBALS.Level.LightMode);

                GLOBALS.Level.DefaultTerrain = RayGui.GuiCheckBox(new(
                    GetScreenWidth() - 380,
                    290,
                    20,
                    20
                ),
                "Default Medium",
                GLOBALS.Level.DefaultTerrain);

                Raylib.DrawText("Water Level",
                    Raylib.GetScreenWidth() - 380,
                    335,
                    11,
                    new(0, 0, 0, 255)
                );

                GLOBALS.Level.WaterLevel = (int)Math.Round(RayGui.GuiSlider(
                    new(
                        width - 290,
                        330,
                        200,
                        20
                    ),
                    "-1",
                    $"{GLOBALS.Level.Height + 10}",
                    GLOBALS.Level.WaterLevel,
                    -1,
                    GLOBALS.Level.Height + 10
                ));


                GLOBALS.Level.WaterAtFront = RayGui.GuiCheckBox(
                    new(
                        Raylib.GetScreenWidth() - 380,
                        360,
                        20,
                        20
                    ),
                    "Water In Front",
                    GLOBALS.Level.WaterAtFront
                );

                //

                var savePressed = RayGui.GuiButton(new(
                        GetScreenWidth() - 390,
                        GetScreenHeight() - 250,
                        360,
                        40
                    ),
                    "Save Project"
                );
                
                var saveAsPressed = RayGui.GuiButton(new(
                        GetScreenWidth() - 390,
                        GetScreenHeight() - 200,
                        360,
                        40
                    ),
                    "Save Project As.."
                );

                if (savePressed)
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

                if (saveAsPressed)
                {
                    _fileDialogMode = 0;
                    _askForPath = true;
                    _saveFileDialog = Utils.SetFilePathAsync();
                    RayGui.GuiLock();
                }

                var loadPressed = RayGui.GuiButton(new(
                        width - 390,
                        height - 150,
                        360,
                        40
                    ),
                    "Load Project"
                );

                var newPressed = RayGui.GuiButton(new(
                        width - 390,
                        height - 100,
                        360,
                        40
                    ),
                    "New Project"
                );

                if (loadPressed)
                {
                    _fileDialogMode = 1;
                    _openFileDialog = Utils.GetFilePathAsync();
                    RayGui.GuiLock();
                }
                if (newPressed)
                {
                    GLOBALS.NewFlag = true;
                    GLOBALS.Page = 6;
                }
            }
        }
        EndDrawing();
    }
}
