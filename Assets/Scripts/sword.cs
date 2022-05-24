using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sword : MonoBehaviour {

	void OnTriggerEnter (Collider other){
		if ((other.gameObject.tag == "Goblin" || other.gameObject.tag == "Ethereal") && other.gameObject.name != client.playerName)
			GameObject.Find ("Client").GetComponent<client> ().Send ("DESTROYIS|" + other.gameObject.name, 0);
	}

	void Update(){
		if (gameObject.GetComponentInParent<goblin> ().basicAttack)
			gameObject.GetComponent<CapsuleCollider> ().enabled = true;
		else
			gameObject.GetComponent<CapsuleCollider> ().enabled = false;
	}
}