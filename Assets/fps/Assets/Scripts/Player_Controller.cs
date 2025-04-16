    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public enum PlayerState
{
    Move,
    Shoot,
    Reload
}
public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
   
    [SerializeField] FirstPersonController firstPersonController;
   [SerializeField] Image crossImage;
   [SerializeField]Camera[] cameras;
   [SerializeField] WeaponBase[] weapons;
   private int CurrentWeaponIndex = -1; //当前武器
   private int previousWeaponIndex = -1;
   private bool canChangeWeapon = true; //能否切换武器

   public PlayerState PlayerState;

   //修改玩家状态
   public void ChangePlayerState(PlayerState newState)
   {
     Debug.Log($"[Player_Controller] ChangePlayerState -> {newState}");  // <<< 调试日志
    PlayerState = newState;
    //武器在进入某个状态时候，或许需要做事情
    weapons[CurrentWeaponIndex].OnEnterPlayeState(newState);
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



    }





    private void Update()
    {
                //驱动武器层
        weapons[CurrentWeaponIndex].OnUpdatePlayeState(PlayerState);
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
    public void Hurt(float damage)
    {

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
