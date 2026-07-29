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

        protected int Hp = 100;
        protected int MaxHp = 100;
        protected int Damage;

        public int shield = 0;

        public void UpdateIndicator()
        {
            HpText.SetText(Hp.ToString());
        }

        private void Awake()
        {
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
            Hp -= damage;
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

