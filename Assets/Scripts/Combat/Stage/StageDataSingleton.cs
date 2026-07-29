using UnityEngine;
using UnityEngine.Serialization;

namespace Combat.Stage
{

    public class StageDataSingleton : MonoBehaviour
    {
        public static StageDataSingleton Instance { get; private set; }
        [FormerlySerializedAs("StagePosition")] public int stagePosition;

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

}
