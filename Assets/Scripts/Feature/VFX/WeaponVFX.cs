using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponVFX : MonoBehaviour
{
    [Header("References")]
    public Transform WeaponTransform;
    public Vector3 Size;

    public void SpawnVFX(GameObject VFX)
    {
        GameObject go = Instantiate(VFX, WeaponTransform.position, WeaponTransform.rotation);
        go.transform.localScale = Size;
        Destroy(go, VFX.GetComponent<VisualEffect>().GetFloat("Lifetime"));
    }

    public void SpawnVFXGlobal(AnimationEvent myEvent)
    {
        GameObject VFX = (GameObject)myEvent.objectReferenceParameter;
        float offset = myEvent.floatParameter;
        GameObject go = Instantiate(VFX, transform.position + transform.forward * offset, Quaternion.identity);
        Destroy(go, VFX.GetComponent<VisualEffect>().GetFloat("Lifetime"));
    }
}
