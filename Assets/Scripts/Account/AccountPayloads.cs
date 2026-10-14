using System;

namespace Account
{
    // JsonUtility 가 읽고 쓰는 필드 이름은 서버 JSON 과 같아야 한다.
    [Serializable]
    internal class AuthTokens
    {
        public string jwt;
        public string refreshToken;
        public long expiresIn;
    }

    [Serializable]
    internal class SignUpBody
    {
        public string email;
        public string name;
        public string password;
    }

    [Serializable]
    internal class LoginBody
    {
        public string email;
        public string password;
    }

    [Serializable]
    internal class RefreshBody
    {
        public string refreshToken;
    }

    [Serializable]
    internal class ValueBody
    {
        public string value;
    }

    [Serializable]
    internal class MemberBody
    {
        public string email;
        public string name;
    }
}
