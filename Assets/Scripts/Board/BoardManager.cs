using System;
using UnityEngine;

namespace Tetris.Runtime
{
    public class BoardManager
    {
        private int boardWidth;
        private int boardHeight;

        public GameObject go_moving;
        public GameObject go_fixed;

        public GameObject go_gridItem;

        private MovingBoard movingBoard;

        private FixedBoard fixedBoard;

        private Block nextBlock;

        private IBoardView view;

        public void Init(IBoardView view, int boardWidth, int boardHeight)
        {
            this.view = view;
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            this.movingBoard = new MovingBoard(boardWidth, this.boardHeight);
            this.fixedBoard = new FixedBoard(boardWidth, this.boardHeight);
        }

        public void PrepareBoard()
        {
            CleanNextBlock();
            // 游戏开始时，先准备下一个板块
            PrepareNextBlock();
        }

        public void NextRoundStart()
        {
            view.OnRoundStart();

            var block = ExtractNextBlock();

            movingBoard.Reset(block);

            PrepareNextBlock();
        }

        private void RoundOver()
        {
            // 跟固定棋盘进行合并
            this.fixedBoard.Combine(this.movingBoard);

            var beforeEliminateBoardDatas = new int[this.fixedBoard.datas.Length];
            Array.Copy(this.fixedBoard.datas, beforeEliminateBoardDatas, beforeEliminateBoardDatas.Length);

            // 检测消除
            this.fixedBoard.Eliminate();

            this.movingBoard.CleanBoard();
            view.DrawMovingBoard(this.movingBoard.datas);

            view.OnRoundOver(beforeEliminateBoardDatas, this.fixedBoard.datas, this.fixedBoard.IsGameOver());

            //view.DrawFixedBoard(this.fixedBoard.datas);

            //view.DrawMovingBoard(this.movingBoard.datas);

            //if (this.fixedBoard.IsGameOver())
            //{
            //    view.OnGameOver();
            //    Debug.Log("游戏结束");
            //}
            //else
            //{
            //    RunNextBlock();
            //}
        }

        /**
         * 根据操作调整移动的棋盘
         */
        public void UpdateMoveingBoard(BlockOperation operation)
        {
            switch (operation)
            {
                case BlockOperation.Down:
                    if (!canDown())
                    {
                        this.RoundOver();
                        return;
                    }
                    this.movingBoard.Down();
                    view.DrawMovingBoard(this.movingBoard.datas);
                    break;
                case BlockOperation.Left:
                    {
                        if (!this.canLeft()) return;
                        this.movingBoard.Left();
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }
                    break;
                case BlockOperation.Right:
                    {
                        if (!this.canRight()) return;
                        this.movingBoard.Right();
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }
                    break;
                case BlockOperation.ToBottom:
                    // TODO 刚准备好的板块 可能还没出现到棋盘上，就被置底了， 应当至少出现了一格，才能下坠
                    var partBoardData = Utils.GenPartBoardData(this.movingBoard.block.binaryArray, this.movingBoard.x, boardWidth);
                    var y = this.movingBoard.y;
                    while(y >= 0)
                    {
                        var isPass = true;
                        for (int i = partBoardData.Length - 1; i >= 0; i--)
                        {
                            if ((partBoardData[i] > 0 && y - i < 0) || (y - i >= 0 && (this.fixedBoard.datas[y - i] & partBoardData[i]) > 0))
                            {
                                isPass = false;
                                break;
                            }
                        }
                        if (!isPass)
                        {
                            y++; // 当前行无法合并，所以得返回上一行
                            break;
                        }
                        y--;
                    }
                    if (y >= this.movingBoard.y) return;
                    this.movingBoard.ToBottom(y);
                    this.RoundOver();
                    break;
                case BlockOperation.Rotate:
                    if (!canRotate()) return;
                    this.movingBoard.Rotate();
                    view.DrawMovingBoard(this.movingBoard.datas);
                    break;
                default:
                    break;
            }
        }

        /**
         * 检测下一次下落能否抵达
         */
        private bool canDown()
        {
            if (this.movingBoard.datas[0] > 0) return false;
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                if (currentMovingValue > 0 && lineIdx - 1 < 0) return false; // 移动行已经处于底端，不能再往下移动
                if ((currentMovingValue & this.fixedBoard.datas[lineIdx - 1]) > 0) return false; // 移动后有重叠， 则不可抵达
                //if (this.fixedBoard.datas[i] == 0 || this.movingBoard.datas[i + 1] == 0) continue;
                //if ((this.fixedBoard.datas[i] & this.movingBoard.datas[i + 1]) > 0) return false; 
            }
            return true;
        }

        private bool canLeft()
        {
            //if (this.movingBoard.x - 1 + this.movingBoard.block.rect.x - 1 < 0) return false;
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // 版块先左移一格，如果超出了界面，则再次右移回来的数据会跟原始数据不一致
                var valueAfterLeftMoving = currentMovingValue >> 1;
                if (currentMovingValue != valueAfterLeftMoving << 1) return false;
                // 该行在合并后是否有重叠数据，如果有，则无法移动
                if ((valueAfterLeftMoving & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        private bool canRight()
        {
            for (int i = 0; i < this.movingBoard.block.size; i++)
            {
                var lineIdx = this.movingBoard.y - i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // 如果最右边存在格子，则无法右移
                if ((currentMovingValue & (int)Math.Pow(2, this.boardWidth - 1)) > 0) return false;
                // 移动后的数据在跟固定棋盘数据合并后如果有重叠数据，则无法移动
                if (((currentMovingValue << 1) & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        //  这里逻辑有问题
        private bool canRotate()
        {
            // 检查移动或旋转后的板块的有效数据是否会超出棋盘
            var rotatedBlockData = Block.Rotate(this.movingBoard.block);

            // 检查移动后的数据能否和固定棋盘合并
            var binaryArray = Block.ToBinaryArray(rotatedBlockData);
            var partBoardData = Utils.GenPartBoardData(binaryArray, this.movingBoard.x, boardWidth);
            for (int i = 0; i < partBoardData.Length; i++)
            {
                if (partBoardData[i] >> this.movingBoard.x != binaryArray[i]) return false; // 检查在合并到局部棋盘时，数据是否有损失
                if ((this.fixedBoard.datas[this.movingBoard.y - i] & partBoardData[i]) > 0) return false; // 检查旋转后能否跟固定棋盘合并
            }

            return true;
        }


        private void PrepareNextBlock(BlockType blockType)
        {
            var block = new Block(blockType, UnityEngine.Random.Range(0, 4));

            nextBlock = block;
            // TODO 显示到界面上
        }

        private void PrepareNextBlock()
        {
            if (nextBlock != null)
            {
                throw new Exception("nextBlock is exist!!");
            }

            var blockValues = Enum.GetValues(typeof(BlockType));

            int randomIdx = UnityEngine.Random.Range(0, blockValues.Length);

            PrepareNextBlock((BlockType)blockValues.GetValue(randomIdx));
        }

        private Block ExtractNextBlock()
        {
            if (nextBlock == null)
            {
                throw new Exception("nextBlock is not exist!!");
            }

            var tempBlock = nextBlock;
            CleanNextBlock();
            return tempBlock;
        }

        private void CleanNextBlock() { nextBlock = null; }
    }
}
