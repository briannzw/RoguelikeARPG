using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponVFX : MonoBehaviour
{
    [Header("References")]
    public Transform WeaponTransform;

    public void SpawnVFX(GameObject VFX)
    {
        GameObject go = Instantiate(VFX, WeaponTransform.position, WeaponTransform.rotation);
        Destroy(go, VFX.GetComponentInChildren<VisualEffect>().GetFloat("Lifetime"));
    }
}
