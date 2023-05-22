using UnityEngine;

public class MinimapShot : MonoBehaviour
{
    [SerializeField] private Camera screenshotCam;
    [SerializeField] private Minimap minimap;

    private void Start()
    {
        minimap = FindObjectOfType<Minimap>();
        minimap.Initialize(screenshotCam);
        DungeonGenerator.Instance.OnDungeonComplete += () => { screenshotCam.enabled = false; gameObject.SetActive(false); };
    }
}
