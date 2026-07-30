using UnityEngine;

public class Spin : MonoBehaviour
{
    [SerializeField]
    private float spinRate = 200.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * spinRate, Space.World);
    }
}
