using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project
{
    public class RandomService : IRandomService
    {
        public T WeightedChoice<T>(List<Tuple<T, int>> choices)
        {
            int totalWeight = 0;
            foreach(Tuple<T, int> entry in choices)
            {
                totalWeight += entry.Item2;
            }
            int randomNumber = Random.Range(1, totalWeight + 1);
            int pos = 0;
            for (int i=0; i < choices.Count; i++)
            {
                if(randomNumber <= choices[i].Item2 + pos)
                {
                    return choices[i].Item1;
                }
                pos += choices[i].Item2;
            }

            return choices[0].Item1;
        }
    }
}