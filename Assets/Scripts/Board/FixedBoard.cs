using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Runtime
{
    public class FixedBoard : IBoard
    {
        private int boardWidth;

        public FixedBoard(int boardWidth, int boardHeight)
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

        /* 消除 */
        public void Eliminate()
        {
            var datasAfterEliminate = new int[this.datas.Length];
            var newIdx = 0;
            var totalValidLine = (int)Math.Pow(2, this.boardWidth) - 1;
            for (int i = 0; i < this.datas.Length; i++)
            {
                if (this.datas[i] > 0 && this.datas[i] != totalValidLine)
                {
                    datasAfterEliminate[newIdx] = this.datas[i];
                    newIdx++;
                }
            }
            this.datas = datasAfterEliminate;
        }
    }
}