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
                        this.datas[this.y - i] = this.block.bynaryArray[i] << this.x;
                    } else
                    {
                        this.datas[this.y - i] = this.block.bynaryArray[i] >> Math.Abs(this.x);
                    }
                }
            }
        }

        /**
         * 默认下落一格，
         * toBottom为true时，下落至底端
         * 
         * return true表示移动成功（移动后，板块有效数据不会超出棋盘）
         */
        public bool Down(bool toBottom = false)
        {
            if (toBottom)
            {
                var offsetY = this.block.size - this.block.rect.y - this.block.rect.height;
                if (this.y == -offsetY) return false;
                this.y = -offsetY;
            }
            else
            {
                if (CheckOutBounding(x, y - 1, this.block.rect)) return false;
                this.y -= 1;
            }
            this.UpdateBoard();
            return true;
        }

        public bool Left()
        {
            if (CheckOutBounding(x - 1, y, this.block.rect)) return false;

            this.x -= 1;
            this.UpdateBoard();
            return true;
        }

        public bool Right()
        {
            if (CheckOutBounding(x + 1, y, this.block.rect)) return false;

            this.x += 1;
            this.UpdateBoard();
            return true;
        }

        public bool Rotate()
        {
            var rotatedBlockData = Block.Rotate(this.block.data);
            var rect = Block.GetRect(rotatedBlockData);
            if (this.CheckOutBounding(x, y, rect)) return false;
            this.block.Rotate();
            this.UpdateBoard();
            return true;
        }

        /**
         * 检查移动或旋转后的板块的有效数据是否会超出棋盘
         */
        private bool CheckOutBounding(int x, int y, RectInt rect)
        {
            // 检查左边
            if (x + rect.x - 1 < 0) return true;
            // 检查右边
            if (x + rect.x + rect.width + 1 >= this.boardWidth) return true;
            // 检查底部
            if (y + this.block.size - rect.x - rect.width - 1 < 0) return true;
            return false;
        }
    }
}
