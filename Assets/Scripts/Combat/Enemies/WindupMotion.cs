using UnityEngine;

namespace Combat.Enemies
{
    // 다음 적 턴에 큰 공격을 하는 적이 예고하는 턴 동안 몸을 움직인다. 머리 위 글자 대신 이 움직임과
    // SlimeAnimator의 예고 프레임이 예고다. 돌진 슬라임은 뒤로 물러나 떨고, 폭발 슬라임은 제자리에서 떤다.
    public sealed class WindupMotion : MonoBehaviour
    {
        const float RearSeconds = 0.2f;
        const float TremblePerSecond = 40f;

        float rearDistance;
        float trembleDistance;

        bool playing;
        Vector3 anchor;
        float elapsed;

        public bool IsPlaying => playing;

        // rearDistance는 오른쪽으로 물러나는 거리다.
        public void Play(float rearDistance, float trembleDistance)
        {
            if (playing) return;
            this.rearDistance = rearDistance;
            this.trembleDistance = trembleDistance;
            anchor = transform.position;
            elapsed = 0f;
            playing = true;
        }

        // 물러난 자리는 그대로 두고 떨림만 멈춘다. 돌진은 물러난 자리에서 출발한다.
        public void Stop()
        {
            if (!playing) return;
            playing = false;
            transform.position = new Vector3(anchor.x + RearOffset(), anchor.y, anchor.z);
        }

        void Update()
        {
            if (!playing) return;
            elapsed += Time.deltaTime;
            float tremble = trembleDistance * Mathf.Sin(elapsed * TremblePerSecond);
            transform.position = new Vector3(anchor.x + RearOffset() + tremble, anchor.y, anchor.z);
        }

        // 적은 언제나 왼쪽(-x)의 아군과 워드를 공격하므로 오른쪽으로 물러난다.
        float RearOffset()
        {
            return rearDistance * Mathf.Clamp01(elapsed / RearSeconds);
        }

        void OnDisable()
        {
            // 쓰러져 풀로 돌아간 적이 예고 동작을 그대로 들고 다시 나오지 않게 한다.
            playing = false;
        }
    }
}
