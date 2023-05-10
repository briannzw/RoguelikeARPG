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

    protected void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(affectedTag))
        {
            Vector3 pos = other.ClosestPointOnBounds(transform.position);
            Damage dmg = Weapon.GetDamage();
            other.GetComponent<Character>().TakeDamage(dmg);
            Weapon.DamagePopup(pos, dmg);

            //VFX
            if (vfxPrefab == null) return;
            GameObject go = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(go, vfxPrefab.GetComponent<VisualEffect>().GetFloat("Lifetime"));
        }
    }
}
