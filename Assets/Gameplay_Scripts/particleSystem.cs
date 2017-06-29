using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleSystem : MonoBehaviour {
	private ParticleSystem ps;
	private List<ParticleCollisionEvent> collisionEvents;
	// Use this for initialization
	void Start () {
		ps = GetComponent<ParticleSystem>();
		var main = ps.main;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	void OnParticleCollision(GameObject other){
		int numCol = ps.GetCollisionEvents (other, collisionEvents);

		Rigidbody rb = other.GetComponent<Rigidbody> ();

		int i = 0;

		while (i< numCol) {

			if(rb)
			{
				Debug.Log ("COLLISION!!!" + Time.realtimeSinceStartup);
			}

			i++;
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
		
}
