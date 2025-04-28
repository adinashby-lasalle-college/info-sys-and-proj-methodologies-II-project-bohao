using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private bool canDamage = false; // 当前攻击动作是否允许造成伤害
    private bool hasDamaged = false; // 当前攻击动作是否已经造成过伤害

    public int AttackDamage = 20; // 可以设定伤害

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return; // 如果当前攻击阶段不能造成伤害，直接跳过

        if (!hasDamaged && other.CompareTag("Player"))
        {
            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null)
            {
                player.Hurt(AttackDamage); // 对玩家造成伤害
                hasDamaged = true; // 标记为已经造成过伤害
            }
        }
    }

    // 动画事件调用
    public void EnableDamage()
    {
        canDamage = true;
        hasDamaged = false;
    }

    public void DisableDamage()
    {
        canDamage = false;
        hasDamaged = false;
    }
}

