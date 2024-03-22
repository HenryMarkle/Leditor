namespace Leditor.Data.Props.Definitions;

public interface ILayered
{
    (int Width, int Height) Size { get; }
    int[] Repeat { get; }
}