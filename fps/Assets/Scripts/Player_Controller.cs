using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;

public enum PlayerState
{
    Move,
    Shoot,
    Reload,
    Paused // 添加暂停状态
}
public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
   
    [SerializeField] FirstPersonController firstPersonController;
    [SerializeField] Image crossImage;
    [SerializeField] Camera[] cameras;
    [SerializeField] WeaponBase[] weapons;
    [SerializeField] int hp = 100;
    [SerializeField] RawImage damageEffectPanel; // 伤害效果面板引用
    [SerializeField] AudioClip hurtSoundClip; // 受伤音效
    
    private int CurrentWeaponIndex = -1; //当前武器
    private int previousWeaponIndex = -1;
    private bool canChangeWeapon = true; //能否切换武器
    private bool isDead = false;
    private Coroutine damageEffectCoroutine; // 协程引用
    
    // 公共方法用于检查玩家是否死亡
    public bool IsDead()
    {
        return isDead;
    }

    public PlayerState PlayerState;

   //修改玩家状态
   public void ChangePlayerState(PlayerState newState)
   {
     Debug.Log($"[Player_Controller] ChangePlayerState -> {newState}");  // <<< 调试日志
     PlayerState = newState;
     
     // 如果是暂停状态，不需要通知武器
     if (newState != PlayerState.Paused && CurrentWeaponIndex >= 0)
     {
        //武器在进入某个状态时候，或许需要做事情
        weapons[CurrentWeaponIndex].OnEnterPlayeState(newState);
     }
   }
   
    public void Awake()
    {
        Instance = this;
        //初始化所有武器
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Init(this);   
        }
        PlayerState = PlayerState.Move;
        //默认第一把武器
        ChangeWeapon(1);//测试手枪

        // 初始隐藏鼠标
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // 如果处于暂停状态，不处理武器和切换逻辑
        if (PlayerState == PlayerState.Paused)
            return;
            
        //驱动武器层
        if (CurrentWeaponIndex >= 0)
        {
            weapons[CurrentWeaponIndex].OnUpdatePlayeState(PlayerState);
        }
        
        //按键检查切换武器
        if(canChangeWeapon == false) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))ChangeWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))ChangeWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))ChangeWeapon(2);
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(previousWeaponIndex >=0)ChangeWeapon(previousWeaponIndex);
        }
    }
    
    // 暂停游戏
    public void PauseGame()
    {
        // 记住之前的状态，暂停时切换到暂停状态
        ChangePlayerState(PlayerState.Paused);
        
        // 禁用FirstPersonController
        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;
        }
    }
    
    // 继续游戏
    public void ResumeGame()
    {
        // 返回到移动状态
        ChangePlayerState(PlayerState.Move);
        
        // 重新启用FirstPersonController
        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;
        }
    }

#region 后坐力
    public void StartShootRecoil(float recoil = 1)
    {
        StartCoroutine(ShootRecoil_Cross(recoil));
        if(shootRecoil_CameraCoroutine!=null)StopCoroutine(shootRecoil_CameraCoroutine);
        shootRecoil_CameraCoroutine = StartCoroutine(ShootRecoil_Camera(recoil));
    }

    IEnumerator ShootRecoil_Cross(float recoil)
    {
        Vector2 scale = crossImage.transform. localScale;

        while (scale.x < 1.3f)
        {
            yield return null;
            scale.x += Time. deltaTime * 3;
            scale.y += Time. deltaTime * 3;
            crossImage.transform.localScale =scale; 
        }
        while(scale.x > 1)
        {
             yield return null;
            scale.x -= Time. deltaTime * 3;
            scale.y -= Time. deltaTime * 3;
            crossImage.transform.localScale =scale;   
        }
        crossImage.transform.localScale = Vector2. one;  
    }
   
    private Coroutine shootRecoil_CameraCoroutine;
 
    IEnumerator ShootRecoil_Camera(float recoil)
    {
        float xOffset = Random.Range(0.3f, 0.6f) * recoil;
        float yOffset = Random.Range(-0.15f, 0.15f) * recoil;
        firstPersonController.xRotOffset = xOffset;
        firstPersonController.yRotOffset = yOffset;
        for(int i = 0;i < 6; i++)
        {
            yield return 0;
        }
        firstPersonController.xRotOffset = 0;
        firstPersonController.yRotOffset = 0;
    }
    #endregion
    public void Hurt(int damage)
    {
        if (isDead) return;

        hp -= damage;
        if (hp < 0) hp = 0;

        UI_MainPanel.Instance.UpdateHP_Text(hp);
        
        // 显示伤害效果
        ShowDamageEffect();
        
        // 播放受伤音效
        if (hurtSoundClip != null)
        {
            AudioSource.PlayClipAtPoint(hurtSoundClip, transform.position);
        }

        if (hp == 0)
        {
            isDead = true;
            Die();
        }
    }

    // 伤害视觉效果方法
    private void ShowDamageEffect()
    {
        // 如果damageEffectPanel为null，则直接返回
        if (damageEffectPanel == null) return;
        
        // 如果已经有一个协程在运行，先停止它
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        
        // 设置面板的alpha值为65/255
        Color panelColor = damageEffectPanel.color;
        panelColor.a = 45f/255f; // Unity中alpha值是0-1，所以需要除以255
        damageEffectPanel.color = panelColor;
        
        // 启动淡出协程
        damageEffectCoroutine = StartCoroutine(FadeOutDamageEffect());
    }
    
    // 淡出效果协程
    private IEnumerator FadeOutDamageEffect()
    {
        // 等待1秒
        yield return new WaitForSeconds(0.8f);
        
        // 将alpha值设回0
        Color panelColor = damageEffectPanel.color;
        panelColor.a = 0f;
        damageEffectPanel.color = panelColor;
        
        damageEffectCoroutine = null;
    }

    private void Die()
    {
        Debug.Log("[Player] Died");
    
        // 显示鼠标并解除锁定
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 1f;

        // 加载GameOver场景
        SceneManager.LoadScene("GameOver");
    }


    private void ChangeWeapon(int newIndex)
    {
        //是否重复
        if(CurrentWeaponIndex == newIndex) return;
        //上一个武器的索引 = 当前武器
        previousWeaponIndex = CurrentWeaponIndex;
        // 记录新的武器索引
        CurrentWeaponIndex = newIndex;
        //如果是第一次使用
        if(previousWeaponIndex < 0)
        {
            //直接进入当前武器
            weapons[CurrentWeaponIndex].Enter();
        }
        else
        {
            //退出当前武器
            weapons[previousWeaponIndex].Exit(OnWeaponExitOver);
            canChangeWeapon =false;

        }
    }
    private void OnWeaponExitOver()
    {
        canChangeWeapon = true;
        weapons[CurrentWeaponIndex].Enter();
    }

    public void InitForEnterWeapon(bool wantCrosshair,bool wantBullet)
    {
        crossImage.gameObject.SetActive(wantCrosshair);
        UI_MainPanel.Instance.InitForEnterWeapon(wantBullet);
    }

    public void UpdateBulletUI(int curr_BulletNum,int curr_MaxBulletNum,int standby_BulletNum)
    {
        UI_MainPanel.Instance.UpdateCurrBullet_Text(curr_BulletNum,curr_MaxBulletNum);
        UI_MainPanel.Instance.UpdateStandByBullet_Text(standby_BulletNum);

    }
    public void SetCameraView(int value)
    {
        for(int i = 0;i<cameras.Length;i++)
        {
            cameras[i].fieldOfView =value;
        }
    }
}