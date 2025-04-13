using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : WeaponBase
{
  [SerializeField] GameObject SightCanvas;
  [SerializeField] GameObject[] Renders;
  //瞄准状态
  private bool isAim =false;
    public override void Enter()
    {
     if(isAim) StopAim();
     isAim = false; 
     base.Enter();
    }
   public override void OnEnterPlayeState(PlayerState playerState)
   {
    Debug.Log($"[Sniper] OnEnterPlayeState: {playerState}"); 
 
        switch(playerState)
        {
            case PlayerState.Shoot:
             if(isAim)
             {
              StopAim();
             }
             isAim =false;
             Debug.Log("[Sniper] Calling OnLeftAttack() ...");  // <<< 调试日志
             OnLeftAttack();
            break;
            case PlayerState.Reload:
            if(isAim) StopAim();
            isAim = false;
             PlayAudio(2);
             animator.SetBool("Reload",true);
             Debug.Log("[Sniper] Reload triggered.");  // <<< 调试日志
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
          if(standby_BulletNum > 0 && Input.GetKeyDown(KeyCode.R))
          {
            player.ChangePlayerState(PlayerState.Reload);
            return;
          }
          
          
          
           canShoot = true;
            if(canShoot && curr_BulletNum>0 &&Input.GetMouseButton(0))
            {
                 Debug.Log("[Sniper] Detected MouseButton(0), changing to Shoot...");
                player.ChangePlayerState(PlayerState.Shoot);
            }
            
            //开关镜
            if(canShoot && curr_BulletNum >0 &&Input.GetMouseButton(1))
            {
              isAim = !isAim;

              if(isAim) StartAim();              
              else StopAim();

            }
            
            break;

        }

    }
    private void StartAim()
    {
      //播放动画
    animator.SetBool("Aim",true);
      //关闭效果火花
      wanrShootEF =false;

    }
    private void StopAim()
    {
    //停止播放动画
    animator.SetBool("Aim",false);
    //开启效果火花
    wanrShootEF =true;     
    //隐藏所有渲染器
    for(int i = 0; i< Renders.Length;i++)
    {
      Renders[i].SetActive(true);
    }
    //关闭狙击镜
    SightCanvas.SetActive(false);
    //设计镜头缩放
    player.SetCameraView(60);


    }


    #region 动画事件

    private void StartLoad()
    {
      PlayAudio(3);
    }
  private void AimOver()
  {
    StartCoroutine(DoAim());
  }

  IEnumerator DoAim()
  {
    //隐藏所有渲染器
    for(int i = 0; i< Renders.Length;i++)
    {
      Renders[i].SetActive(false);
    }
    //停留一点时间
    yield return new WaitForSeconds(0.1f);
    //显示狙击镜
    SightCanvas.SetActive(true);
    //设计镜头缩放
    player.SetCameraView(30);
  }


    #endregion
}
