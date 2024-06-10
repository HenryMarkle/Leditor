using System.Numerics;
using ImGuiNET;
using Leditor.Data.Tiles;
using static Raylib_cs.Raylib;

namespace Leditor.Pages;

#nullable enable

internal class L4MakerPage : EditorPage, IContextListener {
    public override void Dispose()
    {
        if (Disposed) return;

        Disposed = true;

        _tileShader.Dispose();
        _boxTileShader.Dispose();
        _whiteEraser.Dispose();
        _variedStandardPropShader.Dispose();
        _standardPropShader.Dispose();
        _variedSoftPropShader.Dispose();
        _softPropShader.Dispose();
    }

    public void OnProjectCreated(object? sender, EventArgs e)
    {
        ResetBuffers();
    }

    public void OnProjectLoaded(object? sender, EventArgs e)
    {
        ResetBuffers();
    }

    public void OnPageUpdated(int previous, int next)
    {
        if (next == 10) {
            _shouldRedrawLevel = true;
            ResetQuadHandles();
        }
    }
    
    /// Requires a gl context
    internal L4MakerPage() {
        ResetQuadHandles();
        
        //

        _layer1Buffer = new(0, 0);
        _layer2Buffer = new(0, 0);
        _layer3Buffer = new(0, 0);
        _levelBuffer =  new(0, 0);

        //

        _boxTileShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform vec2 offset;
uniform float height;
uniform float width;
uniform int depth; // 0 - 29

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = texture(inputTexture, vec2(fragTexCoord.x * width, fragTexCoord.y * height + offset.y));

	if (newColor.r == 1.0 && newColor.g == 1.0 && newColor.b == 1.0) discard;

	FragColor = fragColor;
}"));

        _tileShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform int layerNum;
uniform float layerHeight;
uniform float layerWidth;
uniform vec4 tint;
uniform int depth; // 0 - 29
uniform int flatShading; // 0 or 1

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = vec4(0);
	float totalWidth = fragTexCoord.x * layerWidth;

	for (int l = layerNum - 1; l > -1; l--) {
		float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);
		
		vec2 newFragTexCoord = vec2(totalWidth, currentHeight);
	
		vec4 c = texture(inputTexture, newFragTexCoord);
		if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;

        float shade = 1.0;

        if (flatShading == 0) {
            shade -= ((depth + l) / 30.0);

            newColor = vec4(fragColor.r * shade, fragColor.g * shade, fragColor.b * shade, fragColor.a);
        } else {
            newColor = fragColor;
        }
	}

    if (newColor.a == 0.0) { discard; }

	FragColor = newColor;
}"));   
    
        _whiteEraser = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform int flipV; // 0 - 1

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
	vec4 newColor = vec4(0,0,0,0);

    if (flipV == 0) {
        newColor = texture(inputTexture, fragTexCoord);
    } else {
        newColor = texture(inputTexture, vec2(fragTexCoord.x, 1.0 - fragTexCoord.y));
    }

    if ((newColor.r == 0.0 && newColor.g == 0.0 && newColor.b == 0.0) || newColor.a == 0.0) {
        discard;
    }

	FragColor = newColor;
}"));
    
        _variedStandardPropShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform int layerNum;
uniform float layerHeight;
uniform float varWidth;
uniform int variation;
uniform int depth;
uniform in flatShading;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 newColor = vec4(0);
    float newXCoord = fragTexCoord.x * varWidth + (variation * varWidth);

    for (int l = layerNum - 1; l > -1; l--) {
        float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);

        vec2 newFragTexCoord = vec2(newXCoord, currentHeight);

        vec4 c = texture(inputTexture, newFragTexCoord);
        
        if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;

        float shade = 1.0;

        if (flatShading == 0) {
            shade -= ((depth + l) / 30.0);

            newColor = vec4(fragColor.r * shade, fragColor.g * shade, fragColor.b * shade, fragColor.a);
        } else {
            newColor = fragColor;
        }
    }

    FragColor = newColor;
}"));
    
        _standardPropShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform int layerNum;
uniform float layerHeight;
uniform float width;
uniform int depth;
uniform int flatShading;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 newColor = vec4(0);

    for (int l = layerNum - 1; l > -1; l--) {
        float currentHeight = fragTexCoord.y * layerHeight + (l * layerHeight);

        vec2 newFragTexCoord = vec2(fragTexCoord.x * width, currentHeight);

        vec4 c = texture(inputTexture, newFragTexCoord);

        if (c.r == 1.0 && c.g == 1.0 && c.b == 1.0) continue;
    
        float shade = 1.0;

        if (flatShading == 0) {
            shade -= ((depth + l) / 30.0);

            newColor = vec4(fragColor.r * shade, fragColor.g * shade, fragColor.b * shade, fragColor.a); 
        } else {
            newColor = fragColor;
        }
    }

if (newColor.a == 0.0) discard;

    FragColor = newColor;
}"));
    
        _variedSoftPropShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform float varWidth;
uniform float height;
uniform int variation;
uniform int depth;
uniform int flatShading;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    float newXCoord = fragTexCoord.x * varWidth + (variation * varWidth);
    float newYCoord = fragTexCoord.y * height;
    
    vec4 c = texture(inputTexture, vec2(newXCoord, newYCoord));

    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || (c.r == 0.0 && c.g == 0.0 && c.b == 0.0)) {
        discard;
    }

    float shade = 1.0;

    vec4 newColor = vec4(0, 0, 0, 0);

    if (flatShading == 0) {
        shade = c.g - depth/30.0;

        if (shade < 0.1) shade = 0.01;

        newColor = vec4(fragColor.r * shade, fragColor.g * shade, fragColor.b * shade, fragColor.a);
    } else {
        newColor = fragColor;
    }

    FragColor = newColor;
}"));
    
        _softPropShader = new(LoadShaderFromMemory(null, @"#version 330

uniform sampler2D inputTexture;
uniform int depth;
uniform int flatShading;

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 FragColor;

void main() {
    vec4 c = texture(inputTexture, fragTexCoord);
    
    if ((c.r == 1.0 && c.g == 1.0 && c.b == 1.0) || (c.r == 0.0 && c.g == 0.0 && c.b == 0.0)) {
        discard;
    }

    float shade = 1.0;

    vec4 newColor = vec4(0, 0, 0, 0);

    if (flatShading == 0) {
        shade = c.g - depth/30.0;

        if (shade < 0.1) shade = 0.01;

        newColor = vec4(fragColor.r * shade, fragColor.g * shade, fragColor.b * shade, fragColor.a);
    } else {
        newColor = fragColor;
    }

    FragColor = vec4(shade, shade, shade, fragColor.a);
}"));
    }

    ~L4MakerPage() {
        if (!Disposed) throw new InvalidOperationException($"{nameof(L4MakerPage)} was not disposed by the consumer.");
    }
    
    #region Fields

    private RL.Managed.RenderTexture2D _layer1Buffer;
    private RL.Managed.RenderTexture2D _layer2Buffer;
    private RL.Managed.RenderTexture2D _layer3Buffer;

    private RL.Managed.RenderTexture2D _levelBuffer;

    private RL.Managed.Shader _whiteEraser;

    private RL.Managed.Shader _tileShader;
    private RL.Managed.Shader _boxTileShader;
    private RL.Managed.Shader _standardPropShader;
    private RL.Managed.Shader _variedStandardPropShader;
    private RL.Managed.Shader _variedSoftPropShader;
    private RL.Managed.Shader _softPropShader;
    private RL.Managed.Shader? _propShader;


    private Camera2D _camera = new() { Zoom = 1.0f };

    private const int QuadHandleRadius = 10;

    private Vector2 _l1TopLeftQuadHandle = new();
    private Vector2 _l1TopRightQuadHandle = new();
    private Vector2 _l1BottomRightQuadHandle = new();
    private Vector2 _l1BottomLeftQuadHandle = new();

    private Vector2 _l2TopLeftQuadHandle = new();
    private Vector2 _l2TopRightQuadHandle = new();
    private Vector2 _l2BottomRightQuadHandle = new();
    private Vector2 _l2BottomLeftQuadHandle = new();

    private Vector2 _l3TopLeftQuadHandle = new();
    private Vector2 _l3TopRightQuadHandle = new();
    private Vector2 _l3BottomRightQuadHandle = new();
    private Vector2 _l3BottomLeftQuadHandle = new();

    private bool _cameras;
    private bool _camerasInnerBounds;

    private bool _layer1 = true;
    private bool _layer2 = true;
    private bool _layer3 = true;

    private bool _genWholeLevel;
    private bool _genEachCamera = true;

    private bool _flatShading = true;

    /// 1, 2, 3, 4 => layer 1
    /// 5, 6, 7, 8 => layer 2
    /// 9, 10, 11, 12 => layer 3
    private int _quadLock;

    private bool _clickLock;


    private bool _shouldRedrawLevel;
    private bool _generateSignal;


    private bool _isNavbarHovered;
    private bool _isOptionsWinHovered;

    #endregion

    #region Methods

    private void ResetQuadHandles() {
        var width = GLOBALS.Level.Width * 20;
        var height = GLOBALS.Level.Height * 20;

        _l1TopLeftQuadHandle = new(-10, -10);
        _l1TopRightQuadHandle = new(width + 10, -10);
        _l1BottomRightQuadHandle = new(width + 10, height + 10);
        _l1BottomLeftQuadHandle = new(-10, height + 10);
        
        _l2TopLeftQuadHandle = new(-30, -30);
        _l2TopRightQuadHandle = new(width + 30, -30);
        _l2BottomRightQuadHandle = new(width + 30, height + 30);
        _l2BottomLeftQuadHandle = new(-30, height + 30);
        
        _l3TopLeftQuadHandle = new(-50, -50);
        _l3TopRightQuadHandle = new(width + 50, -50);
        _l3BottomRightQuadHandle = new(width + 50, height + 50);
        _l3BottomLeftQuadHandle = new(-50, height + 50);
    }

    private void ResetBuffers() {
        _layer1Buffer.Dispose();
        _layer2Buffer.Dispose();
        _layer3Buffer.Dispose();
        _levelBuffer.Dispose();

        var width = GLOBALS.Level.Width * 20;
        var height = GLOBALS.Level.Height * 20;

        _layer1Buffer = new(width, height);
        _layer2Buffer = new(width, height);
        _layer3Buffer = new(width, height);
        _levelBuffer = new(width, height);
    }

    // private byte LayerToShadeByte(int layer) => (byte)(255 - layer/3 * 255);
    private int LayerToShadeInt(int layer) {
        var shade = 255 - layer*255/3;

        Utils.Restrict(ref shade, 10, 250);

        return shade;
    }
    // private float LayerToShadeFloat(int layer) => 1.0f - layer/3f;

    private void DrawTileAsProp(
        in TileDefinition init, 
        in PropQuad quad,
        in Color color,
        int depth,
        bool flat
    )
    {
        var scale = GLOBALS.Scale;
        var texture = init.Texture;

        if (init.Type == Data.Tiles.TileType.Box)
        {
            if (_boxTileShader is null) return;

            var shader = _boxTileShader.Raw;

            var (tWidth, tHeight) = init.Size;
            var bufferPixels = init.BufferTiles * 20;
            
            var height = tHeight * 20;
            var offset = new Vector2(bufferPixels, tHeight * tWidth * 20);
            
            var calcHeight = (float)(height + bufferPixels*2) / (float)texture.Height;
            var calcOffset = Raymath.Vector2Divide(offset, new(texture.Width, texture.Height));
            var calcWidth = (float)(tWidth + init.BufferTiles*2)*20 / texture.Width;
            
            var textureLoc = GetShaderLocation(shader, "inputTexture");

            var widthLoc = GetShaderLocation(shader, "width");
            var heightLoc = GetShaderLocation(shader, "height");
            var offsetLoc = GetShaderLocation(shader, "offset");
            var depthLoc = GetShaderLocation(shader, "depth");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            
            SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
            
            SetShaderValue(shader, heightLoc, calcHeight,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, offsetLoc, calcOffset,
                ShaderUniformDataType.Vec2);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);

            Printers.DrawTextureQuad(texture, quad, color);
            EndShaderMode();
        }
        else
        {
            if (_tileShader is null) return;

            var shader = _tileShader;

            var layerHeight = (init.Size.Item2 + (init.BufferTiles * 2)) * scale;
            float calLayerHeight = (float)layerHeight / (float)texture.Height;
            var textureCutWidth = (init.Size.Item1 + (init.BufferTiles * 2)) * scale;
            float calTextureCutWidth = (float)textureCutWidth / (float)texture.Width;

            var textureLoc = GetShaderLocation(shader, "inputTexture");
            var layerNumLoc = GetShaderLocation(shader, "layerNum");
            var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
            var layerWidthLoc = GetShaderLocation(shader, "layerWidth");
            var depthLoc = GetShaderLocation(shader, "depth");
            var alphaLoc = GetShaderLocation(shader, "alpha");
            var flatLoc = GetShaderLocation(shader, "flatShading");

            BeginShaderMode(shader);

            SetShaderValueTexture(shader, textureLoc, texture);
            SetShaderValue(shader, layerNumLoc, init.Type == Data.Tiles.TileType.VoxelStructRockType ? 1 : init.Repeat.Length,
                ShaderUniformDataType.Int);
            SetShaderValue(shader, layerHeightLoc, calLayerHeight,
                ShaderUniformDataType.Float);
            SetShaderValue(shader, layerWidthLoc, calTextureCutWidth,
                ShaderUniformDataType.Float);
            
            SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
            SetShaderValue(shader, alphaLoc, 1.0f, ShaderUniformDataType.Float);
            SetShaderValue(shader, flatLoc, flat ? 1 : 0, ShaderUniformDataType.Int);

            Printers.DrawTextureQuad(texture, quad, color);
            EndShaderMode();
        }
    }

    internal void DrawVariedStandardProp(
        InitVariedStandardProp init, 
        in Texture2D texture, 
        PropQuad quads,
        Color color,
        int variation,
        int depth,
        bool flat
    )
    {
        var shader = _variedStandardPropShader.Raw;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float) init.Size.y * GLOBALS.Scale;
        var variationWidth = (float) init.Size.x * GLOBALS.Scale;
        
        var calcLayerHeight = layerHeight / texture.Height;
        var calcVariationWidth = variationWidth / texture.Width;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        
        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");
        var flatLoc = GetShaderLocation(shader, "flatShading");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
       
        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(shader, flatLoc, flat ? 1 : 0, ShaderUniformDataType.Int);
        
        Printers.DrawTextureQuad(texture, quads, color, flippedX, flippedY);
        EndShaderMode();
    }

    internal void DrawStandardProp(
        InitStandardProp init, 
        in Texture2D texture, 
        PropQuad quads,
        Color color,
        int depth,
        bool flat
    )
    {
        var shader = _standardPropShader.Raw;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var layerHeight = (float)texture.Height / (float)init.Repeat.Length;
        var calcLayerHeight = layerHeight / texture.Height;
        var calcWidth = (float) init.Size.x * GLOBALS.Scale / texture.Width;

        calcWidth = calcWidth > 1.00000f ? 1.0f : calcWidth;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var layerNumLoc = GetShaderLocation(shader, "layerNum");
        var layerHeightLoc = GetShaderLocation(shader, "layerHeight");
        var widthLoc = GetShaderLocation(shader, "width");
        var depthLoc = GetShaderLocation(shader, "depth");
        var flatLoc = GetShaderLocation(shader, "flatShading");

        BeginShaderMode(shader);
        
        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValue(shader, layerNumLoc, init.Repeat.Length, ShaderUniformDataType.Int);
        SetShaderValue(shader, layerHeightLoc, calcLayerHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, widthLoc, calcWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(shader, flatLoc, flat ? 1 : 0, ShaderUniformDataType.Int);
        
        Printers.DrawTextureQuad(texture, quads, color, flippedX, flippedY);

        EndShaderMode();
    }

    internal void DrawVariedSoftProp(
        InitVariedSoftProp init, 
        in Texture2D texture, 
        PropQuad quads,
        Color color,
        int variation,
        int depth,
        bool flat
    )
    {
        var shader = _variedSoftPropShader.Raw;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var calcHeight = (float) init.SizeInPixels.y / texture.Height;
        var calcVariationWidth = (float) init.SizeInPixels.x / texture.Width;

        var textureLoc = GetShaderLocation(shader, "inputTexture");

        var heightLoc = GetShaderLocation(shader, "height");
        var variationWidthLoc = GetShaderLocation(shader, "varWidth");
        var variationLoc = GetShaderLocation(shader, "variation");
        var depthLoc = GetShaderLocation(shader, "depth");
        var flatLoc = GetShaderLocation(shader, "flatShading");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);

        SetShaderValue(shader, variationWidthLoc, calcVariationWidth, ShaderUniformDataType.Float);
        SetShaderValue(shader, heightLoc, calcHeight, ShaderUniformDataType.Float);
        SetShaderValue(shader, variationLoc, variation, ShaderUniformDataType.Int);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(shader, flatLoc, flat ? 1 : 0, ShaderUniformDataType.Int);
        
        Printers.DrawTextureQuad(texture, quads, color, flippedX, flippedY);
        EndShaderMode();
    }

    internal void DrawSoftProp(in Texture2D texture, in PropQuad quads, Color color, int depth, bool flat)
    {
        var shader = _softPropShader.Raw;

        var flippedX = quads.TopLeft.X > quads.TopRight.X && quads.BottomLeft.X > quads.BottomRight.X;
        var flippedY = quads.TopLeft.Y > quads.BottomLeft.Y && quads.TopRight.Y > quads.BottomRight.Y;
        
        var textureLoc = GetShaderLocation(shader, "inputTexture");
        var depthLoc = GetShaderLocation(shader, "depth");
        var flatLoc = GetShaderLocation(shader, "flatShading");

        BeginShaderMode(shader);

        SetShaderValueTexture(shader, textureLoc, texture);
        SetShaderValue(shader, depthLoc, depth, ShaderUniformDataType.Int);
        SetShaderValue(shader, flatLoc, flat ? 1 : 0, ShaderUniformDataType.Int);
        
        Printers.DrawTextureQuad(texture, quads, color, flippedX, flippedY);
        EndShaderMode();
    }

    private void DrawProp(InitPropType type, TileDefinition? tile, Color color, int category, int index, Prop prop, bool flat) {
        var depth = -prop.Depth + GLOBALS.Layer*10;

        var quads = prop.Quads;

        // const float ratio = 20f / 16f;

        // quads.TopLeft *= ratio;
        // quads.TopRight *= ratio;
        // quads.BottomRight *= ratio;
        // quads.BottomLeft *= ratio;

        switch (type)
        {
            case InitPropType.Tile:
            {
                if (GLOBALS.TileDex is null || tile is null) return;
                
                DrawTileAsProp(tile, quads, color, depth, flat);
            }
                break;

            case InitPropType.Rope:
            case InitPropType.Long:
            break;


            default:
            {
                var texture = GLOBALS.Textures.Props[category][index];
                var init = GLOBALS.Props[category][index];

                // TODO: Could be simplified
                switch (init)
                {
                    case InitVariedStandardProp variedStandard:
                        DrawVariedStandardProp(variedStandard, texture, quads, color, ((PropVariedSettings)prop.Extras.Settings).Variation, depth, flat);
                        break;

                    case InitStandardProp standard:
                        DrawStandardProp(standard, texture, quads, color, depth, flat);
                        break;

                    case InitVariedSoftProp variedSoft:
                        DrawVariedSoftProp(variedSoft, texture, quads, color,  ((PropVariedSoftSettings)prop.Extras.Settings).Variation, depth, flat);
                        break;

                    case InitSoftProp:
                        DrawSoftProp(texture, quads, color, depth, flat);
                        break;
                }
            }
                break;
        }
    }

    /// <summary></summary>
    /// <param name="layer">must be 0, 1, or 2</param>
    private void DrawGeoAndTileLayer(RenderTexture2D renderTexture, Color color, int layer, int scale, bool flat) {
        BeginTextureMode(renderTexture);
        ClearBackground(Color.Black);

        for (var y = 0; y < GLOBALS.Level.Height; y++)
        {
            for (var x = 0; x < GLOBALS.Level.Width; x++)
            {
                TileCell tileCell;

                #if DEBUG
                try
                {
                    tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                }
                catch (IndexOutOfRangeException ie)
                {
                    throw new IndexOutOfRangeException(innerException: ie, message: $"Failed to fetch tile cell from {nameof(GLOBALS.Level.TileMatrix)}[{GLOBALS.Level.TileMatrix.GetLength(0)}, {GLOBALS.Level.TileMatrix.GetLength(1)}, {GLOBALS.Level.TileMatrix.GetLength(2)}]: x, y, or z ({x}, {y}, {layer}) was out of bounds");
                }
                #else
                tileCell = GLOBALS.Level.TileMatrix[y, x, layer];
                #endif

                var geoCell = GLOBALS.Level.GeoMatrix[y, x, layer];

                if (geoCell.Stackables[1]) {
                    var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(1)];

                    DrawTexturePro(
                        stackableTexture, 
                        new(0, 0, stackableTexture.Width, stackableTexture.Height),
                        new(x * scale, y * scale, scale, scale), 
                        new(0, 0), 
                        0, 
                        color
                    );
                }

                if (geoCell.Stackables[2]) {
                    var stackableTexture = GLOBALS.Textures.GeoStackables[Utils.GetStackableTextureIndex(2)];

                    DrawTexturePro(
                        stackableTexture, 
                        new(0, 0, stackableTexture.Width, stackableTexture.Height),
                        new(x * scale, y * scale, scale, scale), 
                        new(0, 0), 
                        0, 
                        color
                    );
                }

                switch (tileCell.Data) {
                    case TileHead h:
                    {
                        var data = h;

                        TileDefinition? init = data.Definition;
                        var undefined = init is null;

                        var tileTexture = undefined
                            ? GLOBALS.Textures.MissingTile 
                            : data.Definition!.Texture;

                        var fcolor = Color.Purple;

                        if (GLOBALS.TileDex?.TryGetTileColor(data.Definition?.Name ?? "", out var foundColor) ?? false)
                        {
                            fcolor = foundColor;
                        }

                        if (undefined)
                        {
                            DrawTexturePro(
                                tileTexture, 
                                new Rectangle(0, 0, tileTexture.Width, tileTexture.Height),
                                new Rectangle(x*scale, y*scale, scale, scale),
                                new Vector2(0, 0),
                                0,
                                Color.White
                            );
                        }
                        else
                        {
                            var center = new Vector2(
                            init!.Size.Item1 % 2 == 0 ? x * scale + scale : x * scale + scale/2f, 
                            init!.Size.Item2 % 2 == 0 ? y * scale + scale : y * scale + scale/2f);

                            var width = (scale / 20f)/2 * (init.Type == Data.Tiles.TileType.Box ? init.Size.Width : init.Size.Width + init.BufferTiles * 2) * 20;
                            var height = (scale / 20f)/2 * ((init.Type == Data.Tiles.TileType.Box
                                ? init.Size.Item2
                                : (init.Size.Item2 + init.BufferTiles * 2)) * 20);

                            var depth2 = Utils.SpecHasDepth(init.Specs);
                            var depth3 = Utils.SpecHasDepth(init.Specs, 2);
                            
                            // DrawTileAsProp(
                            //     init,
                            //     center,
                            //     [
                            //         new(width, -height),
                            //         new(-width, -height),
                            //         new(-width, height),
                            //         new(width, height),
                            //         new(width, -height)
                            //     ],
                            //     layer * 10,
                            //     flat
                            // );

                            var quadOrigin = (new Vector2(x, y) - Vector2.One * init.BufferTiles - Utils.GetTileHeadOrigin(init))*scale;

                            var quad = new PropQuad(
                                quadOrigin,
                                quadOrigin + new Vector2(init.Size.Width + init.BufferTiles * 2,                0) * scale,
                                quadOrigin + new Vector2(init.Size.Width + init.BufferTiles * 2, init.Size.Height + init.BufferTiles * 2) * scale,
                                quadOrigin + new Vector2(0,               init.Size.Height + init.BufferTiles * 2) * scale
                            );


                            DrawTileAsProp(
                                init, 
                                quad,
                                color,
                                layer * 10,
                                flat
                            );
                        }
                    }
                    break;

                    case TileBody b:
                    {
                        var missingTexture = GLOBALS.Textures.MissingTile;
                    
                        var (hx, hy, hz) = b.HeadPosition;

                        var supposedHead = GLOBALS.Level.TileMatrix[hy - 1, hx - 1, hz - 1];

                        if (supposedHead.Data is TileHead { Definition: null } or not TileHead)
                        {
                            // DrawTexturePro(
                            //     GLOBALS.Textures.MissingTile, 
                            //     new Rectangle(0, 0, missingTexture.Width, missingTexture.Height),
                            //     new Rectangle(x*scale, y*scale, scale, scale),
                            //     new(0, 0),
                            //     0,
                            //     Color.White
                            // );
                        }
                    }
                    break;

                    default:
                    Printers.DrawTileSpec(x * scale, y * scale, geoCell.Geo, scale, color);
                    break;
                }
            }
        }
    
        EndTextureMode();
    }

    private void DrawPropLayer(RenderTexture2D renderTexture, Color color, int layer, bool flat) {
        BeginTextureMode(renderTexture);

        var scopeNear = -layer * 10;
        var scopeFar = -(layer*10 + 9);
        
        foreach (var current in GLOBALS.Level.Props)
        {
            // Filter based on depth
            if (current.prop.Depth > scopeNear || current.prop.Depth < scopeFar) continue;

            var (category, index) = current.position;
            
            DrawProp(current.type, current.tile, color, category, index, current.prop, flat);
        }

        EndTextureMode();
    }

    /// <summary>
    /// Requires a drawing context.
    /// </summary>
    private void DrawLayers() {
        // Layer 3

        if (_layer3) {
            DrawGeoAndTileLayer(_layer3Buffer, GLOBALS.Settings.L4Maker.Layer3Color, 2, 20, _flatShading);
            DrawPropLayer(_layer3Buffer, GLOBALS.Settings.L4Maker.Layer3Color, 2, _flatShading);
        }
        
        // Layer 2

        if (_layer2) {
            DrawGeoAndTileLayer(_layer2Buffer, GLOBALS.Settings.L4Maker.Layer2Color, 1, 20, _flatShading);
            DrawPropLayer(_layer2Buffer, GLOBALS.Settings.L4Maker.Layer2Color, 1, _flatShading);
        }
        
        // Layer 1

        if (_layer1) {
            DrawGeoAndTileLayer(_layer1Buffer, GLOBALS.Settings.L4Maker.Layer1Color, 0, 20, _flatShading);
            DrawPropLayer(_layer1Buffer, GLOBALS.Settings.L4Maker.Layer1Color, 0, _flatShading);
        }
    }

    #endregion

    public override void Draw()
    {
        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) _camera = GLOBALS.Camera;

        var worldMouse = GetScreenToWorld2D(GetMousePosition(), _camera);
        var isWinBusy = _isNavbarHovered || _isOptionsWinHovered;

        #region Shortcuts

        if (!isWinBusy || _quadLock != 0 || _clickLock) {

            // Drag

            if (IsMouseButtonDown(MouseButton.Middle))
            {
                _clickLock = true;

                var delta = GetMouseDelta();
                delta = Raymath.Vector2Scale(delta, -1.0f / _camera.Zoom);
                _camera.Target = Raymath.Vector2Add(_camera.Target, delta);
            }

            if (IsMouseButtonReleased(MouseButton.Middle)) {
                _clickLock = false;
            }

            // Zoom

            var tileWheel = GetMouseWheelMove();
            if (tileWheel != 0)
            {
                var mouseWorldPosition = GetScreenToWorld2D(GetMousePosition(), _camera);
                _camera.Offset = GetMousePosition();
                _camera.Target = mouseWorldPosition;
                _camera.Zoom += tileWheel * GLOBALS.ZoomIncrement;
                if (_camera.Zoom < GLOBALS.ZoomIncrement) _camera.Zoom = GLOBALS.ZoomIncrement;
            }

            // Texture quad handles

            switch (_quadLock) {
                case 0:
                if (IsMouseButtonDown(MouseButton.Left) && !_clickLock) {
                    if (CheckCollisionPointCircle(worldMouse, _l1TopLeftQuadHandle, QuadHandleRadius)) _quadLock = 1;
                    else if (CheckCollisionPointCircle(worldMouse, _l1TopRightQuadHandle, QuadHandleRadius)) _quadLock = 2;
                    else if (CheckCollisionPointCircle(worldMouse, _l1BottomRightQuadHandle, QuadHandleRadius)) _quadLock = 3;
                    else if (CheckCollisionPointCircle(worldMouse, _l1BottomLeftQuadHandle, QuadHandleRadius)) _quadLock = 4;
                    
                    else if (CheckCollisionPointCircle(worldMouse, _l2TopLeftQuadHandle, QuadHandleRadius)) _quadLock = 5;
                    else if (CheckCollisionPointCircle(worldMouse, _l2TopRightQuadHandle, QuadHandleRadius)) _quadLock = 6;
                    else if (CheckCollisionPointCircle(worldMouse, _l2BottomRightQuadHandle, QuadHandleRadius)) _quadLock = 7;
                    else if (CheckCollisionPointCircle(worldMouse, _l2BottomLeftQuadHandle, QuadHandleRadius)) _quadLock = 8;
                    
                    else if (CheckCollisionPointCircle(worldMouse, _l3TopLeftQuadHandle, QuadHandleRadius)) _quadLock = 9;
                    else if (CheckCollisionPointCircle(worldMouse, _l3TopRightQuadHandle, QuadHandleRadius)) _quadLock = 10;
                    else if (CheckCollisionPointCircle(worldMouse, _l3BottomRightQuadHandle, QuadHandleRadius)) _quadLock = 11;
                    else if (CheckCollisionPointCircle(worldMouse, _l3BottomLeftQuadHandle, QuadHandleRadius)) _quadLock = 12;

                    _clickLock = true;
                }
                break;

                case 1: _l1TopLeftQuadHandle = worldMouse; break;
                case 2: _l1TopRightQuadHandle = worldMouse; break;
                case 3: _l1BottomRightQuadHandle = worldMouse; break;
                case 4: _l1BottomLeftQuadHandle = worldMouse; break;

                case 5: _l2TopLeftQuadHandle = worldMouse; break;
                case 6: _l2TopRightQuadHandle = worldMouse; break;
                case 7: _l2BottomRightQuadHandle = worldMouse; break;
                case 8: _l2BottomLeftQuadHandle = worldMouse; break;

                case 9: _l3TopLeftQuadHandle = worldMouse; break;
                case 10: _l3TopRightQuadHandle = worldMouse; break;
                case 11: _l3BottomRightQuadHandle = worldMouse; break;
                case 12: _l3BottomLeftQuadHandle = worldMouse; break;
            }
        }


        if (IsMouseButtonReleased(MouseButton.Left)) {
            _quadLock = 0;
            _clickLock = false;
        }

        #endregion

        BeginDrawing();
        {
            if (_generateSignal) {
                _generateSignal = false;
            
                ClearBackground(Color.Black);

                if (GLOBALS.Font is null) {
                    DrawText("Genrating..", (GetScreenWidth() - MeasureText("Generating..", 50))/2, (GetScreenHeight() - 25)/2, 50, Color.White);
                } else {
                    DrawTextPro(GLOBALS.Font.Value!, "Genrating..", new Vector2((GetScreenWidth() - MeasureText("Generating..", 50))/2, (GetScreenHeight() - 25)/2), new Vector2(0, 0), 0, 50, 0, Color.White);
                }

                BeginTextureMode(_levelBuffer);
                ClearBackground(GLOBALS.Settings.L4Maker.BackgroundColor);

                var shader = _whiteEraser;

                if (_layer3) {
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer3Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 0, ShaderUniformDataType.Int);

                    QuadVectors l3Quad = new(_l3TopLeftQuadHandle + new Vector2(50, 50), _l3TopRightQuadHandle + new Vector2(-50, 50), _l3BottomRightQuadHandle + new Vector2(-50, -50), _l3BottomLeftQuadHandle + new Vector2(50, -50));
                    Printers.DrawTextureQuad(_layer3Buffer.Raw.Texture, l3Quad);

                    EndShaderMode();
                }

                if (_layer2) {
                    BeginShaderMode(shader);

                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer2Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 0, ShaderUniformDataType.Int);

                    QuadVectors l2Quad = new(_l2TopLeftQuadHandle + new Vector2(30, 30), _l2TopRightQuadHandle + new Vector2(-30, 30), _l2BottomRightQuadHandle + new Vector2(-30, -30), _l2BottomLeftQuadHandle + new Vector2(30, -30));
                    Printers.DrawTextureQuad(_layer2Buffer.Raw.Texture, l2Quad);

                    EndShaderMode();
                }

                if (_layer1) {
                    BeginShaderMode(shader);

                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer1Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 0, ShaderUniformDataType.Int);

                    QuadVectors l1Quad = new(_l1TopLeftQuadHandle + new Vector2(10, 10), _l1TopRightQuadHandle + new Vector2(-10, 10), _l1BottomRightQuadHandle + new Vector2(-10, -10), _l1BottomLeftQuadHandle + new Vector2(10, -10));
                    Printers.DrawTextureQuad(_layer1Buffer.Raw.Texture, l1Quad);
                    
                    EndShaderMode();
                }

                EndTextureMode();

                var image = LoadImageFromTexture(_levelBuffer.Raw.Texture);

                if (!Directory.Exists(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "l4"))) {
                    Directory.CreateDirectory(Path.Combine(GLOBALS.Paths.ExecutableDirectory, "l4"));
                }

                if (_genEachCamera) {
                    for (var c = 0; c < GLOBALS.Level.Cameras.Count; c++) {
                        var camera = GLOBALS.Level.Cameras[c];

                        var imageCopy = ImageCopy(image);
                        ImageCrop(ref imageCopy, new Rectangle(camera.Coords, new Vector2(GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight)));
                        ExportImage(imageCopy, Path.Combine(GLOBALS.Paths.ExecutableDirectory, "l4", $"{GLOBALS.Level.ProjectName}_{c+1}.png"));
                        UnloadImage(imageCopy);
                    }
                }

                if (_genWholeLevel) ExportImage(image, Path.Combine(GLOBALS.Paths.ExecutableDirectory, "l4", $"{GLOBALS.Level.ProjectName}.png"));

                UnloadImage(image);

                EndDrawing();
                return;
            }

            ClearBackground(Color.Gray);

            if (_shouldRedrawLevel) {
                DrawLayers();
                _shouldRedrawLevel = false;
            }
            
            BeginMode2D(_camera);
            {
                DrawRectangle(0, 0, GLOBALS.Level.Width * 20, GLOBALS.Level.Height * 20, GLOBALS.Settings.L4Maker.BackgroundColor);

                var shader = _whiteEraser;

                if (_layer3) {
                    BeginShaderMode(shader);
                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer3Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 1, ShaderUniformDataType.Int);

                    QuadVectors l3Quad = new(_l3TopLeftQuadHandle + new Vector2(50, 50), _l3TopRightQuadHandle + new Vector2(-50, 50), _l3BottomRightQuadHandle + new Vector2(-50, -50), _l3BottomLeftQuadHandle + new Vector2(50, -50));
                    Printers.DrawTextureQuad(_layer3Buffer.Raw.Texture, l3Quad);

                    EndShaderMode();
                }

                if (_layer2) {
                    BeginShaderMode(shader);

                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer2Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 1, ShaderUniformDataType.Int);

                    QuadVectors l2Quad = new(_l2TopLeftQuadHandle + new Vector2(30, 30), _l2TopRightQuadHandle + new Vector2(-30, 30), _l2BottomRightQuadHandle + new Vector2(-30, -30), _l2BottomLeftQuadHandle + new Vector2(30, -30));
                    Printers.DrawTextureQuad(_layer2Buffer.Raw.Texture, l2Quad);

                    EndShaderMode();
                }

                if (_layer1) {
                    BeginShaderMode(shader);

                    SetShaderValueTexture(shader, GetShaderLocation(shader, "inputTexture"), _layer1Buffer.Raw.Texture);
                    SetShaderValue(shader, GetShaderLocation(shader, "flipV"), 1, ShaderUniformDataType.Int);

                    QuadVectors l1Quad = new(_l1TopLeftQuadHandle + new Vector2(10, 10), _l1TopRightQuadHandle + new Vector2(-10, 10), _l1BottomRightQuadHandle + new Vector2(-10, -10), _l1BottomLeftQuadHandle + new Vector2(10, -10));
                    Printers.DrawTextureQuad(_layer1Buffer.Raw.Texture, l1Quad);
                    
                    EndShaderMode();
                }

                // Cameras

                if (_cameras)
                {
                    var counter = 0;
                    foreach (var cam in GLOBALS.Level.Cameras)
                    {
                        DrawRectangleLinesEx(
                            _camerasInnerBounds 
                                ? Utils.CameraCriticalRectangle(cam.Coords) 
                                : new(cam.Coords.X, cam.Coords.Y, GLOBALS.EditorCameraWidth, GLOBALS.EditorCameraHeight),
                            4f,
                            GLOBALS.Settings.GeneralSettings.ColorfulCameras 
                                ? GLOBALS.CamColors[counter] 
                                : Color.Pink
                        );

                        counter++;
                        Utils.Cycle(ref counter, 0, GLOBALS.CamColors.Length - 1);
                    }
                }

                // Draw the handles

                if (_layer1) {
                    DrawCircleV(_l1BottomLeftQuadHandle, QuadHandleRadius, Color.Blue);
                    DrawCircleV(_l1BottomRightQuadHandle, QuadHandleRadius, Color.Blue);
                    DrawCircleV(_l1TopRightQuadHandle, QuadHandleRadius, Color.Blue);
                    DrawCircleV(_l1TopLeftQuadHandle, QuadHandleRadius, Color.Blue);
                }

                if (_layer2) {
                    DrawCircleV(_l2TopLeftQuadHandle, QuadHandleRadius, Color.Green);
                    DrawCircleV(_l2TopRightQuadHandle, QuadHandleRadius, Color.Green);
                    DrawCircleV(_l2BottomRightQuadHandle, QuadHandleRadius, Color.Green);
                    DrawCircleV(_l2BottomLeftQuadHandle, QuadHandleRadius, Color.Green);
                }

                if (_layer3) {
                    DrawCircleV(_l3TopLeftQuadHandle, QuadHandleRadius, Color.Red);
                    DrawCircleV(_l3TopRightQuadHandle, QuadHandleRadius, Color.Red);
                    DrawCircleV(_l3BottomRightQuadHandle, QuadHandleRadius, Color.Red);
                    DrawCircleV(_l3BottomLeftQuadHandle, QuadHandleRadius, Color.Red);
                }
            }
            EndTextureMode();

            #region ImGui
            {
                rlImGui_cs.rlImGui.Begin();

                ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                // Navigation bar
                
                if (GLOBALS.Settings.GeneralSettings.Navbar) GLOBALS.NavSignal = Printers.ImGui.Nav(out _isNavbarHovered);

                // Options

                var optionsWinOpened = ImGui.Begin("Options##L4MakerOptionsWin");

                var optionsPos = ImGui.GetWindowPos();
                var optionsWinSpace = ImGui.GetWindowSize();

                _isOptionsWinHovered = CheckCollisionPointRec(GetMousePosition(), new(optionsPos.X - 5, optionsPos.Y, optionsWinSpace.X + 10, optionsWinSpace.Y));

                if (optionsWinOpened) {
                    var neither = !(_genWholeLevel || _genEachCamera);

                    if (neither) ImGui.BeginDisabled();
                    
                    if (ImGui.Button("Generate Image", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        _generateSignal = true;
                    }

                    if (neither) ImGui.EndDisabled();

                    ImGui.Spacing();

                    ImGui.Checkbox("Generate Whole Level", ref _genWholeLevel);
                    ImGui.Checkbox("Generate Each Camera", ref _genEachCamera);

                    ImGui.Spacing();

                    if (ImGui.Checkbox("Layer 1", ref _layer1)) {
                    }

                    if (ImGui.Checkbox("Layer 2", ref _layer2)) {
                    }

                    if (ImGui.Checkbox("Layer 3", ref _layer3)) {
                    }

                    ImGui.Spacing();

                    ImGui.Checkbox("Cameras", ref _cameras);

                    if (!_cameras) ImGui.BeginDisabled();
                    ImGui.Checkbox("Cameras' Inner Boundries", ref _camerasInnerBounds);
                    if (!_cameras) ImGui.EndDisabled();

                    ImGui.Spacing();

                    if (ImGui.Checkbox("Flat Shading", ref _flatShading)) {
                        _shouldRedrawLevel = true;
                    }

                    if (ImGui.Button("Reset Quads", ImGui.GetContentRegionAvail() with { Y = 20 })) {
                        ResetQuadHandles();
                    }

                    //

                    var updated = Printers.ImGui.BindObject(GLOBALS.Settings.L4Maker);

                    if (updated) _shouldRedrawLevel = true;

                    ImGui.End();
                }

                rlImGui_cs.rlImGui.End();
            }
            #endregion
        }
        EndDrawing();

        if (GLOBALS.Settings.GeneralSettings.GlobalCamera) GLOBALS.Camera = _camera;
    }
}