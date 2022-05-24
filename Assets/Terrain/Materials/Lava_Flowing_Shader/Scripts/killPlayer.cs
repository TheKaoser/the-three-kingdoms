using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killPlayer : MonoBehaviour {

	void OnTriggerEnter (Collider other){
		if ((other.gameObject.tag == "Goblin" || other.gameObject.tag == "Ethereal"))
			GameObject.Find ("Client").GetComponent<client> ().Send ("DESTROYIS|" + other.gameObject.name, 0);
	}
}