using System;
using System.Collections.Generic;

namespace _Project
{
    public interface IRandomService
    {
        T WeightedChoice<T>(List<Tuple<T, int>> choices);
    }
}