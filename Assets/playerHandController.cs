using System;
using System.Collections;
using UnityEngine;

public class playerHandController : MonoBehaviour
{
    private GameObject Player;
    
    public float meleeRange = 0.5f;
    public float knockbackForce = 1f; 
    public float animationDuration = 0.5f;

    void Start()
    {
        meleeRange = 2.0f;
        Player = GameObject.FindGameObjectWithTag("Player");
        if (Player != null)
        {
            transform.rotation = Player.transform.rotation;
        }
        else
        {
            Debug.LogWarning("playerHandController: 'Player' GameObject not found. Hand will use its initial orientation for 'forward' movement.");
        }
        StartCoroutine(AnimateAndDespawn());
    }

    IEnumerator AnimateAndDespawn()
    {
        Vector3 initialPosition = transform.position;
        Vector3 forwardDirection = transform.forward; 
        Vector3 targetPosition = initialPosition + forwardDirection * meleeRange;
        
        Quaternion initialRotation = transform.rotation;
        Quaternion rotationAmount = Quaternion.Euler(0, 0, 90);
        Quaternion targetRotation = initialRotation * rotationAmount;

        float moveTime = animationDuration / 2f;
        float rotateTime = animationDuration / 2f;
        float elapsedTime = 0f;

        if (meleeRange != 0f)
        {
            if (moveTime > float.Epsilon)
            {
                while (elapsedTime < moveTime)
                {
                    transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / moveTime);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
            }
        }
        transform.position = targetPosition;

        elapsedTime = 0f; 

        if (rotateTime > float.Epsilon)
        {
            while (elapsedTime < rotateTime)
            {
                transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / rotateTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
        transform.rotation = targetRotation;
        
        Destroy(gameObject);
    }
}