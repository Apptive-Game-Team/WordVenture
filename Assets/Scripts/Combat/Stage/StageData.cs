using UnityEngine;

namespace Combat.Stage
{

    [CreateAssetMenu(fileName = "StageData", menuName = "ScriptableObjects/StageData", order = 1)]
    public class StageData : ScriptableObject
    {
        public int stageID;
        public string stageName;
        public Sprite background;
        public WaveData waveData;
        // 비워 두면 StageManager의 스테이지 순서 음악을 쓴다.
        public AudioClip music;
    }

    [System.Serializable]
    public class WaveData
    {
        public BattleScriptContainer[] enemyWaves;
    }

}
