using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightChange : MonoBehaviour {
	private Light light;
	private bool trigger = true;
	// Use this for initialization
	void Start () {
		light = GetComponent<Light> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (this.light.intensity >= 1)
			trigger = true;
		
		if (this.light.intensity == 0) 
			trigger = false;
		



		if (trigger == true) {
			this.light.intensity -= 0.005f;
		} if (trigger == false) {
			this.light.intensity += 0.005f;
		}
	}
}
