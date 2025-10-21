using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;

public class PlayerShoot : AttributesSync
{
    [SynchronizableField] public int health = 100;
    [SerializeField] private int damage = 10;
    public Alteruna.Avatar avatar;

    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int playerSelfLayer;

    private bool isHunter;
    private Multiplayer multiplayer;
    private bool isDead = false; // Track death state to prevent further input

    private void Start()
    {
        multiplayer = FindObjectOfType<Multiplayer>();
        if (multiplayer == null)
        {
            Debug.LogError("PlayerShoot requires a Multiplayer component in the scene!");
            return;
        }

        // Determine if this player is the Hunter (local only for shooting)
        isHunter = avatar.IsMe && avatar.Multiplayer.Me.Index == 0;

        // Set layer recursively on the entire avatar (root)
        int targetLayer = avatar.IsMe ? playerSelfLayer : LayerMask.NameToLayer("Player");
        SetLayerRecursively(avatar.gameObject, targetLayer);

        // Determine if this instance is for the Hunter player (based on possessor index)
        int playerIndex = avatar.Possessor.Index;
        bool isThisHunter = playerIndex == 0;

        // Ensure Body and Visor are set up correctly only for Hunted players
        if (!isThisHunter)
        {
            Transform body = transform.parent.Find("Body");
            if (body == null)
            {
                Debug.LogError($"Body GameObject not found in {avatar.gameObject.name}'s hierarchy!");
            }
            else
            {
                if (!body.GetComponent<Rigidbody>())
                    Debug.LogError($"Rigidbody missing on Body in {avatar.gameObject.name}!");
                Transform visor = body.Find("Visor");
                if (visor == null)
                    Debug.LogError($"Visor GameObject not found under Body in {avatar.gameObject.name}!");
                else if (!visor.GetComponent<Rigidbody>())
                    Debug.LogError($"Rigidbody missing on Visor in {avatar.gameObject.name}!");
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void Update()
    {
        if (!avatar.IsMe || !isHunter || isDead) return; // Only Hunter can shoot, and only if not dead

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Shoot();
    }

    void Shoot()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);
            PlayerShoot playerShoot = hit.transform.root.GetComponentInChildren<PlayerShoot>();
            if (playerShoot != null)
            {
                Debug.Log($"Found PlayerShoot on: {playerShoot.gameObject.name} | IsMe: {playerShoot.avatar.IsMe}");
                playerShoot.BroadcastRemoteMethod(nameof(Hit), damage);
            }
            else
            {
                Debug.Log("No PlayerShoot component found on hit object");
            }
        }
        else
        {
            Debug.Log("No raycast hit");
        }
    }

    [SynchronizableMethod]
    public void Hit(int damageTaken)
    {
        int playerIndex = avatar.Possessor.Index;
        bool isThisHunter = playerIndex == 0;

        if (isThisHunter) return; // Hunters cannot take damage or die

        Debug.Log($"Hit called on: {avatar.gameObject.name} | Current health: {health} | Damage: {damageTaken} | IsMe: {avatar.IsMe}");
        health -= damageTaken;
        if (health <= 0 && !isDead)
        {
            BroadcastRemoteMethod(nameof(Die));
        }
    }

    [SynchronizableMethod]
    void Die()
    {
        if (isDead) return; // Prevent re-running death logic

        int playerIndex = avatar.Possessor.Index;
        bool isThisHunter = playerIndex == 0;

        if (isThisHunter) return; // Hunters cannot die

        isDead = true;
        Debug.Log($"Player Died: {avatar.gameObject.name} | IsMe: {avatar.IsMe}");

        // Disable movement and controller on root
        CharacterController cc = transform.parent.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            Debug.Log($"CharacterController disabled on {avatar.gameObject.name}");
        }

        PlayerController pc = transform.parent.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.enabled = false;
            Debug.Log($"PlayerController disabled on {avatar.gameObject.name}");
        }

        // Find body and visor from root
        Transform body = transform.parent.Find("Body");
        if (body == null)
        {
            Debug.LogError($"Body not found for ragdoll in {avatar.gameObject.name}!");
            return;
        }

        Rigidbody bodyRb = body.GetComponent<Rigidbody>();
        if (bodyRb == null)
        {
            Debug.LogError($"Rigidbody not found on Body in {avatar.gameObject.name}!");
            return;
        }

        Transform visor = body.Find("Visor");
        Rigidbody visorRb = visor != null ? visor.GetComponent<Rigidbody>() : null;
        if (visor == null || visorRb == null)
        {
            Debug.LogError($"Visor or its Rigidbody not found in {avatar.gameObject.name}!");
        }

        // Move HealthText to Body so it follows the ragdoll
        Transform healthText = transform.parent.Find("HealthText");
        if (healthText != null)
        {
            healthText.SetParent(body);
            Debug.Log($"HealthText reparented to Body for {avatar.gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"HealthText not found in {avatar.gameObject.name}'s hierarchy!");
        }

        // Detach body from root so it can ragdoll independently
        body.SetParent(null);
        Debug.Log($"Body detached from {avatar.gameObject.name} for ragdoll");

        // Enable physics for ragdoll on all clients
        bodyRb.isKinematic = false;
        bodyRb.useGravity = true;
        if (visorRb != null)
        {
            visorRb.isKinematic = false;
            visorRb.useGravity = true;
        }

        // Host applies a small force for natural ragdoll
        if (multiplayer.Me.Index == 0)
        {
            bodyRb.AddForce(Vector3.up * 2f + UnityEngine.Random.insideUnitSphere * 2f, ForceMode.Impulse);
            Debug.Log($"Ragdoll physics force applied on host for {avatar.gameObject.name}");
        }

        // For local player: Enter spectator mode
        if (avatar.IsMe)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.transform.SetParent(null);
                SpectatorCamera specCam = cam.gameObject.GetComponent<SpectatorCamera>();
                if (specCam == null)
                {
                    specCam = cam.gameObject.AddComponent<SpectatorCamera>();
                    Debug.Log($"SpectatorCamera component added to {cam.gameObject.name}");
                }
                else
                {
                    Debug.Log($"SpectatorCamera already exists on {cam.gameObject.name}");
                }
            }
            else
            {
                Debug.LogError("Camera.main not found for spectator mode!");
            }
        }

        // Deactivate the root player object (hides any remaining non-ragdoll parts)
        // Keep PlayerShoot active by not deactivating the GameObject it's on
        Transform rootPlayer = transform.parent;
        foreach (Transform child in rootPlayer)
        {
            if (child != transform) // Skip PlayerShoot GameObject
            {
                child.gameObject.SetActive(false);
            }
        }
        Debug.Log($"Root player children (except PlayerShoot) deactivated for {avatar.gameObject.name}");
    }
}