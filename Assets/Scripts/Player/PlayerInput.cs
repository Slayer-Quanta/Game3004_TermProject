using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
	public event Action OnMouseClick, OnMouseRightClick, OnFly, OnInventoryToggle, OnPause;
    [field: SerializeField] public bool RunningPressed { get; private set; }
    [field: SerializeField] public Vector3 MovementInput { get; private set; }
	[field:SerializeField] public Vector2 MousePosition { get; private set; }
    [field: SerializeField] public bool IsJumping { get; private set; }

    [Header("Touch Input")]
    public bool useTouchInput;
    public float touchSensitivity = 0.1f;
    public float deadZone = 1.5f;
    int currentLookTouchId;
    EventSystem eventSystem;
    InputActions inputActions;
    private void Awake()
    {
        eventSystem = EventSystem.current;
        InitInput();
    }
    private void InitInput()
    {
        currentLookTouchId = -1;
        inputActions = new InputActions();
        inputActions.Enable();

<<<<<<< Updated upstream
    void Update()
	{
		GetMouseClick();
		GetMousePosition();
		GetMovementInput();
		GetJumpInput();
		GetRunInput();
		GetFlyInput();
        GetPauseInput();
    }

	private void GetFlyInput()
	{
		if (Input.GetKeyDown(KeyCode.V))
		{
			OnFly?.Invoke();
		}
	}

	private void GetRunInput()
	{
		RunningPressed = Input.GetKey(KeyCode.LeftShift);
	}

	private void GetJumpInput()
	{
		IsJumping = Input.GetButton("Jump");
	}

	private void GetMovementInput()
	{
		MovementInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
	}

	private void GetMousePosition()
	{
		MousePosition = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
	}

	private void GetMouseClick()
	{
		if (Input.GetMouseButtonDown(0))
		{
			OnMouseClick?.Invoke();

		}
	}
    private void GetPauseInput()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            OnPause?.Invoke();
        }
=======

        inputActions.Player.Move.performed += context =>
        {
            MovementInput = new(context.ReadValue<Vector2>().x, 0, context.ReadValue<Vector2>().y);
        };
        inputActions.Player.Move.canceled += context =>
        {
            MovementInput = Vector3.zero;
        };

        if (!useTouchInput)
        {
            inputActions.Player.Look.performed += context =>
            {
                MousePosition = new(context.ReadValue<Vector2>().x, context.ReadValue<Vector2>().y);
            };
            inputActions.Player.Look.canceled += context =>
            {
                MousePosition = Vector2.zero;
            };
        }
        inputActions.Player.Jump.performed += _ => IsJumping = true;
        inputActions.Player.Jump.canceled += _ => IsJumping = false;

        inputActions.Player.Run.performed += _ => RunningPressed = true;
        inputActions.Player.Run.canceled += _ => RunningPressed = false;

        inputActions.Player.Fly.performed += _ => OnFly?.Invoke();
        inputActions.Player.Inventory.performed += _ => OnInventoryToggle?.Invoke();
        inputActions.Player.Back.performed += _ => OnPause?.Invoke();
        inputActions.Player.Attack.performed += _ => OnMouseClick?.Invoke();
        inputActions.Player.Use.performed += _ => OnMouseRightClick?.Invoke();
    }
    void Update()
	{
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (!useTouchInput) { return; }

        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began &&
                !IsTouchOverUI(touch.position) &&
                currentLookTouchId == -1)
            {
                currentLookTouchId = touch.fingerId;
            }

            if ((touch.phase == TouchPhase.Ended ||
                touch.phase == TouchPhase.Canceled) &&
                currentLookTouchId == touch.fingerId)
            {
                currentLookTouchId = -1;
            }

            if (touch.fingerId == currentLookTouchId)
            {
                MousePosition = touch.deltaPosition.magnitude > deadZone ?
                    touch.deltaPosition * touchSensitivity : Vector2.zero;
            }
        } 

        if (currentLookTouchId == -1)
        {
            MousePosition = Vector2.zero;
        }
    }


    private bool IsTouchOverUI(Vector2 touchPosition)
    {
        PointerEventData eventData = new PointerEventData(eventSystem) { position = touchPosition };
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, results);
        return results.Count > 0;
>>>>>>> Stashed changes
    }
}