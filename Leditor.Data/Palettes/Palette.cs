namespace Leditor.Data.Palettes;

public class Palette 
{
    // Sun Section
    public Color[] SunHighlight { get; }
    public Color[] SunBase { get; }
    public Color[] SunShadow { get; }

    // Shade Section
    public Color[] ShadeHighlight { get; }
    public Color[] ShadeBase { get; }
    public Color[] ShadeShadow { get; }

    //
    public Palette(
        Color[] sunHighlight, Color[] sunBase, Color[] sunShadow,
        Color[] shadeHighlight, Color[] shadeBase, Color[] shadeShadow
    )
    {
        if (sunHighlight.Length < 30) throw new ArgumentException("Sun highlight array must be at least of length 30", nameof(sunHighlight));
        if (sunBase.Length < 30) throw new ArgumentException("Sun base array must be at least of length 30", nameof(sunBase));
        if (sunShadow.Length < 30) throw new ArgumentException("Sun shadow array must be at least of length 30", nameof(sunShadow));
        
        if (shadeHighlight.Length < 30) throw new ArgumentException("Shade highlight array must be at least of length 30", nameof(shadeHighlight));
        if (shadeBase.Length < 30) throw new ArgumentException("Shade base array must be at least of length 30", nameof(shadeBase));
        if (shadeShadow.Length < 30) throw new ArgumentException("Shade shadow array must be at least of length 30", nameof(shadeShadow));
    
        SunHighlight = [..sunHighlight];
        SunBase = [..sunBase];
        SunShadow = [..sunShadow];

        ShadeHighlight = [..shadeHighlight];
        ShadeBase = [..shadeBase];
        ShadeShadow = [..shadeShadow];
    }
}