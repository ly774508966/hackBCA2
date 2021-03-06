﻿using UnityEngine;
using System.Collections;

public class CustomPlayerMovement : MonoBehaviour {

	OVRPlayerController controller;

	public GameObject orangeTerrain;
	public GameObject blueTerrain;

	public Camera leftEye, rightEye;
	Color orange = new Color(1,0.65f,0);
	Color blue = Color.cyan;

	bool blinked = false;
	public bool dead = false;

    public Transform Grabbable;
    public Renderer GrabbableRenderer;
    public Transform ForwardVec;
    public float GrabRange;
    private bool Grabbing = false;
    private Vector3 initialGrabPos;

	Vector3 startPos;
	Quaternion startRot;
	Vector3 deathPos;
	AudioSource audio;
	float musicStart = 0.0f;
	
	
    public Animator animator;
	void Start(){
		PlayerPrefs.SetFloat ("Music Time", 0);
	}
	// Use this for initialization
	void Awake () {
		startPos = transform.position;
		startRot = transform.rotation;
		controller = this.gameObject.GetComponent<OVRPlayerController>();

		orangeTerrain.SetActive (false);
		blueTerrain.SetActive (true);

        if(Grabbable != null)
            initialGrabPos = Grabbable.position;

		leftEye.backgroundColor = blue;
		rightEye.backgroundColor = blue;
		audio = this.gameObject.GetComponentInChildren<AudioSource>();
		audio.time = PlayerPrefs.GetFloat("Music Time");
	}
	
	// Update is called once per frame
	void Update () {
		if (!dead) {
			if (Input.GetAxis ("Jump") > 0) {
				controller.Jump ();
			}

            if (Grabbable != null)
            {
                bool InRange = (Grabbable.position - transform.position).sqrMagnitude < GrabRange * GrabRange;
                bool Facing = Vector3.Dot((Grabbable.position - transform.position).normalized, ForwardVec.forward) > 0.7;
                bool CanGrabOrb = InRange && Facing;
                Debug.Log(CanGrabOrb + "");
                if (!Grabbing && CanGrabOrb)
                {
                    Grabbing = true;
                }
                if (Grabbing)
                      Grabbable.transform.position = transform.position + transform.forward;
            }

			if (!blinked && Input.GetAxis ("Blink") > 0) {
				Blink ();
				blinked = true;
			} else if (Input.GetAxis ("Blink") == 0) {
				blinked = false;
			}
		} else {
			transform.position = deathPos;
			
			if (Input.GetAxis("Blink") > 0) {
				transform.position = startPos;
				transform.rotation = startRot;
				dead = false;
				controller.enabled = true;
				controller.HaltUpdateMovement = false;
				orangeTerrain.SetActive(false);
				blueTerrain.SetActive(true);
				Color switchTo = (orangeTerrain.activeSelf ? orange : blue);
				leftEye.backgroundColor = switchTo;
				rightEye.backgroundColor = switchTo;
				blinked = true;
				controller.MoveThrottle = new Vector3(0,0,0);
			}
		}
        animator.SetBool("Dead", dead);
	}

	void Blink() {
		orangeTerrain.SetActive (!orangeTerrain.activeSelf);
		blueTerrain.SetActive (!blueTerrain.activeSelf);

		Color switchTo = (orangeTerrain.activeSelf ? orange : blue);
		leftEye.backgroundColor = switchTo;
		rightEye.backgroundColor = switchTo;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.tag.Equals ("Deadly")) {
			if (!dead) {
				deathPos = this.transform.position;

				print ("test");
			}
			dead = true;
			print ("Dead");
            Grabbing = false;
            if(Grabbable != null)
                Grabbable.position = initialGrabPos;
			controller.HaltUpdateMovement = true;
			controller.enabled = false;
		}


		if (other.gameObject.tag.Equals ("Bouncy")) {
			controller.MoveThrottle = new Vector3(0,0,0);
			controller.FallSpeed = 0;
			controller.Bounce();
		}


		if (other.gameObject.tag.Equals ("Portal")) {
			PlayerPrefs.SetFloat("Music Time",audio.time);
			Application.LoadLevel((Application.loadedLevel+1)%Application.levelCount);

		}

	}


}
