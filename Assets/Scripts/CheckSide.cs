using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckSide : MonoBehaviour
{
    public LayerMask ignoreMe;
    private bool sideFound = false;
 
    private void FixedUpdate()
    {
        if (GetComponent<Rigidbody>().velocity == Vector3.zero && !sideFound)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (!Physics.Raycast(transform.GetChild(i).position, transform.GetChild(i).TransformDirection(Vector3.up), Mathf.Infinity, ~ignoreMe))
                {
                    sideFound = true;

                    //print(transform.GetChild(i).name);
                    GameManager.instance.CheckEvenOrOdd(int.Parse(transform.GetChild(i).name), Random.Range(0, 1000));
                    break;
                }
            }                            
        }
    }
}
