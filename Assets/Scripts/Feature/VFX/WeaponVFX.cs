using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponVFX : MonoBehaviour
{
    [Header("References")]
    public Transform WeaponTransform;
    public Vector3 Size;

    private Weapon weapon;

    private void Start()
    {
        weapon = WeaponTransform.GetComponentInChildren<Weapon>();
    }

    public void SetSkillToWeap(Skill skill)
    {
        weapon.LocalHit.Skill = weapon.SkillMap[skill];
    }

    public void ResetSkill()
    {
        weapon.LocalHit.Skill = null;
    }

    public void SpawnVFX(GameObject VFX)
    {
        GameObject go = Instantiate(VFX, WeaponTransform.transform.position, WeaponTransform.transform.rotation);
        go.transform.localScale = Size;
        Destroy(go, VFX.GetComponent<VisualEffect>().GetFloat("Lifetime"));
    }

    public void SpawnVFXGlobal(AnimationEvent myEvent)
    {
        GameObject VFX = (GameObject)myEvent.objectReferenceParameter;
        float offset = myEvent.floatParameter;
        GameObject go = Instantiate(VFX, transform.position + transform.forward * offset, Quaternion.identity);
        go.transform.forward = transform.forward;
        WeaponHit hit = go.GetComponent<WeaponHit>();
        if (hit != null)
        {
            hit.Weapon = weapon;
            hit.Skill = weapon.SkillMap[(hit.Skill.isBaseSkill) ? hit.Skill : hit.Skill.baseSkill];
        }
        Destroy(go, VFX.GetComponent<VisualEffect>().GetFloat("Lifetime"));
    }

    public void AttackSound()
    {
        weapon.PlaySound();
    }
}
