using UnityEngine;

public class Wire : MonoBehaviour
{
    public int WireType = 0;
    private int rotationState = 0;

    public void Init(int type)
    {
        WireType = type;
    }

    public void RotateWire()
    {
        rotationState = (rotationState + 1) % 4;
        transform.eulerAngles = new Vector3(0, 0, rotationState * 90);
    }
}
