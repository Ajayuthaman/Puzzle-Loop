using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public int WireType;
    public bool IsPowered;

    [SerializeField] private Transform[] wirePrefabs;
    [SerializeField] private float rotationSpeed = 5f;

    private Transform currentWire;
    private int rotation;
    private bool isRotating;

    private List<Transform> connectors = new List<Transform>();
    private SpriteRenderer sprite;

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

        CacheComponents();
    }

    private void CacheComponents()
    {
        if (currentWire.childCount > 0)
            sprite = currentWire.GetChild(0).GetComponent<SpriteRenderer>();

        connectors.Clear();
        for (int i = 1; i < currentWire.childCount; i++)
            connectors.Add(currentWire.GetChild(i));
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

    // ---------------------------------------------------------
    // COLOR UPDATE
    // ---------------------------------------------------------
    public void UpdateVisual()
    {
        if (sprite == null) return;

        if (IsPowered)
            sprite.color = Color.white;
        else
            sprite.color = new Color(1f, 1f, 1f, 0.25f);
    }

    // ---------------------------------------------------------
    // BFS CONNECTOR LOGIC
    // ---------------------------------------------------------
    public void GetConnectedWires(HashSet<Wire> visited, Queue<Wire> q)
    {
        foreach (var p in connectors)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(p.position, Vector2.zero, 0.1f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null) continue;

                // collider -> child -> parent -> Wire
                Transform root = hit.collider.transform.parent?.parent;
                if (root == null) continue;

                Wire other = root.GetComponent<Wire>();
                if (other == null) continue;

                if (!visited.Contains(other))
                {
                    other.IsPowered = true;
                    visited.Add(other);
                    q.Enqueue(other);
                }
            }
        }
    }
}
