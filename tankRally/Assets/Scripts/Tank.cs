using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour
{
    private float _moveDelta;
    private float _rotateDelta;

    void Awake()
    {
        _moveDelta = DataManager.GetInstance().GetTankVelocity();
        _rotateDelta = DataManager.GetInstance().GetTankAngularVelocity();
    }

	void Update () {
	    
        if(Input.GetKey(KeyCode.UpArrow))
            transform.Translate(new Vector3(0, 0, -_moveDelta));
        if (Input.GetKey(KeyCode.DownArrow))
            transform.Translate(new Vector3(0, 0, _moveDelta));
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.Rotate(Vector3.up, _rotateDelta);
        if (Input.GetKey(KeyCode.RightArrow))
            transform.Rotate(Vector3.up, -_rotateDelta);
	}
}
