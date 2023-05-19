using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : Character
{
    [Header("References")]
    protected Animator animator;
    public EnemyScriptableObject enemyStats;

    public Weapon[] enemyWeapon;
    public Transform playerTransform;
    public AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;


    [Header("Parameters")]
    public float detectionRange = 10f;
    public float rotateSpeed = 5f;
    public NavMeshAgent agent;
    public int attackAnim = 2;
    [SerializeField] private float currentHealth;
    private float attackTimer = 0f;
    private float attackedChainDamage = 0;
    private float attackedChainTimer = 0;

    protected float distanceToPlayer;

    [Header("Animation")]
    public AnimationCurve deadAnimCurve;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    protected virtual void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        currentHealth = enemyStats.maxHealth;
        agent.stoppingDistance = enemyStats.attackRange;
        agent.speed = enemyStats.moveSpeed;
        foreach (var weap in enemyWeapon)
        {
            weap.Damage = enemyStats.attackDamage;
            weap.CritRate = enemyStats.critRate;
        }

        agent.enabled = true;
        GameManager.Instance.GameEnd += () => playerTransform = null;
    }

    protected virtual void Update()
    {
        if (playerTransform == null || !agent.enabled)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        attackedChainTimer += Time.deltaTime;
        if (attackedChainTimer > 5f) attackedChainDamage = 0;
        // Ready to attack
        attackTimer -= Time.deltaTime;
        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange)
        {
            agent.isStopped = false;
            animator.SetBool("isMoving", true);
            if (animator.GetBool("canMove"))
            {
                agent.SetDestination(playerTransform.position);

                //if (distanceToPlayer < enemyStats.moveBackDistance)
                //{
                //    MoveBack();
                //}
            }
            if (agent.pathPending) return;
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                animator.SetBool("isMoving", false);
                agent.updateRotation = false;
                FaceTarget();
                if (attackTimer <= 0f)
                {
                    attackTimer = enemyStats.attackCooldown;
                    Attack();
                }

            }
            else
            {
                agent.updateRotation = true;
            }
        }
        else
        {
            agent.isStopped = true;
            animator.SetBool("isMoving", false);
        }
    }
    
    // If too close
    private void MoveBack()
    {
        if (NavMesh.SamplePosition((playerTransform.position - transform.position).normalized * -distanceToPlayer, out var hit, enemyStats.moveBackDistance, NavMesh.GetAreaFromName("Walkable")))
        {
            agent.updateRotation = true;
            animator.SetBool("isMoving", true);
            agent.SetDestination(hit.position);
            agent.isStopped = false;
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }

    private void Attack()
    {
        animator.SetTrigger("Attack" + (Random.Range(0, attackAnim) + 1).ToString());
        audioSource.clip = attackSound;
        audioSource.volume = 1f;
        audioSource.pitch = 1.0f;
        audioSource.loop = false;
        audioSource.Play();
    }

    public override void TakeDamage(Damage damage)
    {
        attackedChainDamage += damage.value;
        attackedChainTimer = 0f;
        if (attackedChainDamage >= enemyStats.cancelableDamage)
        {
            animator.SetTrigger("Hurt");
            attackedChainDamage = 0f;
        }
        currentHealth -= damage.value;

        audioSource.clip = hitSound;
        audioSource.volume = 1f;
        audioSource.pitch = 1.0f;
        audioSource.loop = false;
        audioSource.Play();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        agent.enabled = false;
        animator.SetBool("Dead", true);
        animator.SetBool("canMove", false);
        StartCoroutine(DeadAnim());
        StartCoroutine(DestroyAfter(2f));
    }

    private IEnumerator DeadAnim()
    {
        float timer = 0f;
        Rigidbody rb = GetComponent<Rigidbody>();
        while (timer < deadAnimCurve.keys[deadAnimCurve.length - 1].time)
        {
            rb.MovePosition(new Vector3(rb.position.x, rb.position.y + deadAnimCurve.Evaluate(timer) * agent.height, rb.position.z));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator DestroyAfter(float second)
    {
        yield return new WaitForSeconds(second);
        Destroy(gameObject);
        GameManager.Instance.EnemyDrop();
        StopAllCoroutines();
    }
}
