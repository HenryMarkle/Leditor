namespace Leditor;

public class TileTexturesLoader : IDisposable
{
    private Image[][] _tempImages = [];
    private Texture[][] _array = [];

    public Texture[][] Textures => _array;

    public async Task<List<Action>> PrepareFromPathsAsync(string[][] paths)
    {
        var images = paths.Select(c => 
            c.Select(t => 
                Task.Factory.StartNew(() => Raylib.LoadImage(t))).ToArray()).ToArray();

        _array = new Texture[images.Length][];
        _tempImages = new Image[images.Length][];

        List<Action> actions = [];
        
        for (var category = 0; category < images.Length; category++)
        {
            _array[category] = new Texture[images[category].Length];
            _tempImages[category] = new Image[images[category].Length];

            for (var image = 0; image < images[category].Length; image++)
            {
                _tempImages[category][image] = await images[category][image];

                var categoryCopy = category;
                var indexCopy = image;
                
                actions.Add(() =>
                {
                    _array[categoryCopy][indexCopy] = Raylib.LoadTextureFromImage(_tempImages[categoryCopy][indexCopy]);
                    Raylib.UnloadImage(_tempImages[categoryCopy][indexCopy]);
                });
            }
        }

        return actions;
    }

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here

        foreach (var category in _array)
        {
            foreach (var texture in category)
            {
                Raylib.UnloadTexture(texture);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            // TODO release managed resources here
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~TileTexturesLoader()
    {
        Dispose(false);
    }
}
public class PropTexturesLoader : IDisposable
{
    private Image[] _tempRopesImages = [];
    private Image[] _tempLongsImages = [];
    private Image[][] _tempOthersImages = [];
    
    private Texture[] _ropes = [];
    private Texture[] _longs = [];
    private Texture[][] _others = [];

    public Texture[] Ropes => _ropes;
    public Texture[] Longs => _longs;
    public Texture[][] Others => _others;

    public async Task<List<Action>> PrepareFromPathsAsync(
        string[] ropes,
        string[] longs,
        string[][] others)
    {
        var ropeImages = ropes.Select(r => Task.Factory.StartNew(() => Raylib.LoadImage(r))).ToArray();
        var longsImages = longs.Select(l => Task.Factory.StartNew(() => Raylib.LoadImage(l))).ToArray();
        
        var othersImages = others.Select(c => 
            c.Select(p => 
                Task.Factory.StartNew(() => Raylib.LoadImage(p))).ToArray()).ToArray();

        _tempRopesImages = new Image[ropeImages.Length];
        _tempLongsImages = new Image[longsImages.Length];
        _tempOthersImages = new Image[othersImages.Length][];
        
        _ropes = new Texture[_tempRopesImages.Length];
        _longs = new Texture[_tempLongsImages.Length];
        _others = new Texture[_tempOthersImages.Length][];
        
        List<Action> actions = [];
        
        // Ropes
        
        for (var index = 0; index < ropeImages.Length; index++)
        {
            _tempRopesImages[index] = await ropeImages[index];
            
            var indexCopy = index;
                
            actions.Add(() =>
            {
                _ropes[indexCopy] = Raylib.LoadTextureFromImage(_tempRopesImages[indexCopy]);
                Raylib.UnloadImage(_tempRopesImages[indexCopy]);
            });
        }
        
        // Longs
        
        for (var index = 0; index < longsImages.Length; index++)
        {
            _tempLongsImages[index] = await longsImages[index];
            
            var indexCopy = index;
                
            actions.Add(() =>
            {
                _ropes[indexCopy] = Raylib.LoadTextureFromImage(_tempLongsImages[indexCopy]);
                Raylib.UnloadImage(_tempLongsImages[indexCopy]);
            });
        }
        
        // Others

        for (var category = 0; category < othersImages.Length; category++)
        {
            _tempOthersImages[category] = new Image[othersImages[category].Length];
            _others[category] = new Texture[othersImages[category].Length];

            for (var index = 0; index < othersImages[category].Length; index++)
            {
                _tempOthersImages[category][index] = await othersImages[category][index];
                
                var categoryCopy = category;
                var indexCopy = index;
                
                actions.Add(() =>
                {
                    _others[categoryCopy][indexCopy] = Raylib.LoadTextureFromImage(_tempOthersImages[categoryCopy][indexCopy]);
                    Raylib.UnloadImage(_tempOthersImages[categoryCopy][indexCopy]);
                });
            }
        }

        return actions;
    }

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here

        foreach (var texture in _ropes) Raylib.UnloadTexture(texture);
        foreach (var texture in _longs) Raylib.UnloadTexture(texture);

        foreach (var category in _others)
        {
            foreach (var texture in category)
            {
                Raylib.UnloadTexture(texture);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            // TODO release managed resources here
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~PropTexturesLoader()
    {
        Dispose(false);
    }
}

public class LightTexturesLoader : IDisposable
{
    private Image[] _images = [];
    private Texture[] _array = [];

    public Texture[] Textures => _array;

    public async Task<List<Action>> PrepareFromPathsAsync(string[] paths)
    {
        var images = paths.Select(i => 
            Task.Factory.StartNew(() => Raylib.LoadImage(i)))
            .ToArray();

        _images = new Image[images.Length];
        _array = new Texture[images.Length];

        List<Action> actions = [];
        
        for (var index = 0; index < images.Length; index++)
        {
            _images[index] = await images[index];

            var indexCopy = index;
            
            actions.Add(() =>
            {
                _array[indexCopy] = Raylib.LoadTextureFromImage(_images[indexCopy]);
                Raylib.UnloadImage(_images[indexCopy]);
            });
        }

        return actions;
    }

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            foreach (var texture in _array) Raylib.UnloadTexture(texture);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~LightTexturesLoader()
    {
        Dispose(false);
    }
}