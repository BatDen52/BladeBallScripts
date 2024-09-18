using System;

namespace _Project
{
    [Serializable]
    public class QuaternionData
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public QuaternionData(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
    }
}