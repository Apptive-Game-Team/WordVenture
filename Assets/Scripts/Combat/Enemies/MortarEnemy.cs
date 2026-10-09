using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 움직이지 않고 한 적 턴에는 떨어질 자리를 표시하고, 다음 적 턴에 그 자리에 탄을 떨어뜨린다.
    // 앞줄 아군 슬라임을 넘어 맨 뒤 슬라임이나 워드를 노린다.
    public class MortarEnemy : Enemy
    {
        const float MarkerY = -3.4f;

        [SerializeField] Sprite markerSprite;
        GameObject marker;

        public bool HasMarker => marker != null;
        public float MarkerX => marker != null ? marker.transform.position.x : 0f;

        protected override void TakeTurnAction(float distanceToFrontLine)
        {
            if (marker == null)
            {
                PlaceMarker(AllyFormation.RearTargetX(Player.PlayerInt().transform.position.x));
                return;
            }

            Animator.RangeAttack();
            AllyFormation.HitPoint(marker.transform.position.x, AttackDamage);
            ClearMarker();
        }

        void PlaceMarker(float x)
        {
            marker = new GameObject("MortarMarker");
            marker.transform.position = new Vector3(x, MarkerY, 0f);
            SpriteRenderer markerRenderer = marker.AddComponent<SpriteRenderer>();
            markerRenderer.sprite = markerSprite;
            // 배경보다 앞, 슬라임 발밑에 보이도록 한 칸 앞에 그린다.
            markerRenderer.sortingOrder = 1;
            SetIntent("포격 조준");
        }

        void ClearMarker()
        {
            if (marker != null) Destroy(marker);
            marker = null;
            SetIntent(string.Empty);
        }

        // 쓰러지면 비활성화된다. 표시만 남아 있으면 떨어지지 않을 탄을 경고하게 된다.
        void OnDisable()
        {
            if (marker != null) Destroy(marker);
            marker = null;
        }
    }
}
