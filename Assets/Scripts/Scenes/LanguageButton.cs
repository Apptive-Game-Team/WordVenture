using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Scenes
{
    /// <summary>
    /// 타이틀의 크레딧 버튼을 복제해 바로 왼쪽에 언어 전환 버튼을 둔다.
    /// 씬 파일을 고치지 않고 같은 나무판 모양을 쓰기 위해 실행 중에 만든다.
    /// </summary>
    public static class LanguageButton
    {
        const float Gap = 24f;

        public static Button Create(Button template)
        {
            Button button = Object.Instantiate(template, template.transform.parent);
            button.name = "LanguageButton";
            var rect = (RectTransform)button.transform;
            rect.anchoredPosition -= new Vector2(rect.sizeDelta.x + Gap, 0);
            // 복제하면 크레딧을 여는 persistent call 도 따라오므로 이벤트를 새로 만든다.
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(Toggle);
            // 버튼에는 지금 언어가 아니라 바꿀 언어를 그 언어로 적는다.
            button.GetComponentInChildren<TMP_Text>(true).text =
                Localization.Current == Language.Korean ? "English" : "한국어";
            return button;
        }

        static void Toggle()
        {
            Localization.SetLanguage(Localization.Current == Language.Korean ? Language.English : Language.Korean);
            // 씬의 고정 문구는 씬을 열 때 바뀌므로 타이틀을 다시 연다.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
