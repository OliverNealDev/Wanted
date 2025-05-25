using System;
using UnityEngine;

public class bushController : MonoBehaviour
{
   [SerializeField] private Sprite emptyBushSprite;
   [SerializeField] private Sprite berryBushSprite;
   private SpriteRenderer SR;
   
   [SerializeField] private float berryImmediateSpawnChance = 0.2f;
   [SerializeField] private float minBerrySpawnTime = 6f;
   [SerializeField] private float maxBerrySpawnTime = 18f;
   
   private bool isBerryBush = false;

   void Start()
   {
      SR = GetComponent<SpriteRenderer>();
      
      if (UnityEngine.Random.value < berryImmediateSpawnChance)
      {
         isBerryBush = true;
         SR.sprite = berryBushSprite;
      }
      else
      {
         Invoke("growBerries", UnityEngine.Random.Range(minBerrySpawnTime, maxBerrySpawnTime));
      }
   }
   
   void growBerries()
   {
      if (!isBerryBush)
      {
         isBerryBush = true;
         SR.sprite = berryBushSprite;
      }
      
      Invoke("growBerries", UnityEngine.Random.Range(minBerrySpawnTime, maxBerrySpawnTime));
   }
}
