namespace Leditor.Data.Props.Definitions;

public interface IPixelSized
{
    (int Width, int Height) SizeInPixels { get; }
}