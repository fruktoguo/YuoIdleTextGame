using System;

namespace YuoTools.Editor
{
    public static class UIEditor_SingleCheck
    {
        private static long _lastTime = long.MinValue;

        public static bool SingleCheck()
        {
            if (_lastTime.Equals(System.DateTime.Now.Ticks / 10000000))
            {
                return false;
            }

            _lastTime = DateTime.Now.Ticks / 10000000;
            return true;
        }
    }
} 