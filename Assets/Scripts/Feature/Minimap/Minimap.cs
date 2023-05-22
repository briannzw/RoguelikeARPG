using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public RectTransform marker; //player pointer image
    public RectTransform mapImage;//Map screenshot used in canvas
    public RectTransform mapMaskImage;
    public Transform playerReference;//player
    public Vector2 offset;//Adjust the value to match you map

    private Camera minimapCam;
    private Vector3[] mapBound = new Vector3[4];

    private Vector2 mapDimentions;
    [SerializeField] private Vector2 areaDimentions;

    private void Start()
    {
        mapDimentions = new Vector2(mapImage.sizeDelta.x, mapImage.sizeDelta.y);
    }

    public void Initialize(Camera cam)
    {
        minimapCam = cam;
        mapBound[0] = minimapCam.ScreenToWorldPoint(new Vector3(0, 0, minimapCam.nearClipPlane));
        mapBound[1] = minimapCam.ScreenToWorldPoint(new Vector3(minimapCam.pixelWidth, 0, minimapCam.nearClipPlane));
        mapBound[2] = minimapCam.ScreenToWorldPoint(new Vector3(minimapCam.pixelWidth, minimapCam.pixelHeight, minimapCam.nearClipPlane));
        areaDimentions.x = Mathf.Abs(mapBound[1].x - mapBound[0].x);
        areaDimentions.y = Mathf.Abs(mapBound[2].z - mapBound[1].z);
    }

    private void Update()
    {
        if (minimapCam == null) return;
        SetMarkerPosition();
    }

    private void SetMarkerPosition()
    {
        Vector3 distance = playerReference.position - mapBound[1];
        Vector2 coordinates = new Vector2(distance.x / areaDimentions.x, distance.z / areaDimentions.y);
        //mapImage.anchoredPosition = new Vector2(coordinates.x * mapDimentions.x, coordinates.y * mapDimentions.y) + offset;// - mapMaskImage.sizeDelta / 2;
        marker.anchoredPosition = new Vector2(coordinates.x * mapDimentions.x, coordinates.y * mapDimentions.y) + offset;
        marker.rotation = Quaternion.Euler(new Vector3(0, 0, -playerReference.eulerAngles.y - 90f));
        //Debug.Log(new Vector2(coordinates.x * mapDimentions.x, coordinates.y * mapDimentions.y)); Hasil posisi x dan y dari kanan bawah
        // Coordinate - center of mapImage = mapImage position
    }
}
