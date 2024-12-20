using UnityEngine;

public class MetaballMovement : MonoBehaviour
{
    public float moveRadius = 2f;
    public float moveSpeed = 1f;
    public Vector3 centerOffset;
      
    private float timeOffset;
      
    void Start()
    {
        timeOffset = Random.Range(0f, Mathf.PI * 2);
    }
      
    void Update()
    {
        float time = Time.time * moveSpeed + timeOffset;
          
        transform.position = new Vector3(
            Mathf.Sin(time) * moveRadius,
            Mathf.Cos(time * 0.7f) * moveRadius,
            Mathf.Sin(time * 0.5f) * moveRadius
        ) + centerOffset;
    }
}