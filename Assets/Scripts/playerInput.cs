using UnityEngine;
using System.Collections;

public class playerInput : MonoBehaviour {

	public float maxVelocityX = 15f;
	public float minVelocityX = 6f;
	public float jumpVelocity = 3f;
	public float dashSpeed = 1f;
	public float dashInterval = 0.25f;
	public float wallSlideSpeed = 0.1f;
	public float wallSlideSpeedLoss = 1f;
	public float chargeSpeedGain = 0.2f;
	public float chargeFriction = 0.4f;
	public float smashTime = 1f;

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
	public float smashCooldown = 0f;
	public bool charging = false;

	void Start () {
		body = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}
	
	void Update () {
		velocity.y = body.velocity.y;
		if (wallSliding) {
			velocity.y = -wallSlideSpeed;
		}
		// Update
		lastJumped += Time.deltaTime;
		dashCooldown  = Mathf.Clamp (dashCooldown  - Time.deltaTime, -1, dashInterval);
		smashCooldown = Mathf.Clamp (smashCooldown - Time.deltaTime, -1, smashTime);
		// Handle player input
		if (smashCooldown < 0f) {
			// - Jump
			if (canJump && Input.GetKeyDown (KeyCode.UpArrow)) {
				lastJumped = 0f;
				grounded = false;
				canJump = false;
				velocity.y = jumpVelocity;
				anim.SetTrigger("jump");
				if (velX < minVelocityX) velX = minVelocityX;
				if (wallSliding) {
					turnAround();
				}
			}
			// - Dash
			if (dashCooldown < 0f && Input.GetKeyDown (goingRight ? KeyCode.RightArrow : KeyCode.LeftArrow)) {
				dashCooldown = dashInterval;
				velX += dashSpeed;
				anim.SetTrigger("dash");
				canJump = true;
				if (velX < minVelocityX) velX = minVelocityX;
				if (wallSliding) {
					turnAround();
				}
			}
			// - Charge
			if (dashCooldown < 0f && Input.GetKey (goingRight ? KeyCode.LeftArrow : KeyCode.RightArrow)) {
				charging = true;
				anim.SetBool ("charge", charging);
				wallSliding = false;
				anim.SetBool ("wallSlide", wallSliding);
				velX += Time.deltaTime * chargeSpeedGain;
			} else if (charging) {
				smashCooldown = smashTime;
				charging = false;
				anim.SetBool ("charge", charging);
				if (velX < minVelocityX) velX = minVelocityX;
			}
		}
		// Update velocity
		if (wallSliding) {
			velX = Mathf.Clamp(velX - Time.deltaTime * wallSlideSpeedLoss, 0f, maxVelocityX);
			dashCooldown = 0f;
			smashCooldown = 0f;
		}
		if (velX > maxVelocityX)
			velX = maxVelocityX;
		velocity.x = goingRight ? velX : -velX;
		if (charging) {
			velocity.x *= chargeFriction;
		}
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
		if (lastJumped > 0.05f && smashCooldown < 0f) {
			if (!grounded || !wallSliding) {
				foreach (ContactPoint2D contact in coll.contacts) {
					float contactFactor = Vector2.Dot(contact.normal, Vector2.up);
					Debug.Log ("Dot: " + contactFactor);
					if (contactFactor > 0.5f && body.velocity.y <= 0f) {
						grounded = true;
						canJump = true;
						anim.SetBool ("grounded", grounded);
					} else if (contactFactor < 0.5f && contactFactor > -0.5f && !charging) {
						if ((goingRight && contact.point.x > transform.position.x) || (!goingRight && contact.point.x < transform.position.x)) {
							wallSliding = true;
							canJump = true;
							anim.SetBool ("wallSlide", wallSliding);
						}
					}
				}
			}
			if (grounded && wallSliding) {
				if (velX < minVelocityX) velX = minVelocityX;
				turnAround();
			}
		}
	}
	
	void OnCollisionEnter2D (Collision2D coll) {
		handleCollision (coll);
	}
	void OnCollisionStay2D (Collision2D coll) { // FIXME NOT WORKING
		handleCollision (coll);
	}

	void OnCollisionExit2D (Collision2D coll) {
		if (body.velocity.y < -0.2f)
			grounded = false;
	}
}
