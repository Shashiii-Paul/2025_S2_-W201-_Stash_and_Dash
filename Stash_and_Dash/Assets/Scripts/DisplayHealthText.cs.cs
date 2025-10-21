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
        _playerShoot = GetComponent<PlayerShoot>();
    }

    private void Update()
    {
        if (_playerShoot.health <= 0)
        {
            healthText.text = "Dead";
        }
        else
        {
            healthText.text = _playerShoot.health.ToString();
        }
    }
}