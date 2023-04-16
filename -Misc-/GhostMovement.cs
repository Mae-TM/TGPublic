using UnityEngine;

public class GhostMovement : MonoBehaviour
{
	[SerializeField]
	private float movementSpeed = 20f;

	[SerializeField]
	private Transform camera;

	private void Update()
	{
		if (!KeyboardControl.IsKeyboardBlocked())
		{
			Vector2 vector = KeyboardControl.PlayerControls.Move.ReadValue<Vector2>();
			Vector3 vector2;
			if (camera != null)
			{
				Vector3 normalized = Vector3.Scale(camera.forward, new Vector3(1f, 0f, 1f)).normalized;
				vector2 = vector.y * normalized + vector.x * camera.right;
			}
			else
			{
				vector2 = vector.y * Vector3.forward + vector.x * Vector3.right;
			}
			Vector3 localPosition = base.transform.localPosition + vector2 * movementSpeed * Time.deltaTime;
			localPosition.x = Mathf.Clamp(localPosition.x, -45f, 45f);
			localPosition.z = Mathf.Clamp(localPosition.z, -45f, 45f);
			base.transform.localPosition = localPosition;
			Debug.DrawRay(base.transform.position, vector2, Color.cyan);
		}
	}
}
