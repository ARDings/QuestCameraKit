using Meta.XR;
using UnityEngine;
using System.Collections;
using PassthroughCameraSamples;

public enum SamplingMode
{
    Environment,
    Manual
}

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private SamplingMode samplingMode = SamplingMode.Environment;
    [SerializeField] private float fadeDuration = 3f;  // Dauer des Farbübergangs in Sekunden

    [Header("Environment Sampling")]
    [SerializeField] private Transform raySampleOrigin;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Manual Sampling")]
    [SerializeField] private Transform manualSamplingOrigin;

    [Header("Brightness Correction")]
    [SerializeField, Range(0f, 1f)] private float targetBrightness = 0.8f;
    [SerializeField, Range(0f, 1f)] private float correctionSmoothing = 0.5f;
    [SerializeField] private int roiSize = 3;
    [SerializeField] private float minCorrection = 0.8f;
    [SerializeField] private float maxCorrection = 1.5f;

    private float _prevCorrectionFactor = 1f;
    private Vector3? _lastHitPoint;
    private Camera _mainCamera;
    private Renderer _manualRenderer;
    private WebCamTexture _webcamTexture;
    private WebCamTextureManager _cameraManager;
    private EnvironmentRaycastManager _raycastManager;
    private Color _currentColor;
    private Color _targetColor;
    private Coroutine _fadeCoroutine;

    private void Start()
    {
        _mainCamera = Camera.main;
        _cameraManager = FindAnyObjectByType<WebCamTextureManager>();
        _raycastManager = GetComponent<EnvironmentRaycastManager>();

        if (!_mainCamera || !_cameraManager || !_raycastManager ||
            (samplingMode == SamplingMode.Environment && !raySampleOrigin) ||
            (samplingMode == SamplingMode.Manual && !manualSamplingOrigin))
        {
            Debug.LogError("ColorPicker: Missing required references.");
            return;
        }

        if (manualSamplingOrigin)
        {
            _manualRenderer = manualSamplingOrigin.GetComponent<Renderer>();
            if (_manualRenderer)
            {
                _currentColor = _manualRenderer.material.color;
            }
        }

        SetupLineRenderer();
        StartCoroutine(WaitForWebCam());
    }

    private IEnumerator WaitForWebCam()
    {
        while (!_cameraManager.WebCamTexture || !_cameraManager.WebCamTexture.isPlaying)
        {
            yield return null;
        }

        _webcamTexture = _cameraManager.WebCamTexture;
    }

    private void Update()
    {
        UpdateSamplingPoint();

        if (OVRInput.GetUp(OVRInput.Button.One))
        {
            PickColor();
        }
    }

    private void UpdateSamplingPoint()
    {
        if (samplingMode == SamplingMode.Environment)
        {
            Ray ray = new(raySampleOrigin.position, raySampleOrigin.forward);
            var hitSuccess = _raycastManager.Raycast(ray, out var hit);

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, hitSuccess ? hit.point : ray.origin + ray.direction * 5f);

            _lastHitPoint = hitSuccess ? hit.point : null;
        }
        else
        {
            lineRenderer.enabled = false;
            _lastHitPoint = manualSamplingOrigin.position;
        }
    }

    private void PickColor()
    {
        if (_lastHitPoint == null || !_webcamTexture || !_webcamTexture.isPlaying)
        {
            Debug.LogWarning("ColorPicker: Invalid sampling point or webcam texture not ready.");
            return;
        }

        var uv = WorldToTextureUV(_lastHitPoint.Value);
        var color = SampleAndCorrectColor(uv);

        if (_manualRenderer)
        {
            // Finde das Elternobjekt
            Transform parentTransform = manualSamplingOrigin.parent;
            if (parentTransform != null)
            {
                // Dupliziere das Elternobjekt an seiner aktuellen Position
                GameObject duplicatedParent = Instantiate(parentTransform.gameObject, parentTransform.position, parentTransform.rotation);
                
                // Finde das entsprechende Kind im duplizierten Objekt, das dem manualSamplingOrigin entspricht
                Transform[] duplicatedChildren = duplicatedParent.GetComponentsInChildren<Transform>();
                Renderer duplicatedRenderer = null;
                
                foreach (Transform child in duplicatedChildren)
                {
                    // Überprüfe, ob der Name des Kindes dem des manualSamplingOrigin entspricht
                    if (child.name == manualSamplingOrigin.name)
                    {
                        duplicatedRenderer = child.GetComponent<Renderer>();
                        break;
                    }
                }
                
                if (duplicatedRenderer != null)
                {
                    // Erstelle ein neues Material, um Konflikte zu vermeiden
                    Material newMaterial = new Material(duplicatedRenderer.material);
                    duplicatedRenderer.material = newMaterial;
                    
                    // Starte Farbübergang für das duplizierte Objekt
                    _targetColor = color;
                    
                    // Stoppe vorherige Fade-Coroutine, falls eine läuft
                    if (_fadeCoroutine != null)
                    {
                        StopCoroutine(_fadeCoroutine);
                    }
                    
                    // Starte neue Fade-Coroutine
                    Color startColor = duplicatedRenderer.material.color;
                    _fadeCoroutine = StartCoroutine(FadeToColor(duplicatedRenderer, startColor));
                }
            }
            else
            {
                // Fallback: Wenn kein Elternobjekt vorhanden ist, ändere nur die Farbe des Originals
                _targetColor = color;
                
                if (_fadeCoroutine != null)
                {
                    StopCoroutine(_fadeCoroutine);
                }
                
                _fadeCoroutine = StartCoroutine(FadeToColor(_manualRenderer, _currentColor));
            }
        }
    }

    private IEnumerator FadeToColor(Renderer targetRenderer = null, Color? startColorOverride = null)
    {
        // Wenn kein Ziel-Renderer übergeben wurde, verwende den Standard-Renderer
        if (targetRenderer == null)
        {
            targetRenderer = _manualRenderer;
        }

        Color startColor = startColorOverride ?? _currentColor;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);
            Color currentColor = Color.Lerp(startColor, _targetColor, t);
            
            if (targetRenderer)
            {
                targetRenderer.material.color = currentColor;
            }
            
            yield return null;
        }
        
        // Stelle sicher, dass wir exakt die Zielfarbe erreichen
        if (targetRenderer)
        {
            targetRenderer.material.color = _targetColor;
        }
        
        // Aktualisiere _currentColor nur, wenn wir das Original-Objekt ändern
        if (targetRenderer == _manualRenderer)
        {
            _currentColor = _targetColor;
        }
        
        _fadeCoroutine = null;
    }

    private Vector2 WorldToTextureUV(Vector3 worldPoint)
    {
        var cameraPose = PassthroughCameraUtils.GetCameraPoseInWorld(_cameraManager.eye);
        var localPoint = Quaternion.Inverse(cameraPose.rotation) * (worldPoint - cameraPose.position);
        var intrinsics = PassthroughCameraUtils.GetCameraIntrinsics(_cameraManager.eye);

        if (localPoint.z <= 0.0001f)
        {
            Debug.LogWarning("ColorPicker: Point too close.");
            return Vector2.zero;
        }

        var scaleX = _webcamTexture.width / (float)intrinsics.Resolution.x;
        var scaleY = _webcamTexture.height / (float)intrinsics.Resolution.y;

        var uPixel = intrinsics.FocalLength.x * (localPoint.x / localPoint.z) + intrinsics.PrincipalPoint.x;
        var vPixel = intrinsics.FocalLength.y * (localPoint.y / localPoint.z) + intrinsics.PrincipalPoint.y;

        uPixel *= scaleX;
        vPixel *= scaleY;

        var u = uPixel / _webcamTexture.width;
        var v = vPixel / _webcamTexture.height;

        return new Vector2(u, v);
    }

    private Color SampleAndCorrectColor(Vector2 uv)
    {
        var x = Mathf.Clamp(Mathf.RoundToInt(uv.x * _webcamTexture.width), 0, _webcamTexture.width - 1);
        var y = Mathf.Clamp(Mathf.RoundToInt(uv.y * _webcamTexture.height), 0, _webcamTexture.height - 1);

        var sampledColor = _webcamTexture.GetPixel(x, y);
        var brightness = CalculateRoiBrightness(x, y);

        var factor = Mathf.Clamp(targetBrightness / Mathf.Max(brightness, 0.001f), minCorrection, maxCorrection);
        _prevCorrectionFactor = Mathf.Lerp(_prevCorrectionFactor, factor, correctionSmoothing);

        var corrected = (sampledColor.linear * _prevCorrectionFactor).gamma;
        return new Color(Mathf.Clamp01(corrected.r), Mathf.Clamp01(corrected.g), Mathf.Clamp01(corrected.b), corrected.a);
    }

    private float CalculateRoiBrightness(int x, int y)
    {
        var sum = 0f;
        var count = 0;
        var half = roiSize / 2;

        for (var i = -half; i <= half; i++)
        {
            for (var j = -half; j <= half; j++)
            {
                int xi = x + i, yj = y + j;
                if (xi < 0 || xi >= _webcamTexture.width || yj < 0 || yj >= _webcamTexture.height)
                {
                    continue;
                }

                var pixel = _webcamTexture.GetPixel(xi, yj).linear;
                sum += 0.2126f * pixel.r + 0.7152f * pixel.g + 0.0722f * pixel.b;
                count++;
            }
        }

        return count > 0 ? sum / count : 0f;
    }

    private void SetupLineRenderer()
    {
        if (!lineRenderer)
        {
            return;
        }

        lineRenderer.enabled = true;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = lineRenderer.endWidth = 0.01f;
    }
}
