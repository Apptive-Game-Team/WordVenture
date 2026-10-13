namespace Account
{
    public enum FailureContext
    {
        Credentials,
        SignUp,
        Session,
    }

    // 상태 코드와 본문을 사용자에게 보여 줄 한국어 문구로 바꾼다. 네트워크를 쓰지 않는다.
    public static class AccountFailure
    {
        public const string Network = "서버에 연결할 수 없습니다. 네트워크를 확인해 주세요.";
        public const string WrongCredentials = "이메일 또는 비밀번호가 올바르지 않습니다.";
        public const string LoginAgain = "로그인이 만료되었습니다. 다시 로그인해 주세요.";
        public const string SystemMissing = "서버에 wordventure 시스템이 등록되어 있지 않습니다.";
        public const string ServerError = "서버에 문제가 생겼습니다. 잠시 뒤에 다시 시도해 주세요.";
        public const string Unknown = "요청을 처리하지 못했습니다.";
        const int ServerMessageMaxLength = 100;

        public static string Describe(FailureContext context, long status, string body)
        {
            if (status == 0) return Network;
            if (status >= 500) return ServerError;
            if (IsSystemMissing(status, body)) return SystemMissing;
            switch (context)
            {
                case FailureContext.Credentials:
                    return WrongCredentials;
                case FailureContext.SignUp:
                    if (status == 409) return "이미 가입된 이메일입니다.";
                    return status == 400 && !string.IsNullOrWhiteSpace(body) && body.Length <= ServerMessageMaxLength
                        ? body.Trim()
                        : Unknown;
                default:
                    return status == 401 || (status >= 300 && status < 400) ? LoginAgain : Unknown;
            }
        }

        // key 가 없으면 400 "Key-Value not found ...", system 이 없으면 400 "System not found: ..." 이다.
        public static bool IsKeyMissing(long status, string body) =>
            status == 400 && body != null && body.StartsWith("Key-Value not found");

        public static bool IsSystemMissing(long status, string body) =>
            status == 400 && body != null && body.StartsWith("System not found");
    }
}
