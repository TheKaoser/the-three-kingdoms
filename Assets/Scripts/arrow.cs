using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class arrow : NetworkBehaviour {

	float arrowLifeTime = 2f;
	int isLive = 0;
	float age;

	void Update () {
		
		//If the arrow has been alive too long...
		age += Time.deltaTime;
		if (age > arrowLifeTime) {
			Destroy (gameObject);
		}
	}
		
	public void OnCollisionEnter(Collision other){

		//To only hit the first object
		isLive ++;
		if (isLive==1 || isLive>2)
			return;

		//If the arrow hits a player, tell the server to destroy it
		if ((other.gameObject.tag == "Goblin" || other.gameObject.tag == "Ethereal") && other.gameObject.name != client.playerName && gameObject.tag != "NotMyWeapon")
			GameObject.Find ("Client").GetComponent<client> ().Send ("DESTROYIS|" + other.gameObject.name, 0);
	}

}
