using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有武器的基类，提供进场 / 退场 / 射击等基础逻辑
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    #region 参考组件
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioSource audioSource;
    protected Player_Controller player;
    #endregion

    #region 弹夹 & 子弹
    // 当前子弹数
    protected int curr_BulletNum;
    // 单个弹夹能容纳的最大子弹数
    public int curr_MaxBulletNum;

    // 备用子弹
    protected int standby_BulletNum;
    // 备用子弹上限
    public int standby_MaxBulletNum;
    #endregion

    #region 射击配置
    public int attackValue;      // 攻击力
    public bool wantCrosshair;   // 是否需要准星
    public bool wantBullet;      // 是否需要消耗子弹
    public bool wanrShootEF;     // 是否有开火特效
    public bool wantRecoil;      // 是否有后坐力
    public float recoilStrength; // 后坐力强度
    public bool canThoughWall;   // 是否穿墙射击
    #endregion

    #region 效果 & 音效
    public AudioClip[] audioClips;  // 本武器的多段音效
    #endregion

    /// <summary>
    /// 初始化，每把武器在场景加载或绑定后调用
    /// </summary>
    /// <param name="player">所在的玩家控制器引用</param>
    public virtual void Init(Player_Controller player)
    {
        this.player = player;
        // 初始化当前子弹与备用子弹
        curr_BulletNum = curr_MaxBulletNum;
        standby_BulletNum = standby_MaxBulletNum;
    }

    /// <summary>
    /// 当玩家状态切换时，武器可以做一些事（抽象，需要子类具体实现）
    /// </summary>
    public abstract void OnEnterPlayeState(PlayerState playerState);

    /// <summary>
    /// 每帧Update，武器根据玩家状态进行逻辑（抽象，需要子类具体实现）
    /// </summary>
    public abstract void OnUpdatePlayeState(PlayerState playerState);

    /// <summary>
    /// 切换到本武器时调用（进场动画、启用UI等）
    /// </summary>
    public virtual void Enter()
    {
        // 给UI显示准星或子弹
        player.InitForEnterWeapon(wantCrosshair, wantBullet);

        // 启用武器物体
        gameObject.SetActive(true);

        // 让Animator播放 进场(Draw/Enter)动画
        // 需在Animator里有"Enter" Trigger，并设置AnyState->EnterState过渡
        animator.SetTrigger("Enter");
    }

    // 退场完毕的回调
    private Action onExitOver;

    /// <summary>
    /// 切换武器时先调用Exit，让退场动画播放结束再隐藏并回调
    /// </summary>
    /// <param name="onExitOver">退场结束时回调</param>
    public virtual void Exit(Action onExitOver)
    {
        // 设置Animator播放退场动画
        // 需在Animator里有"Exit" Trigger，并设置AnyState->ExitState过渡
        animator.SetTrigger("Exit");

        // 记录回调方法，等动画末帧调用ExitOver()事件时再执行
        this.onExitOver = onExitOver;

        // 比如切武器时，玩家状态可先回到Move
        player.ChangePlayerState(PlayerState.Move);
    }

    #region 动画事件（必须在Animator动画末帧添加）
    /// <summary>
    /// 如果进场动画结束时需要做一些事，就在动画末帧加个Event调用EnterOver()
    /// </summary>
    private void EnterOver()
    {
        // 视具体需求处理
        Debug.Log($"{gameObject.name} Enter animation finished.");
    }

    /// <summary>
    /// 退场动画末帧调用事件，用于隐藏武器并触发回调
    /// </summary>
    private void ExitOver()
    {
        // 隐藏旧武器
        gameObject.SetActive(false);

        // 调用在Exit()里设置的回调，把控制权还给Player_Controller
        onExitOver?.Invoke();
    }
    #endregion
}
