using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    public BoardManager boardManager;
    public PlayerController playerController;
    public TurnManager turnManager { get; private set;}

    public UIDocument UIDoc;
    private VisualElement m_gameOverPanel;
    private Label m_gameOverMessage;
    private Label m_foodLabel;

    [SerializeField]
    private int m_food = 100;
    private int m_gameLevel = 1;
    //Difficulty scaling variables
    private int width = 16;
    private int height = 16;
    private float totalArea;
    //walls
    private int minWalls = 6;
    private int maxWalls = 10;
    //food
    private int minFood = 3;
    private int maxFood = 6;
    private int foodLevel = 2;
    //enemies
    private int minEnemy = 1;
    private int maxEnemy = 2;
    private int enemyTypes = 1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        turnManager = new TurnManager();
        turnManager.OnTick += OnTurnHappen;

        m_foodLabel = UIDoc.rootVisualElement.Q<Label>("FoodLabel");
        m_gameOverPanel = UIDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
        m_gameOverMessage = m_gameOverPanel.Q<Label>("GameOverMessage");

        m_gameOverPanel.style.visibility = Visibility.Hidden;
        m_foodLabel.text = $"Food: {m_food}";

        StartNewGame();
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood( int amount)
    {
        m_food += amount;
        m_foodLabel.text = $"Food: {m_food}";

        if (m_food <= 0)
        {
            playerController.audioController.PlayDeath();
            playerController.GameOver();
            m_gameOverPanel.style.visibility = Visibility.Visible;
            m_gameOverMessage.text = "Game Over!\n\nYou traveled through " + m_gameLevel + " levels";
        }
    }

    public void NewLevel()
    {
        if (m_gameLevel <= 10)
        {
            width = Random.Range(16, 20);
            height = Random.Range(16, 20);
            totalArea = (width-10) * (height-10); //This one almost got me, its an indent of 5 on BOTH sides to the OOB walls so we need to subtract 10 not 5
            minWalls = Mathf.FloorToInt(totalArea * 0.25f);
            maxWalls = Mathf.FloorToInt(totalArea * 0.35f);
            minFood = Mathf.FloorToInt(totalArea * 0.04f);
            maxFood = Mathf.FloorToInt(totalArea * 0.06f);
            foodLevel = 2;
            minEnemy = 1; maxEnemy = 3;
            enemyTypes = 1;
        }
        else if (m_gameLevel <= 20 && m_gameLevel > 10)
        {
            width = Random.Range(25, 50);
            height = Random.Range(25, 50);
            totalArea = (width - 10) * (height - 10); 
            minWalls = Mathf.FloorToInt(totalArea * 0.30f);
            maxWalls = Mathf.FloorToInt(totalArea * 0.40f);
            minFood = Mathf.FloorToInt(totalArea * 0.01f);
            maxFood = Mathf.FloorToInt(totalArea * 0.03f);
            foodLevel = 3;
            minEnemy = 3; maxEnemy = 7;
            enemyTypes = 2;
        }
        else if (m_gameLevel <= 50 && m_gameLevel > 20)
        {
            width = Random.Range(m_gameLevel, 100);
            height = Random.Range(m_gameLevel, 100);
            totalArea = (width - 10) * (height - 10); //This one almost got me, its an indent of 5 on BOTH sides to the OOB walls so we need to subtract 10 not 5
            minWalls = Mathf.FloorToInt(totalArea * 0.35f);
            maxWalls = Mathf.FloorToInt(totalArea * 0.45f);
            minFood = Mathf.FloorToInt(totalArea * 0.01f);
            maxFood = Mathf.FloorToInt(totalArea * 0.015f);
            foodLevel = 4;
            minEnemy = 7; maxEnemy = 10;
            enemyTypes = 3; 
        }

        StopAllCoroutines();
        turnManager.currentTurn = 0;
        boardManager.Clean();
        boardManager.Init(width, height, minWalls, maxWalls, minFood, maxFood, foodLevel, minEnemy, maxEnemy, enemyTypes);
        playerController.Spawn(boardManager, new Vector2Int(6, 6));

        m_gameLevel += 1;
    }

    public void StartNewGame()
    {
        m_gameOverPanel.style.visibility = Visibility.Hidden;

        m_gameLevel = 1;
        m_food = 100;
        turnManager.currentTurn = 0;
        m_foodLabel.text = $"Food: {m_food}";
        width = 16;
        height = 16;

        boardManager.Clean();
        boardManager.Init(width, height, minWalls, maxWalls, minFood, maxFood, foodLevel, minEnemy, maxEnemy, enemyTypes);

        playerController.Init();
        playerController.Spawn(boardManager, new Vector2Int(6, 6));
    }
}