using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
   
   [SerializeField] Image crossImage;
    public void Awake()
    {
        Instance = this;



    }





    private void Update()
    {
        //testing
        if(Input.GetMouseButtonDown(0))
        {
            StartShootRecoil(1);
        }
    }

    public void StartShootRecoil(float recoil = 1)
    {
        StartCoroutine(ShootRecoil_Cross(recoil));
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




}
