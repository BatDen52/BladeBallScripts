using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project
{
    public static class LocalizedStringExtensions
    {
        public static Task<string> GetLocalizedStringAsync2(this LocalizedString localizedString)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(localizedString.TableReference, localizedString.TableEntryReference);
            operation.Completed += (AsyncOperationHandle<string> handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    taskCompletionSource.SetResult(handle.Result);
                }
                else
                {
                    taskCompletionSource.SetException(handle.OperationException);
                }
            };
            return taskCompletionSource.Task;
        }
    }
}