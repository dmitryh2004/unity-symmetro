using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour, SeatableEntity
{
    public float moveSpeed = 5f;
    public float crouchSpeed = 2f;
    public float sprintSpeed = 8f;
    public float crouchHeight = 1f;
    public float standHeight = 2f;
    public float heightChangeSpeed = 5f;
    public float jumpForce = 7f;
    public LayerMask groundMask;
    public float groundCheckRadius = 0.2f;
    public float headCheckRadius = 0.3f;
    public Transform groundCheckPoint;

    private float standScale = .9f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isCrouching;
    private bool isSprinting;
    private float currentHeight;
    private float currentSpeed;
    private bool isJumpPressed;
    private bool isGrounded;
    private bool isSeated = false;
    private TrainSeat currentSeat = null;

    private PlayerControls controls;
    // PlayerInventoryController inventoryController;
    //[SerializeField] PlayerAudioPlayer audioPlayer;
    private float footstepTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // inventoryController = GetComponent<PlayerInventoryController>();
        controls = new PlayerControls();
        currentHeight = standHeight;
        standScale = transform.localScale.y;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    public void UpdateMove(InputAction.CallbackContext context)
    {
        if (context.started) return;

        moveInput = context.ReadValue<Vector2>();
    }


    public void UpdateCrouch(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        isCrouching = value > 0;
    }

    public void UpdateSprint(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        isSprinting = value > 0 && !isCrouching;
    }

    public void UpdateJump(InputAction.CallbackContext context)
    {
        isJumpPressed = (context.performed) ? true : false;
    }

    public void SetSeated(bool seated)
    {
        isSeated = seated;
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateFootstepTimer();
    }

    void UpdateFootstepTimer()
    {
        bool isMoving = moveInput != Vector2.zero;
        if (isMoving && isGrounded && (!isCrouching))
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= 2f / currentSpeed)
            {
                footstepTimer = 0f;
            }
        }
        else
            footstepTimer = 0f;
    }

    void UpdateSpeed()
    {
        // 1. Проверка: есть ли что-то над головой?
        // Пускаем луч/сферу вверх от текущей позиции
        bool canStandUp = !Physics.SphereCast(transform.position, headCheckRadius, Vector3.up, out _, (standHeight - crouchHeight) / 2f, groundMask);

        // 2. Логика определения скорости и состояния
        // Если игрок ХОЧЕТ встать (isCrouching == false), но НЕ МОЖЕТ (canStandUp == false), 
        // мы заставляем его оставаться в состоянии приседа.
        bool effectivelyCrouching = isCrouching || !canStandUp;

        currentSpeed = moveSpeed;
        if (isSprinting && !effectivelyCrouching) currentSpeed = sprintSpeed;
        if (effectivelyCrouching) currentSpeed = crouchSpeed;

        // 3. Плавное изменение высоты на основе effectivelyCrouching
        float targetHeight = (effectivelyCrouching || isSeated) ? crouchHeight : standHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * heightChangeSpeed);

        // Вычисляем, какой МИРОВОЙ масштаб по Y должен быть у игрока прямо сейчас
        float targetWorldScaleY = (currentHeight / standHeight) * standScale;

        // По умолчанию локальный масштаб равен мировому (если родителя нет)
        float localScaleY = targetWorldScaleY;

        // Если у игрока есть родитель, делим целевой мировой масштаб на масштаб родителя по оси Y
        if (transform.parent != null)
        {
            // lossyScale.y возвращает суммарный мировой масштаб всей родительской цепочки по оси Y
            localScaleY = targetWorldScaleY / transform.parent.lossyScale.y;
        }

        // Применяем скорректированный локальный масштаб
        transform.localScale = new Vector3(transform.localScale.x, localScaleY, transform.localScale.z);

        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundMask);
    }

    void FixedUpdate()
    {
        if (isSeated) return; // если игрок сидит - не двигаемся/прыгаем

        // базовое желаемое направление (в локальных осях)
        Vector3 inputDir = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;

        // по умолчанию двигаемся просто по XZ
        Vector3 moveDirection = inputDir;

        // если на земле — проецируем направление на плоскость склона
        if (isGrounded)
        {
            // получаем нормаль поверхности под ногами
            if (Physics.Raycast(groundCheckPoint.position, Vector3.down, out RaycastHit hit, 1f, groundMask))
            {
                Vector3 groundNormal = hit.normal;

                // проекция направления на плоскость с этой нормалью
                moveDirection = Vector3.ProjectOnPlane(inputDir, groundNormal).normalized;
            }
        }

        // задаём скорость вдоль плоскости
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveDirection.x * currentSpeed;
        velocity.z = moveDirection.z * currentSpeed;
        rb.linearVelocity = velocity;

        // прыжок
        if (isJumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
            isJumpPressed = false;
        }
    }

    public void Seat(TrainSeat seat)
    {
        isSeated = true;
        currentSeat = seat;

        bool isDriverChair = seat.gameObject.CompareTag("TrainDriverChair");
        InputActionMapSwitcher.Instance.SwitchMap(isDriverChair ? "TrainCabinSeat" : "Seat");
        rb.Sleep();
    }

    public void LaunchStandUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (currentSeat != null) currentSeat.StandUpEntity(this);
    }

    public void StandUp()
    {
        isSeated = false;
        currentSeat = null;
        InputActionMapSwitcher.Instance.SwitchMap("Movement");
        rb.WakeUp();
    }

    public Transform GetTransform() => transform;
    public GameObject GetGameObject() => gameObject;
}
