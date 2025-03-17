using UnityEngine;

public class lookatcamera : MonoBehaviour
{
    [Tooltip("Rotationsgeschwindigkeit für das Drehen zur Kamera (höherer Wert = schnellere Rotation)")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("Wenn aktiviert, dreht sich das Objekt nur in der horizontalen Ebene zur Kamera")]
    [SerializeField] private bool horizontalOnly = true;
    
    private Camera mainCamera;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Finde die Hauptkamera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("LookAtCamera: Hauptkamera nicht gefunden!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null) return;
        
        Vector3 targetPosition = mainCamera.transform.position;
        
        if (horizontalOnly)
        {
            // Nur horizontale Rotation (Y-Achse)
            // Wir setzen die Y-Koordinate des Objekts und der Kamera auf den gleichen Wert,
            // damit die Berechnung nur die horizontale Ebene berücksichtigt
            targetPosition.y = transform.position.y;
            
            // Berechne die Richtung zur Kamera
            Vector3 direction = targetPosition - transform.position;
            
            // Nur rotieren, wenn eine Richtung vorhanden ist
            if (direction != Vector3.zero)
            {
                // Berechne die gewünschte Rotation (nur Y-Achse)
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                // Behalte die ursprüngliche X- und Z-Rotation bei
                targetRotation.x = transform.rotation.x;
                targetRotation.z = transform.rotation.z;
                
                // Wende die Rotation mit Interpolation für eine glattere Bewegung an
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Vollständige Rotation (alle Achsen)
            // Für den Fall, dass der Benutzer auch vertikale Rotation wünscht
            Vector3 direction = targetPosition - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }
}
