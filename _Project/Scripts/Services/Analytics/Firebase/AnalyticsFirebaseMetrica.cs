using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Data.Persistent;
using _Project.Timers;
using VContainer;

namespace _Project
{
    public class AnalyticsFirebaseMetrica : IAnalytics
    {

        private readonly IPersistentDataService _persistentDataService;
        private readonly TimeInvoker _timeInvoker;
        
        private PlayerData _playerData;
        
        public AnalyticsFirebaseMetrica(IObjectResolver container)
        {
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _timeInvoker = container.Resolve<TimeInvoker>();
        }
        
        public void Initialize()
        {
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _timeInvoker.OnOneSyncedSecondUnscaledTickedEvent += OnSecondTicked;
        }
        
        private void OnSecondTicked()
        {
            _playerData.TotalPlaytime.Value += 1;
            _playerData.CurrentSessionPlaytime.Value += 1;
            _playerData.CurrentDatePlaytime.Value += 1;
            int totalPlaytimeMinutes = _playerData.TotalPlaytime.Value / 60;
            if (totalPlaytimeMinutes > _playerData.TotalPlaytimeMinutes)
            {
                _playerData.TotalPlaytimeMinutes = totalPlaytimeMinutes;
                LogEvent(
                    AnalyticsEvents.total_playtime,
                    (AnalyticsParameters.minutes_total, _playerData.TotalPlaytimeMinutes)
                );
            }
        }
        
        public void LogEvent(string name, params (string Key, object Value)[] parameters)
        {
            List<(string Key, object Value)> parametersList = AddUniversalParameters(parameters);
            Dictionary<string, object> parameterDictionary = parametersList.ToDictionary(item => item.Key, item => item.Value);
            
            AppMetrica.Instance.ReportEvent(name, parameterDictionary);
            if (name == AnalyticsEvents.level_start || name == AnalyticsEvents.level_complete)
            {
                AppMetrica.Instance.SendEventsBuffer();
            }
            
            List<Firebase.Analytics.Parameter> firebaseParameters = new List<Firebase.Analytics.Parameter>();

            foreach ((string key, object value) in parametersList)
            {
                if (value is string stringValue)
                {
                    Firebase.Analytics.Parameter firebaseParameter = new Firebase.Analytics.Parameter(key, stringValue);
                    firebaseParameters.Add(firebaseParameter);
                }
                else if (value is int intValue)
                {
                    Firebase.Analytics.Parameter firebaseParameter = new Firebase.Analytics.Parameter(key, intValue);
                    firebaseParameters.Add(firebaseParameter);
                }
                else if (value is double doubleValue)
                {
                    Firebase.Analytics.Parameter firebaseParameter = new Firebase.Analytics.Parameter(key, doubleValue);
                    firebaseParameters.Add(firebaseParameter);
                }
                else if (value is long longValue)
                {
                    Firebase.Analytics.Parameter firebaseParameter = new Firebase.Analytics.Parameter(key, longValue);
                    firebaseParameters.Add(firebaseParameter);
                }
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(name, firebaseParameters.ToArray());
        }
        
        private List<(string Key, object Value)> AddUniversalParameters((string Key, object Value)[] parameters = null)
        {
            List<(string Key, object Value)> parametersList = new List<(string Key, object Value)>
            {
                (AnalyticsParameters.days_in_game, (DateTime.Now - _playerData.FirstSessionDateTime).Days),
                (AnalyticsParameters.total_playtime_sec, _playerData.TotalPlaytime.Value),
                (AnalyticsParameters.session_playtime_sec, _playerData.CurrentSessionPlaytime.Value),
                (AnalyticsParameters.total_playtime_min, (int)(_playerData.TotalPlaytime.Value / 60)),
                (AnalyticsParameters.session_number, _playerData.SessionNumber),
                (AnalyticsParameters.scene, _playerData.CurrentScene),
                (AnalyticsParameters.user_level, _playerData.RoundsCount),
                (AnalyticsParameters.last_level, _playerData.RoundsCount - 1)
            };
            parametersList.AddRange(parameters);

            return parametersList;
        }
    }
}