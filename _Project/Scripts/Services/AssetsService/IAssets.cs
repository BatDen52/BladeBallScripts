using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

namespace _Project
{
    public interface IAssets
    {
        void Initialize();
        Task<T> Load<T>(AssetReference assetReference) where T : class;
        Task<T> Load<T>(string address) where T : class;
        void Cleanup();
    }
}