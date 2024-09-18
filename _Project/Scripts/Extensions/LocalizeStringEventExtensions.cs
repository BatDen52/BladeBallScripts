using UnityEngine.Localization.Components;

namespace _Project
{
    public static class LocalizeStringEventExtensions
    {
        public static void UpdateArgument<T>(this LocalizeStringEvent localizeStringEvent, T value)
        {
            if (localizeStringEvent.StringReference.Arguments == null)
                localizeStringEvent.StringReference.Arguments = new object[] { value };
            else
                localizeStringEvent.StringReference.Arguments[0] = value;
            
            localizeStringEvent.RefreshString();
        }

        public static void UpdateArguments<T>(this LocalizeStringEvent localizeStringEvent, params T[] values)
        {
            object[] arguments = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
                arguments[i] = values[i];
            
            localizeStringEvent.StringReference.Arguments = arguments;
            localizeStringEvent.RefreshString();
        }
        
        
    }
}