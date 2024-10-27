using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GrapPosition : MonoBehaviour
{
    private GameObject selectedObject;
    public bool isInsideCollider = false; // 콜라이더 안에 있는지 여부
    public LayerMask grabbableLayer;
    public TMP_Text kgLabel;

    bool isEnable= false;
    private void Start()
    {
        kgLabel.text = "";
        GetComponent<MeshRenderer>().enabled = false;
    }

    // 콜라이더 영역에 들어왔을 때 호출
    private void OnTriggerStay(Collider other)
    {
        // Layer가 일치하면 동작
        if (((1 << other.gameObject.layer) & grabbableLayer) != 0)
        {
            if (isEnable == false)
            {
                GetComponent<MeshRenderer>().enabled = true;
                selectedObject = other.gameObject;
                isInsideCollider = true;
                kgLabel.text = selectedObject.GetComponent<Rigidbody>().mass.ToString("F2")+ " Kg";
                //print("IN" + selectedObject.name);
            }
            else
            {
                GetComponent<MeshRenderer>().enabled = false;
                kgLabel.text = selectedObject.GetComponent<Rigidbody>().mass.ToString("F2") + " Kg";
                selectedObject = other.gameObject;
                isInsideCollider = true;
            }
            
        }
    }

    // 콜라이더 영역에서 나갔을 때 호출
    private void OnTriggerExit(Collider other)
    {
        // Layer가 일치하면 동작
        if (((1 << other.gameObject.layer) & grabbableLayer) != 0)
        {
            GetComponent<MeshRenderer>().enabled = false;
            isInsideCollider = false;
            if (other.GetComponent<Rigidbody>().isKinematic)
                isEnable = false;
            kgLabel.text = "";
        }
    }

    // 마우스 버튼이 올라갔을 때 호출
    public void DisableUp(GameObject _selectedObject)
    {
        //if (isInsideCollider && selectedObject != null)
        if (isInsideCollider)
        {

            if (((1 << _selectedObject.gameObject.layer) & grabbableLayer) != 0)
            {
                _selectedObject.transform.position = transform.position;
                _selectedObject.GetComponent<Rigidbody>().isKinematic = false; // 물리효과 다시 적용
                isEnable = true;

            }
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void OnClickSlim()
    {
        Vector3 scaled = selectedObject.transform.localScale;
        Vector3 newScale = new Vector3(scaled.x, scaled.y, Mathf.Clamp(selectedObject.transform.localScale.z / 1.5f, 0.04828631f, 0.130373f));
        selectedObject.transform.localScale = newScale;

    }
    public void OnClickWide()
    {
        Vector3 scaled = selectedObject.transform.localScale;
        Vector3 newScale = new Vector3(scaled.x, scaled.y, Mathf.Clamp(selectedObject.transform.localScale.z * 1.2f, 0.04828631f, 0.130373f));
        selectedObject.transform.localScale = newScale;
    }
    public void OnClickWeightUP()
    {
        float originMass = selectedObject.GetComponent<Rigidbody>().mass + 0.05f;
        selectedObject.GetComponent<Rigidbody>().mass = originMass;
        kgLabel.text = originMass.ToString("F2") + " Kg";
    }
    public void OnClickWeightDown()
    {
        float originMass = selectedObject.GetComponent<Rigidbody>().mass - 0.05f;
        selectedObject.GetComponent<Rigidbody>().mass = originMass;
        kgLabel.text = originMass.ToString("F2") + " Kg";
    }
    public void Reset()
    {
        selectedObject.GetComponent<Rigidbody>().mass = 1;
        selectedObject = null;
        kgLabel.text = "1.00 Kg";
        isEnable = false;
    }
}
