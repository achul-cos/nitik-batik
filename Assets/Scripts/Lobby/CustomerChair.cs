using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerChair : MonoBehaviour
{
    public int index;
    public Vector3 originalPosition => transform.position;
    public Vector3 offPosition
    {
        get
        {
            return new Vector3(11f, originalPosition.y, originalPosition.z);
        }
    }
}
