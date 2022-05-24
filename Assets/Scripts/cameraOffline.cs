using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraOffline : MonoBehaviour {

	public GameObject character;
	private Vector3 offset;
	private Transform target;
	private static Vector3 mousePosition;

	float yaw;
	float pitch;

	// Use this for initialization
	void Start () {
		offset = transform.position - character.transform.position;
	}

	// Update is called once per frame
	void Update () {

		yaw += Input.GetAxis ("Mouse X") / 2;
		pitch += Input.GetAxis ("Mouse Y") / 2;

		if (yaw > 10)
			yaw = 10;
		else if (yaw < -10)
			yaw = -10;

		if (pitch > 10)
			pitch = 10;
		else if (pitch < -10)
			pitch = -10;

		mousePosition = new Vector3 (yaw, 0, pitch);
		transform.position = character.transform.position + offset + new Vector3 (yaw, 0, pitch);


		if(Input.GetKey(KeyCode.Space)) {
			transform.position = character.transform.position - transform.forward * 100;
		}

	}


	public static Vector3 mousePos (){
		return (mousePosition);
	}
}
