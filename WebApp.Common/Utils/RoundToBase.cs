using System;

namespace WebApp.Common.Utils
{
    public static class RoundToBase
    {
        public static long RoundOff(this long number, int _base)
        {
            int mid = _base % 2 == 0 ? _base / 2 : _base / 2 + 1;
            var rem = number % _base;
            if (rem < mid)
            {
                return number / _base * _base;
            }
            else
            {
                return (number / _base + 1) * _base;
            }
        }
    }
}