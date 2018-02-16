using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public float m_bulletForce = 10.0f;
	public Rigidbody m_rb;
	public int m_minDamange = 5;
	public int m_maxDamage = 10;

	private float m_destroyThisTimer = 2.5f;

	void Start() {
		if(m_rb == null) {
			m_rb = GetComponent<Rigidbody>();
		}
		Destroy(this.gameObject, m_destroyThisTimer);
	}

	void FixedUpdate() {
		m_rb.AddForce(transform.forward * m_bulletForce);
	}

	void OnTriggerEnter(Collider other) {
		if(other.gameObject.tag == "Enemy") {
			Health healthScript = other.GetComponent<Health>();
			if(healthScript != null) {
				healthScript.ApplyDamage(Random.Range(m_minDamange, m_maxDamage));
				healthScript.SetAgroTarget(gameObject.transform);
			}
		}
		if(!(other.gameObject.tag == "Projectile") && !(other.gameObject.tag == "NPC")) {
			Destroy(this.gameObject);
		}
	}
}
