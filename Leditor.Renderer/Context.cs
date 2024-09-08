using Leditor.Data;
using Leditor.Serialization;
using Leditor.Serialization.IO;
using Leditor.Renderer.Partial;


namespace Leditor.Renderer;

/// <summary>
/// Shared state between pages
/// </summary>
public class Context
{
    public delegate void PageChangedEventHandler(int previous, int next);
    public event PageChangedEventHandler? PageChanged;

    public event EventHandler? LevelLoadingStarted;
    public event EventHandler? LevelLoadingFailed;
    public event EventHandler? LevelLoaded;

    public event EventHandler? TerminationSignal;

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

    public long Frame { get; set; }

    public Serilog.ILogger? Logger { get; init; }

    public Textures? Textures { get; set; }

    public Registry? Registry { get; set; }

    public LevelState? Level { get; private set; }
    public Engine? Engine { get; set; }

    private Task? _levelLoadTask;

    public bool IsLoadingLevel => _levelLoadTask is { IsCompleted: false, IsFaulted: false };

    public void SignalTermination(object sender)
    {
        if (sender.GetType() == typeof(Program)) TerminationSignal?.Invoke(this, EventArgs.Empty);
    }

    public void LoadLevelFromFile(string path)
    {
        if (Registry is not { Tiles: not null, Props: not null, Materials: not null }) return;

        LevelLoadingStarted?.Invoke(this, EventArgs.Empty);

        _levelLoadTask = Task.Factory.StartNew(() => {
            try {
                var resultTask = Serialization.Level.Importers.LoadProjectAsync(
                    path, 
                    Registry.Tiles.Names, 
                    Registry.Props.Names,
                    Registry.Materials.Names
                );

                resultTask.Wait();

                var result = resultTask.Result;

                Level ??= new(0, 0, (0, 0, 0, 0), new("Undefined", new()));

                Level.Import(
                    result.Width,
                    result.Height,
                    (result.BufferTiles.Left, result.BufferTiles.Top, result.BufferTiles.Right, result.BufferTiles.Bottom),
                    result.GeoMatrix!,
                    result.TileMatrix!,
                    result.MaterialColorMatrix!,
                    result.Effects,
                    result.Cameras,
                    result.PropsArray ?? [],
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
                
                LevelLoaded?.Invoke(this, EventArgs.Empty);
            } catch (Exception e) {
                Logger?.Error(e, "[Context] Failed to load level");
                Console.WriteLine("Failed to load level: "+e);
                LevelLoadingFailed?.Invoke(this, EventArgs.Empty);
            }

        });
    }
}