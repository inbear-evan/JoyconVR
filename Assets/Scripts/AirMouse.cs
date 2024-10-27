using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class AirMouse : MonoBehaviour
{
    [SerializeField] bool showMouse = true;
    [SerializeField] string groundTag = "GROUND";
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grabbaleLayer;
    [SerializeField] float rotateSpeed = 5.0f;
    [SerializeField] float movementThreshold = 2.0f;
    [SerializeField] float holdTimeThreshold = 1.0f;
    [SerializeField] float lineDistance = 10;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] GameObject groundCirclePrefab;
    [SerializeField] float defaultLineWidth = 0.1f;
    [SerializeField] float interactLineWidth = 0.05f;
    [SerializeField] float grabDistanceAdjustSpeed = 0.5f;
    [SerializeField] GrapPosition grapPosition;
    [SerializeField] GrapPosition grapPosition2;

    private Vector3 lastMousePosition;
    private bool isMousePressed;
    private bool isCurveActive;
    private Vector3 groundPoint;
    private bool validGroundPoint;
    private GameObject groundCircleInstance;
    private Material groundCircleInstanceMaterial;
    private RaycastHit hit;
    private GameObject clickObject;
    private GameObject selectedObject;

    private bool isGrabbing = false;
    private Vector3 grabOffset;
    private float grabDistance = 3.0f;
    private Vector3 initialRayOrigin;
    private Vector3 initialRayDirection;
    private Quaternion initialRotation;

    [SerializeField] float frictionRotationSpeed = 100.0f; // Friction 회전 속도
    [SerializeField] float grabDistanceMin = 1.0f;
    [SerializeField] float grabDistanceMax = 2.5f;

    void Start()
    {
        Cursor.visible = showMouse;
        lastMousePosition = Input.mousePosition;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = defaultLineWidth;
            lineRenderer.endWidth = defaultLineWidth;
            ChangeLineColor(Color.white);
        }

        if (groundCirclePrefab != null)
        {
            groundCircleInstance = Instantiate(groundCirclePrefab);
            groundCircleInstance.SetActive(false);
            groundCircleInstanceMaterial = groundCircleInstance.GetComponent<Renderer>().material;
        }
    }

    void Update()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;

        if (mouseDelta.magnitude > movementThreshold)
        {
            float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
            float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;

            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            float newLocalRotationX = localEulerAngles.x - rotateX;
            if (newLocalRotationX > 180) newLocalRotationX -= 360;
            newLocalRotationX = Mathf.Clamp(newLocalRotationX, -90, 90);

            float newLocalRotationY = localEulerAngles.y + rotateY;
            if (newLocalRotationY > 180) newLocalRotationY -= 360;
            newLocalRotationY = Mathf.Clamp(newLocalRotationY, -90, 90);

            transform.localRotation = Quaternion.Euler(newLocalRotationX, newLocalRotationY, 0);
            lastMousePosition = currentMousePosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isGrabbing)
            {
                if (grapPosition != null && grapPosition.isInsideCollider)
                {
                    grapPosition.DisableUp(selectedObject); // 첫 번째 GrapPosition의 범위 내에서 놓기
                }
                else if (grapPosition2 != null && grapPosition2.isInsideCollider)
                {
                    grapPosition2.DisableUp(selectedObject); // 두 번째 GrapPosition의 범위 내에서 놓기
                }
                ReleaseObject();
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
                }
            }

            isCurveActive = false;
            HideGroundCircle();
        }

        Ray ray = new Ray(transform.position, transform.forward);
        Vector3 rayEndPoint = ray.origin + ray.direction * lineDistance;

        // 그랩 거리 조절
        if (isGrabbing)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            grabDistance = Mathf.Clamp(grabDistance + scroll * grabDistanceAdjustSpeed, grabDistanceMin, grabDistanceMax);
        }

        if (!isGrabbing)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, lineDistance, groundLayer | interactableLayer | grabbaleLayer))
            {
                groundPoint = hitInfo.point;
                validGroundPoint = true;
                clickObject = hitInfo.collider.gameObject;

                if (((1 << hitInfo.collider.gameObject.layer) & groundLayer) != 0)
                {
                    isCurveActive = true;
                    ChangeLineColor(Color.red);
                    ShowGroundCircle(groundPoint);
                    DrawBezierCurve(ray.origin, groundPoint);
                }
                else if (((1 << hitInfo.collider.gameObject.layer) & interactableLayer | grabbaleLayer) != 0)
                {
                    ChangeLineColor(Color.blue);
                    SetLineWidth(interactLineWidth);
                    lineRenderer.SetPosition(1, hitInfo.point);
                    selectedObject = hitInfo.collider.gameObject;
                }
                else
                {
                    isCurveActive = false;
                    ChangeLineColor(Color.white);
                    SetLineWidth(defaultLineWidth);
                }

                if (!isCurveActive)
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, ray.origin);
                    lineRenderer.SetPosition(1, hitInfo.point);
                }

                if (((1 << hitInfo.collider.gameObject.layer) & grabbaleLayer) != 0 && isMousePressed && !isGrabbing)
                {
                    selectedObject = hitInfo.collider.gameObject;
                    if (selectedObject != null)
                    {
                        isGrabbing = true;
                        selectedObject.GetComponent<Rigidbody>().isKinematic = true;
                        grabDistance = Vector3.Distance(transform.position, hitInfo.point);
                        grabOffset = selectedObject.transform.position - ray.GetPoint(grabDistance);
                        initialRayOrigin = ray.origin;
                        initialRayDirection = ray.direction;
                        initialRotation = selectedObject.transform.localRotation; // 초기 회전 저장
                    }
                }
            }
            else
            {
                clickObject = null;
                validGroundPoint = false;
                HideGroundCircle();
                ChangeLineColor(Color.white);
                SetLineWidth(defaultLineWidth);
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, ray.origin);
                lineRenderer.SetPosition(1, rayEndPoint);
            }
        }
        else if (isGrabbing && selectedObject != null)
        {
            initialRayOrigin = transform.position;
            initialRayDirection = transform.forward;

            Vector3 targetPosition = initialRayOrigin + initialRayDirection * grabDistance + grabOffset;
            selectedObject.transform.position = targetPosition;

            lineRenderer.SetPosition(0, initialRayOrigin);
            lineRenderer.SetPosition(1, selectedObject.transform.position);

            float rotateX = mouseDelta.y * rotateSpeed * Time.deltaTime;
            float rotateY = mouseDelta.x * rotateSpeed * Time.deltaTime;

            Vector3 localEulerAngles = transform.localRotation.eulerAngles;
            float newLocalRotationX = localEulerAngles.x - rotateX;
            if (newLocalRotationX > 180) newLocalRotationX -= 360;
            newLocalRotationX = Mathf.Clamp(newLocalRotationX, -90, 90);

            float newLocalRotationY = localEulerAngles.y + rotateY;
            if (newLocalRotationY > 180) newLocalRotationY -= 360;
            newLocalRotationY = Mathf.Clamp(newLocalRotationY, -90, 90);

            transform.localRotation = Quaternion.Euler(newLocalRotationX, newLocalRotationY, 0);
            lastMousePosition = currentMousePosition;
        }

        // friction 로직: 그랩 중이거나 상호작용 레이어에 있을 때 동작
        if (isMousePressed && selectedObject != null && ((1 << selectedObject.layer) & interactableLayer) != 0)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            float rotationAmount = mouseY * frictionRotationSpeed * Time.deltaTime;

            selectedObject.transform.Rotate(Vector3.right, rotationAmount, Space.Self);
        }
    }

    public GameObject GetCollisionObject()
    {
        return clickObject;
    }

    void ReleaseObject()
    {
        if (selectedObject != null)
        {
            selectedObject.GetComponent<Rigidbody>().isKinematic = false;
            selectedObject = null;
        }
        isGrabbing = false;
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
            Vector3 controlPoint = (startPoint + endPoint) / 2 + Vector3.up * 2.0f;
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
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    void TeleportToGroundPoint()
    {
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
}
