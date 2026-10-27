using System.Collections.Generic;
using Combat.Enemies;
using UnityEngine;

namespace Combat.Stage
{

    public class StageManager : MonoBehaviour
    {
        public List<StageData> stageDataList;
        private StageData currentStageData;
        private BattleWaveController battleWaveController;

        [SerializeField] AudioSource audioSource;
        [SerializeField] List<AudioClip> audioClips;

        [SerializeField] GameObject rainyBackground;
        void Start()
        {
            battleWaveController = FindObjectOfType<BattleWaveController>();

            int stagePosition = StageDataSingleton.Instance.stagePosition;
            LoadStage(stagePosition);
            SetupBattle(currentStageData, stagePosition);
        }

        void LoadStage(int stagePosition)
        {
            foreach (var stageData in stageDataList)
            {
                if (stageData.stageID == stagePosition)
                {
                    currentStageData = stageData;
                    break;
                }
            }
        }

        void SetupBattle(StageData stageData, int stagePosition)
        {
            SetBackground(stageData, stagePosition);

            battleWaveController.battleScript = stageData.waveData.enemyWaves[0];
            battleWaveController.Start1();
        }

        void SetBackground(StageData stageData, int stagePosition)
        {
            GameObject backgroundObject = GameObject.Find("PlainBackground");
            PlayMusic(stageData.music != null ? stageData.music : audioClips[stagePosition]);
            if (stagePosition == 3)
            {
                backgroundObject.SetActive(false);
                rainyBackground.SetActive(true);
            }else
            {
                backgroundObject.SetActive(true);
                rainyBackground.SetActive(false);
            }


            if (backgroundObject != null)
            {
                SpriteRenderer renderer = backgroundObject.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = stageData.background;
                }
            }
        }

        public void OnEnemySpawned(Enemy enemy)
        {
            if (enemy is BossEnemy && currentStageData != null && currentStageData.bossMusic != null)
                PlayMusic(currentStageData.bossMusic);
        }

        void PlayMusic(AudioClip clip)
        {
            // 세계수 외피에서 본체로 바뀌어도 같은 보스 곡을 처음부터 다시 틀지 않는다.
            if (audioSource == null || clip == null || audioSource.clip == clip) return;
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.pitch = 1f;
            audioSource.Play();
        }
    }

}
