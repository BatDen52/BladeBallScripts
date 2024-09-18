using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Data.Static;
using _Project.Data.Static.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project
{
    public interface IGameFactory
    {
        Character CreatePlayer(Vector3 position, Quaternion rotation);
        void DestroyBots();
        BladeBall CreateBladeBall(Vector3 position);
        BladeBall BladeBall { get; }
        Character Player { get; }
        List<Bot> Bots { get; }
        void CleanUp();
        void DestroyPlayer();
        void DestroyBladeBall();
        void DestroyBot(Bot bot);
        Bot CreateBot(Vector3 position, Quaternion rotation, List<Tuple<BotConfig, int>> botConfigs);
        Bot CreateBot(Vector3 position, Quaternion rotation, BotConfig config, int weaponId, Skin skin, Skill skill);
    }
}