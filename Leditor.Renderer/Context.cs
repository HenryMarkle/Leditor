using Leditor.Data;
using Leditor.Serialization;
using Leditor.Serialization.IO;


namespace Leditor.Renderer;

public class Context
{
    public delegate void PageChangedEventHandler(int previous, int next);
    public event PageChangedEventHandler? PageChanged;

    public event EventHandler? LevelLoaded;

    private int _page;

    public int PrevPage { get; private set; }

    public int Page
    {
        get => _page;

        set
        {
            PrevPage = _page;
            _page = value;

            PageChanged?.Invoke(PrevPage, Page);
        }
    }


    public Serilog.ILogger? Logger { get; init; }

    public Registry? Registry { get; set; }

    public LevelState? Level { get; private set; }

    private Task? _levelLoadTask;

    public bool IsLoadingLevel => _levelLoadTask is { IsCompleted: false, IsFaulted: false };
    public Task? LT => _levelLoadTask;

    public void LoadLevelFromFile(string path)
    {
        if (Registry is not { Tiles: not null, Props: not null, Materials: not null }) return;

        _levelLoadTask = Task.Factory.StartNew(() => {
            var resultTask = Serialization.Level.Importers.LoadProjectAsync(
                path, 
                Registry.Tiles.Names, 
                Registry.Props.Names,
                Registry.Materials.Names
            );

            resultTask.Wait();

            var result = resultTask.Result;

            Level ??= new(0, 0, (0, 0, 0, 0), null);

            Level.Import(
                result.Width,
                result.Height,
                (result.BufferTiles.Left, result.BufferTiles.Top, result.BufferTiles.Right, result.BufferTiles.Bottom),
                result.GeoMatrix,
                result.TileMatrix,
                result.MaterialColorMatrix,
                result.Effects,
                result.Cameras,
                result.PropsArray,
                result.LightSettings,
                result.LightMode,
                result.DefaultTerrain,
                result.Seed,
                result.WaterLevel,
                result.WaterInFront,
                result.DefaultMaterial,
                result.Name
            );

            LevelLoaded?.Invoke(this, EventArgs.Empty);
        });
    }
}