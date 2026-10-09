using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 곡사 탄 하나. 쏜 자리에서 착탄 표시까지 포물선으로 날아가고, 떨어진 순간 그 자리에 피해를 준 뒤 표시와 함께 사라진다.
    // 비행 시간은 적 턴 1초(TurnBattleSystem.TurnTime) 안에 떨어지도록 그보다 짧게 둔다.
    public sealed class MortarShell : MonoBehaviour
    {
        public const float FlightTime = 0.6f;
        const float ArcHeight = 3f;
        const float Scale = 0.2f;

        Vector3 start;
        Vector3 end;
        GameObject marker;
        int damage;
        float elapsed;

        public static void Launch(Sprite sprite, Vector3 from, GameObject marker, int damage)
        {
            var shellObject = new GameObject("MortarShell");
            shellObject.transform.position = from;
            shellObject.transform.localScale = Vector3.one * Scale;
            SpriteRenderer shellRenderer = shellObject.AddComponent<SpriteRenderer>();
            shellRenderer.sprite = sprite;
            // 슬라임과 착탄 표시보다 앞에 그린다.
            shellRenderer.sortingOrder = 2;

            MortarShell shell = shellObject.AddComponent<MortarShell>();
            shell.start = from;
            shell.end = marker.transform.position;
            shell.marker = marker;
            shell.damage = damage;
        }

        void Update()
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / FlightTime);
            Vector3 position = Vector3.Lerp(start, end, t);
            position.y += ArcHeight * 4f * t * (1f - t);
            transform.position = position;
            if (t < 1f) return;

            AllyFormation.HitPoint(end.x, damage);
            if (marker != null) Destroy(marker);
            Destroy(gameObject);
        }
    }
}
