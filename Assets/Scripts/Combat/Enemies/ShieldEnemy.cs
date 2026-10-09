using System.Collections.Generic;

namespace Combat.Enemies
{
    // HP가 높고 공격력이 낮은 근접 적. 바로 뒤에 선 적이 받는 주문 피해를 절반으로 줄인다.
    public class ShieldEnemy : SwordEnemy
    {
        const float GuardDistance = 2.5f;
        static readonly List<ShieldEnemy> ActiveShields = new List<ShieldEnemy>();

        void OnEnable() => ActiveShields.Add(this);
        void OnDisable() => ActiveShields.Remove(this);

        // 적은 오른쪽에서 워드 쪽(왼쪽)으로 온다. 방패보다 오른쪽 2.5 unit 안에 선 적이 보호받는다.
        public static bool IsGuarding(Enemy enemy)
        {
            float x = enemy.transform.position.x;
            foreach (ShieldEnemy shield in ActiveShields)
            {
                if (shield == enemy || !shield.IsAlive) continue;
                float gap = x - shield.transform.position.x;
                if (gap > 0f && gap <= GuardDistance) return true;
            }
            return false;
        }
    }
}
