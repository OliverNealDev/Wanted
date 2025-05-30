using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class playerController : MonoBehaviour
{
    private float _currentMoveSpeed;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeedMultiplier = 2f;
    [SerializeField] private float maxStamina = 1f;
    [SerializeField] private float staminaDrainRate = 1f;
    [SerializeField] private float _currentStamina = 0.5f;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private float turnSpeed = 5f;

    [SerializeField] private float seenToSpottedTime = 0.4f;

    private Camera cam;
    private Vector2 _movementInput;
    private SpriteRenderer spriteRenderer;
    public List<GameObject> foliageUnder = new List<GameObject>();
    
    [SerializeField] private float meleeRange = 0.5f;
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField] private float meleeCooldown = 0.5f;
    [SerializeField] private float meleeCooldownTimer = 0f;
    [SerializeField] private GameObject playerHand;

    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.15f; 
    [SerializeField] private float pitchVariation = 0.1f;
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private bool wasEffectivelySprintingLastFrame = false;

    void Start()
    {
        cam = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = _currentStamina;
        }
        _currentMoveSpeed = walkSpeed;
    }

    void Update()
    {
        meleeCooldownTimer += Time.deltaTime;
        
        _movementInput = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) _movementInput.y += 1;
        if (Input.GetKey(KeyCode.S)) _movementInput.y -= 1;
        if (Input.GetKey(KeyCode.A)) _movementInput.x -= 1;
        if (Input.GetKey(KeyCode.D)) _movementInput.x += 1;

        if (_movementInput.magnitude > 1)
        {
            _movementInput.Normalize();
        }

        bool isMoving = _movementInput.magnitude > 0;
        bool isHoldingSprintKey = Input.GetKey(KeyCode.LeftShift);
        bool hasStaminaToSprint = _currentStamina > 0;
        bool isActuallySprintingThisFrame = false;

        if (isHoldingSprintKey && hasStaminaToSprint && isMoving)
        {
            _currentMoveSpeed = walkSpeed * sprintSpeedMultiplier;
            _currentStamina -= staminaDrainRate * Time.deltaTime;
            isActuallySprintingThisFrame = true;
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

        if (isMoving)
        {
            transform.Translate(_movementInput * (_currentMoveSpeed * Time.deltaTime), Space.World);

            float angle = Mathf.Atan2(_movementInput.y, _movementInput.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        bool isEffectivelySprintingForAudio = isActuallySprintingThisFrame && isMoving;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (isEffectivelySprintingForAudio && !wasEffectivelySprintingLastFrame)
            {
                PlayFootstepSound();
                footstepTimer = sprintStepInterval;
            }
            else if (!isEffectivelySprintingForAudio && wasEffectivelySprintingLastFrame)
            {
                PlayFootstepSound();
                footstepTimer = walkStepInterval;
            }
            else if (footstepTimer <= 0f)
            {
                PlayFootstepSound();
                footstepTimer = isEffectivelySprintingForAudio ? sprintStepInterval : walkStepInterval;
            }
        }
        else
        {
            footstepTimer = 0f; 
        }
        
        wasEffectivelySprintingLastFrame = isEffectivelySprintingForAudio;

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
    }

    void PlayFootstepSound()
    {
        if (audioSource != null && footstepSounds != null && footstepSounds.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, footstepSounds.Length);
            AudioClip stepSound = footstepSounds[randomIndex];
            if (stepSound != null)
            {
                audioSource.pitch = 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(stepSound);
            }
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
