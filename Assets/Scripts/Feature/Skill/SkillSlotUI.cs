using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Image childImage;

    public void Initialize(Sprite sprite)
    {
        image.sprite = sprite;
        childImage.sprite = sprite;
    }

    public void SetValue(float value)
    {
        childImage.fillAmount = value;
    }
}