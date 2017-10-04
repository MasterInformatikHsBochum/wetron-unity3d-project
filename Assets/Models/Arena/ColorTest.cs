using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour {
	public Material changeMat;
	private Color color;
	// Use this for initialization
	void Start () {
		foreach (Renderer comp in GetComponentsInChildren<Renderer>()) {
			//Debug.Log (comp.material.name);
			if (comp.sharedMaterial == changeMat) {
				Debug.Log ("true :" + comp);
				comp.material.color = Color.blue;
			}
		}
		foreach (TrailRenderer comp in GetComponents<TrailRenderer>()) {
			Debug.Log ("comp : " + comp.ToString ());

			for(int i = 0; i < comp.colorGradient.colorKeys.Length; i++ ) {
				Debug.Log ("First color :" + comp.colorGradient.colorKeys [i].color);
				comp.colorGradient.colorKeys[i].color = Color.blue;
				Debug.Log ("Second color :" + comp.colorGradient.colorKeys [i].color);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
