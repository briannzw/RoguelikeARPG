using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponHit : MonoBehaviour
{
    [Header("References")]
    public Weapon Weapon;
    public GameObject vfxPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //VFX
            Vector3 pos = other.ClosestPointOnBounds(transform.position);
            GameObject go = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(go, vfxPrefab.GetComponent<VisualEffect>().GetFloat("Lifetime"));

            float dmg = Weapon.GetDamage();
            other.GetComponent<Enemy>().TakeDamage(dmg);
            Weapon.DamagePopup(pos, dmg, Weapon.Critted);
        }
    }
}
