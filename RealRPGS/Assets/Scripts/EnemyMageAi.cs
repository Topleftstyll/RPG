using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMageAi : MonoBehaviour {

	public GameObject m_fireBall;
	public GameObject m_fireballSpawn;
	public GameObject m_target;
    public int m_maxRange;
	public int m_sightRange;
    private Vector3 m_targetTran;
	private Animator m_animationController;
	private Vector3 m_startPosition;
	private bool m_isAtStartPosition = true;

    void Start() {
    	m_target = GameObject.FindWithTag("Player");
		m_animationController = GetComponent<Animator>();
		m_startPosition = transform.position;
    }

    void Update() {
		m_targetTran = m_target.transform.position;
		if(Vector3.Distance(transform.position, m_target.transform.position) >= m_maxRange) {
			if(!(Vector3.Distance(transform.position, m_target.transform.position) >= m_sightRange)) {
				m_isAtStartPosition = false;
				transform.LookAt(m_targetTran);
				transform.Translate(Vector3.forward * Time.deltaTime);
				m_animationController.SetBool("isAttacking", false);
				m_animationController.SetBool("magicAttack", false);
				m_animationController.SetBool("isRunning", true);
			} else {
				if(!m_isAtStartPosition) {
					transform.LookAt(m_startPosition);
					m_startPosition.y = 0;
					transform.position = Vector3.MoveTowards(transform.position, m_startPosition, Time.deltaTime);
					if(transform.position == m_startPosition) {
						m_isAtStartPosition = true;
						m_animationController.SetBool("isRunning", false);
					}
				}
			}
		}
        if (Vector3.Distance(transform.position, m_target.transform.position) <= m_maxRange) {
			transform.LookAt(m_targetTran);
			m_animationController.SetBool("isRunning", false);
			m_animationController.SetBool("isAttacking", true);
			m_animationController.SetBool("magicAttack", true);
		}
    }

	public void SpawnFireball() {
		Instantiate(m_fireBall, m_fireballSpawn.transform.position, transform.rotation);
	}
}