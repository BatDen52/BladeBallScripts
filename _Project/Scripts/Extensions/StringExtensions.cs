using System;

namespace _Project
{
    public static class StringExtensions
    {
        public static int TryParseInt(this string s, int defaultValue = 0) => 
            Int32.TryParse(s, out int i) ? i : defaultValue;
        
        public static long TryParseLong(this string s, long defaultValue = 0) => 
            long.TryParse(s, out long i) ? i : defaultValue;
        
    }
}