namespace Leditor;

public class TileTexturesLoader
{
    private Image[][] _tempImages = [];
    private Texture2D[][] _array = [];

    public Texture2D[][] Textures => _array;

    public async Task<List<Action>> PrepareFromPathsAsync(string[][] paths)
    {
        var images = paths.Select(c => 
            c.Select(t => 
                Task.Factory.StartNew(() => Raylib.LoadImage(t))).ToArray()).ToArray();

        _array = new Texture2D[images.Length][];
        _tempImages = new Image[images.Length][];

        List<Action> actions = [];
        
        for (var category = 0; category < images.Length; category++)
        {
            _array[category] = new Texture2D[images[category].Length];
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
}
public class PropTexturesLoader
{
    private Image[] _tempRopesImages = [];
    private Image[] _tempLongsImages = [];
    private Image[][] _tempOthersImages = [];
    
    private Texture2D[] _ropes = [];
    private Texture2D[] _longs = [];
    private Texture2D[][] _others = [];

    public Texture2D[] Ropes => _ropes;
    public Texture2D[] Longs => _longs;
    public Texture2D[][] Others => _others;

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
        
        _ropes = new Texture2D[_tempRopesImages.Length];
        _longs = new Texture2D[_tempLongsImages.Length];
        _others = new Texture2D[_tempOthersImages.Length][];
        
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
                Raylib.ImageCrop(ref _tempLongsImages[indexCopy], new Rectangle(0, 1, _tempLongsImages[indexCopy].Width, _tempLongsImages[indexCopy].Height - 1));
                _longs[indexCopy] = Raylib.LoadTextureFromImage(_tempLongsImages[indexCopy]);
                Raylib.UnloadImage(_tempLongsImages[indexCopy]);
            });
        }
        
        // Others

        for (var category = 0; category < othersImages.Length; category++)
        {
            _tempOthersImages[category] = new Image[othersImages[category].Length];
            _others[category] = new Texture2D[othersImages[category].Length];

            for (var index = 0; index < othersImages[category].Length; index++)
            {
                _tempOthersImages[category][index] = await othersImages[category][index];
                
                var categoryCopy = category;
                var indexCopy = index;
                
                actions.Add(() =>
                {
                    ref var img = ref _tempOthersImages[categoryCopy][indexCopy];

                    Raylib.ImageCrop(ref img, new Rectangle(0, 1, img.Width, img.Height - 1));

                    _others[categoryCopy][indexCopy] = Raylib.LoadTextureFromImage(img);
                    Raylib.UnloadImage(img);
                });
            }
        }

        return actions;
    }
}
public class LightTexturesLoader
{
    private Image[] _images = [];
    private Texture2D[] _array = [];

    public Texture2D[] Textures => _array;

    public async Task<List<Action>> PrepareFromPathsAsync(string[] paths)
    {
        var images = paths.Select(i => 
            Task.Factory.StartNew(() => Raylib.LoadImage(i)))
            .ToArray();

        _images = new Image[images.Length];
        _array = new Texture2D[images.Length];

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
}