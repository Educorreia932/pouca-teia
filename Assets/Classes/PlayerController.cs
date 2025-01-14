﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public AudioClip eatenSound;
    public AudioClip jumpSound;

    [HideInInspector]
    public int isOnSurface = 0;

    public GameObject webOriginal;

    public LayerMask layerMask;

    [HideInInspector]
    public GameObject web;
    [SerializeField]
    private float jump;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator;

    private bool isWalking = false;
    private bool canJump = true;

    [HideInInspector]
    public bool startingWeb = false;
    [HideInInspector]
    public bool makingWeb = false;

    [HideInInspector]
    public Vector3 movement;
    public GameManager gameManager;


    [HideInInspector]
    public int points = 0;
    
    public void StartWeb(Vector3 StartPosition)
    {
        this.web.GetComponent<LineRenderer>().SetPosition(0, StartPosition);
    }

    public void EndWeb(Vector3 EndPosition)
    {
        this.web.GetComponent<LineRenderer>().SetPosition(1, EndPosition);
        if(!makingWeb)
        {
            this.web.GetComponent<Web>().AddColliderToLine();
        }
    }

    public void Eat()
    {
        this.audioSource.PlayOneShot(eatenSound);
        this.points++;
    }

    void Start()
    {
        this.audioSource = this.GetComponent<AudioSource>();
        this.animator = this.GetComponent<Animator>();
    }
    
    void Update()
    {
        if(!gameManager.roundStarted)
            return;


        if (this.isOnSurface > 1)
        {
            this.isWalking = false;
            this.movement = Vector3.zero;

            float horiInput = Input.GetAxisRaw("Horizontal");
            float vertInput = Input.GetAxisRaw("Vertical");

            if (!Mathf.Approximately(horiInput, 0) || !Mathf.Approximately(vertInput, 0))
            {
                this.gameObject.transform.up 
                    = new Vector3(horiInput * speed * Time.deltaTime, 
                                    vertInput * speed * Time.deltaTime, 
                                    0.0f);
            }

            // I had the bright idea of raycasting three times instead of making a collider
            // If you stumble on this code, learn from my stupidity and this better
            // - Miguel
            RaycastHit2D hit1 = Physics2D.Raycast(this.transform.position + this.transform.up * 0.5f + this.transform.transform.right * 0.1f, 
                                                this.transform.up, 
                                                0.5f, 
                                                this.layerMask);
            RaycastHit2D hit2 = Physics2D.Raycast(this.transform.position + this.transform.up * 0.5f - this.transform.transform.right * 0.1f, 
                                                this.transform.up, 
                                                0.5f, 
                                                this.layerMask);
            RaycastHit2D hit3 = Physics2D.Raycast(this.transform.position + this.transform.up * 0.5f, 
                                                this.transform.up, 
                                                0.5f, 
                                                this.layerMask);
            if (!Mathf.Approximately(horiInput, 0))
            {
                this.movement += new Vector3(horiInput * speed * Time.deltaTime, 0.0f, 0.0f);
                this.isWalking = true;
            }
            if (!Mathf.Approximately(vertInput, 0))
            {
                if ((hit1.collider != null || hit2.collider != null) && vertInput > 0.0f)
                    this.movement += new Vector3(0.0f, vertInput * speed * Time.deltaTime, 0.0f);
                else if (vertInput < 0.0f)
                    this.movement += new Vector3(0.0f, vertInput * speed * Time.deltaTime, 0.0f);
                this.isWalking = true;
            }
            if (Input.GetButton("Jump") && canJump)
            {
                canJump = false;
                this.isWalking = false;
                this.animator.ResetTrigger("Idle");
                this.animator.ResetTrigger("Walk");
                this.animator.SetTrigger("Jump");
                this.movement += new Vector3(0.0f, vertInput * speed * Time.deltaTime, 0.0f);
                this.GetComponent<Rigidbody2D>().velocity = this.movement * jump;
                this.audioSource.PlayOneShot(this.jumpSound);
                if(!makingWeb)
                {
                    this.startingWeb = true;
                    this.web = Instantiate(this.webOriginal);
                }


                StartCoroutine("CooldownJump");
            }

            if (this.isWalking)
            {
                this.animator.ResetTrigger("Idle");
                this.animator.SetTrigger("Walk");
            }
            else
            {
                this.animator.ResetTrigger("Walk");
                this.animator.SetTrigger("Idle");
            }

            if (this.isWalking)
            {
                this.gameObject.transform.position += this.movement;
            }

            /* this.gameObject.transform.up = this.movement; */

        }
        else
        {
            this.GetComponent<Rigidbody2D>().gravityScale = 1;
        }

        if (this.makingWeb)
        {
            this.EndWeb(this.gameObject.transform.GetChild(0).transform.position);
        }
    }

    IEnumerator CooldownJump()
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(0.5f);
        canJump = true;
    }
}
