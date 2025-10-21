using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayHealthText : MonoBehaviour
{
    [SerializeField] private TextMeshPro healthText;
    private PlayerShoot _playerShoot;

    private void Start()
    {
        // Find PlayerShoot in parent or siblings
        _playerShoot = GetComponentInParent<PlayerShoot>();
        if (_playerShoot == null)
        {
            _playerShoot = transform.parent.GetComponentInChildren<PlayerShoot>();
        }
        if (_playerShoot == null)
        {
            Debug.LogError($"PlayerShoot component not found for {gameObject.name}!");
        }
        if (healthText == null)
        {
            Debug.LogError($"TextMeshPro component not assigned on {gameObject.name}!");
        }
    }

    private void Update()
    {
        if (_playerShoot == null || healthText == null) return;

        int currentHealth = _playerShoot.health;
        healthText.text = currentHealth <= 0 ? "Dead" : currentHealth.ToString();
        Debug.Log($"HealthText Update for {gameObject.name}: health={currentHealth}, text={healthText.text}");
    }
}