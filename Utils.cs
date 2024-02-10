using System.Numerics;
using System.Windows.Forms;
using System.Threading;

namespace Leditor;

/// A collection of helper functions used across pages
internal static class Utils
{
    internal static async Task<string> GetFilePathAsync()
    {
        var path = string.Empty;
        
        var thread = new Thread(() =>
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = GLOBALS.Paths.ProjectsDirectory,
                Filter = "txt files (*.txt)|*.txt",
                Multiselect = false,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
            }
        });
            
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return path;
    }

    internal static async Task<string> SetFilePathAsync()
    {
        var path = string.Empty;
        
        var thread = new Thread(() =>
        {
            var dialog = new SaveFileDialog
            {
                InitialDirectory = GLOBALS.Paths.ProjectsDirectory,
                Filter = "txt files (*.txt)|*.txt",
                FileName = GLOBALS.Level.ProjectName
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
            }
        });
            
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        return path;
    }
    
    public static int GetBrushStrength(string effect) => effect switch
    {
        "BlackGoo" or "Fungi Flowers" or "Lighthouse Flowers" or
            "Fern" or "Giant Mushroom" or "Sprawlbush" or
            "featherFern" or "Fungus Tree" or "Restore As Scaffolding" or "Restore As Pipes" => 100,

        _ => 10
    };

    public static EffectOptions[] NewEffectOptions(string name)
    {
        EffectOptions[] options = name switch
        {
            "Slime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "LSime" => [new("3D", ["Off", "On"], "Off")],
            
            "Fat Slime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "Scales" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "SlimeX3" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "Yes")
            ],
            
            "DecalsOnlySlime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Rust" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Barnacles" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Colored Barnacles" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Clovers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Ivy" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Color Intensity", ["High", "Medium", "Low", "None", "Random"], "Medium"),
                new("Fruit Density", ["High", "Medium", "Low", "None"], "None"),
                new("Leaf Density", [], new Random().Next(100))
            ],
            
            "Little Flowers" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Detail Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Rotate", ["Off", "On"], "Off")
            ],
            
            "Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Sand" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Super Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
            ],
            
            "Ultra Super Erode" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Roughen" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Impacts" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Super Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Destructive Melt" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("3D", ["Off", "On"], "Off"),
                new("Affect Gradients and Decals", ["Yes", "No"], "No")
            ],
            
            "Rubble" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Colored Rubble" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Fungi Flowers" or "Lighthouse Flowers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1")
            ],
            
            "Colored Fungi Flowers" or "Colored Lighthouse Flowers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Foliage" or "High Grass" or "High Fern" or "Mistletoe" or "Reeds" or "Lavenders" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Ring Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Assorted Trash" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "None")
            ],
            
            "Fern" or "Giant Mushroom" or "Sprawlbush" or "featherFern" or "Fungus Tree" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "1"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Root Grass" or "Growers" or "Cacti" or "Rain Moss" or "Dense Mold" or 
                "Seed Pods" or "Grass" or "Arm Growers" or "Horse Tails" or "Circuit Plants" or 
                "Feather Plants" or "Mini Growers" or "Left Facing Kelp" or "Right Facing Kelp" or 
                "Club Moss" or "Moss Wall" or "Mixed Facing Kelp" or "Bubble Grower" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Rollers" or "Thorn Growers" or "Garbage Spirals" or "Spinets" or "Small Springs" or "Fuzzy Growers" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Wires" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Fatness", ["1px", "2px", "3px", "random"], "2px")
            ],
            
            "Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Size", ["Small", "FAT"], "Small")
            ],
            
            "Colored Wires" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Fatness", ["1px", "2px", "3px", "random"], "2px"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "Colored Chains" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Size", ["Small", "FAT"], "Small"),
                new("Effect Color", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ],
            
            "DarkSlime" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
            ],
            
            "Hang Roots" or "Thick Roots" or "Shadow Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Colored Hang Roots" or "Colored Thick Roots" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Colored Shadow Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Root Plants" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All"),
                new("Color", ["Color1", "Color2", "Dead"], "Color2")
            ],
            
            "Restore As Scaffolding" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Restore As Pipes" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Ceramic Chaos" => [
                new("Ceramic Color", ["None", "Colored"], "Colored"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "DaddyCorruption" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Wastewater Mold" => [
                new("Color", ["Color1", "Color2", "Dead"], "Color2"),
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Corruption No Eye" or "Slag" => [
                new("Layers", ["All", "1", "2", "3", "1:st and 2:nd", "2:nd and 3:rd"], "All")
            ],
            
            "Stained Glass Properties" => [
                new("Variation", ["1", "2", "3"], "1"),
                new("Color 1", ["EffectColor1", "EffectColor2", "None"], "EffectColor1"),
                new("Color 2", ["EffectColor1", "EffectColor2", "None"], "EffectColor2")
            ]
        };

        return [
            new("Delete/Move", ["Delete", "Move Back", "Move Forth"], ""),
            new("Seed", [], new Random().Next(1000)),
            ..options
        ];
    }
    
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
    internal static Vector2 GetTileHeadOrigin(in InitTile init)
    {
        var (width, height) = init.Size;
        return new Vector2(GetMiddle(width), GetMiddle(height));
    }

    /// Maps a geo block id to a block texture index
    public static int GetBlockIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        4 => 3,
        5 => 4,
        6 => 5,
        7 => 6,
        9 => 7,

        _ => -1,
    };

    /// Maps a UI texture index to block ID
    public static int GetBlockID(uint index) => index switch
    {
        0 => 1,
        1 => 0,
        7 => 4,
        3 => 2,
        2 => 3,
        6 => 6,
        30 => 9,
        _ => -1
    };


    /// Maps a UI texture index to a stackable ID
    public static int GetStackableID(uint index) => index switch
    {
        28 => 9,
        29 => 10,
        9 => 11,
        10 => 1,
        11 => 2,
        14 => 4,
        15 => 5,
        16 => 7,
        17 => 6,
        18 => 3,
        19 => 18,
        20 => 21,
        21 => 19,
        22 => 13,
        23 => 20,
        24 => 12,

        _ => -1
    };

    public static int GetStackableTextureIndex(int id) => id switch
    {
        1 => 0,
        2 => 1,
        3 => 2,
        5 => 3,
        6 => 27,
        7 => 28,
        9 => 18,
        10 => 29,
        12 => 30,
        13 => 16,
        18 => 19,
        19 => 20,
        20 => 21,
        21 => 17,
        _ => -1
    };

    /// <summary>
    /// This is used to determine the index of the stackable texture, including the directional ones.
    /// </summary>
    /// <param name="id">the ID of the geo-tile feature</param>
    /// <param name="context">a 3x3 slice of the geo-matrix where the geo-tile feature is in the middle</param>
    /// <returns>the index of the texture in the textures array (GLOBALS.Textures.GeoStackables)</returns>
    public static int GetStackableTextureIndex(int id, RunCell[][] context)
    {
        var i = id switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => -4,
            5 => 3,
            6 => 27,
            7 => 28,
            9 => 18,
            10 => 29,
            11 => -11,
            12 => 30,
            13 => 16,
            18 => 19,
            19 => 20,
            20 => 21,
            21 => 17,
            _ => -1
        };


        if (i == -4)
        {
            if (
                context[0][0].Stackables[4] || context[0][1].Stackables[4] || context[0][2].Stackables[4] ||
                context[1][0].Stackables[4] || context[1][2].Stackables[4] ||
                context[2][0].Stackables[4] || context[2][1].Stackables[4] || context[2][2].Stackables[4]
            ) return 26;

            var pattern = (
                false, context[0][1].Stackables[5] ^ context[0][1].Stackables[6] ^ context[0][1].Stackables[7] ^ context[0][1].Stackables[19] ^ context[0][1].Stackables[21], false,
                context[1][0].Stackables[5] ^ context[1][0].Stackables[6] ^ context[1][0].Stackables[7] ^ context[1][0].Stackables[19] ^ context[1][0].Stackables[21], false, context[1][2].Stackables[5] ^ context[1][2].Stackables[6] ^ context[1][2].Stackables[7] ^ context[1][2].Stackables[19] ^ context[1][2].Stackables[21],
                false, context[2][1].Stackables[5] ^ context[2][1].Stackables[6] ^ context[2][1].Stackables[7] ^ context[2][1].Stackables[19] ^ context[2][1].Stackables[21], false
            );

            var directionIndex = pattern switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 25,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 24,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 22,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 23,

                _ => 26
            };

            if (directionIndex == 26) return 26;

            var geoPattern = (
                context[0][0].Geo, context[0][1].Geo, context[0][2].Geo,
                context[1][0].Geo, 0, context[1][2].Geo,
                context[2][0].Geo, context[2][1].Geo, context[2][2].Geo
            );

            directionIndex = geoPattern switch
            {

                (
                    1, _, 1,
                    1, _, 1,
                    1, 1, 1
                ) => context[0][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, _,
                    1, 1, 1
                ) => context[1][2].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    1, _, 1,
                    1, _, 1
                ) => context[2][1].Geo is 0 or 6 ? directionIndex : 26,

                (
                    1, 1, 1,
                    _, _, 1,
                    1, 1, 1
                ) => context[1][0].Geo is 0 or 6 ? directionIndex : 26,

                _ => 26
            };

            return directionIndex;
        }
        else if (i == -11)
        {
            i = (
                false, context[0][1].Stackables[11], false,
                context[1][0].Stackables[11], false, context[1][2].Stackables[11],
                false, context[2][1].Stackables[11], false
            ) switch
            {

                (
                    _, true, _,
                    false, _, false,
                    _, false, _
                ) => 33,

                (
                    _, false, _,
                    false, _, true,
                    _, false, _
                ) => 32,

                (
                    _, false, _,
                    false, _, false,
                    _, true, _
                ) => 31,

                (
                    _, false, _,
                    true, _, false,
                    _, false, _
                ) => 34,

                //

                (
                    _, true, _,
                    false, _, true,
                    _, false, _
                ) => 13,

                (
                    _, false, _,
                    false, _, true,
                    _, true, _
                ) => 5,

                (
                    _, false, _,
                    true, _, false,
                    _, true, _
                ) => 4,

                (
                    _, true, _,
                    true, _, false,
                    _, false, _
                ) => 10,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, false, _
                ) => 12,

                (
                    _, true, _,
                    false, _, true,
                    _, true, _
                ) => 9,

                (
                    _, false, _,
                    true, _, true,
                    _, true, _
                ) => 8,

                (
                    _, true, _,
                    true, _, false,
                    _, true, _
                ) => 11,

                //

                (
                    _, false, _,
                    true, _, true,
                    _, false, _
                ) => 7,

                (
                    _, true, _,
                    false, _, false,
                    _, true, _
                ) => 15,

                //

                (
                    _, true, _,
                    true, _, true,
                    _, true, _
                ) => 6,

                (
                    _, false, _,
                    false, _, false,
                    _, false, _
                ) => (
                false, context[0][1].Geo == 1, false,
                context[1][0].Geo == 1, context[1][1].Geo == 1, context[1][2].Geo == 1,
                false, context[2][0].Geo == 1, false
                ) switch
                {
                    (
                    _, false, _,
                    true, true, true,
                    _, false, _
                    ) => 15,
                    (
                    _, true, _,
                    false, true, false,
                    _, true, _
                    ) => 7,
                    _ => 14
                }
            };
        }

        return i;
    }


    /// <summary>
    /// Determines the id (direction) of the slope depending on the surrounding geos.
    /// </summary>
    /// <param name="context">a 3x3 slice of the geo-matrix where the slope is in the middle</param>
    /// <returns>the ID of the slope representing the proper direction</returns>
    public static int GetCorrectSlopeID(RunCell[][] context)
    {
        var fi = (
            false, context[0][1].Geo is 1, false,
            context[1][0].Geo is 1, false, context[1][2].Geo is 1,
            false, context[2][1].Geo is 1, false
        ) switch
        {
            (
                _, false, _,
                true, _, false,
                _, true, _
            ) => 2,
            (
                _, false, _,
                false, _, true,
                _, true, _
            ) => 3,
            (
                _, true, _,
                true, _, false,
                _, false, _
            ) => 4,
            (
                _, true, _,
                false, _, true,
                _, false, _
            ) => 5,

            _ => -1

        };

        if (fi == -1) return -1;

        var ssi = context[0][1].Geo is 2 or 3 or 4 or 5 || 
                  context[1][0].Geo is 2 or 3 or 4 or 5 || 
                  context[1][2].Geo is 2 or 3 or 4 or 5 ||
                  context[2][1].Geo is 2 or 3 or 4 or 5;

        if (ssi) return -1;

        return fi;
    }

    internal static RunCell[,,] Resize(RunCell[,,] array, int width, int height, int newWidth, int newHeight, RunCell[] layersFill)
    {

        RunCell[,,] newArray = new RunCell[newHeight, newWidth, 3];

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

    internal static void Resize((string, EffectOptions, double[,])[] list, int width, int height, int newWidth, int newHeight)
    {
        for (int i = 0; i < list.Length; i++)
        {
            var array = list[i].Item3;
            var newArray = new double[newHeight, newWidth];

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
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }

            }
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
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }
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
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }

                    for (int y = height; y < newHeight; y++)
                    {
                        for (int x = 0; x < newWidth; x++)
                        {
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                            newArray[y, x] = 0;
                        }
                    }
                }
            }
            list[i].Item3 = newArray;
        }
    }

    internal static RunCell[,,] NewGeoMatrix(int width, int height, int geoFill = 0)
    {
        RunCell[,,] matrix = new RunCell[height, width, 3];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                matrix[y, x, 0] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y, x, 1] = new() { Geo = geoFill, Stackables = new bool[22] };
                matrix[y, x, 2] = new() { Geo = 0, Stackables = new bool[22] };
            }
        }

        return matrix;
    }

    internal static Color[,,] NewMaterialColorMatrix(int width, int height, Color @default)
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

    /// <summary>
    /// Retrieves a 3x3 slide from the geo-matrix.
    /// </summary>
    /// <param name="matrix">the geo-matrix (GLOBALS.Level.GeoMatrix)</param>
    /// <param name="width">the width of the matrix</param>
    /// <param name="height">the height of the matrix</param>
    /// <param name="x">x-position of the middle of the slice</param>
    /// <param name="y">y-position of the middle of the slice</param>
    /// <param name="z">z-position of the middle of the slice</param>
    internal static RunCell[][] GetContext(RunCell[,,] matrix, int width, int height, int x, int y, int z) =>
        [
            [
                x > 0 && y > 0 ? matrix[y - 1, x - 1, z] : new(),
                y > 0 ? matrix[y - 1, x, z] : new(),
                x < width - 1 && y > 0 ? matrix[y - 1, x + 1, z] : new()
            ],
            [
                x > 0 ? matrix[y, x - 1, z] : new(),
                matrix[y, x, z],
                x < width - 1 ? matrix[y, x + 1, z] : new(),
            ],
            [
                x > 0 && y < height - 1 ? matrix[y + 1, x - 1, z] : new(),
                y < height - 1 ? matrix[y + 1, x, z] : new(),
                x < width - 1 && y < height - 1 ? matrix[y + 1, x + 1, z] : new()
            ]
        ];
    
    /// <summary>
    /// Retrieves a 3x3 slide from a temporary 0-depth geo-matrix slice (for copy-paste feature).
    /// </summary>
    /// <param name="matrix">the geo-matrix (GLOBALS.Level.GeoMatrix)</param>
    /// <param name="x">x-position of the middle of the slice</param>
    /// <param name="y">y-position of the middle of the slice</param>
    internal static RunCell[][] GetContext(RunCell[,] matrix, int x, int y) =>
        [
            [
                x > 0 && y > 0 ? matrix[y - 1, x - 1] : new(),
                y > 0 ? matrix[y - 1, x] : new(),
                x < matrix.GetLength(1) - 1 && y > 0 ? matrix[y - 1, x + 1] : new()
            ],
            [
                x > 0 ? matrix[y, x - 1] : new(),
                matrix[y, x],
                x < matrix.GetLength(1) - 1 ? matrix[y, x + 1] : new(),
            ],
            [
                x > 0 && y < matrix.GetLength(0) - 1 ? matrix[y + 1, x - 1] : new(),
                y < matrix.GetLength(0) - 1 ? matrix[y + 1, x] : new(),
                x < matrix.GetLength(1) - 1 && y < matrix.GetLength(0) - 1 ? matrix[y + 1, x + 1] : new()
            ]
        ];


    /// Meaningless name; this function turns a sequel of stackable IDs to an array that can be used at leditor runtime
    internal static bool[] DecomposeStackables(IEnumerable<int> seq)
    {
        bool[] bools = new bool[22];

        foreach (var i in seq) bools[i] = true;

        return bools;
    }

    /// <summary>
    /// Generic resize method of a 3D array (with the z dimension being exactly 3).
    /// </summary>
    /// <param name="array">The matrix</param>
    /// <param name="newWidth">new matrix width</param>
    /// <param name="newHeight">new matrix height</param>
    /// <param name="layersFill"></param>
    /// <typeparam name="T">a 3-length list (representing the three level layers) of geo IDs to fill extra space with</typeparam>
    /// <returns>a new matrix with <paramref name="newWidth"/> and <paramref name="newHeight"/> as the new dimensions</returns>
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

        return array;
    }

    internal static TileCell[,,] NewTileMatrix(int width, int height)
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

    internal static bool[] NewStackables(bool fill = false)
    {
        var array = new bool[22];
        
        for (var i = 0; i < array.Length; i++) array[i] = fill;

        return array;
    }

    /// <summary>
    /// Determines where the tile preview texture starts in the tile texture.
    /// </summary>
    /// <param name="init">a reference to the tile definition</param>
    /// <returns>a number representing the y-depth value where the tile preview starts</returns>
    internal static int GetTilePreviewStartingHeight(in InitTile init)
    {
        var (width, height) = init.Size;
        var bufferTiles = init.BufferTiles;
        var repeatL = init.Repeat.Length;
        var scale = GLOBALS.Scale;

        var offset = init.Type switch
        {
            InitTileType.VoxelStruct => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRockType => 1 + scale * ((bufferTiles * 2) + height),
            InitTileType.Box => scale * height * width + (scale * (height + (2 * bufferTiles))),
            InitTileType.VoxelStructRandomDisplaceVertical => 1 + scale * ((bufferTiles * 2) + height) * repeatL,
            InitTileType.VoxelStructRandomDisplaceHorizontal => 1 + scale * ((bufferTiles * 2) + height) * repeatL,

            _ => 1 + scale * ((bufferTiles * 2) + height) * repeatL
        };

        return offset;
    }

    internal static Rectangle EncloseQuads(PropQuads quads)
    {
        var nearestX = Math.Min(Math.Min(quads.TopLeft.X, quads.TopRight.X), Math.Min(quads.BottomLeft.X, quads.BottomRight.X));
        var nearestY = Math.Min(Math.Min(quads.TopLeft.Y, quads.TopRight.Y), Math.Min(quads.BottomLeft.Y, quads.BottomRight.Y));

        var furthestX = Math.Max(Math.Max(quads.TopLeft.X, quads.TopRight.X), Math.Max(quads.BottomLeft.X, quads.BottomRight.X));
        var furthestY = Math.Max(Math.Max(quads.TopLeft.Y, quads.TopRight.Y), Math.Max(quads.BottomLeft.Y, quads.BottomRight.Y));
       
        return new Rectangle(nearestX, nearestY, furthestX - nearestX, furthestY - nearestY);
    }
    
    internal static Rectangle EncloseQuads(ref PropQuads quads)
    {
        var nearestX = Math.Min(Math.Min(quads.TopLeft.X, quads.TopRight.X), Math.Min(quads.BottomLeft.X, quads.BottomRight.X));
        var nearestY = Math.Min(Math.Min(quads.TopLeft.Y, quads.TopRight.Y), Math.Min(quads.BottomLeft.Y, quads.BottomRight.Y));

        var furthestX = Math.Max(Math.Max(quads.TopLeft.X, quads.TopRight.X), Math.Max(quads.BottomLeft.X, quads.BottomRight.X));
        var furthestY = Math.Max(Math.Max(quads.TopLeft.Y, quads.TopRight.Y), Math.Max(quads.BottomLeft.Y, quads.BottomRight.Y));
       
        return new Rectangle(nearestX, nearestY, furthestX - nearestX, furthestY - nearestY);
    }
    
    internal static Rectangle EncloseProps(IEnumerable<PropQuads> quadsList)
    {
        float 
            minX = float.MaxValue, 
            minY = float.MaxValue, 
            maxX = float.MinValue, 
            maxY = float.MinValue;

        foreach (var q in quadsList)
        {
            minX = Math.Min(
                minX, 
                Math.Min(
                    Math.Min(q.TopLeft.X, q.TopRight.X), 
                    Math.Min(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            minY = Math.Min(
                minY, 
                Math.Min(
                    Math.Min(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Min(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
            
            maxX = Math.Max(
                maxX, 
                Math.Max(
                    Math.Max(q.TopLeft.X, q.TopRight.X), 
                    Math.Max(q.BottomLeft.X, q.BottomRight.X)
                )
            );
            
            maxY = Math.Max(
                maxY, 
                Math.Max(
                    Math.Max(q.TopLeft.Y, q.TopRight.Y), 
                    Math.Max(q.BottomLeft.Y, q.BottomRight.Y)
                )
            );
        }
        
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    }

    //TODO: change to ref
    internal static PropQuads RotatePropQuads(PropQuads quads, float angle)
    {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        // Enclose the quads

        var rect = EncloseQuads(quads);
        
        // Get the center of the rectangle

        var center = new Vector2(rect.x + rect.width/2, rect.y + rect.height/2);

        // var center = new Vector2(0, 0);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = quads.TopLeft.X;
            var y = quads.TopLeft.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the top right corner
            var x = quads.TopRight.X;
            var y = quads.TopRight.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newTopRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom right corner
            var x = quads.BottomRight.X;
            var y = quads.BottomRight.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newBottomRight = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        { // Rotate the bottom left corner
            var x = quads.BottomLeft.X;
            var y = quads.BottomLeft.Y;

            var dx = center.X - x;
            var dy = center.Y - y;

            newBottomLeft = new Vector2(center.X + dx * cosRotation - dy * sinRotation, center.Y + dx * sinRotation + dy * cosRotation);
        }
        
        return new(newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }
    
    internal static PropQuads RotatePropQuads(PropQuads quads, float angle, Vector2 center)
    {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        Vector2 newTopLeft, newTopRight, newBottomRight, newBottomLeft;

        { // Rotate the top left corner
            var x = quads.TopLeft.X;
            var y = quads.TopLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;
            
            newTopLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
            
            //Console.WriteLine(newTopLeft);
        }
        
        { // Rotate the top right corner
            var x = quads.TopRight.X;
            var y = quads.TopRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newTopRight = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the bottom right corner
            var x = quads.BottomRight.X;
            var y = quads.BottomRight.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomRight = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        { // Rotate the bottom left corner
            var x = quads.BottomLeft.X;
            var y = quads.BottomLeft.Y;

            var dx = x - center.X;
            var dy = y - center.Y;

            newBottomLeft = new Vector2(
                center.X + dx * cosRotation - dy * sinRotation, 
                center.Y + dx * sinRotation + dy * cosRotation
            );
        }
        
        return new(newTopLeft, newTopRight, newBottomRight, newBottomLeft);
    }
    
    internal static void RotatePoints(float angle, Vector2 center, Vector2[] points) {
        // Convert angle to radians

        var radian = float.DegreesToRadians(angle);
        
        var sinRotation = (float)Math.Sin(radian);
        var cosRotation = (float)Math.Cos(radian);
        
        for (var p = 0; p < points.Length; p++)
        {
            ref var point = ref points[p];

            var dx = point.X - center.X;
            var dy = point.Y - center.Y;

            point.X = center.X + dx * cosRotation - dy * sinRotation;
            point.Y = center.Y + dx * sinRotation + dy * cosRotation;
        }
    }

    internal static Vector2 QuadsCenter(ref PropQuads quads)
    {
        var rect = EncloseQuads(quads);
        return new(rect.X + rect.width/2, rect.y + rect.height/2);
    }

    internal static Vector2 RectangleCenter(ref Rectangle rectangle) => new(rectangle.X + rectangle.width / 2, rectangle.Y + rectangle.height / 2);

    internal static void ScaleQuads(ref PropQuads quads, float factor)
    {
        var enclose = EncloseQuads(quads);
        var center = RectangleCenter(ref enclose);
        
        quads.TopLeft = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.TopLeft, center), 
                factor
            ), 
            center
        );
                        
        quads.TopRight = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.TopRight, center), 
                factor
            ), 
            center
        );
                        
        quads.BottomLeft = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.BottomLeft, center), 
                factor
            ), 
            center
        ); 
                        
        quads.BottomRight = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.BottomRight, center), 
                factor
            ), 
            center
        );
    }
    
    internal static void ScaleQuads(ref PropQuads quads, Vector2 center, float factor)
    {
        quads.TopLeft = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.TopLeft, center), 
                factor
            ), 
            center
        );
                        
        quads.TopRight = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.TopRight, center), 
                factor
            ), 
            center
        );
                        
        quads.BottomLeft = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.BottomLeft, center), 
                factor
            ), 
            center
        ); 
                        
        quads.BottomRight = RayMath.Vector2Add(
            RayMath.Vector2Scale(
                RayMath.Vector2Subtract(quads.BottomRight, center), 
                factor
            ), 
            center
        );
    }

    internal static void ScaleQuadsX(ref PropQuads quads, Vector2 center, float factor)
    {
        quads.TopLeft = quads.TopLeft with { X = (quads.TopLeft.X - center.X) * factor + center.X };
        quads.BottomLeft = quads.BottomLeft with { X = (quads.BottomLeft.X - center.X) * factor + center.X };
        quads.TopRight = quads.TopRight with { X = (quads.TopRight.X - center.X) * factor + center.X };
        quads.BottomRight = quads.BottomRight with { X = (quads.BottomRight.X - center.X) * factor + center.X };
    }

    internal static void ScaleQuadsY(ref PropQuads quads, Vector2 center, float factor)
    {
        quads.TopLeft = quads.TopLeft with { Y = (quads.TopLeft.Y - center.Y) * factor + center.Y };
        quads.BottomLeft = quads.BottomLeft with { Y = (quads.BottomLeft.Y - center.Y) * factor + center.Y };
        quads.TopRight = quads.TopRight with { Y = (quads.TopRight.Y - center.Y) * factor + center.Y };
        quads.BottomRight = quads.BottomRight with { Y = (quads.BottomRight.Y - center.Y) * factor + center.Y };
    }

    internal static (Vector2 pA, Vector2 pB) RopeEnds(in PropQuads quads)
    {
        return (
            RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopLeft, quads.BottomLeft), new(2f, 2f)), 
            RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopRight, quads.BottomRight), new(2f, 2f))
        );
    }
    
    internal static (Vector2 pA, Vector2 pB) RopeEnds(PropQuads quads)
    {
        return (
            RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopLeft, quads.BottomLeft), new(2f, 2f)), 
            RayMath.Vector2Divide(RayMath.Vector2Add(quads.TopRight, quads.BottomRight), new(2f, 2f))
        );
    }

    internal static (Vector2 left, Vector2 top, Vector2 right, Vector2 bottom) LongSides(in PropQuads quad)
    {
        var (left, right) = RopeEnds(quad);

        var top = new Vector2((quad.TopLeft.X + quad.TopRight.X)/2f, (quad.TopLeft.Y + quad.TopRight.Y)/2f);
        var bottom = new Vector2((quad.BottomLeft.X + quad.BottomRight.X)/2f, (quad.BottomLeft.Y + quad.BottomRight.Y)/2f);
        
        return (left, top, right, bottom);
    }
    
    internal static Vector2[] GenerateRopePoints(in Vector2 pointA, in Vector2 pointB, int count = 3)
    {
        var distance = RayMath.Vector2Distance(pointA, pointB);

        var delta = distance / count;

        List<Vector2> points = [];
        
        for (var step = 0; step < count; step++) points.Add(RayMath.Vector2MoveTowards(pointA, pointB, delta * step));

        return [..points];
    }

    internal static Vector2[] Casteljau(int steps, Vector2[] points) {
        Vector2 GetCasteljauPoint(int r, int i, double t) { 
            if(r == 0) return points[i];

            var p1 = GetCasteljauPoint(r - 1, i, t);
            var p2 = GetCasteljauPoint(r - 1, i + 1, t);

            return new Vector2((int) ((1 - t) * p1.X + t * p2.X), (int) ((1 - t) * p1.Y + t * p2.Y));
        }
        
        List<Vector2> tmp = [];
        
        for (double t = 0, i = 0; i < steps; i++, t = 1f/steps*i) { 
            tmp.Add(GetCasteljauPoint(points.Length-1, 0, t));
        }

        return [.. tmp];
    }

    public static bool GeoStackEq(bool[] stc1, bool[] stc2)
    {
        if (stc1.Length != stc2.Length) return false;

        for (var i = 0; i < stc1.Length; i++)
        {
            if (stc1[i] != stc2[i]) return false;
        }

        return true;
    }
}