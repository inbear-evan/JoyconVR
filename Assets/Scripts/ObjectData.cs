using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ObjectData : MonoBehaviour
{
    public AirMouse airmouse;
    public Text txt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (airmouse.GetCollisionObject() != null)
            txt.text = airmouse.GetCollisionObject().name;
        else
            txt.text = "null";
    }
}
