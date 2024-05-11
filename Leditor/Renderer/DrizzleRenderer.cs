using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Drizzle.Lingo.Runtime;
using Drizzle.Logic;
using Drizzle.Logic.Rendering;
using Drizzle.Ported;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Leditor.Renderer;

#nullable enable

enum RenderPreviewStage 
{
    Setup,
    Props,
    Effects,
    Lights,
}

/// <summary>
/// The collection of render preview images
/// </summary>
class RenderPreviewImages : IDisposable
{
    public RL.Managed.Image[] Layers;
    
    // this is used for the effects stage
    public RL.Managed.Image? BlackOut1;
    public RL.Managed.Image? BlackOut2;

    public bool RenderBlackOut1;
    public bool RenderBlackOut2;

    public RenderPreviewStage Stage;

    private int lastWidth = -1;
    private int lastHeight = -1;
    private PixelFormat oldPixelFormat;

    public RenderPreviewImages()
    {
        Stage = RenderPreviewStage.Setup;
        Layers = new RL.Managed.Image[30];
        oldPixelFormat = PixelFormat.UncompressedR8G8B8A8;
    }

    public void SetSize(int newWidth, int newHeight, PixelFormat format)
    {
        if (lastWidth == newWidth && lastHeight == newHeight)
        {
            if (format != oldPixelFormat)
            {
                for (var i = 0; i < 30; i++)
                {
                    Layers[i]?.Format(format);
                }
            }

            oldPixelFormat = format;
            return;
        }

        lastWidth = newWidth;
        lastHeight = newHeight;

        for (var i = 0; i < 30; i++)
        {
            Layers[i]?.Dispose();
            Layers[i] = new RL.Managed.Image(newWidth, newHeight, Raylib_cs.Color.Black);
            Layers[i].Format(format);
        }
        
        if (BlackOut1 is not null)
        {
            BlackOut1.Dispose();
            BlackOut1 = new RL.Managed.Image(newWidth, newHeight, Raylib_cs.Color.Black);
            //BlackOut1.Format(PixelFormat.UncompressedGrayscale);
        }

        if (BlackOut2 is not null)
        {
            BlackOut2.Dispose();
            BlackOut2 = new RL.Managed.Image(newWidth, newHeight, Raylib_cs.Color.Black);
            //BlackOut2.Format(PixelFormat.UncompressedGrayscale);
        }
    }

    public void EnableBlackOut()
    {
        BlackOut1 ??= new RL.Managed.Image(lastWidth, lastHeight, Raylib_cs.Color.Black);
        BlackOut2 ??= new RL.Managed.Image(lastWidth, lastHeight, Raylib_cs.Color.Black);

        if (BlackOut1.Raw.Format != PixelFormat.UncompressedGrayscale)
        {
            //BlackOut1.Format(PixelFormat.UncompressedGrayscale);
        }

        if (BlackOut2.Raw.Format != PixelFormat.UncompressedGrayscale)
        {
            //BlackOut2.Format(PixelFormat.UncompressedGrayscale);
        }
    }

    public void DisableBlackOut()
    {
        BlackOut1?.Dispose();
        BlackOut2?.Dispose();

        BlackOut1 = null;
        BlackOut2 = null;
    }

    public void Dispose()
    {
        for (var i = 0; i < 30; i++)
        {
            Layers[i]?.Dispose();
        }

        BlackOut1?.Dispose();
        BlackOut2?.Dispose();
    }
}

class DrizzleRender : IDisposable
{
    private abstract record ThreadMessage;

    private record MessageRenderStarted : ThreadMessage;
    private record MessageLevelLoading : ThreadMessage;
    private record MessageRenderFailed(Exception Exception) : ThreadMessage;
    private record MessageRenderCancelled : ThreadMessage;
    private record MessageRenderFinished : ThreadMessage;
    private record MessageRenderProgress(float Percentage) : ThreadMessage;
    private record MessageDoCancel : ThreadMessage;
    private record MessageReceievePreview(RenderPreview Preview) : ThreadMessage;

    private static LingoRuntime? staticRuntime = null; 

    private class RenderThread
    {
        public ConcurrentQueue<ThreadMessage> Queue;
        public ConcurrentQueue<ThreadMessage> InQueue;

        public string filePath;
        public LevelRenderer? Renderer;
        public Action<RenderStatus>? StatusChanged = null;

        public RenderThread(string filePath)
        {
            Queue = new ConcurrentQueue<ThreadMessage>();
            InQueue = new ConcurrentQueue<ThreadMessage>();
            this.filePath = filePath;
        }

        public void ThreadProc()
        {
            try
            {
                if (GLOBALS.Settings.GeneralSettings.CacheRendererRuntime)
                {
                    if (!GLOBALS.LingoRuntimeInitTask.IsCompleted) GLOBALS.LingoRuntimeInitTask.Wait();
                    
                    var runtime = GLOBALS.LingoRuntime.Clone();
                    EditorRuntimeHelpers.RunLoadLevel(runtime, filePath);
                    Renderer = new LevelRenderer(runtime, null);
                }
                else
                {
                    var runtime = new LingoRuntime(typeof(MovieScript).Assembly);
                    runtime.Init();
                    EditorRuntimeHelpers.RunStartup(runtime);
                    EditorRuntimeHelpers.RunLoadLevel(runtime, filePath);
                    Renderer = new LevelRenderer(runtime, null);
                }

                // process user cancel if cancelled while init
                // zygote runtime
                if (InQueue.TryDequeue(out ThreadMessage? msg))
                {
                    if (msg is MessageDoCancel)
                        throw new RenderCancelledException();
                }

                Queue.Enqueue(new MessageLevelLoading());
                // RainEd.Logger.Information("RENDER: Loading {LevelName}", Path.GetFileNameWithoutExtension(filePath));

                Renderer.StatusChanged += StatusChanged;
                Renderer.PreviewSnapshot += PreviewSnapshot;
                
                // process user cancel if cancelled while init
                // zygote runtime
                if (InQueue.TryDequeue(out msg))
                {
                    if (msg is MessageDoCancel)
                        throw new RenderCancelledException();
                }
                Queue.Enqueue(new MessageRenderStarted());

                // RainEd.Logger.Information("RENDER: Begin");
                Renderer.DoRender();
                
                // Move rendered level to /levels
                try
                {
                    if (!Directory.Exists(GLOBALS.Paths.LevelsDirectory))
                        Directory.CreateDirectory(GLOBALS.Paths.LevelsDirectory);

                    var files = Directory.GetFiles(Path.Combine(GLOBALS.Paths.RendererDirectory, "Levels"));

                    foreach (var file in files)
                    {
                        var name = Path.GetFileName(file);

                        if (name.StartsWith(GLOBALS.Level.ProjectName))
                        {
                            File.Move(file, Path.Combine(GLOBALS.Paths.LevelsDirectory, name), true);
                        }
                    }
                }
                catch (Exception e)
                {
                    // TODO: Report the exception
                    Console.WriteLine(e);
                }
                
                // RainEd.Logger.Information("Render successful!");
                Queue.Enqueue(new MessageRenderFinished());
            }
            catch (RenderCancelledException)
            {
                // RainEd.Logger.Information("Render was cancelled");
                Queue.Enqueue(new MessageRenderCancelled());
            }
            catch (Exception e)
            {
                Queue.Enqueue(new MessageRenderFailed(e));
            }
        }

        private void PreviewSnapshot(RenderPreview renderPreview)
        {
            Queue.Enqueue(new MessageReceievePreview(renderPreview));
        }
    }

    private readonly RenderThread threadState;
    private readonly Thread thread;

    public enum RenderState
    {
        Initializing,
        Loading,
        Rendering,
        Finished,
        Cancelling,
        Canceled,
        Errored
    };

    private RenderState state;
    private RenderStage currentStage;
    private float progress = 0.0f;
    public RenderState State { get => state; }
    public RenderStage Stage { get => currentStage; }
    public float RenderProgress { get => progress; }
    public bool IsDone { get => state == RenderState.Canceled || state == RenderState.Finished; }

    private readonly int cameraCount;
    private int camsDone = 0;

    public string DisplayString = string.Empty;
    public int CameraCount { get => cameraCount; }
    public int CamerasDone { get => camsDone; }

    public RenderPreviewImages? PreviewImages;
    public Action? PreviewUpdated;

    public DrizzleRender()
    {
        cameraCount = GLOBALS.Level.Cameras.Count;

        state = RenderState.Initializing;

        var filePath = Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt");
        
        if (string.IsNullOrEmpty(filePath)) throw new Exception("Render called but level wasn't saved");
        
        // create render layer preview images
        PreviewImages = new RenderPreviewImages();

        threadState = new RenderThread(filePath);
        threadState.StatusChanged += StatusChanged;
        Configuration.Default.PreferContiguousImageBuffers = true;
        thread = new Thread(new ThreadStart(threadState.ThreadProc))
        {
            CurrentCulture = Thread.CurrentThread.CurrentCulture
        };
        thread.Start();
    }

    public void Dispose()
    {
        PreviewImages?.Dispose();
    }

    private void StatusChanged(RenderStatus status)
    {
        // RainEd.Logger.Debug("Status changed");

        var stageEnum = status.Stage.Stage;

        camsDone = status.CountCamerasDone;

        // from 0 to 1
        float stageProgress = 0f;

        switch (status.Stage)
        {
            case RenderStageStatusLayers layers:
            {
                stageProgress = (3 - layers.CurrentLayer) / 3f;
                DisplayString = $"Rendering tiles...\nLayer: {layers.CurrentLayer}";
                break;
            }

            case RenderStageStatusProps:
            {
                DisplayString = "Rendering props...";
                break;
            }

            case RenderStageStatusLight light:
            {
                stageProgress = light.CurrentLayer / 30f;
                DisplayString = $"Rendering light...\nLayer: {light.CurrentLayer}";
                break;
            }

            case RenderStageStatusRenderColors:
            {
                DisplayString = "Rendering colors...";
                break;
            }

            case RenderStageStatusFinalize:
            {
                DisplayString = "Finalizing...";
                break;
            }

            case RenderStageStatusEffects effects:
            {
                var builder = new StringBuilder();
                builder.Append("Rendering effects...\n");
                
                for (int i = 0; i < effects.EffectNames.Count; i++)
                {
                    if (i == effects.CurrentEffect - 1)
                        builder.Append("> ");
                    else
                        builder.Append("  ");

                    builder.Append(effects.EffectNames[i]);
                    builder.Append('\n');
                }

                stageProgress = Math.Clamp((effects.CurrentEffect - 1f) / effects.EffectNames.Count, 0f, 1f);

                DisplayString = builder.ToString();
                break;
            }
        }

        // send progress
        currentStage = stageEnum;
        float renderProgress = status.CountCamerasDone * 10 + stageEnum switch
        {
            RenderStage.Start => 0f,
            RenderStage.CameraSetup => 0f,
            RenderStage.RenderLayers => 1f,
            RenderStage.RenderPropsPreEffects => 2f,
            RenderStage.RenderEffects => 3f,
            RenderStage.RenderPropsPostEffects => 4f,
            RenderStage.RenderLight => 5f,
            RenderStage.Finalize => 6f,
            RenderStage.RenderColors => 7f,
            RenderStage.Finished => 8f,
            RenderStage.SaveFile => 9f,
            _ => throw new ArgumentOutOfRangeException()
        };

        progress = (renderProgress + Math.Clamp(stageProgress, 0f, 1f)) / (cameraCount * 10f);

        if (stageEnum == RenderStage.Start && PreviewImages is not null)
        {
            PreviewImages.Stage = RenderPreviewStage.Setup;
        }
    }

    public void Cancel()
    {
        state = RenderState.Cancelling;

        if (threadState.Renderer is not null)
        {
            threadState.Renderer.CancelRender();
        }
        else
        {
            threadState.InQueue.Enqueue(new MessageDoCancel());
        }
    }

    public void Update()
    {
        while (threadState.Queue.TryDequeue(out ThreadMessage? messageGeneral))
        {
            if (messageGeneral is null) continue;

            switch (messageGeneral)
            {
                case MessageRenderProgress msgProgress:
                    progress = msgProgress.Percentage;
                    break;
                
                case MessageRenderFinished:
                    state = RenderState.Finished;
                    // RainEd.Logger.Debug("Close render thread");
                    progress = 1f;
                    DisplayString = "";
                    thread.Join();
                    break;

                case MessageLevelLoading:
                    state = RenderState.Loading;
                    break;
                
                case MessageRenderStarted:
                    state = RenderState.Rendering;
                    break;
                
                case MessageRenderFailed msgFail:
                Console.WriteLine(msgFail);
                    // RainEd.Logger.Error("Error occured when rendering level:\n{ErrorMessage}", msgFail.Exception);
                    thread.Join();
                    state = RenderState.Errored;
                    break;
                
                case MessageRenderCancelled:
                    state = RenderState.Canceled;
                    thread.Join();
                    break;
                
                case MessageReceievePreview preview:
                    ProcessPreview(preview.Preview);
                    break;
            }
            
            threadState.Renderer?.RequestPreview();
        }
    }

    private RenderPreview? lastRenderPreview;

    private void ProcessPreview(RenderPreview renderPreview)
    {
        if (PreviewImages is null) return;

        // RainEd.Logger.Verbose("Receive preview");
        
        lastRenderPreview = renderPreview;
        PreviewUpdated?.Invoke();
    }

    public void UpdatePreviewImages()
    {
        if (PreviewImages is null) return;

        switch (lastRenderPreview)
        {
            case RenderPreviewProps props:
                PreviewImages.Stage = RenderPreviewStage.Props;
                PreviewImages.SetSize(2000, 1200, PixelFormat.UncompressedR8G8B8A8);
                PreviewImages.DisableBlackOut();

                for (var i = 0; i < 30; i++)
                {
                    CopyLingoImage(props.Layers[i], PreviewImages.Layers[i]);
                }

                break;
            
            case RenderPreviewEffects effects:
            {
                PreviewImages.Stage = RenderPreviewStage.Effects;
                PreviewImages.SetSize(2000, 1200, PixelFormat.UncompressedR8G8B8A8);
                PreviewImages.EnableBlackOut();

                for (var i = 0; i < 30; i++)
                {
                    CopyLingoImage(effects.Layers[i], PreviewImages.Layers[i]);
                }

                CopyLingoImage(effects.BlackOut1, PreviewImages.BlackOut1!);
                CopyLingoImage(effects.BlackOut2, PreviewImages.BlackOut2!);

                PreviewImages.RenderBlackOut1 = effects.BlackOut1.Width != 1;
                PreviewImages.RenderBlackOut2 = effects.BlackOut2.Width != 1;
                
                break;
            }

            case RenderPreviewLights lights:
            {
                PreviewImages.Stage = RenderPreviewStage.Lights;
                PreviewImages.SetSize(2300, 1500, PixelFormat.UncompressedGrayscale);
                PreviewImages.DisableBlackOut();

                for (var i = 0; i < 30; i++)
                {
                    CopyLingoImage(lights.Layers[i], PreviewImages.Layers[i]);
                }
                
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)] // hopefully this is a good idea...
    private unsafe static void CopyLingoImage(LingoImage srcImg, Raylib_cs.Image dstImg)
    {
        if (srcImg.Width != dstImg.Width || srcImg.Height != dstImg.Height)
        {
            // RainEd.Logger.Warning("Mismatched image sizes");
            return;
        }
        
        if (srcImg.Depth == 1)
        {
            if (dstImg.Format != PixelFormat.UncompressedGrayscale)
                throw new Exception("Mismatched image formats");

            // convert 1 bit per pixel bitmap to
            // 8 bits per pixel grayscale image
            byte* dstData = (byte*) dstImg.Data;
            
            int k = 0;
            for (int i = 0; i < (dstImg.Width * dstImg.Height) / 8; i++)
            {
                var v = srcImg.ImageBuffer[i];
                dstData[k++] = (byte)(255 * (v & 1));
                dstData[k++] = (byte)(255 * ((v >> 1) & 1));
                dstData[k++] = (byte)(255 * ((v >> 2) & 1));
                dstData[k++] = (byte)(255 * ((v >> 3) & 1));
                dstData[k++] = (byte)(255 * ((v >> 4) & 1));
                dstData[k++] = (byte)(255 * ((v >> 5) & 1));
                dstData[k++] = (byte)(255 * ((v >> 6) & 1));
                dstData[k++] = (byte)(255 * ((v >> 7) & 1));
            } 
        }
        else if (srcImg.Depth == 32)
        {
            if (dstImg.Format != PixelFormat.UncompressedR8G8B8A8)
                throw new Exception("Mismatched image formats");
            
            // note: BGR format - is converted to RGB in the shader
            Marshal.Copy(srcImg.ImageBuffer, 0, (nint)dstImg.Data, dstImg.Width * dstImg.Height * 4);
        }
        else
        {
            throw new Exception("Unknown image depth " + srcImg.Depth);
        }
    }
}