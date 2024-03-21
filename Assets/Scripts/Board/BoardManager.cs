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

        private GameStatus _gameStatus = GameStatus.Prepare;

        private float nextDownWaitingTime = 0;

        private static float AUTO_DOWN_INTERVAL = 0.2f;

        private MovingBoard movingBoard;

        private FixedBoard fixedBoard;

        private Block nextBlock;

        private IBoardView view;

        public void Init(IBoardView view, int boardWidth, int boardHeight)
        {
            this.view = view;
            this.boardWidth = boardWidth;
            this.boardHeight = boardHeight;
            this.movingBoard = new MovingBoard(boardWidth, boardHeight);
            this.fixedBoard = new FixedBoard(boardWidth, boardHeight);
        }

        public void StartGame()
        {
            // 需要先播放开场动画
            gameStatus = GameStatus.Running;
            nextDownWaitingTime = 0;

            CleanNextBlock();
            // 游戏开始时，先准备下一个板块
            PrepareNextBlock();

            // 开始下一板块
            RunNextBlock();
        }

        private void RunNextBlock()
        {
            var block = ExtractNextBlock();

            movingBoard.Reset(block);

            PrepareNextBlock();
        }

        /**
         * 根据操作调整移动的棋盘
         */
        private void UpdateMoveingBoard(BlockOperation operation)
        {
            switch (operation)
            {
                case BlockOperation.Down:
                    if (!canDown())
                    {
                        // 跟固定棋盘进行合并
                        this.fixedBoard.Combine(this.movingBoard);
                        view.DrawFixedBoard(this.fixedBoard.datas);
                        if (this.fixedBoard.IsGameOver())
                        {
                            gameStatus = GameStatus.GameOver;
                            Debug.Log("游戏结束");
                        } else
                        {
                            RunNextBlock();
                        }
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

                    break;
                case BlockOperation.Rotate:
                    //var newLineBlockData = Block.Create(movingBoard.lineBlockInfo.type, movingBoard.lineBlockInfo.rotateCount + 1);


                    //movingBoardInfo.lineBlockInfo.rotateCount++;
                    //movingBoardInfo.lineBlockInfo.lineBlockData = 
                    //DrawBoard(BoardType.Moving);
                    break;
                default:
                    break;
            }
        }

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

        /**
         * 
         */
        public void Tick(float speedRate)
        {

            if (gameStatus == GameStatus.Running)
            {
                // 加速下落
                nextDownWaitingTime += Time.deltaTime;
                if (nextDownWaitingTime > AUTO_DOWN_INTERVAL / speedRate)
                {
                    nextDownWaitingTime = 0;

                    UpdateMoveingBoard(BlockOperation.Down);
                }
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
                var lineIdx = this.movingBoard.y + i;
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
                var lineIdx = this.movingBoard.y + i;
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
                var lineIdx = this.movingBoard.y + i;
                if (lineIdx < 0) continue;
                var currentMovingValue = this.movingBoard.datas[lineIdx];
                if (currentMovingValue == 0) continue;
                // 版块先右移一格，如果超出了界面，则再次左移回来的数据会跟原始数据不一致
                var valueAfterLeftMoving = currentMovingValue << 1;
                if (currentMovingValue != valueAfterLeftMoving >> 1) return false;
                // 移动后的数据在跟固定棋盘数据合并后如果有重叠数据，则无法移动
                if ((valueAfterLeftMoving & this.fixedBoard.datas[lineIdx]) > 0) return false;
            }
            return true;
        }

        private bool canRotate()
        {
            // 检查移动或旋转后的板块的有效数据是否会超出棋盘
            var rotatedBlockData = Block.Rotate(this.movingBoard.block.data);
            var rect = Block.GetRect(rotatedBlockData);

            // 检查左边
            if (this.movingBoard.x + rect.x - 1 < 0) return false;
            // 检查右边
            if (this.movingBoard.x + rect.x + rect.width + 1 >= this.boardWidth) return false;
            // 检查底部
            if (this.movingBoard.y + this.movingBoard.block.size - rect.x - rect.width - 1 < 0) return false;

            // 检查移动后的数据能否和固定棋盘合并
            var binaryArray = Block.ToBinaryArray(rotatedBlockData);


            return false;
        }


        private void PrepareNextBlock(BlockType blockType, bool isRandom)
        {
            var block = new Block(blockType, UnityEngine.Random.Range(0, 4));

            //LineBlockInfo lineBlockInfo = new LineBlockInfo();
            //lineBlockInfo.type = blockType;
            //var rotateCount = ;
            //var blockData = Block.Create(blockType, rotateCount);
            //var lineBlockData = new int[blockData.Length];
            ////var startX = Mathf.CeilToInt((boardWidth - blockData.Length) / 2f);
            ////for (int i = 0; i < blockData.Length; i++)
            ////{
            ////    lineBlockData[i] = blockData[i] << startX;
            ////}
            ////lineBlockInfo.blockPosX = startX;
            //lineBlockInfo.rotateCount = rotateCount;
            //lineBlockInfo.lineBlockData = lineBlockData;
            nextBlock = block;
            //nextLineBlock = lineBlockInfo;
            // 显示到界面上
        }

        private void PrepareNextBlock()
        {
            if (nextBlock != null)
            {
                throw new Exception("nextBlock is exist!!");
            }

            var blockValues = Enum.GetValues(typeof(BlockType));

            int randomIdx = UnityEngine.Random.Range(0, blockValues.Length);

            PrepareNextBlock((BlockType)blockValues.GetValue(randomIdx), true);
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
