using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ClickSelectObject : MonoBehaviour
{

    public GameObject selectUpBtn;
    public GameObject selectDownBtn;

    public GrapPosition upPosition;
    public GrapPosition downPosition;

    public GameObject selectObj;
    //fasle = up, true = down;
    bool selectState = false;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(upPosition.isInsideCollider == false && downPosition.isInsideCollider == false)
        {
            selectObj.transform.position = new Vector3(-1.0812f, 0.797f, 1.609f);
            //selectObj.transform.rotation = new Quaternion().;
            selectObj.transform.localScale = new Vector3(3.95f, 3.95f, 3.95f);
        }
        if (upPosition.isInsideCollider && selectState)
        {
            selectObj.transform.position = new Vector3(-1.0812f, 0.844f, 1.609f);
            //selectObj.transform.rotation = Quaternion.identity;
            selectObj.transform.localScale = new Vector3(3.95f, 3.95f, 3.95f);
        }
        if(downPosition.isInsideCollider && selectState == false)
        {
            selectObj.transform.position = new Vector3(-1.0812f, 0.844f, 1.791f);
            //selectObj.transform.rotation = Quaternion.identity;
            selectObj.transform.localScale = new Vector3(3.95f, 3.95f, 3.95f);
        }
    }

    public void OnClickUp()
    {
        selectState = true;
    }
    public void OnClickDown()
    {
        selectState = false;
    }
}
