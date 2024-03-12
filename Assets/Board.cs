using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

struct ItemInfo
{
    public Transform transform;
    public int x;
    public int y;
}

public enum BlockType
{
    I, O, S, Z, L, J, T,
}

public class Board : MonoBehaviour
{
    public GameObject go_moving;
    public GameObject go_fixed;

    public GameObject go_gridItem;

    private Dictionary<int, Transform> usingItemMap = new Dictionary<int, Transform>();

    private List<Transform> cacheItems = new List<Transform>();

    private int[] boardFixedData = new int[20];

    void Start()
    {
        //for (int i = 0; i < boardFixedData.Length; i++)
        //{
        //    boardFixedData[i] = (int)Mathf.Pow(2, 10) - 1;
        //}
        DrawFixedBoard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) // ×ó
        {

        }
        else if (Input.GetKeyDown(KeyCode.S)) // ÏÂ
        {

        }
        else if (Input.GetKeyDown(KeyCode.D)) // ÓÒ
        {

        }
        else if (Input.GetKeyDown(KeyCode.Space)) // Ðý×ª
        {

        }
    }

    private int posToIndex(int x, int y)
    {
        return x + y * 10;
    }

    private Vector2Int indexToPos(int index)
    {
        return new Vector2Int(index % 10, index / 10);
    }

    private void DrawBlock(BlockType blockType)
    {

    }

    private void DrawFixedBoard()
    {
        for (int y = 0; y < boardFixedData.Length; y++)
        {
            var lineData = boardFixedData[y];
            for (int x = 0; x < 10; x++)
            {
                var existBlock = (lineData & (int)Mathf.Pow(2, x)) > 0;
                var index = posToIndex(x, y);

                if (existBlock)
                {
                    if (!usingItemMap.ContainsKey(index))
                    {
                        var item = getOrCreateItem(this.go_fixed.transform);
                        item.anchoredPosition = new Vector2(x * 54, y * 54);
                        usingItemMap.Add(index, item);
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
    }

    private RectTransform getOrCreateItem(Transform parent)
    {
        if (cacheItems.Count > 0)
        {
            var result = cacheItems[cacheItems.Count - 1];
            cacheItems.RemoveAt(cacheItems.Count - 1);
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
}

