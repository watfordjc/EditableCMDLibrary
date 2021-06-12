using System;
using System.IO;
using System.Media;
using System.Runtime.Versioning;

namespace uk.JohnCook.dotnet.EditableCMDLibrary.Utils
{
    /// <summary>
    /// Helper methods for sounds
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class SoundUtils
    {
        /// <summary>
        /// Plays an error "beep". Uses the Windows Background sound if it exists, or sends a beep through the PC (motherboard) speaker.
        /// </summary>
        /// <param name="soundPlayer">The <see cref="SoundPlayer"/> instance to be used to play the audio.</param>
        public static void Beep(SoundPlayer soundPlayer)
        {
            string errorSound = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + Path.DirectorySeparatorChar + "Media" + Path.DirectorySeparatorChar + "Windows Background.wav";
            if (File.Exists(errorSound))
            {
                soundPlayer.SoundLocation = errorSound;
                soundPlayer.Play();
            }
            else
            {
                Console.Beep();
            }
        }
    }
}
