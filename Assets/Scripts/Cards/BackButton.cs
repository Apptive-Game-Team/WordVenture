using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cards
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
