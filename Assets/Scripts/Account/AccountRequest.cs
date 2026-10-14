using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace Account
{
    // 요청 하나를 보내고 상태 코드와 본문을 돌려준다. 토큰 갱신은 AccountService 가 맡는다.
    internal static class AccountRequest
    {
        const int TimeoutSeconds = 10;

        public static IEnumerator Send(string method, string path, string bearer, string json, Action<AccountResponse> done)
        {
            using (UnityWebRequest request = new UnityWebRequest(AccountSession.ServerUrl + path, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                if (json != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                if (!string.IsNullOrEmpty(bearer)) request.SetRequestHeader("Authorization", "Bearer " + bearer);
                request.redirectLimit = 0;
                request.timeout = TimeoutSeconds;
                yield return request.SendWebRequest();
                done(new AccountResponse(request.responseCode, request.downloadHandler.text));
            }
        }
    }
}
