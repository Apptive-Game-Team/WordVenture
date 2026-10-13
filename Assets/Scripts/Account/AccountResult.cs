namespace Account
{
    public readonly struct AccountResult
    {
        public bool Success { get; }
        // 화면에 그대로 보여 줄 수 있는 한국어 문구
        public string Message { get; }

        public AccountResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
