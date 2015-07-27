using System;
using System.Text;

namespace FirebaseSharp.Portable
{
    internal class FirebasePushIdGenerator
    {
        // Modeled after base64 web-safe chars, but ordered by ASCII.
        private const string PushCharsString = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        private static readonly char[] PushChars;
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private readonly Random _rng = new Random();

        // Timestamp of last push, used to prevent local collisions if you push twice in one ms.
        private long _lastPushTime;
        private readonly byte[] _lastRandChars = new byte[12];

        static FirebasePushIdGenerator()
        {
            PushChars = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(PushCharsString));
        }

        public string Next()
        {
            // We generate 72-bits of randomness which get turned into 12 characters and
            // appended to the timestamp to prevent collisions with other clients. We store the last
            // characters we generated because in the event of a collision, we'll use those same
            // characters except "incremented" by one.
            StringBuilder id = new StringBuilder(20);

            long now = (long) (DateTimeOffset.Now - Epoch).TotalMilliseconds;
            bool duplicateTime = (now == _lastPushTime);
            _lastPushTime = now;

            char[] timeStampChars = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                int index = (int) (now%PushChars.Length);
                timeStampChars[i] = PushChars[index];
                now = (long) Math.Floor((double) now/PushChars.Length);
            }
            if (now != 0)
            {
                throw new Exception("We should have converted the entire timestamp.");
            }

            id.Append(timeStampChars);

            if (!duplicateTime)
            {
                for (int i = 0; i < 12; i++)
                {
                    _lastRandChars[i] = (byte) _rng.Next(0, PushChars.Length);
                }
            }
            else
            {
                // If the timestamp hasn't changed since last push, use the same random number,
                //except incremented by 1.
                int lastIndex = 11;
                for (; lastIndex >= 0 && _lastRandChars[lastIndex] == PushChars.Length - 1; lastIndex--)
                {
                    _lastRandChars[lastIndex] = 0;
                }
                _lastRandChars[lastIndex]++;
            }

            for (int i = 0; i < 12; i++)
            {
                id.Append(PushChars[_lastRandChars[i]]);
            }

            if (id.Length != 20)
            {
                throw new Exception("Length should be 20.");
            }

            return id.ToString();
        }
    }
}