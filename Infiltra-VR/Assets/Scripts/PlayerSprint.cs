using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

[DisallowMultipleComponent]
public class PlayerSprint : MonoBehaviour
{
    [Header("Movement Provider")]
    public ContinuousMoveProvider moveProvider;

    [Header("Speed Settings")]
    [Min(0f)] public float walkSpeed = 1.5f;
    [Min(0f)] public float runSpeed = 3f;

    [Header("Input Action (Opsional)")]
    [Tooltip("Jika kosong, otomatis memakai klik stik kiri controller XR.")]
    public InputActionReference sprintAction;

    private InputAction fallbackSprintAction;
    private InputAction activeSprintAction;
    private bool actionEnabledByThisComponent;
    private bool isSprinting;

    private void Awake()
    {
        if (moveProvider == null)
            moveProvider = GetComponent<ContinuousMoveProvider>();

        if (moveProvider == null)
            moveProvider = GetComponentInChildren<ContinuousMoveProvider>(true);
    }

    private void OnEnable()
    {
        activeSprintAction = sprintAction != null ? sprintAction.action : BuatFallbackSprintAction();
        if (activeSprintAction != null)
        {
            activeSprintAction.started += OnSprintStart;
            activeSprintAction.canceled += OnSprintEnd;
            actionEnabledByThisComponent = !activeSprintAction.enabled;
            if (actionEnabledByThisComponent)
                activeSprintAction.Enable();
        }

        TerapkanKecepatan(walkSpeed);
    }

    private void OnDisable()
    {
        if (activeSprintAction != null)
        {
            activeSprintAction.started -= OnSprintStart;
            activeSprintAction.canceled -= OnSprintEnd;
            if (actionEnabledByThisComponent)
                activeSprintAction.Disable();
        }

        isSprinting = false;
        TerapkanKecepatan(walkSpeed);
        activeSprintAction = null;
        actionEnabledByThisComponent = false;
    }

    private void OnDestroy()
    {
        fallbackSprintAction?.Dispose();
    }

    private void OnValidate()
    {
        walkSpeed = Mathf.Max(0f, walkSpeed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
    }

    private InputAction BuatFallbackSprintAction()
    {
        if (fallbackSprintAction != null)
            return fallbackSprintAction;

        fallbackSprintAction = new InputAction("Sprint", InputActionType.Button);
        fallbackSprintAction.AddBinding("<XRController>{LeftHand}/{Primary2DAxisClick}");
        fallbackSprintAction.AddBinding("<Gamepad>/leftStickPress");
        fallbackSprintAction.AddBinding("<Keyboard>/leftShift");
        return fallbackSprintAction;
    }

    private void OnSprintStart(InputAction.CallbackContext context)
    {
        isSprinting = true;
        TerapkanKecepatan(runSpeed);
    }

    private void OnSprintEnd(InputAction.CallbackContext context)
    {
        isSprinting = false;
        TerapkanKecepatan(walkSpeed);
    }

    private void TerapkanKecepatan(float speed)
    {
        if (moveProvider != null)
            moveProvider.moveSpeed = speed;
    }

    public void SetRunSpeed(float speed)
    {
        runSpeed = Mathf.Max(walkSpeed, speed);
        if (isSprinting)
            TerapkanKecepatan(runSpeed);
    }

    public void SetWalkSpeed(float speed)
    {
        walkSpeed = Mathf.Max(0f, speed);
        runSpeed = Mathf.Max(walkSpeed, runSpeed);
        if (!isSprinting)
            TerapkanKecepatan(walkSpeed);
    }
}
