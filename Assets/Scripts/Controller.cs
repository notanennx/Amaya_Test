using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using System;
using DG.Tweening;
using UnityEngine.UI;

/*
    Не успел разрулить партикл систему.
    Не допёр, как поступить с многовенным удалением кнопок, из-за чего не посмотреть bounce анимацию.
*/

public class Controller : MonoBehaviour
{
    [System.Serializable]
    public class Skin
    {
        public string skinName;
        public Sprite[] spriteList;
    }

    // Access
    [Header("Access")]
    public Text taskDescription;
    public Image fadeScreen;
    public GameObject restartButton;
    public GameObject cellPrefab;

    // Tasking
    [Header("Tasking")]
    public Sprite taskSprite;
    private int taskProgress;
    private bool taskEnabled;
    public List<Sprite> taskPool = new List<Sprite>(); // Пул пополняется таскСпрайтами.

    // Reskins
    [Header("Reskins")]
    public Skin[] skinsLibrary;
    private Sprite[] usedSprites;

    // Miscellaneous
    //[Header("Miscellaneous")]
    private List<Sprite> spritePool = new List<Sprite>();
    private int rowSize = 3;
    private float cellGap = 32f;
    private float cellSize = 96f;

    // Start
    void Start()
    {
        DOTween.Init();

        taskProgress = 1;
        Initialize(1, true);
    }

    // Static
    public static Controller i;
    void Awake()
    {
        i = this;
    }

    // Initialize
    void Initialize(int level, bool firstTime)
    {
        // Select
        int skinNum = Random.Range(0, skinsLibrary.Length);

        // Filling
        spritePool.Clear();
        for (int i=0; i < skinsLibrary[skinNum].spriteList.Length; i++)
        {
            spritePool.Add(skinsLibrary[skinNum].spriteList[i]);
        }

        // Bulding
        switch (level)
        {
            // 3 Cells
            case 1:
                BuildLevel(3);
            break;

            // 6 Cells
            case 2:
                BuildLevel(6);
            break;

            // 9 Cells
            case 3:
                BuildLevel(9);
            break;
        }

        // First Time
        if (firstTime)
        {
            taskDescription.DOFade(1, 2f);
            GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
            for(var i=0 ; i<cells.Length ; i++)
            {
                cells[i].transform.DOPunchScale(new Vector3(0.1f, 0.1f, 1f), 0.6f, 8, 1f);
            }
        }
    }

    // Build Level
    private List<Sprite> curPool = new List<Sprite>();
    void BuildLevel(int amount)
    {
        // Clear
        DestroyCells();
        curPool.Clear();

        // Task
        taskEnabled = true;

        // Build
        int curRow = 0;
        int curNum = 0;
        int curSkip = (amount/rowSize) - 1;
        for (int i=0; i<amount; i++)
        {
            float xPos = curNum*(cellSize+cellGap) - 0.5f*((rowSize-1)*(cellSize+cellGap));
            float yPos = -curRow*(cellSize+cellGap) + 0.5f*(curSkip)*(cellSize+cellGap);

            // Create
            GameObject newCell = CreateCell(xPos, yPos);

            // Next row
            curNum += 1;
            if (curNum >= rowSize)
            {
                curNum = 0;
                curRow += 1;
            }
        }

        // Tasking
        SelectTask();
    }

    // Selects a task
    void SelectTask()
    {
        taskSprite = curPool[Random.Range(0, curPool.Count)];
        taskDescription.text = "Find "+taskSprite.name;

        taskPool.Add(taskSprite);
    }

    // Grab Sprite
    Sprite SelectSprite()
    {
        // Select
        int selectedNum = Random.Range(0, spritePool.Count);
        Sprite selectedSprite = spritePool[selectedNum];

        // Remove
        spritePool.Remove(selectedSprite);
        return selectedSprite;
    }

    // Create Cell
    GameObject CreateCell(float x, float y)
    {
        GameObject newCell = Instantiate(cellPrefab, transform.position, transform.rotation) as GameObject;
            newCell.transform.SetParent(GameObject.Find("Cells").transform, false);
            newCell.transform.localPosition = new Vector3(x, y, 0f);

            Cell cellScript = newCell.GetComponent<Cell>();
                cellScript.sprite = SelectSprite();

            // Adding
            curPool.Add(cellScript.sprite);

            // Duplicates
            for (int i=0; i<taskPool.Count; i++) curPool.Remove(taskPool[i]);

        return newCell;
    }

    // Destroys all cells
    void DestroyCells()
    {
        GameObject[] oldCells = GameObject.FindGameObjectsWithTag("Cell");
        for(var i=0 ; i<oldCells.Length ; i++) Destroy(oldCells[i]);
    }

    // UI RELATED STUFF LIKE BUTTON PRESSING AND SHIT
    public void CheckCell(GameObject cellObject)
    {
        if (!taskEnabled) return;

        // Sprite Check
        Transform childTransform = cellObject.transform.Find("Image");
            Cell childScript = cellObject.GetComponent<Cell>();

        // Correct
        if (taskSprite == childScript.sprite)
        {
            // Effect
            /*
                Не смог продумать, как лучше разрулить анимацию, перед тем как потрём все ячейки.
                Впиливать сюда coroutine не кажется хорошей идеей, может надо было разрулить через Button анимации?

                childTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 1f), 0.3f, 2, 1f);

                Практически та же история с партикл системой, которую я не успел сделать.
                Скорее всего, ее лучше прикрутить к контроллеру и оттуда спавнить партикли на позиции нажатой кнопки и т.д.
            */

            // Progress
            taskProgress += 1;
            if (taskProgress > 3)
            {
                taskEnabled = false;
                taskProgress = 1;

                fadeScreen.DOFade(1, 0.8f);

                restartButton.SetActive(true);
            }
            else
            {
                Initialize(taskProgress, false);
            }
        }
        // Incorrect
        else
        {
            // Effect
            childTransform.DOShakePosition(0.3f, 32f, 24, 0, false, true);
        }
    }

    // Pressed Restart
    public void ButtonRestart()
    {
        taskPool.Clear();
        fadeScreen.DOFade(0, 0.8f);

        taskDescription.color = new Color(255f, 255f, 255f, 0f);
        taskDescription.DOFade(1, 2f);

        Initialize(1, true);
        /*
            Повторное создание ячеек обсирается DoTween ошибками, жалуясь на то, что ячеек нет, а мы их аксессим.
            Но ячейки есть, анимации есть, ошибки тоже, почему-то, есть.
        */
    }
}
