using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deck_Manage;
using Enemy;
using System;

namespace MagicAAffinity
{

    [Serializable]
    public class FloatList
    {
        public List<float> targets = new List<float>();
    }

    [Serializable]
    public class Table
    {
        public List<FloatList> rows = new List<FloatList>();
    }


    [CreateAssetMenu]
    public class MagicAffinityTable : ScriptableObject
    {
        //[SerializeField] Dictionary<MagicType ,Dictionary<MagicType, float>> table = new Dictionary<MagicType, Dictionary<MagicType, float>>();
        //[SerializeField] List<List<float>> table = new List<List<float>>();
        [SerializeField]
        private Table table = new Table();

        private int MagicToNum(MagicType type)
        {
            switch (type)
            {
                case MagicType.Ice:
                    return 0;
                case MagicType.Lightning:
                    return 1;
                case MagicType.Fire:
                    return 2;
                case MagicType.Rock:
                    return 3;
                case MagicType.Holy:
                    return 4;
            }
            return -1;
        }

        public float GetAffinity(MagicType magic, MagicType target)
        {
            return table.rows[MagicToNum(magic)].targets[MagicToNum(target)];
        }
    }

}

