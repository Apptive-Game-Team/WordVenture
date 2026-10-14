using Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 설정 메뉴에서 여는 크레딧 창. 설정 메뉴 prefab 에 들어 있어서 어느 씬에서나 열린다.
    /// </summary>
    public class Credits : MonoBehaviour
    {
        [SerializeField] RectTransform panel;
        [SerializeField] Button closeButton;
        TMP_Text[] labels;
        string[] koreanLabels;
        GameObject previousSelection;

        public bool IsOpen => gameObject.activeSelf;

        void RememberKoreanLabels()
        {
            // 설정 메뉴는 씬이 바뀌어도 남기 때문에 Localization 이 씬을 열 때 바꾸는 문구에 들어가지 않는다.
            // 한국어 원문을 기억해 두고 열 때마다 지금 언어로 바꾼다.
            labels = GetComponentsInChildren<TMP_Text>(true);
            koreanLabels = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++) koreanLabels[i] = labels[i].text;
        }

        public void Show()
        {
            if (IsOpen) return;
            if (labels == null) RememberKoreanLabels();
            for (int i = 0; i < labels.Length; i++) labels[i].text = Localization.Translate(koreanLabels[i]);
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            gameObject.SetActive(true);
            closeButton.Select();
        }

        public void Hide()
        {
            if (!IsOpen) return;
            gameObject.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(previousSelection);
        }

        public void OpenProfile(string account)
        {
            Application.OpenURL("https://github.com/" + account);
        }

        void LateUpdate()
        {
            var bounds = ((RectTransform)transform).rect;
            float scale = Mathf.Min(1, Mathf.Min(bounds.width / 820f, bounds.height / 840f));
            panel.localScale = Vector3.one * scale;
        }
    }
}
