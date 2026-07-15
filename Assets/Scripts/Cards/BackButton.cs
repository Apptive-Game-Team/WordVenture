using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using WordVenture.Combat.UI;
using WordVenture.Core;

namespace WordVenture.Cards
{

    public class BackButton : MonoBehaviour
    {
        public void BackToMain()
        {
            SceneManager.LoadScene("TitleScene");
            GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>().SavePlayData();
        }
    }

}
