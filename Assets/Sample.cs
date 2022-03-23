using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshCutter.Slicer(gameObject, new Vector3(1, 0, 0), new Vector3(0,0.25f,0));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
