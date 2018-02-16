using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPGCameraController : MonoBehaviour {

	public Transform m_target;
	public float m_heightOffset = 1.7f;
	public float m_distance = 12.0f;
	public float m_offsetFromWall = 0.1f;
	public float m_maxDistance = 20.0f;
	public float m_minDistance = 0.6f;
	public float m_xSpeed = 200.0f;
	public float m_ySpeed = 200.0f;
	public float m_yMinLimit = -80.0f;
	public float m_yMaxLimit = 80.0f;
	public float m_zoomSpeed = 5.0f;
	public float m_autoRotationSpeed = 5.0f;
	public LayerMask m_collisionLayers = -1;
	public bool m_alwaysRotateToRearOfTarget = false;
	public bool m_allowMouseInputX = true;
	public bool m_allowMouseInputY = true;

	private float m_xDeg = 0.0f;
	private float m_yDeg = 0.0f;
	private float m_currentDistance;
	private float m_desiredDistance;
	private float m_correctedDistance;
	private bool m_rotateBehind = false;
	private bool m_mouseSideButton = false;

	void Start() {
		Vector3 angles = transform.eulerAngles;
		Vector3 distance = m_target.position - transform.position;
		m_xDeg = angles.x;
		m_yDeg = angles.y;

		m_currentDistance = distance.magnitude;
		m_desiredDistance = m_currentDistance;
		m_correctedDistance = m_currentDistance;

		if(m_alwaysRotateToRearOfTarget) {
			m_rotateBehind = true;
		}
	}

	void LateUpdate() {
		// auto move button pressed
		if(Input.GetButton("Toggle Move")) {
			m_mouseSideButton = !m_mouseSideButton;
		}

		// player moved or inturrpted the auto-move
		if(m_mouseSideButton && (Input.GetAxis("Vertical") != 0 || Input.GetButton("Jump")) || (Input.GetMouseButton(0) && Input.GetMouseButton(1))) {
			m_mouseSideButton = false;
		}

		// if either mousebuttons are down, let the mouse govern cameras position
		if(GUIUtility.hotControl == 0) {
			if(Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
				// check to see if the mouse input is allowed on the axis
				if(m_allowMouseInputX) {
					m_xDeg += Input.GetAxis("Mouse X") * m_xSpeed * 0.02f; // TODO: Fuck you magic numbers
				} else {
					RotateBehindTarget();
				}
				if(m_allowMouseInputY) {
					m_yDeg -= Input.GetAxis("Mouse Y") * m_ySpeed * 0.02f; // TODO: Fuck you magic numbers
				}

				if(!m_alwaysRotateToRearOfTarget) {
					m_rotateBehind = false;
				}
			} else if(Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0 || m_rotateBehind || m_mouseSideButton) {
				RotateBehindTarget();
			} 
		} // end of GUIUtility.hotControl check

		// ensure the cameras pitch is within our contraints
		m_yDeg = ClampAngle(m_yDeg, m_yMinLimit, m_yMaxLimit);
		// set the cameras rotation
		Quaternion rotation = Quaternion.Euler(m_yDeg, m_xDeg, 0);
		// calculate the desired distance
		m_desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * m_zoomSpeed * Mathf.Abs(m_desiredDistance);
		m_desiredDistance = Mathf.Clamp(m_desiredDistance, m_minDistance, m_maxDistance);
		m_correctedDistance = m_desiredDistance;

		// calculate desired camera position
		Vector3 targetOffset = new Vector3(0, -m_heightOffset, 0);
		Vector3 position = m_target.transform.position - (rotation * Vector3.forward * m_desiredDistance + targetOffset);

		// check for collision using the true targets desired registration point as set by height
		RaycastHit collisionHit;
		Vector3 trueTargetPosition = new Vector3(m_target.transform.position.x, m_target.transform.position.y + m_heightOffset, m_target.transform.position.z);

		// if there is a collision we can correct it
		bool isCorrected = false;
		if(Physics.Linecast(trueTargetPosition, position, out collisionHit, m_collisionLayers)) {
			// calculate the distance from the original estimated position to the collision
			// location, subtracting the safe "offset" distance from the object we hit. the
			// offset will keep the camera from being right on top of the surface we hit. which
			// usually shows up as the surface geometry getting partialy clipped by the cameras
			// near clip plane.
			m_correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - m_offsetFromWall;
			isCorrected = true;
		}

		if (!isCorrected || m_correctedDistance > m_currentDistance) {
			m_currentDistance = Mathf.Lerp(m_currentDistance, m_correctedDistance, Time.deltaTime * m_zoomSpeed);
		} else {
			m_currentDistance = m_correctedDistance;
		}

		// keep within the limits
		m_currentDistance = Mathf.Clamp(m_currentDistance, m_minDistance, m_maxDistance);

		// recalculate position based on current distance
		position = m_target.transform.position - (rotation * Vector3.forward * m_currentDistance + targetOffset);

		// finally set the rotation and position of the camera
		transform.rotation = rotation;
		transform.position = position;
	}

	private void RotateBehindTarget() {
		float targetRotationAngle = m_target.transform.eulerAngles.y;
		float currentRotationAngle = transform.eulerAngles.y;
		m_xDeg = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, m_autoRotationSpeed * Time.deltaTime);
		
		if(targetRotationAngle == currentRotationAngle) {
			if(!m_alwaysRotateToRearOfTarget) {
				m_rotateBehind = false;
			}
		} else {
			m_rotateBehind = true;
		}
	}

	/// <summary>
	/// ClampAngle - keeps the angle between 0 and 360 degrees
	/// </summary>
	/// <param name="angle">the angle we have</param>
	/// <param name="min">the min value</param>
	/// <param name="max">the max value</param>
	/// <returns>the angle between min and max</returns>

	private float ClampAngle(float angle, float min, float max) {
		if(angle < -360f) {
			angle += 360f;
		}

		if(angle > 360f) {
			angle -= 360f;
		}

		return Mathf.Clamp(angle, min, max);
	}
}