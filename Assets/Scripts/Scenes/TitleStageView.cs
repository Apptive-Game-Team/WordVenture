using Combat.Stage;
using Core;
using UnityEngine;

namespace Scenes
{
    /// <summary>
    /// 저장된 진행도가 있으면 타이틀 배경을 이어 할 지역의 전투 배경으로 바꾼다.
    /// StagePosition 은 다음에 깰 스테이지 번호다. 마지막 스테이지보다 크면(2부를 끝낸 10) 모두 깬 것이다.
    /// </summary>
    public class TitleStageView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] StageData[] stages;
        // 빗길은 전투 씬처럼 비가 내리는 애니메이션 배경을 띄운다(StageManager.SetBackground 와 같은 stage 번호).
        [SerializeField] GameObject rainyBackgroundPrefab;
        const int RainyStageId = 3;

        Sprite titleSprite;
        Vector3 titleScale;
        Camera view;
        float framedAspect;
        bool showsStage;
        SpriteRenderer rain;

        void Awake()
        {
            titleSprite = background.sprite;
            titleScale = background.transform.localScale;
            view = Camera.main;
        }

        public void Show(int stagePosition)
        {
            StageData stage = Find(stages, stagePosition);
            showsStage = stage != null && stage.background != null;
            bool raining = showsStage && stage.stageID == RainyStageId && rainyBackgroundPrefab != null;
            if (raining && rain == null)
                rain = Instantiate(rainyBackgroundPrefab, background.transform.position, Quaternion.identity).GetComponent<SpriteRenderer>();
            if (rain != null) rain.gameObject.SetActive(raining);
            background.enabled = !raining;

            if (!showsStage)
            {
                background.sprite = titleSprite;
                background.transform.localScale = titleScale;
                return;
            }
            background.sprite = stage.background;
            Frame();
        }

        // 계정 창에서 이 기기와 서버의 진행도를 비교할 때도 같은 이름을 쓴다. 계정 창은 설정 메뉴 prefab 에 있어서
        // 타이틀의 이 컴포넌트를 참조하지 못하므로, 같은 stage 목록을 넘겨 받는다.
        public static string Describe(StageData[] stages, int stagePosition)
        {
            StageData stage = Find(stages, stagePosition);
            if (stage == null) return Localization.Translate("진행도 없음");
            if (stagePosition > stage.stageID) return Localization.Translate("모든 지역 완료");
            return string.IsNullOrEmpty(stage.stageName)
                ? "Stage " + stage.stageID
                : Localization.Translate(stage.stageName);
        }

        // stagePosition 이하에서 가장 뒤의 스테이지. 진행도가 없으면 null 이다.
        static StageData Find(StageData[] stages, int stagePosition)
        {
            if (stagePosition < 0) return null;
            StageData found = null;
            foreach (StageData stage in stages)
                if (stage != null && stage.stageID <= stagePosition && (found == null || stage.stageID > found.stageID))
                    found = stage;
            return found;
        }

        void LateUpdate()
        {
            if (showsStage && !Mathf.Approximately(view.aspect, framedAspect)) Frame();
        }

        // 전투 배경(2:1)은 타이틀 배경(16:9)과 비율이 달라서, 카메라 화면을 빈틈없이 덮는 배율을 다시 구한다.
        void Frame()
        {
            if (view == null) return;
            framedAspect = view.aspect;
            Cover(background);
            if (rain != null && rain.gameObject.activeSelf) Cover(rain);
        }

        void Cover(SpriteRenderer target)
        {
            float height = view.orthographicSize * 2;
            Vector2 size = target.sprite.bounds.size;
            float scale = Mathf.Max(height * view.aspect / size.x, height / size.y);
            target.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
