using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlane : MonoBehaviour
{
    public GameObject FrictionPlane1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = FrictionPlane1.transform.rotation;
    }
}
