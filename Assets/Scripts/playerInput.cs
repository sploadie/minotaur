using UnityEngine;
using System.Collections;

public class playerInput : MonoBehaviour {

	public float jumpVelocity = 3f;
	public float dashSpeed = 1f;
	public float dashInterval = 0.25f;

	public Rigidbody2D body { get; private set; }
	public Animator anim { get; private set; }

	public Vector2 velocity = Vector2.zero;
	private float lastJumped = 1f;
	public bool canJump = false;
	public bool grounded = false;
	public bool goingRight = true;
	public float dashCooldown = 0f;

	void Start () {
		body = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}
	
	void Update () {
		velocity.y = body.velocity.y;
		// Update
		lastJumped += Time.deltaTime;
		dashCooldown -= Time.deltaTime;
		// Handle player input
		// - Jump
		if (Input.GetKeyDown (KeyCode.UpArrow) && canJump) {
			lastJumped = 0f;
			grounded = false;
			canJump = false;
			velocity.y = jumpVelocity;
			anim.SetTrigger("jump");
		}
		// - Dash
		if (Input.GetKeyDown (goingRight ? KeyCode.RightArrow : KeyCode.LeftArrow) && dashCooldown < 0f) {
			dashCooldown = dashInterval;
			velocity.x += dashSpeed;
			if (velocity.x < 1f)
				velocity.x = 1f;
			anim.SetTrigger("dash");
//			anim.Play("somaDash");
			canJump = true;
		}
		// Update velocity
		body.velocity = velocity;
		// Update animation variables
		anim.SetFloat ("velX", Mathf.Abs(velocity.x));
		anim.SetBool ("grounded", grounded);
	}

	private void checkGrounded(Collision2D coll) {
		if (!grounded && lastJumped > 0.1f) {
			Debug.Log ("Dot: " + Vector2.Dot(coll.contacts [0].normal, Vector2.up));
			if (Vector2.Dot(coll.contacts [0].normal, Vector2.up) > 0.5f) {
				grounded = true;
				canJump = true;
				anim.SetBool ("grounded", grounded);
			}
		}
	}
	
	void OnCollisionEnter2D (Collision2D coll) {
		checkGrounded (coll);
	}
	void OnCollisionStay2D (Collision2D coll) {
		checkGrounded (coll);
	}

	void OnCollisionExit2D (Collision2D coll) {
		if (body.velocity.y < -0.2f)
			grounded = false;
	}
}
