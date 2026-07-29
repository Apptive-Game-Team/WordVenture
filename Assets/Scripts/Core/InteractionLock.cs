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
        public static bool IsLocked { get; set; }
    }
}
