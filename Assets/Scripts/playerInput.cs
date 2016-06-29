using UnityEngine;
using System.Collections;

public class playerInput : MonoBehaviour {

	public float jumpVelocity = 3f;
	public float dashSpeed = 1f;
	public float dashInterval = 0.25f;
	public float wallSlideSpeed = 0.1f;

	public Rigidbody2D body { get; private set; }
	public Animator anim { get; private set; }

	public Vector2 velocity = Vector2.zero;
	public float velX = 0f;
	private float lastJumped = 1f;
	public bool canJump = false;
	public bool grounded = false;
	public bool wallSliding = false;
	public bool goingRight = true;
	public float dashCooldown = 0f;

	void Start () {
		body = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}
	
	void Update () {
		velocity.y = body.velocity.y;
		if (wallSliding) {
			velX = 0.1f;
			velocity.y = -wallSlideSpeed;
		}
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
			if (wallSliding)
				turnAround();
		}
		// - Dash
		if (Input.GetKeyDown (goingRight ? KeyCode.RightArrow : KeyCode.LeftArrow) && dashCooldown < 0f) {
			dashCooldown = dashInterval;
			velX += dashSpeed;
			anim.SetTrigger("dash");
			canJump = true;
			if (wallSliding)
				turnAround();
		}
		// Update velocity
		velocity.x = goingRight ? velX : -velX;
		body.velocity = velocity;
		// Update animation variables
		anim.SetFloat ("velX", Mathf.Abs(velocity.x));
		anim.SetBool ("grounded", grounded);
	}

	private void turnAround() {
		goingRight = !goingRight;
		transform.localScale = new Vector3 ((goingRight ? 1f : -1f), 1f, 1f);
		wallSliding = false;
		anim.SetBool ("wallSlide", wallSliding);
	}

	private void handleCollision(Collision2D coll) {
		if (lastJumped > 0.01f) {
			if (!grounded || !wallSliding) {
				ContactPoint2D contact = coll.contacts [0];
				float contactFactor = Vector2.Dot(contact.normal, Vector2.up);
				Debug.Log ("Dot: " + contactFactor);
				if (contactFactor > 0.5f) {
					grounded = true;
					canJump = true;
					anim.SetBool ("grounded", grounded);
				} else if (contactFactor < 0.5f && contactFactor > -0.5f) {
					if ((goingRight && contact.point.x > transform.position.x) || (!goingRight && contact.point.x < transform.position.x)) {
						wallSliding = true;
						canJump = true;
						anim.SetBool ("wallSlide", wallSliding);
					}
				}
			}
			if (grounded && wallSliding) {
				turnAround();
			}
		}
	}
	
	void OnCollisionEnter2D (Collision2D coll) {
		handleCollision (coll);
	}
	void OnCollisionStay2D (Collision2D coll) {
		handleCollision (coll);
	}

	void OnCollisionExit2D (Collision2D coll) {
		if (body.velocity.y < -0.2f)
			grounded = false;
	}
}
