using UnityEngine;
using System.Collections.Generic;

public class FindQRTracker : MonoBehaviour
{
    private GameObject marker1 = null;
    private GameObject marker2 = null;
    private GameObject clonedObject = null;
    private LineRenderer lineRenderer = null;
    private GameObject middleCircle = null;
    
    // Position history für die letzten 30 Frames
    private Queue<Vector3> marker1PositionHistory = new Queue<Vector3>();
    private Queue<Vector3> marker2PositionHistory = new Queue<Vector3>();
    private const int HISTORY_LENGTH = 30;
    
    // Rotationsgeschwindigkeit in Grad pro Sekunde
    [SerializeField] private float rotationSpeed = 30.0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // LineRenderer für die Verbindungslinie hinzufügen
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.positionCount = 2;
        
        // Kreis-Objekt für die Mitte erstellen
        middleCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        middleCircle.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        middleCircle.GetComponent<Renderer>().material.color = Color.green;
        middleCircle.SetActive(false);
        
        UpdateMarkerReferences();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMarkerReferences();
        HandleTwoMarkers();
        
        // Rotation für dieses Objekt anwenden
        if (marker1 != null)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        // Rotation für den Klon anwenden
        if (clonedObject != null && marker2 != null)
        {
            clonedObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    // Sucht nach allen GameObjects mit dem Tag "Marker" und aktualisiert die Referenzen
    private void UpdateMarkerReferences()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        
        if (markers.Length >= 2)
        {
            // Die letzten zwei im Array verwenden
            marker1 = markers[markers.Length - 1];
            marker2 = markers[markers.Length - 2];
        }
        else if (markers.Length == 1)
        {
            marker1 = markers[0];
            marker2 = null;
        }
        else
        {
            marker1 = null;
            marker2 = null;
        }
    }
    
    // Verarbeite zwei Marker, wenn vorhanden
    private void HandleTwoMarkers()
    {
        // Erstes Marker-Objekt verarbeiten
        if (marker1 != null)
        {
            // Position in History aufnehmen
            AddToPositionHistory(marker1PositionHistory, marker1.transform.position);
            
            // Dieses Objekt zur geglätteten Position bewegen
            Vector3 smoothedPosition = CalculateSmoothedPosition(marker1PositionHistory);
            transform.position = smoothedPosition;
        }
        
        // Bei zwei Markern
        if (marker1 != null && marker2 != null)
        {
            // Position des zweiten Markers in History aufnehmen
            AddToPositionHistory(marker2PositionHistory, marker2.transform.position);
            Vector3 smoothedPosition2 = CalculateSmoothedPosition(marker2PositionHistory);
            
            // Clone erstellen, wenn noch nicht vorhanden
            if (clonedObject == null)
            {
                clonedObject = Instantiate(gameObject, smoothedPosition2, Quaternion.identity);
                
                // LineRenderer und dieses Skript vom Klon entfernen, damit keine Endlosschleife entsteht
                Destroy(clonedObject.GetComponent<FindQRTracker>());
                Destroy(clonedObject.GetComponent<LineRenderer>());
            }
            else
            {
                // Klon auf den zweiten Marker setzen
                clonedObject.transform.position = smoothedPosition2;
            }
            
            // Linie zwischen beiden Objekten zeichnen
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, clonedObject.transform.position);
            
            // Kreis in der Mitte positionieren
            Vector3 middlePosition = (transform.position + clonedObject.transform.position) / 2;
            middleCircle.transform.position = middlePosition;
            middleCircle.SetActive(true);
        }
        else
        {
            // Wenn weniger als 2 Marker, Linie und Kreis ausblenden
            if (lineRenderer != null) lineRenderer.enabled = false;
            if (middleCircle != null) middleCircle.SetActive(false);
            
            // Optional: Klon entfernen, wenn er existiert
            if (clonedObject != null)
            {
                Destroy(clonedObject);
                clonedObject = null;
            }
        }
    }
    
    // Fügt eine Position zur History hinzu und hält die maximale Länge ein
    private void AddToPositionHistory(Queue<Vector3> history, Vector3 position)
    {
        history.Enqueue(position);
        
        while (history.Count > HISTORY_LENGTH)
        {
            history.Dequeue();
        }
    }
    
    // Berechnet die durchschnittliche Position aus der History
    private Vector3 CalculateSmoothedPosition(Queue<Vector3> history)
    {
        if (history.Count == 0)
            return Vector3.zero;
            
        Vector3 sum = Vector3.zero;
        foreach (Vector3 pos in history)
        {
            sum += pos;
        }
        
        return sum / history.Count;
    }
}