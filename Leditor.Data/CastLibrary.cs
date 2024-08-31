namespace Leditor.Data;

public readonly record struct CastLibrary(
    string Name,
    int Offset,
    Dictionary<string, CastMember> Members
)
{
    public CastMember this[string name] => Members[name];
};