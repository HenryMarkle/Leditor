using static Raylib_CsLo.Raylib;
using Pidgin;
namespace Leditor;

#nullable enable

public class LoadProjectPage : IPage
{
    private readonly Serilog.Core.Logger _logger;
    
    private int _explorerPage;
    
    private Task<LoadFileResult> _loadFileTask = Task.FromResult(new LoadFileResult());
    
    private readonly byte[] _explorerPanelBytes = "Load Project"u8.ToArray();

    private readonly string[] _projectFiles;

    public LoadProjectPage(Serilog.Core.Logger logger)
    {
        _logger = logger;
        
        try
        {
            _projectFiles = Directory.EnumerateFiles(GLOBALS.Paths.ProjectsDirectory).ToArray();
        }
        catch (Exception e)
        {
            _logger.Fatal($"failed to read project files: {e}");
            _projectFiles = [];
        }
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
                                    res.TileMatrix![y, x, z].Data.CategoryPostition = (c + 5, i + 1, name);

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
    
    private static LoadFileResult LoadProject(string filePath)
    {
        try
        {
            var text = File.ReadAllText(filePath).ReplaceLineEndings().Split(Environment.NewLine);

            var lightMapFileName = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".png");

            if (!File.Exists(lightMapFileName)) return new();

            var lightMap = Raylib.LoadImage(lightMapFileName);

            if (text.Length < 7) return new LoadFileResult();

            var obj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[0]);
            var tilesObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[1]);
            var obj2 = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[5]);
            var effObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[2]);
            var camsObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[6]);
            var propsObj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text[8]);

            var mtx = Lingo.Tools.GetGeoMatrix(obj, out int givenHeight, out int givenWidth);
            var tlMtx = Lingo.Tools.GetTileMatrix(tilesObj, out int tlHeight, out int tlWidth);
            var buffers = Lingo.Tools.GetBufferTiles(obj2);
            var effects = Lingo.Tools.GetEffects(effObj, givenWidth, givenHeight);
            var cams = Lingo.Tools.GetCameras(camsObj);
            var props = Lingo.Tools.GetProps(propsObj);

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
        if (IsKeyPressed(KeyboardKey.KEY_ZERO)) GLOBALS.Page = 0;

        const int buttonHeight = 40;
        var maxCount = (GetScreenHeight() - 400) / buttonHeight;
        var buttonOffsetX = 120;
        var buttonWidth = GetScreenWidth() - 240;

        if (IsKeyPressed(KeyboardKey.KEY_W) && _explorerPage > 0) _explorerPage--;
        if (IsKeyPressed(KeyboardKey.KEY_S) && _explorerPage < (_projectFiles.Length / maxCount)) _explorerPage++;


        BeginDrawing();
        {
            if (RayGui.GuiIsLocked()) // loading a project
            {
                ClearBackground(new Color(0, 0, 0, 130));


                if (!_loadFileTask.IsCompleted)
                {
                    DrawText("Please wait..", GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    EndDrawing();
                    return;
                }

                LoadFileResult res = _loadFileTask.Result;

                if (!res.Success)
                {
                    _logger.Debug("failed to load level project");
                    RayGui.GuiUnlock();
                    EndDrawing();
                    return;
                }

                // Validate if tiles are defined in Init.txt

                if (GLOBALS.TileCheck is null)
                {
                    GLOBALS.TileCheck = Task.Factory.StartNew(() => CheckTileIntegrity(res));

                    EndDrawing();
                    return;
                }
                
                // Validate if props have textures

                if (GLOBALS.PropCheck is null)
                {
                    GLOBALS.PropCheck = Task.Factory.StartNew(() => CheckPropIntegrity(res));
                    
                    EndDrawing();
                    return;
                }

                if (!GLOBALS.TileCheck.IsCompleted || !GLOBALS.PropCheck.IsCompleted)
                {
                    DrawText("Validating..", Raylib.GetScreenWidth() / 2 - 100, Raylib.GetScreenHeight() / 2 - 20, 30, new(255, 255, 255, 255));
                    EndDrawing();
                    return;
                }

                if (GLOBALS.TileCheck.Result != TileCheckResult.Ok)
                {
                    GLOBALS.Page = 13;
                    RayGui.GuiUnlock();
                    
                    EndDrawing();
                    return;
                }

                if (GLOBALS.PropCheck.Result != PropCheckResult.Ok)
                {
                    GLOBALS.Page = 19;
                    RayGui.GuiUnlock();
                    
                    EndDrawing();
                    return;
                }

                // TODO: reset camera targets
                
                GLOBALS.Level.Import(
                    res.Width, 
                    res.Height,
                    (res.BufferTiles.Left, res.BufferTiles.Top, res.BufferTiles.Right, res.BufferTiles.Bottom),
                    res.GeoMatrix!,
                    res.TileMatrix!,
                    res.MaterialColorMatrix!,
                    res.Effects.Select(effect => (effect.Item1, Effects.GetEffectOptions(effect.Item1), effect.Item2)).ToArray(),
                    res.Cameras,
                    res.PropsArray!
                );

                var lightMapTexture = LoadTextureFromImage(res.LightMapImage);

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

                UnloadImage(res.LightMapImage);

                UnloadTexture(lightMapTexture);

                GLOBALS.ProjectName = res.Name;
                GLOBALS.Page = 1;

                GLOBALS.TileCheck = null;
                RayGui.GuiUnlock();
            }
            else // choosing a project
            {

                ClearBackground(new(170, 170, 170, 255));

                Rectangle panelRect = new(
                    100, 
                    100, 
                    GetScreenWidth() - 200, 
                    GetScreenHeight() - 200
                );

                unsafe
                {
                    fixed (byte* pt = _explorerPanelBytes)
                    {
                        RayGui.GuiPanel(panelRect, (sbyte*)pt);
                    }
                }

                //no projects
                if (_projectFiles.Length == 0)
                {
                    Raylib.DrawText(
                        "You have no projects yet",
                        GetScreenWidth() / 2 - 200,
                        GetScreenHeight() / 2 - 50,
                        30,
                        new(0, 0, 0, 255)
                    );

                    if (RayGui.GuiButton(new(GetScreenWidth() / 2 - 100, GetScreenHeight() / 2 + 50, 200, 50), "Create New Project"))
                    {
                        GLOBALS.NewFlag = true;
                        GLOBALS.Page = 6;
                    }
                }
                // there are projects
                else
                {
                    DrawText("[W] - Page Up  [S] - Page Down", GetScreenWidth() / 2 - 220, 150, 30, new(0, 0, 0, 255));

                    if (maxCount > _projectFiles.Length)
                    {
                        for (int f = 0; f < _projectFiles.Length; f++)
                        {
                            if (!_projectFiles[f].EndsWith(".txt")) continue;

                            RayGui.GuiButton(
                                new Rectangle(buttonOffsetX, f * buttonHeight + 210, buttonWidth, buttonHeight - 1),
                                Path.GetFileNameWithoutExtension(_projectFiles[f])
                            );
                        }
                    }
                    else
                    {
                        var currentPage = _projectFiles.Where(f => f.EndsWith(".txt")).Skip(maxCount * _explorerPage).Take(maxCount);
                        var counter = 0;

                        foreach (var f in currentPage)
                        {
                            var isPressed = RayGui.GuiButton(
                                new Rectangle(buttonOffsetX, counter * buttonHeight + 210, buttonWidth, buttonHeight - 1),
                                Path.GetFileNameWithoutExtension(f)
                            );

                            if (isPressed)
                            {
                                RayGui.GuiLock();

                                // LOAD PROJECT FILE
                                _loadFileTask = Task.Factory.StartNew(() => LoadProject(f));
                            }

                            counter++;
                        }
                    }

                    DrawText(
                        $"Page {_explorerPage}/{maxCount}",
                        GetScreenWidth() / 2 - 90,
                        GetScreenHeight() - 160,
                        30,
                        new(0, 0, 0, 255)
                    );
                }

            }
        }
        Raylib.EndDrawing();
     }
}

