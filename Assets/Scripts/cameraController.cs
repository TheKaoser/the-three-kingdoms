using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour {

	public GameObject character;
	private Vector3 offset;

	float yaw;
	float pitch;

	// Use this for initialization
	void Start () {
		character = GameObject.Find(client.getNameAvatar ());
		offset = transform.position - character.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

		if (character == null)
			return;

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

		transform.position = character.transform.position + offset + new Vector3 (yaw, 0, pitch);
	}
}