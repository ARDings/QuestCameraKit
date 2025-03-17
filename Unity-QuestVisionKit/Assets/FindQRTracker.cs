using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

public class FindQRTracker : MonoBehaviour
{
    private GameObject marker1 = null;
    private GameObject marker2 = null;
    private GameObject clonedObject1 = null;
    private GameObject clonedObject2 = null;
    private LineRenderer lineRenderer = null;
    
    [SerializeField] private GameObject cubePrefab = null; // Feld für das Würfel-Prefab im Inspector
    [SerializeField] private GameObject markerObjectPrefab = null; // Prefab für die Objekte auf den Markern
    private GameObject middleCube = null;
    private GameObject directionArrow = null; // Neuer Pfeil für die Richtungsanzeige
    private bool bothMarkersPlaced = false;
    
    // Position history für die ersten 90 Frames und die zweiten 90 Frames
    private Queue<Vector3> marker1PositionHistory = new Queue<Vector3>();
    private Queue<Vector3> marker2PositionHistory = new Queue<Vector3>();
    
    // Anzahl der Frames für stabile Positionierung reduziert
    private const int HISTORY_LENGTH = 60;  // Von 90 auf 60 reduziert
    
    // Marker-IDs für bessere Konsistenz speichern
    private int marker1ID = -1;
    private int marker2ID = -1;
    
    // Minimale Distanz zwischen den Markern (50cm)
    private const float MIN_MARKER_DISTANCE = 0.5f;
    
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
        
        // Würfel-Objekt erstellen oder Prefab instanziieren
        if (cubePrefab != null)
        {
            middleCube = Instantiate(cubePrefab);
        }
        else
        {
            // Fallback: Würfel-Objekt für die Mitte erstellen, wenn kein Prefab gesetzt ist
            middleCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            middleCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // 30x30x30 cm

            // Standardmaterial verwenden
            Renderer cubeRenderer = middleCube.GetComponent<Renderer>();
            cubeRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cubeRenderer.material.color = Color.green;
        }
        
        // Richtungspfeil erstellen (als Zylinder)
        directionArrow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        
        // Zylinder als Kind dem Würfel hinzufügen und positionieren
        directionArrow.transform.SetParent(middleCube.transform);
        directionArrow.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f); // Dünner, länglicher Zylinder
        directionArrow.transform.localPosition = new Vector3(0, 0, 0.25f); // Vor dem Würfel platzieren (Z-Achse)
        directionArrow.transform.localRotation = Quaternion.Euler(90, 0, 0); // Rotieren, damit er in Z-Richtung zeigt
        
        // Richtungspfeil einfärben
        Renderer arrowRenderer = directionArrow.GetComponent<Renderer>();
        arrowRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        arrowRenderer.material.color = Color.red;
        
        // Würfel und Pfeil initial ausblenden
        middleCube.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Prüfe auf Stick-Druck des linken Controllers
        CheckForResetInput();
        
        // Wenn beide Marker platziert sind, nicht mehr nach weiteren suchen
        if (!bothMarkersPlaced)
        {
            UpdateMarkerReferences();
            HandleMarkers();
        }
    }
    
    // Prüft, ob der Stick des linken Controllers gedrückt wurde
    private void CheckForResetInput()
    {
        // Prüfen, ob der linke Controller-Stick gedrückt wurde
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid)
        {
            bool stickPressed = false;
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out stickPressed);
            
            if (stickPressed)
            {
                ResetAll();
            }
        }
    }
    
    // Setzt alle Objekte zurück
    private void ResetAll()
    {
        bothMarkersPlaced = false;
        
        if (clonedObject1 != null)
        {
            Destroy(clonedObject1);
            clonedObject1 = null;
        }
        
        if (clonedObject2 != null)
        {
            Destroy(clonedObject2);
            clonedObject2 = null;
        }
        
        marker1 = null;
        marker2 = null;
        marker1PositionHistory.Clear();
        marker2PositionHistory.Clear();
        
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (middleCube != null) middleCube.SetActive(false);
        
        Debug.Log("QR Tracker: Alles zurückgesetzt");
    }
    
    // Sucht nach allen GameObjects mit dem Tag "Marker" und aktualisiert die Referenzen
    private void UpdateMarkerReferences()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
        
        // Kein Marker gefunden
        if (markers.Length == 0)
        {
            Debug.Log("QR Tracker: Keine Marker gefunden");
            return;
        }
        
        // Erster Marker noch nicht gesetzt oder verloren gegangen
        if (marker1 == null)
        {
            marker1 = markers[0];
            marker1ID = marker1.GetInstanceID();
            Debug.Log("QR Tracker: Erster Marker gefunden/wiederhergestellt: " + marker1.name + " (ID: " + marker1ID + ")");
            // History zurücksetzen
            marker1PositionHistory.Clear();
            return;
        }
        
        // Überprüfen, ob der erste Marker noch existiert und aktualisieren
        bool marker1Found = false;
        foreach (GameObject marker in markers)
        {
            if (marker.GetInstanceID() == marker1ID)
            {
                marker1 = marker; // Referenz auffrischen
                marker1Found = true;
                break;
            }
        }
        
        if (!marker1Found && markers.Length > 0)
        {
            // Ersten Marker neu zuweisen, wenn er verloren gegangen ist
            marker1 = markers[0];
            marker1ID = marker1.GetInstanceID();
            marker1PositionHistory.Clear();
            Debug.Log("QR Tracker: Erster Marker neu zugewiesen: " + marker1.name + " (ID: " + marker1ID + ")");
        }
        
        // Wenn wir schon den ersten Marker haben, suchen wir nach einem anderen Marker
        if (marker1 != null && marker2 == null && markers.Length >= 2)
        {
            foreach (GameObject potentialMarker2 in markers)
            {
                // Wir prüfen, ob es sich um einen anderen Marker handelt
                // und ob der Marker weit genug entfernt ist
                if (potentialMarker2.GetInstanceID() != marker1ID &&
                    Vector3.Distance(potentialMarker2.transform.position, marker1.transform.position) >= MIN_MARKER_DISTANCE)
                {
                    marker2 = potentialMarker2;
                    marker2ID = marker2.GetInstanceID();
                    // History zurücksetzen
                    marker2PositionHistory.Clear();
                    Debug.Log("QR Tracker: Zweiter Marker gefunden: " + marker2.name + 
                              " (ID: " + marker2ID + ", Abstand: " + Vector3.Distance(marker2.transform.position, marker1.transform.position) + " m)");
                    break;
                }
            }
        }
        // Falls der zweite Marker bereits bekannt ist, prüfen, ob er noch existiert
        else if (marker1 != null && marker2 != null)
        {
            bool marker2Found = false;
            foreach (GameObject marker in markers)
            {
                if (marker.GetInstanceID() == marker2ID)
                {
                    marker2 = marker; // Referenz auffrischen
                    marker2Found = true;
                    break;
                }
            }
            
            if (!marker2Found)
            {
                // Wenn der zweite Marker verloren gegangen ist, nach einem neuen suchen
                marker2 = null;
                marker2ID = -1;
                marker2PositionHistory.Clear();
                Debug.Log("QR Tracker: Zweiter Marker verloren, suche nach neuem zweiten Marker");
                
                // Sofort versuchen, einen neuen zweiten Marker zu finden
                foreach (GameObject potentialMarker2 in markers)
                {
                    if (potentialMarker2.GetInstanceID() != marker1ID &&
                        Vector3.Distance(potentialMarker2.transform.position, marker1.transform.position) >= MIN_MARKER_DISTANCE)
                    {
                        marker2 = potentialMarker2;
                        marker2ID = marker2.GetInstanceID();
                        marker2PositionHistory.Clear();
                        Debug.Log("QR Tracker: Neuer zweiter Marker gefunden: " + marker2.name + 
                                  " (ID: " + marker2ID + ", Abstand: " + Vector3.Distance(marker2.transform.position, marker1.transform.position) + " m)");
                        break;
                    }
                }
            }
        }
    }
    
    // Verarbeite Marker, wenn vorhanden
    private void HandleMarkers()
    {
        // Erstes Marker-Objekt verarbeiten
        if (marker1 != null && clonedObject1 == null)
        {
            // Position in History aufnehmen
            AddToPositionHistory(marker1PositionHistory, marker1.transform.position);
            
            // Sobald genügend Frames gesammelt wurden, ersten Klon erstellen
            if (marker1PositionHistory.Count >= HISTORY_LENGTH)
            {
                Vector3 smoothedPosition = CalculateSmoothedPosition(marker1PositionHistory);
                
                // Prefab oder Fallback verwenden
                if (markerObjectPrefab != null) 
                {
                    clonedObject1 = Instantiate(markerObjectPrefab, smoothedPosition, Quaternion.identity);
                }
                else 
                {
                    // Fallback: Gameobject klonen, wenn kein Prefab gesetzt ist
                    clonedObject1 = Instantiate(gameObject, smoothedPosition, Quaternion.identity);
                    
                    // Skript vom Klon entfernen, damit keine Endlosschleife entsteht
                    Destroy(clonedObject1.GetComponent<FindQRTracker>());
                    
                    // LineRenderer vom Klon entfernen
                    if (clonedObject1.GetComponent<LineRenderer>() != null)
                    {
                        Destroy(clonedObject1.GetComponent<LineRenderer>());
                    }
                }
                
                Debug.Log("QR Tracker: Erstes Objekt platziert bei " + smoothedPosition);
            }
        }
        
        // Zweites Marker-Objekt verarbeiten
        if (marker2 != null && clonedObject1 != null && clonedObject2 == null)
        {
            // Position in History aufnehmen
            AddToPositionHistory(marker2PositionHistory, marker2.transform.position);
            
            // Debug-Ausgabe für Fortschritt
            if (marker2PositionHistory.Count % 10 == 0 || marker2PositionHistory.Count == HISTORY_LENGTH)
            {
                Debug.Log("QR Tracker: Sammle Daten für zweiten Marker: " + marker2PositionHistory.Count + 
                          "/" + HISTORY_LENGTH + " Frames");
            }
            
            // Sobald genügend Frames gesammelt wurden, zweiten Klon erstellen
            if (marker2PositionHistory.Count >= HISTORY_LENGTH)
            {
                Vector3 smoothedPosition = CalculateSmoothedPosition(marker2PositionHistory);
                
                // Prefab oder Fallback verwenden
                if (markerObjectPrefab != null) 
                {
                    clonedObject2 = Instantiate(markerObjectPrefab, smoothedPosition, Quaternion.identity);
                }
                else 
                {
                    // Fallback: Gameobject klonen, wenn kein Prefab gesetzt ist
                    clonedObject2 = Instantiate(gameObject, smoothedPosition, Quaternion.identity);
                    
                    // Skript vom Klon entfernen, damit keine Endlosschleife entsteht
                    Destroy(clonedObject2.GetComponent<FindQRTracker>());
                    
                    // LineRenderer vom Klon entfernen
                    if (clonedObject2.GetComponent<LineRenderer>() != null)
                    {
                        Destroy(clonedObject2.GetComponent<LineRenderer>());
                    }
                }
                
                // Marker sind platziert, nicht mehr nach weiteren suchen
                bothMarkersPlaced = true;
                Debug.Log("QR Tracker: Zweites Objekt platziert bei " + smoothedPosition);
            }
        }
        
        // Bei zwei Klonen Linie und Würfel anzeigen
        if (clonedObject1 != null && clonedObject2 != null)
        {
            // Linie zwischen beiden Objekten zeichnen
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, clonedObject1.transform.position);
            lineRenderer.SetPosition(1, clonedObject2.transform.position);
            
            // Würfel in der Mitte positionieren
            Vector3 middlePosition = (clonedObject1.transform.position + clonedObject2.transform.position) / 2;
            middleCube.transform.position = middlePosition;
            middleCube.SetActive(true);
            
            Debug.Log("QR Tracker: Linie und Würfel zwischen Objekten gezeichnet");
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