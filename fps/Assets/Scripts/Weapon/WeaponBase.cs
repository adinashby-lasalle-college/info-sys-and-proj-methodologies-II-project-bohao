using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using System;

//武器基类
public abstract class WeaponBase : MonoBehaviour
{
    
    #region 弹夹
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioSource audioSource;
    protected Player_Controller player;
    protected int curr_BulletNum;//当前子弹
    public int curr_MaxBulletNum;//最大子弹数量
    protected int standby_BulletNum;  //备用子弹
    public int standby_MaxBulletNum;  //备用子弹上限
    #endregion
    

    #region 射击
    public int attackValue;//攻击力
    public bool wantCrosshair;//准星
    public bool wantBullet; //是否需要子弹
    public bool wanrShootEF;//特效
    public bool wantRecoil;//后坐力
    public float recoilStrength;//后坐力强度
    public bool canThoughWall;//射击穿墙
    #endregion

    #region 效果
    public AudioClip[] audioClips;//单个武器的音效

    #endregion

    public virtual void Init(Player_Controller player)
    {
        this.player = player;
        //初始化子弹
        curr_BulletNum = curr_MaxBulletNum;
        standby_BulletNum = standby_MaxBulletNum;
    }

    public abstract void OnEnterPlayeState(PlayerState playerState);
    public abstract void OnUpdatePlayeState(PlayerState playerState);
    public virtual  void Enter()
    {
       //初始化武器
       player.InitForEnterWeapon(wantCrosshair,wantBullet);
       
       
        gameObject.SetActive(true);
    }
    private Action onExitOver;
    public virtual void Exit(Action onExitOver)
    {
        animator.SetTrigger("Exit");
        this.onExitOver = onExitOver;
        player.ChangePlayerState(PlayerState.Move);
    }

    #region 动画事件
    private void EnterOver()
    {

    }
    private void ExitOver()
    {
        
    }

#endregion
    
    
}
