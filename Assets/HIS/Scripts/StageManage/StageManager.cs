using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        int stagePosition = StageDataSingleton.Instance.StagePosition;
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
        SetBackground(stageData.background, stagePosition);

        battleWaveController.battleScript = stageData.waveData.enemyWaves[0];
        battleWaveController.Start1();
    }

    void SetBackground(Sprite background, int stagePosition)
    {
        GameObject backgroundObject = GameObject.Find("PlainBackground");
        audioSource.clip = audioClips[stagePosition];
        audioSource.Play();
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
                renderer.sprite = background;
            }
        }
    }
}
