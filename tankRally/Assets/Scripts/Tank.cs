using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour
{
    private const float MoveDelta = 0.2f;
    private const float RotateDelta = 1f;

	void Update () {
	    
        if(Input.GetKey(KeyCode.UpArrow))
            transform.Translate(new Vector3(0, 0, -MoveDelta));
        if (Input.GetKey(KeyCode.DownArrow))
            transform.Translate(new Vector3(0, 0, MoveDelta));
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up, RotateDelta);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up, -RotateDelta);
	}
}
