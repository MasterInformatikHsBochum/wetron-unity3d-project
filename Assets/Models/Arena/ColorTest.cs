using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorTest : MonoBehaviour {
	public Material changeMat;
	public Color[] pColor;
	void Start () {
		pColor [0] = Color.blue;
		pColor [1] = Color.red;
		pColor [2] = Color.cyan;
		pColor [3] = Color.green;
		pColor [4] = Color.yellow;
		pColor [5] = Color.magenta;
	}

	public void calcColorPlayer(GameObject model, int playerID){
		int i = playerID % 6;
		foreach (TrailRenderer comp in model.GetComponents<TrailRenderer>()) {
			//	comp.colorGradient.SetKeys(grdKey, grdAlpha);
			comp.startColor = pColor[i];
			comp.endColor = pColor[i];
		}

		foreach (Renderer comp in model.GetComponentsInChildren<Renderer>()) {
			//Debug.Log (comp.material.name);
			if (comp.sharedMaterial == changeMat) {
				comp.material.color = pColor[i];
				//comp.material.color = new Color(1.0f, 0.982f, 0.982f, 1.0f);
			}
		}

	}
	// Update is called once per frame
	void Update () {
		
	}
}
