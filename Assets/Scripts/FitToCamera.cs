using UnityEngine;

public class FitToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public float distanceFromCamera = 10f;

    private void Start()
    {
        Fit();
    }

    private void OnValidate()
    {
        Fit();
    }

    public void Fit()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        transform.position = targetCamera.transform.position + targetCamera.transform.forward * distanceFromCamera;
        transform.rotation = targetCamera.transform.rotation;

        if (targetCamera.orthographic)
        {
            float height = targetCamera.orthographicSize * 2f;
            float width = height * targetCamera.aspect;

            transform.localScale = new Vector3(width, height, 1f);
        }
        else
        {
            float height = 2f * distanceFromCamera * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float width = height * targetCamera.aspect;

            transform.localScale = new Vector3(width, height, 1f);
        }
    }
}