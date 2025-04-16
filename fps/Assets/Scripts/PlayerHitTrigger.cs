using UnityEngine;

public class PlayerHitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 检查是否是 Boss 的攻击碰撞体
        if (other.CompareTag("BossAttack"))
        {
            Debug.Log("💥 玩家被Boss攻击命中！");

            // 调用玩家受伤逻辑
            if (Player_Controller.Instance != null)
            {
                Player_Controller.Instance.Hurt(30); // 你可以更改伤害数值
            }
        }
    }
}
