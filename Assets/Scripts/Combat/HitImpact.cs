using System.Collections;
using UnityEngine;

namespace Combat
{
    // 높을수록 강한 타격이다. 히트 스톱과 화면 흔들림 길이가 이 순서로 커진다.
    public enum HitStrength { None, Resisted, Normal, Weak, Reaction }

    // 주문이 적에게 맞았을 때의 히트 스톱과 카메라 흔들림. 수치 조정은 이곳에서 한다.
    public static class HitImpact
    {
        const float NormalHitStopSeconds = 0.04f;
        const float WeakHitStopSeconds = 0.08f;
        const float ReactionHitStopSeconds = 0.12f;
        // 흔들림 폭은 월드 단위다.
        const float NormalShakeAmplitude = 0.04f;
        const float WeakShakeAmplitude = 0.12f;
        const float ReactionShakeAmplitude = 0.2f;
        const float ShakeSeconds = 0.18f;

        // appliedAffinity는 균열 보정까지 반영한 ElementalStatus.HitResult.Affinity다.
        public static HitStrength Classify(float appliedAffinity, bool hasReaction)
        {
            // 신성의 음수 상성은 회복이라 타격 효과를 내지 않는다.
            if (appliedAffinity <= 0f) return HitStrength.None;
            if (hasReaction) return HitStrength.Reaction;
            if (appliedAffinity > 1f) return HitStrength.Weak;
            if (appliedAffinity < 1f) return HitStrength.Resisted;
            return HitStrength.Normal;
        }

        // 저항 타격은 멈추거나 흔들지 않는다. 약하게 들어갔다는 느낌을 살리기 위해서다.
        public static float GetHitStopSeconds(HitStrength strength)
        {
            switch (strength)
            {
                case HitStrength.Normal: return NormalHitStopSeconds;
                case HitStrength.Weak: return WeakHitStopSeconds;
                case HitStrength.Reaction: return ReactionHitStopSeconds;
                default: return 0f;
            }
        }

        public static float GetShakeAmplitude(HitStrength strength)
        {
            switch (strength)
            {
                case HitStrength.Normal: return NormalShakeAmplitude;
                case HitStrength.Weak: return WeakShakeAmplitude;
                case HitStrength.Reaction: return ReactionShakeAmplitude;
                default: return 0f;
            }
        }

        public static void Play(HitStrength strength)
        {
            float stopSeconds = GetHitStopSeconds(strength);
            float amplitude = GetShakeAmplitude(strength);
            if (stopSeconds <= 0f && amplitude <= 0f) return;
            // 에디터 테스트에서는 timeScale과 카메라를 건드리지 않는다.
            if (!Application.isPlaying) return;
            Runner runner = Runner.GetOrCreate();
            if (runner != null) runner.Play(stopSeconds, amplitude);
        }

        // 프로젝트에서 Time.timeScale을 바꾸는 곳은 여기뿐이다. 현재 씬에 숨겨 두어 전투 씬을 떠나면 함께 사라지고,
        // 사라질 때 timeScale과 카메라 위치를 되돌린다.
        sealed class Runner : MonoBehaviour
        {
            static Runner instance;
            float hitStopEndTime;
            Coroutine hitStop;
            Camera shakenCamera;
            Vector3 cameraOrigin;
            float shakeStartTime;
            float shakeAmplitude;

            public static Runner GetOrCreate()
            {
                if (instance != null) return instance.isActiveAndEnabled ? instance : null;
                // DontDestroyOnLoad를 쓰지 않고, HideFlags도 DontSave를 빼야 씬과 함께 사라진다.
                var holder = new GameObject("HitImpact") { hideFlags = HideFlags.HideInHierarchy };
                instance = holder.AddComponent<Runner>();
                return instance;
            }

            public void Play(float stopSeconds, float amplitude)
            {
                float now = Time.unscaledTime;
                if (stopSeconds > 0f)
                {
                    // 겹친 타격은 시간을 더하지 않고 더 늦게 끝나는 쪽까지만 늘린다.
                    hitStopEndTime = Mathf.Max(hitStopEndTime, now + stopSeconds);
                    if (hitStop == null) hitStop = StartCoroutine(HitStopping());
                }
                if (amplitude > 0f) StartShake(amplitude, now);
            }

            IEnumerator HitStopping()
            {
                Time.timeScale = 0f;
                while (Time.unscaledTime < hitStopEndTime)
                    yield return new WaitForSecondsRealtime(hitStopEndTime - Time.unscaledTime);
                Time.timeScale = 1f;
                hitStop = null;
            }

            void StartShake(float amplitude, float now)
            {
                Camera camera = Camera.main;
                if (camera == null) return;
                if (shakenCamera != camera)
                {
                    RestoreCamera();
                    shakenCamera = camera;
                    cameraOrigin = camera.transform.localPosition;
                    shakeAmplitude = 0f;
                }
                // 겹친 흔들림은 남은 폭과 새 폭 중 큰 쪽으로 감쇠를 다시 시작한다. 원점은 처음 값을 유지해 카메라가 밀리지 않는다.
                shakeAmplitude = Mathf.Max(GetCurrentShakeAmplitude(now), amplitude);
                shakeStartTime = now;
            }

            float GetCurrentShakeAmplitude(float now)
            {
                float progress = (now - shakeStartTime) / ShakeSeconds;
                return progress >= 1f ? 0f : shakeAmplitude * (1f - progress);
            }

            // 히트 스톱 중에도 흔들리도록 unscaled 시간을 쓴다. LateUpdate는 timeScale이 0이어도 매 프레임 불린다.
            void LateUpdate()
            {
                if (shakenCamera == null)
                {
                    shakenCamera = null;
                    return;
                }
                float amplitude = GetCurrentShakeAmplitude(Time.unscaledTime);
                if (amplitude <= 0f)
                {
                    RestoreCamera();
                    return;
                }
                // 매 프레임 원점에서 다시 계산하므로 오프셋이 쌓이지 않는다.
                Vector2 offset = UnityEngine.Random.insideUnitCircle * amplitude;
                shakenCamera.transform.localPosition = cameraOrigin + new Vector3(offset.x, offset.y, 0f);
            }

            void RestoreCamera()
            {
                if (shakenCamera != null) shakenCamera.transform.localPosition = cameraOrigin;
                shakenCamera = null;
                shakeAmplitude = 0f;
            }

            void RestoreTime()
            {
                // 비활성화되면 코루틴도 멈추므로 여기서 직접 되돌린다.
                Time.timeScale = 1f;
                hitStop = null;
                hitStopEndTime = 0f;
            }

            void OnDisable()
            {
                RestoreTime();
                RestoreCamera();
            }

            void OnDestroy()
            {
                RestoreTime();
                RestoreCamera();
                if (instance == this) instance = null;
            }
        }
    }
}
