using System;
using Board.Model;
using Board.Presenter;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Board.Editor
{
    public class BoardDebugger : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _boardDebuggerTree;
        [SerializeField] private VisualTreeAsset _blockComponentDebug;

        private static BoardData _boardData;
        private static BoardCtrl _boardCtrl;
        
        private const string RowsName = "board-debugger-rows__value-label";
        private const string ColsName = "board-debugger-cols__value-label";
        private const string MatchLineOneLogName = "board-debugger__match-line-one-log-box";
        private const string MatchCubeLogName = "board-debugger__match-line-cube-box";
        private const string MatchLineBothLogName = "board-debugger__match-line-both-log-box";
        private const string MatchShapeLLogName = "board-debugger__match-shape-l-log-box";
        private const string MatchShapeTLogName = "board-debugger__match-shape-t-cube-box";
        private const string DisableLogName = "board-debugger__disable-log-box";
        
        private const string BlockStateBorderName = "block-component-debug__state-border";
        private const string BlockTypeName = "block-component-debug__type";
        private const string BlockStateLabelName = "block-component-debug__state-label";
        private const string BlockCoordLabelName = "block-component-debug__coord-label";

        private ScrollView _scrollView;
        private Label _rows;
        private Label _cols;
        private Button _reloadButton;
        private Button _clearMatchLogButton;
        private Button _clearDisableLogButton;
        private BoardDebuggerLog _matchLineOneLog;
        private BoardDebuggerLog _matchCubeLog;
        private BoardDebuggerLog _matchLineBothLog;
        private BoardDebuggerLog _matchShapeLLog;
        private BoardDebuggerLog _matchShapeTLog;
        private BoardDebuggerLog _disableLog;

        private VisualElement[] _rowParents;
        private static BoardDebugger _instance;
        private bool _showLog = true;


        public static void ShowWindow(BoardCtrl boardCtrl)
        {
            if (_instance == null)
            {
                _instance = GetWindow<BoardDebugger>();
                _instance.titleContent = new GUIContent("Board Debugger");
                _instance.position = new Rect(new Vector2(500f, 500f), new Vector2(500f, 500f));
                
            }
            else
            {
                _boardCtrl = boardCtrl;
                _boardData = _boardCtrl.GetBoardData();
            }
        }

        private void CreateGUI()
        {
            if (_boardDebuggerTree == null)
            {
                Debug.Log("BoardDebuggerLayout.UXML is null");
                return;
            }
            
            if (_instance == null)
            {
                _instance = GetWindow<BoardDebugger>();
            }
            _boardDebuggerTree.CloneTree(rootVisualElement);
            InitElement();
            InitBoard();
        }

        private void Update()
        {
            if (_boardData != null && _boardCtrl != null && _rowParents != null)
                DrawBoard();
        }

        private void InitElement()
        {
            _scrollView = rootVisualElement.Q<ScrollView>();
            _scrollView.Clear();
            _rows = rootVisualElement.Q<Label>(RowsName);
            _cols = rootVisualElement.Q<Label>(ColsName);
            _reloadButton = rootVisualElement.Q<Button>("board-debugger__reload-button");
            _reloadButton.RegisterCallback<ClickEvent>(ReloadGUI);
            _clearMatchLogButton = rootVisualElement.Q<Button>("board-debugger__clear-button");
            _clearMatchLogButton.RegisterCallback<ClickEvent>(ClearMatchLog);
            _clearDisableLogButton = rootVisualElement.Q<Button>("board-debugger__clear-disable-log-button");
            _clearDisableLogButton.RegisterCallback<ClickEvent>(ClearDisableLog);
            _matchLineOneLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(MatchLineOneLogName));
            _matchCubeLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(MatchCubeLogName));
            _matchLineBothLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(MatchLineBothLogName));
            _matchShapeLLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(MatchShapeLLogName));
            _matchShapeTLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(MatchShapeTLogName));
            _disableLog = new BoardDebuggerLog(rootVisualElement.Q<GroupBox>(DisableLogName));
        }

        private void ClearMatchLog(ClickEvent evt)
        {
            _matchLineOneLog.Clear();
            _matchCubeLog.Clear();
            _matchLineBothLog.Clear();
            _matchShapeLLog.Clear();
            _matchShapeTLog.Clear();
        }

        private void ClearDisableLog(ClickEvent evt)
        {
            _disableLog.Clear();
        }

        private void ReloadGUI(ClickEvent evt)
        {
            _boardCtrl = FindObjectOfType(typeof(BoardCtrl)) as BoardCtrl;

            if (_boardCtrl == null)
            {
                Debug.Log("Board Ctrl Not Found");
                return;
            }

            _boardData = _boardCtrl.GetBoardData();
            InitElement();
            InitBoard();
        }

        private void InitBoard()
        {
            if (_boardData == null)
                return;
            _rows.text = _boardData.Rows.ToString();
            _cols.text = _boardData.Cols.ToString();

            _rowParents = new VisualElement[_boardData.Rows];
            for (int y = 0; y < _boardData.Rows; y++)
            {
                _rowParents[y] = new VisualElement()
                {
                    style = { flexDirection = FlexDirection.Row }
                };
                for (int x = 0; x < _boardData.Cols; x++)
                {
                    var cell = _blockComponentDebug.Instantiate();
                    SetBlockState(cell, _boardData.BlockDataArray2D[y, x].state);
                    SetBlockType(cell, _boardData.BlockDataArray2D[y, x].currentType);
                    SetBlockCoord(cell, _boardData.BlockDataArray2D[y, x].array2dIdx);
                    _rowParents[y].Add(cell);
                }
                _scrollView.Add(_rowParents[y]);
            }
        }

        private void DrawBoard()
        {
            if (_rowParents == null || _rowParents.Length == 0)
                return;
            for (int y = 0; y < _boardData.Rows; y++)
            {
                for (int x = 0; x < _boardData.Cols; x++)
                {
                    var cell = _rowParents[y].ElementAt(x);
                    SetBlockState(cell, _boardData.BlockDataArray2D[y, x].state);
                    SetBlockType(cell, _boardData.BlockDataArray2D[y, x].currentType);
                    SetBlockCoord(cell, _boardData.BlockDataArray2D[y, x].array2dIdx);
                }
            }
        }

        private void SetBlockType(VisualElement cell, BlockType type)
        {
            cell.Q(BlockTypeName).style.backgroundColor = type switch
            {
                BlockType.None => Color.grey,
                BlockType.Red => Color.red,
                BlockType.Green => Color.green,
                BlockType.Blue => Color.blue,
                BlockType.Yellow => Color.yellow,
                _ => cell.Q(BlockTypeName).style.backgroundColor
            };
        }

        private void SetBlockState(VisualElement cell, BlockState state)
        {
            var color = state switch
            {
                BlockState.Disable => Color.gray,
                BlockState.Enable => Color.green,
                BlockState.Updating => Color.red,
                BlockState.Wait => Color.white,
                _ => cell.Q(BlockStateBorderName).style.borderBottomColor
            };
            
            cell.Q(BlockStateBorderName).style.borderBottomColor = color;
            cell.Q(BlockStateBorderName).style.borderRightColor = color;
            cell.Q(BlockStateBorderName).style.borderLeftColor = color;
            cell.Q(BlockStateBorderName).style.borderTopColor = color;

            cell.Q<Label>(BlockStateLabelName).text = state.ToString();
        }
        
        private void SetBlockCoord(VisualElement cell, BoardVec2 array2dIdx)
        {
            cell.Q<Label>(BlockCoordLabelName).text = array2dIdx;
        }

        public static void Log(string msg, LogType logType)
        {
            if (_instance == null || !_instance._showLog)
                return;
            switch (logType)
            {
                case LogType.FindMatchLineOneSide:
                    _instance._matchLineOneLog.Log(msg);
                    break;
                case LogType.FindMatchCube:
                    _instance._matchCubeLog.Log(msg);
                    break;
                case LogType.FindMatchLineBothSide:
                    _instance._matchLineBothLog.Log(msg);
                    break;
                case LogType.FindMatchL:
                    _instance._matchShapeLLog.Log(msg);
                    break;
                case LogType.FindMatchT:
                    _instance._matchShapeTLog.Log(msg);
                    break;
                case LogType.Disable:
                    _instance._disableLog.Log(msg);
                    break;
            }
        }
        
        public static void Indent(LogType logType)
        {
            if (_instance == null || !_instance._showLog)
                return;
            switch (logType)
            {
                case LogType.FindMatchLineOneSide:
                    _instance._matchLineOneLog.Indent();
                    break;
                case LogType.FindMatchCube:
                    _instance._matchCubeLog.Indent();
                    break;
                case LogType.FindMatchLineBothSide:
                    _instance._matchLineBothLog.Indent();
                    break;
                case LogType.FindMatchL:
                    _instance._matchShapeLLog.Indent();
                    break;
                case LogType.FindMatchT:
                    _instance._matchShapeTLog.Indent();
                    break;
                case LogType.Disable:
                    _instance._disableLog.Indent();
                    break;
            }
        }
        
        public static void Unindent(LogType logType)
        {
            if (_instance == null || !_instance._showLog)
                return;
            switch (logType)
            {
                case LogType.FindMatchLineOneSide:
                    _instance._matchLineOneLog.Unindent();
                    break;
                case LogType.FindMatchCube:
                    _instance._matchCubeLog.Unindent();
                    break;
                case LogType.FindMatchLineBothSide:
                    _instance._matchLineBothLog.Unindent();
                    break;
                case LogType.FindMatchL:
                    _instance._matchShapeLLog.Unindent();
                    break;
                case LogType.FindMatchT:
                    _instance._matchShapeTLog.Unindent();
                    break;
                case LogType.Disable:
                    _instance._disableLog.Unindent();
                    break;
            }
        }

        public static void ShowLog(bool showLog)
        {
            if (_instance == null)
                return;
            _instance._showLog = showLog;
        }
    }
}