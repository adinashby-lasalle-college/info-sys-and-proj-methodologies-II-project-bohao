using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioSource audioSource;
    protected Player_Controller player;

    protected int curr_BulletNum;
    public int curr_MaxBulletNum;
    protected int standby_BulletNum;
    public int standby_MaxBulletNum;

    public int attackValue;
    public bool wantCrosshair;
    public bool wantBullet;
    public bool wanrShootEF;
    public bool wantRecoil;
    public float recoilStrength;
    public bool canThoughWall;
    protected bool canShoot = false;
    private bool wantReloadOnEnter = false;

    [SerializeField] AudioClip[] audioClips;
    [SerializeField] protected GameObject[] prefab_BulletEF;
    [SerializeField] protected GameObject shootEF;

    public virtual void Init(Player_Controller player)
    {
        this.player = player;
        curr_BulletNum = curr_MaxBulletNum;
        standby_BulletNum = standby_MaxBulletNum;
    }

    public abstract void OnEnterPlayeState(PlayerState playerState);
    public abstract void OnUpdatePlayeState(PlayerState playerState);

    public virtual void Enter()
    {
        gameObject.SetActive(true);
        if (audioSource != null) audioSource.enabled = true;
        player.InitForEnterWeapon(wantCrosshair, wantBullet);
        canShoot = false;

        if (wantBullet)
        {
            player.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
            if (curr_BulletNum > 0)
                PlayAudio(0);
            else
                wantReloadOnEnter = true;
        }

        if (shootEF != null)
            shootEF.SetActive(false);

        animator.SetTrigger("Enter");
    }

    private System.Action onExitOver;

    public virtual void Exit(System.Action onExitOver)
    {
        animator.SetTrigger("Exit");
        this.onExitOver = onExitOver;
        player.ChangePlayerState(PlayerState.Move);
    }

    private void EnterOver()
    {
        canShoot = true;
        if (wantReloadOnEnter)
            player.ChangePlayerState(PlayerState.Reload);
    }

    private void ExitOver()
    {
        gameObject.SetActive(false);
        onExitOver?.Invoke();
    }

    public virtual void ShootOver()
    {
        canShoot = true;
        if (wanrShootEF) shootEF.SetActive(false);
        if (player.PlayerState == PlayerState.Shoot)
            player.ChangePlayerState(PlayerState.Move);
    }

    protected virtual void OnLeftAttack()
    {
        if (wantBullet)
        {
            curr_BulletNum--;
            player.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        }

        canShoot = false;
        animator.SetTrigger("Shoot");
        if (wanrShootEF) shootEF.SetActive(true);
        if (wantRecoil) player.StartShootRecoil(recoilStrength);

        PlayAudio(1);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (canThoughWall)
        {
            foreach (var hitInfo in Physics.RaycastAll(ray, 1500f))
                HitGameObject(hitInfo);
        }
        else if (Physics.Raycast(ray, out RaycastHit hit, 1500f))
        {
            HitGameObject(hit);
        }
    }

    private void HitGameObject(RaycastHit hitInfo)
    {
        if (hitInfo.collider.CompareTag("Zombie"))
        {
            GameObject go = Instantiate(prefab_BulletEF[1], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);

            ZombieController zombie = hitInfo.collider.GetComponent<ZombieController>() ??
                                       hitInfo.collider.GetComponentInParent<ZombieController>();
            if (zombie != null)
                zombie.Hurt(attackValue);
        }
        else if (hitInfo.collider.CompareTag("Boss"))
        {
            GameObject go = Instantiate(prefab_BulletEF[1], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);

            BossController boss = hitInfo.collider.GetComponent<BossController>() ??
                                  hitInfo.collider.GetComponentInParent<BossController>();

            if (boss != null)
            {
                bool isHeadshot = hitInfo.collider == boss.headCollider;

                boss.Hurt(attackValue, isHeadshot);

                Vector3 hitDirection = (boss.transform.position - Camera.main.transform.position).normalized;
                boss.ApplyKnockback(hitDirection, isHeadshot ? 5f : 2.5f); // 爆头击退更大
            }
        }
        else if (hitInfo.collider.gameObject != player.gameObject)
        {
            GameObject go = Instantiate(prefab_BulletEF[0], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
        }
    }

    protected void PlayAudio(int index)
    {
        if (audioClips.Length > index)
            audioSource.PlayOneShot(audioClips[index]);
    }

    private void ReloadOver()
    {
        int want = curr_MaxBulletNum - curr_BulletNum;
        if (standby_BulletNum - want < 0)
            want = standby_BulletNum;

        standby_BulletNum -= want;
        curr_BulletNum += want;

        player.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        animator.SetBool("Reload", false);
        canShoot = true;
        player.ChangePlayerState(PlayerState.Move);
    }
}
