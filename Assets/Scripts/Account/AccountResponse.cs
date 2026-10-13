namespace Account
{
    internal readonly struct AccountResponse
    {
        // 0 이면 서버에 닿지 못한 것(네트워크 실패, 시간 초과)이다.
        public long Status { get; }
        public string Body { get; }
        // refresh 까지 거절되어 로컬 로그인 정보를 지운 경우
        public bool SessionLost { get; }

        public AccountResponse(long status, string body, bool sessionLost = false)
        {
            Status = status;
            Body = body ?? "";
            SessionLost = sessionLost;
        }

        public bool IsSuccess => Status >= 200 && Status < 300;
        // 인증이 없거나 만료되면 서버가 401 이 아니라 /login 으로 302 를 보낸다.
        public bool IsExpired => Status == 401 || (Status >= 300 && Status < 400);
    }
}
