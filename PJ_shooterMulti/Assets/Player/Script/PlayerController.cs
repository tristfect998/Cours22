﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;
    //Rigidbody rigidBody;
    [SyncVar]
    Vector3 Velocity = Vector3.zero;
    public const float INITIALJUMPVELOCITY = 10;
    public const float GRAVITYSTRENGHT = -15f;
    public const float MOVESPEED = 5;

    private bool hasControl = true;
    private bool isAlive = true;
    public GameObject deathParticleSystemObject;
    public GameObject mesh;

    private Camera mycam;
    private AudioListener audioListener;
    private PlayerRespawner playerRespawner;

    private Oscillator registeredContactOscillator;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    public override void OnStartServer()
    {
        playerRespawner = FindObjectOfType<PlayerRespawner>();
    }

    public override void OnStartLocalPlayer()
    {
        mycam = GetComponentInChildren<Camera>();
        audioListener = GetComponentInChildren<AudioListener>();
        mycam.enabled = true;
        audioListener.enabled = true;
    }

    void Update()
    {
        if (isLocalPlayer && hasControl)
        {
            ProcessGravity();
            ProcessMovementInput();
        }
        Move();
    }

    private void ProcessGravity()
    {
        if (!characterController.isGrounded)
        {
            Velocity.y += GRAVITYSTRENGHT * Time.deltaTime;
        }
        else
        {
            if (Velocity.y < 0)
            {
                Velocity.y = 0;
            }
        }
    }

    private Vector3 CalculatedMovementVector()
    {
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        Vector3 mouvement = new Vector3(horizontal, 0, vertical);
        Vector3 ajustedMovement = transform.TransformDirection(mouvement);
        ajustedMovement = ajustedMovement.normalized * MOVESPEED;
        if (registeredContactOscillator != null)
        {
            ajustedMovement += registeredContactOscillator.GetVelocity();
        }
        return ajustedMovement;
    }

    private void ProcessMovementInput()
    {
        Vector3 velocityMovementVector = CalculatedMovementVector();
        Velocity.x = velocityMovementVector.x;
        Velocity.z = velocityMovementVector.z;
        if (Input.GetAxis("Jump") > 0)
        {
            if (characterController.isGrounded)
            {
                Velocity.y = INITIALJUMPVELOCITY;
            }
        }
    }

    private void Move()
    {
        //  rigidbody.
        if (hasControl)
        {
            characterController.Move(Velocity * Time.deltaTime);
        }

    }

    public bool IsAlive()
    {
        return isAlive;
    }

    [ClientRpc]
    public void RpcDie()
    {
        hasControl = false;
        characterController.enabled = false;
        Instantiate(deathParticleSystemObject, gameObject.transform.position, gameObject.transform.rotation);
        isAlive = false;
        mesh.SetActive(false);
        playerRespawner = FindObjectOfType<PlayerRespawner>();
        if(playerRespawner != null)
            playerRespawner.CmdRespawn(gameObject);
    }

    [ClientRpc]
    public void RpcRevive(GameObject spawn)
    {
        gameObject.transform.position = spawn.transform.position;
        gameObject.transform.rotation = spawn.transform.rotation;
        hasControl = true;
        characterController.enabled = true;
        mesh.SetActive(true);
        isAlive = true;
    }
}
