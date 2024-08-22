using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FrictionRecog : MonoBehaviour
{

    //Aluminum,wood,Plastic
    public GameObject frictionObject;
    public GameObject frictionObject2;
    public GameObject frictionObject3;
    public TMP_Text frintctionTxt;
    public TMP_Text frintctionTxt2;
    public TMP_Text frintctionTxt3;
    public TMP_Text frintctionPlaneAngle;

    
    private float planeAngle;

    private void OnTriggerEnter(Collider other)
    {
       
        
        //if (other.CompareTag("Player")) 
        {
            if(frictionObject.name == other.name)
            {
                //Debug.Log("Friction 1");
                frintctionTxt.text = frintctionPlaneAngle.text;
            }
            if (frictionObject2.name == other.name)
            {
                //Debug.Log("Friction 2");
                frintctionTxt2.text = frintctionPlaneAngle.text;
            }
            if (frictionObject3.name == other.name)
            {
                //Debug.Log("Friction 3");
                frintctionTxt3.text = frintctionPlaneAngle.text;
            }

        }
    }

    public void OnClickObjectReset()
    {
        //Aluminum,wood,Plastic
        frictionObject.transform.position = new Vector3(-0.782000065f, 0.888400018f, 2.05850005f); 
        frictionObject.transform.rotation = Quaternion.identity;


        frictionObject2.transform.position = new Vector3(-0.782000065f, 0.888400018f, 1.875f);
        frictionObject2.transform.rotation = Quaternion.identity;


        frictionObject3.transform.position = new Vector3(-0.782000065f, 0.888400018f, 1.69500005f);
        frictionObject3.transform.rotation = Quaternion.identity;

        frintctionTxt.text = "0";
        frintctionTxt2.text = "0";
        frintctionTxt3.text = "0";
        frintctionPlaneAngle.text = "0";
    }
}
