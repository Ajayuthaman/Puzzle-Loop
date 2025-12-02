using System.Collections;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public int WireType;

    [SerializeField] private Transform[] wirePrefabs;
    [SerializeField] private float rotationSpeed = 5f;

    private Transform currentWire;
    private int rotation;
    private bool isRotating;

    private const int MAX_ROT = 3;
    private const int DEG = 90;
    private static readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

    public void Init(int encodedValue)
    {
        WireType = encodedValue % 10;
        rotation = encodedValue / 10;

        currentWire = Instantiate(wirePrefabs[WireType], transform);
        currentWire.localPosition = Vector3.zero;

        currentWire.eulerAngles = new Vector3(0, 0, rotation * DEG);
    }

    public void Rotate()
    {
        if (isRotating) return;

        rotation = (rotation + 1) % (MAX_ROT + 1);
        StartCoroutine(RotateSmooth());
    }

    private IEnumerator RotateSmooth()
    {
        isRotating = true;

        float startZ = currentWire.eulerAngles.z;
        float targetZ = rotation * DEG;

        float t = 0;
        float duration = 1f / rotationSpeed;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / duration);
            lerp = lerp * lerp * (3f - 2f * lerp);

            float angle = Mathf.LerpAngle(startZ, targetZ, lerp);
            currentWire.eulerAngles = new Vector3(0, 0, angle);

            yield return waitFrame;
        }

        currentWire.eulerAngles = new Vector3(0, 0, targetZ);
        isRotating = false;
    }
}
