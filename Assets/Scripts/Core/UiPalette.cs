using UnityEngine;

namespace Core
{
    // 타이틀 버튼과 어울리는 어두운 나무색 UI 공용 팔레트. 런타임에 UI 를 만드는 스크립트가 같은 색을 쓴다.
    public static class UiPalette
    {
        public static readonly Color Panel = new Color(0.16f, 0.105f, 0.065f);
        public static readonly Color Border = new Color(0.72f, 0.53f, 0.25f);
        public static readonly Color Text = new Color(0.98f, 0.91f, 0.70f);
        public static readonly Color Cell = new Color(0.26f, 0.18f, 0.11f);
        public static readonly Color SelectedCell = new Color(0.5f, 0.36f, 0.17f);
    }
}
