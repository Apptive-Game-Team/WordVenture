using System.Collections;
using System.Collections.Generic;
using Core;
using Tutorial;
using UnityEngine;

namespace Story
{
    // 튜토리얼이 설명하지 않는 규칙을 처음 만났을 때 한 번만 보여 주는 할아버지의 기록.
    // 값은 SeenTips 비트로 저장되므로 새 항목은 enum 끝에만 추가한다.
    public enum GameTip
    {
        HandLimit,
        EnemyRage,
        Result,
        Upgrade,
        AllyUpgrade,
    }

    public static class GameTips
    {
        public const string SeenKey = "SeenTips";
        const string DialogueResource = "Story/ActOneDialogues";
        const string Title = "할아버지의 기록";
        // 할아버지는 이미 세상을 떠났으므로 직접 말하지 않는다. 워드가 남겨진 기록을 펼쳐 읽는 나레이션으로 보여 준다.
        public const string IntroLine = "워드가 할아버지의 기록을 넘기자, 이런 때를 위해 적어 둔 장이 나왔다.";

        public static bool HasSeen(GameTip tip) => (PlayerPrefs.GetInt(SeenKey, 0) & Bit(tip)) != 0;

        public static void MarkSeen(GameTip tip)
        {
            PlayerPrefs.SetInt(SeenKey, PlayerPrefs.GetInt(SeenKey, 0) | Bit(tip));
            PlayerPrefs.Save();
        }

        static int Bit(GameTip tip) => 1 << (int)tip;

        // 튜토리얼이 진행 중이면 입력을 안내와 다투므로 아무것도 하지 않고, 읽음으로도 기록하지 않는다.
        static bool IsTutorialRunning =>
            TutorialController.Instance != null && !SaveLoadController.IsTutorialEnded;

        // 아직 보지 않은 팁만 한 번에 이어서 보여 주고, 보여 준 팁을 읽음으로 기록한다.
        public static IEnumerator ShowOnce(params GameTip[] tips)
        {
            if (IsTutorialRunning || UnseenLines(tips).Count == 0) yield break;
            var data = Resources.Load<StageDialogueData>(DialogueResource);
            if (data == null) yield break;

            if (InteractionLock.IsLocked) yield return new WaitUntil(() => !InteractionLock.IsLocked);
            // 기다리는 사이 같은 팁을 다른 곳에서 보여 줬거나 튜토리얼이 시작됐을 수 있다.
            if (IsTutorialRunning) yield break;
            var shown = new List<GameTip>();
            var lines = UnseenLines(tips, shown);
            if (lines.Count == 0) yield break;
            lines.Insert(0, RecordLine(IntroLine));

            var chapter = new StageDialogueChapter
            {
                stageID = -1,
                title = Title,
                lines = lines.ToArray(),
            };
            bool completed = false;
            var view = new GameObject("GameTipDialogue", typeof(RectTransform)).AddComponent<StageDialogueView>();
            // 다 읽고 닫을 때 기록한다. 읽는 도중에 게임을 끄면 다음에 다시 보여 준다.
            // 대화창이 잠금을 푸는 순간 바로 기록해야, 같은 팁을 기다리던 다른 코루틴이 다시 띄우지 않는다.
            view.Begin(data, chapter, () =>
            {
                foreach (GameTip tip in shown) MarkSeen(tip);
                completed = true;
            });
            yield return new WaitUntil(() => completed);
        }

        static StageDialogueLine RecordLine(string text) =>
            new StageDialogueLine { narration = true, gesture = WordGesture.ReadBook, text = text };

        static List<StageDialogueLine> UnseenLines(GameTip[] tips, List<GameTip> shown = null)
        {
            var lines = new List<StageDialogueLine>();
            foreach (GameTip tip in tips)
            {
                if (HasSeen(tip)) continue;
                foreach (string text in TextsOf(tip)) lines.Add(RecordLine(text));
                shown?.Add(tip);
            }
            return lines;
        }

        public static string[] TextsOf(GameTip tip)
        {
            switch (tip)
            {
                case GameTip.HandLimit:
                    return new[]
                    {
                        "손패에는 주문 카드와 속성 카드를 같은 수만큼, 처음에는 2장씩만 들 수 있단다. 더 뽑은 카드는 불타서 사라진단다.",
                        "쓸 수 없는 카드는 손패 왼쪽의 버리기 자리로 끌어다 놓거라. 한 턴에 한 장만 버릴 수 있단다.",
                    };
                case GameTip.EnemyRage:
                    return new[]
                    {
                        "슬라임 머리 위에 빨간 화살표가 떴구나. 싸움이 길어질수록 슬라임들은 점점 더 화가 난단다.",
                        "3턴마다 슬라임의 공격이 더 세지니, 오래 끌지 말고 서둘러 끝내거라.",
                    };
                case GameTip.Result:
                    return new[]
                    {
                        "싸움이 끝나면 걸린 턴, 쓴 카드, 깎인 HP, 원소 연계로 등급을 매긴단다.",
                        "등급이 높을수록 마력 조각을 더 많이 얻는단다.",
                    };
                case GameTip.Upgrade:
                    return new[]
                    {
                        "마력 조각이 생겼구나. 오른쪽 위의 업그레이드 버튼(U)으로 쓸 수 있단다.",
                        "공격력, 최대 HP, 손패 상한을 올릴 수 있지만 조각은 귀하니 신중하게 고르거라.",
                    };
                case GameTip.AllyUpgrade:
                    return new[]
                    {
                        "이제 Spawn 카드로 아군 슬라임을 불러낼 수 있단다.",
                        "업그레이드 창에서 아군 슬라임도 강하게 만들 수 있게 되었구나.",
                    };
                default:
                    return new string[0];
            }
        }
    }
}
