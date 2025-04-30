using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance;
    public GameObject prefab_Zombie;
    public List<ZombieController> zombies;//当前场景中僵尸

    private Queue<ZombieController> zombiePool = new Queue<ZombieController>();//备用僵尸
    public Transform Pool;
    
    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        // 确保所有僵尸预制件都有Zombie标签 (注意大写Z)
        if (prefab_Zombie != null && prefab_Zombie.tag != "Zombie")
        {
            prefab_Zombie.tag = "Zombie";
            Debug.Log("Set zombie prefab tag to 'Zombie'");
        }
        
        StartCoroutine(CheckZombie());
    }

    // 检查僵尸
    IEnumerator CheckZombie()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            // 僵尸数量不够，产生僵尸
            if (zombies.Count<3)
            {
                // 池子里面有，从池子拿
                if (zombiePool.Count>0)
                {
                    ZombieController zb = zombiePool.Dequeue();
                    zb.transform.SetParent(transform);
                    zb.transform.position = GameManager.Instance.GetPoints();
                    zombies.Add(zb);
                    zb.gameObject.SetActive(true);
                    zb.Init();
                    
                    // 确保激活的僵尸有正确的标签 (注意大写Z)
                    if (zb.gameObject.tag != "Zombie")
                    {
                        zb.gameObject.tag = "Zombie";
                    }
                    
                    yield return new WaitForSeconds(2);
                }
                // 池子没有，就实例化
                else
                {
                    GameObject zb = Instantiate(prefab_Zombie, GameManager.Instance.GetPoints(), Quaternion.identity, transform);
                    
                    // 确保新实例化的僵尸有正确的标签 (注意大写Z)
                    if (zb.tag != "Zombie")
                    {
                        zb.tag = "Zombie";
                    }
                    
                    zombies.Add(zb.GetComponent<ZombieController>());
                }
            }
        }
    }

    public void ZombieDead(ZombieController zombie)
    {
        // 从活动僵尸列表中移除
        zombies.Remove(zombie);
        
        // 更新任务进度 - 当僵尸死亡时通知任务系统
        if (MissionSystem.Instance != null)
        {
            // 记录僵尸标签以便调试
            string zombieTag = zombie.gameObject.tag;
            Debug.Log($"Zombie with tag '{zombieTag}' died. Updating mission progress.");
            
            // 使用僵尸的标签更新任务进度
            MissionSystem.Instance.UpdateMissionProgress(zombieTag);
        }
        else
        {
            Debug.LogWarning("MissionSystem not found when zombie died!");
        }
        
        // 将僵尸返回对象池
        zombiePool.Enqueue(zombie);
        zombie.gameObject.SetActive(false);
        zombie.transform.SetParent(Pool);
    }
}