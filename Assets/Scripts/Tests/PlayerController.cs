//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
//public class PlayerController : MonoBehaviour
//{
//    public float speed = 10.0f;
//    public float jumpForce = 5.0f;           // 跳跃力度
//    public SpriteRenderer spriteRenderer;
//    private bool moveRight;
//    //private Rigidbody2D rb;
//    //private bool isGrounded = true;          // 简单的落地标记
//    // Start is called before the first frame update
//    void Start()
//    {
//        if (spriteRenderer == null)
//        {
//            spriteRenderer = GetComponent<SpriteRenderer>();
//        }

//        //rb = GetComponent<Rigidbody2D>();

//        moveRight = false;
//        if (Input.GetAxis("Horizontal") > 0)
//        {
//            moveRight = true;
//        }
//        else if (Input.GetAxis("Horizontal") < 0)
//        {
//            moveRight = false;
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // 水平移动（保持原来的写法）
//        if (Input.GetAxisRaw("Horizontal") > 0)
//        {
//            this.transform.position += new Vector3(1, 0, 0) * speed * Time.deltaTime;
//            moveRight = true;
//        }
//        else if (Input.GetAxisRaw("Horizontal") < 0)
//        {
//            this.transform.position -= new Vector3(1, 0, 0) * speed * Time.deltaTime;
//            moveRight = false;
//        }

//        // 空格跳跃，只在着地时才能起跳
//        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
//        {
//            Debug.Log("Jump pressed");
//            isGrounded = false;
//            rb.velocity = new Vector2(rb.velocity.x, 0f); // 清空当前竖直速度，保证跳跃一致
//            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
//        }

//        // 根据朝向翻转
//        if (moveRight)
//        {
//            spriteRenderer.flipX = false;
//        }
//        else
//        {
//            spriteRenderer.flipX = true;
//        }
//    }

//    //// 简单的落地检测：与任何碰撞体接触都算落地
//    //private void OnCollisionEnter2D(Collision2D collision)
//    //{
//    //    // 如果想更精细，可以判断 collision.gameObject.tag == "Ground"
//    //    isGrounded = true;
//    //}
//}
