using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Dummy : Enemy
{
    [Header("References")]
    public Material HitMaterial;
    private Material _material;

    [Header("Parameters")]
    public float ComboDuration = 5f;

    private float comboTimer = 0f;

    public Action<Dictionary<CombatEnum, float>> OnCombatValueChanged;
    private Dictionary<CombatEnum, float> CombatValues = new Dictionary<CombatEnum, float>();

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;

        foreach (CombatEnum item in Enum.GetValues(typeof(CombatEnum)))
        {
            CombatValues.Add(item, 0);
        }

        UpdateUI();
    }

    private void Update()
    {
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

    public override void TakeDamage(float damage)
    {
        comboTimer = 0;
        CombatValues[CombatEnum.HitCount]++;
        CombatValues[CombatEnum.DamageDealt] += damage;
        UpdateUI();
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
}
