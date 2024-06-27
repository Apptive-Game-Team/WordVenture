using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicBattle
{

    public enum MagicType
    {
        NONE = 0,
        SHOOT = 1,
        HEAL = 2,
        SUMMON = 3,
        DROP = 4,
    }

    public enum ElementalType
    {
        NONE = 0,
        FIRE = 1,
        ICE = 2,
        ROCK = 3,
        LIGHTNIG = 4,
    }

    public enum TargetType
    {
        NONE = 0,
        ENEMY = 1,
        ALLY = 2,
        ME = 3,
    }


    public interface IMagic
    {
        public void OnCast();
    }


    public abstract class Magic : IMagic
    {
        public MagicType magicType;
        public ElementalType elementalType;
        public TargetType targetType;

        protected DamageComponent damage;

        public abstract void OnCast();
    }

    public class DamageComponent
    {
        public float damage;
        public ElementalType elementalType;
    }


    public class ShootMagic : Magic
    {
        public override void OnCast()
        {
            //투사체를 생성.

            damage = new DamageComponent();
        }
    }

}

