using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerTest : MonoBehaviour
{
    public float speed;
    public SpriteRenderer sr;
    private bool moveRight;
    
    // 跳跃相关参数
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float groundDrag = 0.1f;
    private float velocityY = 0f;
    private bool isGrounded = false;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        moveRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        // 水平移动
        float deltaX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        this.transform.position += new Vector3(deltaX, 0, 0);
        Debug.Log("DeltaX: " + deltaX);
        moveRight = deltaX > 0;

        if (moveRight) { 
            sr.flipX = false;
        }
        else
        {
            sr.flipX = true;
        }
        
        // 地面检测
        CheckGround();
        
        // 跳跃输入
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        // 应用重力
        ApplyGravity();
    }
    
    /// <summary>
    /// 检测是否在地面上
    /// </summary>
    private void CheckGround()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Vector3 checkPos = transform.position + Vector3.down * (collider.bounds.extents.y + groundCheckDistance);
            Collider2D hitCollider = Physics2D.OverlapCircle(checkPos, 0.05f, groundLayer);
            isGrounded = hitCollider != null;
        }
        else
        {
            // 如果没有 Collider2D，使用简单的 Raycast 检测
            isGrounded = Physics2D.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        }
    }
    
    /// <summary>
    /// 执行跳跃
    /// </summary>
    private void Jump()
    {
        velocityY = jumpForce;
        isGrounded = false;
        Debug.Log("跳跃！");
    }
    
    /// <summary>
    /// 应用重力和Y轴速度
    /// </summary>
    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocityY += gravity * Time.deltaTime;
        }
        else if (velocityY < 0)
        {
            velocityY = 0f;
        }
        
        float deltaY = velocityY * Time.deltaTime;
        transform.position += new Vector3(0, deltaY, 0);
    }
}
