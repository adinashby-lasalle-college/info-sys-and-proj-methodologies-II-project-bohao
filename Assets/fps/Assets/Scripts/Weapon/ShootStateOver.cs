using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ShootStateOver : StateMachineBehaviour
{
    // 当动画播放到末尾并即将切换到下一个状态时，会调用这个方法
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 尝试在与 Animator 相同或上层的对象上查找 WeaponBase 脚本
        WeaponBase weapon = animator.GetComponentInParent<WeaponBase>();
        if (weapon != null)
        {
            // 直接调用 ShootOver()，相当于动画结束事件
            weapon.ShootOver();
        }
    }
}
