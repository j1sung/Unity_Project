using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Player 종류 설정
    [SerializeField] protected enum PlayerType { Human, Enemy }
    protected PlayerType playerType; // HumanPlayer인지 EnemyPlayer인지 구분

    // Player 상태 변수들
    protected int score; // Player 점수 변수
    protected bool isPower; // Power 존재 변수
    protected Vector2 initialPosition; // Player 위치 초기화 변수
    protected Quaternion initialRotation; // Player 회전값 초기화 변수

    // Player 이동 변수들
    protected float speed = 3f; // 이동속도
    protected float inputValueY; // Y축 입력 값
    protected float rotationInput; // 회전 입력 값
    protected Rigidbody2D body; // Rigidbody2D 컴포넌트

    // Player 하위 무기(오브젝트)
    protected Collider2D swordCollider; // 하위 오브젝트인 검의 콜라이더
    protected Collider2D playerCollider; // 플레이어 자신의 콜라이더

    // Sword 공격 관련
    private Transform swordTransform;       // 검의 Transform 저장
    private Quaternion initialSwordRotation; // 검의 초기 회전값 저장 변수
    [SerializeField] private float rotationSpeed = 360f;  // 검이 회전하는 속도
    [SerializeField] private float attackCooldown = 1.0f; // 쿨타임 시간
    private bool canAttack = true;        // 공격 가능한 상태인지 체크

    // P_Attack 공격 관련
    public GameObject p_AttackPrefab; // P_Attack 프리팹
    public Transform firePoint; // 발사 위치(플레이어 앞)
    public float p_AttackSpeed = 10f; // 발사 속도

    // Ring 상태
    private GameObject ring;

    protected virtual void Start()
    {
        // 플레이어 초기화(위치는 자식 클래스에서 각각 정의)
        score = 0; // 점수 초기화
        isPower = false; // Power 비활성화

        // 하위 오브젝트에서 "Ring"을 찾아서 참조
        ring = transform.Find("Ring").gameObject;
        ring.SetActive(false); // Power 없으면 Ring 안보이게 설정

        // 검의 콜라이더를 가져옴 (하위 오브젝트에서 찾음)
        swordCollider = GetComponentInChildren<Collider2D>();

        // 플레이어 자신의 콜라이더를 가져옴
        playerCollider = GetComponent<Collider2D>();

        // 플레이어와 자신의 검 사이의 충돌을 무시
        if (swordCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(swordCollider, playerCollider, true);
        }

        // 하위 오브젝트인 검 Transform 가져옴
        if (playerType == PlayerType.Human)
        {
            Debug.Log("HumanType");
            swordTransform = transform.Find("Sword1");
            initialSwordRotation = swordTransform.localRotation; // 검의 초기 회전값을 저장 (검 오브젝트의 로컬 회전값)
        }
        else if(playerType == PlayerType.Enemy)
        {
            Debug.Log("EnemyType");
            swordTransform = transform.Find("Sword2"); 
            initialSwordRotation = swordTransform.localRotation; // 검의 초기 회전값을 저장 (검 오브젝트의 로컬 회전값)
        }
        else
        {
            Debug.LogError("Sword not found!");
        }
        // firePoint는 플레이어 앞에서 발사되는 위치를 설정
        firePoint = transform.Find("FirePoint"); // 플레이어 앞에 빈 오브젝트로 FirePoint 설정

    }

    protected virtual void Initialize()
    {
        body = GetComponent<Rigidbody2D>(); // 자식 클래스의 Rigidbody2D 컴포넌트 받아옴
    }

    // Get 메서드들
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
        // 키 입력 처리
        HandleInput();
    }

    private void FixedUpdate()
    {
        if (body != null)
        {
            // 이동 벡터를 계산하여 현재 회전 방향으로 이동 (W/S 키로 이동)
            Vector2 moveDirection = new Vector2(-Mathf.Sin(body.rotation * Mathf.Deg2Rad), Mathf.Cos(body.rotation * Mathf.Deg2Rad));
            body.velocity = moveDirection * inputValueY * speed;

            // RigidBody2D의 회전 값 적용 (A/D 키) - 직접 각도 값을 더해줌
            body.MoveRotation(body.rotation + rotationInput * (speed+100) * Time.fixedDeltaTime);
        }
    }

    // 키 입력 메서드(공격, 대시)
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

    // 공격 메서드
    public void Attack()
    {
        if (isPower == false)
        {
            StartCoroutine(DefaultAttack()); // 기본 공격
        }
        else if (isPower == true)
        {
            StartCoroutine(P_Attack()); // P_Attack 공격
        }
    }

    // 움직임 메서드
    public void Dash() 
    {
        if (isPower == true)
        {
            StartCoroutine(DashSkill()); // Dash 사용
        }
    }

    // 일반공격 코루틴
    IEnumerator DefaultAttack()
    {
        canAttack = false;  // 공격 중에는 공격 불가 상태로 변경
        float rotationAmount = 0f;  // 회전한 각도 누적
        float targetRotation = 90f; // 검이 회전할 목표 각도 (90도 회전)

        while (rotationAmount < targetRotation)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;  // 회전 속도 계산
            rotationAmount += rotationStep;  // 누적 회전 각도

            // 현재 각도를 기준으로 90도 회전
            swordTransform.localRotation = initialSwordRotation * Quaternion.Euler(0f, 0f, Mathf.Min(rotationAmount, targetRotation));
            yield return null;  // 한 프레임 대기
        }
        // 다시 초기 위치로 돌아감!
        swordTransform.localRotation = initialSwordRotation;

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;  // 쿨타임이 끝난 후 다시 공격 가능
    }
    
    // P_Attack 코루틴
    IEnumerator P_Attack()
    {
        ring.SetActive(false); // Ring 비활성화
        isPower = false; // Power 다시 필요

        // 발사체 생성 (firePoint 위치에서)                       
        GameObject p_Attack = Instantiate(p_AttackPrefab, firePoint.position, firePoint.rotation);

        // 발사체에 Rigidbody2D가 붙어 있다면, 바라보는 방향으로 발사
        Rigidbody2D rb = p_Attack.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // 플레이어가 보는 방향으로 발사
            rb.velocity = firePoint.up * p_AttackSpeed; // 오른쪽 방향으로 발사 (2D 기준)
        }

        // 일정 시간 후 발사체 삭제 (1초 후 삭제 예시)
        Destroy(p_Attack, 1.0f); // 1초 뒤에 발사체 삭제

        yield return new WaitForSeconds(1.0f); // 1초 대기

    }
    

    
    // Dash 코루틴
    IEnumerator DashSkill()
    {
        ring.SetActive(false); // Ring 비활성화
        yield return new WaitForSeconds(1.0f); // 1초 대기

        isPower = false; // Power 다시 필요
    }


    // 충돌 처리 구현부
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 Power인지 확인
        if (collision.gameObject.CompareTag("Power") && isPower == false)
        {
            isPower = true;
            ring.SetActive(true);
            Debug.Log(isPower + " Power 휙득!");
            Destroy(collision.gameObject); // Power 삭제
        }

        // 검이나 검기 충돌 처리
        // 충돌한 오브젝트가 p_Attack 태그인지 Weapon인지 확인  
        if (collision.gameObject.CompareTag("P_Attack") || collision.gameObject.CompareTag("Weapon"))  // 태그로 확인
        {
            // 만약 라운드 이동 중이라면 충돌 무시
            if (UI.instance.isRoundTransitioning)
            {
                Debug.Log("라운드 이동 중이므로 충돌 무시");
                return;
            }

            // 첫 번째 충돌을 기록하고 처리 시작
            if (!UI.instance.isProcessingCollision) // isProcessingCollision == false 일때
            {
                UI.instance.isProcessingCollision = true; // 충돌 처리여부 true 변경
                UI.instance.isTwo = false; // 두 번째 충돌 여부 초기화

                if (collision.gameObject.CompareTag("P_Attack"))
                    Destroy(collision.gameObject);  // 충돌한 P_Attack프리펩 제거

                StartCoroutine(HandleCollisionWithDelay(this.playerType));
            }
            else // 이미 충돌을 처리 중이면 두번째 충돌 처리
            {
                if (UI.instance.playerSequence == (UI.Player)this.playerType) -> 여기 지금 일치시켜야 하는데 어카지?
                {
                    UI.instance.isProcessingCollision = false; // 다시 충돌 처리여부 false 초기화
                    UI.instance.isTwo = true; // 두번째 플레이어 충돌 인식


                    ScoreChange(); // 공격 성공 플레이어 점수 변동

                    if (collision.gameObject.CompareTag("P_Attack"))
                        Destroy(collision.gameObject);  // 충돌한 P_Attack프리펩 제거
                }
                
            }
        }
    }

    // 두번째 충돌이 발생한다면 첫번째 충돌은 대기하며 두번째 충돌을 기다리고 NextRound()를 최종 호출하는 코루틴
    private IEnumerator HandleCollisionWithDelay(PlayerType firstPlayerType)
    {
        UI.instance.playerSequence = (UI.Player)firstPlayerType; // 첫번째 충돌 플레이어가 human인지 enemy인지 구별

        // 딜레이 시간 동안 대기하며 두 번째 충돌 대기
        yield return new WaitForSeconds(UI.delayThreshold);

        ScoreChange(); // 공격 성공 플레이어 점수 변동

        if (UI.instance.isTwo == true)
        {
            UI.instance.isTwo = false;
            Debug.Log("동시에 충돌이 일어났습니다!");
        }
        else
        {
            Debug.Log("한번의 충돌이 일어났습니다!");
        }
        
        UI.instance.NextRound(); // UI의 NextRound 호출
    }

    private void ScoreChange() // 공격 성공 플레이어 점수 변동
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // 나 자신을 제외한 다른 플레이어를 찾기
            if (player != this.gameObject)
            {
                Player otherPlayer = player.GetComponent<Player>();
                if (otherPlayer != null)
                {
                    // 다른 플레이어의 score 1 증가
                    otherPlayer.score += 1;
                    break; // 다른 플레이어 하나에만 적용하고 종료
                }
            }
        }
    }
}
