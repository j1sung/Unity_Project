using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Player ���� ����
    [SerializeField] protected enum PlayerType { Human, Enemy }
    protected PlayerType playerType; // HumanPlayer���� EnemyPlayer���� ����

    // Player ���� ������
    protected int score; // Player ���� ����
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
        score = 0; // ���� �ʱ�ȭ
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
    public int getScore()
    {
        return score;
    }
    
    public Vector2 getPosition()
    {
        return initialPosition;
    }

    public Quaternion getRotation()
    {
        return initialRotation;
    }

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
            Dash();
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
    public void Dash() 
    {
        if (isPower == true)
        {
            StartCoroutine(DashSkill()); // Dash ���
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
    IEnumerator DashSkill()
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

        // ���̳� �˱� �浹 ó��
        // �浹�� ������Ʈ�� p_Attack �±����� Weapon���� Ȯ��  
        if (collision.gameObject.CompareTag("P_Attack") || collision.gameObject.CompareTag("Weapon"))  // �±׷� Ȯ��
        {
            // ���� ���� �̵� ���̶�� �浹 ����
            if (UI.instance.isRoundTransitioning)
            {
                Debug.Log("���� �̵� ���̹Ƿ� �浹 ����");
                return;
            }

            // ù ��° �浹�� ����ϰ� ó�� ����
            if (!UI.instance.isProcessingCollision) // isProcessingCollision == false �϶�
            {
                UI.instance.isProcessingCollision = true; // �浹 ó������ true ����
                UI.instance.isTwo = false; // �� ��° �浹 ���� �ʱ�ȭ

                if (collision.gameObject.CompareTag("P_Attack"))
                    Destroy(collision.gameObject);  // �浹�� P_Attack������ ����

                StartCoroutine(HandleCollisionWithDelay(this.playerType));
            }
            else // �̹� �浹�� ó�� ���̸� �ι�° �浹 ó��
            {
                if (UI.instance.playerSequence == (UI.Player)this.playerType) -> ���� ���� ��ġ���Ѿ� �ϴµ� ��ī��?
                {
                    UI.instance.isProcessingCollision = false; // �ٽ� �浹 ó������ false �ʱ�ȭ
                    UI.instance.isTwo = true; // �ι�° �÷��̾� �浹 �ν�


                    ScoreChange(); // ���� ���� �÷��̾� ���� ����

                    if (collision.gameObject.CompareTag("P_Attack"))
                        Destroy(collision.gameObject);  // �浹�� P_Attack������ ����
                }
                
            }
        }
    }

    // �ι�° �浹�� �߻��Ѵٸ� ù��° �浹�� ����ϸ� �ι�° �浹�� ��ٸ��� NextRound()�� ���� ȣ���ϴ� �ڷ�ƾ
    private IEnumerator HandleCollisionWithDelay(PlayerType firstPlayerType)
    {
        UI.instance.playerSequence = (UI.Player)firstPlayerType; // ù��° �浹 �÷��̾ human���� enemy���� ����

        // ������ �ð� ���� ����ϸ� �� ��° �浹 ���
        yield return new WaitForSeconds(UI.delayThreshold);

        ScoreChange(); // ���� ���� �÷��̾� ���� ����

        if (UI.instance.isTwo == true)
        {
            UI.instance.isTwo = false;
            Debug.Log("���ÿ� �浹�� �Ͼ���ϴ�!");
        }
        else
        {
            Debug.Log("�ѹ��� �浹�� �Ͼ���ϴ�!");
        }
        
        UI.instance.NextRound(); // UI�� NextRound ȣ��
    }

    private void ScoreChange() // ���� ���� �÷��̾� ���� ����
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // �� �ڽ��� ������ �ٸ� �÷��̾ ã��
            if (player != this.gameObject)
            {
                Player otherPlayer = player.GetComponent<Player>();
                if (otherPlayer != null)
                {
                    // �ٸ� �÷��̾��� score 1 ����
                    otherPlayer.score += 1;
                    break; // �ٸ� �÷��̾� �ϳ����� �����ϰ� ����
                }
            }
        }
    }
}
