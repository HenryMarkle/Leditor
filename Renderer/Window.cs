using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using RenderState = Leditor.Renderer.DrizzleRender.RenderState;
namespace Leditor.Renderer;

internal class Camera
{
    // camera size is 70x39 tiles
    // camera red border is 52.5x35
    // left black inner border is 1 tile away
    // game resolution is 1040x800 
    // render scales up pixels by 1.25 (each tile is 16 pixels) (me smort. i was already aware tiles were 20 px large)
    // quad save format: [A, O]
    //  A: clockwise angle in degrees, where 0 is up
    //  O: offset number from 0 to 1 (1.0 translate to 4 tiles) 
    // corner order is: TL, TR, BR, BL
    public Vector2 Position;
    public float[] CornerOffsets = new float[4];
    public float[] CornerAngles = new float[4];

    public readonly static Vector2 WidescreenSize = new(70f, 40f);
    public readonly static Vector2 StandardSize = new(52.5f, 40f);

    public Camera(Vector2 position)
    {
        Position = position;
    }

    public Camera() : this(new(1f, 1f))
    {}


    public Vector2 GetCornerOffset(int cornerIndex)
    {
        return new Vector2(
            MathF.Sin(CornerAngles[cornerIndex]),
            -MathF.Cos(CornerAngles[cornerIndex])
        ) * CornerOffsets[cornerIndex] * 4f;
    }

    public Vector2 GetCornerPosition(int cornerIndex, bool offset)
    {
        int x = (cornerIndex == 1 || cornerIndex == 2) ? 1 : 0;
        int y = (cornerIndex & 2) >> 1;
        
        return offset ?
            Position + new Vector2(WidescreenSize.X * x, WidescreenSize.Y * y) + GetCornerOffset(cornerIndex)
            :
            Position + new Vector2(WidescreenSize.X * x, WidescreenSize.Y * y);
    }
}

class DrizzleRenderWindow : IDisposable
{
    public readonly DrizzleRender? drizzleRenderer;
    private bool isOpen = false;
    private readonly Texture2D[]? previewTextures = null;
    private readonly RenderTexture2D previewComposite;
    private bool needUpdateTextures = false;

    private const string LayerPreviewShaderSource = @"
        #version 330

        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform sampler2D texture0;
        uniform vec4 colDiffuse;

        out vec4 finalColor;

        void main()
        {
            vec4 texelColor = texture(texture0, fragTexCoord);
            bool isWhite = texelColor.r == 1.0 && texelColor.g == 1.0 && texelColor.b == 1.0;
            vec3 correctColor = texelColor.bgr;
            
            finalColor = vec4(
                mix(correctColor, vec3(1.0, 1.0, 1.0), fragColor.r * 0.8),
                1.0 - float(isWhite)
            ) * colDiffuse;
        }    
    ";

    private Shader layerPreviewShader;

    public DrizzleRenderWindow()
    {
        try
        {
            drizzleRenderer = new DrizzleRender();
            drizzleRenderer.PreviewUpdated += () =>
            {
                needUpdateTextures = true;
            };

            previewTextures = new Texture2D[30];

            for (int i = 0; i < 30; i++)
            {
                previewTextures[i] = Raylib.LoadTextureFromImage(drizzleRenderer.RenderLayerPreviews[i]);
            }
        }
        catch (Exception e)
        {
            // TODO: report the problem
        }

        layerPreviewShader = Raylib.LoadShaderFromMemory(null, LayerPreviewShaderSource);

        previewComposite = Raylib.LoadRenderTexture(
            (int)Camera.WidescreenSize.X * 20,
            (int)Camera.WidescreenSize.Y * 20
        );

        Raylib.BeginTextureMode(previewComposite);
        Raylib.ClearBackground(Color.White);
        Raylib.EndTextureMode();
    }

    public void Dispose()
    {
        drizzleRenderer?.Dispose();
        
        Raylib.UnloadRenderTexture(previewComposite);

        if (previewTextures is not null)
        {
            for (var i = 0; i < previewTextures.Length; i++)
            {
                // previewTextures[i].Dispose();
                
                Raylib.UnloadTexture(previewTextures[i]);
            }
        }

        Raylib.UnloadShader(layerPreviewShader);
    }

    public bool DrawWindow()
    {
        if (!isOpen)
        {
            ImGui.OpenPopup("Render");
            isOpen = false;
        }

        var doClose = false;

        drizzleRenderer?.Update();

        if (ImGui.BeginPopupModal("Render"))
        {
            bool cancelDisabled = true;
            bool closeDisabled = false;
            bool revealDisabled = true;
            float renderProgress = 0f;

            if (drizzleRenderer is not null)
            {
                // yes i know this code is a bit iffy
                cancelDisabled =
                    drizzleRenderer.State == RenderState.Cancelling ||
                    drizzleRenderer.State == RenderState.Errored || drizzleRenderer.IsDone;
                
                closeDisabled = !drizzleRenderer.IsDone && drizzleRenderer.State != RenderState.Errored;
                revealDisabled = !drizzleRenderer.IsDone || drizzleRenderer.State == RenderState.Canceled;
                renderProgress = drizzleRenderer.RenderProgress;
            }

            // cancel button (disabled if cancelling/canceled)
            if (cancelDisabled)
                ImGui.BeginDisabled();
            
            if (ImGui.Button("Cancel"))
                drizzleRenderer?.Cancel();
            
            if (cancelDisabled)
                ImGui.EndDisabled();

            // close button (disabled if render process is not done)
            if (closeDisabled)
                ImGui.BeginDisabled();
            
            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                doClose = true;
                ImGui.CloseCurrentPopup();
            }

            if (closeDisabled)
                ImGui.EndDisabled();

            // show in file browser button
            if (revealDisabled)
                ImGui.BeginDisabled();
            
            ImGui.SameLine();
            // if (ImGui.Button("Show In File Browser"))
            //     RainEd.Instance.ShowPathInSystemBrowser(Path.Combine(
            //         Boot.AppDataPath,
            //         "Data", "Levels",
            //         Path.GetFileNameWithoutExtension(RainEd.Instance.CurrentFilePath) + ".txt"
            //     ), true);
            
            if (revealDisabled)
                ImGui.EndDisabled();
            
            ImGui.SameLine();
            ImGui.ProgressBar(renderProgress, new Vector2(-1.0f, 0.0f));

            // status sidebar
            if (ImGui.BeginChild("##status", new Vector2(ImGui.GetTextLineHeight() * 20.0f, ImGui.GetContentRegionAvail().Y)))
            {
                if (drizzleRenderer is null || drizzleRenderer.State == RenderState.Errored)
                {
                    ImGui.Text("An error occured!\nCheck the logs for more info.");
                }
                else if (drizzleRenderer.State == RenderState.Cancelling)
                {
                    ImGui.Text("Cancelling...");
                }
                else if (drizzleRenderer.State == RenderState.Canceled)
                {
                    ImGui.Text("Canceled");
                }
                else if (drizzleRenderer.State == RenderState.Errored)
                {
                }
                else
                {
                    if (drizzleRenderer.State == RenderState.Finished)
                    {
                        ImGui.Text("Done!");
                    }
                    else if (drizzleRenderer.State == RenderState.Initializing)
                    {
                        ImGui.Text("Initializing Zygote runtime...");
                    }
                    else
                    {
                        ImGui.Text($"Rendering {drizzleRenderer.CamerasDone+1} of {drizzleRenderer.CameraCount} cameras...");
                    }

                    ImGui.TextUnformatted(drizzleRenderer.DisplayString);
                }
            } ImGui.EndChild();

            // preview image
            ImGui.SameLine();

            if (needUpdateTextures && previewTextures is not null)
            {
                needUpdateTextures = false;

                for (int i = 0; i < 30; i++)
                {
                    // drizzleRenderer!.RenderLayerPreviews[i].UpdateTexture(previewTextures[i]);

                    unsafe
                    {
                        Raylib.UpdateTexture(previewTextures[i], drizzleRenderer!.RenderLayerPreviews[i].Data);
                    }
                }
                
                UpdateComposite();
            }

            int cWidth = previewComposite.Texture.Width;
            int cHeight = previewComposite.Texture.Height;
            rlImGui.ImageRect(
                previewComposite.Texture,
                (int)(cWidth / 1.25f), (int)(cHeight / 1.25f),
                new Rectangle(0, cHeight, cWidth, -cHeight)
            );
            
            ImGui.EndPopup();
        }

        return doClose;
    }

    private void UpdateComposite()
    {
        Raylib.BeginTextureMode(previewComposite);
        Raylib.ClearBackground(Color.White);

        Raylib.BeginShaderMode(layerPreviewShader);

        for (int i = 29; i >= 0; i--)
        {
            float fadeValue = i / 30f;
            Raylib.DrawTexture(previewTextures![i], -300, -200, new Color((int)(fadeValue * 255f), 0, 0, 255));
        }
        Raylib.EndShaderMode();

        Raylib.EndTextureMode();
    }
}