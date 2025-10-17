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
        if (avatar.IsMe)
            avatar.gameObject.layer = playerSelfLayer;
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
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit,
                Mathf.Infinity, playerLayer))
        {
            PlayerShoot playerShoot = hit.transform.GetComponentInChildren<PlayerShoot>();
            playerShoot.Hit(damage);
        }
    }

    public void Hit(int damageTaken)
    {
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
