using Combat.Allies;
using UnityEngine;

namespace Combat.Enemies
{
    // 곡사 포격 한 번의 조준과 발사. 곡사 슬라임과 최종 보스가 같이 쓴다.
    // 조준할 때 맨 뒤 아군 슬라임(없으면 워드) 자리에 표시를 놓고, 다음 적 턴에 그 자리로 탄을 쏜다.
    public sealed class MortarStrike
    {
        const float MarkerY = -3.4f;

        readonly Sprite markerSprite;
        readonly Sprite shellSprite;
        GameObject marker;

        public MortarStrike(Sprite markerSprite, Sprite shellSprite)
        {
            this.markerSprite = markerSprite;
            this.shellSprite = shellSprite;
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

        // 표시는 탄에 넘긴다. 탄이 MortarShell.FlightTime 뒤에 떨어지면서 피해를 주고 표시를 지운다.
        public void Fire(Vector3 from, int damage)
        {
            if (marker == null) return;
            MortarShell.Launch(shellSprite, from, marker, damage);
            marker = null;
        }

        // 쏘기 전에 쏜 사람이 쓰러지면 표시만 남아 떨어지지 않을 탄을 경고하게 되므로 지운다.
        // 이미 날아가는 탄은 표시를 가지고 있으므로 그대로 떨어진다.
        public void Clear()
        {
            if (marker != null) Object.Destroy(marker);
            marker = null;
        }
    }
}
