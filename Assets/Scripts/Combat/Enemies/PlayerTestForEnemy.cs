using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Combat.Enemies
{
    public class Player : MonoBehaviour
    {
        Animator animator;

        public static Player PlayerInt()
        {
            return _instance;
        }

        static Player _instance;

        protected TMP_Text HpText;

        protected int Hp;
        protected int MaxHp;
        protected int Damage;

        public int shield = 0;

        public void UpdateIndicator()
        {
            HpText.SetText(Hp.ToString());
        }

        private void Awake()
        {
            MaxHp = PlayerUpgrades.PlayerMaxHp;
            Hp = MaxHp;
            InitIndicators();
            animator = GetComponent<Animator>();
            _instance = this;
        }

        public void AttackAnima()
        {
            animator.SetTrigger("Attack");
        }


        protected void Death()
        {
            animator.SetTrigger("Death");
            //gameObject.SetActive(false);
            SceneManager.LoadScene("GameOverScene");
        }

        public void TakeHit(int damage)
        {
            // 음수 피해는 회복이라 최대 HP를 넘지 않게 자른다.
            Hp = Mathf.Min(Hp - damage, MaxHp);
            if (Hp <= 0)
            {
                Death();
                return;
            }
            else if (damage > 0)
            {
                animator.SetTrigger("GetHit");
                UpdateIndicator();
            } else
            {
                UpdateIndicator();
            }

        }

        private void InitIndicators()
        {
            HpText = gameObject.GetComponentInChildren<TMP_Text>();

            HpText.SetText(MaxHp.ToString());
        }
    }
}

