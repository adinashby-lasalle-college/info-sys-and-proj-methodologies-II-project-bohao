using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private bool canDamage = false; // ��ǰ���������Ƿ���������˺�
    private bool hasDamaged = false; // ��ǰ���������Ƿ��Ѿ���ɹ��˺�

    public int AttackDamage = 20; // �����趨�˺�

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return; // �����ǰ�����׶β�������˺���ֱ������

        if (!hasDamaged && other.CompareTag("Player"))
        {
            Player_Controller player = other.GetComponent<Player_Controller>();
            if (player != null)
            {
                player.Hurt(AttackDamage); // ���������˺�
                hasDamaged = true; // ���Ϊ�Ѿ���ɹ��˺�
            }
        }
    }

    // �����¼�����
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

