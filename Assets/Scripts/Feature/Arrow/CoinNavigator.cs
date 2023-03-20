using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinNavigator : MonoBehaviour
{
    public Vector3 rotationOffset;
    private Transform followTransform;
    private Image image;
    private Camera cam;

    private Vector3 targetPosLocal;
    private float targetAngle;

    public Transform CurrentFollow => followTransform;

    private void Start()
    {
        image = GetComponent<Image>();
        cam = Camera.main;
        transform.Rotate(rotationOffset * Mathf.Deg2Rad);
        Follow(null);
    }

    private void Update()
    {
        if (followTransform == null) return;
        targetPosLocal = cam.transform.InverseTransformPoint(followTransform.position);
        targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg - 90;
        transform.eulerAngles = new Vector3(0f, 0f, targetAngle);
    }

    public void Follow(Transform target)
    {
        followTransform = target;
        image.enabled = target != null;
    }
}
