using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Player ���� ����
    [SerializeField] protected enum PlayerType { Human, Enemy }
    protected PlayerType playerType; // HumanPlayer���� EnemyPlayer���� ����

    // Player ���� ������
    protected int hp; // Player ü�� ����
    protected bool isPower; // Power ���� ����
    protected Vector2 initialPosition; // Player ��ġ �ʱ�ȭ ����
    protected Quaternion initialRotation; // Player ȸ���� �ʱ�ȭ ����

    // Player �̵� ������
    protected float speed = 3f; // �̵��ӵ�
    protected float inputValueY; // Y�� �Է� ��
    protected float rotationInput; // ȸ�� �Է� ��
    protected Rigidbody2D body; // Rigidbody2D ������Ʈ

    // Player ���� ����(������Ʈ)
    protected Collider2D swordCollider; // ���� ������Ʈ�� ���� �ݶ��̴�
    protected Collider2D playerCollider; // �÷��̾� �ڽ��� �ݶ��̴�

    // Sword ���� ����
    private Transform swordTransform;       // ���� Transform ����
    private Quaternion initialSwordRotation; // ���� �ʱ� ȸ���� ���� ����
    [SerializeField] private float rotationSpeed = 360f;  // ���� ȸ���ϴ� �ӵ�
    [SerializeField] private float attackCooldown = 1.0f; // ��Ÿ�� �ð�
    private bool canAttack = true;        // ���� ������ �������� üũ

    // P_Attack ���� ����
    public GameObject p_AttackPrefab; // P_Attack ������
    public Transform firePoint; // �߻� ��ġ(�÷��̾� ��)
    public float p_AttackSpeed = 10f; // �߻� �ӵ�

    // Ring ����
    private GameObject ring;

    protected virtual void Start()
    {
        // �÷��̾� �ʱ�ȭ(��ġ�� �ڽ� Ŭ�������� ���� ����)
        hp = 3; // ü�� �ʱ�ȭ
        isPower = false; // Power ��Ȱ��ȭ

        // ���� ������Ʈ���� "Ring"�� ã�Ƽ� ����
        ring = transform.Find("Ring").gameObject;
        ring.SetActive(false); // Power ������ Ring �Ⱥ��̰� ����

        // ���� �ݶ��̴��� ������ (���� ������Ʈ���� ã��)
        swordCollider = GetComponentInChildren<Collider2D>();

        // �÷��̾� �ڽ��� �ݶ��̴��� ������
        playerCollider = GetComponent<Collider2D>();

        // �÷��̾�� �ڽ��� �� ������ �浹�� ����
        if (swordCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(swordCollider, playerCollider, true);
        }

        // ���� ������Ʈ�� �� Transform ������
        if (playerType == PlayerType.Human)
        {
            Debug.Log("HumanType");
            swordTransform = transform.Find("Sword1");
            initialSwordRotation = swordTransform.localRotation; // ���� �ʱ� ȸ������ ���� (�� ������Ʈ�� ���� ȸ����)
        }
        else if(playerType == PlayerType.Enemy)
        {
            Debug.Log("EnemyType");
            swordTransform = transform.Find("Sword2"); 
            initialSwordRotation = swordTransform.localRotation; // ���� �ʱ� ȸ������ ���� (�� ������Ʈ�� ���� ȸ����)
        }
        else
        {
            Debug.LogError("Sword not found!");
        }
        // firePoint�� �÷��̾� �տ��� �߻�Ǵ� ��ġ�� ����
        firePoint = transform.Find("FirePoint"); // �÷��̾� �տ� �� ������Ʈ�� FirePoint ����

    }

    protected virtual void Initialize()
    {
        body = GetComponent<Rigidbody2D>(); // �ڽ� Ŭ������ Rigidbody2D ������Ʈ �޾ƿ�
    }

    // Get �޼����
    public int getHp()
    {
        return hp;
    }
    
    public Vector2 getPosition()
    {
        return initialPosition;
    }

    public Quaternion getRotation()
    {
        return initialRotation;
    }

    /*
    // �÷��̾��� ���� �ʱ�ȭ
    public void ResetPlayerState()
    {
        hp = 3; // ü�� �ʱ�ȭ
        isPower= false; // Power ��Ȱ��ȭ
        transform.position = initialPosition; // ��ġ�� �ʱ� ��ġ�� �ǵ���
        Debug.Log("Player state has been reset.");
    }
    */
    protected virtual void Update()
    {
        // Ű �Է� ó��
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (body != null)
        {
            // �̵� ���͸� ����Ͽ� ���� ȸ�� �������� �̵� (W/S Ű�� �̵�)
            Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
            body.velocity = moveDirection * inputValueY * speed;

            // RigidBody2D�� ȸ�� �� ���� (A/D Ű) - ���� ���� ���� ������
            body.MoveRotation(body.rotation + rotationInput * (speed+100) * Time.fixedDeltaTime);
        }
    }

    // Ű �Է� �޼���(����, ���)
    void HandleInput()
    {
        if(Input.GetKeyDown(KeyCode.Space) && canAttack)
        {
            Attack();
        }
        else if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            Move();
        }
    }

    // ���� �޼���
    public void Attack()
    {
        if (isPower == false)
        {
            StartCoroutine(DefaultAttack()); // �⺻ ����
        }
        else if (isPower == true)
        {
            StartCoroutine(P_Attack()); // P_Attack ����
        }
    }

    // ������ �޼���
    public void Move() 
    {
        if (isPower == true)
        {
            StartCoroutine(Dash()); // Dash ���
        }
    }

    // �Ϲݰ��� �ڷ�ƾ
    IEnumerator DefaultAttack()
    {
        canAttack = false;  // ���� �߿��� ���� �Ұ� ���·� ����
        float rotationAmount = 0f;  // ȸ���� ���� ����
        float targetRotation = 90f; // ���� ȸ���� ��ǥ ���� (90�� ȸ��)

        while (rotationAmount < targetRotation)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;  // ȸ�� �ӵ� ���
            rotationAmount += rotationStep;  // ���� ȸ�� ����

            // ���� ������ �������� 90�� ȸ��
            swordTransform.localRotation = initialSwordRotation * Quaternion.Euler(0f, 0f, Mathf.Min(rotationAmount, targetRotation));
            yield return null;  // �� ������ ���
        }
        // �ٽ� �ʱ� ��ġ�� ���ư�!
        swordTransform.localRotation = initialSwordRotation;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;  // ��Ÿ���� ���� �� �ٽ� ���� ����
    }
    
    // P_Attack �ڷ�ƾ
    IEnumerator P_Attack()
    {
        ring.SetActive(false); // Ring ��Ȱ��ȭ
        isPower = false; // Power �ٽ� �ʿ�

        // �߻�ü ���� (firePoint ��ġ����)                       
        GameObject p_Attack = Instantiate(p_AttackPrefab, firePoint.position, firePoint.rotation);

        // �߻�ü�� Rigidbody2D�� �پ� �ִٸ�, �ٶ󺸴� �������� �߻�
        Rigidbody2D rb = p_Attack.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // �÷��̾ ���� �������� �߻�
            rb.velocity = firePoint.up * p_AttackSpeed; // ������ �������� �߻� (2D ����)
        }

        // ���� �ð� �� �߻�ü ���� (1�� �� ���� ����)
        Destroy(p_Attack, 1.0f); // 1�� �ڿ� �߻�ü ����

        yield return new WaitForSeconds(1.0f); // 1�� ���
    }
    

    
    // Dash �ڷ�ƾ
    IEnumerator Dash()
    {
        ring.SetActive(false); // Ring ��Ȱ��ȭ
        yield return new WaitForSeconds(1.0f); // 1�� ���

        isPower = false; // Power �ٽ� �ʿ�
    }
    
   

    // �浹 ó�� ������
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �浹�� ������Ʈ�� Power���� Ȯ��
        if (collision.gameObject.CompareTag("Power") && isPower == false)
        {
            isPower = true;
            ring.SetActive(true);
            Debug.Log(isPower + " Power �׵�!");
            Destroy(collision.gameObject); // Power ����
        }

        // ���� �浹 ó��
        // �浹�� ������Ʈ�� Weapon �±׸� ������ �ִ��� Ȯ��
        if (collision.gameObject.CompareTag("Weapon"))
        {
            // ��� �÷��̾��� Player ��ũ��Ʈ�� ������ (���� �θ𿡼� Player ������Ʈ�� ã��)
            Player attackingPlayer = collision.GetComponentInParent<Player>();

            // �ڽŰ� �ٸ� Ÿ���� �÷��̾��� �˿� ���� ��쿡�� ü�� ����
            if (attackingPlayer != null && attackingPlayer.playerType != this.playerType)
            {
                this.hp -= 1;     // �� hp�� 1 ����
                UI.instance.NextRound(); // UI�� NextRound ȣ��
            }
        }

        // �浹�� ������Ʈ�� p_AttackPrefab ���������� Ȯ�� (�±� �Ǵ� �̸����� Ȯ�� ����)
        if (collision.gameObject.CompareTag("P_Attack"))  // �±׷� Ȯ��
        {
            this.hp -=1;  // �� hp�� 1 ����
            Destroy(collision.gameObject);  // �浹�� ������ ����
            UI.instance.NextRound(); // UI�� NextRound ȣ��
        }
    }
}
