using UnityEngine;

public class HexMapCamera : MonoBehaviour 
{
    public static HexMapCamera instance;
    public static bool Locked { set { instance.enabled = !value; } }
    public Transform followTransform;
    public HexGrid grid;
    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float movementTime;
    public float fastSpeedMultiplier;
    public float rotationSpeed;
    public Vector3 zoomAmount;
    public Vector3 dragStartPosition;
    public Vector3 dragCurrentPosition;
    public Vector3 rotateStartPosition;
    public Vector3 rotateCurrentPosition;
	Transform swivel, stick;
    float zoom = 1f;
    float rotationAngle;
    Vector3 newZoom;
    Vector3 newPosition;
    Quaternion cameraZoomRotation;
    Quaternion newRotation;
    float speedMultiplier;

	void Awake() 
    {
        instance = this;
		swivel = transform.GetChild(0);
		stick = swivel.GetChild(0);
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = stick.localPosition;
        cameraZoomRotation = swivel.localRotation;
	}

    void Update()
    {
        if (followTransform != null)
        {
            transform.position = followTransform.position;
        }
        else
        {
            HandleMouseInput();
            HandleKeyboardInput();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        stick.localPosition = Vector3.Lerp(stick.localPosition, newZoom, Time.deltaTime * movementTime);
        swivel.localRotation = Quaternion.Lerp(swivel.localRotation, cameraZoomRotation, Time.deltaTime * movementTime);
    }

    public static void ValidatePosition() 
    {
		instance.AdjustPosition(0f, 0f);
	}

    private void HandleKeyboardInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedMultiplier = fastSpeedMultiplier;
        }
        else
        {
            speedMultiplier = 0.5f;
        }

        if (Input.GetKey(KeyCode.R))
        {
            AdjustZoom(0.01f);
        }
        if (Input.GetKey(KeyCode.F))
        {
            AdjustZoom(-0.01f);
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
        if (Input.GetMouseButtonDown(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = ClampPosition(transform.position + dragStartPosition - dragCurrentPosition);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            rotateStartPosition = Input.mousePosition;
        }        
        if (Input.GetMouseButton(1))
        {
            rotateCurrentPosition = Input.mousePosition;

            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
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
        newRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }

    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom * speedMultiplier, moveSpeedMaxZoom * speedMultiplier, zoom) * damping;

        newPosition = transform.position;
        newPosition += direction * distance;
        newPosition = ClampPosition(newPosition);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.cellCountX - 0.5f) * (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax = (grid.cellCountZ - 1f) * (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);
        return position;
    }

    void AdjustZoom(float delta)
    {
        zoom = Mathf.Clamp01(zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        newZoom = new Vector3(0f, 0f, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        cameraZoomRotation = Quaternion.Euler(angle, 0f, 0f);
    }
}