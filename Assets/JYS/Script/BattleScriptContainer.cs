using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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
