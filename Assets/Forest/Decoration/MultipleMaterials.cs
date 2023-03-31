using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipleMaterials : MonoBehaviour
{
    public Material[] materials;

    private void Start()
    {
        GetComponent<MeshRenderer>().sharedMaterials = materials;
        Destroy(this);
    }
}
