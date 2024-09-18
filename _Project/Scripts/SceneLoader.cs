using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project
{
    public class SceneLoader
    {
        public void Load(string name, LoadSceneMode mode = LoadSceneMode.Single, Action then = null) =>
            Timing.RunCoroutine(_LoadScene(name, mode, then));
        
        public void LoadAddressable(string name, LoadSceneMode mode = LoadSceneMode.Single, Action<SceneInstance> then = null) =>
            Timing.RunCoroutine(_LoadAddressableScene(name, mode, then));
        
        public void LoadAddressable(AssetReference sceneRef, LoadSceneMode mode = LoadSceneMode.Single, Action<SceneInstance> then = null) =>
            Timing.RunCoroutine(_LoadAddressableScene(sceneRef, mode, then));
        
        public void UnloadAddressable(AsyncOperationHandle handle,
            UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true, Action then = null) =>
            Timing.RunCoroutine(_UnloadAddressableScene(handle, unloadOptions, autoReleaseHandle, then));
        
        public void UnloadAddressable(SceneInstance sceneInstance,
            UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true, Action then = null) =>
            Timing.RunCoroutine(_UnloadAddressableScene(sceneInstance, unloadOptions, autoReleaseHandle, then));
    
        private IEnumerator<float> _LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action then = null)
        {
            if (SceneManager.GetActiveScene().name == sceneName)
            {
                then?.Invoke();
                yield break;
            }
      
            AsyncOperation waitNextScene = SceneManager.LoadSceneAsync(sceneName, mode);

            while (!waitNextScene.isDone)
                yield return Timing.WaitForOneFrame;
      
            then?.Invoke();
        }
        
        private IEnumerator<float> _LoadAddressableScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, Action<SceneInstance> then = null)
        {
            AsyncOperationHandle<SceneInstance> waitNextScene = LoadAddressableScene(sceneName, mode);
            
            while (!waitNextScene.IsDone)
                yield return Timing.WaitForOneFrame;;
      
            then?.Invoke(waitNextScene.Result);
        }
        
        private IEnumerator<float> _LoadAddressableScene(AssetReference sceneRef, LoadSceneMode mode = LoadSceneMode.Single, Action<SceneInstance> then = null)
        {
            AsyncOperationHandle<SceneInstance> waitNextScene = LoadAddressableScene(sceneRef, mode);
            while (!waitNextScene.IsDone)
                yield return Timing.WaitForOneFrame;;
      
            then?.Invoke(waitNextScene.Result);
        }

        private AsyncOperationHandle<SceneInstance> LoadAddressableScene(AssetReference sceneRef, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(sceneRef, mode, activateOnLoad, priority);
        }
        
        private AsyncOperationHandle<SceneInstance> LoadAddressableScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return Addressables.LoadSceneAsync(sceneName, mode, activateOnLoad, priority);
        }

        private IEnumerator<float> _UnloadAddressableScene(AsyncOperationHandle handle,
            UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true, Action then = null)
        {
            AsyncOperationHandle<SceneInstance> waitUnloadScene = UnloadAddressableScene(handle, unloadOptions, autoReleaseHandle);
            while (!waitUnloadScene.IsDone)
                yield return Timing.WaitForOneFrame;;
      
            then?.Invoke();
        }
        
        private IEnumerator<float> _UnloadAddressableScene(SceneInstance sceneInstance, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true, Action then = null)
        {
            AsyncOperationHandle<SceneInstance> waitUnloadScene = UnloadAddressableScene(sceneInstance, unloadOptions, autoReleaseHandle);
            while (!waitUnloadScene.IsDone)
                yield return Timing.WaitForOneFrame;;
      
            then?.Invoke();
        }
        
        private AsyncOperationHandle<SceneInstance> UnloadAddressableScene(AsyncOperationHandle handle, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(handle, unloadOptions, autoReleaseHandle);
        }
        
        private AsyncOperationHandle<SceneInstance> UnloadAddressableScene(SceneInstance sceneInstance, UnloadSceneOptions unloadOptions = UnloadSceneOptions.None, bool autoReleaseHandle = true)
        {
            return Addressables.UnloadSceneAsync(sceneInstance, unloadOptions, autoReleaseHandle);
        }
    }
}