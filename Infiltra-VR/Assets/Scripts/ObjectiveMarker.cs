using UnityEngine;

public class ObjectiveMarker : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Kecepatan rotasi marker")]
    [SerializeField] private float rotationSpeed = 90f;
    
    [Tooltip("Kecepatan naik-turun (bobbing) marker")]
    [SerializeField] private float bobbingSpeed = 2f;
    
    [Tooltip("Seberapa tinggi marker naik-turun")]
    [SerializeField] private float bobbingHeight = 0.2f;

    private float originalY;

    private void Start()
    {
        // Simpan posisi Y awal
        originalY = transform.localPosition.y;
    }

    private void Update()
    {
        // Rotasi
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Naik Turun (Bobbing)
        float newY = originalY + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}
