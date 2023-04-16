using UnityEngine;
using UnityStandardAssets.Cameras;

public class MSPAOrthoController : AbstractTargetFollower
{
	public static Camera main;

	public float cameraAngle;

	[SerializeField]
	private float positionDampTime = 0.3f;

	[SerializeField]
	private float angleVelocity = 6f;

	[SerializeField]
	private float zoomVelocity = 5f;

	public float zoom = 1f;

	[SerializeField]
	private float leniency = 0.1875f;

	[SerializeField]
	private float distance = 40f;

	private Vector3 cameraVelocity;

	private Vector3 focusPoint;

	private float currentZoom = 1f;

	private float currentCameraAngle;

	private Vector3 cameraForward;

	private Vector3 billboardForward;

	private Quaternion billboardRotation;

	private void Awake()
	{
		cameraForward = base.transform.forward;
		billboardForward = Vector3.ProjectOnPlane(cameraForward, Vector3.up);
		if (!main)
		{
			main = GetComponent<Camera>();
		}
		if (!base.Target)
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		base.transform.position = GetTargetPosition();
		cameraVelocity = Vector3.zero;
	}

	public override void SetTarget(Transform to)
	{
		SetTarget(to, leniency);
	}

	public void SetTarget(Transform to, float leniency)
	{
		if (base.Target != to)
		{
			m_Target = to;
			this.leniency = leniency;
			base.transform.position = GetTargetPosition();
			cameraVelocity = Vector3.zero;
			base.enabled = true;
		}
	}

	public void SetZoom(float value)
	{
		zoom = 1.6666666f * Mathf.Exp(value);
	}

	public void ForceUpdate()
	{
		FollowTarget(float.PositiveInfinity);
	}

	protected override void FollowTarget(float deltaTime)
	{
		float num = currentCameraAngle;
		currentCameraAngle = Mathf.Lerp(num, cameraAngle, angleVelocity * deltaTime);
		float a = currentZoom;
		currentZoom = Mathf.Lerp(a, zoom, zoomVelocity * deltaTime);
		base.transform.position += TransformOffset(currentZoom, currentCameraAngle) - TransformOffset(a, num);
		if ((bool)base.Target)
		{
			billboardRotation = GetLookRotation(billboardForward);
			base.transform.rotation = GetLookRotation(cameraForward);
			Debug.DrawRay(base.Target.position, TransformOffset(currentZoom, currentCameraAngle), Color.red);
			base.transform.position = GetTargetPosition();
		}
	}

	private Vector3 TransformOffset(float zoom, float angle)
	{
		if (base.Target == null)
		{
			return Vector3.zero;
		}
		return base.Target.rotation * Quaternion.AngleAxis(angle, Vector3.up) * -cameraForward * (distance * zoom);
	}

	private Quaternion GetLookRotation(Vector3 forward)
	{
		Vector3 vector = base.Target.rotation * Quaternion.AngleAxis(currentCameraAngle, Vector3.up) * forward;
		Debug.DrawRay(base.transform.position, vector * 6f, Color.blue);
		return Quaternion.LookRotation(vector, base.Target.up);
	}

	public void SetFocusPoint(Vector3 to)
	{
		focusPoint = to;
	}

	private Vector3 GetTargetPosition()
	{
		Vector3 position = base.Target.position;
		Vector3 vector = TransformOffset(currentZoom, currentCameraAngle);
		Vector3 vector2 = base.transform.position - vector;
		Ray ray = main.ViewportPointToRay(0.5f * Vector3.one);
		Ray mouseRay = KeyboardControl.GetMouseRay(main);
		Plane plane = new Plane(base.Target.up, position);
		focusPoint = position;
		if (plane.Raycast(ray, out var enter) && plane.Raycast(mouseRay, out var enter2))
		{
			focusPoint += leniency * (mouseRay.GetPoint(enter2) - ray.GetPoint(enter));
		}
		focusPoint = Vector3.SmoothDamp(vector2, focusPoint, ref cameraVelocity, positionDampTime);
		MoveFocusToInclude(position, vector2, 0.5f);
		return focusPoint + vector;
	}

	private void MoveFocusToInclude(Vector3 pos, Vector3 prevFocus, float maxDist)
	{
		Vector2 b = main.WorldToViewportPoint(pos - focusPoint + prevFocus);
		float num = Vector2.Distance(0.5f * Vector3.one, b);
		if (num > maxDist)
		{
			focusPoint = pos + (focusPoint - pos) / num * maxDist;
		}
	}

	public Quaternion GetBillboardRotation()
	{
		return billboardRotation;
	}
}
