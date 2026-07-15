using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using WordVenture.Cards;
using WordVenture.Combat.Enemies;
using WordVenture.Combat.Spells;

namespace WordVenture.Combat.UI
{

    public class CombineButton : MonoBehaviour
    {
        public GameObject CombineZone;
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
            if (!CombineZone.activeSelf)
            {
                CombineZone.SetActive(true);
            }
            else if (CombineZone.activeSelf)
            {
                CombineZone.SetActive(false);
            }
        }
    }

}
