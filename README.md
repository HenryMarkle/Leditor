# Henry's Leditor
An alternative tool to the Rain World's official level editor.

## Work in Progress

This project is still work-in-progress (WIP). Expect bugs and missing features, and when you do find them, report them here.

## Build From Source

### Step One: Download the source code
Clone the repository

```bash
git clone https://github.com/HenryMarkle/Leditor.git
```

And inside the solution, clone the renderer submodule

```bash
git clone --branch embedded https://github.com/HenryMarkle/Drizzle
```

Note: Make sure the build is in `release` mode.

### Step Two: Build the renderer

Inside the /Drizzle directory, run the two commands in sequence:

```bash
dotnet run --project Drizzle.Transpiler
```
```bash
dotnet build --configuration Release
```

### Step Three: Build the entire solution

Run the command at the top level of the solution:
```bash
dotnet build --configuration Release
```

Finally, copy the `/assets` folder to the newly built's direcroty.
