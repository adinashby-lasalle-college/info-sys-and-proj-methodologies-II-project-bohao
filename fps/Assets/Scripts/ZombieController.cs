using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieState
{
    Idle,
    Walk,
    Run,
    Attack,
    Hurt,
    Dead
}

public class ZombieController : MonoBehaviour
{
    [SerializeField]
    private ZombieState zombieState;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    public Zombie_Weapon weapon;

    private int hp = 100;
    public AudioClip[] FootstepAudioClips;  // 行走的音效
    public AudioClip[] IdelAudioClips;      // 待机的音效
    public AudioClip[] HurtAudioClips;      // 受伤的音效
    public AudioClip[] AttackAudioClips;    // 攻击的音效

    private Vector3 target;

    // 状态切换时的逻辑
    public ZombieState ZombieState
    {
        get => zombieState;
        set
        {
            if (zombieState==ZombieState.Dead
               && value!=ZombieState.Idle )
            {
                return;
            }
            zombieState = value;

            switch (zombieState)
            {
                case ZombieState.Idle:
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    navMeshAgent.enabled = false;
                    Invoke("GoWalk", Random.Range(1, 3));
                    break;
                case ZombieState.Walk:
                    animator.SetBool("Walk", true);
                    animator.SetBool("Run", false);
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = 0.3f;
                    // 去一个目标点
                    target = GameManager.Instance.GetPoints();
                    navMeshAgent.SetDestination(target);
                    break;
                case ZombieState.Run:
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", true);
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = 3.5f;
                    break;
                case ZombieState.Attack:
                    navMeshAgent.enabled = true;
                    animator.SetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    break;
                case ZombieState.Hurt:
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    animator.SetTrigger("Hurt");
                    break;
                case ZombieState.Dead:
                    navMeshAgent.enabled = false;
                    animator.SetTrigger("Dead");
                    capsuleCollider.enabled = false;
                    Invoke("Destroy", 5);
                    break;
                default:
                    break;
            }
        }
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        weapon.Init(this);
        ZombieState = ZombieState.Idle;
    }

    // 处理脏数据
    public void Init()
    {
        animator.SetTrigger("Init");
  
        capsuleCollider.enabled = true;
        hp = 100;
        ZombieState = ZombieState.Idle;

    }

    void Update()
    {
        StateForUpdate();
    }

    void StateForUpdate()
    {
        // float dis = Player_Controller.Instance.PlayerState==PlayerState.Shoot?30f: 10f;
        float dis = 10f;
        switch (zombieState)
        {
            case ZombieState.Idle:
                break;
            case ZombieState.Walk:
                if (Vector3.Distance(transform.position, Player_Controller.Instance.transform.position) < dis)
                {
                    // 去追玩家
                    ZombieState = ZombieState.Run;
                    return;
                }
                if (Vector3.Distance(target, transform.position) <= 1)
                {
                    ZombieState = ZombieState.Idle;
                }

                break;
            case ZombieState.Run:
                // 一直追玩家
                navMeshAgent.SetDestination(Player_Controller.Instance.transform.position);
                if (Vector3.Distance(transform.position, Player_Controller.Instance.transform.position) < 2.5f)
                {
                    ZombieState = ZombieState.Attack;
                }

                break;
            case ZombieState.Attack:
                if (animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Attack"
                     && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    ZombieState = ZombieState.Run;
                }
                break;
            case ZombieState.Hurt:
                break;
            case ZombieState.Dead:
                break;
            default:
                break;
        }
    }

    void GoWalk()
    {
        ZombieState = ZombieState.Walk;

    }

    public void Hurt(int value)
    {
        hp -= value;
        if (hp<=0)
        {
            ZombieState = ZombieState.Dead;
        }
        else
        {
            // 击退
            StartCoroutine(MovePuase());
        }
    }

    void Destroy()
    {
        ZombieManager.Instance.ZombieDead(this);
    }

    IEnumerator MovePuase()
    {
        ZombieState = ZombieState.Hurt;
        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(0.5f);
        if (ZombieState!=ZombieState.Run)
        {
            ZombieState = ZombieState.Run;
        }
    
    }


    #region 动画事件
    void IdelAudio()
    {
        if (Random.Range(0, 4) == 1)
        {
            audioSource.PlayOneShot(IdelAudioClips[Random.Range(0, IdelAudioClips.Length)]);
        }
    }
    void FootStep()
    {
        audioSource.PlayOneShot(FootstepAudioClips[Random.Range(0, IdelAudioClips.Length)]);
    }
    private void HurtAudio()
    {
        audioSource.PlayOneShot(HurtAudioClips[Random.Range(0, HurtAudioClips.Length)]);
    }
    private void AttackAudio()
    {
        audioSource.PlayOneShot(AttackAudioClips[Random.Range(0, AttackAudioClips.Length)]);
    }
    public void StartAttack()
    {
        weapon.StartAttack();
    }
    public void EndAttack()
    {
        weapon.EndAttack();
    }

    #endregion
}
