namespace Leditor.Data.Geometry;

#nullable disable

public enum GeoType
{
    Air = 0,
    Solid = 1,
    // ReSharper disable once InconsistentNaming
    SlopeNE = 2,
    // ReSharper disable once InconsistentNaming
    SlopeNW = 3,
    // ReSharper disable once InconsistentNaming
    SlopeES = 4,
    // ReSharper disable once InconsistentNaming
    SlopeSW = 5,
    Platform = 6,
    ShortcutEntrance = 7,
    Glass = 9
}

public struct Geo
{
    public GeoType Type { get; set; }

    private bool[] _features;
    public bool[] Features
    {
        get => _features;
        set
        {
            if (value.Length != 22) 
                throw new InvalidDataException("Features array must be of size 22");

            _features = [..value];
        }
    }

    public Geo()
    {
        Type = GeoType.Air;
        _features = new bool[22];
    }

    public Geo(GeoType type)
    {
        Type = type;
        _features = new bool[22];
    }

    public Geo(GeoType type, bool[] features)
    {
        if (features.Length != 22) 
            throw new ArgumentException("Features array must be of size 22");
        
        Type = type;
        _features = features;
    }
}