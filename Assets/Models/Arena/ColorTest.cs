using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour {
	public Material changeMat;
	// Use this for initialization
	void Start () {
		foreach (Renderer comp in GetComponentsInChildren<Renderer>()) {
			//Debug.Log (comp.material.name);
			if (comp.sharedMaterial == changeMat ) {
				Debug.Log ("true :" + comp);
				comp.material.color = Color.blue;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
