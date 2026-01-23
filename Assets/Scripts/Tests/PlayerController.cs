using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f;
    public SpriteRenderer spriteRenderer;
    private bool moveRight;
    // Start is called before the first frame update
    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        moveRight = false;
        if (Input.GetAxis("Horizontal") > 0)
        {
            moveRight = true;
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            moveRight = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            this.transform.position += new Vector3(1, 0, 0) * speed * Time.deltaTime;
            moveRight = true;
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            this.transform.position -= new Vector3(1, 0, 0) * speed * Time.deltaTime;
            moveRight = false;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.transform.position += new Vector3(0, 1, 0) * speed * Time.deltaTime;
        }
        
        if (moveRight) 
        {
            spriteRenderer.flipX = false;   
        }
        else
        {
            spriteRenderer.flipX = true;
        }
    }
}
