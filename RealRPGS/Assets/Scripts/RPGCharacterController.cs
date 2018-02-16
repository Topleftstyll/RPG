using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RPGCharacterController : MonoBehaviour {

	public string m_moveStatus = "idle";
	public bool m_walkByDefault = true;
	public float m_gravity = 20.0f;

	// movement speeds
	public float m_jumpSpeed = 8.0f;
	public float m_runSpeed = 10.0f;
	public float m_walkSpeed = 4.0f;
	public float m_turnSpeed = 250.0f;
	public float m_moveBackwardsMultiplier = 0.75f;
	public Collider m_weaponHitBox;
	public float m_animTime = 1.0f;
	public GameObject m_fireBall;
	public GameObject m_fireballSpawn;
	public Health m_health;
	public int m_healAmount = 20;
	public Text m_fireballCDText;
	public Text m_healCDText;
	public Text m_specialSwordCDText;
	public Button m_fireballButton;
	public Button m_healButton;
	public Button m_specialSwordButton;
	public float m_fireballCD = 6.0f;
	public float m_healCD = 8.0f;
	public float m_specialSwordCD = 10.0f;
	public Canvas m_questWindow;
	public Text m_questTrackerText;
	public Text m_questContextText;
	public Text m_questButtonText;
	public bool m_questAccepted = false;
	public Slider m_experienceSlider;
	public Text m_experienceText;
	public float m_playerCurrentExperience = 0.0f;
	public float m_questExperience = 900.0f;
	public Text m_levelText;
	public float m_playerExperienceToLevel = 1000.0f;
	public Text m_levelUpText;
	public int m_questNumberTracker;
	public NPCQuestStatus m_npcScript;

	// internal variables
	private float m_speedMultiplier = 0.0f;
	private bool m_grounded = false;
	private Vector3 m_moveDirection = Vector3.zero;
	private bool m_isWalking = false;
	private bool m_jumping = false;
	private bool m_mouseSideDown = false;
	private CharacterController m_controller;
	private Animator m_animationController;
	private int m_attackState;
	private float m_animTimer;
	private bool m_usedFireAbility = false;
	private bool m_usedHealAbility = false;
	private bool m_usedSpecialSwordAbility = false;
	private Button m_buttonPressed;
	private Text m_textToUse;
	private int m_level = 1;
	private float m_levelUpTimer = 0.0f;

	void Awake() {
		// get the controllers
		m_controller = GetComponent<CharacterController>();
		m_animationController = GetComponent<Animator>();
		m_attackState = Animator.StringToHash("UpperTorso.attack");
		m_health = GetComponent<Health>();
		m_questWindow.enabled = false;
		m_questTrackerText.enabled = false;
		SetExperience(0.0f);
		m_levelText.text = "Lv. " + m_level.ToString();
		m_levelUpText.enabled = false;
		//m_weaponHitBox.enabled = false;
	}

	void Update() {
		m_moveStatus = "idle";
		m_isWalking = m_walkByDefault;

		// hold run to run
		if(Input.GetAxis("Run") != 0) {
			m_isWalking = !m_walkByDefault;
		}

		if(m_grounded) {
			// if the player is steering with the right mouse button ... A/D strafe
			if(Input.GetMouseButton(1)) {
				m_moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
			} else {
				m_moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
			}

			// auto move button pressed
			if(Input.GetButtonDown("Toggle Move")) {
				m_mouseSideDown = !m_mouseSideDown;
			}

			// player moved or otherwise interrupted auto-move
			if(m_mouseSideDown && (Input.GetAxis("Vertical") != 0 || Input.GetButton("Jump")) || (Input.GetMouseButton(0) && Input.GetMouseButton(1))) {
				m_mouseSideDown = false;
			}

			// left+right mouse button movement
			if((Input.GetMouseButton(0) && Input.GetMouseButton(1)) || m_mouseSideDown) {
				m_moveDirection.z = 1;
			}

			// if not strafing with right mouse and horizontal, check for strage keys
			if(!(Input.GetMouseButton(1) && Input.GetAxis("Horizontal") != 0)) {
				m_moveDirection.x -= Input.GetAxis("Strafing");
			}

			// if moving forward/backwards and sideways at the same time. compensate for distance
			if(((Input.GetMouseButton(1) && Input.GetAxis("Horizontal") != 0) ||Input.GetAxis("Strafing") != 0) && Input.GetAxis("Vertical") != 0){
				m_moveDirection *= 0.707f; // TODO: fuck you magic number
			}

			// apply the move backwards multiplier if not moving forwards only.
			if((Input.GetMouseButton(1) && Input.GetAxis("Horizontal") != 0) || Input.GetAxis("Strafing") != 0 || Input.GetAxis("Vertical") < 0) {
				m_speedMultiplier = m_moveBackwardsMultiplier;
			} else {
				m_speedMultiplier = 1.0f;
			}

			// use the run or the walk speed
			m_moveDirection *= m_isWalking ? m_walkSpeed * m_speedMultiplier : m_runSpeed * m_speedMultiplier;

			// jump
			if(Input.GetButton("Jump")) {
				m_jumping = true;
				m_moveDirection.y = m_jumpSpeed;
			}

			// tell the animator whats going on
			if(m_moveDirection.magnitude > 0.05f) { //TODO: fuck you magic numer
				m_animationController.SetBool("isWalking", true);
			} else {
				m_animationController.SetBool("isWalking", false);
			}

			m_animationController.SetFloat("Speed", m_moveDirection.z);
			m_animationController.SetFloat("Direction", m_moveDirection.x);

			// transform direction
			m_moveDirection = transform.TransformDirection(m_moveDirection);
		} // end if grounded

		// character must face the same direction as the camera when the right mouse button is down
		if(Input.GetMouseButton(1)) {
			transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
		} else {
			transform.Rotate(0, Input.GetAxis("Horizontal") * m_turnSpeed * Time.deltaTime, 0);
		}

		// apply gravity
		m_moveDirection.y -= m_gravity * Time.deltaTime;

		// move charactercontroller and check if grounded
		m_grounded = ((m_controller.Move(m_moveDirection * Time.deltaTime)) & CollisionFlags.Below) != 0;

		// reset jumping after grounded
		m_jumping = m_grounded ? false : m_jumping;

		if(m_jumping) {
			m_moveStatus = "jump";
		}

		// set the ability timer
		if(m_animationController.GetBool("isAttacking")) {
			m_animTimer += Time.deltaTime;
		}
		
		if(m_usedFireAbility) {
			m_fireballCD -= Time.deltaTime;		
			m_fireballCDText.text = Mathf.FloorToInt(m_fireballCD).ToString();
			if(m_fireballCD <= 0) {
				m_usedFireAbility = false;
				m_fireballCD = 6.0f;
				SetButtonColor(m_fireballButton, Color.white);
				m_fireballCDText.text = "";
			}
		}

		if(m_usedHealAbility) {
			m_healCD -= Time.deltaTime;		
			m_healCDText.text = Mathf.FloorToInt(m_healCD).ToString();
			if(m_healCD <= 0) {
				m_usedHealAbility = false;
				m_healCD = 8.0f;
				SetButtonColor(m_healButton, Color.white);
				m_healCDText.text = "";
			}
		}

		if(m_usedSpecialSwordAbility) {
			m_specialSwordCD -= Time.deltaTime;		
			m_specialSwordCDText.text = Mathf.FloorToInt(m_specialSwordCD).ToString();
			if(m_specialSwordCD <= 0) {
				m_usedSpecialSwordAbility = false;
				m_specialSwordCD = 10.0f;
				SetButtonColor(m_specialSwordButton, Color.white);
				m_specialSwordCDText.text = "";
			}
		}

		if(m_levelUpText.enabled == true) {
			m_levelUpTimer += Time.deltaTime;
			if(m_levelUpTimer > 3) {
				m_levelUpText.enabled = false;
				m_levelUpTimer = 0.0f;
			}
		}

		// is the player attacking?
		AnimatorStateInfo currentUpperTorseState = m_animationController.GetCurrentAnimatorStateInfo(1);
		if(currentUpperTorseState.fullPathHash == m_attackState) {
			//m_weaponHitBox.enabled = true;
		} else {
			if(Input.GetButtonDown("SwordAttack") && !m_animationController.GetBool("isAttacking")) {
				m_animTimer += .5f;
				//m_weaponHitBox.enabled = true;
				SetAttackAnim("swordAttack");
				setAllSpeedsZero();
			} else if(Input.GetButtonDown("MagicAttack") && !m_usedFireAbility && !m_animationController.GetBool("isAttacking")) {
				m_animTimer += .5f;
				SetAttackAnim("magicAttack");
				setAllSpeedsZero();
				SetButtonColor(m_fireballButton, Color.black);
				SetCooldownVariables(m_fireballCD, m_fireballCDText);
				m_usedFireAbility = true;
			} else if(Input.GetButtonDown("HealSelf") && !m_usedHealAbility && !m_animationController.GetBool("isAttacking")) {
				m_animTimer += .5f;
				//m_weaponHitBox.enabled = true;
				SetAttackAnim("healSelf");
				setAllSpeedsZero();
				SetButtonColor(m_healButton, Color.black);
				SetCooldownVariables(m_healCD, m_healCDText);
				m_usedHealAbility = true;
			} else if(Input.GetButtonDown("SpecialSwordAttack") && !m_usedSpecialSwordAbility && !m_animationController.GetBool("isAttacking")) {
				m_animTimer += .5f;
				//m_weaponHitBox.enabled = true;
				SetAttackAnim("specialSwordAttack");
				setAllSpeedsZero();
				SetButtonColor(m_specialSwordButton, Color.black);
				SetCooldownVariables(m_specialSwordCD, m_specialSwordCDText);
				m_usedSpecialSwordAbility = true;
			} else if (m_animTimer > m_animTime){
				ResetAnimEndVariables();
			}
		}
	}

	public void SetCooldownVariables(float timer, Text text) {
		text.text = timer.ToString();
	}

	public void ResetAnimEndVariables() {
		m_animationController.SetBool("isAttacking", false);
		m_animationController.SetBool("magicAttack", false);
		m_animationController.SetBool("swordAttack", false);
		m_animationController.SetBool("specialSwordAttack", false);
		m_animationController.SetBool("healSelf", false);
		m_animTimer = 0.0f;
		m_runSpeed = 10.0f;
		m_walkSpeed = 4.0f;
		//m_weaponHitBox.enabled = false;
	}

	public void SetButtonColor(Button button, Color color) {
		ColorBlock cb = button.colors;
       	cb.normalColor = color;
        button.colors = cb;
	}

	public void SetAttackAnim(string anim) {
		m_animationController.SetBool("isAttacking", true);
		m_animationController.SetBool(anim, true);
	}

	public void setAllSpeedsZero() {
		m_runSpeed = 0.0f;
		m_walkSpeed = 0.0f;
	}

	public void SpawnFireball() {
		Instantiate(m_fireBall, m_fireballSpawn.transform.position, transform.rotation);
	}

	public void healSelf() {
		m_health.Heal(m_healAmount);
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag == "NPC") {
			if(m_questNumberTracker == 8) {
				m_questContextText.text = "Wow you acutally killed 8 bandits? I sent you out there on a fools errand. Well, anyways heres your reward!";
				m_questWindow.enabled = true;
				m_questButtonText.text = "Complete Quest";
			} else if (!m_questAccepted) {
				m_questWindow.enabled = true;
			}
		}
	}

	public void ExitQuestWindow() {
		m_questWindow.enabled = false;
	}

	public void AcceptedQuest() {
		// completed quest
		if (m_questNumberTracker == 8) {
			m_questNumberTracker = 0;
			m_questTrackerText.enabled = false;
			m_questWindow.enabled = false;
			SetExperience(m_questExperience);
			CheckIfLevelUp();
			m_npcScript.m_questStatusImage.enabled = false;
		} else {
			//m_completedQuest = false;
			m_questAccepted = true;
			m_questWindow.enabled = false;
			m_questTrackerText.enabled = true;
		}
	}

	public void UpdateQuest() {
		m_questNumberTracker++;
		m_questTrackerText.text = "Enemies Killed: " + m_questNumberTracker.ToString() + "/8";
	}

	public float CalculateExperience() {
		return m_playerCurrentExperience / m_playerExperienceToLevel;
	}

	public void SetExperience(float experience) {
		m_playerCurrentExperience += experience;
		m_experienceSlider.value = CalculateExperience();
		m_experienceText.text = m_playerCurrentExperience + "/" + m_playerExperienceToLevel;
	}

	public void SetLevel() {
		m_level++;
		m_levelText.text = "Lv. " + m_level.ToString();
	}

	public void CheckIfLevelUp() {
		if(m_playerCurrentExperience >= m_playerExperienceToLevel) {
			float leftOverExperience = m_playerCurrentExperience - m_playerExperienceToLevel;
			m_playerCurrentExperience = 0;
			SetExperience(leftOverExperience);
			SetLevel();
			m_levelUpText.enabled = true;
		}
	}
}