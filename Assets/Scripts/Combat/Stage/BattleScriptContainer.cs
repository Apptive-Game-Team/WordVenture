using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Stage
{


    [Serializable]
    public struct BattleWaveData
    {
        public List<EnemySpawnData> enemySpawnDatasInWave;
    }

    [Serializable]
    public struct EnemySpawnData{
        [FormerlySerializedAs("SpawnPositionX")] public float spawnPositionX;
        [FormerlySerializedAs("EnemyId")] public int enemyId;
    }



    [CreateAssetMenu]
    public class BattleScriptContainer : ScriptableObject
    {
        [SerializeField] List<BattleWaveData> battleWaveDatas;

        public List<BattleWaveData> GetBattleWaveDatas()
        {
            return battleWaveDatas;
        }
    }

}
