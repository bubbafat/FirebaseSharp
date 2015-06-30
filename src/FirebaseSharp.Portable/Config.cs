using System;

namespace FirebaseSharp.Portable
{
    static internal class Config
    {
        private static TimeSpan _networkReadTimeout = TimeSpan.FromSeconds(45);

        public static TimeSpan NetworkReadTimeout
        {
            get { return _networkReadTimeout; }
            set { _networkReadTimeout = value; }
        }
    }
}
