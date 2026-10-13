using UnityEngine;

namespace Core
{
    /// <summary>
    /// 튜토리얼 대화창처럼 화면을 점유하는 연출이 떠 있는 동안 그 뒤의 게임플레이 입력을 막는다.
    ///
    /// uGUI 버튼(Back, 조합 버튼 등)은 전체 화면 blocker 이미지로 막지만,
    /// 콜라이더 기반 <c>OnMouseXXX</c>와 직접 쏘는 Physics2D 레이, 그리고 Update에서
    /// 바로 읽는 <c>Input</c>은 UI에 가려지지 않는다. 그쪽은 이 잠금을 확인해야 한다.
    /// </summary>
    public static class InteractionLock
    {
        static bool locked;

        static bool paused;
        static int resumedFrame = -1;

        /// <summary>
        /// 설정 메뉴가 열려 있는 동안 켜진다. 튜토리얼과 대화창이 <see cref="IsLocked"/>를 풀어도
        /// 메뉴가 닫히기 전까지는 입력이 막혀 있도록 따로 둔다.
        ///
        /// 메뉴를 닫은 Esc 키나 클릭도 <c>Input.anyKeyDown</c>에 잡히므로, 닫은 프레임까지는 켜진 것으로 본다.
        /// </summary>
        public static bool IsPaused
        {
            get => paused || Time.frameCount == resumedFrame;
            set
            {
                if (paused && !value) resumedFrame = Time.frameCount;
                paused = value;
            }
        }

        public static bool IsLocked
        {
            get => locked || IsPaused;
            set => locked = value;
        }
    }
}
