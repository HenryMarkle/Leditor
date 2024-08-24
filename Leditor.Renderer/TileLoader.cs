namespace Leditor.Renderer;

using Leditor.Serialization;
using Leditor.Data.Tiles;

using Raylib_cs;

public sealed class TileLoader
{
    private readonly record struct ImagePair(TileDefinition Tile, Image Image);
    private readonly record struct Pack(ImagePair[] Pairs, string Folder);

    /// <summary>
    /// Is set to true when _loadTask is completed.
    /// </summary>
    public bool IsReady { get; private set; }

    /// <summary>
    /// Indicates that the process is completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Is set by the constructor.
    /// </summary>
    private Task<List<Pack>>? _loadTask;

    private TileDefinition[] _resultTiles;

    private int _packCursor;
    private int _tileCursor;
    private int _resultCursor;

    private Serilog.ILogger? Logger { get; init; }

    public TileLoader(in Folders folders, Serilog.ILogger? logger)
    {
        logger?.Information("[TileLoader] Begin loading tiles");

        var tileFolders = folders.Tiles;

        var tileCount = 0;

        _loadTask = Task.Factory.StartNew(() => {
            List<(string folder, Task<TileDefinition[]> task)> imported = [];
            
            foreach (var folder in tileFolders)
            {
                if (!Directory.Exists(folder)) {
                    logger?.Error("[TileLoader] Failed to load init; folder not found: \"{}\"", folder);
                    continue;
                }

                if (!Program.FileExistsInFolder(folder, "init.txt"))
                {
                    logger?.Error("[TileLoader] init.txt was not found in folder \"{folder}\"", folder);
                    continue;
                }

                var initPath = Program.GetFilePathInFolder(folder, "init.txt");

                if (initPath is null) {
                    logger?.Error("[TileLoader] init.txt was not found in folder \"{folder}\"", folder);
                    continue;
                }

                var t = TileImporter.ParseInitAsync_NoCategories(initPath, logger);

                imported.Add((folder, t));
            }

            Task.WhenAll(imported.Select(i => i.task)).Wait();

            logger?.Information("[TileLoader] Load complete");


            var filtered = imported
                .Where(t => {
                    if (t.task.IsFaulted) {
                        logger?.Error(t.task.Exception, "[TileLoader] Failed to load and parse init.txt: {NewLine}{Exception}");
                        Console.WriteLine(t.task.Exception);
                    }

                    return t.task.IsCompletedSuccessfully;
                })
                .Select(pack => {
                    var imagePairTasks = pack.task.Result
                        .Where(tile => {
                            var exists = Program.FileExistsInFolder(pack.folder, $"{tile.Name}.png");

                            if (!exists)
                            {
                                Logger?.Error($"[TileLoader] Could not find image for tile \"{tile.Name}\"; path does not exists: \"{Path.Combine(pack.folder, $"{tile.Name}.png")}\"");
                            }

                            return exists;
                        })
                        .Select(tile => {
                            string imagePath = Program.GetFilePathInFolder(pack.folder, $"{tile.Name}.png")!;

                            return Task.Factory.StartNew(() => (tile, Raylib.LoadImage(imagePath)));
                        });

                    Task.WhenAll(imagePairTasks).Wait();

                    var imagePairs = imagePairTasks
                        .Where(task => {
                            if (task.IsFaulted)
                            {
                                Logger?.Error(task.Exception, "[TileLoader] Failed to load tile image: {Exception}");
                            }

                            return task.IsCompletedSuccessfully;
                        })
                        .Select(task => {
                            tileCount++;
                            return new ImagePair(task.Result.Item1, task.Result.Item2);
                        })
                        .ToArray();

                    return new Pack(imagePairs, pack.folder);
                })
                .ToList();

            IsReady = true;

            return filtered;        
        });

        _resultTiles = new TileDefinition[tileCount];

        Logger = logger;
    }

    public bool LoadNext()
    {
        if (!IsReady || _loadTask is null || !_loadTask.IsCompleted) return false;

        if (_packCursor >= _loadTask.Result.Count) {
            IsCompleted = true;
            return true;
        }

        var currentPack = _loadTask.Result[_packCursor];

        if (_tileCursor >= currentPack.Pairs.Length)
        {
            _tileCursor = 0;
            _packCursor++;
            return false;
        }

        var currentPair = currentPack.Pairs[_tileCursor];

        var texture = Raylib.LoadTextureFromImage(currentPair.Image);

        currentPair.Tile.Texture = texture;

        Raylib.UnloadImage(currentPair.Image);

        #if DEBUG
        if (_resultCursor >= _resultTiles.Length)
        {
            Logger?.Fatal("[TileLoader::LoadNext] Result cursor was out of bounds");
            throw new IndexOutOfRangeException($"{nameof(_resultCursor)} was out of bounds ({_resultCursor})");
        }
        #endif

        _resultTiles[_resultCursor] = currentPair.Tile;
        _resultCursor++;

        return false;
    }

    /// <summary>
    /// Must be called only when IsCompleted is true.
    /// </summary>
    /// <returns></returns>
    public TileDefinition[] GetTiles() => _resultTiles;
}