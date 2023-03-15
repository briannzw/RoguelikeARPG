using TMPro;
using UnityEngine;

public class DamagePopUpAnimation : MonoBehaviour
{
    [Header("References")]
    public TMP_Text text;

    [Header("Parameters")]
    public AnimationCurve opacityCurve;
    public AnimationCurve scaleCurve;
    public AnimationCurve heightCurve;

    [Header("Critical")]
    public AnimationCurve critScaleCurve;

    private float timer = 0f;
    private Vector3 origin;

    private void Awake()
    {
        origin = transform.position;
    }

    private void Update()
    {
        text.color = new Color(1f, 1f, 1f, opacityCurve.Evaluate(timer));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(timer);
        transform.position = origin + new Vector3(0f, 1f + heightCurve.Evaluate(timer), 0f);
        timer += Time.deltaTime;
    }
}
