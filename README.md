# Henry's Leditor
An alternative tool to the Rain World's official level editor.

## Work in Progress

This project is still work-in-progress (WIP). Expect bugs and missing features, and when you do find them, report them here.

## Build From Source

### Step One: Download the source code
Clone the repository

```bash
git clone --recursive https://github.com/HenryMarkle/Leditor.git
```

### Step Two: Build the renderer

Move to the newly cloned repo

```bash
cd Leditor
```

Then into the `/Drizzle` directory

```bash
cd Drizzle
```

Run the two commands in sequence:

```bash
dotnet run --project Drizzle.Transpiler
```
```bash
dotnet build --configuration Release
```

Go back to the top level.

```bash
cd ..
```

### Step Three: Build the entire solution

Run the command at the top level of the solution:
```bash
dotnet build --configuration Release
```

### Step Four: Copy the assets

Finally, copy the `/assets` folder to the newly built's directory.
Which is usually placed in `/Leditor/bin/Release/net8.0/`.
