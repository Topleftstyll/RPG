using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour {

	public float m_maxHP = 100.0f;
	public float m_deathTime = 3.0f;
	public float m_hitReact = 0.1f;
	public Slider m_healthBar;
	public RPGCharacterController m_player;
	public Text m_hpText;
	public float m_perKillExperience = 100.0f;

	private float m_currentHealth;
	private Animator m_animController;
	private float m_hitDelay;
	private Transform m_agroTarget;
	private bool m_isDead = false;
	

	void Start() {
		m_animController = GetComponent<Animator>();
		m_currentHealth = m_maxHP;
		m_healthBar.value = CalculateHealth();
		if(m_hpText != null) {
			m_hpText.text = m_currentHealth + "/" + m_maxHP;
		}
	}

	void Update() {
		if(m_hitDelay <= 0) {
			m_animController.SetBool("tookDamage", false);
		} else {
			m_hitDelay -= Time.deltaTime;
		}

		if(m_currentHealth <= 0 && !m_isDead) {
			m_currentHealth = 0;
			if(m_hpText != null) {
				m_hpText.text = m_currentHealth + "/" + m_maxHP;
			}
			Die();
		}
	}

	public void ApplyDamage(int amount) {
		m_currentHealth -= amount;
		m_healthBar.value = CalculateHealth();
		if(m_hpText != null) {
			m_hpText.text = m_currentHealth + "/" + m_maxHP;
		}
		if(m_currentHealth <= 0) {
			m_hitDelay = m_deathTime;
			m_animController.SetBool("died", true);
		} else {
			m_hitDelay = m_hitReact;
			m_animController.SetBool("tookDamage", true);
		}
	}

	public void Heal(int amount) {
		m_currentHealth += amount;
		m_healthBar.value = CalculateHealth();
		if(m_currentHealth > m_maxHP) {
			m_currentHealth = m_maxHP;
		}
		if(m_hpText != null) {
			m_hpText.text = m_currentHealth + "/" + m_maxHP;
		}
	}

	public void Die() {
		m_isDead = true;
		Collider collider = GetComponent<Collider>();
		collider.enabled = false;
		m_animController.SetBool("isDead", true);
		if (this.gameObject.tag == "Enemy") {
			if(m_player.m_questAccepted) {
				m_player.UpdateQuest();
			}
			m_player.SetExperience(m_perKillExperience);
			m_player.CheckIfLevelUp();
		}
	}

	public void DestroyObject() {
		Destroy(this.gameObject);
	}

	public float CalculateHealth() {
		return m_currentHealth / m_maxHP;
	}

	public Transform GetAgroTarget() {
		return m_agroTarget ? m_agroTarget : null;
	}

	public void SetAgroTarget(Transform target) {
		m_agroTarget = target;
	}
}
