using System;
using System.Collections;
using UnityEngine;

namespace Account
{
    // 코루틴 API. 호출자가 StartCoroutine 으로 실행하고, 콜백은 정확히 한 번 호출된다.
    public static class AccountService
    {
        const string SaveKey = "save";
        const string FeedbackIdKey = "feedback-id";
        const string MemberPath = "/api/members/me";
        const string NoSaveMessage = "서버에 저장된 데이터가 없습니다.";

        enum RefreshOutcome { Refreshed, Rejected, Unreachable }

        public static IEnumerator SignUp(string email, string name, string password, Action<AccountResult> done)
        {
            string json = JsonUtility.ToJson(new SignUpBody { email = email, name = name, password = password });
            yield return Enter("/api/members", json, name, FailureContext.SignUp, "가입했습니다.", done);
        }

        public static IEnumerator Login(string email, string password, Action<AccountResult> done)
        {
            string json = JsonUtility.ToJson(new LoginBody { email = email, password = password });
            yield return Enter("/api/members/login", json, email, FailureContext.Credentials, "로그인했습니다.", done);
        }

        public static IEnumerator RestoreSession(Action<AccountResult> done)
        {
            if (string.IsNullOrEmpty(AccountSession.RefreshToken))
            {
                done(new AccountResult(false, ""));
                yield break;
            }
            RefreshOutcome outcome = RefreshOutcome.Rejected;
            yield return Refresh(value => outcome = value);
            if (outcome == RefreshOutcome.Refreshed)
            {
                yield return OpenWithMemberName(null);
                done(new AccountResult(true, "로그인 상태를 불러왔습니다."));
            }
            else
            {
                done(new AccountResult(false, outcome == RefreshOutcome.Unreachable ? AccountFailure.Network : AccountFailure.LoginAgain));
            }
        }

        public static IEnumerator Logout(Action<AccountResult> done)
        {
            string refreshToken = AccountSession.RefreshToken;
            AccountSession.Clear();
            if (!string.IsNullOrEmpty(refreshToken))
            {
                // 서버 호출이 실패해도 로컬 로그아웃은 이미 끝났다.
                string json = JsonUtility.ToJson(new RefreshBody { refreshToken = refreshToken });
                yield return AccountRequest.Send("POST", "/api/auth/logout", null, json, _ => { });
            }
            done(new AccountResult(true, "로그아웃했습니다."));
        }

        public static IEnumerator UploadSave(Action<AccountResult> done)
        {
            AccountResponse response = default;
            yield return AuthorizedValue("PUT", SaveKey, CloudSave.FromLocal().Serialize(), value => response = value);
            if (response.IsSuccess)
                yield return AuthorizedValue("PUT", UpgradeCloudSave.ServerKey, UpgradeCloudSave.FromLocal().Serialize(), value => response = value);
            done(response.IsSuccess
                ? new AccountResult(true, "진행 상황을 서버에 저장했습니다.")
                : Failed(response));
        }

        public static IEnumerator DownloadSave(Action<AccountResult, CloudSave> done)
        {
            AccountResponse response = default;
            yield return AuthorizedValue("GET", SaveKey, null, value => response = value);
            if (AccountFailure.IsKeyMissing(response.Status, response.Body))
            {
                done(new AccountResult(true, NoSaveMessage), null);
            }
            else if (!response.IsSuccess)
            {
                done(Failed(response), null);
            }
            else if (!CloudSave.TryParse(ReadValue(response.Body), out CloudSave save))
            {
                done(new AccountResult(false, "서버에 저장된 데이터를 읽을 수 없습니다."), null);
            }
            else
            {
                // 업그레이드 키가 없는 예전 저장은 그대로 성공이고, 이 기기의 업그레이드를 건드리지 않는다.
                yield return AuthorizedValue("GET", UpgradeCloudSave.ServerKey, null, value => response = value);
                if (response.IsSuccess)
                {
                    if (!UpgradeCloudSave.TryParse(ReadValue(response.Body), out UpgradeCloudSave upgrades))
                    {
                        done(new AccountResult(false, "서버에 저장된 데이터를 읽을 수 없습니다."), null);
                        yield break;
                    }
                    save.Upgrades = upgrades;
                }
                else if (!AccountFailure.IsKeyMissing(response.Status, response.Body))
                {
                    done(Failed(response), null);
                    yield break;
                }
                done(new AccountResult(true, "서버에서 진행 상황을 불러왔습니다."), save);
            }
        }

        // GyeMong 형식: "feedback-id" 를 읽어(없으면 0) 1 올려 저장하고, "feedback-{id}" 에 내용을 저장한다.
        public static IEnumerator SendFeedback(string text, Action<AccountResult> done)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                done(new AccountResult(false, "피드백 내용을 입력해 주세요."));
                yield break;
            }
            if (text.Length > AccountSession.FeedbackMaxLength)
            {
                done(new AccountResult(false, $"피드백은 {AccountSession.FeedbackMaxLength}자 이하로 입력해 주세요."));
                yield break;
            }

            AccountResponse response = default;
            yield return AuthorizedValue("GET", FeedbackIdKey, null, value => response = value);
            int id = 0;
            if (response.IsSuccess) int.TryParse(ReadValue(response.Body), out id);
            else if (!AccountFailure.IsKeyMissing(response.Status, response.Body))
            {
                done(Failed(response));
                yield break;
            }

            id++;
            yield return AuthorizedValue("PUT", FeedbackIdKey, id.ToString(), value => response = value);
            if (response.IsSuccess)
                yield return AuthorizedValue("PUT", $"feedback-{id}", text, value => response = value);
            done(response.IsSuccess ? new AccountResult(true, "피드백을 보냈습니다. 감사합니다.") : Failed(response));
        }

        // 가입과 로그인은 요청 방식만 다르고 뒤처리는 같다.
        static IEnumerator Enter(string path, string json, string fallbackName, FailureContext context, string successMessage, Action<AccountResult> done)
        {
            AccountResponse response = default;
            yield return AccountRequest.Send("POST", path, null, json, value => response = value);
            AuthTokens tokens = response.IsSuccess ? ReadTokens(response.Body) : null;
            if (tokens == null)
            {
                done(new AccountResult(false, response.IsSuccess
                    ? AccountFailure.Unknown
                    : AccountFailure.Describe(context, response.Status, response.Body)));
                yield break;
            }
            AccountSession.Store(tokens);
            yield return OpenWithMemberName(fallbackName);
            done(new AccountResult(true, successMessage));
        }

        // 이름은 /api/members/me 에서 읽는다. 읽지 못해도 로그인은 유지한다.
        static IEnumerator OpenWithMemberName(string fallbackName)
        {
            AccountResponse response = default;
            yield return Authorized("GET", MemberPath, null, value => response = value);
            MemberBody member = response.IsSuccess ? FromJson<MemberBody>(response.Body) : null;
            string name = member != null && !string.IsNullOrEmpty(member.name) ? member.name : fallbackName;
            if (AccountSession.IsLoggedIn) AccountSession.Open(string.IsNullOrEmpty(name) ? "플레이어" : name);
        }

        static IEnumerator AuthorizedValue(string method, string key, string value, Action<AccountResponse> done)
        {
            string path = $"/api/systems/{AccountSession.SystemName}/members/me/key-values/{key}";
            string json = value == null ? null : JsonUtility.ToJson(new ValueBody { value = value });
            yield return Authorized(method, path, json, done);
        }

        // access token 이 곧 만료되거나 만료 응답(302/401)을 받으면 refresh 를 한 번 하고 요청을 한 번 다시 보낸다.
        static IEnumerator Authorized(string method, string path, string json, Action<AccountResponse> done)
        {
            AccountResponse response = default;
            if (AccountSession.IsLoggedIn && !AccountSession.IsAccessTokenExpiring)
            {
                yield return AccountRequest.Send(method, path, AccountSession.AccessToken, json, value => response = value);
                if (!response.IsExpired)
                {
                    done(response);
                    yield break;
                }
            }

            RefreshOutcome outcome = RefreshOutcome.Rejected;
            yield return Refresh(value => outcome = value);
            if (outcome == RefreshOutcome.Unreachable)
            {
                done(new AccountResponse(0, ""));
            }
            else if (outcome == RefreshOutcome.Rejected)
            {
                done(new AccountResponse(401, "", sessionLost: true));
            }
            else
            {
                yield return AccountRequest.Send(method, path, AccountSession.AccessToken, json, value => response = value);
                done(response);
            }
        }

        // refresh token 은 호출마다 바뀐다. 서버가 거절했을 때만 로그아웃하고, 네트워크나 서버 오류는 토큰을 남긴다.
        static IEnumerator Refresh(Action<RefreshOutcome> done)
        {
            string refreshToken = AccountSession.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
            {
                AccountSession.Clear();
                done(RefreshOutcome.Rejected);
                yield break;
            }

            AccountResponse response = default;
            string json = JsonUtility.ToJson(new RefreshBody { refreshToken = refreshToken });
            yield return AccountRequest.Send("POST", "/api/auth/refresh", null, json, value => response = value);
            if (response.Status == 0 || response.Status >= 500)
            {
                done(RefreshOutcome.Unreachable);
                yield break;
            }

            AuthTokens tokens = response.IsSuccess ? ReadTokens(response.Body) : null;
            if (tokens == null)
            {
                AccountSession.Clear();
                done(RefreshOutcome.Rejected);
                yield break;
            }
            AccountSession.Store(tokens);
            done(RefreshOutcome.Refreshed);
        }

        static AccountResult Failed(AccountResponse response) => new AccountResult(false,
            response.SessionLost ? AccountFailure.LoginAgain : AccountFailure.Describe(FailureContext.Session, response.Status, response.Body));

        static AuthTokens ReadTokens(string body)
        {
            AuthTokens tokens = FromJson<AuthTokens>(body);
            return tokens != null && !string.IsNullOrEmpty(tokens.jwt) && !string.IsNullOrEmpty(tokens.refreshToken) ? tokens : null;
        }

        static string ReadValue(string body) => FromJson<ValueBody>(body)?.value;

        static T FromJson<T>(string body) where T : class
        {
            try { return JsonUtility.FromJson<T>(body); }
            catch (ArgumentException) { return null; }
        }
    }
}
