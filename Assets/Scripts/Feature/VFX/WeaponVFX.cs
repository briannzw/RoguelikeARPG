using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponVFX : MonoBehaviour
{
    [Header("References")]
    public Transform WeaponTransform;
    public Vector3 Size;
    private AudioSource audioSource;
    private AudioClip attackSound;

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

    public void AttackSound(){
        audioSource = GetComponent<AudioSource>();
        attackSound = Resources.Load<AudioClip>("SFX/swordHit");
        audioSource.clip = attackSound;
        audioSource.volume = 10f;
        audioSource.pitch = 1.0f;
        audioSource.loop = false;
        audioSource.Play();
    }
}
