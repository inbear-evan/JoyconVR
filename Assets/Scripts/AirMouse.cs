using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND"; // �ٴ� �±�
    [SerializeField] LayerMask groundLayer;  // �ٴ� ���̾� ����ũ
    [SerializeField] LayerMask interactableLayer; // Ư�� ��ü ���̾� ����ũ
    [SerializeField] LayerMask grabbaleLayer; // Ư�� ��ü ���̾� ����ũ
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
    private bool isInteracting;
    private GameObject selectedObject;
    private Quaternion initialRotation;


    private bool isGrabbing = false; // �׷� ���� ����
    public float grabDistance = 3.0f;  // ��� �ִ� ��ü���� �Ÿ�
    private Vector3 grabOffset;
    private Vector3 grabPoint; // Ray �� ����
    public GrapPosition grapPosition;
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
        if (selectedObject != null)
        {
            initialRotation = selectedObject.transform.localRotation;
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
            print("Mouse Down Event Triggered");
        }

        if (Input.GetMouseButtonUp(0)) // ���� ���콺 ��ư���� ���� ��
        {
      
            if (isGrabbing)
            {
                if (GameObject.Find("GrabPosition") != null)
                {
                    grapPosition.DisableUp(selectedObject);
                }
                ReleaseObject();
                print("ReleaseObject Called");
            }

            // Reset selectedObject
            clickObject = hit.collider != null ? hit.collider.gameObject : null;
            if (clickObject != null)
            {
                print("Hit Object: " + clickObject.name);
            }
            else
            {
                print("No object hit.");
            }

            selectedObject = null;
            isMousePressed = false;

            if (isCurveActive && validGroundPoint && (((1 << clickObject.layer) & groundLayer) != 0))
            {
                TeleportToGroundPoint();
            }

            if (clickObject != null && clickObject.layer == LayerMask.NameToLayer("INTERACTUI"))
            {
                Button button = clickObject.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    Debug.Log("Button clicked: " + hit.transform.name);
                }
            }

            isCurveActive = false;
            HideGroundCircle();
        }

        // ���콺 �������� ��ũ�� ��ǥ�κ��� Ray�� �߻�
        Ray rayScreen = Camera.main.ScreenPointToRay(currentMousePosition);

        // Ray�� �� ������ ���
        Vector3 rayEndPoint = rayScreen.origin + rayScreen.direction * 5000;
        if (isGrabbing) { 
            if (Physics.Raycast(rayScreen, out RaycastHit hitInfo, 5000, grabbaleLayer))
            {
                rayEndPoint = hitInfo.point;

            }
        }   
        else
        {
            if (Physics.Raycast(rayScreen, out RaycastHit hitInfo, 5000))
            {
                rayEndPoint = hitInfo.point;

            }
        }
            // Raycast�� ���콺 �����ӿ� ���� ������ ������ ������Ʈ
            Ray ray = new Ray(transform.position, rayEndPoint - transform.position);
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, lineDistance, groundLayer | interactableLayer | grabbaleLayer))
        {
            groundPoint = hit.point;
            validGroundPoint = true;

            if (!isGrabbing)
            {
                //clickObject = hit.collider.gameObject;
                if (((1 << hit.collider.gameObject.layer) & groundLayer) != 0)
                {
                    //validGroundPoint = true;
                    isCurveActive = true;
                    ChangeLineColor(Color.red);
                    ShowGroundCircle(groundPoint); // �ڷ���Ʈ ����� Ȱ��ȭ�� ��쿡�� ���� ǥ��
                    DrawBezierCurve(ray.origin, groundPoint); // ������ � �׸���
                }

                else if (((1 << hit.collider.gameObject.layer) & interactableLayer | grabbaleLayer) != 0)
                {

                    //Debug.Log(hit.collider.gameObject.name);
                    ChangeLineColor(Color.blue); // Ư�� ��ü�� ����� �� �Ķ������� ����
                    SetLineWidth(interactLineWidth); // Ư�� ��ü�� ����� �� ������ ��� ����

                    Vector3 hitPoint = hit.point; // �浹 ������ ��ġ�� ����
                    lineRenderer.SetPosition(1, hitPoint); // ���η������� �� ��ġ�� �浹 �������� ����

                    selectedObject = hit.collider.gameObject;
                    //Debug.Log(selectedObject.name);

                }
                else
                {
                    isCurveActive = false;
                    ChangeLineColor(Color.white); // �׷��� ������ �⺻ �������� ����
                    SetLineWidth(defaultLineWidth); // �⺻ �β��� ����
                }
            }
            if(!isCurveActive)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, hit.point); // ����ĳ��Ʈ�� ���� �������� ���� �׸���
            }

            // Frictions
            if (isMousePressed && selectedObject != null && ((1 << selectedObject.layer) & interactableLayer) != 0 &&
    selectedObject.layer != LayerMask.GetMask("INTERACTUI"))
            {
                float rotationSpeed = 100.0f; // ȸ�� �ӵ� ������ ���� ���
                float mouseY = Input.GetAxis("Mouse Y"); // ���콺 Y�� �̵� ��
                float rotationAmount = 0; // ȸ���� ���� ���

                // ���� �����̼��� �ʱ� �����̼� �������� ��ȯ (������� ����)
                Quaternion currentRotation = selectedObject.transform.localRotation;
                Quaternion relativeRotation = Quaternion.Inverse(initialRotation) * currentRotation;

                // Z�� ȸ�� ������ ������
                float zAngle = relativeRotation.eulerAngles.x;

                // zAngle ���� -180������ 180�� ������ ��ȯ
                if (zAngle > 180)
                {
                    zAngle -= 360;
                }

                // ȸ���Ϸ��� ���� ������ ������ �Ѿ�� �ʵ��� ����
                float targetAngle = zAngle - mouseY * rotationSpeed * Time.deltaTime;
                targetAngle = Mathf.Clamp(targetAngle, -25, 0);

                // ���ѵ� ȸ�� �������� ���� ������ ���� ���� ȸ���� ���� ���
                rotationAmount = targetAngle - zAngle;

                // ������ Z�� �������� ȸ�� ����
                selectedObject.transform.Rotate(-Vector3.right, rotationAmount);

                // ȸ���� �Ϸ�� �Ŀ��� ��� �ʱ� �����̼��� �������� ����
                //initialRotation = selectedObject.transform.localRotation;
            }

            //Grab
            groundPoint = hit.point;
            validGroundPoint = true;
            //Grab - Ŭ���� ������Ʈ�� Grabbable ���̾ ���ϴ� ���
            if (((1 << hit.collider.gameObject.layer) & grabbaleLayer) != 0)
            {
                // �׷� ������ ������Ʈ�� Ŭ���� ���
                if (isMousePressed && !isGrabbing)
                {
                    selectedObject = hit.collider.gameObject;
                    if (selectedObject != null)
                    {
                        isGrabbing = true;
                        selectedObject.GetComponent<Rigidbody>().isKinematic = true; // ����ȿ�� ����

                        // ó�� ����� ���� Ray �������� ��ü ������ �Ÿ� ���
                        grabDistance = Vector3.Distance(Camera.main.transform.position, hit.point);

                        // ��ü�� Ray ���� ������� ��ġ (������) ���
                        grabOffset = selectedObject.transform.position - hit.point;
                    }
                }

                // �׷� ������ ���, ó�� ���� �Ÿ��� ����� ��ġ�� �����ϸ鼭 ������
                if (isGrabbing && selectedObject != null)
                {
                    Vector3 targetPosition = ray.GetPoint(grabDistance) + grabOffset;
                    selectedObject.transform.position = ray.origin + ray.direction * grabDistance; 
                }
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

    public void RecenterView()
    {
        Api.Recenter();
    }
    void ReleaseObject()
    {
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Rigidbody>().isKinematic = false; // ����ȿ�� �ٽ� ����
            selectedObject = null;
            
        }
        isGrabbing = false;
    }

}
