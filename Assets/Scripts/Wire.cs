using UnityEngine;
using System.Collections.Generic;

public class Wire : MonoBehaviour
{
    public int WireType = 0;

    [SerializeField] private Transform[] wirePrefabs;
    private Transform currentWire;

    private List<Transform> connectionPoints;

    private int rotationState = 0;

    public void Init(int encoded)
    {
        WireType = encoded % 10;
        int rot = encoded / 10;

        currentWire = Instantiate(wirePrefabs[WireType], transform);
        currentWire.localEulerAngles = new Vector3(0, 0, rot * 90);
    }


    public void RotateWire()
    {
        rotationState = (rotationState + 1) % 4;

        currentWire.eulerAngles = new Vector3(0, 0, rotationState * 90);
    }
}
