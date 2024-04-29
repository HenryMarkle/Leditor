namespace Leditor;

#nullable enable

internal sealed class PaletteLoader : IDisposable
{
    public bool Disposed { get; private set; }

    public void Dispose() {
        if (Disposed) return;
        Disposed = true;

        if (_palettes.Count == 0) return;
    
        foreach (var texture in _palettes) Raylib.UnloadTexture(texture);
        _palettes.Clear();
    }

    ~PaletteLoader() {
        if (!Disposed) throw new InvalidOperationException("PaletteLoader wasn't disposed by consumer.");
    }

    private readonly IEnumerator<(string path, string name)> _palettePaths;

    private List<Texture2D> _palettes;
    private List<string> _paletteNames;

    public int TotalProgress { get; private set; }

    public bool Done { get; private set; }

    internal PaletteLoader(string dir)
    {
        if (!Directory.Exists(dir)) throw new DirectoryNotFoundException(dir);

        var paths = Directory
            .GetFiles(dir)
            .Select((f) => (f, Path.GetFileNameWithoutExtension(f)))
            .Where(f => f.Item2.StartsWith("palette") && f.Item1.EndsWith(".png"));

        TotalProgress = paths.Count();

        _palettePaths = paths.GetEnumerator();

        _palettes = [];
        _paletteNames = [];

    }

    internal bool Proceed() 
    {
        if (!_palettePaths.MoveNext()) 
        {
            Done = true;
            return true;
        }

        _palettes.Add(Raylib.LoadTexture(_palettePaths.Current.path));
        _paletteNames.Add(_palettePaths.Current.name);

        TotalProgress++;
        return false;
    }

    internal (Texture2D[] textures, string[] names) GetPalettes() 
    {
        if (!Done) throw new InvalidOperationException("PaletteLoader isn't done yet.");

        Texture2D[] array = [ .._palettes ];
    
        _palettes.Clear();

        return (array, _paletteNames.ToArray());
    }
}