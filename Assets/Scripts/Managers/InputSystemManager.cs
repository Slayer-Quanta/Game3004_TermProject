using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemManager : MonoBehaviour
{
    [Header("Input Action Asset")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Action Map Settings")]
    [SerializeField] private string actionMapName = "Player";

    [Header("Input Action Names")]
    [SerializeField] private string[] actionNames = { "Move", "Jump", "Run", "Look", "Attack" };

    private InputAction[] inputActionsArray;

    public Vector3 MoveInput { get; private set; }
    public bool JumpInput { get; private set; }
    public float RunInput { get; private set; }
    public bool AttackInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    private static InputSystemManager instance;
    public static InputSystemManager Instance
    {
        get => instance;
        private set => instance = value;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InputSystemManager initialized successfully.");
        }
        else
        {
            Debug.LogWarning("Duplicate InputSystemManager detected. Destroying the new instance.");
            Destroy(gameObject);
            return;
        }

        InitializeInputActions();
        RegisterInputCallbacks();
    }

    private void InitializeInputActions()
    {
        Debug.Log("Initializing input actions.");
        var actionMap = inputActions.FindActionMap(actionMapName);

        if (actionMap == null)
        {
            Debug.LogError($"Action Map '{actionMapName}' not found.");
            return;
        }

        inputActionsArray = new InputAction[actionNames.Length];

        for (int i = 0; i < actionNames.Length; i++)
        {
            inputActionsArray[i] = actionMap.FindAction(actionNames[i]);

            if (inputActionsArray[i] == null)
            {
                Debug.LogError($"Action '{actionNames[i]}' not found in Action Map '{actionMap.name}'.");
            }
        }
    }

    private void RegisterInputCallbacks()
    {
        Debug.Log("Registering input callbacks.");

        if (inputActionsArray == null || inputActionsArray.Length == 0)
        {
            Debug.LogWarning("Input actions array is null or empty.");
            return;
        }

        for (int i = 0; i < actionNames.Length; i++)
        {
            switch (actionNames[i])
            {
                case "Move":
                    inputActionsArray[i].performed += OnMovePerformed;
                    inputActionsArray[i].canceled += OnMoveCanceled;
                    break;
                case "Jump":
                    inputActionsArray[i].performed += OnJumpPerformed;
                    inputActionsArray[i].canceled += OnJumpCanceled;
                    break;
                case "Run":
                    inputActionsArray[i].performed += OnRunPerformed;
                    inputActionsArray[i].canceled += OnRunCanceled;
                    break;
                case "Look":
                    inputActionsArray[i].performed += OnLookPerformed;
                    inputActionsArray[i].canceled += OnLookCanceled;
                    break;
                case "Attack":
                    inputActionsArray[i].performed += OnAttackPerformed;
                    break;
            }
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        MoveInput = new Vector3(input.x, 0, input.y);
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        MoveInput = Vector3.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        JumpInput = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        JumpInput = false;
    }

    private void OnRunPerformed(InputAction.CallbackContext ctx)
    {
        RunInput = ctx.ReadValue<float>();
    }

    private void OnRunCanceled(InputAction.CallbackContext ctx)
    {
        RunInput = 0;
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        LookInput = Vector2.zero;
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        AttackInput = true;
    }

    private void OnEnable()
    {
        if (inputActionsArray != null)
        {
            foreach (var action in inputActionsArray)
            {
                action?.Enable();
            }
        }
    }

    private void OnDisable()
    {
        if (inputActionsArray != null)
        {
            foreach (var action in inputActionsArray)
            {
                action?.Disable();
            }
        }
    }
}
