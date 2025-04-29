using System.Collections;
using UnityEngine;

public class Pistol : WeaponBase
{
    private bool isTripleShooting = false;
    private int tripleShootCount = 0;
    private float tripleShootInterval = 0.1f; // 三连发每发间隔0.1秒
    private Vector3 originalPosition;
    private bool isMovementLocked = false;

    public override void OnEnterPlayeState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Shoot:
                OnLeftAttack();
                break;
            case PlayerState.Reload:
                PlayAudio(2);
                animator.SetBool("Reload", true);
                break;
        }
    }

    public override void OnUpdatePlayeState(PlayerState playerState)
    {
        if (isMovementLocked)
            return; // 如果锁定，直接禁止操作

        switch (playerState)
        {
            case PlayerState.Move:
                if (curr_BulletNum == 0 && standby_BulletNum > 0)
                {
                    player.ChangePlayerState(PlayerState.Reload);
                    return;
                }
                if (standby_BulletNum > 0 && Input.GetKeyDown(KeyCode.R))
                {
                    player.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                canShoot = true;

                if (canShoot && curr_BulletNum > 0)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        // 左键单发
                        player.ChangePlayerState(PlayerState.Shoot);
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        // 右键三连发
                        if (!isTripleShooting)
                        {
                            StartCoroutine(TripleShoot());
                        }
                    }
                }

                break;
        }
    }

    private IEnumerator TripleShoot()
    {
        isTripleShooting = true;
        tripleShootCount = 3;
        isMovementLocked = true; // 锁定移动
        originalPosition = player.transform.position; // 记录原位置

        while (tripleShootCount > 0 && curr_BulletNum > 0)
        {
            player.ChangePlayerState(PlayerState.Shoot);

            //  小抖动后坐力（视觉）
            ApplyRecoilEffect();

            tripleShootCount--;
            yield return new WaitForSeconds(tripleShootInterval);
        }

        isTripleShooting = false;
        isMovementLocked = false;
    }

    private void ApplyRecoilEffect()
    {
        // 简单的后坐力效果，可以自己改强度
        Vector3 recoil = new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            0f
        );

        player.transform.position += recoil;
    }
}
