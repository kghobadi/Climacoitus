using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCam : MonoBehaviour {

    Transform cammy;

	void Start () {
        cammy = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(cammy);
	}
}
