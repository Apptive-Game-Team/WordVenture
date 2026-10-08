using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 곡사 포격 한 번의 조준과 착탄. 곡사 슬라임과 최종 보스가 같이 쓴다.
    // 조준할 때 맨 뒤 아군 슬라임(없으면 워드) 자리에 표시를 놓고, 다음 적 턴에 그 자리에 떨어뜨린다.
    public sealed class MortarStrike
    {
        const float MarkerY = -3.4f;

        readonly Sprite markerSprite;
        GameObject marker;

        public MortarStrike(Sprite markerSprite)
        {
            this.markerSprite = markerSprite;
        }

        public bool IsAimed => marker != null;
        public float TargetX => marker != null ? marker.transform.position.x : 0f;

        public void Aim()
        {
            Clear();
            float x = AllyFormation.RearTargetX(Player.PlayerInt().transform.position.x);
            marker = new GameObject("MortarMarker");
            marker.transform.position = new Vector3(x, MarkerY, 0f);
            SpriteRenderer markerRenderer = marker.AddComponent<SpriteRenderer>();
            markerRenderer.sprite = markerSprite;
            // 배경보다 앞, 슬라임 발밑에 보이도록 한 칸 앞에 그린다.
            markerRenderer.sortingOrder = 1;
        }

        public void Fire(int damage)
        {
            if (marker == null) return;
            AllyFormation.HitPoint(marker.transform.position.x, damage);
            Clear();
        }

        // 쏜 사람이 쓰러지면 표시만 남아 떨어지지 않을 탄을 경고하게 되므로 지운다.
        public void Clear()
        {
            if (marker != null) Object.Destroy(marker);
            marker = null;
        }
    }
}
