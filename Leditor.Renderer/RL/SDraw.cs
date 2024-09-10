using Raylib_cs;
using static Raylib_cs.Raylib;

using System.Numerics;
using Leditor.Data;

using Color = Raylib_cs.Color;

namespace Leditor.Renderer.RL;

/// <summary>
/// A namespace containing draw functions that draw into the viewport and use shaders.
/// Call after BeginDrawing() and before EndDrawing().
/// </summary>
public static class SDraw
{
    public static void Draw_NoWhite_NoColor(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Rectangle src, 
        Rectangle dest
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            DrawTexturePro(
                texture,
                src,
                dest,
                Vector2.Zero,
                0,
                Color.White
            );
            EndShaderMode();
        }
        EndTextureMode();
    }

    public static void Draw_NoWhite_NoColor(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Rectangle src, 
        Quad dest
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            Draw.DrawQuad(
                texture,
                src,
                dest
            );
            EndShaderMode();
        }
        EndTextureMode();
    }
    
    public static void Draw_NoWhite_NoColor(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Quad dest
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            Draw.DrawQuad(
                texture,
                new Rectangle(0, 0, texture.Width, texture.Height),
                dest
            );
            EndShaderMode();
        }
        EndTextureMode();
    }

    public static void Draw_NoWhite_Color(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Rectangle src, 
        Rectangle dest,
        Color color
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            DrawTexturePro(
                texture,
                src,
                dest,
                Vector2.Zero,
                0,
                color
            );
            EndShaderMode();
        }
        EndTextureMode();
    }

    public static void Draw_NoWhite_Color(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Rectangle dest,
        Color color
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            DrawTexturePro(
                texture,
                new Rectangle(0, 0, texture.Width, texture.Height),
                dest,
                Vector2.Zero,
                0,
                color
            );
            EndShaderMode();
        }
        EndTextureMode();
    }

    public static void Draw_NoWhite_Color(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Rectangle src, 
        Quad dest,
        Color color
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            Draw.DrawQuad(
                texture,
                src,
                dest,
                color
            );
            EndShaderMode();
        }
        EndTextureMode();
    }
    
    public static void Draw_NoWhite_Color(
        RenderTexture2D rt, 
        Texture2D texture, 
        Shader shader, 
        Quad dest,
        Color color
    )
    {
        BeginTextureMode(rt);
        {
            BeginShaderMode(shader);
            SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), texture);
            Draw.DrawQuad(
                texture,
                new Rectangle(0, 0, texture.Width, texture.Height),
                dest,
                color
            );
            EndShaderMode();
        }
        EndTextureMode();
    }
}