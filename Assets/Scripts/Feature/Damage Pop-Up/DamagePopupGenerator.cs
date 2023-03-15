using TMPro;
using UnityEngine;

public class DamagePopupGenerator : MonoBehaviour
{
    [Header("References")]
    public GameObject popUpPrefab;

    [Header("Parameter")]
    public Color NormalColor = Color.white;
    public Color CritColor = Color.yellow;

    public void CreatePopUp(Vector3 position, float value, bool isCrit)
    {
        GameObject obj = Instantiate(popUpPrefab, position, Quaternion.identity);
        var tmpText = obj.GetComponentInChildren<TMP_Text>();
        tmpText.text = value.ToString();
        tmpText.faceColor = isCrit ? CritColor : NormalColor;
        if (isCrit)
        {
            DamagePopUpAnimation anim = obj.GetComponent<DamagePopUpAnimation>();
            anim.scaleCurve = anim.critScaleCurve;
        }

        Destroy(obj, 1f);
    }
}
