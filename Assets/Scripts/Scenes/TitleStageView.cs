using Combat.Stage;
using Core;
using TMPro;
using UnityEngine;

namespace Scenes
{
    /// <summary>
    /// 저장된 진행도가 있으면 이어 할 지역 이름을 보여 주고 타이틀 배경을 그 지역의 전투 배경으로 바꾼다.
    /// StagePosition 은 다음에 깰 스테이지 번호다. 마지막 스테이지보다 크면(2부를 끝낸 10) 모두 깬 것이다.
    /// </summary>
    public class TitleStageView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer background;
        [SerializeField] GameObject plaque;
        [SerializeField] TMP_Text label;
        [SerializeField] StageData[] stages;

        Sprite titleSprite;
        Vector3 titleScale;
        Camera view;
        float framedAspect;
        bool showsStage;

        void Awake()
        {
            titleSprite = background.sprite;
            titleScale = background.transform.localScale;
            view = Camera.main;
        }

        public void Show(int stagePosition)
        {
            StageData stage = Find(stagePosition);
            showsStage = stage != null && stage.background != null;
            plaque.SetActive(stage != null);
            if (stage != null)
                label.text = string.Format(Localization.Translate("이어하기: {0}"), Describe(stagePosition));

            if (!showsStage)
            {
                background.sprite = titleSprite;
                background.transform.localScale = titleScale;
                return;
            }
            background.sprite = stage.background;
            Frame();
        }

        // 계정 창에서 이 기기와 서버의 진행도를 비교할 때도 같은 이름을 쓴다.
        public string Describe(int stagePosition)
        {
            StageData stage = Find(stagePosition);
            if (stage == null) return Localization.Translate("진행도 없음");
            if (stagePosition > stage.stageID) return Localization.Translate("모든 지역 완료");
            return string.IsNullOrEmpty(stage.stageName)
                ? "Stage " + stage.stageID
                : Localization.Translate(stage.stageName);
        }

        // stagePosition 이하에서 가장 뒤의 스테이지. 진행도가 없으면 null 이다.
        StageData Find(int stagePosition)
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

        // 전투 배경마다 크기가 달라서 카메라 화면을 빈틈없이 덮는 배율을 다시 구한다.
        void Frame()
        {
            if (view == null) return;
            framedAspect = view.aspect;
            float height = view.orthographicSize * 2;
            Vector2 size = background.sprite.bounds.size;
            float scale = Mathf.Max(height * view.aspect / size.x, height / size.y);
            background.transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}
