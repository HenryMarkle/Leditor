using System.Numerics;
using Leditor.Pages;
using Pidgin;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

internal class StartPage : EditorPage
{
    internal event EventHandler? ProjectLoaded;

    private bool _uiLocked;

    private Task<string>? _openFileDialog;
    private Task<LoadFileResult>? _loadFileTask;
    
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
                                        Logger.Warning($"missing tile texture detected: matrix index: ({x}, {y}, {z}); category {category}, position: {position}, name: \"{name}\"");
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
                            Logger.Warning($"missing material: matrix index: ({x}, {y}, {z}); Name: \"{materialName}\"");
                            return TileCheckResult.MissingMaterial;
                        }
                    }

                    skip:
                    { }
                }
            }
        }

        Logger.Debug("tile check passed");

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
                
                Logger.Error($"prop texture \"{path}\"");
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

            var lightMap = LoadImage(lightMapFileName);

            if (text.Length < 7) return new LoadFileResult();

            var objTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[0]));
            var tilesObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[1]));
            var terrainObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[4]));
            var obj2Task = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[5]));
            var effObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[2]));
            var lightObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[3]));
            var camsObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[6]));
            var waterObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[7]));
            var propsObjTask = Task.Factory.StartNew(() => Drizzle.Lingo.Runtime.Parser.LingoParser.Expression.ParseOrThrow(text[8]));

            var obj = await objTask;
            var tilesObj = await tilesObjTask;
            var terrainModeObj = await terrainObjTask;
            var obj2 = await obj2Task;
            var effObj = await effObjTask;
            var lightObj = await lightObjTask;
            var camsObj = await camsObjTask;
            var waterObj = await waterObjTask;
            var propsObj = await propsObjTask;

            var mtx = Serialization.Importers.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
            var tlMtx = Serialization.Importers.GetTileMatrix(tilesObj, out _, out _);
            var defaultMaterial = Serialization.Importers.GetDefaultMaterial(tilesObj);
            var buffers = Serialization.Importers.GetBufferTiles(obj2);
            var terrain = Serialization.Importers.GetTerrainMedium(terrainModeObj);
            var lightMode = Serialization.Importers.GetLightMode(obj2);
            var seed = Serialization.Importers.GetSeed(obj2);
            var waterData = Serialization.Importers.GetWaterData(waterObj);
            var effects = Serialization.Importers.GetEffects(effObj, givenWidth, givenHeight);
            var cams = Serialization.Importers.GetCameras(camsObj);
            
            // TODO: catch PropNotFoundException
            var props = Serialization.Importers.GetProps(propsObj);
            var lightSettings = Serialization.Importers.GetLightSettings(lightObj);

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
    
    public override void Draw()
    {
        GLOBALS.PreviousPage = 0;

        BeginDrawing();
        {
            if (_uiLocked)
            {
                ClearBackground(Color.Black);
                
                if (_openFileDialog!.IsCompleted != true)
                {
                    DrawText("Please wait..", GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));

                    EndDrawing();
                    return;
                }
                if (string.IsNullOrEmpty(_openFileDialog.Result))
                {
                    _openFileDialog = null;
                    _uiLocked = false;
                    EndDrawing();
                    return;
                }
                if (_loadFileTask is null)
                {
                    _loadFileTask = Utils.LoadProjectAsync(_openFileDialog.Result);
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
                        _uiLocked = false;
                        
                        EndDrawing();
                        return;
                    }
                }

                // Prop check failure
                if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                {
                    GLOBALS.Page = 19;
                    _uiLocked = false;
                    
                    EndDrawing();
                    return;
                }
                
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
                
                ProjectLoaded?.Invoke(this, new LevelLoadedEventArgs(GLOBALS.TileCheck.Result == TileCheckResult.Missing));

                GLOBALS.TileCheck = null;
                GLOBALS.PropCheck = null;
                _loadFileTask = null;
                
                #if DEBUG
                Logger.Debug($"Invoking {nameof(ProjectLoaded)} event");
                #endif
                
                var parent = Directory.GetParent(_openFileDialog.Result)?.FullName;
                    
                GLOBALS.ProjectPath = parent ?? GLOBALS.ProjectPath;
                GLOBALS.Level.ProjectName = Path.GetFileNameWithoutExtension(_openFileDialog.Result);

                _uiLocked = false;
            }
            else
            {
                ClearBackground(new Color(170, 170, 170, 255));
                
                // Create

                var createRect = new Rectangle(GetScreenWidth() / 2f - 200, GetScreenHeight() / 2f - 42, 400, 40);
                var createHovered = CheckCollisionPointRec(GetMousePosition(), createRect);
                
                DrawRectangleLinesEx(createRect, 1, Color.Black);

                if (GLOBALS.Font is null)
                    DrawText("Create", (int)(createRect.X + 5), (int)(createRect.Y + 10), 20, Color.Black);
                else 
                    DrawTextEx(GLOBALS.Font.Value, "Create", new Vector2(createRect.X + (createRect.Width - MeasureText("Create", 20))/2, createRect.Y + 10), 20, 1, Color.Black);
                
                if (createHovered)
                {
                    DrawRectangleRec(createRect, Color.Blue with { A = 100 });
                    
                    if (IsMouseButtonPressed(MouseButton.Left))
                    {
                        GLOBALS.NewFlag = true;
                        GLOBALS.Page = 6;
                    }
                }
                
                // Load

                var loadRect = new Rectangle(GetScreenWidth() / 2f - 200, GetScreenHeight() / 2f, 400, 40);
                var loadHovered = CheckCollisionPointRec(GetMousePosition(), loadRect);
                
                DrawRectangleLinesEx(loadRect, 1, Color.Black);

                if (GLOBALS.Font is null)
                    DrawText("Load", (int)(loadRect.X + 5), (int)(loadRect.Y + 10), 20, Color.Black);
                else 
                    DrawTextEx(GLOBALS.Font.Value, "Load", new Vector2(loadRect.X + (loadRect.Width - MeasureText("Load", 20))/2, loadRect.Y + 10), 20, 1, Color.Black);
                
                if (loadHovered)
                {
                    DrawRectangleRec(loadRect, Color.Blue with { A = 100 });
                    
                    if (IsMouseButtonPressed(MouseButton.Left))
                    {
                        _openFileDialog = Utils.GetFilePathAsync();
                        _uiLocked = true;
                    }
                }
            }
        }
        EndDrawing();
    }
}