using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NPCQuestStatus : MonoBehaviour {

	public Image m_questStatusImage;
	public RPGCharacterController m_player;

	void Start() {
		m_questStatusImage.color = Color.yellow;
	}

	void Update() {
		if(m_player.m_questAccepted) {
			m_questStatusImage.color = Color.red;
			if(m_player.m_questNumberTracker == 8) {
				m_questStatusImage.color = Color.green;
			}
		}
	}
}
