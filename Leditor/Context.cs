namespace Leditor;

// TODO: A replacement of GLOBALS
public sealed class Context
{
    public LevelState Level { get; set; }
    public int Layer { get; set; }
    public string ProjectPath { get; set; } = string.Empty;
    
    public int[] CamQuadLocks { get; set; } = [0, 0];
    public int CamLock { get; set; }
    
    public int Page { get; set; } = 0;
    public int PreviousPage { get; set; } = 0;
    
    public bool ResizeFlag { get; set; }
    public bool NewFlag { get; set; }
    
    public Camera2D Camera { get; set; }
    
    public Gram Gram { get; set; } = new(100);

    // TODO: Unfinished
    public async Task<bool> LoadProjectAsync(string path)
    {
        var result = await Utils.LoadProjectAsync(path);

        if (!result.Success) return false;
        
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

        ProjectLoaded?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public async Task CreateProjectAsync(
        int width,
        int height,
        (int left, int top, int right, int bottom) padding,
        int[] geoIdFill)
    {
        await Task.Factory.StartNew(() => Level.New(width, height, padding, geoIdFill));
        ProjectCreated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler ProjectCreated; 
    public event EventHandler ProjectLoaded;
}