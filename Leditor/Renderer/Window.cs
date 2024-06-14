using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using RenderState = Leditor.Renderer.DrizzleRender.RenderState;
namespace Leditor.Renderer;

class DrizzleRenderWindow : IDisposable
{
    public readonly DrizzleRender? drizzleRenderer;
    private bool isOpen = false;
    private readonly RL.Managed.Texture2D?[]? previewLayers = null;
    private RL.Managed.Texture2D? previewBlackout1 = null;
    private RL.Managed.Texture2D? previewBlackout2 = null;
    private readonly RL.Managed.RenderTexture2D previewComposite;
    private bool needUpdateTextures = false;

    private const string LayerPreviewShaderSource = @"
        #version 330

        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform sampler2D texture0;
        uniform vec4 colDiffuse;

        out vec4 FragColor;

        void main()
        {
            vec4 texelColor = texture(texture0, fragTexCoord);
            bool isWhite = texelColor.r == 1.0 && texelColor.g == 1.0 && texelColor.b == 1.0;
            vec3 correctColor = texelColor.bgr;
            
            FragColor = vec4(
                mix(correctColor, vec3(1.0), fragColor.r * 0.8),
                1.0 - float(isWhite)
            ) * colDiffuse;
        }    
    ";

    private const string LayerPreviewLightShaderSource = @"
        #version 330

        in vec2 fragTexCoord;
        in vec4 fragColor;

        uniform sampler2D texture0;
        uniform vec4 colDiffuse;

        out vec4 FragColor;

        void main()
        {
            vec4 texelColor = texture(texture0, fragTexCoord);
            bool isWhite = texelColor.r == 1.0 && texelColor.g == 1.0 && texelColor.b == 1.0;
            vec3 correctColor = texelColor.bgr;
            
            FragColor = vec4(
                vec3(1.0, 0.0, 0.0),
                1.0 - float(isWhite)
            ) * colDiffuse;
        }    
    ";

    private RL.Managed.Shader layerPreviewShader;
    private RL.Managed.Shader layerPreviewLightShader;

    public DrizzleRenderWindow()
    {
        try
        {
            drizzleRenderer = new DrizzleRender();
            drizzleRenderer.PreviewUpdated += () =>
            {
                needUpdateTextures = true;
            };

            previewLayers = new RL.Managed.Texture2D[30];
        }
        catch (Exception e)
        {
            // RainEd.Logger.Error("Error occured when initializing render:\n{ErrorMessage}", e);
        }

        layerPreviewShader = new RL.Managed.Shader(Raylib.LoadShaderFromMemory(null, LayerPreviewShaderSource));
        layerPreviewLightShader = new RL.Managed.Shader(Raylib.LoadShaderFromMemory(null, LayerPreviewLightShaderSource));

        previewComposite = new RL.Managed.RenderTexture2D(70 * 20, 40 * 20);

        Raylib.BeginTextureMode(previewComposite);
        Raylib.ClearBackground(Color.White);
        Raylib.EndTextureMode();
    }

    public void Dispose()
    {
        drizzleRenderer?.Dispose();
        previewComposite.Dispose();

        if (previewLayers is not null)
        {
            for (int i = 0; i < previewLayers.Length; i++)
            {
                previewLayers[i]?.Dispose();
            }
        }

        previewBlackout1?.Dispose();
        previewBlackout2?.Dispose();

        layerPreviewShader.Dispose();
        layerPreviewLightShader.Dispose();
    }

    private void ShowControlButtons(ref bool doClose)
    {
        bool cancelDisabled = true;
        bool closeDisabled = false;
        bool revealDisabled = true;

        if (drizzleRenderer is not null)
        {
            // yes i know this code is a bit iffy
            cancelDisabled =
                drizzleRenderer.State == RenderState.Cancelling ||
                drizzleRenderer.State == RenderState.Errored || drizzleRenderer.IsDone;
            
            closeDisabled = !drizzleRenderer.IsDone && drizzleRenderer.State != RenderState.Errored;
            revealDisabled = !drizzleRenderer.IsDone || drizzleRenderer.State == RenderState.Canceled;
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
        
        // ImGui.SameLine();
        // if (ImGui.Button("Show In File Browser"))
        //     RainEd.Instance.ShowPathInSystemBrowser(Path.Combine(
        //         RainEd.Instance.AssetDataPath,
        //         "Levels",
        //         Path.GetFileNameWithoutExtension(RainEd.Instance.CurrentFilePath) + ".txt"
        //     ), true);
        
        if (revealDisabled)
            ImGui.EndDisabled();
    }

    private void ShowStatusText()
    {
        // status sidebar
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
            else if (drizzleRenderer.State == RenderState.Loading)
            {
                ImGui.Text("Loading level...");
            }
            else
            {
                ImGui.Text($"Rendering {drizzleRenderer.CamerasDone+1} of {drizzleRenderer.CameraCount} cameras...");
            }

            ImGui.TextUnformatted(drizzleRenderer.DisplayString);
        }
    }

    public bool DrawWindow(Serilog.ILogger? logger = null)
    {
        if (!isOpen)
        {
            ImGui.OpenPopup("Render");
            isOpen = false;
        }

        var doClose = false;

        drizzleRenderer?.Update(logger);

        ImGuiExt.CenterNextWindow(ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(new Vector2(0f, ImGui.GetTextLineHeight() * 30.0f), Vector2.One * 9999f);
        if (ImGuiExt.BeginPopupModal("Render", ImGuiWindowFlags.AlwaysAutoResize))
        {
            bool isPreviewEnabled = drizzleRenderer!.PreviewImages is not null;
            float renderProgress = 0f;

            if (drizzleRenderer is not null)
                renderProgress = drizzleRenderer.RenderProgress;

            // if preview is enabled, show the progress bar above the preview image
            // otherwise show it below the button line and above the status text
            if (isPreviewEnabled)
            {
                ImGui.BeginGroup();
                ShowControlButtons(ref doClose);
                ShowStatusText();
                ImGui.EndGroup();

                ImGui.SameLine();
                ImGui.BeginGroup();
                ImGui.ProgressBar(renderProgress, new Vector2(-0.000001f, 0.0f));
                
                // update the preview texture
                var previewImages = drizzleRenderer!.PreviewImages;
                if (needUpdateTextures && previewImages is not null)
                {
                    if (previewLayers is null)
                        throw new NullReferenceException("previewLayers is null");

                    needUpdateTextures = false;

                    drizzleRenderer.UpdatePreviewImages();

                    // update preview images
                    for (int i = 0; i < 30; i++)
                    {
                        var img = previewImages.Layers[i];
                        UpdateTexture(img, ref previewLayers[i]);
                    }
                    UpdateTexture(previewImages.BlackOut1, ref previewBlackout1);
                    UpdateTexture(previewImages.BlackOut2, ref previewBlackout2);
                    UpdateComposite();
                }

                int cWidth = previewComposite.Raw.Texture.Width;
                int cHeight = previewComposite.Raw.Texture.Height;
                rlImGui.ImageRect(
                    previewComposite.Raw.Texture,
                    (int)(cWidth / 1.25f), (int)(cHeight / 1.25f),
                    new Rectangle(0, cHeight, cWidth, -cHeight)
                );
                ImGui.EndGroup();
            }
            else
            {
                ShowControlButtons(ref doClose);

                ImGui.ProgressBar(renderProgress, new Vector2(ImGui.GetContentRegionAvail().X, 0.0f));
                ShowStatusText();
            }
            
            ImGui.EndPopup();
        }

        return doClose;
    }

    private static void UpdateTexture(RL.Managed.Image? img, ref RL.Managed.Texture2D? tex)
    {
        if (img == null)
        {
            tex?.Dispose();
            tex = null;
            return;
        }

        if (tex == null || img.Raw.Width != tex.Raw.Width || img.Raw.Height != tex.Raw.Height)
        {
            tex?.Dispose();
            tex = new RL.Managed.Texture2D(img);
        }
        else
        {
            tex.Update(img);
        }
    }

    private void UpdateComposite()
    {
        Raylib.BeginTextureMode(previewComposite);
        Raylib.ClearBackground(Color.White);

        var previewStatus = drizzleRenderer!.PreviewImages!;
        var renderStage = previewStatus.Stage;

        if (renderStage != RenderPreviewStage.Setup)
        {
            var shader = layerPreviewShader;
            if (renderStage == RenderPreviewStage.Lights)
            {
                shader = layerPreviewLightShader;
                Raylib.ClearBackground(Color.Black);
            }

            Raylib.BeginShaderMode(shader);

            for (int i = 29; i >= 0; i--)
            {
                var tex = previewLayers![i];

                if (tex is not null)
                {
                    var ox = (previewComposite.Raw.Texture.Width - tex.Raw.Width) / 2f;
                    var oy = (previewComposite.Raw.Texture.Height - tex.Raw.Height) / 2f;

                    float fadeValue = i / 30f;
                    Raylib.DrawTextureV(tex, new Vector2(ox - i, oy - i), new Color((int)(fadeValue * 255f), 0, 0, 255));
                }
            }

            if (previewBlackout1 is not null && previewStatus.RenderBlackOut1)
            {
                var ox = (previewComposite.Raw.Texture.Width - previewBlackout1.Raw.Width) / 2f;
                var oy = (previewComposite.Raw.Texture.Height - previewBlackout1.Raw.Height) / 2f;
                Raylib.DrawTexture(previewBlackout1, (int)ox, (int)oy, new Color(0, 0, 0, 255));
            }

            if (previewBlackout2 is not null && previewStatus.RenderBlackOut2)
            {
                var ox = (previewComposite.Raw.Texture.Width - previewBlackout2.Raw.Width) / 2f;
                var oy = (previewComposite.Raw.Texture.Height - previewBlackout2.Raw.Height) / 2f;
                Raylib.DrawTexture(previewBlackout2, (int)ox, (int)oy, new Color(0, 0, 0, 255));
            }

            Raylib.EndShaderMode();
        }

        Raylib.EndTextureMode();
    }
}