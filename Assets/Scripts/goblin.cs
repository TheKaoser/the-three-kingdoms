using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goblin : MonoBehaviour {

	public GameObject arrow;

	//Collisions
	CharacterController controller;
	public float gravity = -10;
	public float velocityY;

	//Smooth turning variables
	public float turnSmoothTime = 0.2f;
	float turnSmoothVelocity;

    //Goblin's shift ability
    public static bool shiftActivated = false;
	public float cdShift;
	bool flagShift = true;
	float shiftAnimationTime;

    //Goblin's E ability
    public static bool eActivated = false;
    public float cdE;
	bool flagE = true;
	float eAnimationTime;

	//Goblin's Mouse ability
    public static bool mouseActivated = false;
	public float cdMouse;
	bool flagMouse;
	float mouseAnimationTime;
	public bool basicAttack = false;

	public bool movement;

	Vector3 mPosition;
	Vector3 screenPos;

	private float enhance;

	// Use this for initialization
	void Start () {
		controller = GetComponent<CharacterController> ();
		movement = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (gameObject.tag == "Dead") {
			movement = false;
			cdMouse = 0;
			cdShift = 0;
			cdE = 0;
			basicAttack = false;
			return;
		}

		enhance = client.yourLevel / 100F;

		//Rotation of character
		Vector2 input = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		Vector2 inputDir = input.normalized;

		if (inputDir != Vector2.zero && shiftAnimationTime < 0 && eAnimationTime < 0 && mouseAnimationTime < 0) {
			float targetRotation = Mathf.Atan2 (inputDir.x, inputDir.y) * Mathf.Rad2Deg;
			transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle (transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, turnSmoothTime - (turnSmoothTime * enhance));
		}

		//Calculating speed
		float speed = 10 * inputDir.magnitude * (1F + enhance);

		//Possibility to cast habilities
		shiftAnimationTime -= Time.deltaTime;
		if (shiftAnimationTime > 0) {
			//GameObject.Find("Client").GetComponent<client>().SendOurPosition ();
			speed = 50;
		}

		eAnimationTime -= Time.deltaTime;
		if (eAnimationTime > 0)
			speed = 0;

		mouseAnimationTime -= Time.deltaTime;
		if (mouseAnimationTime > 0) {
			basicAttack = false;
			speed = 0;
		}
			
		//Shift goblin ability
		if(Input.GetKey(KeyCode.LeftShift) && flagShift == true && eAnimationTime < 0 && mouseAnimationTime < 0){

			//Turn arround character so it looks to the mouse pointer
			// Generate a plane that intersects the transform's position with an upwards normal.
			Plane playerPlane = new Plane(Vector3.up, transform.position);

			// Generate a ray from the cursor position
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// Determine the point where the cursor ray intersects the plane.
			// This will be the point that the object must look towards to be looking at the mouse.
			// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
			// then find the point along that ray that meets that distance.  This will be the point
			// to look at.
			float hitdist = 0.0f;
			// If the ray is parallel to the plane, Raycast will return false.
			if (playerPlane.Raycast (ray, out hitdist)) 
			{
				// Get the point along the ray that hits the calculated distance.
				Vector3 targetPoint = ray.GetPoint(hitdist);

				// Determine the target rotation.  This is the rotation if the transform looks at the target point.
				Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

				// Smoothly rotate towards the target point.
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1000 * Time.deltaTime);
			}

			GameObject.Find ("Client").GetComponent<client> ().Send ("ANIMATION|Turn_Over|" + transform.eulerAngles.y, 0);

            shiftActivated = true;
			cdShift = 3 - (3 * enhance);
			flagShift = false;
			shiftAnimationTime = 0.625f - (0.625f * enhance);
		}

		cdShift -= Time.deltaTime;
		if ( cdShift < 0 )
			flagShift = true;

		//E goblin ability
		if(Input.GetKeyDown("e") && flagE == true && (shiftAnimationTime - (0.15f - (0.15f * enhance))) < 0 && mouseAnimationTime < 0){

			//Turn arround character so it looks to the mouse pointer
			// Generate a plane that intersects the transform's position with an upwards normal.
			Plane playerPlane = new Plane(Vector3.up, transform.position);

			// Generate a ray from the cursor position
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// Determine the point where the cursor ray intersects the plane.
			// This will be the point that the object must look towards to be looking at the mouse.
			// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
			// then find the point along that ray that meets that distance.  This will be the point
			// to look at.
			float hitdist = 0.0f;
			// If the ray is parallel to the plane, Raycast will return false.
			if (playerPlane.Raycast (ray, out hitdist)) 
			{
				// Get the point along the ray that hits the calculated distance.
				Vector3 targetPoint = ray.GetPoint(hitdist);

				// Determine the target rotation.  This is the rotation if the transform looks at the target point.
				Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

				// Smoothly rotate towards the target point.
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1000 * Time.deltaTime);
			}

			GameObject.Find ("Client").GetComponent<client> ().Send ("ANIMATION|Shot|" + transform.eulerAngles.y, 0);

            eActivated = true;
			cdE = 3 - (3 * enhance);
			flagE = false;
			eAnimationTime = 1.2f - (1.2f * enhance);
		}

		cdE -= Time.deltaTime;
		if ( cdE < 0 )
			flagE = true;

		//Clic goblin ability
		if(Input.GetMouseButton(0) && flagMouse == true && (shiftAnimationTime - (0.15f - (0.15f * enhance))) < 0 && eAnimationTime < 0){

			//Turn arround character so it looks to the mouse pointer
			// Generate a plane that intersects the transform's position with an upwards normal.
			Plane playerPlane = new Plane(Vector3.up, transform.position);

			// Generate a ray from the cursor position
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			// Determine the point where the cursor ray intersects the plane.
			// This will be the point that the object must look towards to be looking at the mouse.
			// Raycasting to a Plane object only gives us a distance, so we'll have to take the distance,
			// then find the point along that ray that meets that distance.  This will be the point
			// to look at.
			float hitdist = 0.0f;
			// If the ray is parallel to the plane, Raycast will return false.
			if (playerPlane.Raycast (ray, out hitdist)) 
			{
				// Get the point along the ray that hits the calculated distance.
				Vector3 targetPoint = ray.GetPoint(hitdist);

				// Determine the target rotation.  This is the rotation if the transform looks at the target point.
				Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position);

				// Smoothly rotate towards the target point.
				transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1000 * Time.deltaTime);
			}

			basicAttack = true;
			GameObject.Find ("Client").GetComponent<client> ().Send ("ANIMATION|Sword_Strike|" + transform.eulerAngles.y, 0);

            mouseActivated = true;
			cdMouse = 1 - (1 * enhance);
			flagMouse = false;
			mouseAnimationTime = 0.667f - (0.667f * enhance);
		}

		cdMouse -= Time.deltaTime;
		if ( cdMouse < 0 )
			flagMouse = true;

		//Movement of character
		velocityY += Time.deltaTime * gravity;
		Vector3 velocity = transform.forward * speed + Vector3.up * velocityY;
		controller.Move (velocity * Time.deltaTime);
		if (controller.isGrounded)
			velocityY = 0;

		//System to activate the movement bool and send message just when you start or stop moving
		if (input.magnitude != 0 && movement != true) {
			GameObject.Find ("Client").GetComponent<client> ().Send ("MOVEMENT|true", 0);
			movement = true;
		} else if (inputDir.magnitude != 0) {
		} else if (inputDir.magnitude == 0 && movement != false){
			GameObject.Find ("Client").GetComponent<client> ().Send ("MOVEMENT|false", 0);
			movement = false;
		}
	}		
}