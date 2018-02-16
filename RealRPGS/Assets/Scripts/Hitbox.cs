using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

	public int m_normMinDamange = 3;
	public int m_normMaxDamage = 7;
	public int m_specialMinDamange = 10;
	public int m_specialMaxDamage = 25;
	public Animator m_animator;

	void OnTriggerEnter(Collider other) {
		// TODO: fix the sword so it works 
		Debug.Log(other.name);
		if(other.gameObject.tag == "Enemy") {
			Debug.Log("hit enemy");
			if(m_animator.GetBool("isAttacking")) {
				Debug.Log(m_animator.GetBool("isAttacking"));
				Health healthScript = other.GetComponent<Health>();
				if(healthScript != null) {
					if(m_animator.GetBool("swordAttack")) {
						healthScript.ApplyDamage(Random.Range(m_normMinDamange, m_normMaxDamage));
					}
					if(m_animator.GetBool("specialSwordAttack")) {
						healthScript.ApplyDamage(Random.Range(m_specialMinDamange, m_specialMaxDamage));
					}
					healthScript.SetAgroTarget(gameObject.transform);
				}
			}
		}
	}
}
