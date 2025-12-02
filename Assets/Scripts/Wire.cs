using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Wire: represents a single cell on the grid.
/// - Handles visual prefab instantiation (different wire shapes)
/// - Handles rotation input & smooth rotation animation
/// - Exposes connection points used by PowerGridManager for Color-fill
/// </summary>
public class Wire : MonoBehaviour
{
    // Public state used by manager and other systems
    [HideInInspector] public bool IsPowered;   
    [HideInInspector] public int WireType;    

    [SerializeField] private Transform[] _wirePrefabs;
    [SerializeField] private float rotationSpeed = 5f; 

    private Transform currentWire;           
    private int rotation;                   
    private bool isRotating;                 // true while coroutine rotates the visual
    private SpriteRenderer wireSprite;       // used to dim/un-dim visuals when not powered
    private List<Transform> connectionPoints;// children used to raycast connections

    // constants
    private const int MIN_ROTATION = 0;
    private const int MAX_ROTATION = 3;
    private const int ROTATION_DEGREES = 90;
    private const float RAYCAST_DISTANCE = 0.1f;

    private static readonly Vector3 zeroVector = Vector3.zero;
    private static readonly WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

    /// <summary>
    /// Initialize this wire with an encoded value from LevelData.
    /// </summary>
    public void Init(int encoded)
    {
        // decode type and initial rotation
        WireType = encoded % 10;
        currentWire = Instantiate(_wirePrefabs[WireType], transform);
        currentWire.localPosition = zeroVector;

        if (WireType == 1 ) 
        {
            rotation = encoded / 10;
        }
        else
        {
            // for non-power random orientation at spawn for variety
            rotation = Random.Range(MIN_ROTATION, MAX_ROTATION + 1);
        }

        // apply the visual rotation
        currentWire.eulerAngles = new Vector3(0, 0, rotation * ROTATION_DEGREES);

        // mark power state initial: keep power sources and empty cells considered "powered" visually
        if (WireType == 0 || WireType == 1)
        {
            IsPowered = true;
        }

        // only cache components for interactive wire types (not empty or power source visuals)
        if (WireType == 0) return;

        CacheComponents();
    }

    /// <summary>
    /// Cache commonly used child components: sprite and connection points.
    /// Assumes child(0) is the sprite and children 1+ are connection points.
    /// </summary>
    private void CacheComponents()
    {
        wireSprite = currentWire.GetChild(0).GetComponent<SpriteRenderer>();

        // the rest of the children are connection points (to raycast to neighbors)
        int childCount = currentWire.childCount;
        connectionPoints = new List<Transform>(childCount - 1);

        for (int i = 1; i < childCount; i++)
        {
            connectionPoints.Add(currentWire.GetChild(i));
        }

        // visuals reflect the current powered state
        UpdateVisuals();
    }

    /// <summary>
    /// Called by PowerGridManager when the player clicks on a cell.
    /// Rotates only if this wire type is rotatable (not empty/power) and not currently rotating.
    /// </summary>
    public void UpdateInput()
    {
        // don't rotate empty, power source; also skip if already rotating
        if (WireType == 0 || WireType == 1 || isRotating)
        {
            return;
        }

        // increment rotation state and start smooth rotation
        rotation = (rotation + 1) % (MAX_ROTATION + 1);
        StartCoroutine(RotateSmooth());
    }

    /// <summary>
    /// Smoothly interpolates the rotation from current angle to target angle using a smoothstep.
    /// Notifies PowerGridManager when rotation completes so fill state can be recalculated.
    /// </summary>
    private IEnumerator RotateSmooth()
    {
        isRotating = true;
        float startAngle = currentWire.eulerAngles.z;
        float targetAngle = rotation * ROTATION_DEGREES;

        float elapsed = 0f;
        float duration = 1f / rotationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // smoothstep (ease in/out) for a nicer feel
            t = t * t * (3f - 2f * t);

            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, t);
            currentWire.eulerAngles = new Vector3(0, 0, currentAngle);

            yield return waitFrame;
        }

        // ensure final angle is exact
        currentWire.eulerAngles = new Vector3(0, 0, targetAngle);
        isRotating = false;

        // Ask the manager to recalculate power propagation now that this wire moved
        if (PowerGridManager.Instance != null)
        {
            PowerGridManager.Instance.UpdateFillState();
        }
    }

    /// <summary>
    /// Called by manager to refresh visuals when powered state changes.
    /// </summary>
    public void UpdateFilled()
    {
        UpdateVisuals();
    }

    /// <summary>
    /// Update sprite alpha / color to show powered vs unpowered.
    /// Uses a dimmed alpha for unpowered wires.
    /// </summary>
    private void UpdateVisuals()
    {
        if (WireType == 0)
        {
            // empty cell — keep sprite fully white (or rely on the placeholder)
            if (wireSprite != null) wireSprite.color = Color.white;
            return;
        }

        if (wireSprite == null) return;

        if (!IsPowered)
        {
            // dim unpowered wires
            WireSpriteSetter(wireSprite, new Color(1, 1, 1, 0.15f));
        }
        else
        {
            // fully bright when powered
            WireSpriteSetter(wireSprite, Color.white);
        }
    }

    // small helper to set color
    private void WireSpriteSetter(SpriteRenderer sr, Color c)
    {
        sr.color = c;
    }

    /// <summary>
    /// Color-fill helper:
    /// Raycasts from each connection point to detect adjacent Wire objects.
    /// Adds newly discovered wires to the queue (BFS) to propagate power.
    /// </summary>
    public void GetConnectedWires(HashSet<Wire> visited, Queue<Wire> toCheck)
    {
        if (connectionPoints == null) return;

        foreach (var point in connectionPoints)
        {
            // small zero-distance raycast to detect colliders placed at the connection point
            RaycastHit2D[] hits = Physics2D.RaycastAll(point.position, Vector2.zero, RAYCAST_DISTANCE);

            for (int i = 0; i < hits.Length; i++)
            {
                Transform parentParent = null;
                if (hits[i].collider != null && hits[i].collider.transform.parent != null)
                    parentParent = hits[i].collider.transform.parent.parent;

                if (parentParent == null) continue;

                Wire connectedWire = parentParent.GetComponent<Wire>();

                if (connectedWire != null && !visited.Contains(connectedWire))
                {
                    visited.Add(connectedWire);
                    toCheck.Enqueue(connectedWire);
                }
            }
        }
    }
}
