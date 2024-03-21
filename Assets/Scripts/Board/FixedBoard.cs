using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public class FixedBoard : IBoard
    {
        private int boardWidth;

        public FixedBoard(int boardHeight, int boardWidth)
        {
            this.datas = new int[boardHeight + Block.MAX_SIZE];
            this.boardWidth = boardWidth;    
        }

        public int[] datas { get; private set; }

        public void Combine(IBoard targetBoard)
        {
            var length = Mathf.Min(targetBoard.datas.Length, this.datas.Length);
            for (int i = 0; i < length; i++)
            {
                this.datas[i] |= targetBoard.datas[i];
            }
        }

        /**
         * 判断是否游戏结束
         * 1. 非可视区域存在数据
         */
        public bool IsGameOver()
        {
            for (int i = this.datas.Length - Block.MAX_SIZE; i < this.datas.Length; i++)
            {
                if (this.datas[i] > 0) return true;
            }
            return false;
        }
    }
}