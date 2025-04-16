using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainPanel : MonoBehaviour
{
    public static UI_MainPanel Instance;
    public Text Hp_Text;
    public Image Hp_CoverImage;    // 添加血量覆盖图层
    public Text CurrBullet_Text;
    public Image CurrBullet_Image;
    public Text StandByBullet_Text;
    public Image StandByBullet_Image;
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHP_Text(int hp)
    {
        Hp_Text.text = hp.ToString();
        if (hp>30)
        {
            Hp_Text.color = Color.white;
        }
        else
        {
            Hp_Text.color = Color.red;
        }
        
        // 更新血量覆盖图层
        if (Hp_CoverImage != null)
        {
            // 假设最大血量为100，计算血量百分比
            float healthPercentage = hp / 100f;
            // 血量降低时，覆盖图增加
            Hp_CoverImage.fillAmount = 1 - healthPercentage;
        }
    }
    
    public void UpdateCurrBullet_Text(int cur,int max)
    {
        CurrBullet_Text.text = cur + "/" + max;
        if (cur < 5)
        {
            CurrBullet_Text.color = Color.red;
        }
        else
        {
            CurrBullet_Text.color = Color.white;
        }
    }

    public void UpdateStandByBullet_Text(int num)
    {
        StandByBullet_Text.text = num.ToString();
        if (num < 30)
        {
            StandByBullet_Text.color = Color.red;
        }
        else
        {
            StandByBullet_Text.color = Color.white;
        }
    }

    /// <summary>
    /// 进入武器的初始化
    /// </summary>

    public void InitForEnterWeapon( bool wantBullet)
    {
        CurrBullet_Text.gameObject.SetActive(wantBullet);
        CurrBullet_Image.gameObject.SetActive(wantBullet);;
        StandByBullet_Text.gameObject.SetActive(wantBullet);
        StandByBullet_Image.gameObject.SetActive(wantBullet);
    }
}