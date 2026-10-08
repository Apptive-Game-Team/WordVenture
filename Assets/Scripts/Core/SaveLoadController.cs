using Map;
using Story;
using UnityEngine;

namespace Core
{

    public class SaveLoadController : MonoBehaviour
    {
        public const string TutorialEndedKey = "TutorialEnded";
        public const string StageDialogueSeenKey = "ActOneDialogueSeen";
        public const string ActTwoDialogueSeenKey = "ActTwoDialogueSeen";

        // seenBit 는 StageDialogueChapter.SeenBit 이다. PlayerPrefs의 int 하나는 31비트까지 쓰므로
        // 1부 비트(0~29)와 2부 비트(32~61)를 다른 키에 기록한다.
        public static bool HasSeenStageDialogue(int seenBit)
        {
            if (!TryGetDialogueSeenSlot(seenBit, out string key, out int bit)) return false;
            return (PlayerPrefs.GetInt(key, 0) & (1 << bit)) != 0;
        }

        public static void MarkStageDialogueSeen(int seenBit)
        {
            if (!TryGetDialogueSeenSlot(seenBit, out string key, out int bit)) return;
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) | (1 << bit));
            PlayerPrefs.SetInt("StagePosition", MapMove.StagePosition);
            PlayerPrefs.Save();
        }
        static bool TryGetDialogueSeenSlot(int seenBit, out string key, out int bit)
        {
            key = seenBit >= StageDialogueChapter.ActTwoBitOffset ? ActTwoDialogueSeenKey : StageDialogueSeenKey;
            bit = seenBit >= StageDialogueChapter.ActTwoBitOffset ? seenBit - StageDialogueChapter.ActTwoBitOffset : seenBit;
            return seenBit >= 0 && bit < 31;
        }

        public static bool IsTutorialEnded => PlayerPrefs.GetInt(TutorialEndedKey, 0) == 1;

        public static void MarkTutorialEnded()
        {
            PlayerPrefs.SetInt(TutorialEndedKey, 1);
            PlayerPrefs.Save();
        }

        private static SaveLoadController _instance = null;

        void Awake()
        {
            if (null == _instance)
            {
                //이 클래스 인스턴스가 탄생했을 때 전역변수 instance에 게임매니저 인스턴스가 담겨있지 않다면, 자신을 넣어준다.
                _instance = this;

                //씬 전환이 되더라도 파괴되지 않게 한다.
                //gameObject만으로도 이 스크립트가 컴포넌트로서 붙어있는 Hierarchy상의 게임오브젝트라는 뜻이지만,
                //나는 헷갈림 방지를 위해 this를 붙여주기도 한다.
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                //만약 씬 이동이 되었는데 그 씬에도 Hierarchy에 GameMgr이 존재할 수도 있다.
                //그럴 경우엔 이전 씬에서 사용하던 인스턴스를 계속 사용해주는 경우가 많은 것 같다.
                //그래서 이미 전역변수인 instance에 인스턴스가 존재한다면 자신(새로운 씬의 GameMgr)을 삭제해준다.
                Destroy(this.gameObject);
            }
        }

        //게임 매니저 인스턴스에 접근할 수 있는 프로퍼티. static이므로 다른 클래스에서 맘껏 호출할 수 있다.
        public static SaveLoadController Instance
        {
            get
            {
                if (null == _instance)
                {
                    return null;
                }
                return _instance;
            }
        }

        public void SavePlayData()
        {
            PlayerPrefs.SetInt("StagePosition", MapMove.StagePosition);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
            #else
                    Application.Quit();
            #endif
        }

        public int LoadPlayData()
        {
            MapMove.StagePosition = PlayerPrefs.GetInt("StagePosition", -1);
            return MapMove.StagePosition;
        }

        private void OnApplicationQuit()
        {
            SavePlayData();
        }

        public void InitPlayData()
        {
            PlayerPrefs.SetInt("StagePosition", -1);
            PlayerPrefs.DeleteKey(TutorialEndedKey);
            PlayerPrefs.DeleteKey(StageDialogueSeenKey);
            PlayerPrefs.DeleteKey(ActTwoDialogueSeenKey);
            PlayerPrefs.Save();
        }
    }

}
