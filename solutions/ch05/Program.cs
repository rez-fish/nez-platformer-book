using System;

namespace Spirefall
{
    static class Program
    {
        // STAThread: marks this entry point as single-threaded-apartment.
        // XNA/FNA templates have always carried it; some platform dialogs
        // and COM interop expect it. [unverified: required-ness on FNA]
        [STAThread]
        static void Main(string[] args)
        {
            using var game = new SpirefallGame();
            game.Run();
        }
    }
}
