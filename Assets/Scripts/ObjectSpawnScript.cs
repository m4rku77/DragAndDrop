using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawnScript : MonoBehaviour
{
    [Header("Vehicles to spawn")]
    public GameObject[] carPrefabs;

    [Header("Drop places (targets)")]
    public GameObject[] placeObjects;

    [Header("Spawn Area Settings")]
    public Vector2 spawnAreaMin = new Vector2(-400f, -250f);
    public Vector2 spawnAreaMax = new Vector2(400f, 250f);
    public bool useCanvas = true;

    [Header("Spacing Settings")]
    public float minDistance = 120f;
    public bool expandAreaIfCrowded = true;

    // ✅ Master list of all used positions (cars + places)
    private readonly List<Vector2> usedPositions = new List<Vector2>();

    void Start()
    {
        if (carPrefabs.Length == 0 && placeObjects.Length == 0)
        {
            Debug.LogWarning("⚠️ No objects assigned to spawn!");
            return;
        }

        // 1️⃣ Spawn cars
        foreach (GameObject car in carPrefabs)
        {
            Vector2 pos = GetRandomPosition();
            ApplyPosition(car, pos);
            usedPositions.Add(pos);
        }

        // 2️⃣ Spawn places (respecting car positions too)
        foreach (GameObject place in placeObjects)
        {
            Vector2 pos = GetRandomPosition();
            ApplyPosition(place, pos);
            usedPositions.Add(pos);
        }
    }

    private Vector2 GetRandomPosition()
    {
        const int maxAttempts = 100;
        int attempts = 0;
        Vector2 randomPos = Vector2.zero;
        bool valid = false;

        float expansionStep = 0f;

        while (!valid && attempts < maxAttempts)
        {
            attempts++;

            randomPos = new Vector2(
                Random.Range(spawnAreaMin.x - expansionStep, spawnAreaMax.x + expansionStep),
                Random.Range(spawnAreaMin.y - expansionStep, spawnAreaMax.y + expansionStep)
            );

            valid = true;

            foreach (Vector2 used in usedPositions)
            {
                if (Vector2.Distance(randomPos, used) < minDistance)
                {
                    valid = false;
                    break;
                }
            }

            // Gradually expand spawn area if too crowded
            if (!valid && expandAreaIfCrowded && attempts % 25 == 0)
            {
                expansionStep += 50f; // expands range slightly every 25 attempts
            }
        }

        if (!valid)
            Debug.LogWarning($"⚠️ Could not find valid position after {maxAttempts} attempts. Using last position anyway.");

        return randomPos;
    }

    private void ApplyPosition(GameObject obj, Vector2 pos)
    {
        if (useCanvas)
        {
            RectTransform rect = obj.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = pos;
        }
        else
        {
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
    }
}
