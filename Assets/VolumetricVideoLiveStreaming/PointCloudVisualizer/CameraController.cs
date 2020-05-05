// **********************************************************
// * CameraController.cs
// *
// * Reference
// *   http://esprog.hatenablog.com/entry/2016/03/20/033322
// *
// **********************************************************

using UnityEngine;

public class CameraController : MonoBehaviour
{
	public enum MouseButtonType
	{
		Left,
		Middle,
		Right,
		None,
	}

	void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		Track();
		MouseButtonType buttonType = GetInputMouseButton();
		switch(buttonType)
		{
			case MouseButtonType.Left:
				break;
			case MouseButtonType.Right:
				Rotate();
				break;
			case MouseButtonType.Middle:
				Move();
				break;
			default:
				break;
		}
#endif
	}

	private void Track()
	{
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		transform.position += transform.forward * 2.0f * scroll; 
	}

	private void Rotate()
	{
		Vector2 angle = Vector2.zero;
		angle.x = Input.GetAxis("Mouse X");
		angle.y = Input.GetAxis("Mouse Y");
    	transform.RotateAround(transform.position, Vector3.up, 2.0f * angle.x);
		transform.RotateAround(transform.position, transform.right, -2.0f * angle.y);
	}

	private void Move()
	{
		Vector3 horizontal = transform.right * (-Input.GetAxis("Mouse X")) * 0.5f;
		Vector3 vertical = transform.up * (-Input.GetAxis("Mouse Y")) * 0.5f;
		transform.position += (horizontal + vertical);
	}

	private MouseButtonType GetInputMouseButton()
	{
		if(Input.GetMouseButton(0))
		{
			return MouseButtonType.Left;
		}
		else if(Input.GetMouseButton(1))
		{
			return MouseButtonType.Right;
		}
		else if(Input.GetMouseButton(2))
		{
			return MouseButtonType.Middle;
		}
		else
		{
			return MouseButtonType.None;
		}
	}
}