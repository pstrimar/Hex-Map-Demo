using System;
using UnityEngine;

public class HexMapCamera : MonoBehaviour 
{
    public HexGrid grid;
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float movementTime;
    public float fastSpeedMultiplier;
    public float rotationSpeed;
    public Vector3 zoomAmount;
    public Vector3 newZoom;
    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
	Transform swivel, stick;
    float zoom = 1f;
    float rotationAngle;
    Quaternion cameraRotation;
    float speedMultiplier;

	void Awake() 
    {
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
        newZoom = stick.localPosition;
        cameraRotation = swivel.localRotation;
	}

    void Update()
    {
        HandleMouseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedMultiplier = fastSpeedMultiplier;
        }
        else
        {
            speedMultiplier = 1;
        }

        if (Input.GetKey(KeyCode.R))
        {
            AdjustZoom(10);
        }
        if (Input.GetKey(KeyCode.F))
        {
            AdjustZoom(-10);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0 || zDelta != 0)
        {
            AdjustPosition(xDelta, zDelta);
        }

        stick.localPosition = Vector3.Lerp(stick.localPosition, newZoom, Time.deltaTime * movementTime);
        swivel.localRotation = Quaternion.Lerp(swivel.localRotation, cameraRotation, Time.deltaTime * movementTime);
    }

    void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            if (zoomDelta != 0f)
            {
                AdjustZoom(zoomDelta);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(1))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                Vector3 newPosition = ClampPosition(transform.localPosition + dragStartPosition - dragCurrentPosition);
                transform.localPosition = newPosition;
            }
        }
    }

    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom * speedMultiplier, moveSpeedMaxZoom * speedMultiplier, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1f) * (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);
        return position;
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        //stick.localPosition = new Vector3(0f, 0f, distance);
        newZoom = new Vector3(0f, 0f, distance);
        //stick.localPosition = Vector3.Lerp(stick.localPosition, newPosition, Time.deltaTime * movementTime);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        cameraRotation = Quaternion.Euler(angle, 0f, 0f);
        // swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}