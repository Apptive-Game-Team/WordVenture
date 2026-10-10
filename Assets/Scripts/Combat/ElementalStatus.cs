using System;
using System.Collections.Generic;
using Cards;
using UnityEngine;

namespace Combat
{
    // 수치 조정은 이곳에서 한다. 상태 피해는 직접 공격으로 취급하지 않아 연쇄 발동하지 않는다.
    public sealed class ElementalStatus
    {
        public const int FreezeThreshold = 3;
        public const float BurnRatio = 0.2f;
        public const float ShockBonus = 0.25f;
        public const float ShatterBonus = 0.5f;
        public const float WeakenMultiplier = 0.7f;
        // HitResult.Reaction 값. ReactionBurstVfx가 이 값으로 재생할 효과를 고른다.
        public const string OverloadReaction = "불꽃 방전";
        public const string ShatterReaction = "쇄빙";
        public const string ParalysisReaction = "신경 마비";
        public const string LavaCrackReaction = "용암 균열";
        readonly MagicType defenseElement;
        public ElementalStatus() : this(MagicType.Undead) { }
        public ElementalStatus(MagicType defenseElement) { this.defenseElement = defenseElement; }
        public int RequiredChill => defenseElement == MagicType.Ice ? FreezeThreshold * 2 : FreezeThreshold;
        public string ResistanceDescription => defenseElement == MagicType.Fire ? "불 속성 · 화상 피해 50%"
            : defenseElement == MagicType.Ice ? "얼음 속성 · 빙결에 냉기 6점 필요"
            : defenseElement == MagicType.Lightning ? "번개 속성 · 감전 증폭 50%"
            : defenseElement == MagicType.Rock ? "바위 속성 · 균열 효과 50%"
            : defenseElement == MagicType.Holy ? "신성 속성" : "언데드 속성";
        public int BurnDamage { get; private set; }
        public int BurnTurns { get; private set; }
        public int Chill { get; private set; }
        public int ChillTurns { get; private set; }
        public bool Frozen { get; private set; }
        public int FreezeImmunity { get; private set; }
        public int ShockTurns { get; private set; }
        public int FractureTurns { get; private set; }
        public int WeakenTurns { get; private set; }
        public bool Weakened => WeakenTurns > 0;
        bool attackUsed;
        bool frozenConsumed;
        public float MovementMultiplier => Chill > 0 ? 0.5f : 1f;
        public float AttackMultiplier => Weakened ? WeakenMultiplier : 1f;

        public struct HitResult
        {
            public int Damage;
            public int ExtraDamage;
            public string Reaction;
            public bool RemovesBurn;
            public bool RemovesChill;
            // 균열 보정까지 반영해 실제로 쓴 상성. 타격감 세기(HitImpact)를 고르는 데 쓴다.
            public float Affinity;
        }

        public HitResult Preview(MagicType element, MagicType spell, float baseDamage, float affinity, bool boss)
        {
            return ((ElementalStatus)MemberwiseClone()).Hit(element, spell, baseDamage, affinity, boss);
        }

        public HitResult Hit(MagicType element, MagicType spell, float baseDamage, float affinity, bool boss)
        {
            baseDamage = Mathf.Max(0, baseDamage);
            // 신성의 음수 상성은 기존 회복 동작이다. 회복은 공격 준비 상태를 소비하지 않는다.
            if (affinity <= 0f)
                return new HitResult { Damage = (int)(baseDamage * affinity), Reaction = string.Empty, Affinity = affinity };
            bool hadFracture = FractureTurns > 0;
            bool hadShock = ShockTurns > 0;
            float hitAffinity = affinity;
            if (hadFracture && affinity < 1f)
                hitAffinity = defenseElement == MagicType.Rock ? Mathf.Lerp(affinity, 1f, 0.5f) : 1f;
            float shockBonus = defenseElement == MagicType.Lightning ? ShockBonus * 0.5f : ShockBonus;
            var result = new HitResult
            {
                // 균열은 약점 보너스를 유지하고 저항(1 미만)만 무시한다.
                Damage = Mathf.FloorToInt(baseDamage * hitAffinity * (hadShock ? 1f + shockBonus : 1f)),
                Reaction = string.Empty,
                Affinity = hitAffinity
            };
            ShockTurns = FractureTurns = 0;
            int burnDuration = spell == MagicType.Drop ? 1 : 2;

            if (element == MagicType.Lightning && BurnTurns > 0)
            {
                result.ExtraDamage = BurnDamage * BurnTurns;
                result.Reaction = OverloadReaction;
                BurnDamage = BurnTurns = 0;
            }
            else if (element == MagicType.Rock && (Chill > 0 || Frozen))
            {
                result.ExtraDamage = Mathf.FloorToInt(baseDamage * ShatterBonus);
                result.Reaction = ShatterReaction;
                ClearChill();
            }
            else if (element == MagicType.Ice && hadShock)
            {
                WeakenTurns = 2;
                result.Reaction = ParalysisReaction;
            }
            else if (element == MagicType.Fire && hadFracture)
            {
                burnDuration = 3;
                result.Reaction = LavaCrackReaction;
            }

            switch (element)
            {
                case MagicType.Fire:
                    result.RemovesChill = Chill > 0 || Frozen;
                    ClearChill();
                    float burnResistance = defenseElement == MagicType.Fire ? 0.5f : 1f;
                    BurnDamage = Mathf.Max(BurnDamage, Mathf.Max(1,
                        Mathf.FloorToInt(baseDamage * hitAffinity * BurnRatio * burnResistance)));
                    BurnTurns = Mathf.Max(BurnTurns, burnDuration);
                    break;
                case MagicType.Ice:
                    result.RemovesBurn = BurnTurns > 0;
                    BurnDamage = BurnTurns = 0;
                    if (!Frozen)
                    {
                        Chill = Mathf.Min(RequiredChill, Chill + (spell == MagicType.Shoot ? 2 : 1));
                        ChillTurns = 2;
                        if (Chill >= RequiredChill && FreezeImmunity == 0)
                        {
                            Chill = 0;
                            ChillTurns = 0;
                            if (boss) WeakenTurns = 2;
                            else Frozen = true;
                            FreezeImmunity = 2;
                        }
                    }
                    break;
                case MagicType.Lightning:
                    ShockTurns = 2;
                    break;
                case MagicType.Rock:
                    FractureTurns = 2;
                    break;
            }
            return result;
        }

        void ClearChill()
        {
            if (Frozen) FreezeImmunity = Mathf.Max(FreezeImmunity, 2);
            Frozen = false;
            frozenConsumed = false;
            Chill = ChillTurns = 0;
        }

        // 적 턴 시작 시 소비하되 면역/약화는 턴 종료까지 유지한다.
        public bool BeginEnemyTurn()
        {
            bool skip = Frozen && !frozenConsumed;
            frozenConsumed = skip;
            attackUsed = false;
            return skip;
        }

        public int GetAttackDamage(int damage)
        {
            attackUsed = true;
            return Mathf.FloorToInt(damage * AttackMultiplier);
        }

        public int EndEnemyTurn()
        {
            if (frozenConsumed) Frozen = false;
            frozenConsumed = false;
            int damage = BurnTurns > 0 ? BurnDamage : 0;
            BurnTurns = Mathf.Max(0, BurnTurns - 1);
            if (BurnTurns == 0) BurnDamage = 0;
            ChillTurns = Mathf.Max(0, ChillTurns - 1);
            if (ChillTurns == 0) Chill = 0;
            ShockTurns = Mathf.Max(0, ShockTurns - 1);
            FractureTurns = Mathf.Max(0, FractureTurns - 1);
            FreezeImmunity = Mathf.Max(0, FreezeImmunity - 1);
            WeakenTurns = attackUsed ? 0 : Mathf.Max(0, WeakenTurns - 1);
            return damage;
        }

        public string Describe()
        {
            var parts = new List<string>();
            if (BurnTurns > 0) parts.Add($"<color=#FF9955>화상 {BurnDamage}×{BurnTurns}</color>");
            if (Chill > 0) parts.Add($"<color=#88DDFF>냉기 {Chill}/{RequiredChill} ({ChillTurns})</color>");
            if (Frozen) parts.Add("<color=#88DDFF>빙결</color>");
            if (ShockTurns > 0) parts.Add($"<color=#FFFF77>감전 ({ShockTurns})</color>");
            if (FractureTurns > 0) parts.Add($"<color=#DDBB88>균열 ({FractureTurns})</color>");
            if (Weakened) parts.Add("<color=#CCAAFF>공격 −30%</color>");
            if (FreezeImmunity > 0 && !Frozen) parts.Add("빙결 면역");
            return string.Join(" · ", parts);
        }
    }
}
