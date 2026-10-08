using System;
using TMPro;
using UnityEngine;

namespace Story
{
    [Serializable]
    public struct StageDialogueLine
    {
        public bool wordSpeaking;
        [TextArea(2, 4)] public string text;
    }

    [Serializable]
    public sealed class StageDialogueChapter
    {
        public int stageID;
        public string title;
        public Sprite background;
        public StageDialogueLine[] lines;
    }

    [CreateAssetMenu(menuName = "Story/Stage Dialogue")]
    public sealed class StageDialogueData : ScriptableObject
    {
        public Sprite wordPortrait;
        public Sprite villagerPortrait;
        public TMP_FontAsset font;
        public StageDialogueChapter[] chapters;

        public StageDialogueChapter FindChapter(int stageID)
        {
            if (chapters == null) return null;
            foreach (var chapter in chapters)
                if (chapter.stageID == stageID) return chapter;
            return null;
        }
    }
}
