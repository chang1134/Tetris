using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static Tetris.Runtime.BoardDefine;

namespace Tetris.Runtime
{


    public class BoardManager
    {



        private static int BOARD_WIDTH = 10;
        private static int BOARD_HEIGHT = 20;

        public GameObject go_moving;
        public GameObject go_fixed;

        public GameObject go_gridItem;

        // 棋盘上正在使用的棋子
        private Dictionary<int, Transform> fixedItemMap = new Dictionary<int, Transform>();
        private Dictionary<int, Transform> movingItemMap = new Dictionary<int, Transform>();

        // 隐藏的缓存棋子
        private List<Transform> cacheItems = new List<Transform>();

        private GameStatus _gameStatus = GameStatus.Prepare;

        private float nextDownWaitingTime = 0;

        private static float AUTO_DOWN_INTERVAL = 0.2f;

        private MovingBoard movingBoard;

        private FixedBoard fixedBoard;

        private Block nextBlock;

        private IBoardView view;

        public void Init(IBoardView view)
        {
            this.view = view;
            this.movingBoard = new MovingBoard(BOARD_HEIGHT, BOARD_WIDTH);
            this.fixedBoard = new FixedBoard(BOARD_HEIGHT, BOARD_WIDTH);
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
                    //if (!checkNextStepReachable())
                    //{
                    //    // 当前板块已经停止，需要检测板块是否完全显示在棋盘上
                    //    // 如果没有，则游戏结束
                    //    // 如果有， 则需要并入Fixed棋盘
                    //    gameStatus = GameStatus.GameOver;
                    //    return;
                    //}
                    if (!checkNextStepReachable()) // 下方是最后一层 或者 下方已经存在阻挡的格子
                    {
                        // 跟固定棋盘进行合并
                        this.fixedBoard.Combine(this.movingBoard);
                        view.DrawFixedBoard(this.fixedBoard.datas);
                        if (movingBoard.board[BOARD_HEIGHT] == 0)
                        {
                            RunNextBlock();
                        }
                        else
                        {
                            gameStatus = GameStatus.GameOver;
                            Debug.Log("游戏结束");
                        }
                        return;
                    }
                    this.movingBoard.Down();
                    view.DrawMovingBoard(this.movingBoard.datas);
                    break;
                case BlockOperation.Left:
                    {
                        var length = Math.Max(movingBoard.lineBlockInfo.lineBlockData.Length, movingBoard.board.Length - movingBoard.lineBlockInfo.blockPosY);

                        // 所有移动中的棋子左移一格
                        var isOutRange = false;
                        for (int i = 0; i < length; i++)
                        {
                            var index = movingBoard.y + i;
                            if ((movingBoard.board[index] & 1) > 0)
                            {
                                isOutRange = true;
                                break;
                            }
                        }
                        if (isOutRange) return; // 左移移会超出范围
                        for (int i = 0; i < length; i++)
                        {
                            var index = movingBoard.lineBlockInfo.blockPosY + i;
                            movingBoard.board[index] = movingBoard.board[index] >> 1;
                        }
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }

                    break;
                case BlockOperation.Right:
                    {
                        var length = Math.Max(movingBoard.lineBlockInfo.lineBlockData.Length, movingBoard.board.Length - movingBoard.lineBlockInfo.blockPosY);
                        // 所有移动中的棋子右移一格
                        var isOutRange = false;
                        for (int i = 0; i < length; i++)
                        {
                            var index = movingBoard.lineBlockInfo.blockPosY + i;
                            if ((movingBoard.board[index] & (int)Math.Pow(2, BOARD_WIDTH - 1)) > 0)
                            {
                                isOutRange = true;
                                break;
                            }
                        }
                        if (isOutRange) return; // 右移会超出范围
                        for (int i = 0; i < length; i++)
                        {
                            var index = movingBoard.lineBlockInfo.blockPosY + i;
                            movingBoard.board[index] = movingBoard.board[index] << 1;
                        }
                        view.DrawMovingBoard(this.movingBoard.datas);
                    }
                    break;
                case BlockOperation.ToBottom:

                    break;
                case BlockOperation.Rotate:
                    var newLineBlockData = Block.Create(movingBoard.lineBlockInfo.type, movingBoard.lineBlockInfo.rotateCount + 1);


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
        private bool checkNextStepReachable()
        {
            if (this.movingBoard.datas[0] > 0) return false;
            for (int i = 0; i < BOARD_HEIGHT; i++)
            {
                if (this.fixedBoard.datas[i] == 0 || this.movingBoard.datas[i + 1] == 0) continue;
                if ((this.fixedBoard.datas[i] & this.movingBoard.datas[i + 1]) > 0) return false; // 有重叠， 则不可抵达
            }
            return true;
        }


        private void PrepareNextBlock(BlockType blockType, bool isRandom)
        {
            var block = new Block(blockType, UnityEngine.Random.Range(0, 4));

            //LineBlockInfo lineBlockInfo = new LineBlockInfo();
            //lineBlockInfo.type = blockType;
            //var rotateCount = ;
            //var blockData = Block.Create(blockType, rotateCount);
            //var lineBlockData = new int[blockData.Length];
            ////var startX = Mathf.CeilToInt((BOARD_WIDTH - blockData.Length) / 2f);
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
