using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class orb : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		GetComponent<Light> ().color = Random.ColorHSV (0.5f, 0.6f, 0.6f, 0.6f, 0.4f, 0.4f);
		GetComponent<Light> ().range = Random.Range (2f, 3f);
		GetComponent<Light> ().intensity = Random.Range (3f, 4f);
	}

	public void OnCollisionEnter(Collision other){

		//To only hit the first object
		if (client.players [GameObject.Find ("Client").GetComponent<client> ().ourClientId].avatar.GetComponent<wizard> ().hasOrb)
			return;

		//If the orb hits a player, tell the server to destroy it
		if ((other.gameObject.tag == "Goblin" || other.gameObject.tag == "Ethereal") && other.gameObject.name != client.playerName && gameObject.tag != "NotMyWeapon")
			GameObject.Find ("Client").GetComponent<client> ().Send ("DESTROYIS|" + other.gameObject.name, 0);
	}
}
