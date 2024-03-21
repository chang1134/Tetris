using System;
using UnityEngine;

namespace Tetris.Runtime
{
    public class LineBlockInfo
    {
        public int rotateCount;
        public int blockPosX;
        public int blockPosY;
        public BlockType type;
        public int[] lineBlockData;
    }

    public class MovingBoard : IBoard
    {
        public Block block { get; private set; }

        /**
         * 当前非二进制区块的第0个数据处于非二进制棋盘的位置
         * 注意：1. 棋盘的左下角为原点
         *      2. 区块位置可能会超出棋盘（存在空行）
         */
        public int x { get; private set; }
        public int y { get; private set; }

        private int boardWidth;

        public MovingBoard(int boardHeight, int boardWidth)
        {
            datas = new int[boardHeight + Block.MAX_SIZE];
            this.boardWidth = boardWidth;
        }

        public int[] datas { get; private set; }

        public void Reset(Block block)
        {
            var offsetY = this.datas.Length - Block.MAX_SIZE + block.size - block.rect.y - block.rect.height;
            var offsetX = Mathf.CeilToInt((this.boardWidth - block.size) / 2f);
            this.Reset(block, offsetX, this.datas.Length - offsetY - 1);
        }

        public void Reset(Block block, int x, int y)
        {
            CleanBoard();

            this.block = block;
            this.x = x;
            this.y = y;
            this.UpdateBoard();
        }

        private void CleanBoard()
        {
            if (this.datas == null) return;
            for (int i = 0; i < this.datas.Length; i++)
            {
                this.datas[i] = 0;
            }
        }

        private void UpdateBoard()
        {
            CleanBoard();
            for (int i = 0; i < block.size; i++)
            {
                if (this.y - i >= 0)
                {
                    if (this.x >= 0)
                    {
                        this.datas[this.y - i] = this.block.binaryArray[i] << this.x;
                    } else
                    {
                        this.datas[this.y - i] = this.block.binaryArray[i] >> Math.Abs(this.x);
                    }
                }
            }
        }

        /**
         * 下落一格
         */
        public void Down()
        {
            this.y -= 1;
            this.UpdateBoard();
        }

        public void Left()
        {
            this.x -= 1;
            this.UpdateBoard();
        }

        public void Right()
        {
            this.x += 1;
            this.UpdateBoard();
        }

        public void Rotate()
        {
            this.block.Rotate();
            this.UpdateBoard();
        }
    }
}
