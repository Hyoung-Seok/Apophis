using System;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float _currentHealth;

    private void Start()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        Debug.Log(_currentHealth);
        
        if(_currentHealth <= 0f)
            Destroy(gameObject);
    }
}
