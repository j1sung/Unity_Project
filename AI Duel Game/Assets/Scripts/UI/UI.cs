using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UI instance;

    // UI - 게임 텍스트 출력 변수
    [SerializeField] private Text gameText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text playerScoreText;
    [SerializeField] private Text aiScoreText;

    // Player 객체 참조 변수
    public HumanPlayer humanPlayer;
    public EnemyPlayer enemyPlayer;

    // Player 충돌 순서
    public enum Player { FirstPlayer, SecondPlayer }
    public Player playerSequence;

    // UI - Power 프리펩 출력 관련
    public GameObject PowerPrefab;  // Power 프리팹
    [SerializeField] private int PowerNumber = 7;  // Power 7개 생성
    public Camera mainCamera;    // 카메라 참조
    int humanScore;  // 플레이어 score 개수
    int aiScore;      // AI score 개수

    // Score 전환 관련
    private int currentScore; // 현재 스코어

    // 플레이어 충돌(무승부) 분기 관련
    public bool isProcessingCollision = false; // 충돌 처리 중인지 여부
    public const float delayThreshold = 0.1f;  // 100ms 딜레이 (동시 충돌 여부 확인)
    public bool isTwo = false; // 두번째 충돌 발생 여부

    // Round 전환 관련
    private int currentRound; // 현재 라운드
    public bool isRoundTransitioning = false; // 라운드 이동 중인지 여부

    // 게임 상태 관련
    private bool gameStarted; // 게임이 시작됐는지 여부 확인하는 변수
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
        {
            instance = this;
            Debug.Log("새로운 UI 인스턴스가 생성되었습니다.");
        }
    }

    void Start()
    {
        Debug.Log("Start");
        // Round 초기화&출력
        currentRound = 1; // 현재 라운드 초기화
        UpdateRoundText();

        // Player Score 출력
        AssignPlayers();
        UpdateScoreText();

        // 게임 초기 설정
        gameStarted = false; // 게임 시작 유무
        Time.timeScale = 0;  // 시간 정지 (모든 게임 오브젝트 멈춤)
    }

    // Player 객체 연결 & Score정보 휙득
    void AssignPlayers()
    {
        humanPlayer = FindObjectOfType<HumanPlayer>();  // HumanPlayer 스크립트를 찾아서 할당
        enemyPlayer = FindObjectOfType<EnemyPlayer>();  // EnemyPlayer 스크립트를 찾아서 할당
        humanScore = humanPlayer.getScore();
        aiScore = enemyPlayer.getScore();
    }

    // 플레이어들 스코어 텍스트를 업데이트하는 함수
    void UpdateScoreText()
    {
        playerScoreText.text = "Score: " + humanScore;
        aiScoreText.text = "Score: " + aiScore;

    }
    

    // Power 프리펩 화면 생성!
    void SpawnPowers()
    {
        ClearPower();

        Vector2 mapCenter = new Vector2(0, 0);  // 맵 중심
        float mapRadius = 3.5f;                   // 맵 반지름

        for (int i = 0; i < PowerNumber; i++)
        {
            // 원형 맵 내부의 랜덤 위치에 생성
            Vector2 powerPosition = GetRandomPositionInCircle(mapCenter, mapRadius);
            Instantiate(PowerPrefab, powerPosition, Quaternion.identity);
        }
    }
    public Vector2 GetRandomPositionInCircle(Vector2 center, float radius)
    {
        // 랜덤한 각도 (0 ~ 360도) 생성
        float angle = Random.Range(0f, Mathf.PI * 2);

        // 랜덤한 거리 (0 ~ radius) 생성
        float distance = Random.Range(0f, radius);

        // Polar Coordinates를 Cartesian Coordinates로 변환
        float x = center.x + Mathf.Cos(angle) * distance;
        float y = center.y + Mathf.Sin(angle) * distance;

        return new Vector2(x, y);
    }

    void ClearPower()
    {
        // "HP" 태그를 가진 오브젝트를 모두 찾아서 삭제
        GameObject[] existingPower = GameObject.FindGameObjectsWithTag("Power");
        foreach (GameObject power in existingPower)
        {
            Destroy(power);
        }
    }

    private void Update()
    {
        // 엔터키 누르면 게임 시작! (gameStarted == True)
        if (!gameStarted && Input.GetKeyDown(KeyCode.Return)) // 매 프레임 눌렸는지 확인해야 함
        {
            StartGame();
        }
    }

    // 게임 시작
    public void StartGame()
    {
        gameStarted = true;
        SpawnPowers();     // Power 생성됨!
        StartCoroutine(ShowReadyFight()); // 게임 시작 안내
        
    }

    IEnumerator ShowReadyFight()
    {
        yield return new WaitForSecondsRealtime(1.0f);  // 1초 대기
        // "Ready?" 텍스트 표시
        gameText.text = "Ready?"; 
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // "Fight!" 텍스트 표시
        gameText.text = "Fight!";
        yield return new WaitForSecondsRealtime(1.0f);  // 1초 대기

        // "Fight!" 텍스트 사라지게 하려면 빈 문자열로 설정
        gameText.text = "";

        Time.timeScale = 1; // 시간 재개 (게임 정상 속도로 진행)
    }

    // 한 라운드가 끝날 때 호출하여 다음 라운드를 진행 함수
    public void NextRound()  // 공격에 성공한 플레이어의 점수를 1개 올리고 호출됨
    {
        isProcessingCollision = false;

        // "P_Attack" 태그를 가진 모든 게임 오브젝트를 찾아 삭제
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag("P_Attack");

        foreach (GameObject obj in objectsWithTag)
        {
            Destroy(obj); // 각 오브젝트를 삭제
        }

        // 각 플레이어 Score 정보 다시 받아옴
        AssignPlayers();
        
        // 둘 중 하나가 먼저 3점에 도달했고, 점수가 다르면 이긴 상황 (승패 결정)
        if ((humanScore >= 3 || aiScore >= 3) && (humanScore != aiScore))
        {
            StartCoroutine(ShowEnd()); // 게임 결과 출력과 새 게임 시작!

        }
        else // 비긴 상황이거나, 둘 다 3점 미만인 경우 (게임이 아직 끝나지 않음)
        {
            StartCoroutine(ShowKO());
            currentRound++;   // 라운드 수 증가
            ResetGameState(); // 게임 상태를 초기화 시킴
            UpdateRoundText(); // 다음 라운드 표기
        }
    }

    IEnumerator ShowKO()
    {
        Time.timeScale = 0; // 시간 정지 (모든 게임 오브젝트 멈춤)

        // "KO!" 텍스트 표시
        gameText.text = "KO!";
        yield return new WaitForSecondsRealtime(1.0f);  // 1초 대기

        // 텍스트 사라지게
        gameText.text = "";

    }

    IEnumerator ShowEnd()
    {
        Time.timeScale = 0; // 시간 정지 (모든 게임 오브젝트 멈춤)

        if (humanScore > aiScore) // HumanPlayer가 이기면
        {
            // "You Win!" 텍스트 표시
            gameText.text = "You Win!";
        }
        else // HumanPlayer가 지면
        {
            // "You Lose!" 텍스트 표시
            gameText.text = "You Lose!";
        }
        yield return new WaitForSecondsRealtime(2.0f);  // 2초 대기

        // 텍스트 사라지게
        gameText.text = "";

        Debug.Log("All rounds completed!");
        ResetGame(); // 게임 리셋
    }

    // 게임 상태만 초기화 - UI
    void ResetGameState()
    {
        // 수정된 Score 다시 출력
        UpdateScoreText();

        // 플레이어 위치 초기화
        humanPlayer.transform.position = humanPlayer.getPosition();
        enemyPlayer.transform.position = enemyPlayer.getPosition();

        // 플레이어 방향 초기화
        humanPlayer.transform.rotation = humanPlayer.getRotation();
        enemyPlayer.transform.rotation = enemyPlayer.getRotation();

        StartGame();// GameText 다시 출력& 게임시작!

    }

    // 라운드 텍스트를 업데이트하는 함수
    void UpdateRoundText()
    {
        roundText.text = "Round " + currentRound;
    }

    // 씬 초기화
    void ResetGame()
    {
        // <방법 3가지>
        // 1) 특정 오브젝트만 비활성화 -> 활성화 그 오브젝트만 Start() 재실행 -> 해당 오브젝트 스크립트만 초기화!

        // 2) 모든 오브젝트를 비활성화 -> 활성화 (이건 씬 초기화보다 느릴지도)

        // 3) 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // 게임 내역(데이터)을 저장하고 씬을 초기화한 후 다시 로드할 수 있음
    }

}
