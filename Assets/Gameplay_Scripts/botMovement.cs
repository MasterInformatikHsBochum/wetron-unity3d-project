using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class botMovement : MonoBehaviour {
	private int speed = 15;
	private float time = 5;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Translate (Vector3.forward * Time.deltaTime * speed);

		time -= Time.deltaTime;

		if (time < 0) {
			transform.rotation = transform.rotation * Quaternion.Euler (1, 90, 1);
			time = 5;
		}
	}
}
