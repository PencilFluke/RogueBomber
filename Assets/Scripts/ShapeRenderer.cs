using System;
using UnityEngine;

public class ShapeRenderer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private int steps;
    public float radius;
    [SerializeField] private Color color = Color.red;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void RenderCircle(int steps, float radius, bool isLoop, Color color, float alpha)
    {
        GradientColorKey[] colors = new GradientColorKey[2]
        {
            new GradientColorKey(color, 0.0f),
            new GradientColorKey(color, 1.0f)
        };
        GradientAlphaKey[] alphas = new GradientAlphaKey[2]
        {
            new GradientAlphaKey(alpha, 0.0f),
            new GradientAlphaKey(alpha, 1.0f)
        };

        Gradient gradient = new Gradient();
        gradient.SetKeys(colors, alphas);

        lineRenderer.colorGradient = gradient;
        lineRenderer.loop = isLoop;
        lineRenderer.positionCount = steps;

        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            float circumferenceProgress = (float)currentStep / steps;
            float currentRadian = circumferenceProgress * 2 * Mathf.PI;
            float xScaled = Mathf.Cos(currentRadian);
            float yScaled = Mathf.Sin(currentRadian);

            float x = xScaled * radius;
            float y = yScaled * radius;

            Vector3 position = new Vector3(x, 0, y);

            lineRenderer.SetPosition(currentStep, position);
        }
    }

    void RenderLine(int pointCount, int steps)
    {
        lineRenderer.positionCount = pointCount;
        for (int currentStep = 0; currentStep < steps; currentStep++)
        {
            lineRenderer.SetPosition(currentStep, new Vector3(Mathf.Sqrt(currentStep), 1, currentStep));
        }
    }
}
