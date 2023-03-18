using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponHit : MonoBehaviour
{
    [Header("References")]
    public Weapon Weapon;
    public GameObject vfxPrefab;
    [TagField]
    public string affectedTag;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(affectedTag))
        {
            Vector3 pos = other.ClosestPointOnBounds(transform.position);
            float dmg = Weapon.GetDamage();
            other.GetComponent<Character>().TakeDamage(dmg);
            Weapon.DamagePopup(pos, dmg, Weapon.Critted);

            //VFX
            if (vfxPrefab == null) return;
            GameObject go = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(go, vfxPrefab.GetComponent<VisualEffect>().GetFloat("Lifetime"));
        }
    }
}