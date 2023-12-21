namespace Leditor.Common;

public static class CommonUtils {
    public static RunCell[,,] Resize(RunCell[,,] array, int width, int height, int newWidth, int newHeight) {

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
                        newArray[y,x,0] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,1] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,2] = new() { Geo = 0, Stackables = new bool[22] };
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
                        newArray[y,x,0] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,1] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,2] = new() { Geo = 0, Stackables = new bool[22] };
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
                        newArray[y,x,0] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,1] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,2] = new() { Geo = 0, Stackables = new bool[22] };
                    }
                }

                for (int y = height; y < newHeight; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y,x,0] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,1] = new() { Geo = 0, Stackables = new bool[22] };
                        newArray[y,x,2] = new() { Geo = 0, Stackables = new bool[22] };
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

    public static bool[,] Resize(bool[,] array, int width, int height, int newWidth, int newHeight, int scale) {
        int xx = (height * scale)+300, yy = (width * scale)+300;
        int nxx = (newHeight * scale)+300, nyy = (newWidth * scale)+300;
        bool[,] newArray = new bool[nyy, nxx];

        if (yy > nyy) {
            if (xx > nxx) {
                for (int y = 0; y < nyy; y++) {
                    for (int x = 0; x < nxx; x++) {
                        newArray[y,x] = array[y, x];
                        newArray[y,x] = array[y, x];
                        newArray[y,x] = array[y, x];
                    }
                }
            } else {
                for (int y = 0; y < nyy; y++) {
                    for (int x = 0; x < xx; x++) {
                        newArray[y,x] = array[y, x];
                        newArray[y,x] = array[y, x];
                        newArray[y,x] = array[y, x];
                    }

                    for (int x = xx; x < nxx; x++) {
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                    }
                }
            }
            
        } else {
            if (yy > nxx) {
                for (int y = 0; y < yy; y++) {
                    for (int x = 0; x < nxx; x++) {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }
                }

                for (int y = yy; y < nyy; y++) {
                    for (int x = 0; x < newWidth; x++) {
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                    }
                }
            } else {
                for (int y = 0; y < yy; y++) {
                    for (int x = 0; x < xx; x++) {
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                        newArray[y, x] = array[y, x];
                    }

                    for (int x = xx; x < nxx; x++) {
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                    }
                }

                for (int y = yy; y < nyy; y++) {
                    for (int x = 0; x < nxx; x++) {
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                        newArray[y,x] = false;
                    }
                }
            }
        }

        return newArray;
    }

    public static bool[,] NewLightMatrix(int width, int height, int scale, bool fill = false) {
        int xx = (height * scale)+300, yy = (width * scale)+300;
        bool[,] matrix = new bool[yy, xx];

        if (fill) {
            for (int y = 0; y < yy; y++) {
                for (int x = 0; x < xx; x++) {
                    matrix[y,x] = true;
                }
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
}