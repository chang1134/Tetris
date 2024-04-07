using UnityEngine;

namespace Tetris.Runtime
{
    public static class Utils
    {
        /**
         * 通过二进制区块 和 偏移
         */
        public static int[] GenPartBoardData(int[] binaryArray, int x, int boardWidth, out bool isDataLose)
        {
            isDataLose = false;
            var unvalidTopRange = (int)Mathf.Pow(2, Block.MAX_SIZE) - 1;
            var validRange = (int)Mathf.Pow(2, (boardWidth + Block.MAX_SIZE)) - 1;
            x = x + Block.MAX_SIZE;
            if (x < 0 || x >= Block.MAX_SIZE + boardWidth) return new int[binaryArray.Length];
            var newBinaryArray = new int[binaryArray.Length];
            for (int i = 0; i < binaryArray.Length; i++)
            {
                var curValue = binaryArray[i] << x;
                newBinaryArray[i] = (curValue & validRange) >> Block.MAX_SIZE;
                if (curValue > validRange || (curValue & unvalidTopRange) > 0)
                    isDataLose = true; // 存在数据丢失
            }
            return newBinaryArray;
        }
    }
}
