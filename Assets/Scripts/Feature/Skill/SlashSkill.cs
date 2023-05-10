using UnityEngine;
using static UnityEditor.PlayerSettings;

public class SlashSkill : WeaponHit
{
    public AnimationCurve velocityCurve;
    //[SerializeField] private Rigidbody rb;

    [SerializeField] private Vector3 offset;
    private float timer = 0f;

    private void Start()
    {
        transform.position += offset;
    }

    private void Update()
    {
        transform.Translate(transform.forward * velocityCurve.Evaluate(timer) * Time.deltaTime, Space.World);
        timer += Time.deltaTime;
    }
}
