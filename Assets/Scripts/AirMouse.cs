using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND"; // �ٴ� �±�
    [SerializeField] LayerMask groundLayer;  // �ٴ� ���̾� ����ũ
    [SerializeField] LayerMask interactableLayer; // Ư�� ��ü ���̾� ����ũ
    [SerializeField] float rotateSpeed = 5.0f;
    [SerializeField] float movementThreshold = 2.0f;
    [SerializeField] float holdTimeThreshold = 1.0f; // ������ �־�� �ϴ� �ð�
    [SerializeField] float lineDistance = 10;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject groundCirclePrefab; // �� �̹����� ǥ���� ������
    [SerializeField] float defaultLineWidth = 0.1f; // �⺻ ���� �β�
    [SerializeField] float interactLineWidth = 0.05f; // ��ȣ�ۿ� �� ���� �β�

    private Vector3 lastMousePosition;
    private bool isMousePressed;
    private bool isCurveActive;
    private float mousePressTime;
    private Vector3 groundPoint;
    private bool validGroundPoint;
    private bool usingTag;
    private GameObject groundCircleInstance;
    Material groundCircleInstanceMaterial;
    RaycastHit hit;
    GameObject clickObject;

    void Start()
    {
        Cursor.visible = showMouse;
        lastMousePosition = Input.mousePosition;

        // LineRenderer ����
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = defaultLineWidth;
            lineRenderer.endWidth = defaultLineWidth;
            ChangeLineColor(Color.white); // �⺻ ������ �Ͼ������ ����
        }

        // Ground Circle �ʱ�ȭ
        if (groundCirclePrefab != null)
        {
            groundCircleInstance = Instantiate(groundCirclePrefab);
            groundCircleInstance.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ

            groundCircleInstanceMaterial = groundCircleInstance.GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        if (mouseDelta.magnitude > movementThreshold)
        {
            // ���콺 �̵��� ���� ȸ�� ���� ���
            float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
            float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;

            // ���� ���� ȸ�� ���� ��������
            Vector3 localEulerAngles = transform.localRotation.eulerAngles;

            // ���� X�� ȸ�� ���� (-90������ 90�� ����)
            float newLocalRotationX = localEulerAngles.x - rotateX;
            if (newLocalRotationX > 180) newLocalRotationX -= 360;
            newLocalRotationX = Mathf.Clamp(newLocalRotationX, -90, 90);

            // ���� Y�� ȸ�� ���� (-90������ 90�� ����)
            float newLocalRotationY = localEulerAngles.y + rotateY;
            if (newLocalRotationY > 180) newLocalRotationY -= 360;
            newLocalRotationY = Mathf.Clamp(newLocalRotationY, -90, 90);

            // ���ο� ���� ȸ�� ������ ����
            transform.localRotation = Quaternion.Euler(newLocalRotationX, newLocalRotationY, 0);

            // ���� ���콺 ��ġ�� ������ ��ġ�� ������Ʈ
            lastMousePosition = currentMousePosition;
        }

        // ���콺 Ŭ�� ���� ������Ʈ
        if (Input.GetMouseButtonDown(0)) // ���� ���콺 ��ư Ŭ��
        {
            isMousePressed = true;
            //mousePressTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0)) // ���� ���콺 ��ư���� ���� ��
        {
            clickObject = hit.collider.gameObject;
            isMousePressed = false;
            if (isCurveActive && validGroundPoint && (((1 << clickObject.layer) & groundLayer) != 0))
            {
                TeleportToGroundPoint();
            }
            isCurveActive = false;
            HideGroundCircle();
           
        }

        // Raycast�� ���콺 �����ӿ� ���� ������ ������ ������Ʈ
        Ray ray = new Ray(transform.position, transform.forward);
      

        if (Physics.Raycast(ray, out hit, lineDistance, groundLayer | interactableLayer))
        {
            groundPoint = hit.point;
            validGroundPoint = true;

            //clickObject = hit.collider.gameObject;
            if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
            {
                //validGroundPoint = true;
                isCurveActive = true;
                ChangeLineColor(Color.red);
                ShowGroundCircle(groundPoint); // �ڷ���Ʈ ����� Ȱ��ȭ�� ��쿡�� ���� ǥ��
                DrawBezierCurve(ray.origin, groundPoint); // ������ � �׸���
            }
            
            else if (((1 << hit.collider.gameObject.layer) & interactableLayer) != 0)
            {
                //Debug.Log(hit.collider.gameObject.name);
                isCurveActive = false;
                ChangeLineColor(Color.blue); // Ư�� ��ü�� ����� �� �Ķ������� ����
                SetLineWidth(interactLineWidth); // Ư�� ��ü�� ����� �� ������ ��� ����
            }
            else
            {
                isCurveActive = false;
                ChangeLineColor(Color.white); // �׷��� ������ �⺻ �������� ����
                SetLineWidth(defaultLineWidth); // �⺻ �β��� ����
            }

            if(!isCurveActive)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, hit.point); // ����ĳ��Ʈ�� ���� �������� ���� �׸���
                
            }
        }
        else
        {
            clickObject = null;
            validGroundPoint = false;
            HideGroundCircle(); // ��ȿ���� ���� ��� ���� ����

            ChangeLineColor(Color.white); // �⺻ �������� ����
            SetLineWidth(defaultLineWidth); // �⺻ �β��� ����
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * lineDistance); // �⺻ �Ÿ���ŭ ���� �׸���
        }
    }
    


    void ChangeLineColor(Color color)
    {
        if (lineRenderer != null)
        {
            lineRenderer.material.color = color;
        }

        if (groundCircleInstanceMaterial != null)
        {
            groundCircleInstanceMaterial.color = color;
        }
    }

    void SetLineWidth(float width)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }

    void DrawBezierCurve(Vector3 startPoint, Vector3 endPoint)
    {
        if (lineRenderer != null)
        {
            Vector3 controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 2.0f; // Control point for the Bezier curve

            int segmentCount = 20;
            lineRenderer.positionCount = segmentCount + 1;
            for (int i = 0; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 bezierPoint = CalculateBezierPoint(t, startPoint, controlPoint, endPoint);
                lineRenderer.SetPosition(i, bezierPoint);
            }
        }
    }

    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0; // u^2 * P0
        p += 2 * u * t * p1; // 2 * u * t * P1
        p += tt * p2; // t^2 * P2
        return p;
    }

    void TeleportToGroundPoint()
    {
        // �ڷ���Ʈ ��� ����
        if (validGroundPoint)
        {
            transform.parent.parent.position = new Vector3(groundPoint.x, transform.parent.parent.position.y, groundPoint.z);
        }
    }

    void ShowGroundCircle(Vector3 position)
    {
        if (groundCircleInstance != null)
        {
            groundCircleInstance.transform.position = position;
            groundCircleInstance.SetActive(true);
        }
    }

    void HideGroundCircle()
    {
        if (groundCircleInstance != null)
        {
            groundCircleInstance.SetActive(false);
        }
    }

    // �΋H�� ���ӿ�����Ʈ ������
    public GameObject GetCollisionObject()
    {
        if (clickObject != null)
            return clickObject;
        else return null;
    }
}
