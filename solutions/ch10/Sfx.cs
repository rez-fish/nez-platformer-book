using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Nez;

namespace Spirefall
{
    /// <summary>
    /// Central sound board: load once per scene, play from anywhere.
    /// Sounds are plain .wav files under Content/Audio/, loaded with
    /// SoundEffect.FromStream — no content-pipeline build step needed.
    /// (XNA's Content.Load would want compiled .xnb files; FromStream keeps
    /// the book's "edit a file, re-run" loop intact.)
    /// </summary>
    public static class Sfx
    {
        static SoundEffect _jump;
        static SoundEffect _land;
        static SoundEffect _dash;

        public static void Load()
        {
            _jump = LoadWav("jump");
            _land = LoadWav("land");
            _dash = LoadWav("dash");
        }

        static SoundEffect LoadWav(string name)
        {
            // TitleContainer resolves paths relative to the exe folder;
            // the csproj's Content/** rule puts the wavs there.
            var stream = TitleContainer.OpenStream($"Content/Audio/{name}.wav");
            return SoundEffect.FromStream(stream);
        }

        public static void PlayJump() => _jump?.Play();
        public static void PlayLand() => _land?.Play();
        public static void PlayDash() => _dash?.Play();
    }
}
