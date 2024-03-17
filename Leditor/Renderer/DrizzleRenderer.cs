using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Drizzle.Lingo.Runtime;
using Drizzle.Logic;
using Drizzle.Logic.Rendering;
using Drizzle.Ported;
using SixLabors.ImageSharp;
using Image = Raylib_cs.Image;

namespace Leditor.Renderer;

internal class DrizzleRender : IDisposable
{
    private abstract record ThreadMessage;

    private record MessageRenderStarted : ThreadMessage;
    private record MessageRenderFailed(Exception Exception) : ThreadMessage;
    private record MessageRenderCancelled : ThreadMessage;
    private record MessageRenderFinished : ThreadMessage;
    private record MessageRenderProgress(float Percentage) : ThreadMessage;
    private record MessageDoCancel : ThreadMessage;
    private record MessageReceivePreview(RenderPreview Preview) : ThreadMessage;

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
                    EditorRuntimeHelpers.RunLoadLevel(GLOBALS.LingoRuntime, filePath);
                    Renderer = new LevelRenderer(GLOBALS.LingoRuntime, null);
                }
                else
                {
                    var runtime = new LingoRuntime(typeof(MovieScript).Assembly);
                    runtime.Init();
                    EditorRuntimeHelpers.RunStartup(runtime);
                    EditorRuntimeHelpers.RunLoadLevel(runtime, filePath);
                    Renderer = new LevelRenderer(runtime, null);
                }

                Renderer.StatusChanged += StatusChanged;
                Renderer.PreviewSnapshot += PreviewSnapshot;
                Queue.Enqueue(new MessageRenderStarted());

                // process user cancel if cancelled while init
                // zygote runtime
                if (InQueue.TryDequeue(out ThreadMessage? msg))
                {
                    if (msg is MessageDoCancel)
                        throw new RenderCancelledException();
                }

                Renderer.DoRender();
                
                //
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
                
                
                //
                
                Queue.Enqueue(new MessageRenderFinished());
            }
            catch (RenderCancelledException)
            {
                Queue.Enqueue(new MessageRenderCancelled());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Queue.Enqueue(new MessageRenderFailed(e));
            }
        }

        private void PreviewSnapshot(RenderPreview renderPreview)
        {
            Queue.Enqueue(new MessageReceivePreview(renderPreview));
        }
    }

    private readonly RenderThread threadState;
    private readonly Thread thread;

    public enum RenderState
    {
        Initializing,
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

    public readonly Image[] RenderLayerPreviews;
    public Action? PreviewUpdated;

    public DrizzleRender()
    {
        cameraCount = GLOBALS.Level.Cameras.Count;

        state = RenderState.Initializing;
        var filePath = Path.Combine(GLOBALS.ProjectPath, GLOBALS.Level.ProjectName+".txt");
        if (string.IsNullOrEmpty(filePath)) throw new Exception("Render called but level wasn't saved");

        // create render layer preview images
        var renderW = 2000;
        var renderH = 1200;
        RenderLayerPreviews = new Image[30];

        for (int i = 0; i < 30; i++)
        {
            RenderLayerPreviews[i] = Raylib.GenImageColor((int)renderW, (int)renderH, Raylib_cs.Color.Black);
        } 

        //
        // TODO: Save level here
        //

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
        foreach (var image in RenderLayerPreviews)
            Raylib.UnloadImage(image);
    }

    private void StatusChanged(RenderStatus status)
    {
        var renderer = threadState.Renderer!;

        var camIndex = status.CameraIndex;
        var stageEnum = status.Stage.Stage;

        camsDone = status.CountCamerasDone;

        switch (status.Stage)
        {
            case RenderStageStatusLayers layers:
            {
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
                    if (i == effects.CurrentEffect)
                        builder.Append("> ");
                    else
                        builder.Append("  ");

                    builder.Append(effects.EffectNames[i]);
                    builder.Append('\n');
                }

                DisplayString = builder.ToString();
                break;
            }
        }

        // send progress
        currentStage = stageEnum;
        var renderProgress = status.CountCamerasDone * 10 + stageEnum switch
        {
            RenderStage.Start => 0,
            RenderStage.CameraSetup => 0,
            RenderStage.RenderLayers => 1,
            RenderStage.RenderPropsPreEffects => 2,
            RenderStage.RenderEffects => 3,
            RenderStage.RenderPropsPostEffects => 4,
            RenderStage.RenderLight => 5,
            RenderStage.Finalize => 6,
            RenderStage.RenderColors => 7,
            RenderStage.Finished => 8,
            RenderStage.SaveFile => 9,
            _ => throw new ArgumentOutOfRangeException()
        };

        progress = renderProgress / (cameraCount * 10f);
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
                    progress = 1f;
                    DisplayString = "";
                    thread.Join();
                    break;
                
                case MessageRenderStarted:
                    state = RenderState.Rendering;
                    break;
                
                case MessageRenderFailed msgFail:
                    thread.Join();
                    state = RenderState.Errored;
                    break;
                
                case MessageRenderCancelled:
                    state = RenderState.Canceled;
                    thread.Join();
                    break;
                
                case MessageReceivePreview preview:
                    ProcessPreview(preview.Preview);
                    break;
            }
            
            threadState.Renderer?.RequestPreview();
        }
    }

    private void ProcessPreview(RenderPreview renderPreview)
    {
        switch (renderPreview)
        {
            case RenderPreviewEffects effects:
            {
                ProcessLingoImageLayers(effects.Layers);
                break;
            }

            case RenderPreviewLights lights:
            {
                // TODO: light stage uses a differently sized image buffer
                // ProcessLingoLightImageLayers(lights.Layers);
                break;
            }

            case RenderPreviewProps props:
            {
                ProcessLingoImageLayers(props.Layers);
            }
                break;
        }

        PreviewUpdated?.Invoke();
    }

    private void ProcessLingoImageLayers(LingoImage[] layers)
    {
        // Console.WriteLine($"Source: {layers[0].Width} * {layers[0].Height} * {layers[0].Depth}");
        // Console.WriteLine($"Dest: {RenderLayerPreviews[0].Width} * {RenderLayerPreviews[0].Height} * 4");
        
        // Lingo Image:
        // 2000, 1200
        // Output:
        // 1400, 800
        if (layers.Length != 30)
            throw new Exception("Count of layers is not 30");
        
        for (var i = 0; i < layers.Length; i++)
        {
            var img = layers[i];
            var dstImage = RenderLayerPreviews[i];

            unsafe
            {
                Marshal.Copy(img.ImageBuffer, 0, (nint) dstImage.Data, dstImage.Width * dstImage.Height * 4);
            }
        }
    }
    
    private void ProcessLingoLightImageLayers(LingoImage[] layers)
    {
        // Console.WriteLine($"Source: {layers[0].Width} * {layers[0].Height} * {layers[0].Depth}");
        // Console.WriteLine($"Dest: {RenderLayerPreviews[0].Width} * {RenderLayerPreviews[0].Height} * 4");
        
        // Lingo Image:
        // 2300, 1500
        // Output:
        // 1400, 800
        if (layers.Length != 30)
            throw new Exception("Count of layers is not 30");
        
        for (var i = 0; i < layers.Length; i++)
        {
            var img = layers[i];
            var dstImage = RenderLayerPreviews[i];

            unsafe
            {
                Marshal.Copy(
                    img.ImageBuffer, 
                    0, 
                    (nint) dstImage.Data, 
                    dstImage.Width * dstImage.Height);
            }
        }
    }
}