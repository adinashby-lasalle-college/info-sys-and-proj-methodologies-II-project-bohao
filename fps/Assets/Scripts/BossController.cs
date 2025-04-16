using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossState
{
    Idle,
    Walk,
    Run,
    Attack,
    Hurt,
    Dead
}

public class BossController : MonoBehaviour
{
    [SerializeField]
    private BossState bossState;
    private NavMeshAgent navMeshAgent;
    private AudioSource audioSource;
    private Animator animator;
    
    [Header("碰撞器设置")]
    [SerializeField] public CapsuleCollider bodyCollider;  // 身体碰撞盒
    [SerializeField] public BoxCollider headCollider;      // 头部碰撞盒
    [SerializeField] public BoxCollider attackCollider;    // 攻击碰撞盒
    
    [Header("属性设置")]
    [SerializeField] private int maxHp = 200;
    private int hp;
    [SerializeField] private float walkSpeed = 0.3f;
    [SerializeField] private float runSpeed = 2.0f;
    [SerializeField] private float detectRange = 12f;
    [SerializeField] private float attackRange = 5.0f;  // 增加攻击范围
    
    [Header("攻击设置")]
    [SerializeField] private int attackDamage = 20;
    private bool isAttacked = false;
    [SerializeField] private float attackCooldown = 1.0f; // 攻击冷却时间
    private float nextAttackTime = 0f; // 下次可攻击时间
    private Coroutine attackCoroutine; // 跟踪攻击协程
    
    [Header("音效设置")]
    public AudioClip[] FootstepAudioClips;  // 行走的音效
    public AudioClip[] IdelAudioClips;      // 待机的音效
    public AudioClip[] HurtAudioClips;      // 受伤的音效
    public AudioClip[] AttackAudioClips;    // 攻击的音效
    
    [Header("掉落物设置")]
    [SerializeField] private GameObject[] dropItems;     // 可能掉落的物品预制体
    [SerializeField] private float dropChance = 0.7f;    // 掉落概率

    private Vector3 target;
    private Transform playerTransform;
    
    // 调试用变量
    [Header("调试")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool forceRun = false; // 调试用：强制一直追击玩家
    
    // 巡逻点生成的范围
    private float patrolRangeX = 20f;
    private float patrolRangeZ = 20f;

    // 头部受击额外伤害系数
    private float headShotMultiplier = 1.5f;

    [SerializeField] private GameObject attackTriggerObject; // 👈 攻击触发器的引用（AttackTrigger）

    // 状态切换时的逻辑
    public BossState BossState
    {
        get => bossState;
        set
        {
            // 防止重复设置相同状态
            if (bossState == value) return;
            
            // 死亡状态特殊处理
            if (bossState == BossState.Dead && value != BossState.Idle)
            {
                return;
            }
            
            if (showDebugInfo)
                Debug.Log($"Boss状态切换: {bossState} -> {value}");
            
            bossState = value;

            switch (bossState)
            {
                case BossState.Idle:
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    navMeshAgent.enabled = false;
                    // 确保攻击碰撞盒禁用
                    if (attackCollider != null)
                        attackCollider.enabled = false;
                    Invoke("GoWalk", Random.Range(1, 3));
                    break;
                    
                case BossState.Walk:
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", true);
                    animator.SetBool("Run", false);
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = walkSpeed;
                    // 确保攻击碰撞盒禁用
                    if (attackCollider != null)
                        attackCollider.enabled = false;
                    // 去一个随机目标点
                    target = GetRandomPoint();
                    navMeshAgent.SetDestination(target);
                    break;
                    
                case BossState.Run:
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", true);
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = runSpeed;
                    // 确保攻击碰撞盒禁用
                    if (attackCollider != null)
                        attackCollider.enabled = false;
                    // 立即更新目标位置
                    if (playerTransform != null)
                        navMeshAgent.SetDestination(playerTransform.position);
                    break;
                    
                case BossState.Attack:
                    // 停止之前可能存在的攻击协程
                    if (attackCoroutine != null)
                        StopCoroutine(attackCoroutine);

                    // 重置所有动画参数，确保干净的状态
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    
                    // 设置攻击触发器
                    animator.SetTrigger("Attack");
                    
                    // 保持导航代理启用但停止
                    navMeshAgent.enabled = true;
                    navMeshAgent.isStopped = true;
                    
                    // 重置攻击标志
                    isAttacked = false;
                    nextAttackTime = Time.time + attackCooldown;
                    
                    // 启动攻击控制协程
                    attackCoroutine = StartCoroutine(AttackSequence());
                    break;
                    
                case BossState.Hurt:
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    animator.SetTrigger("Hurt");
                    // 确保攻击碰撞盒禁用
                    if (attackCollider != null)
                        attackCollider.enabled = false;
                    break;
                    
                case BossState.Dead:
                    animator.ResetTrigger("Attack");
                    animator.SetBool("Walk", false);
                    animator.SetBool("Run", false);
                    animator.SetTrigger("Dead");
                    // 禁用所有碰撞盒
                    if (bodyCollider != null)
                        bodyCollider.enabled = false;
                    if (headCollider != null)
                        headCollider.enabled = false;
                    if (attackCollider != null)
                        attackCollider.enabled = false;
                    navMeshAgent.enabled = false;
                    Invoke("DestroyBoss", 5);
                    break;
                    
                default:
                    break;
            }
        }
    }

    // 攻击序列协程
    private IEnumerator AttackSequence()
    {
        float attackStartTime = Time.time;
        
        if (showDebugInfo)
            Debug.Log("开始攻击序列");
        
        // 等待攻击动画开始播放（确保触发器已应用）
        yield return new WaitForSeconds(0.1f);
        
        // 等待攻击动画结束
        bool attackComplete = false;
        float timeout = 3.0f; // 防止无限等待
        
        while (!attackComplete && Time.time - attackStartTime < timeout)
        {
            // 检查当前动画状态
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查是否是攻击动画并且接近完成
            bool isAttackAnim = stateInfo.IsName("Attack") || stateInfo.IsTag("Attack");
            float normalizedTime = stateInfo.normalizedTime;
            
            
            // 如果攻击动画播放超过90%，认为已完成
            if (isAttackAnim && normalizedTime >= 0.9f)
            {
                attackComplete = true;
                
                if (showDebugInfo)
                    Debug.Log("攻击动画完成");
            }
            
            yield return null;
        }
        
        // 确保攻击碰撞盒被禁用
        if (attackCollider != null)
            attackCollider.enabled = false;
        
        // 如果超时，强制完成
        if (!attackComplete && showDebugInfo)
            Debug.LogWarning("攻击动画超时未完成，强制继续");
        
        // 恢复导航
        navMeshAgent.isStopped = false;
        
        // 检查是否应该返回追击状态（如果没有死亡或处于受伤状态）
        if (bossState == BossState.Attack)
        {
            if (showDebugInfo)
                Debug.Log("攻击完成，返回追击状态");
                
            BossState = BossState.Run;
        }
        // 攻击完成后切换状态
        if (bossState == BossState.Attack)
        {
            BossState = BossState.Run;

            // 补上动画参数，防止卡住
            animator.SetTrigger("Attack"); // 清除Trigger
            animator.ResetTrigger("Attack");
            animator.SetBool("Run", true);  // <-- 确保动画状态机能走到 Run
        }
    }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        
        // 如果没有在Inspector中指定碰撞器，则尝试自动获取
        if (bodyCollider == null)
            bodyCollider = GetComponent<CapsuleCollider>();
            
        // 查找玩家
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 确保攻击碰撞盒最初是禁用的，并设置为触发器
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;  // 确保是触发器
            
            // 调试日志
            if (showDebugInfo)
                Debug.Log("Boss攻击碰撞盒初始化为触发器");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("未找到Boss攻击碰撞盒！请在Inspector中设置");
        }
        
        // 配置NavMeshAgent
        if (navMeshAgent != null)
        {
            navMeshAgent.stoppingDistance = attackRange * 0.8f;
            navMeshAgent.autoBraking = false;
            navMeshAgent.updateRotation = true;
        }
        
        hp = maxHp;
        BossState = BossState.Idle;
        if (attackTriggerObject != null)
            attackTriggerObject.SetActive(false); // 启动时关闭

    }

    void OnEnable()
    {
        // 确保在启用时重置所有触发器
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Dead");
        }
    }

    // 处理脏数据
    public void Init()
    {
        animator.SetTrigger("Init");
        animator.ResetTrigger("Attack");
        
        if (bodyCollider != null)
            bodyCollider.enabled = true;
        if (headCollider != null)
            headCollider.enabled = true;
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            attackCollider.isTrigger = true;
        }
        hp = maxHp;
        BossState = BossState.Idle;
    }

    void Update()
    {
        // 如果已死亡，不执行任何更新
        if (bossState == BossState.Dead) return;
        
        // 如果找不到玩家，尝试重新查找
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            return;
        }
        
        // 调试用：强制追击
        if (forceRun && bossState != BossState.Run && bossState != BossState.Attack && bossState != BossState.Dead && bossState != BossState.Hurt)
        {
            BossState = BossState.Run;
        }
        
        // 更新状态
        StateForUpdate();
        
        // 在追击状态下更新路径
        if (bossState == BossState.Run && navMeshAgent.enabled && playerTransform != null)
        {
            navMeshAgent.SetDestination(playerTransform.position);
        }
    }

    void StateForUpdate()
    {
        if (playerTransform == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        switch (bossState)
        {
            case BossState.Idle:
                // 检查玩家是否在附近
                if (distanceToPlayer < detectRange)
                {
                    BossState = BossState.Run;
                }
                break;
                
            case BossState.Walk:
                if (distanceToPlayer < detectRange)
                {
                    // 去追玩家
                    BossState = BossState.Run;
                    return;
                }
                if (Vector3.Distance(target, transform.position) <= 1)
                {
                    BossState = BossState.Idle;
                }
                break;
                
            case BossState.Run:
                // 持续朝向玩家
                if (playerTransform != null)
                {
                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
                    }
                }
                
                // 检查攻击距离
                if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
                {
                    BossState = BossState.Attack;
                }
                break;
                
            case BossState.Attack:
                // 攻击状态由AttackSequence协程处理
                break;
                
            case BossState.Hurt:
                // 受伤状态由MovePause协程处理
                break;
                
            default:
                break;
        }
    }

    void GoWalk()
    {
        if (bossState != BossState.Dead)
            BossState = BossState.Walk;
    }

    // 接收伤害的方法（可由玩家的子弹调用）
    public void Hurt(int value, bool isHeadshot = false)
    {
        // 如果是爆头，增加伤害
        int finalDamage = isHeadshot ? Mathf.RoundToInt(value * headShotMultiplier) : value;
        
        if (showDebugInfo)
            Debug.Log($"Boss受到{finalDamage}点伤害，爆头: {isHeadshot}");
        
        hp -= finalDamage;
        if (hp <= 0)
        {
            BossState = BossState.Dead;
        }
        else
        {
            // 击退
            StartCoroutine(MovePause());
        }
    }

    void DestroyBoss()
    {
        // 实现Boss死亡逻辑
        DropItems();
        
        // 更新任务进度 - 通知任务系统Boss已被击杀
        if (MissionSystem.Instance != null)
        {
            // 记录Boss标签以便任务追踪
            string bossTag = gameObject.tag;
            Debug.Log($"Boss with tag '{bossTag}' has been defeated. Updating mission progress.");
            
            // 使用Boss的标签更新任务进度
            MissionSystem.Instance.UpdateMissionProgress(bossTag);
        }
        else
        {
            Debug.LogWarning("MissionSystem not found when Boss was defeated!");
        }
        
        // 可以在这里添加击败Boss时的特效、音效或其他游戏逻辑
        
        Destroy(gameObject);
    }
    
    // 掉落物品
    void DropItems()
    {
        if (dropItems == null || dropItems.Length == 0) return;
        
        if (Random.value <= dropChance)
        {
            int randomIndex = Random.Range(0, dropItems.Length);
            if (dropItems[randomIndex] != null)
            {
                Instantiate(dropItems[randomIndex], transform.position, Quaternion.identity);
            }
        }
    }

    IEnumerator MovePause()
    {
        BossState = BossState.Hurt;
        navMeshAgent.enabled = false;
        yield return new WaitForSeconds(0.5f);
        if (BossState != BossState.Dead)
        {
            navMeshAgent.enabled = true;
            BossState = BossState.Run;
        }
    }
    
    // 生成随机巡逻点
    Vector3 GetRandomPoint()
    {
        Vector3 randomPoint = transform.position;
        randomPoint.x += Random.Range(-patrolRangeX, patrolRangeX);
        randomPoint.z += Random.Range(-patrolRangeZ, patrolRangeZ);
        
        // 确保随机点在NavMesh上
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 20f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        // 如果没有找到有效点，返回当前位置
        return transform.position;
    }

    // 处理触发器碰撞以进行攻击
    private void OnTriggerEnter(Collider other)
    {
        // 调试用：记录所有碰撞
        if (showDebugInfo)
            Debug.Log($"触发器碰撞: {other.gameObject.name}, Tag: {other.gameObject.tag}, isAttacked: {isAttacked}");
            
        if (!isAttacked && other.CompareTag("Player"))
        {
            isAttacked = true;
            
            if (showDebugInfo)
                Debug.Log($"Boss攻击命中玩家，造成{attackDamage}点伤害");
                
            // 尝试获取Player_Controller组件并调用Hurt方法
            Player_Controller playerController = other.GetComponent<Player_Controller>();
            if (playerController != null)
            {
                playerController.Hurt(attackDamage);
            }
            else
            {
                // 尝试使用单例实例
                if (Player_Controller.Instance != null)
                {
                    Player_Controller.Instance.Hurt(attackDamage);
                }
                else if (showDebugInfo)
                {
                    Debug.LogError("无法找到Player_Controller组件或其实例！");
                }
            }
        }
    }

    // 手动测试攻击触发（可用于调试）
    public void TestAttackTrigger()
    {
        if (showDebugInfo)
            Debug.Log("手动测试攻击触发");
            
        if (Player_Controller.Instance != null)
        {
            Player_Controller.Instance.Hurt(attackDamage);
            Debug.Log($"手动测试：对玩家造成{attackDamage}点伤害");
        }
    }
    public void EnableAttackTrigger()
    {
        if (attackTriggerObject != null)
            attackTriggerObject.SetActive(true);

        // 调用启用伤害
        attackTriggerObject.GetComponent<AttackTrigger>()?.EnableDamage();

        if (showDebugInfo)
            Debug.Log("攻击触发器启用");
    }

    public void DisableAttackTrigger()
    {
        if (attackTriggerObject != null)
            attackTriggerObject.SetActive(false);

        // 调用关闭伤害
        attackTriggerObject.GetComponent<AttackTrigger>()?.DisableDamage();

        if (showDebugInfo)
            Debug.Log("攻击触发器禁用");
    }
    #region 动画事件
    void IdelAudio()
    {
        if (IdelAudioClips == null || IdelAudioClips.Length == 0) return;
        
        if (Random.Range(0, 4) == 1)
        {
            audioSource.PlayOneShot(IdelAudioClips[Random.Range(0, IdelAudioClips.Length)]);
        }
    }
    
    void FootStep()
    {
        if (FootstepAudioClips == null || FootstepAudioClips.Length == 0) return;
        audioSource.PlayOneShot(FootstepAudioClips[Random.Range(0, FootstepAudioClips.Length)]);
    }
    
    private void HurtAudio()
    {
        if (HurtAudioClips == null || HurtAudioClips.Length == 0) return;
        audioSource.PlayOneShot(HurtAudioClips[Random.Range(0, HurtAudioClips.Length)]);
    }
    
    private void AttackAudio()
    {
        if (AttackAudioClips == null || AttackAudioClips.Length == 0) return;
        audioSource.PlayOneShot(AttackAudioClips[Random.Range(0, AttackAudioClips.Length)]);
    }
    
    // 攻击开始（动画事件调用）
    public void StartAttack()
    {
        // 启用攻击碰撞盒
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
            
            if (showDebugInfo)
                Debug.Log("动画事件：Boss攻击碰撞盒已启用");
        }
        isAttacked = false; // 重置攻击标志
    }
    
    // 攻击结束（动画事件调用）
    public void EndAttack()
    {
        // 禁用攻击碰撞盒
        if (attackCollider != null)
        {
            attackCollider.enabled = false;
            
            if (showDebugInfo)
                Debug.Log("动画事件：Boss攻击碰撞盒已禁用");
        }
    }
    
    // 在编辑器中可视化碰撞盒
    private void OnDrawGizmosSelected()
    {
        // 可视化检测范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        
        // 可视化攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 可视化头部碰撞盒
        if (headCollider != null)
        {
            Gizmos.color = Color.yellow;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                headCollider.transform.position,
                headCollider.transform.rotation,
                headCollider.transform.lossyScale
            );
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(headCollider.center, headCollider.size);
        }
        
        // 可视化攻击碰撞盒
        if (attackCollider != null)
        {
            Gizmos.color = Color.red;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(
                attackCollider.transform.position,
                attackCollider.transform.rotation,
                attackCollider.transform.lossyScale
            );
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(attackCollider.center, attackCollider.size);
        }
    }
    #endregion
}