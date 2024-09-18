using System;
using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.StateMachine;
using _Project.UI;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using static _Project.GameplayStateConfig;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace _Project
{
    public class GameplayState : IPayloadedState<GameplayStateConfig>
    {
        private readonly IUIService _uiService;
        private readonly IInput _input;
        private readonly IGameFactory _gameFactory;
        private readonly ICameraService _cameraService;
        private readonly IPersistentDataService _persistentDataService;
        private readonly StaticData _staticData;
        private readonly IAudio _audio;
        private readonly IAds _ads;
        private readonly IAnalytics _analytics;

        private PlayerData _playerData;
        private SpawnPoints _spawnPoints;
        private bool _isRoundFinished;
        private int _charactersCount;
        private DateTime _roundStartDateTime;

        private GameplayStateConfig _stateConfig;

        public IntReactiveProperty PlayerDuelScore = new IntReactiveProperty();
        public IntReactiveProperty BotsDuelScore = new IntReactiveProperty();

        public IGameStateMachine GameStateMachine { get; set; }

        [Inject]
        public GameplayState(IObjectResolver container)
        {
            _uiService = container.Resolve<IUIService>();
            _input = container.Resolve<IInput>();
            _gameFactory = container.Resolve<IGameFactory>();
            _cameraService = container.Resolve<ICameraService>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
            _audio = container.Resolve<IAudio>();
            _ads = container.Resolve<IAds>();
            _analytics = container.Resolve<IAnalytics>();
        }

        public void Enter(GameplayStateConfig payload)
        {
            PlayerDuelScore.Value = 0;
            BotsDuelScore.Value = 0;

            _stateConfig = payload;
            _playerData = _persistentDataService.PersistentData.PlayerData;
            _isRoundFinished = false;

            _spawnPoints = GetSpawnPoints();
            _spawnPoints.Points.Shuffle();

            // _uiService.CreateHUD();
            _uiService.HUD.HideLobbyButtons();
            _uiService.HUD.EnableActionsButtons();

            _audio.SetAudioListenerActive(false);
            _cameraService.EnableCharacterCamera();
            _uiService.HideLoadingScreen();
            SetInput();
            _playerData.StartRoundCount++;


            StartRound(_stateConfig);
            _roundStartDateTime = DateTime.Now;

            switch (_stateConfig.Kind)
            {
                case (BattleKind.CLASSIC):
                    _uiService.HUD.HideDuelUI();
                    _playerData.StartRoundDeathmatch++;
                    _playerData.LastRoundMode = "deathmatch";
                    _analytics.LogEvent(AnalyticsEvents.level_start,
                            (AnalyticsParameters.level, _playerData.StartRoundDeathmatch),
                            (AnalyticsParameters.mode, _playerData.LastRoundMode)
                        );
                    break;
                case (BattleKind.DUEL_1_1):
                    _uiService.HUD.ConfigureDuelHUD(this);
                    _playerData.StartRoundDuel_1_1++;
                    _playerData.LastRoundMode = "duel_1_1";
                    _analytics.LogEvent(AnalyticsEvents.level_start,
                        (AnalyticsParameters.level, _playerData.StartRoundDuel_1_1),
                        (AnalyticsParameters.mode, _playerData.LastRoundMode)
                    );
                    break;
                default:
                    throw new System.InvalidOperationException("Unknown battle kind! Can't initialize HUD!");
            }

            _persistentDataService.Save();

        }

        private int GetCharactersCount(GameplayStateConfig stateConfig)
        {
            int charactersCount = 0;
            if (stateConfig.Kind == GameplayStateConfig.BattleKind.CLASSIC)
            {
                charactersCount = Random.Range(_staticData.Settings.MinPlayers, _staticData.Settings.MaxPlayers + 1);
            }
            else
            if (stateConfig.Kind == GameplayStateConfig.BattleKind.DUEL_1_1)
            {
                charactersCount = 2;
            }
            else
            {
                throw new System.NotImplementedException("UNKNOWN GAMEPLAY KIND!");
            }

            return charactersCount;
        }

        private void StartRound(GameplayStateConfig stateConfig)
        {
            Queue<Transform> spawnPointsQueue = null;

            switch (stateConfig.Kind)
            {
                case BattleKind.CLASSIC:
                    spawnPointsQueue = new Queue<Transform>(_spawnPoints.Points);
                    break;
                case BattleKind.DUEL_1_1:
                    spawnPointsQueue = new Queue<Transform>(_spawnPoints.Points);
                    break;
            }

            _charactersCount = GetCharactersCount(stateConfig);
            int botsCount = _charactersCount - 1;

            var player = SpawnPlayer(_gameFactory, spawnPointsQueue);
            var bots = SpawnBots(_gameFactory, spawnPointsQueue, botsCount);
            SpawnBladeBall(_gameFactory, _spawnPoints, player, bots);
        }

        private void ReStartRound(GameplayStateConfig stateConfig)
        {
            Timing.RunCoroutine(_ReStartRound(stateConfig));
        }

        private IEnumerator<float> _ReStartRound(GameplayStateConfig stateConfig)
        {
            yield return Timing.WaitForSeconds(_staticData.Settings.DelayAfterAllKilled);

            Queue<Transform> spawnPointsQueue = null;

            switch (stateConfig.Kind)
            {
                case BattleKind.CLASSIC:
                    spawnPointsQueue = new Queue<Transform>(_spawnPoints.Points);
                    break;
                case BattleKind.DUEL_1_1:
                    spawnPointsQueue = new Queue<Transform>(_spawnPoints.Points);
                    break;
            }

            _charactersCount = GetCharactersCount(stateConfig);
            int botsCount = _charactersCount - 1;

            int playerScore = _gameFactory.Player.ScorePoints;
            _audio.SetAudioListenerActive(false);
            var player = SpawnPlayer(_gameFactory, spawnPointsQueue);
            player.ScorePoints = playerScore;

            var bots = ReSpawnBots(_gameFactory, spawnPointsQueue);
            SpawnBladeBall(_gameFactory, _spawnPoints, player, bots);
        }

        private void SetInput()
        {
            _input.Enable();
            _input.EnableActionsInputs();
            _input.UpdateRotationLock();
        }

        public void Exit()
        {
            _playerData.RoundsCount++;
            _persistentDataService.Save();
        }

        private SpawnPoints GetSpawnPoints()
        {
            LifetimeScope sceneScope = Object.FindObjectOfType<LifetimeScope>();
            SpawnPoints result = sceneScope.Container.Resolve<SpawnPoints>();
            return result;
        }

        private void SpawnBladeBall(IGameFactory gameFactory, SpawnPoints spawnPoints, ICharacter player, List<Bot> bots)
        {
            CleanupBladeball(gameFactory);
            var bladeBall = gameFactory.CreateBladeBall(spawnPoints.BladeBallSpawnPoint.position);
            foreach (Bot bot in bots)
            {
                bladeBall.Targets.Add(bot.GetComponent<Character>());
                bot.BladeBall = bladeBall;
            }

            bladeBall.Targets.Add(player);
            bladeBall.AllCharactersKilled += AllCharactersKilled;
            bladeBall.KillWithBlock += OnKillWithBlock;
            bladeBall.Kill += OnKill;

            float respawnTimeSeconds = bladeBall.RespawnTime;
            _uiService.HUD.StartCountdown(respawnTimeSeconds);
        }

        private void CleanupBladeball(IGameFactory gameFactory)
        {
            if (gameFactory.BladeBall != null)
            {
                gameFactory.BladeBall.AllCharactersKilled -= AllCharactersKilled;
                gameFactory.BladeBall.KillWithBlock -= OnKillWithBlock;
                gameFactory.DestroyBladeBall();
            }
        }

        private void CleanUp()
        {
            _cameraService.DisableCharacterCamera();
            _uiService.CloseRewardedRoundFinished();
            _uiService.HUD.UnsubscribeFromCharacter();
            _uiService.HUD.HideDuelUI();
            _uiService.ShowLoadingScreen();
            CleanupBladeball(_gameFactory);
            _gameFactory.Player.Died -= PlayerHasDied;
            _gameFactory.Player.DieByAccident -= CharacterDieByAccident;

            foreach( Bot bot in _gameFactory.Bots) 
            {
                bot.Character.DieByAccident -= CharacterDieByAccident;
            }
            
            _gameFactory.CleanUp();
            EnterNextState();
        }

        private void OnKillWithBlock(ICharacter character)
        {
            if (character.IsLocalPlayer)
            {
                _playerData.KillsCount++;
                _analytics.LogEvent(AnalyticsEvents.kill,
                    (AnalyticsParameters.count, _playerData.KillsCount),
                    (AnalyticsParameters.mode, _playerData.LastRoundMode));
                
                _playerData.LastRoundReward += _staticData.Settings.CoinsForKill;
                _persistentDataService.Save();
                _audio.PlaySound(_audio.Sounds.KillByPlayer);

                if (_gameFactory.BladeBall.Targets.Count > 1)
                {
                    _uiService.HUD.ShowKillText();
                }
            }
        }

        private void OnKill()
        {
            _charactersCount -= 1;
            if (_charactersCount == 2)
            {
                _audio.PlaySound(_audio.Sounds.Standoff);
                _uiService.HUD.ShowStandoffText();
            }
        }

        private void CharacterDieByAccident(ICharacter character)
        {
            if (character.IsLocalPlayer)
            {
                _analytics.LogEvent(AnalyticsEvents.death_accident);
                PlayerHasDied();
            }
            else
            {
                OnKill();
                _gameFactory.BladeBall.CharacterDied(character);
            }
        }

        private void AllCharactersKilled(ICharacter lastStandingCharacter)
        {
            if (lastStandingCharacter.IsLocalPlayer)
            {
                lastStandingCharacter.AddScorePoints(1);
                PlayerDuelScore.Value = lastStandingCharacter.ScorePoints;
            }
            else
            {
                _audio.SetAudioListenerActive(true, _gameFactory.Player.transform.position);
            }

            if (lastStandingCharacter.ScorePoints < _stateConfig.ScoreToWin)
            {
                //In duel on every round end player kills someone so
                //we force message display
                if (lastStandingCharacter.IsLocalPlayer &&
                    _stateConfig.Kind == BattleKind.DUEL_1_1)
                {
                    _uiService.HUD.ShowKillText();
                }
                ReStartRound(_stateConfig);
                return;
            }

            FinishRound(lastStandingCharacter);
        }

        private void PlayerHasDied()
        {
            if (_gameFactory.BladeBall.Targets.Count == 1)
            {
                var lastStandingCharacter = _gameFactory.BladeBall.Targets[0];
                lastStandingCharacter.AddScorePoints(1);
                BotsDuelScore.Value = lastStandingCharacter.ScorePoints;

                if (lastStandingCharacter.ScorePoints < _stateConfig.ScoreToWin)
                {
                    //ReStartRound(_stateConfig);
                    return;
                }
            }
            FinishRound(null);
        }

        private void FinishRound(ICharacter lastStandingCharacter)
        {
            if (_isRoundFinished)
            {
                return;
            }
            
            _isRoundFinished = true;

            if (lastStandingCharacter == null || !lastStandingCharacter.IsLocalPlayer)
            {
                _playerData.DeathCount++;
                _persistentDataService.Save();

                int deathCount = 0;
                if (_stateConfig.Kind == BattleKind.CLASSIC)
                {
                    _playerData.DeathCountClassic++;
                    deathCount = _playerData.DeathCountClassic;
                }
                if (_stateConfig.Kind == BattleKind.DUEL_1_1)
                {
                    _playerData.DeathCountDuel_1_1++;
                    deathCount = _playerData.DeathCountDuel_1_1;
                }
                _analytics.LogEvent(AnalyticsEvents.death,
                    (AnalyticsParameters.count, deathCount),
                    (AnalyticsParameters.mode, _playerData.LastRoundMode));
                _analytics.LogEvent(AnalyticsEvents.level_fail,
                    (AnalyticsParameters.level, _playerData.RoundsCount),
                    (AnalyticsParameters.time_spent, (DateTime.Now - _roundStartDateTime).Seconds),
                    (AnalyticsParameters.mode, _playerData.LastRoundMode)
                );
                _audio.SetAudioListenerActive(true, _gameFactory.Player.transform.position);
            }
            else
            {
                _playerData.VictoriesCount++;

                _analytics.LogEvent(AnalyticsEvents.level_complete,
                    (AnalyticsParameters.level, _playerData.RoundsCount),
                    (AnalyticsParameters.time_spent, (DateTime.Now - _roundStartDateTime).Seconds),
                    (AnalyticsParameters.mode, _playerData.LastRoundMode)
                );
                
                _uiService.HUD.ShowWinText();
                _audio.PlaySound(_audio.Sounds.Finish);
                _playerData.LastRoundReward += _staticData.Settings.CoinsForWin;
                _persistentDataService.Save();
            }

            _input.Disable();
            Timing.RunCoroutine(_FinishRound());
        }

        private IEnumerator<float> _FinishRound()
        {
            yield return Timing.WaitForSeconds(_staticData.Settings.DelayAfterAllKilled);
            if (_playerData.LastRoundReward > 0)
            {
                ProposeRewarded();
            }
            else
            {
                CleanUp();
            }
        }

        private void ProposeRewarded()
        {
            _uiService.ShowRewardedRoundFinished(
                _playerData.LastRoundReward,
                onAccept: OnRewardedAccept,
                onDecline: OnRewardedDecline
                );
        }

        private void OnRewardedAccept()
        {
            // _analytics.LogEvent(AnalyticsEvents.REWARD_REQUEST, _playerData.LastRoundReward);
            _ads.ShowRewarded(placement: "deathmatch", onSuccess: () =>
            {
                _playerData.LastRoundReward *= 3;
                _persistentDataService.Save();
                CleanUp();
            });
        }

        private void OnRewardedDecline()
        {
            CleanUp();
        }
        
        
        
        private void EnterNextState()
        {
            GameStateMachine.Enter<LobbyState>();
        }

        private Character SpawnPlayer(IGameFactory gameFactory, Queue<Transform> spawnPointsQueue)
        {
            Transform spawnPoint = spawnPointsQueue.Dequeue();
            gameFactory.DestroyPlayer();
            Character player = gameFactory.CreatePlayer(spawnPoint.position, spawnPoint.rotation);
            player.Died += PlayerHasDied;
            player.IsLocalPlayer = true;
            player.DieByAccident += CharacterDieByAccident;
            _cameraService.SetCharacterCameraTarget(player.CameraTarget);
            _uiService.HUD.SubscribeToCharacter(player);
            return player;
        }

        private List<Bot> SpawnBots(IGameFactory factory, Queue<Transform> spawnPointsQueue, int botsCount)
        {
            for (int i = 0; i < botsCount; i++)
            {
                Transform spawnPoint = spawnPointsQueue.Dequeue();
                var bot = factory.CreateBot(spawnPoint.position, spawnPoint.rotation, _staticData.Settings.BotConfigs);
                var character = bot.GetComponent<Character>();
                character.DieByAccident += CharacterDieByAccident;
            }

            return factory.Bots;
        }

        private List<Bot> ReSpawnBots(IGameFactory factory, Queue<Transform> spawnPointsQueue)
        {
            var bots = factory.Bots.ToArray(); //make intentional copy
            for (int i = 0; i < bots.Length; i++)
            {
                var bot = bots[i];
                var config = bot.Config;
                int weaponId = bot.WeaponId;
                int score = bot.ScorePoints;
                var skill = bot.UsedSkillPrefab;
                var skin = bot.UsedSkinPrefab;

                Transform spawnPoint = spawnPointsQueue.Dequeue();
                factory.DestroyBot(bot);
                var newBot = factory.CreateBot(spawnPoint.position, spawnPoint.rotation, config, weaponId, skin, skill);
                var character = newBot.GetComponent<Character>();
                character.DieByAccident += CharacterDieByAccident;
                newBot.ScorePoints = score;
            }

            return factory.Bots;
        }
    }
}