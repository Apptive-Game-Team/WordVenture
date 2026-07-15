using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Stage
{


    [Serializable]
    public struct BattleWaveData
    {
        public List<EnemySpawnData> enemySpawnDatasInWave;
    }

    [Serializable]
    public struct EnemySpawnData{
        public float SpawnPositionX;
        public int EnemyId;
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
