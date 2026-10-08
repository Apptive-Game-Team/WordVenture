using System;
using TMPro;
using UnityEngine;

namespace Story
{
    [Serializable]
    public struct StageDialogueLine
    {
        public bool wordSpeaking;
        [Tooltip("켜면 이름과 초상화 없이 나레이션으로 보여 준다.")]
        public bool narration;
        [TextArea(2, 4)] public string text;
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

        // 읽음 기록 비트: 클리어 0~4, 입장 5~9, wave 10~29. 기존 세이브의 클리어 비트를 유지한다.
        public int SeenBit
        {
            get
            {
                if (stageID < 0 || stageID >= 5) return -1;
                switch (moment)
                {
                    case StageDialogueMoment.Clear: return stageID;
                    case StageDialogueMoment.Enter: return 5 + stageID;
                    default: return wave >= 0 && wave < 4 ? 10 + stageID * 4 + wave : -1;
                }
            }
        }
    }

    [CreateAssetMenu(menuName = "Story/Stage Dialogue")]
    public sealed class StageDialogueData : ScriptableObject
    {
        public Sprite wordPortrait;
        public TMP_FontAsset font;
        public StageDialogueChapter[] chapters;

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
