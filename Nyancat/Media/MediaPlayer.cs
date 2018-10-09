using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nyancat.Media
{
    public class MediaPlayer
    {
        private Stream _stream = null;
        private byte[] _streamData = null;

        public MediaPlayer(Stream stream)
        {
            _stream = stream;
        }

        private void LoadAndPlay(SoundFlags flags)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            if (_streamData == null)
            {
                int streamLen = (int)_stream.Length;
                _streamData = new byte[streamLen];
                _stream.Read(_streamData, 0, streamLen);
            }

            Winmm.PlaySound(_streamData, IntPtr.Zero, SoundFlags.SND_MEMORY | SoundFlags.SND_NODEFAULT | flags);
        }

        public void Play()
        {
            LoadAndPlay(SoundFlags.SND_ASYNC);
        }

        public void PlayLoop()
        {
            LoadAndPlay(SoundFlags.SND_LOOP | SoundFlags.SND_ASYNC);
        }

        public void Stop()
        {
            Winmm.PlaySound((byte[])null, IntPtr.Zero, SoundFlags.SND_PURGE);
        }
        
        // http://www.pinvoke.net/default.aspx/winmm.PlaySound
        // https://msdn.microsoft.com/en-us/library/windows/desktop/dd743680%28v=vs.85%29.aspx?f=255&MSPPError=-2147217396

        [Flags]
        public enum SoundFlags : uint
        {
            /// <summary>play synchronously (default)</summary>
            SND_SYNC = 0x0000,
            /// <summary>play asynchronously</summary>
            SND_ASYNC = 0x0001,
            /// <summary>silence (!default) if sound not found</summary>
            SND_NODEFAULT = 0x0002,
            /// <summary>pszSound points to a memory file</summary>
            SND_MEMORY = 0x0004,
            /// <summary>loop the sound until next sndPlaySound</summary>
            SND_LOOP = 0x0008,
            /// <summary>don't stop any currently playing sound</summary>
            SND_NOSTOP = 0x0010,
            /// <summary>Stop Playing Wave</summary>
            SND_PURGE = 0x40,
            /// <summary>The pszSound parameter is an application-specific alias in the registry. You can combine this flag with the SND_ALIAS or SND_ALIAS_ID flag to specify an application-defined sound alias.</summary>
            SND_APPLICATION = 0x80,
            /// <summary>don't wait if the driver is busy</summary>
            SND_NOWAIT = 0x00002000,
            /// <summary>name is a registry alias</summary>
            SND_ALIAS = 0x00010000,
            /// <summary>alias is a predefined id</summary>
            SND_ALIAS_ID = 0x00110000,
            /// <summary>name is file name</summary>
            SND_FILENAME = 0x00020000,
            /// <summary>name is resource name or atom</summary>
            SND_RESOURCE = 0x00040004
        }

        private class Winmm
        {
            [DllImport("winmm.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
            internal static extern bool PlaySound(byte[] soundName, IntPtr hmod, SoundFlags soundFlags);
        }
    }
}
