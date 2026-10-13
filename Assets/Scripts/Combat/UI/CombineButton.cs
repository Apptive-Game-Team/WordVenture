using UnityEngine;
using Core;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Combat.UI
{

    public class CombineButton : MonoBehaviour
    {
        [FormerlySerializedAs("CombineZone")] public GameObject combineZone;
        public Button activateButton; // 버튼 참조
        // 조합창이 열려 있는 동안 버튼을 눌린 그림으로 두고, 그림 속 버튼 면이 내려간 만큼 글자도 내린다.
        [SerializeField] Sprite openSprite;
        [SerializeField] RectTransform label;
        [SerializeField] float openLabelOffset = 8f;

        Sprite closedSprite;
        SpriteState closedSpriteState;
        Vector2 closedLabelPosition;
        bool shownOpen;

        void Awake()
        {
            closedSprite = activateButton.image.sprite;
            closedSpriteState = activateButton.spriteState;
            closedLabelPosition = label.anchoredPosition;
        }

        //void Start()
        //{
        //    if (activateButton != null)
        //    {
        //        activateButton.onClick.AddListener(OnButtonClick);
        //    }
        //}

        void Update()
        {
            // 주문이 준비되는 동안 조합창 버튼이 잠겨 있다는 것을 버튼 색으로 보여준다.
            if (activateButton != null)
            {
                activateButton.interactable = !IsSpellCasting();
            }
            ShowOpen(combineZone.activeSelf);
        }

        void ShowOpen(bool open)
        {
            if (shownOpen == open)
            {
                return;
            }

            shownOpen = open;
            activateButton.image.sprite = open ? openSprite : closedSprite;
            // 열린 동안 마우스를 올려도 올라온 그림으로 바뀌지 않게 한다.
            SpriteState spriteState = closedSpriteState;
            if (open)
            {
                spriteState.highlightedSprite = openSprite;
            }
            activateButton.spriteState = spriteState;
            label.anchoredPosition = closedLabelPosition + (open ? Vector2.down * openLabelOffset : Vector2.zero);
        }

        static bool IsSpellCasting()
        {
            return CombineZone.Instance != null && CombineZone.Instance.IsCasting;
        }

        public void OnButtonClick()
        {
            if (InteractionLock.IsLocked || IsSpellCasting()) return;
            if (!combineZone.activeSelf)
            {
                combineZone.SetActive(true);
            }
            else if (combineZone.activeSelf)
            {
                combineZone.SetActive(false);
            }
        }
    }

}
