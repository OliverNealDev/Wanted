using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [Header("Movement")]
    private float _currentMoveSpeed;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float _currentStamina = 0.5f;
    [SerializeField] private Slider staminaSlider;

    [SerializeField] private float turnSpeed = 5f;

    [Header("Visibility")]
    [SerializeField] private float seenToSpottedTime = 0.4f;

    private Camera cam;
    private Vector2 _movementInput;

    private SpriteRenderer spriteRenderer;

    public List<GameObject> foliageUnder = new List<GameObject>();

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = _currentStamina;
        }
    }

    void Update()
    {
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && _currentStamina > 0;

        if (isTryingToSprint)
        {
            _currentMoveSpeed = walkSpeed * sprintSpeedMultiplier;
            _currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            _currentMoveSpeed = walkSpeed;
        }
        
        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        if (staminaSlider != null)
        {
            staminaSlider.value = _currentStamina;
        }

        _movementInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) _movementInput.y += 1;
        if (Input.GetKey(KeyCode.S)) _movementInput.y -= 1;
        if (Input.GetKey(KeyCode.A)) _movementInput.x -= 1;
        if (Input.GetKey(KeyCode.D)) _movementInput.x += 1;

        if (_movementInput.magnitude > 1)
        {
            _movementInput.Normalize();
        }

        transform.Translate(_movementInput * (_currentMoveSpeed * Time.deltaTime), Space.World);

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool isUnderBerryBush = false;
            GameObject bushFound = null;
            foreach (GameObject foliageObject in foliageUnder)
            {
                if (foliageObject.CompareTag("berryBush"))
                {
                    isUnderBerryBush = true;
                    bushFound = foliageObject;
                    break;
                }
            }

            if (isUnderBerryBush && bushFound != null)
            {
                _currentStamina += 0.2f;
                _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina); 
                bushController bushScript = bushFound.GetComponent<bushController>();
                if (bushScript != null)
                {
                    bushScript.onBerriesEaten();
                }
                if (staminaSlider != null)
                {
                    staminaSlider.value = _currentStamina;
                }
            }
        }

        Vector3 mouseScreenPosition = Input.mousePosition;
        if (cam != null)
        {
            float playerScreenDepth = cam.WorldToScreenPoint(transform.position).z;
            mouseScreenPosition.z = playerScreenDepth;
            Vector2 mousePos = cam.ScreenToWorldPoint(mouseScreenPosition);

            Vector2 lookDir = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    public void cherryConsumed()
    {
        _currentStamina += 0.1f;
        _currentStamina = Mathf.Clamp(_currentStamina, 0, maxStamina);
        if (staminaSlider != null)
        {
            staminaSlider.value = _currentStamina;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tree") || other.CompareTag("berryBush"))
        {
            if (!foliageUnder.Contains(other.gameObject))
            {
                foliageUnder.Add(other.gameObject);
            }
            if (spriteRenderer != null)
            {
                 spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Tree") || other.CompareTag("berryBush"))
        {
            foliageUnder.Remove(other.gameObject);
            if (foliageUnder.Count == 0 && spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }
}