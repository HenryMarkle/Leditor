using Leditor.Serialization.Parser;
using Pidgin;

namespace Leditor.Serialization.IO;

public static class Parsers
{
    public static Task<AstNode.Base> ParseGeoFromProjectAsync(string line)
    {
        return Task.Factory.StartNew(() => LingoParser.Expression.ParseOrThrow(line));
    }
}