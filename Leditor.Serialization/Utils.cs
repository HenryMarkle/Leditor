using System.Diagnostics.Contracts;
using Drizzle.Lingo.Runtime.Parser;
using Leditor.Serialization.Exceptions;

namespace Leditor.Serialization;

internal static class Utils
{
    [Pure]
    internal static T? TryGet<T>(AstNode.PropertyList propertyList, string key)
        where T : AstNode.Base
    {
        var property = propertyList
            .Values
            .FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == key)
            .Value;

        if (property is T t) return t;
        
        return null;
    }

    [Pure]
    internal static T Get<T>(AstNode.PropertyList propertyList, string key)
        where T : AstNode.Base
    {
        var property = (T?) propertyList
            .Values
            .FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == key)
            .Value;

        return property ?? throw new MissingPropertyParseException(key);
    }
    
    [Pure]
    internal static int GetInt(AstNode.Base number)
    {
        return number is AstNode.UnaryOperator u 
            ? ((AstNode.Number)u.Expression).Value.IntValue * -1
            : ((AstNode.Number)number).Value.IntValue;
    }
    
    [Pure]
    internal static float GetFloat(AstNode.Base number)
    {
        return number is AstNode.UnaryOperator u 
            ? (float) ((AstNode.Number)u.Expression).Value.DecimalValue * -1
            : (float) ((AstNode.Number)number).Value.DecimalValue;
    }

    [Pure]
    internal static (int, int) GetIntPair(AstNode.GlobalCall globalCall)
    {
        var args = globalCall.Arguments;

        return (GetInt(args[0]), GetInt(args[1]));
    }
    
    [Pure]
    internal static (float, float) GetFloatPair(AstNode.GlobalCall globalCall)
    {
        var args = globalCall.Arguments;

        return (GetInt(args[0]), GetInt(args[1]));
    }

    [Pure]
    internal static (byte r, byte g, byte b) GetColor(AstNode.GlobalCall globalCall)
    {
        var args = globalCall.Arguments;
        
        return ((byte)GetInt(args[0]), (byte)GetInt(args[1]), (byte)GetInt(args[2]));
    }

    [Pure]
    internal static int[] GetIntArray(AstNode.List list) => [..list.Values.Select(GetInt)];
}