using System.Collections;
using System.Collections.Generic;
using Tetris.Runtime;
using UnityEditor;
using UnityEngine;

namespace Tetris.Editor
{
    public class Test : MonoBehaviour
    {
        [MenuItem("≤‚ ‘/Utils")]
        public static void TestUtils()
        {
            for (int i = -5; i < 20; i++)
            {
                var result = Utils.GenPartBoardData(new int[] { 15, 15, 15, 15 }, i, 10, out var isDataLose);
                Debug.Log($"222∆´“∆£∫{i} [15,15,15,15]=>[{result[0]},{result[1]},{result[2]},{result[3]}]");
            }
        }
    }

}