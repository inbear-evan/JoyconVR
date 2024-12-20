using UnityEngine;

public class PointerImage : MonoBehaviour
{
    public Transform SphereTransfrom;
    private RectTransform rectTransform;
    
    private float scale = 50.0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        float x = SphereTransfrom.position.x * scale;
        float y = SphereTransfrom.position.z * -scale;
        rectTransform.anchoredPosition = new Vector3(x, y, 0);
    }
}
