using System;
using TMPro;
using UnityEngine;

namespace Story
{
    [Serializable]
    public struct StageDialogueLine
    {
        public bool wordSpeaking;
        [Tooltip("켜면 이름 없이 나레이션으로 보여 준다. 초상화는 둘 다 어둡게 남는다.")]
        public bool narration;
        [Tooltip("나레이션이 워드의 몸짓을 설명하면 워드 초상화를 그 몸짓 그림으로 바꾸고 밝게 보여 준다.")]
        public WordGesture gesture;
        [TextArea(2, 4)] public string text;
    }

    // 워드는 세계수 결말 전까지 말하지 않는다. 하려던 말은 나레이션의 몸짓으로 보여 준다.
    // 같은 몸짓이라도 상황에 따라 표정이 다른 그림을 쓴다. 값은 에셋에 숫자로 저장되므로 새 몸짓은 끝에 붙인다.
    public enum WordGesture
    {
        None,
        // 부드럽게 미소 지으며 끄덕인다.
        Nod,
        // 슬프게 고개를 젓는다.
        ShakeHead,
        // 진지하게 가리킨다.
        Point,
        Tilt,
        // 펼친 마법서를 보여 준다.
        ShowBook,
        RaiseCard,
        PalmOut,
        // 굳게 결심하고 끄덕인다.
        NodFirm,
        // 적에게 단호하게 고개를 젓는다.
        ShakeHeadFirm,
        // 걱정스러운 얼굴로 가리킨다.
        PointWorried,
        // 덮인 마법서의 표지를 보여 준다.
        ShowCover,
        // 목을 짚어 말할 수 없다고 알린다.
        TouchThroat,
    }

    [Serializable]
    public struct WordGesturePortrait
    {
        public WordGesture gesture;
        public Sprite portrait;
    }

    public enum StageDialogueMoment
    {
        // 스테이지를 클리어한 뒤 보상 화면에서 나갈 때
        Clear,
        // 맵에서 스테이지에 들어갈 때
        Enter,
        // 전투 중 wave 를 시작하기 직전
        Wave,
    }

    [Serializable]
    public sealed class StageDialogueChapter
    {
        public int stageID;
        public StageDialogueMoment moment;
        [Tooltip("moment 가 Wave 일 때 이 번호의 wave 를 시작하기 직전에 재생한다.")]
        public int wave;
        public string title;
        public string speakerName;
        public Sprite speakerPortrait;
        [Tooltip("비워 두면 배경 그림 없이 지금 화면 위에 대화창을 띄운다.")]
        public Sprite background;
        public StageDialogueLine[] lines;

        // 읽음 기록 비트: 장마다 클리어 0~4, 입장 5~9, wave 10~29. 기존 세이브의 1부 비트를 유지한다.
        // 2부(스테이지 5~9)는 같은 배치에 ActTwoBitOffset을 더한다. SaveLoadController가 이 비트를
        // 2부 전용 저장 키에 기록한다.
        public const int StagesPerAct = 5;
        public const int ActTwoBitOffset = 32;

        public int SeenBit
        {
            get
            {
                if (stageID < 0 || stageID >= StagesPerAct * 2) return -1;
                int stageInAct = stageID % StagesPerAct;
                int actOffset = stageID < StagesPerAct ? 0 : ActTwoBitOffset;
                switch (moment)
                {
                    case StageDialogueMoment.Clear: return actOffset + stageInAct;
                    case StageDialogueMoment.Enter: return actOffset + 5 + stageInAct;
                    default: return wave >= 0 && wave < 4 ? actOffset + 10 + stageInAct * 4 + wave : -1;
                }
            }
        }
    }

    [CreateAssetMenu(menuName = "Story/Stage Dialogue")]
    public sealed class StageDialogueData : ScriptableObject
    {
        public Sprite wordPortrait;
        public WordGesturePortrait[] gesturePortraits;
        public TMP_FontAsset font;
        public StageDialogueChapter[] chapters;

        // 몸짓 그림이 없으면 평소 초상화를 쓴다.
        public Sprite WordPortraitFor(WordGesture gesture)
        {
            if (gesture == WordGesture.None || gesturePortraits == null) return wordPortrait;
            foreach (var entry in gesturePortraits)
                if (entry.gesture == gesture && entry.portrait != null)
                    return entry.portrait;
            return wordPortrait;
        }

        public StageDialogueChapter FindChapter(int stageID,
            StageDialogueMoment moment = StageDialogueMoment.Clear, int wave = 0)
        {
            if (chapters == null) return null;
            foreach (var chapter in chapters)
                if (chapter.stageID == stageID && chapter.moment == moment
                    && (moment != StageDialogueMoment.Wave || chapter.wave == wave))
                    return chapter;
            return null;
        }
    }
}
