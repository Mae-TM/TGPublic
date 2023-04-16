using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotationControls : MonoBehaviour
{
	private MSPAOrthoController cameraController;

	private void Start()
	{
		cameraController = GetComponent<MSPAOrthoController>();
	}

	private void OnEnable()
	{
		KeyboardControl.CameraControls.Rotate.performed += Rotate;
	}

	private void OnDisable()
	{
		if (KeyboardControl.IsReady)
		{
			KeyboardControl.CameraControls.Rotate.performed -= Rotate;
		}
	}

	private void Rotate(InputAction.CallbackContext context)
	{
		if (!KeyboardControl.IsKeyboardBlocked())
		{
			cameraController.cameraAngle -= context.ReadValue<float>() * 180f;
		}
	}

	public void RotateRight()
	{
		cameraController.cameraAngle -= 180f;
	}

	public void RotateLeft()
	{
		cameraController.cameraAngle += 180f;
	}
}
