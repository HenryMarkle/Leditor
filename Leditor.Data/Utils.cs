namespace Leditor.Data;

public static class Utils
{
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

}