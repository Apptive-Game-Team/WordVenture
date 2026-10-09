using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes
{
    public class TitleCredits : MonoBehaviour
    {
        [SerializeField] RectTransform panel;
        [SerializeField] GameObject overlay;
        [SerializeField] Button openButton;
        [SerializeField] Button closeButton;
        [SerializeField] CanvasGroup menu;
        GameObject previousSelection;
        bool previousInteractable, previousBlocksRaycasts;

        void Awake()
        {
            LanguageButton.Create(openButton);
        }

        public void Show()
        {
            if (overlay.activeSelf) return;
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            previousInteractable = menu.interactable;
            previousBlocksRaycasts = menu.blocksRaycasts;
            menu.interactable = menu.blocksRaycasts = false;
            overlay.SetActive(true);
            closeButton.Select();
        }

        public void Hide()
        {
            if (!overlay.activeSelf) return;
            overlay.SetActive(false);
            menu.interactable = previousInteractable;
            menu.blocksRaycasts = previousBlocksRaycasts;
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousSelection != null ? previousSelection : openButton.gameObject);
        }

        public void OpenProfile(string account)
        {
            Application.OpenURL("https://github.com/" + account);
        }

        void Update()
        {
            if (overlay.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Hide();
        }

        void LateUpdate()
        {
            var bounds = ((RectTransform)transform).rect;
            float scale = Mathf.Min(1, Mathf.Min(bounds.width / 820f, bounds.height / 840f));
            panel.localScale = Vector3.one * scale;
        }
    }
}
