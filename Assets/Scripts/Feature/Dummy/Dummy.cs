using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dummy : MonoBehaviour
{
    [Header("References")]
    public TMP_Text BillboardLabel;
    public Material HitMaterial;
    private Material _material;

    [Header("Parameters")]
    public float ComboDuration = 5f;

    private float comboTimer = 0f;
    private int hitCount = 0;

    private void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
    }

    private void Update()
    {
        if (hitCount == 0) return;

        comboTimer += Time.deltaTime;
        if(comboTimer > ComboDuration)
        {
            hitCount = 0;
            comboTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("test");
        if (other.CompareTag("Weapon"))
        {
            GetComponent<MeshRenderer>().material = HitMaterial;
            comboTimer = 0;
            hitCount++;
            BillboardLabel.text = "Hit Count : " + hitCount.ToString();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            GetComponent<MeshRenderer>().material = _material;
        }
    }
}
