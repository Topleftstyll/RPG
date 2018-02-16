using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowTarget : MonoBehaviour {

	public Transform target;
	private NavMeshAgent agent;

	// use this for initialization
	void Start() {
		agent = GetComponent<NavMeshAgent>();
	}

	void Update() {
		agent.SetDestination(target.position);
	}
}
