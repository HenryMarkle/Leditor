using System.Diagnostics.Contracts;
using System.Numerics;
using Leditor.Data;
using Leditor.Serialization.Exceptions;
using Leditor.Serialization.Parser;

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
        var property = propertyList
            .Values
            .FirstOrDefault(p => ((AstNode.Symbol)p.Key).Value == key)
            .Value;

        if (property is null) throw new MissingPropertyParseException(key);

        if (property is not T t) throw new InvalidCastException($"Property \"{key}\" was not of type {typeof(T)}");
        
        return t;
    }
    
    [Pure]
    internal static int GetInt(AstNode.Base number)
    {
        return number switch
        {
            AstNode.Number n => n.Value.IntValue,
            AstNode.UnaryOperator un => un.Expression is AstNode.Number nn
                ? nn.Value.IntValue * -1
                : throw new InvalidCastException(),
            
            _ => throw new InvalidCastException()
        };
    }

    [Pure]
    internal static bool TryGetInt(AstNode.Base? number, out int integer)
    {
        if (number is null)
        {
            integer = 0;
            return false;
        }
        
        switch (number)
        {
            case AstNode.Number n:
                integer = n.Value.IntValue;
                return true;
            
            case AstNode.UnaryOperator u:
                if (u.Expression is AstNode.Number nn)
                {
                    integer = nn.Value.IntValue * -1;
                    return true;
                }

                integer = 0;
                return false;
            
            default:
                integer = 0;
                return false;
        }
    }
    
    [Pure]
    internal static bool TryGetFloat(AstNode.Base? number, out float @float)
    {
        if (number is null)
        {
            @float = 0;
            return false;
        }
        
        switch (number)
        {
            case AstNode.Number n:
                @float = (float)n.Value.DecimalValue;
                return true;
            
            case AstNode.UnaryOperator u:
                if (u.Expression is AstNode.Number nn)
                {
                    @float = (float) nn.Value.DecimalValue * -1;
                    return true;
                }

                @float = 0;
                return false;
            
            default:
                @float = 0;
                return false;
        }
    }

    [Pure]
    internal static Quad GetQuad(AstNode.List list)
    {
        var topLeftAst = (AstNode.GlobalCall)list.Values[0];
        var topRightAst = (AstNode.GlobalCall)list.Values[1];
        var bottomRightAst = (AstNode.GlobalCall)list.Values[2];
        var bottomLeftAst = (AstNode.GlobalCall)list.Values[3];

        return new Quad(
            GetIntVector(topLeftAst), 
            GetIntVector(topRightAst), 
            GetIntVector(bottomRightAst), 
            GetIntVector(bottomLeftAst)
        );
    }

    [Pure]
    internal static bool TryGetQuad(AstNode.List list, out Quad quad)
    {
        var b1 = list.Values[0];
        var b2 = list.Values[1];
        var b3 = list.Values[2];
        var b4 = list.Values[3];

        var q1 = TryGetIntVector(b1, out var v1);
        var q2 = TryGetIntVector(b2, out var v2);
        var q3 = TryGetIntVector(b3, out var v3);
        var q4 = TryGetIntVector(b4, out var v4);

        if (!(q1 && q2 && q3 && q4))
        {
            quad = new Quad();
            return false;
        }

        quad = new Quad(v1, v2, v3, v4);
        return true;
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
    internal static Vector2 GetIntVector(AstNode.GlobalCall globalCall)
    {
        var args = globalCall.Arguments;

        return new Vector2(GetInt(args[0]), GetInt(args[1]));
    }

    [Pure]
    internal static bool TryGetIntVector(AstNode.Base? @base, out Vector2 vector)
    {
        if (@base is not AstNode.GlobalCall g)
        {
            vector = new Vector2(0, 0);
            return false;
        }

        if (g.Name != "point" || g.Arguments.Length != 2)
        {
            vector = new Vector2(0, 0);
            return false;
        }
        
        var args = g.Arguments;

        if (!(TryGetInt(args[0], out var x) && TryGetInt(args[1], out var y)))
        {
            vector = new Vector2(0, 0);
            return false;
        }

        vector = new Vector2(x, y);
        return true;
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