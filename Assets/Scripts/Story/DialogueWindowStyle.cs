using TMPro;
using UnityEngine;

namespace Story
{
    [CreateAssetMenu(menuName = "Story/Dialogue Window Style")]
    public sealed class DialogueWindowStyle : ScriptableObject
    {
        public TMP_FontAsset font;
        public Sprite wordPortrait;
        public Color panelColor = new Color(0.16f, 0.105f, 0.065f, 0.97f);
        public Color borderColor = new Color(0.72f, 0.53f, 0.25f);
        public Color nameColor = new Color(1f, 0.85f, 0.55f);
        public Color textColor = new Color(0.98f, 0.91f, 0.70f);
        public Color promptColor = new Color(0.78f, 0.68f, 0.5f);
        public Color listenerColor = new Color(0.55f, 0.55f, 0.62f);
        public int nameSize = 34;
        public int textSize = 34;
        public int promptSize = 24;
    }
}
