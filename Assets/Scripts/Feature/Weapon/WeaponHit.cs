using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponHit : MonoBehaviour
{
    [Header("References")]
    public GameObject vfxPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject go = Instantiate(vfxPrefab, other.ClosestPointOnBounds(transform.position), Quaternion.identity);
            Destroy(go, vfxPrefab.GetComponent<VisualEffect>().GetFloat("Lifetime"));
        }
    }
}
