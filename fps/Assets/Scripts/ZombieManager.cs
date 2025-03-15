using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieManager : MonoBehaviour
{
    public static ZombieManager Instance;
    public GameObject prefab_Zombie;
    public List<ZombieController> zombies;

    private Queue<ZombieController> zombiePool = new Queue<ZombieController>();
    public Transform Pool;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
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
                    yield return new WaitForSeconds(2);
                }
                // 池子没有，就实例化
                else
                {
                    GameObject zb = Instantiate(prefab_Zombie, GameManager.Instance.GetPoints(), Quaternion.identity, transform);
                    zombies.Add(zb.GetComponent<ZombieController>());
                }
            }
        }
    }

    public void ZombieDead(ZombieController zombie)
    {
        zombies.Remove(zombie);
        zombiePool.Enqueue(zombie);
        zombie.gameObject.SetActive(false);
        zombie.transform.SetParent(Pool);

    }
}
