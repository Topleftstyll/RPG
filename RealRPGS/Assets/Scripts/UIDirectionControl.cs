using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDirectionControl : MonoBehaviour {

	public bool m_useRelativeRotation = true;

	private Quaternion m_relativeRotation;

	void start() {
		m_relativeRotation = transform.parent.localRotation;
	}

	void Update() {
		if(m_useRelativeRotation) {
			transform.rotation = m_relativeRotation;
		}
	}
}
