using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes
{
    /// <summary>
    /// 계정 요청을 들어온 순서대로 하나씩 보낸다. AccountService 는 token refresh 를 잠그지 않으므로
    /// 인증 요청 두 개가 겹치면 refresh token 이 꼬인다. 씬을 바꿔도 끊기지 않도록 씬 밖의 오브젝트에서 돌린다.
    /// </summary>
    public sealed class AccountRequests : MonoBehaviour
    {
        static readonly Queue<IEnumerator> Pending = new Queue<IEnumerator>();
        static AccountRequests runner;

        public static bool IsBusy { get; private set; }
        public static event Action BusyChanged;

        public static void Enqueue(IEnumerator request)
        {
            Pending.Enqueue(request);
            if (IsBusy) return;
            if (runner == null)
            {
                runner = new GameObject(nameof(AccountRequests)).AddComponent<AccountRequests>();
                DontDestroyOnLoad(runner.gameObject);
            }
            runner.StartCoroutine(runner.RunAll());
        }

        IEnumerator RunAll()
        {
            SetBusy(true);
            while (Pending.Count > 0) yield return StartCoroutine(Pending.Dequeue());
            SetBusy(false);
        }

        static void SetBusy(bool busy)
        {
            IsBusy = busy;
            BusyChanged?.Invoke();
        }

        // 도메인 리로드를 끈 플레이 모드에서도 이전 실행의 대기열이 남지 않게 한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetState()
        {
            Pending.Clear();
            IsBusy = false;
            BusyChanged = null;
        }
    }
}
