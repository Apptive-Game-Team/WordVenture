using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using WordVenture.Combat.Enemies;

namespace WordVenture.Combat.Stage
{

    [CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
    public class StageData : ScriptableObject
    {
        public int stageID;
        public string stageName;
        public Sprite background;
        public WaveData waveData;
    }

    [System.Serializable]
    public class WaveData
    {
        public BattleScriptContainer[] enemyWaves;
    }

}
