using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float mHorizontalMovementSpeed = 1;
	[SerializeField]
	private float mJumpSpeed = 3;
	[SerializeField]
	private float mCoyoteTime = 0.1f;

	[SerializeField]
	private float mGroundDistance = 0.1f;

	[SerializeField]
	private Camera mFollowCamera; // Camera to follow the player
	private Vector3 mCameraOffset;

	private Rigidbody2D mRigidBody;
	private Collider2D mCollider;

	private float mHorizontalAxis = 0;
	private float mJumpAxis = 0;
	private float mPreviousJumpAxis = 0;

	private bool mIsOnGround = false;
	private float mTimeSinceOnGround = 0;


	private void Start() {
		InitComponents();
	}

	private void InitComponents() {
		mRigidBody = GetComponent<Rigidbody2D>();
		mCollider = GetComponent<Collider2D>();

		if (mFollowCamera != null) {
			mCameraOffset = mFollowCamera.transform.position;
		}
	}

	private void Update() {
		UpdateValues();
		MovePlayer();
		MoveCamera();
	}

	// Get inputs from the user and update other values
	private void UpdateValues() {
		mHorizontalAxis = Input.GetAxisRaw("Horizontal");
		mPreviousJumpAxis = mJumpAxis;
		mJumpAxis = Input.GetAxisRaw("Jump");

		// Preform a downwards raycast to check if we are in close enough proximity to the ground
		RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, mGroundDistance + mCollider.bounds.extents.y);

		// Update if we are on the ground
		bool onGround = false;
		foreach (RaycastHit2D hit in hits) {
			if (hit.rigidbody != mRigidBody) onGround = true;
		}

		// Implement Coyote Time
		// Basically, you have a small time after leaving the ground in which you
		// are still counted as on the ground
		if (!onGround) {
			mTimeSinceOnGround += Time.deltaTime;
			if (mTimeSinceOnGround >= mCoyoteTime) {
				mIsOnGround = false;
			}
		} else {
			mTimeSinceOnGround = 0;
			mIsOnGround = true;
		}


	}

	// Move the player based on inputs
	private void MovePlayer() {

		// Can't move the player without a RigidBody
		if (mRigidBody == null) return;

		Vector2 newVelocity = mRigidBody.velocity;
		float newAngularVelocity = 0;

		if (mHorizontalAxis > 0) {
			// Moving to the right
			// Maintain vertical velocity, but change horizontal
			newVelocity.x = mHorizontalMovementSpeed;

			// Set the angular velocity (YAY MATH!)
			newAngularVelocity = -1 * Mathf.Pow(mCollider.bounds.extents.y, 2) * mHorizontalMovementSpeed * 180f;

		} else if (mHorizontalAxis < 0) {
			// Moving to the left
			newVelocity.x = -1 * mHorizontalMovementSpeed;
			newAngularVelocity = Mathf.Pow(mCollider.bounds.extents.y, 2) * mHorizontalMovementSpeed * 180f;

		} else {
			// Not pressing move, so remove velocity
			newVelocity.x = 0;
		}

		if (mJumpAxis > 0 && mPreviousJumpAxis == 0 && mIsOnGround) {
			// Player has re-pressed the jump button, and is on the ground
			newVelocity.y = mJumpSpeed;
		}

		mRigidBody.velocity = newVelocity;
		mRigidBody.angularVelocity = newAngularVelocity;
	}

	// Move the following camera to the player's position
	private void MoveCamera() {
		if (mFollowCamera != null)
			mFollowCamera.transform.position = transform.position + mCameraOffset;
	}
}
