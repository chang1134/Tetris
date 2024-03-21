using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public static class Utils
    {
        [UnityEditor.MenuItem("测试/Utils")]
        public static void Test()
        {
            for (int i = -5; i < 20; i++)
            {
                var result = GenPartBoardData(new int[]{15,15,15,15}, i, 10);
                Debug.Log($"偏移：{i} [15,15,15,15]=>[{result[0]},{result[1]},{result[2]},{result[3]}]");
            }
        }

        /**
         * 通过二进制区块 和 偏移
         */
        public static int[] GenPartBoardData(int[] binaryArray, int x, int boardWidth)
        {
            var size = binaryArray.Length;
            if (x < 0)
            {
                var newBinaryArray = new int[size];
                if (-x < size)
                {
                    for (int i = 0; i < binaryArray.Length; i++)
                    {
                        newBinaryArray[i] = binaryArray[i] >> (-x);
                    }
                }
                return newBinaryArray;
            } 
            if (x >= boardWidth - size)
            {
                var newBinaryArray = new int[size];
                var range = (int)Mathf.Pow(2, size) - 1;
                for (int i = 0; i < binaryArray.Length; i++)
                {
                    newBinaryArray[i] = (binaryArray[i] << (boardWidth - x)) & range;
                }
                return newBinaryArray;
            }

            var result = new int[binaryArray.Length];
            for (int i = 0; i < binaryArray.Length; i++)
            {
                if (x >= 0)
                {
                    result[binaryArray.Length - 1 - i] = binaryArray[i] << x;
                }
                else
                {
                    result[binaryArray.Length - 1 - i] = binaryArray[i] >> (-x);
                }
            }
            return result;
        }
    }
}
