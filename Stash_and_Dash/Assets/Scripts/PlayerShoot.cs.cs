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

    private void Start()
    {
        // Set layer recursively on root (avatar.gameObject) and all children
        int targetLayer = avatar.IsMe ? playerSelfLayer : LayerMask.NameToLayer("Player");
        SetLayerRecursively(avatar.gameObject, targetLayer);
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
        if (!avatar.IsMe)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
            Shoot();
    }

    void Shoot()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, Mathf.Infinity, playerLayer))
        {
            Debug.Log("Raycast hit: " + hit.transform.name);  // Check if raycast detects anything
            // Get root (Player), then find PlayerShoot script on its children
            PlayerShoot playerShoot = hit.transform.root.GetComponentInChildren<PlayerShoot>();
            if (playerShoot != null)
            {
                Debug.Log("Found PlayerShoot on: " + playerShoot.gameObject.name + " | IsMe: " + playerShoot.avatar.IsMe);
                // Call remotely for network sync instead of direct
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

    [SynchronizableMethod]  // Makes this callable over network
    public void Hit(int damageTaken)
    {
        Debug.Log("Hit called on: " + gameObject.name + " | Current health: " + health + " | Damage: " + damageTaken + " | IsMe: " + avatar.IsMe);
        health -= damageTaken;
        if (health <= 0)
        {
            BroadcastRemoteMethod("Die");
        }
    }

    [SynchronizableMethod]
    void Die()
    {
        Debug.Log("Player Died");
    }
}