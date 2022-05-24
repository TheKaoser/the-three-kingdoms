using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nameTag : MonoBehaviour {

	public GameObject player;
	
	// Update is called once per frame
	void Update () {
        if (player == null) {
            Destroy(gameObject);
        }
        else {
            transform.position = player.transform.position + new Vector3(0, 10, 0);
            transform.eulerAngles = new Vector3 (50, 0, 0);
        }
	}
}
