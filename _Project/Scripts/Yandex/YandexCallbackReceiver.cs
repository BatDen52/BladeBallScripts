using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using VContainer;

namespace _Project
{
    public class YandexCallbackReceiver : MonoBehaviour
    {
        private IAds _ads;
        private IPersistentDataService _persistentDataService;

        [Inject]
        private void Construct(IAds ads, IPersistentDataService persistentDataService)
        {
            _ads = ads;
            _persistentDataService = persistentDataService;
        }

        public void OnInterstitialFinished()
        {
            _ads.OnInterstitialFinished();
        }

        public void OnRewardedFinished()
        {
            _ads.OnRewardedFinished();
        }

        public void OnRewardedClose()
        {
            _ads.OnRewardedClose();
        }

        public void OnRewardedError()
        {
            _ads.OnRewardedError();
        }
        
        public void LoadPlayerDataFromYandex(string json)
        {
            _persistentDataService.LoadFromJson(json);
        }

        public void SetLanguage()
        {
#if UNITY_WEBGL

#pragma warning disable 0168
            try
            {
                string language = Yandex.SDK.GetLang();
                StartCoroutine(LoadLocale(language));
            }
            catch (Exception e)
            {
            }
#pragma warning restore 0168

#endif
        }

        private IEnumerator LoadLocale(string languageIdentifier)
        {
            yield return LocalizationSettings.InitializationOperation;
            LocaleIdentifier localeCode = new LocaleIdentifier(languageIdentifier);
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                Locale locale = LocalizationSettings.AvailableLocales.Locales[i];
                LocaleIdentifier identifier = locale.Identifier;
                if (identifier == localeCode)
                {
                    LocalizationSettings.SelectedLocale = locale;
                }
            }
        }
    }
}