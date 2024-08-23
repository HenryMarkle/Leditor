namespace Leditor.Renderer;

using Raylib_cs;
using static Raylib_cs.Raylib;

public class Canvas
{
    public RenderTexture2D[] Sublayers { get; private set; }

    public Canvas()
    {
        Sublayers = new RenderTexture2D[30];

        for (var i = 0; i < 30; i++)
        {
            var rt = LoadRenderTexture(2000, 1200);

            BeginTextureMode(rt);
            ClearBackground(Color.White);
            EndTextureMode();

            Sublayers[i] = rt;
        }
    }
}