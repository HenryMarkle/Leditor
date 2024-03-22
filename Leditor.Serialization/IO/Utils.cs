using System.Diagnostics.Contracts;
using Leditor.RL.Managed;

namespace Leditor.Serialization.IO;

public static class Utils
{
    internal static async Task<Dictionary<string, Image>> LoadImages(string directory)
    {
        var files = Directory.GetFiles(directory);
        
        Task<(string name, Image image)>[] imageTasks = files
            .Where(f => Path.GetExtension(f) == ".png")
            .Select(f => Task.Factory.StartNew(() =>
            {
                var name = Path.GetFileNameWithoutExtension(f);

                return (name, new Image(f));
            }))
            .ToArray();

        Dictionary<string, Image> dict = new();

        foreach (var task in imageTasks)
        {
            var (name, image) = await task;
            dict.Add(name, image);
        }

        return dict;
    }
    internal static Func<Texture2D> PrepareImage(Image image) => () => new Texture2D(image);
    
    internal static IEnumerable<Func<Texture2D>> PrepareImages(IEnumerable<Image> images)
        => images.Select(PrepareImage);
}