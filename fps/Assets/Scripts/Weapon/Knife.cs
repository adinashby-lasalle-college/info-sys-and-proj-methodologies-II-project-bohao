using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class Knife : WeaponBase
{
    [SerializeField] Knife_Collider knife_Collider;
    private bool isLeftAttack;
   public override void Init(Player_Controller player)
   {
        base.Init(player);
        knife_Collider.Init(this);

    }
    public override void OnEnterPlayeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Shoot:
               
                if (isLeftAttack)
                {
                    OnLeftAttack();
                }
                else
                {
                    OnRightAttack();
                }
                
                break;


        }

    }
    public override void OnUpdatePlayeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Move:
                if(canShoot && Input.GetMouseButton(0))
                {
                    isLeftAttack = true;
                    player.ChangePlayerState(PlayerState.Shoot);
                    return;
                }
          
                if (canShoot && Input.GetMouseButton(1))
                {
                    isLeftAttack = false;
                    player.ChangePlayerState(PlayerState.Shoot);
                    return;
                }

                break;

        }

    }

    public void HitTarget(GameObject hitObj,Vector3 efPos)
    {
        PlayAudio(2);
        //ÅÐ¶ÏÊÇ·ñ¹¥»÷µ½½©Ê¬
        if (hitObj.CompareTag("Zombie"))
        {
            GameObject go = Instantiate(prefab_BulletEF[1], efPos, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
            ZombieController zombie = hitObj.GetComponent<ZombieController>();
            if (zombie == null) zombie = hitObj.GetComponentInParent<ZombieController>();
            zombie.Hurt(attackValue);
        }
        else if (hitObj != player.gameObject)
        {
            GameObject go = Instantiate(prefab_BulletEF[0], efPos, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
        }
    }
    protected override void OnLeftAttack()
    {
        PlayAudio(1);
        animator.SetBool("Shoot",true);
       animator.SetBool("IsLeft", true);
        knife_Collider.StartHit();
    }
    private void OnRightAttack()
    {
        PlayAudio(1);
        animator.SetBool("Shoot",true);
        animator.SetBool("IsLeft", false);
        knife_Collider.StartHit();
    }

    public override void ShootOver()
    {
        base.ShootOver();
        knife_Collider.StopHit();
        animator.SetBool("Shoot", false);
    }
    public override void Enter()
    {
        base.Enter();        
        PlayAudio(0);        
    }
}
