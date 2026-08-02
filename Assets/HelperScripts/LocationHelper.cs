using UnityEngine;

public static class LocationHelper
{
    static public Vector3 GetRandomPointInXZCircle(float minRadius, float maxRadius, float elevation)
    {
        Vector2 randomDirection = GetRandomXZDirection();
        Vector2 point = randomDirection * Random.Range(minRadius, maxRadius);

        return new Vector3(point.x, elevation, point.y);
    }

    static public Vector2 GetRandomXZDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }
}
