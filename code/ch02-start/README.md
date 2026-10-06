# ch02-start

Project state at the beginning of Chapter 2: the completed Chapter 1.

You have a `Spirefall` project that opens a cornflower-blue window via
`dotnet run`. These files are the reference end-state of Chapter 1
(identical to `/solutions/ch01`):

- `Spirefall.csproj` — net8.0 exe, project references to pinned FNA + Nez
- `Program.cs` — entry point, creates `SpirefallGame`, calls `Run()`
- `SpirefallGame.cs` — `Core` subclass, assigns the first `Scene`

If your own files differ cosmetically, that's fine. If `dotnet run` doesn't
show the window, redo Chapter 1's checkpoint before starting Chapter 2.
