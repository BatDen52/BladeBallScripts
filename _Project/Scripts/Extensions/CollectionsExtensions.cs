using System.Collections.Generic;

namespace _Project
{
    public static class CollectionsExtensions
    {
        public static void Shuffle<T>(this List<T> list)  
        {  
            System.Random rng = new System.Random();  
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rng.Next(n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }  
        }
    }
}