using System.Collections.Generic;
using UnityEngine;

public class MarkerConnectionManager : MonoBehaviour
{
    [SerializeField] private GameObject objectToClone;
    [SerializeField] private GameObject circlePrefab;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int positionBufferSize = 30;
    
    private GameObject clonedObject;
    private GameObject circleObject;
    private Dictionary<string, Queue<Vector3>> positionHistory = new Dictionary<string, Queue<Vector3>>();
    private string firstMarkerId;
    private string secondMarkerId;
    
    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;
        }
        
        lineRenderer.enabled = false;
    }
    
    private void Update()
    {
        // Check active markers in MarkerPool
        Dictionary<string, MarkerController> activeMarkers = GetActiveMarkers();
        
        if (activeMarkers.Count >= 2)
        {
            string[] keys = new string[activeMarkers.Count];
            activeMarkers.Keys.CopyTo(keys, 0);
            
            // Use the first two active markers
            firstMarkerId = keys[0];
            secondMarkerId = keys[1];
            
            // Create cloned object if it doesn't exist
            if (clonedObject == null && objectToClone != null)
            {
                clonedObject = Instantiate(objectToClone);
            }
            
            // Create circle if it doesn't exist
            if (circleObject == null && circlePrefab != null)
            {
                circleObject = Instantiate(circlePrefab);
            }
            
            // Update position histories
            UpdatePositionHistory(firstMarkerId, activeMarkers[firstMarkerId].transform.position);
            UpdatePositionHistory(secondMarkerId, activeMarkers[secondMarkerId].transform.position);
            
            // Calculate averaged positions
            Vector3 firstPos = CalculateAveragePosition(firstMarkerId);
            Vector3 secondPos = CalculateAveragePosition(secondMarkerId);
            
            // Position objects
            objectToClone.transform.position = firstPos;
            if (clonedObject != null)
            {
                clonedObject.transform.position = secondPos;
            }
            
            // Calculate midpoint and position circle
            Vector3 midpoint = (firstPos + secondPos) / 2f;
            if (circleObject != null)
            {
                circleObject.transform.position = midpoint;
            }
            
            // Draw line
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, firstPos);
            lineRenderer.SetPosition(1, secondPos);
        }
        else
        {
            // Hide line if fewer than 2 markers
            lineRenderer.enabled = false;
            
            // Hide cloned object and circle
            if (clonedObject != null)
            {
                clonedObject.SetActive(false);
            }
            
            if (circleObject != null)
            {
                circleObject.SetActive(false);
            }
        }
    }
    
    private Dictionary<string, MarkerController> GetActiveMarkers()
    {
        // This function will need to get active markers from QrCodeDisplayManager
        // For now, we'll implement a placeholder that you'll need to update
        Dictionary<string, MarkerController> activeMarkers = new Dictionary<string, MarkerController>();
        
        // Find QrCodeDisplayManager in the scene
        QrCodeDisplayManager displayManager = FindObjectOfType<QrCodeDisplayManager>();
        if (displayManager != null)
        {
            // This assumes QrCodeDisplayManager has a public property to access active markers
            // You may need to modify QrCodeDisplayManager to expose this
            activeMarkers = displayManager.GetActiveMarkers();
        }
        
        return activeMarkers;
    }
    
    private void UpdatePositionHistory(string markerId, Vector3 position)
    {
        if (!positionHistory.ContainsKey(markerId))
        {
            positionHistory[markerId] = new Queue<Vector3>();
        }
        
        Queue<Vector3> history = positionHistory[markerId];
        
        history.Enqueue(position);
        
        // Keep only the last N positions
        while (history.Count > positionBufferSize)
        {
            history.Dequeue();
        }
    }
    
    private Vector3 CalculateAveragePosition(string markerId)
    {
        if (!positionHistory.ContainsKey(markerId) || positionHistory[markerId].Count == 0)
        {
            return Vector3.zero;
        }
        
        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in positionHistory[markerId])
        {
            sum += pos;
        }
        
        return sum / positionHistory[markerId].Count;
    }
} 