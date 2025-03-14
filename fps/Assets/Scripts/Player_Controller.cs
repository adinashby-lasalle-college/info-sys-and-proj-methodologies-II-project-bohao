using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;


public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
   
    [SerializeField] FirstPersonController firstPersonController;
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
    public void Hurt(float damage)
    {
        
    }
}
