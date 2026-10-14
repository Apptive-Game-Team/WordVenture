using UnityEngine;

namespace Combat.Enemies
{
    public class MagicEnemy : Enemy
    {
        [SerializeField] GameObject fireShoot;

        public override string AttackDescription => "사거리 안까지 다가와 마법 탄을 쏜다. 탄은 처음 닿은 아군 슬라임이나 워드에게 맞는다.";

        protected override void Start()
        {
            base.Start();
        }

        public override void Attack(float distanceToFrontLine)
        {
            Animator.RangeAttack();
            GameObject projectile = Instantiate(fireShoot, transform.position,Quaternion.identity);
            projectile.GetComponent<EnemyProjectile>().InitProjectileDamage(AttackDamage);
        }
        //private void OnTriggerEnter2D(Collider2D other)
        //{
        //    if (other.CompareTag("Attack"))
        //    {
        //        hp -= 1;
        //    }
        //    if (other.CompareTag("Heal"))
        //    {
        //        hp += 1;
        //    }
        //    if (hp <= 0)
        //    {
        //        Destroy(gameObject);
        //    }
        //}
    }
}
