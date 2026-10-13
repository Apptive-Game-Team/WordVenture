using System;
using UnityEngine;

namespace Account
{
    public static class AccountSession
    {
        public const string ServerUrl = "https://account.theevilent.com";
        public const string SystemName = "wordventure";
        public const int FeedbackMaxLength = 500;
        public const string RefreshTokenKey = "AccountRefreshToken";

        public static bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
        public static string MemberName { get; private set; }
        // 로그인, 로그아웃, 세션 복원 때 발생한다.
        public static event Action Changed;

        // access token 은 메모리에만 둔다. refresh token 은 매번 바뀌므로 받을 때마다 저장한다.
        internal static string AccessToken { get; private set; }
        // WebGL 에서는 브라우저가 302 를 따라가 버려 만료 응답을 알아챌 수 없다. 만료 1분 전에 미리 refresh 한다.
        internal static bool IsAccessTokenExpiring => DateTime.UtcNow.AddMinutes(1) >= accessTokenExpiresAt;
        static DateTime accessTokenExpiresAt;
        internal static string RefreshToken => PlayerPrefs.GetString(RefreshTokenKey, "");

        internal static void Store(AuthTokens tokens)
        {
            AccessToken = tokens.jwt;
            accessTokenExpiresAt = DateTime.UtcNow.AddSeconds(tokens.expiresIn);
            if (!string.IsNullOrEmpty(tokens.refreshToken))
            {
                PlayerPrefs.SetString(RefreshTokenKey, tokens.refreshToken);
                PlayerPrefs.Save();
            }
        }

        internal static void Open(string memberName)
        {
            MemberName = memberName;
            Changed?.Invoke();
        }

        internal static void Clear()
        {
            bool wasActive = IsLoggedIn || PlayerPrefs.HasKey(RefreshTokenKey);
            AccessToken = null;
            MemberName = null;
            PlayerPrefs.DeleteKey(RefreshTokenKey);
            PlayerPrefs.Save();
            if (wasActive) Changed?.Invoke();
        }
    }
}
