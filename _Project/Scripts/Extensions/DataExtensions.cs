using Newtonsoft.Json;
using UnityEngine;

namespace _Project
{
    public static class DataExtensions
    {
        public static string ToJson(this object obj) =>
            JsonConvert.SerializeObject(obj);

        public static T FromJson<T>(this string json) =>
            JsonConvert.DeserializeObject<T>(json);

    public static Vector3Data AsVectorData(this Vector3 vector) => 
            new Vector3Data(vector.x, vector.y, vector.z);
    
        public static Vector3 AsUnityVector(this Vector3Data vector3Data) => 
            new Vector3(vector3Data.X, vector3Data.Y, vector3Data.Z);
            
        public static QuaternionData AsQuaternionData(this Quaternion quaternion) => 
            new QuaternionData(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
            
        public static Quaternion AsUnityQuaternion(this QuaternionData quaternionDataData) => 
            new Quaternion(quaternionDataData.X, quaternionDataData.Y, quaternionDataData.Z, quaternionDataData.W);
    }
}