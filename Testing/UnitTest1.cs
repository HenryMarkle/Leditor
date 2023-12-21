namespace Leditor.Testing;

using System.IO.Pipes;
using Leditor.Lingo;
using Leditor.Lingo.Drizzle;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Pidgin;
using Xunit.Abstractions;

public class UnitTest1
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
    }
    
    [Fact]
    public void TestGeoLoad()
    {
        bool success = true;


        try {
            var text = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\ST_A05.txt").Split("\r")[0];
            var obj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text);
            var result = Tools.GetGeoMatrix(obj, out int h, out int w);

            // _output.WriteLine($"Height: {h}, Width: {w}");

            // _output.WriteLine(Tools.StringifyBase(obj));
        } catch (Exception e) {
            _output.WriteLine(e.ToString());
            success = false;
        }

        Assert.True(success, "Failed to parse the world geometry matrix");
    }

    [Fact]
    public void TestExtraTilesLoad()
    {
        bool success = true;

        try {
            var lines = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\ST_A05.txt").Split("\r");
            
            // _output.WriteLine(lines.Length.ToString());
            Assert.True(lines.Length > 5);

            var text = lines[5];
            
            var obj = Lingo.Drizzle.LingoParser.Expression.ParseOrThrow(text);

            // _output.WriteLine(Tools.StringifyBase(obj));

            var result = Tools.GetBufferTiles(obj);

            // _output.WriteLine(result.ToString());
        } catch (Exception e){
            _output.WriteLine(e.ToString());
            success = false;
        }

        Assert.True(success, "Could not get extra tiles");
    }

    [Fact]
    public void TestTileMatrixLoad() {
        bool success = true;

        try {
            var lines = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\ST_A05.txt").Split("\r");

            // _output.WriteLine(lines.Length.ToString());
            Assert.True(lines.Length > 5);

            var text = lines[1];
            
            dynamic obj = LingoParser.Expression.ParseOrThrow(text);

            var matrix = ((AstNode.PropertyList)obj).Values.Single(p => ((AstNode.Symbol)p.Key).Value == "tlmatrix").Value;

            var result = Tools.GetTileMatrix(matrix, out _, out _);

            // _output.WriteLine("PARSED TILE MATRIX: "+result.ToString());
        } catch (Exception e) {
            _output.WriteLine("ERROR MESSAGE: "+e.ToString());
            success = false;
        }

        Assert.True(success, "Failed to load tiles matrix");
    }

    [Fact]
    public void TestTileInitLoad() {
        bool success = true;

        try {
            var text = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\Init.txt").ReplaceLineEndings(Environment.NewLine);

            foreach (var line in text.Split(Environment.NewLine)) {
                if (string.IsNullOrEmpty(line)) continue;

                var stringified = Tools.StringifyBase(LingoParser.Expression.ParseOrThrow(line));
                // _output.WriteLine(stringified);
            }

            var result = Tools.GetTileInit(text);

            // _output.WriteLine("PARSED TILE INIT: "+result.ToString());
        } catch (Exception e) {
            _output.WriteLine("ERROR MESSAGE: "+e.ToString());
            success = false;
        }

        Assert.True(success, "Failed to load tile init");
    }

    [Fact]
    public void TestEffectsLoad() {
        bool success = true;

        try {
            var lines = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\ST_A05.txt").Split("\r");

            // _output.WriteLine(lines.Length.ToString());
            Assert.True(lines.Length > 5);

            var text = lines[2];
            
            dynamic obj = LingoParser.Expression.ParseOrThrow(text);

            var result = Tools.GetEffects(obj, 72, 43);

            _output.WriteLine("PARSED EFFECTS LIST: "+result.ToString());
        } catch (Exception e) {
            _output.WriteLine("ERROR MESSAGE: "+e.ToString());
            success = false;
        }

        Assert.True(success, "Failed to load effects");
    }

    [Fact]
    public void TestCamerasLoad() {
        bool success = true;

        try {
            var lines = File.ReadAllText(@"C:\Users\Henry Markle\Projects\Language-Specific\C#\leditor\Testing\assets\ST_A05.txt").Split("\r");
        
            var text = lines[6];
            
            var obj = LingoParser.Expression.ParseOrThrow(text);

            var result = Tools.GetCameras(obj);

            foreach (var cam in result) {
                _output.WriteLine($"CAMERA: {cam.Coords}; QUADS: [{cam.Quads}]");
            }
        } catch (Exception e) {
            _output.WriteLine("ERROR MESSAGE: "+e.ToString());
            success = false;
        }

        Assert.True(success, "Failed to load cameras");
    }
}