using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Enemy : Character
{
    [Header("References")]
    private Animator animator;
    public EnemyScriptableObject enemyStats;
    public Material HitMaterial;
    private Material _material;

     [Header("Parameters")]
    public float detectionRange = 10f;
    [SerializeField]
    private float currentHealth;
    private Transform player;
    private Rigidbody rb;
    public float ComboDuration = 5f;
    private float comboTimer = 0f;

    public Action<Dictionary<CombatEnum, float>> OnCombatValueChanged;
    private Dictionary<CombatEnum, float> CombatValues = new Dictionary<CombatEnum, float>();


    private void Awake(){
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        currentHealth = enemyStats.maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        _material = GetComponent<MeshRenderer>().material;

        foreach (CombatEnum item in Enum.GetValues(typeof(CombatEnum)))
        {
            CombatValues.Add(item, 0);
        }

        UpdateUI();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange && distanceToPlayer > enemyStats.attackRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            rb.MovePosition(transform.position + direction * enemyStats.moveSpeed * Time.deltaTime);
            rb.transform.rotation = rotation;
            animator.SetBool("Move", true);
            animator.SetBool("Attack", false);
        }
        else if(distanceToPlayer <= enemyStats.attackRange){
            OnAttack();
            animator.SetBool("Attack", true);
            animator.SetBool("Move", false);
        }
        else{
            animator.SetBool("Attack", false);
            animator.SetBool("Move", false);    
        }


        if (CombatValues[CombatEnum.HitCount] == 0) return;

        comboTimer += Time.deltaTime;
        if (comboTimer > ComboDuration)
        {
            CombatValues[CombatEnum.HitCount] = 0;
            CombatValues[CombatEnum.DamageDealt] = 0;
            comboTimer = 0f;
            UpdateUI();
        }
    }

    private void OnAttack()
    {
        if(animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Blend Tree"))
        animator.SetTrigger("Attack");
    }

    public override void TakeDamage(float damage)
    {

        comboTimer = 0;
        CombatValues[CombatEnum.HitCount]++;
        CombatValues[CombatEnum.DamageDealt] += damage;
        UpdateUI();

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

     private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            GetComponent<MeshRenderer>().material = HitMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            GetComponent<MeshRenderer>().material = _material;
        }
    }

    private void UpdateUI()
    {
        OnCombatValueChanged?.Invoke(CombatValues);
    }
    

    private void Die()
    {
        // add death behavior here
        Destroy(gameObject);
    }
}
