# Henry's Leditor

An alternative to the current Rain World's official level editor, powered by .NET C# and [Raylib](https://github.com/raysan5/raylib).

## Features

- Better overall performance
- Better controls
- Re-bindable shortcuts
- Better overall experience

## Missing Features

- Light editor's stretch map
- "Move Level" tool in the geometry editor

## Incomplete Functionalities

- Settings Page
- Props editor's copy function
- Rope/Long props placement mechanism

## Level Rendering

This editor is shipped with [Drizzle](https://github.com/SlimeCubed/Drizzle) ([Slime_Cubed's fork](https://github.com/SlimeCubed)). In case you don't see a blue button named "RENDER" on the main page, you need to manually add the renderer in a directory named `/renderer`

## Cross-Platform Support

This project only runs on Windows, at the moment.

## Notes

- Undo functionality is only available for the geometry editor, at the moment

## Building From Source

### Step 1

Clone the repo to a local directory on your machine

```
https://github.com/HenryMarkle/Leditor.git
```

### Step 2

Run the build command

```
dotnet build
```

### Step 3

Extract the assets from `assets.zip` file from the latest release and copy them to the built executable's top directory

### Step 4 (Optional)

Create a directory names `/renderer` and put prebuilt Drizzle in it
