using Leditor.Data;
using Leditor.Data.Generic;
using System.Collections;
using System.Collections.Immutable;

namespace Leditor.Renderer.Generic;

/// <summary>
/// This class does not manage the definition's texture.
/// </summary>
/// <typeparam name="T">A tile, prop, or a material.</typeparam>
public class Dex<T> where T : IIdentifiable<string>, ITexture
{
    public ImmutableArray<T> Definitions { get; private set; }

    private readonly Dictionary<string, T> _names;

    public ImmutableDictionary<string, T> Names => _names.ToImmutableDictionary();

    public delegate void DexDefinitionsRegisteredEventHandler(T[] registered);
    public event DexDefinitionsRegisteredEventHandler? Registered;

    public T Get(string name) => _names[name];
    public bool TryGet(string name, out T? result) => _names.TryGetValue(name, out result);
    public void Register(params T[] definitions)
    {
        Definitions = [ ..Definitions, ..definitions ];

        foreach (var definition in definitions) _names.Add(definition.Name, definition);

        Registered?.Invoke(definitions);
    }

    public Dex(T[] definintions)
    {
        Definitions = [ ..definintions ];
        _names = new(definintions.Length);

        foreach (var d in definintions)
        {
            if (!_names.TryAdd(d.Name, d))
            {
                Raylib_cs.Raylib.UnloadTexture(d.Texture);
            }
        }
    }

    public Dex(T[][] definitions)
    {
        Definitions = definitions.SelectMany(d => d).ToImmutableArray();
        _names = new(Definitions.Length);

        foreach (var d in Definitions) _names.Add(d.Name, d);
    }
}