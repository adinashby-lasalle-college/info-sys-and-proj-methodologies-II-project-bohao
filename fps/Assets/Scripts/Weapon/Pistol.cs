using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pistol : WeaponBase
{

   public override void OnEnterPlayeState(PlayerState playerState)
   {
        switch(playerState)
        {
            case PlayerState.Shoot:
                OnLeftAttack();
            break;
            case PlayerState.Reload:
             PlayAudio(2);
             animator.SetBool("Reload",true);
            break;

        }

    }
    public override void OnUpdatePlayeState(PlayerState playerState)
    {
        switch(playerState)
        {
            case PlayerState.Move:
          
          if(curr_BulletNum ==0 && standby_BulletNum > 0)
          {
            player.ChangePlayerState(PlayerState.Reload);
            return;
          }
          if(standby_BulletNum > 0 && Input.GetKeyDown(KeyCode.E))
          {
            player.ChangePlayerState(PlayerState.Reload);
            return;
          }
          
          
          
           canShoot = true;
            if(canShoot && curr_BulletNum>0 &&Input.GetMouseButton(0))
            {
                player.ChangePlayerState(PlayerState.Shoot);
            }
        
            
            break;

        }

    }



}