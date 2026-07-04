using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Camera")]
    public Transform cameraTransform; // Ссылка на Transform камеры (или её родителя)
    public float mouseSensitivity = 2f;
    public float cameraPitchMin = -70f;
    public float cameraPitchMax = 70f;

    private float cameraPitch = 0f; // Текущий наклон камеры по X

    private PlayerControls controls;
    // PlayerInventoryController inventoryController;
    private Vector2 lookInput;

    void Awake()
    {
        controls = new PlayerControls();
        // inventoryController = GetComponent<PlayerInventoryController>();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void UpdateLook(InputAction.CallbackContext context)
    {
        if (context.performed) lookInput = context.ReadValue<Vector2>();
        else lookInput = Vector2.zero;
    }

    void Update()
    {
        // Вращение персонажа по оси Y (горизонталь)
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(0, mouseX, 0);

        // Вращение камеры по оси X (вертикаль)
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }
}
