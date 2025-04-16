using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private bool canDamage = false;

    public void EnableDamage()
    {
        canDamage = true;
    }

    public void DisableDamage()
    {
        canDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            // 造成一次伤害后关闭
            canDamage = false;

            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null)
            {
                player.Hurt(30);  // Boss 攻击伤害
                Debug.Log("Boss攻击命中玩家，造成30伤害");
            }
        }
    }
}