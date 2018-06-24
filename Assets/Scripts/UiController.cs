using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiController : MonoBehaviour {

    public Vector3 goalPosition;
    public Vector3 startPosition;

    bool isMoving;
    bool movingOut;
    public bool shown;

    void Start () {
        GetComponent<RectTransform>().position += startPosition;
        shown = false;
	}
	
	void Update () {
		if (isMoving && transform.position != goalPosition)
        {
            float minspeed = 1;
            Vector3 speed = (goalPosition - transform.position) / 2f;
            if (speed.sqrMagnitude <= minspeed)
            {
                transform.position = goalPosition;
            } else
            {
                transform.position += speed;
            }
        }

        if (movingOut)
        {
            transform.position += new Vector3(0, -5, 0);
            if (transform.position.y <= -100)
            {
                Destroy(this);
            }
        }
	}

    public void Show()
    {
        goalPosition = transform.position - startPosition;
        isMoving = true;
        shown = true;
    }

    public void Hide()
    {
        goalPosition = transform.position + startPosition;
        isMoving = true;
        shown = false;
    }

    public void MoveOut()
    {
        movingOut = true;
    }
}
