using System;
using System.Collections;
using System.Collections.Generic;
using Cards;
using Combat.Enemies;
using UnityEngine;

namespace Combat.Allies
{
    // 워드 앞의 아군 자리 3칸. 먼저 소환한 슬라임이 맨 앞(적과 가장 가까운 칸)에 서고,
    // 나중에 소환한 슬라임이 그 뒤에 선다. 맨 앞 슬라임이 쓰러지면 뒤 슬라임이 한 칸씩 앞으로 나온다.
    // 전투 씬에 처음 Spawn을 시전할 때 만들어지고, 씬이 바뀌면 슬라임과 함께 사라진다.
    public class AllyFormation : MonoBehaviour
    {
        public const int SlotCount = 3;
        const string AllySlimePrefabPath = "Combat/AllySlime";

        // 맨 앞 칸은 워드에서 4.2만큼 앞이다. 근접 적은 맨 앞 칸에서 사거리(3)만큼 떨어져 멈추므로,
        // 적이 생성되는 x=6~8 지점과 워드(x=-7.17) 사이에 세 칸과 적이 모두 들어간다.
        const float FrontSlotOffset = 4.2f;
        const float SlotSpacing = 1.4f;
        // 적 pool이 적을 세우는 높이와 같다.
        const float GroundY = -3f;
        const float SecondsBetweenAttacks = 0.4f;

        static AllyFormation current;

        readonly List<AllySlime> allies = new List<AllySlime>();
        Transform anchor;
        AllySlime allySlimePrefab;

        public static AllyFormation Current => current;
        public static bool CanSpawn => current == null || current.allies.Count < SlotCount;
        public IReadOnlyList<AllySlime> Allies => allies;
        public AllySlime Front => allies.Count > 0 ? allies[0] : null;

        public static AllyFormation GetOrCreate(Transform anchor)
        {
            if (current != null) return current;
            current = new GameObject(nameof(AllyFormation)).AddComponent<AllyFormation>();
            current.anchor = anchor;
            current.allySlimePrefab = Resources.Load<AllySlime>(AllySlimePrefabPath);
            return current;
        }

        // 적은 워드 대신 이 x 좌표까지의 거리로 이동과 공격을 정한다. 앞 슬라임이 쓰러져 뒤 슬라임이
        // 앞으로 걸어가는 중에도 기준선이 흔들리지 않도록 슬라임 위치 대신 맨 앞 칸의 위치를 쓴다.
        public static float FrontLineX(float playerX)
        {
            if (current == null || current.allies.Count == 0) return playerX;
            return current.SlotPosition(0).x;
        }

        // 근접 공격은 공격한 적과 워드 사이에 선 슬라임 중 적에게 가장 가까운 슬라임이 대신 받는다.
        // 슬라임을 소환하기 전에 이미 맨 앞 칸보다 워드 쪽으로 온 적은 자기 뒤의 슬라임을 때리지 않는다.
        public static void HitFrontLine(int damage, float attackerX)
        {
            AllySlime blocker = current != null ? current.FindBlocker(attackerX) : null;
            if (blocker != null)
            {
                blocker.TakeHit(damage);
                return;
            }
            Player.PlayerInt().TakeHit(damage);
        }

        AllySlime FindBlocker(float attackerX)
        {
            foreach (AllySlime ally in allies)
                if (ally.IsAlive && ally.transform.position.x <= attackerX) return ally;
            return null;
        }

        public AllySlime Spawn(MagicType element, MagicAffinityTable affinityTable)
        {
            if (allies.Count >= SlotCount) return null;
            Vector3 slot = SlotPosition(allies.Count);
            AllySlime ally = Instantiate(allySlimePrefab, slot, allySlimePrefab.transform.rotation, transform);
            ally.Initialize(element, affinityTable);
            ally.Died += OnAllyDied;
            allies.Add(ally);
            return ally;
        }

        // 앞줄부터 한 마리씩, 그 순간 가장 앞에 있는 적을 공격한다. 앞 슬라임의 공격으로 적이
        // 쓰러지면 뒤 슬라임은 다음 적을 공격한다.
        public IEnumerator AttackFrontEnemies(Func<Enemy> findFrontEnemy)
        {
            foreach (AllySlime ally in allies.ToArray())
            {
                Enemy target = findFrontEnemy();
                if (target == null) yield break;
                if (ally == null || !ally.IsAlive) continue;
                ally.Attack(target);
                yield return new WaitForSeconds(SecondsBetweenAttacks);
            }
        }

        void OnAllyDied(AllySlime ally)
        {
            ally.Died -= OnAllyDied;
            allies.Remove(ally);
            for (int i = 0; i < allies.Count; i++)
                allies[i].MoveTo(SlotPosition(i));
        }

        Vector3 SlotPosition(int slotIndex)
        {
            float anchorX = anchor != null ? anchor.position.x : 0f;
            return new Vector3(anchorX + FrontSlotOffset - SlotSpacing * slotIndex, GroundY, 0f);
        }

        void OnDestroy()
        {
            if (current == this) current = null;
        }
    }
}
