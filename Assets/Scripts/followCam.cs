using UnityEngine;
using System.Collections;

public class followCam : MonoBehaviour {

	public Transform focus;
	public Vector3 position;
	public float zAxis = -10f;

	// Use this for initialization
	void Start () {
		updatePosition ();
	}
	
	// Update is called once per frame
	void Update () {
		updatePosition ();
	}

	private void updatePosition() {
		position = focus.position;
		position.z = zAxis;
		transform.position = position;
	}
}
