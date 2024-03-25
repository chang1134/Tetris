using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tetris.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour, IBoardView
{
    private static int BOARD_WIDTH = 10;
    private static int BOARD_HEIGHT = 20;

    public GameObject go_moving;
    public GameObject go_fixed;

    public GameObject go_gridItem;

    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtEliminate;
    public TextMeshProUGUI txtSpeed;
    public TextMeshProUGUI txtTime;

    public Button btnLeft;
    public Button btnRight;
    public Button btnDrop;
    public Button btnRotate;
    public ButtonPress btnSpeedUp;

    // 棋盘上正在使用的棋子
    private Dictionary<int, Transform> fixedItemMap = new Dictionary<int, Transform>();
    private Dictionary<int, Transform> movingItemMap = new Dictionary<int, Transform>();

    // 隐藏的缓存棋子
    private List<Transform> cacheItems = new List<Transform>();

    private BoardManager boardMgr = new BoardManager();

    // 下次下移所等待的时间
    private float nextDownWaitingTime = 0;

    private static float AUTO_DOWN_INTERVAL = 1f;

    private GameStatus _gameStatus = GameStatus.Prepare;

    private GameStatus gameStatus
    {
        get { return _gameStatus; }
        set
        {
            if (_gameStatus == value) return;
            _gameStatus = value;
            if (value == GameStatus.Pause)
            {
                // 显示暂停界面
            }
            else
            {
                // 显示游戏结束
            }
        }
    }

    public float DownInterval
    {
        get { return AUTO_DOWN_INTERVAL; }
    }

    void Start()
    {
        btnRotate.onClick.AddListener(() => { if (gameStatus == GameStatus.RoundRuning) boardMgr.UpdateMoveingBoard(BlockOperation.Rotate); });
        btnRight.onClick.AddListener(() => { if (gameStatus == GameStatus.RoundRuning) boardMgr.UpdateMoveingBoard(BlockOperation.Right); });
        btnLeft.onClick.AddListener(() => { if (gameStatus == GameStatus.RoundRuning) boardMgr.UpdateMoveingBoard(BlockOperation.Left); });
        btnDrop.onClick.AddListener(() => { if (gameStatus == GameStatus.RoundRuning) boardMgr.UpdateMoveingBoard(BlockOperation.Drop); });

        btnSpeedUp.onPress.AddListener(() => this.IsSpeedUpBtnDown = true);
        btnSpeedUp.onRelease.AddListener(() => this.IsSpeedUpBtnDown = false);


        boardMgr.Init(this, BOARD_WIDTH, BOARD_HEIGHT);

        gameStatus = GameStatus.RoundRuning;

        boardMgr.PrepareBoard();
        boardMgr.NextRoundStart();
    }

    bool IsSpeedUpBtnDown = false;

    void Update()
    {
        if (gameStatus == GameStatus.RoundRuning)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) boardMgr.UpdateMoveingBoard(BlockOperation.Rotate);
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) boardMgr.UpdateMoveingBoard(BlockOperation.Left);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) boardMgr.UpdateMoveingBoard(BlockOperation.Right);
            if (Input.GetKeyDown(KeyCode.Space)) boardMgr.UpdateMoveingBoard(BlockOperation.Drop);

            // 加速下落
            nextDownWaitingTime += Time.deltaTime * ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || IsSpeedUpBtnDown) ? 5f : 1f);
            if (nextDownWaitingTime > AUTO_DOWN_INTERVAL)
            {
                nextDownWaitingTime = 0;
                boardMgr.UpdateMoveingBoard(BlockOperation.Down);
            }
        }
    }

    private int PosToIndex(int x, int y)
    {
        return x + y * 10;
    }

    private Vector2Int IndexToPos(int index)
    {
        return new Vector2Int(index % 10, index / 10);
    }

    private void SetPosition(Transform transform, int index)
    {
        var pos = IndexToPos(index);
        this.SetPosition(transform, pos.x, pos.y);
    }
    private void SetPosition(Transform transform, int x, int y)
    {
        (transform as RectTransform).anchoredPosition = new Vector2(x * 54, y * 54);
    }

    public void DrawMovingBoard(int[] board)
    {
        DrawBoard(board, this.go_moving.transform, this.movingItemMap);
    }

    // boardData是个二进制数组
    private List<int> DrawBoard(int[] data, Transform parent, Dictionary<int, Transform> usingItemMap)
    {
        var needCreateList = new List<int>();

        for (int y = 0; y < data.Length; y++)
        {
            var lineData = data[y];
            for (int x = 0; x < 10; x++)
            {
                var existBlock = (lineData & (int)Mathf.Pow(2, x)) > 0;
                var index = PosToIndex(x, y);

                if (existBlock)
                {
                    if (!usingItemMap.ContainsKey(index))
                    {
                        needCreateList.Add(index);
                    }
                    else
                    {
                        usingItemMap[index].SetParent(parent);
                    }
                }
                else
                {
                    if (usingItemMap.ContainsKey(index))
                    {
                        AddToCache(usingItemMap[index]);
                        usingItemMap.Remove(index);
                    }
                }
            }
        }

        for (int i = 0; i < needCreateList.Count; i++)
        {
            var index = needCreateList[i];
            var item = getOrCreateItem(parent);
            SetPosition(item, index);
            usingItemMap.Add(index, item);
        }
        return needCreateList;
    }

    private RectTransform getOrCreateItem(Transform parent)
    {
        if (cacheItems.Count > 0)
        {
            var result = cacheItems[cacheItems.Count - 1];
            cacheItems.RemoveAt(cacheItems.Count - 1);
            result.gameObject.SetActive(true);
            result.SetParent(parent);
            return result as RectTransform;
        }
        var item = GameObject.Instantiate(this.go_gridItem, parent);
        item.SetActive(true);
        return item.transform as RectTransform;
    }

    private void AddToCache(Transform transform)
    {
        this.cacheItems.Add(transform);
        transform.gameObject.SetActive(false);
    }

    public void OnRoundOver(int[] beforeEliminateBoardDatas, int[] afterEliminateBoardDatas, bool gameOver)
    {
        StartCoroutine(HandleRoundOver(beforeEliminateBoardDatas, afterEliminateBoardDatas, gameOver));
    }

    IEnumerator HandleRoundOver(int[] beforeEliminateBoardDatas, int[] afterEliminateBoardDatas, bool gameOver)
    {
        this.gameStatus = GameStatus.RoundOver;

        // 先绘制下合并后未消除的格子
        var newItemIdxs = DrawBoard(beforeEliminateBoardDatas, this.go_fixed.transform, this.fixedItemMap);

        var eliminateCount = 0;

        var totalValidLine = (int)Math.Pow(2, BOARD_WIDTH) - 1;
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            if (beforeEliminateBoardDatas[y] == totalValidLine)
            {
                eliminateCount++;
                for (int x = 0; x < BOARD_WIDTH; x++)
                {
                    this.fixedItemMap[PosToIndex(x, y)].GetComponent<Animation>().Play("Combine");
                }
            }
        }
        // 播放消除行动画
        if (eliminateCount == 0)
        {
            // 播放板块插入动画
            List<Transform> animItems = newItemIdxs.Select((x) => this.fixedItemMap[x]).ToList();
            for (int i = 0; i < animItems.Count; i++)
            {
                animItems[i].GetComponent<Animation>().Play("Combine");
            }
        }
        var waitingTime = this.go_gridItem.GetComponent<Animation>().clip.length;
        yield return new WaitForSeconds(waitingTime);
        if (eliminateCount > 0)
        {
            // 消除动画播完后，开始绘制消除后的数据
            DrawBoard(afterEliminateBoardDatas, this.go_fixed.transform, this.fixedItemMap);
        }

        if (gameOver)
        {
            gameStatus = GameStatus.GameOver;
            Debug.Log("BoardView GameOver");
        } else
        {
            boardMgr.NextRoundStart();
        }
    }

    public void OnRoundStart()
    {
        this.gameStatus = GameStatus.RoundRuning;

        nextDownWaitingTime = DownInterval;
    }
}

