using TMPro;
using UnityEngine;

namespace Combat
{
    // 캐릭터 머리 위에 하트와 체력 숫자를 한 줄로 놓고, 둘을 합친 폭의 가운데가 이 오브젝트의 x 위치에 오게 한다.
    // 숫자 자릿수가 바뀌면 폭이 달라지므로 그때마다 다시 놓는다.
    // 부모 배율이 음수여도(워드 -0.5, 방향을 바꾼 적) 하트와 숫자가 뒤집히지 않도록 이 오브젝트의 월드 배율을 1로 맞춘다.
    public sealed class HpLabelLayout : MonoBehaviour
    {
        // 1080p에서 월드 1 unit은 화면 108 픽셀이다.
        const float ScreenPixelsPerUnit = 108f;
        const float GapPixels = 6f;

        [SerializeField] SpriteRenderer heart;
        [SerializeField] TMP_Text text;
        // 하트 그림 폭(월드 unit)이다. 그림 한 칸을 화면 4 픽셀로 보이게 한 9칸 폭이다.
        [SerializeField] float heartWidth = 9f * 4f / ScreenPixelsPerUnit;
        // 글자 상하 중심이 하트 중심과 어긋나는 만큼 숫자를 올리거나 내린다.
        [SerializeField] float textOffsetY;

        float laidOutTextWidth = -1f;
        Vector3 laidOutParentScale;

        void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        void LateUpdate()
        {
            Layout();
        }

        void OnTextChanged(Object changed)
        {
            if (changed == text) Layout();
        }

        void Layout()
        {
            if (heart == null || text == null) return;
            Transform parent = transform.parent;
            Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;
            if (parentScale != laidOutParentScale)
            {
                transform.localScale = new Vector3(Inverse(parentScale.x), Inverse(parentScale.y), Inverse(parentScale.z));
                laidOutParentScale = parentScale;
            }

            float textWidth = text.preferredWidth;
            if (Mathf.Approximately(textWidth, laidOutTextWidth)) return;
            laidOutTextWidth = textWidth;
            float gap = GapPixels / ScreenPixelsPerUnit;
            float left = -(heartWidth + gap + textWidth) * 0.5f;
            heart.transform.localPosition = new Vector3(left + heartWidth * 0.5f, 0f, 0f);
            text.transform.localPosition = new Vector3(left + heartWidth + gap, textOffsetY, 0f);
        }

        static float Inverse(float scale)
        {
            return Mathf.Abs(scale) < 0.0001f ? 1f : 1f / scale;
        }
    }
}
