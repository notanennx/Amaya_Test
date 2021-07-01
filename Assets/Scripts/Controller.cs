using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using System;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [System.Serializable]
    public class Skin
    {
        public string skinName;
        public Sprite[] spriteList;
    }

    // Tasking
    [Header("Tasking")]
    public int taskProgress;
    public bool taskEnabled;
    public Text taskDescription;
    public Sprite taskSprite;

    // Reskins
    [Header("Reskins")]
    public Skin[] skinsLibrary;
    private Sprite[] usedSprites;

    // Miscellaneous
    [Header("Miscellaneous")]
    public GameObject restartButton;
    public GameObject cellPrefab;
    public List<Sprite> spritePool = new List<Sprite>();
    private int rowSize = 3;
    private float cellGap = 24f;
    private float cellSize = 96f;

    // Start
    void Start()
    {
        Initialize(1);
    }

    // Static
    public static Controller i;
    void Awake()
    {
        i = this;
    }

    // Initialize
    void Initialize(int level)
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
    }

    // Build Level
    public List<Sprite> curPool = new List<Sprite>();
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

        return newCell;
    }

    // Destroys all cells
    void DestroyCells()
    {
        GameObject[] oldCells = GameObject.FindGameObjectsWithTag("Cell");
        for(var i=0 ; i<oldCells.Length ; i++) Destroy(oldCells[i]);
    }

    // UI RELATED STUFF LIKE BUTTON PRESSING AND SHIT
    public void CheckCell(GameObject cellObject, Sprite sprite)
    {
        if (!taskEnabled) return;

        if (taskSprite == sprite)
        {
            taskProgress += 1;
            if (taskProgress > 3)
            {

                taskEnabled = false;
                taskProgress = 1;

                restartButton.SetActive(true);
            }
            else
            {
                Initialize(taskProgress);
            }
        }
    }


    // Pressed Cell
    //void ButtonCell()
    //{

    //}

    // Pressed Restart
    public void ButtonRestart()
    {
        Initialize(1);
    }
}
