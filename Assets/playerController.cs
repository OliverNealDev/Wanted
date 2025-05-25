using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [Header("Movement")] 
    private float _currentMoveSpeed;
    
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float maxStamina = 1f;
    //[SerializeField] private float staminaRegenRate = 5f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float _currentStamina = 0.5f;
    [SerializeField] private Slider staminaSlider;
    
    [SerializeField] private float turnSpeed = 5f;
    
    [Header("Visibility")]
    [SerializeField] private float seenToSpottedTime = 0.4f;
    [SerializeField] private float sneakingAudioDistance = 5f;
    [SerializeField] private float walkingAudioDistance = 10f;
    [SerializeField] private float runningAudioDistance = 15f;
    [SerializeField] private float twigBranchAudioDistance = 20f;
    
    private Camera cam;
    private Vector2 _movementInput;
    
    private SpriteRenderer spriteRenderer;
    
    private List<GameObject> foliageUnder = new List<GameObject>();

    void Start()
    {
        //_currentStamina = maxStamina;
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        staminaSlider.value = _currentStamina;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && _currentStamina > 0)
        {
            _currentMoveSpeed = walkSpeed * sprintSpeedMultiplier;
            _currentStamina -= staminaDrainRate * Time.deltaTime;
            staminaSlider.value = _currentStamina;
        }
        else
        {
            _currentMoveSpeed = walkSpeed;
            /*if (_currentStamina < maxStamina)
            {
                _currentStamina += staminaRegenRate * Time.deltaTime;
                staminaSlider.value = _currentStamina / maxStamina;
            }*/
        }
        
        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);

        _movementInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
        {
            _movementInput.y += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            _movementInput.y -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            _movementInput.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            _movementInput.x += 1;
        }

        if (_movementInput.magnitude > 1)
        {
            _movementInput.Normalize();
        }
        
        transform.Translate(_movementInput * (_currentMoveSpeed * Time.deltaTime), Space.World);

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isUnderBerryBush = false;
            GameObject bushUnder = null;
            foreach (GameObject foliageUnder in foliageUnder)
            {
                if (foliageUnder.CompareTag("berryBush"))
                {
                    isUnderBerryBush = true;
                    bushUnder = foliageUnder;
                }
            }

            if (isUnderBerryBush)
            {
                _currentStamina += 0.2f;
                bushUnder.GetComponent<bushController>().onBerriesEaten();
                staminaSlider.value = _currentStamina / maxStamina;
            }
        }
        
        Vector3 mouseScreenPosition = Input.mousePosition;
        float playerScreenDepth = cam.WorldToScreenPoint(transform.position).z;
        mouseScreenPosition.z = playerScreenDepth;
        Vector2 mousePos = cam.ScreenToWorldPoint(mouseScreenPosition);
        
        Vector2 lookDir = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tree") || other.CompareTag("berryBush"))
        {
            Debug.Log(other.name);
            
            if (!foliageUnder.Contains(other.gameObject))
            {
                foliageUnder.Add(other.gameObject);
            }
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tree") || other.CompareTag("berryBush"))
        {
            Debug.Log("Exited: " + other.name);
            
            foliageUnder.Remove(other.gameObject);
            if (foliageUnder.Count == 0)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}