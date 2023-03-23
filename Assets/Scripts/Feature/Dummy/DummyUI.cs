using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DummyUI : MonoBehaviour
{
    [Header("References")]
    public Dummy Dummy;
    public TMP_Text BillboardLabel;

    [Header("Parameters")]
    public float DPSUpdateInterval = 0.5f;

    private Dictionary<CombatEnum, float> lastValues;
    private float lastDamageDealt = 0;
    private float elapsedTime = 0f;
    private float timer = 0f;

    private void Start()
    {
        Dummy.OnCombatValueChanged += SetLabel;
    }

    private void Update()
    {
        if (lastDamageDealt == 0f) return;

        elapsedTime += Time.deltaTime;
        timer += Time.deltaTime;
        if(timer > DPSUpdateInterval)
        {
            timer = 0f;
            lastValues[CombatEnum.DPS] = lastDamageDealt / elapsedTime;
            SetLabel(lastValues);
        }
    }

    public void SetLabel(Dictionary<CombatEnum, float> values)
    {
        BillboardLabel.text = "";
        foreach (var item in values)
        {
            BillboardLabel.text += item.Key.ToString() + " : " + item.Value.ToString("F2") + "\n";
        }

        if (values[CombatEnum.DamageTaken] == 0) elapsedTime = 0f;
        lastValues = values;
        lastDamageDealt = values[CombatEnum.DamageTaken];
    }

}
