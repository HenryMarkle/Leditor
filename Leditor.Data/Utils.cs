using System.Numerics;
using Leditor.Data.Tiles;

namespace Leditor.Data;

public static class Utils
{
    public static int GetMiddle(int number)
    {
        if (number < 3) return 0;
        if (number % 2 == 0) return number / 2 - 1;
        return number / 2;
    }

    /// <summary>
    /// Determines the "middle" of a tile, which where the tile head is positioned.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    public static Vector2 GetTileHeadPosition(in TileDefinition init)
    {
        var (width, height) = init.Size;
        return new Vector2(GetMiddle(width), GetMiddle(height));
    }

    /// <summary>
    /// Determines the "middle" of a tile, which where the tile head is positioned.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    public static (int, int) GetTileHeadPositionI(in TileDefinition init)
    {
        var (width, height) = init.Size;
        return (GetMiddle(width), GetMiddle(height));
    }

    public static bool InBounds<T>(T[,,] matrix, int x, int y)
    {
        return x >= 0 && x < matrix.GetLength(1) && y >= 0 && y < matrix.GetLength(0);
    }

    public static bool InBounds<T>(T[,] matrix, int x, int y)
    {
        return x >= 0 && x < matrix.GetLength(1) && y >= 0 && y < matrix.GetLength(0);
    }

    internal static T[,,] Resize<T>(
        T[,,] matrix,
        
        int left,
        int top,
        int right,
        int bottom,

        T[] layerFill
    ) where T : new() {

        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        var depth = matrix.GetLength(2);

        T[,,] newMatrix = new T[newHeight, newWidth, depth];

        // Default value

        for (var y = 0; y < newHeight; y++) {
            for (var x = 0; x < newWidth; x++) {
                for (var z = 0; z < depth; z++)
                {
                    newMatrix[y, x, z] = layerFill[z];
                }
            }
        } 

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                //

                newMatrix[ny, nx, 0] = matrix[y, x, 0];
                newMatrix[ny, nx, 1] = matrix[y, x, 1];
                newMatrix[ny, nx, 2] = matrix[y, x, 2];

                for (var z = 0; z < depth; z++)
                {
                    newMatrix[ny, nx, z] = matrix[y, x, z];
                }
            }
        }

        return newMatrix;
    }

    internal static T[,] Resize<T>(
        T[,] matrix,
        
        int left,
        int top,
        int right,
        int bottom
    ) where T : new() {
        if (left == 0 && top == 0 && right == 0 && bottom == 0) return matrix;
        if (-left == matrix.GetLength(1) || -right == matrix.GetLength(1) ||
            -top == matrix.GetLength(0) || -bottom == matrix.GetLength(0)) return matrix;

        var width = matrix.GetLength(1);
        var height = matrix.GetLength(0);

        var newWidth = width + left + right;
        var newHeight = height + top + bottom;

        T[,] newMatrix = new T[newHeight, newWidth];

        // Copy old matrix to new matrix

        for (var y = 0; y < height; y++) {
            var ny = y + top;

            if (ny >= newHeight) break;
            if (ny < 0) continue;

            for (var x = 0; x < width; x++) {
                var nx = x + left;

                if (nx >= newWidth) break;
                if (nx < 0) continue;

                //

                newMatrix[ny, nx] = matrix[y, x];
            }
        }

        return newMatrix;
    }

    internal static T[,,] Resize<T>(T[,,] array, int newWidth, int newHeight)

    {
        var width = array.GetLength(1);
        var height = array.GetLength(0);
        
        var newArray = new T[newHeight, newWidth, 3];

        // old height is larger
        if (height > newHeight)
        {
            if (width > newWidth)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }
            }

        }
        // new height is larger or equal
        else
        {
            if (width > newWidth)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }
            }
            // new width is larger
            else
            {

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = default!;
                        newArray[y, x, 1] = default!;
                        newArray[y, x, 2] = default!;
                    }
                }
            }
        }

        return newArray;
    }

    internal static T[,,] Resize<T>(T[,,] array, int newWidth, int newHeight, ReadOnlySpan<T> layersFill)
        where T : notnull, new()
    {
        var width = array.GetLength(1);
        var height = array.GetLength(0);
        
        var newArray = new T[newHeight, newWidth, 3];

        // old height is larger
        if (height > newHeight)
        {
            if (width > newWidth)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }

        }
        // new height is larger or equal
        else
        {
            if (width > newWidth)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
            // new width is larger
            else
            {

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x, 0] = layersFill[0];
                        newArray[y, x, 1] = layersFill[1];
                        newArray[y, x, 2] = layersFill[2];
                    }
                }
            }
        }

        return newArray;
    }

    internal static T[,] Resize<T>(T[,] array, int newWidth, int newHeight)
    {
        var width = array.GetLength(1);
        var height = array.GetLength(0);
        
        var newArray = new T[newHeight, newWidth];

        // old height is larger
        if (height > newHeight)
        {
            if (width > newWidth)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }
                }
            }
            else
            {
                for (int y = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }
            }

        }
        // new height is larger or equal
        else
        {
            if (width > newWidth)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }
            }
            // new width is larger
            else
            {

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }

                    for (int x = width; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }

                for (int y = height; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++)
                    {
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                        newArray[y, x] = default!;
                    }
                }
            }
        }

        return newArray;
    }

    public static Color[,,] NewMaterialColorMatrix(int width, int height, Color @default)
    {
        Color[,,] matrix = new Color[height, width, 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x, 0] = @default;
                matrix[y, x, 1] = @default;
                matrix[y, x, 2] = @default;
            }
        }

        return matrix;
    }
}