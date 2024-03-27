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
git clone --branch embedded https://github.com/pkhead/Drizzle
```

Note: Make sure the build is in `release` mode.

### Step Two: Build the renderer

Inside the /Drizzle directory, run the two commands in sequence:

```bash
dotnet run --project Drizzle.Transpiler
```
```bash
dotnet build
```

### Step Three: Build the entire solution

Run the command at the top level of the solution:
```bash
dotnet build
```

Note: If you want to build the project in `release` mode, you need to copy the `assets` folder to the build folder.
