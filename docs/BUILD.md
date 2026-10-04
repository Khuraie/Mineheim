# Building Mineheim

## Prerequisites

- .NET 6 SDK
- Valheim (current stable Steam build - pinned in `build.txt`)
- BepInEx 5.4.x (x64) installed into the Valheim folder
- Jotunn installed into `BepInEx/plugins`

Mineheim compiles against the DLLs of the local Valheim install. Game, Unity,
and library DLLs are reference-only: they are never copied to the output and
never bundled (PROJECT.md "Legal").

## Build

```bash
dotnet build -c Release
# Output: bin/Release/net472/Mineheim.dll
```

Mineheim targets `net472` (like Jotunn): BepInEx 5.4 loads plugins into Unity
Mono (CLR 4.x), which cannot load `net6.0` assemblies. The build still uses
the .NET 6+ SDK; `Microsoft.NETFramework.ReferenceAssemblies` supplies the
reference assemblies so no Visual Studio targeting pack is needed. See the
`TODO(spec)` in `Mineheim.csproj` - PROJECT.md's stated output path needs to be
aligned with this.

If Valheim lives somewhere unusual, point the build at it:

```bash
dotnet build -c Release -p:ValheimDir="D:/Games/Valheim"
# or
set VALHEIM_DIR=D:\Games\Valheim
dotnet build -c Release
```

## Install

Copy `Mineheim.dll` into `<Valheim>/BepInEx/plugins` and launch Valheim.

## Verify (M1)

1. Launch Valheim once; `BepInEx/LogOutput.log` must contain
   `Mineheim v0.1.0 loaded` and no Mineheim exceptions.
2. Load a fresh world (Meadows, day 1).
3. Press F5 while standing still: the log prints `Mineheim mode: ON`.
4. Press F5 while moving or in combat: the toggle is refused with a reason.

Full milestone checklists live in `docs/MILESTONES.md`.
