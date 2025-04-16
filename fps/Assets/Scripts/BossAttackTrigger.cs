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
            // ���һ���˺���ر�
            canDamage = false;

            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null)
            {
                player.Hurt(30);  // Boss �����˺�
                Debug.Log("Boss����������ң����30�˺�");
            }
        }
    }
}