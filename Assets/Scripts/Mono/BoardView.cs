using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tetris.Runtime;
using UnityEngine;


public class BoardView : MonoBehaviour, IBoardView
{
    private static int BOARD_WIDTH = 10;
    private static int BOARD_HEIGHT = 20;

    public GameObject go_moving;
    public GameObject go_fixed;

    public GameObject go_gridItem;

    // 棋盘上正在使用的棋子
    private Dictionary<int, Transform> fixedItemMap = new Dictionary<int, Transform>();
    private Dictionary<int, Transform> movingItemMap = new Dictionary<int, Transform>();

    // 隐藏的缓存棋子
    private List<Transform> cacheItems = new List<Transform>();

    private BoardManager boardMgr = new BoardManager();

    // 下次下移所等待的时间
    private float nextDownWaitingTime = 0;

    private static float AUTO_DOWN_INTERVAL = 0.2f;

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
        //for (int i = 0; i < boardFixedData.Length; i++)
        //{
        //    boardFixedData[i] = (int)Mathf.Pow(2, 10) - 1;
        //}
        //DrawFixedBoard();
        //DrawBlock(Block.I);
        //gameStatus = GameStatus.Running;
        boardMgr.Init(this, BOARD_WIDTH, BOARD_HEIGHT);

        gameStatus = GameStatus.Running;

        boardMgr.PrepareBoard();
    }

    void Update()
    {
        if (gameStatus == GameStatus.Running)
        {
            if (Input.GetKeyDown(KeyCode.W)) boardMgr.UpdateMoveingBoard(BlockOperation.Rotate);
            if (Input.GetKeyDown(KeyCode.A)) boardMgr.UpdateMoveingBoard(BlockOperation.Left);
            if (Input.GetKeyDown(KeyCode.D)) boardMgr.UpdateMoveingBoard(BlockOperation.Right);
            if (Input.GetKeyDown(KeyCode.Space)) boardMgr.UpdateMoveingBoard(BlockOperation.ToBottom);

            // 加速下落
            nextDownWaitingTime += Time.deltaTime;
            if (nextDownWaitingTime > AUTO_DOWN_INTERVAL / (Input.GetKey(KeyCode.S) ? 5f : 1f))
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

    public void DrawFixedBoard(int[] board)
    {
        var newItemIdxs = DrawBoard(board, this.go_fixed.transform, this.fixedItemMap);
        var newItems = newItemIdxs.Select((x) => this.fixedItemMap[x]).ToList();
        for (int i = 0; i < newItems.Count; i++)
        {
            newItems[i].GetComponent<Animation>().Play("Combine");
        }
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

    public void OnGameOver()
    {
        gameStatus = GameStatus.GameOver;
        Debug.Log("BoardView GameOver");
    }

    public void OnRoundStart()
    {
        nextDownWaitingTime = DownInterval;
    }
}

