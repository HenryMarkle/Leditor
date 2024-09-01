namespace Leditor.Renderer.RL;

using ImGuiNET;
using rlImGui_cs;

using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;
using Leditor.Data;

using Color = Raylib_cs.Color;

/// <summary>
/// Consist of functions that draw to the enclosing draw mode (viewport).
/// Must be called after BeginDrawMode() and before EndDrawMode().
/// </summary>
public static class Draw
{
    public static void DrawQuad(Texture2D texture, Quad quad)
    {
        var flippedX = quad.TopLeft.X > quad.TopRight.X + 0.5f && quad.BottomLeft.X > quad.BottomRight.X + 0.5f;
        var flippedY = quad.TopLeft.Y > quad.BottomLeft.Y + 0.5f && quad.TopRight.Y > quad.BottomRight.Y + 0.5f;

        var (topRight, topLeft, bottomLeft, bottomRight) = (flippedX, flippedY) switch
        {
            (false, false) => (quad.TopRight, quad.TopLeft, quad.BottomLeft, quad.BottomRight),
            (false, true ) => (quad.BottomRight, quad.BottomLeft, quad.TopLeft, quad.TopRight),
            (true , false) => (quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft),
            (true , true ) => (quad.BottomLeft, quad.BottomRight, quad.TopRight, quad.TopLeft)
        };

        var ((vTopRightX, vTopRightY), (vTopLeftX, vTopLeftY), (vBottomLeftX, vBottomLeftY), (vBottomRightX, vBottomRightY)) = (flippedX, flippedY) switch
        {
            (false, false) => ((1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f), (1.0f, 1.0f)),
            (false, true) => ((1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f)),
            (true, false) => ((0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f)),
            (true, true) => ((0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f), (0.0f, 0.0f))
        };

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(255, 255, 255, 255);
        
        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(vTopLeftX, vTopLeftY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);

        Rlgl.TexCoord2f(vBottomLeftX, vBottomLeftY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(vBottomRightX, vBottomRightY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);

        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }
    
    public static void DrawQuad(Texture2D texture, Quad quad, Color color)
    {
        var flippedX = quad.TopLeft.X > quad.TopRight.X + 0.5f && quad.BottomLeft.X > quad.BottomRight.X + 0.5f;
        var flippedY = quad.TopLeft.Y > quad.BottomLeft.Y + 0.5f && quad.TopRight.Y > quad.BottomRight.Y + 0.5f;

        var (topRight, topLeft, bottomLeft, bottomRight) = (flippedX, flippedY) switch
        {
            (false, false) => (quad.TopRight, quad.TopLeft, quad.BottomLeft, quad.BottomRight),
            (false, true ) => (quad.BottomRight, quad.BottomLeft, quad.TopLeft, quad.TopRight),
            (true , false) => (quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft),
            (true , true ) => (quad.BottomLeft, quad.BottomRight, quad.TopRight, quad.TopLeft)
        };

        var ((vTopRightX, vTopRightY), (vTopLeftX, vTopLeftY), (vBottomLeftX, vBottomLeftY), (vBottomRightX, vBottomRightY)) = (flippedX, flippedY) switch
        {
            (false, false) => ((1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f), (1.0f, 1.0f)),
            (false, true) => ((1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f)),
            (true, false) => ((0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f)),
            (true, true) => ((0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f), (0.0f, 0.0f))
        };

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        
        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(vTopLeftX, vTopLeftY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);

        Rlgl.TexCoord2f(vBottomLeftX, vBottomLeftY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(vBottomRightX, vBottomRightY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);

        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    public static void DrawQuad(Texture2D texture, Rectangle source, Quad quad)
    {
        var flippedX = quad.TopLeft.X > quad.TopRight.X + 0.5f && quad.BottomLeft.X > quad.BottomRight.X + 0.5f;
        var flippedY = quad.TopLeft.Y > quad.BottomLeft.Y + 0.5f && quad.TopRight.Y > quad.BottomRight.Y + 0.5f;

        var (topRight, topLeft, bottomLeft, bottomRight) = (flippedX, flippedY) switch
        {
            (false, false) => (quad.TopRight, quad.TopLeft, quad.BottomLeft, quad.BottomRight),
            (false, true ) => (quad.BottomRight, quad.BottomLeft, quad.TopLeft, quad.TopRight),
            (true , false) => (quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft),
            (true , true ) => (quad.BottomLeft, quad.BottomRight, quad.TopRight, quad.TopLeft)
        };

        var left = source.X / texture.Width;
        var top = source.Y / texture.Height;
        var right = (source.X + source.Width) / texture.Width;
        var bottom = (source.Y + source.Height) / texture.Height;

        var ((vTopRightX, vTopRightY), (vTopLeftX, vTopLeftY), (vBottomLeftX, vBottomLeftY), (vBottomRightX, vBottomRightY)) = (flippedX, flippedY) switch
        {
            (false, false) => ((right, top), (left, top), (left, bottom), (right, bottom)),
            (false, true) => ((right, bottom), (left, bottom), (left, top), (right, top)),
            (true, false) => ((left, top), (right, top), (right, bottom), (left, bottom)),
            (true, true) => ((left, bottom), (right, bottom), (right, top), (left, top))
        };

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(255, 255, 255, 255);
        
        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(vTopLeftX, vTopLeftY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);

        Rlgl.TexCoord2f(vBottomLeftX, vBottomLeftY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(vBottomRightX, vBottomRightY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);

        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    public static void DrawQuad(Texture2D texture, Rectangle source, Quad quad, Color color)
    {
        var flippedX = quad.TopLeft.X > quad.TopRight.X + 0.5f && quad.BottomLeft.X > quad.BottomRight.X + 0.5f;
        var flippedY = quad.TopLeft.Y > quad.BottomLeft.Y + 0.5f && quad.TopRight.Y > quad.BottomRight.Y + 0.5f;

        var (topRight, topLeft, bottomLeft, bottomRight) = (flippedX, flippedY) switch
        {
            (false, false) => (quad.TopRight, quad.TopLeft, quad.BottomLeft, quad.BottomRight),
            (false, true ) => (quad.BottomRight, quad.BottomLeft, quad.TopLeft, quad.TopRight),
            (true , false) => (quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft),
            (true , true ) => (quad.BottomLeft, quad.BottomRight, quad.TopRight, quad.TopLeft)
        };

        var left = source.X / texture.Width;
        var top = source.Y / texture.Height;
        var right = (source.X + source.Width) / texture.Width;
        var bottom = (source.Y + source.Height) / texture.Height;

        var ((vTopRightX, vTopRightY), (vTopLeftX, vTopLeftY), (vBottomLeftX, vBottomLeftY), (vBottomRightX, vBottomRightY)) = (flippedX, flippedY) switch
        {
            (false, false) => ((right, top), (left, top), (left, bottom), (right, bottom)),
            (false, true) => ((right, bottom), (left, bottom), (left, top), (right, top)),
            (true, false) => ((left, top), (right, top), (right, bottom), (left, bottom)),
            (true, true) => ((left, bottom), (right, bottom), (right, top), (left, top))
        };

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        
        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(vTopLeftX, vTopLeftY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);

        Rlgl.TexCoord2f(vBottomLeftX, vBottomLeftY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(vBottomRightX, vBottomRightY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);

        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    public static void DrawQuad(Texture2D texture, Quad quad, bool flipX, bool flipY)
    {
        var (topRight, topLeft, bottomLeft, bottomRight) = (flipX, flipY) switch
        {
            (false, false) => (quad.TopRight, quad.TopLeft, quad.BottomLeft, quad.BottomRight),
            (false, true ) => (quad.BottomRight, quad.BottomLeft, quad.TopLeft, quad.TopRight),
            (true , false) => (quad.TopLeft, quad.TopRight, quad.BottomRight, quad.BottomLeft),
            (true , true ) => (quad.BottomLeft, quad.BottomRight, quad.TopRight, quad.TopLeft)
        };

        var ((vTopRightX, vTopRightY), (vTopLeftX, vTopLeftY), (vBottomLeftX, vBottomLeftY), (vBottomRightX, vBottomRightY)) = (flipX, flipY) switch
        {
            (false, false) => ((1.0f, 0.0f), (0.0f, 0.0f), (0.0f, 1.0f), (1.0f, 1.0f)),
            (false, true) => ((1.0f, 1.0f), (0.0f, 1.0f), (0.0f, 0.0f), (1.0f, 0.0f)),
            (true, false) => ((0.0f, 0.0f), (1.0f, 0.0f), (1.0f, 1.0f), (0.0f, 1.0f)),
            (true, true) => ((0.0f, 1.0f), (1.0f, 1.0f), (1.0f, 0.0f), (0.0f, 0.0f))
        };

        Rlgl.SetTexture(texture.Id);

        Rlgl.Begin(0x0007);
        Rlgl.Color4ub(255, 255, 255, 255);
        
        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.TexCoord2f(vTopLeftX, vTopLeftY);
        Rlgl.Vertex2f(topLeft.X, topLeft.Y);

        Rlgl.TexCoord2f(vBottomLeftX, vBottomLeftY);
        Rlgl.Vertex2f(bottomLeft.X, bottomLeft.Y);
        
        Rlgl.TexCoord2f(vBottomRightX, vBottomRightY);
        Rlgl.Vertex2f(bottomRight.X, bottomRight.Y);

        Rlgl.TexCoord2f(vTopRightX, vTopRightY);
        Rlgl.Vertex2f(topRight.X, topRight.Y);
        
        Rlgl.End();

        Rlgl.SetTexture(0);
    }

    public static void DrawTextureDarkest(Texture2D texture, Rectangle source, Rectangle destination)
    {
        BeginBlendMode(BlendMode.Custom);

        Rlgl.SetBlendFactors(1, 1, 0x8007);
        DrawTexturePro(texture, source, destination, Vector2.Zero, 0, Color.White);
        
        EndBlendMode();
    }

    public static void DrawToEffectColor(
        in Texture2D member, 
        in Rectangle src, 
        in Rectangle dest, 
        RenderTexture2D[] gradient,
        int lr,
        float zbleed, 
        float blend = float.NaN
    ) {
        if (float.IsNaN(blend)) blend = 1.0f;

        lr = Renderer.Utils.Restrict(lr, 0, 29);
    
        var dup = LoadRenderTexture(member.Width, member.Height);
        BeginTextureMode(dup);
        ClearBackground(Color.White);
        DrawTexture(member, 0, 0, Color.White);
        EndTextureMode();
        
        if (blend != 0)
        {
            BeginTextureMode(dup);
            DrawRectangle(0, 0, dup.Texture.Width, dup.Texture.Height, Color.White with { A = (byte)(100*(1.0 - blend)) });
            EndTextureMode();
        }

        BeginTextureMode(gradient[lr]);
        {
            DrawTextureDarkest(
                dup.Texture,
                src,
                dest
            );
        }
        EndTextureMode();

        if (zbleed <= 0) return;

        if (zbleed < 1)
        {
            BeginTextureMode(dup);
            DrawRectangle(0, 0, dup.Texture.Width, dup.Texture.Height, Color.White with { A = (byte)(100*(1.0 - zbleed)) });
            EndTextureMode();
        }

        var next = Renderer.Utils.Restrict(lr + 1, 0, 29);

        BeginTextureMode(gradient[next]);
        {
            DrawTextureDarkest(
                dup.Texture,
                src,
                dest
            );
        }
        EndTextureMode();

        next = Renderer.Utils.Restrict(lr - 1, 0, 29);

        BeginTextureMode(gradient[next]);
        {
            DrawTextureDarkest(
                dup.Texture,
                src,
                dest
            );
        }
        EndTextureMode();

        UnloadRenderTexture(dup);
    }

    /// <summary>
    /// Draws a simple progress bar.
    /// </summary>
    /// <param name="rect">the dimensions of progress bar</param>
    /// <param name="progress">a precentage from 0 to 1</param>
    /// <param name="color">the filling color</param>
    public static void ProgressBar(Rectangle rect, float progress, Color color)
    {
        DrawRectangleLinesEx(rect, 3, color);
        DrawRectangleRec(rect with { Width = rect.Width * progress }, color);
    }

    /// <summary>
    /// Draws the initial loading screen.
    /// </summary>
    /// <param name="tiles">tiles loading progress</param>
    /// <param name="props">props loading progress</param>
    /// <param name="materials">materials loading progress</param>
    public static void LoadingScreen(
        float cast,
        float tiles, 
        float props, 
        float materials
    )
    {
        ClearBackground(Color.Black);

        var castRect = new Rectangle(100, 200, GetScreenWidth() - 200, 30);
        var tilesRect = new Rectangle(100, 250, GetScreenWidth() - 200, 30);
        var propsRect = new Rectangle(100, 300, GetScreenWidth() - 200, 30);
        var materRect = new Rectangle(100, 350, GetScreenWidth() - 200, 30);

        ProgressBar(castRect, cast, Color.White);
        ProgressBar(tilesRect, tiles, Color.White);
        ProgressBar(propsRect, props, Color.White);
        ProgressBar(materRect, materials, Color.White);
    }
}
