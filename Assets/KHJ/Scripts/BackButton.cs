using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void BackToMain()
    {
        SceneManager.LoadScene("TitleScene");
        GameObject.Find("SaveLoadController").GetComponent<SaveLoadController>().SavePlayData();
    }
}
