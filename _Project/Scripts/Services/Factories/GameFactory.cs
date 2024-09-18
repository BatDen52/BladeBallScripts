using System;
using System.Collections.Generic;
using _Project.Data.Persistent;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace _Project
{
    public class GameFactory : IGameFactory
    {
        public BladeBall BladeBall { get; private set; }
        public Character Player { get; private set; }
        public List<Bot> Bots { get; } = new List<Bot>();

        private readonly IObjectResolver _container;
        private readonly IAssets _assets;
        private readonly IPersistentDataService _persistentDataService;
        private readonly StaticData _staticData;
        private readonly IRandomService _randomService;

        private readonly BladeBall _bladeBallPrefab;
        private readonly Character _characterPrefab;
        private readonly Bot _botPrefab;


        [Inject]
        public GameFactory(IObjectResolver container)
        {
            _container = container;
            _assets = container.Resolve<IAssets>();
            _persistentDataService = container.Resolve<IPersistentDataService>();
            _staticData = container.Resolve<StaticData>();
            _randomService = container.Resolve<IRandomService>();

            _bladeBallPrefab = container.Resolve<BladeBall>();
            _characterPrefab = container.Resolve<Character>();
            _botPrefab = container.Resolve<Bot>();
        }

        public Character CreatePlayer(Vector3 position, Quaternion rotation)
        {
            if (Player != null)
                throw new InvalidOperationException("Trying to create player while previous still exists! Did you forget to call DestroyPlayer()?");

            PlayerData playerData = _persistentDataService.PersistentData.PlayerData;

            Player = _container.Instantiate(_characterPrefab, position, rotation);
            int validSkinId = playerData.CurrentSkinId < _staticData.Settings.Skins.Count ?
                playerData.CurrentSkinId : 0;
            Player.SetSkin(_staticData.Settings.Skins[validSkinId].Prefab);
            Player.SetWeapon(playerData.CurrentWeaponId);
            int validSkillId = playerData.CurrentSkillId < _staticData.Settings.Skills.Count ?
                            playerData.CurrentSkillId : 0;
            Player.SkillActivator.SetSkill(_staticData.Settings.Skills[validSkillId]);
            return Player;
        }

        public Bot CreateBot(Vector3 position, Quaternion rotation, List<Tuple<BotConfig, int>> botConfigs)
        {
            Bot bot = _container.Instantiate(_botPrefab, position, rotation);
            BotConfig config = _randomService.WeightedChoice(botConfigs);
            bot.SetConfig(config);
            bot.SetRandomSkin();
            bot.SetRandomSkill();
            bot.Character.SetRandomWeapon();
            bot.Character.SetBladeBall(BladeBall);
            Bots.Add(bot);
            return bot;
        }

        public Bot CreateBot(Vector3 position, Quaternion rotation, BotConfig config, int weaponId, Skin skin, Skill skill)
        {
            Bot bot = _container.Instantiate(_botPrefab, position, rotation);
            bot.SetConfig(config);
            bot.SetSkin(skin);
            bot.SetSkill(skill);
            bot.Character.SetWeapon(weaponId);
            bot.Character.SetBladeBall(BladeBall);
            Bots.Add(bot);
            return bot;
        }

        public void DestroyBot(Bot bot)
        {
            if (Bots.Remove(bot))
            {
                Object.Destroy(bot.gameObject);
            }
        }

        public BladeBall CreateBladeBall(Vector3 position)
        {
            if (BladeBall != null)
                throw new InvalidOperationException("Trying to create blade ball while previous still exists! Did you forget to call DestroyBladeBall()?");
            BladeBall = _container.Instantiate(_bladeBallPrefab, position, Quaternion.identity);
            BladeBall.SetPlayer(Player);

            return BladeBall;
        }

        public void CleanUp()
        {
            DestroyPlayer();
            DestroyBots();
            DestroyBladeBall();
        }

        public void DestroyBladeBall()
        {
            if (BladeBall != null)
            {
                Object.Destroy(BladeBall.gameObject);
                BladeBall = null;
            }
        }

        public void DestroyPlayer()
        {
            if (Player != null)
            {
                Object.Destroy(Player.gameObject);
                Player = null;
            }
        }

        public void DestroyBots()
        {
            foreach (Bot bot in Bots)
            {
                Object.Destroy(bot.gameObject);
            }
            Bots.Clear();
        }


    }
}