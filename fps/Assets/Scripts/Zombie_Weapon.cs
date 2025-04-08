using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_Weapon : MonoBehaviour
{
    private ZombieController zombie;
    private BoxCollider boxCollider;

    public void Init(ZombieController zombie)
    {
        this.zombie = zombie;
        boxCollider = GetComponent<BoxCollider>();
    }
    public void StartAttack()
    {
        isAttacked = false;
        boxCollider.enabled = true;
    }
    public void EndAttack()
    {
        boxCollider.enabled = false;
    }
    private bool isAttacked = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!isAttacked&&other.gameObject.tag == "Player")
        {
            isAttacked = true;
            Player_Controller.Instance.Hurt(10);
        }
    }
}
