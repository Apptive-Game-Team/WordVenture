using UnityEngine;
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

        // Update is called once per frame
        void Update()
        {

        }

        public void OnButtonClick()
        {
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
