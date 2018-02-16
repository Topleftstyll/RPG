using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour {

	public Transform m_target;

	void LateUpdate() {
		Vector3 newPosition = m_target.position;
		newPosition.y = transform.position.y;
		transform.position = newPosition;
	}
}
