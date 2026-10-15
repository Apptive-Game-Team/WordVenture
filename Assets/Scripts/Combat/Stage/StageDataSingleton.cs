using Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Stage
{

    public class StageDataSingleton : MonoBehaviour
    {
        public static StageDataSingleton Instance { get; private set; }
        [FormerlySerializedAs("StagePosition")] public int stagePosition;
        // 방금 끝낸 전투가 이 스테이지를 처음 깬 것인지. 다시 깬 전투는 진행도, 새 카드, 엔딩을 주지 않는다.
        [System.NonSerialized] public bool isFirstClear;
        // 방금 끝낸 전투의 점수와 받은 마력 조각. 클리어 화면의 결과 화면이 보여 준다.
        // hasResult 가 false 면(클리어 씬을 에디터에서 바로 연 경우) 결과 화면을 띄우지 않는다.
        [System.NonSerialized] public bool hasResult;
        [System.NonSerialized] public StageScore lastScore;
        [System.NonSerialized] public StageRewardResult lastReward;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

}
