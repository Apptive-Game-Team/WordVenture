using UnityEngine;

public class StageDataSingleton : MonoBehaviour
{
    public static StageDataSingleton Instance { get; private set; }
    public int StagePosition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
