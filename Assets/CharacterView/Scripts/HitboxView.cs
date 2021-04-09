using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HitstunConstants;

public class HitboxView : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    public void setRect(float x, float y, float z, bool facingRight, int[] box)
    {
        //box pivot is bottom left
        if (facingRight)
        {
            transform.position = new Vector3(x + box[0] / Constants.SCALE, y + box[2] / Constants.SCALE, z);
        }
        else
        {
            transform.position = new Vector3(x - box[1] / Constants.SCALE, y + box[2] / Constants.SCALE, z);
        }
        transform.localScale = new Vector3(Mathf.Abs(box[0] - box[1]) / 10, Mathf.Abs(box[2] - box[3]) / 10, 1);
    }

}
