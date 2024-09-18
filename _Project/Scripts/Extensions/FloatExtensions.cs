using UnityEngine;

namespace _Project
{
    public static class FloatExtensions
    {
        public static string ToTimeString(this float timeInSeconds, string format = "{0:00}:{1:00}")
        {
            int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60F);
            int seconds = Mathf.RoundToInt(timeInSeconds % 60F);
            return string.Format(format, minutes, seconds);
        }
        
        public static string ToTimeStringWithHours(this float timeInSeconds, string format = "{0:00}:{1:00}::{2:00}")
        {
            int hours = Mathf.FloorToInt(timeInSeconds / 3600F);
            int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60F);
            int seconds = Mathf.RoundToInt(timeInSeconds % 60F);
            return string.Format(format, hours, minutes, seconds);
        }
    }
}