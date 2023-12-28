namespace Leditor.Common;

public static class CommonUtils {
    public static RunCell[,,] Resize(RunCell[,,] array, int width, int height, int newWidth, int newHeight, RunCell[] layersFill) {

        RunCell[,,] newArray = new RunCell[newHeight, newWidth, 3];

        if (height > newHeight) {
            if (width > newWidth) {
                for (int y = 0; y < newHeight; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y,x,0] = array[y, x, 0];
                        newArray[y,x,1] = array[y, x, 1];
                        newArray[y,x,2] = array[y, x, 2];
                    }
                }
            } else {
                for (int y = 0; y < newHeight; y++) {
                    for (int x = 0; x < width; x++) {
                        newArray[y,x,0] = array[y, x, 0];
                        newArray[y,x,1] = array[y, x, 1];
                        newArray[y,x,2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++) {
                        newArray[y,x,0] = layersFill[0];
                        newArray[y,x,1] = layersFill[1];
                        newArray[y,x,2] = layersFill[2];
                    }
                }
            }
            
        } else {
            if (width > newWidth) {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }
                }

                for (int y = height; y < newHeight; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y,x,0] = layersFill[0];
                        newArray[y,x,1] = layersFill[1];
                        newArray[y,x,2] = layersFill[2];
                    }
                }
            } else {
                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++) {
                        newArray[y, x, 0] = array[y, x, 0];
                        newArray[y, x, 1] = array[y, x, 1];
                        newArray[y, x, 2] = array[y, x, 2];
                    }

                    for (int x = width; x < newWidth; x++) {
                        newArray[y,x,0] = layersFill[0];
                        newArray[y,x,1] = layersFill[1];
                        newArray[y,x,2] = layersFill[2];
                    }
                }

                for (int y = height; y < newHeight; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y,x,0] = layersFill[0];
                        newArray[y,x,1] = layersFill[1];
                        newArray[y,x,2] = layersFill[2];
                    }
                }
            }
        }

        return newArray;
    }

    public static RunCell[,,] NewGeoMatrix(int width, int height, int geoFill = 0) {
        RunCell[,,] matrix = new RunCell[height, width, 3];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                matrix[y,x,0] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y,x,1] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y,x,2] = new() { Geo = 0, Stackables = new bool[22] };
            }
        }

        return matrix;
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

    public static RunCell[][] GetContext(RunCell[,,] matrix, int width, int height, int x, int y, int z) =>
        [
            [ 
                x > 0 && y > 0 ? matrix[y - 1, x - 1, z] : new(), 
                y > 0 ? matrix[y - 1, x, z] : new(), 
                x < width -1 && y > 0 ? matrix[y - 1, x + 1, z] : new()
            ],
            [
                x > 0 ? matrix[y, x - 1, z] : new(),
                new(),
                x < width -1 ? matrix[y, x + 1, z] : new(),
            ],
            [
                x > 0 && y < height -1 ? matrix[y + 1, x - 1, z] : new(),
                y < height -1 ? matrix[y + 1,x, z] : new(),
                x < width -1 && y < height -1 ? matrix[y + 1, x + 1, z] : new()
            ]
        ];
    
    public static bool[] DecomposeStackables(IEnumerable<int> seq) {
        bool[] bools = new bool[22];

        foreach (var i in seq) bools[i] = true;

        return bools;
    }
    
    public static T[,,] Resize<T>(T[,,] array, int width, int height, int newWidth, int newHeight, T[] layersFill)
        where T : notnull, new()
    {
        T[,,] newArray = new T[newHeight, newWidth, 3];

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

        return array;
    }


    public static TileCell[,,] NewTileMatrix(int width, int height)
    {
        TileCell[,,] matrix = new TileCell[height, width, 3];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x, 0] = new()
                {
                    Type = TileType.Default,
                    Data = new TileDefault()
                };
                
                matrix[y, x, 1] = new()
                {
                    Type = TileType.Default,
                    Data = new TileDefault()
                };
                
                matrix[y, x, 2] = new()
                {
                    Type = TileType.Default,
                    Data = new TileDefault()
                };
            }
        }

        return matrix;
    }
}