using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Alteruna;

public class PlayerShoot : AttributesSync
{
    // Default health value for all players
    private const int DefaultHealth = 100;
    // Default damage applied when shooting
    private const int DefaultDamage = 10;
    // Force applied to ragdoll for natural effect
    private const float RagdollForce = 2f;

    // Player health synchronized across network
    [SynchronizableField] public int health = DefaultHealth;

    // Damage this player deals
    [SerializeField] private int damage = DefaultDamage;
    // Layer mask to identify targetable players
    [SerializeField] private LayerMask playerLayer;
    // Layer assigned to the local player
    [SerializeField] private int playerSelfLayer;

    // Reference to the player's Alteruna avatar
    public Alteruna.Avatar avatar;
    // Reference to multiplayer manager in scene
    private Multiplayer multiplayer;

    // True if this player is the Hunter
    private bool isHunter;
    // Tracks if this player has died to prevent further actions
    private bool isDead = false;


    private void Start()
    {
        // Find Multiplayer component in scene
        multiplayer = FindObjectOfType<Multiplayer>();
        if (multiplayer == null)
        {
            Debug.LogError("PlayerShoot requires a Multiplayer component in the scene!");
            return;
        }

        // Determine if this player is the Hunter (local player only)
        isHunter = avatar.IsMe && avatar.Multiplayer.Me.Index == 0;

        // Set layers recursively for this avatar and all children
        int targetLayer = avatar.IsMe ? playerSelfLayer : LayerMask.NameToLayer("Player");
        SetLayerRecursively(avatar.gameObject, targetLayer);

        // For non-Hunter players, check Body and Visor exist and have Rigidbody
        if (!IsLocalHunter())
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

    private void Update()
    {
        // Only allow the local Hunter to shoot and if not dead
        if (!avatar.IsMe || !isHunter || isDead) return;

        // Detect shooting input (left mouse button)
        if (Input.GetKeyDown(KeyCode.Mouse0))
            Shoot();
    }


    // Recursively sets the layer of a GameObject and all its children
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // Perform a raycast from the main camera to detect target players
    private void Shoot()
    {
        // Check if Camera.main exists
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("Camera.main not found. Cannot perform shooting.");
            return;
        }

        // Raycast forward from camera to detect target player
        if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward,
                            out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);

            // Attempt to get PlayerShoot component on hit object
            PlayerShoot target = hit.transform.root.GetComponentInChildren<PlayerShoot>();
            if (target != null)
            {
                // Broadcast damage to target over network
                target.BroadcastRemoteMethod(nameof(Hit), damage);
            }
            else
            {
                Debug.LogWarning("No PlayerShoot component found on hit object");
            }
        }
        else
        {
            Debug.Log("No raycast hit");
        }
    }

    // Called remotely to apply damage to this player
    [SynchronizableMethod]
    public void Hit(int damageTaken)
    {
        // Hunters cannot take damage
        if (IsLocalHunter()) return;

        // Validate damage (prevent negative values)
        damageTaken = Mathf.Max(0, damageTaken);

        // Apply damage
        health -= damageTaken;

        // If health drops below zero and player not dead, broadcast death
        if (health <= 0 && !isDead)
        {
            BroadcastRemoteMethod(nameof(Die));
        }
    }

    // Called remotely to handle player death
    [SynchronizableMethod]
    private void Die()
    {
        // If already dead or Hunter, do nothing
        if (isDead || IsLocalHunter()) return;

        // Mark as dead
        isDead = true;

        // Disable movement and player controls
        DisableControllers();

        // Enable ragdoll physics
        EnableRagdoll();

        // Switch local player to spectator camera
        EnterSpectatorMode();

        // Deactivate remaining player parts
        DeactivateRootChildren();
    }


    // Returns true if this player is the local Hunter
    private bool IsLocalHunter()
    {
        return avatar.Possessor.Index == 0;
    }

    // Disable CharacterController and PlayerController on root
    private void DisableControllers()
    {
        CharacterController cc = transform.parent.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        PlayerController pc = transform.parent.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;
    }

    // Enable ragdoll physics for Body and Visor
    private void EnableRagdoll()
    {
        Transform body = transform.parent.Find("Body");
        if (body == null) { Debug.LogError("Body not found!"); return; }

        Rigidbody bodyRb = body.GetComponent<Rigidbody>();
        if (bodyRb == null) { Debug.LogError("Body Rigidbody not found!"); return; }

        Transform visor = body.Find("Visor");
        Rigidbody visorRb = visor?.GetComponent<Rigidbody>();

        // Move HealthText to body so it follows ragdoll
        Transform healthText = transform.parent.Find("HealthText");
        if (healthText != null) healthText.SetParent(body);

        // Detach body from root to allow ragdoll movement
        body.SetParent(null);

        // Enable physics
        bodyRb.isKinematic = false;
        bodyRb.useGravity = true;
        if (visorRb != null)
        {
            visorRb.isKinematic = false;
            visorRb.useGravity = true;
        }

        // Apply force for natural ragdoll effect (host only)
        if (multiplayer.Me.Index == 0)
        {
            bodyRb.AddForce(Vector3.up * RagdollForce + UnityEngine.Random.insideUnitSphere * RagdollForce,
                            ForceMode.Impulse);
        }
    }

    // Switch the local player camera to spectator mode
    private void EnterSpectatorMode()
    {
        if (!avatar.IsMe) return;

        Camera cam = Camera.main;
        if (cam == null) { Debug.LogWarning("Camera.main not found for spectator mode."); return; }

        cam.transform.SetParent(null);

        // Add SpectatorCamera component if not already present
        SpectatorCamera specCam = cam.GetComponent<SpectatorCamera>();
        if (specCam == null)
        {
            cam.gameObject.AddComponent<SpectatorCamera>();
        }
    }

    // Deactivate all children of the root player except this script
    private void DeactivateRootChildren()
    {
        Transform root = transform.parent;
        foreach (Transform child in root)
        {
            if (child != transform) child.gameObject.SetActive(false);
        }
    }
}
