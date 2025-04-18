using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	public event Action OnMouseClick, OnFly;
	public bool RunningPressed { get; private set; }
	public Vector3 MovementInput { get; private set; }
	public Vector2 MousePosition { get; private set; }
	public bool IsJumping { get; private set; }

    public event Action OnPause;

    public event Action OnInventoryToggle;

    void Update()
	{
		GetMouseClick();
		GetMousePosition();
		GetMovementInput();
		GetJumpInput();
		GetRunInput();
		GetFlyInput();
        GetPauseInput();
		GetInventoryInput();
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
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            OnMouseClick?.Invoke();
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click
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
    }

    private void GetInventoryInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            OnInventoryToggle?.Invoke();
        }
    }
}