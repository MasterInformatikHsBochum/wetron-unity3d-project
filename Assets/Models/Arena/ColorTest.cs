using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour {
	public Material changeMat;
	public Color pColor;
	void Start () {
		pColor = Color.blue;

		foreach (TrailRenderer comp in GetComponents<TrailRenderer>()) {
		//	comp.colorGradient.SetKeys(grdKey, grdAlpha);
			comp.startColor = pColor;
			comp.endColor = pColor;
		}

		foreach (Renderer comp in GetComponentsInChildren<Renderer>()) {
			//Debug.Log (comp.material.name);
			if (comp.sharedMaterial == changeMat) {
				comp.material.color = pColor;
				//comp.material.color = new Color(1.0f, 0.982f, 0.982f, 1.0f);
			}
		}

	}

	// Update is called once per frame
	void Update () {
		
	}
}
