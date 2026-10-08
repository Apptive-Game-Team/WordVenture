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
