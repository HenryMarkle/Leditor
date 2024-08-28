/*
    Code in this file was taken from Drizzle.Lingo.Runtime/Parser (https://github.com/SlimeCubed/Drizzle/tree/community/Drizzle.Lingo.Runtime/Parser)
*/

using System.Text;

namespace Leditor.Serialization.Parser;

public static class DebugPrint
{
    public static string PrintAstNode(AstNode.Base node)
    {
        var sb = new StringBuilder();

        node.DebugPrint(sb, 0);

        return sb.ToString();
    }
}