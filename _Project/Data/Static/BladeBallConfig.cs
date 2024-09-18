using UnityEngine;

namespace _Project.Data.Static
{
    [CreateAssetMenu(fileName = "BladeBallConfig", menuName = "_Project/BladeBallConfig", order = 0)]
    public class BladeBallConfig : ScriptableObject
    {
        public float TargetBlockRadius = 1f;
        public float TargetKillRadius = 0.1f;
        public float SpeedIncrement = 1f;
        public Vector3 TargetOffset = Vector3.zero;
        public float RespawnTime = 5f;
        public float StartSpeed = 7f;
        public float PlayerChooseProbability = 1f;
    }
}