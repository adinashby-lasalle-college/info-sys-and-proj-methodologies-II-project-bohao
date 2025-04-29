using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum BossState
{
    Idle,
    Run,
    Attack,
    Dead
}

public class BossController : MonoBehaviour
{
    [SerializeField] private BossState bossState;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private Transform playerTransform;

    [Header("属性设置")]
    [SerializeField] private int maxHp = 200;
    private int hp;
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float rageRunSpeed = 10.0f;
    private bool hasRaged = false;

    [SerializeField] private float detectRange = 12f;
    [SerializeField] private float attackRange = 5.0f;
    [SerializeField] private float attackCooldown = 1.5f;
    private float nextAttackTime = 0f;

    [Header("攻击设置")]
    private bool canAttack = false;
    private bool hasDealtDamage = false;
    private bool showDebugInfo = true;

    [Header("头部碰撞")]
    public BoxCollider headCollider;

    private bool isKnockback = false; // 是否正在击退

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        hp = maxHp;
    }

    void Update()
    {
        if (bossState == BossState.Dead) return;
        if (playerTransform == null) return;

        if (!isKnockback && bossState == BossState.Run && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(playerTransform.position);
        }

        StateForUpdate();
    }

    void StateForUpdate()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (bossState == BossState.Idle && distance < detectRange)
            BossState = BossState.Run;

        if (bossState == BossState.Run)
        {
            if (distance <= attackRange && Time.time >= nextAttackTime)
                BossState = BossState.Attack;
        }
    }

    public BossState BossState
    {
        get => bossState;
        set
        {
            if (bossState == value) return;
            bossState = value;

            switch (bossState)
            {
                case BossState.Idle:
                    animator.SetBool("Run", false);
                    navMeshAgent.enabled = false;
                    break;
                case BossState.Run:
                    animator.SetBool("Run", true);
                    navMeshAgent.enabled = true;
                    navMeshAgent.speed = hasRaged ? rageRunSpeed : runSpeed;
                    navMeshAgent.isStopped = false;
                    break;
                case BossState.Attack:
                    animator.SetTrigger("Attack");
                    navMeshAgent.isStopped = true;
                    nextAttackTime = Time.time + attackCooldown;
                    hasDealtDamage = false;
                    break;
                case BossState.Dead:
                    animator.SetTrigger("Dead");
                    navMeshAgent.enabled = false;
                    break;
            }
        }
    }

    public void EnableDamage()
    {
        if (showDebugInfo) Debug.Log("[Boss] EnableDamage()");
        canAttack = true;
        hasDealtDamage = false;
    }

    public void DisableDamage()
    {
        if (showDebugInfo) Debug.Log("[Boss] DisableDamage()");
        canAttack = false;
    }

    public void AttackOver()
    {
        if (showDebugInfo) Debug.Log("[Boss] AttackOver()");
        if (bossState == BossState.Attack)
        {
            BossState = BossState.Run;
        }
    }

    private void TryDealDamage()
    {
        if (!canAttack || hasDealtDamage) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= attackRange + 1f)
        {
            var playerStatus = playerTransform.GetComponent<Player_Controller>();
            if (playerStatus != null)
            {
                playerStatus.Hurt(20);
                hasDealtDamage = true;

                if (showDebugInfo) Debug.Log("[Boss] 成功对玩家造成20点伤害！");
            }
        }
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        if (bossState == BossState.Dead) return;

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            float realForce = Mathf.Clamp(force, 1f, 5f);
            isKnockback = true;
            navMeshAgent.isStopped = true;

            Vector3 knockbackTarget = transform.position + new Vector3(direction.x, 0, direction.z) * force;

            // 防止飞到空中，锁定在地形上
            RaycastHit hit;
            if (Physics.Raycast(knockbackTarget + Vector3.up * 5f, Vector3.down, out hit, 10f, LayerMask.GetMask("Default")))
            {
                knockbackTarget.y = hit.point.y;
            }
            else
            {
                // 如果没检测到地形，保持原高度
                knockbackTarget.y = transform.position.y;
            }

            StartCoroutine(DoKnockback(knockbackTarget));
        }
    }



    private IEnumerator DoKnockback(Vector3 targetPosition)
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        isKnockback = false;

        if (bossState != BossState.Dead)
        {
            navMeshAgent.isStopped = false;
        }
    }

    public void Hurt(int damage, bool isHeadshot = false)
    {
        int finalDamage = damage;
        if (isHeadshot)
        {
            finalDamage = Mathf.RoundToInt(damage * 1.5f);
        }

        hp -= finalDamage;

        if (hp <= 0)
        {
            Die();
        }
        else
        {
            if (showDebugInfo)
                Debug.Log($"Boss受到{finalDamage}点伤害。剩余血量: {hp}");

            if (!hasRaged && hp <= maxHp / 2)
            {
                Rage();
            }
        }
    }

    private void Rage()
    {
        hasRaged = true;
        if (navMeshAgent.enabled)
        {
            navMeshAgent.speed = rageRunSpeed;
        }

        if (showDebugInfo)
            Debug.Log("[Boss] 进入狂暴状态！移动速度暴增！");
    }

    private void Die()
    {
        BossState = BossState.Dead;
        animator.SetTrigger("Dead");
        navMeshAgent.enabled = false;
    }

    void FixedUpdate()
    {
        if (canAttack && !hasDealtDamage)
        {
            TryDealDamage();
        }
    }
}
