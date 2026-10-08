using TMPro;
using UnityEngine;

namespace Story
{
    [CreateAssetMenu(menuName = "Story/Dialogue Window Style")]
    public sealed class DialogueWindowStyle : ScriptableObject
    {
        public TMP_FontAsset font;
        public Sprite wordPortrait;
        public Color panelColor = new Color(0.08f, 0.07f, 0.14f, 0.97f);
        public Color borderColor = new Color(0.78f, 0.59f, 0.32f);
        public Color nameColor = new Color(1f, 0.85f, 0.55f);
        public Color textColor = Color.white;
        public Color promptColor = new Color(0.75f, 0.73f, 0.82f);
        public Color listenerColor = new Color(0.55f, 0.55f, 0.62f);
        public int nameSize = 34;
        public int textSize = 34;
        public int promptSize = 24;
    }
}
