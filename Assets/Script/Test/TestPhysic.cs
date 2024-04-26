using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhysic : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion quaternion1 = Quaternion.identity;
        Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.zero, new Vector3(1,1,1));
    }
}
