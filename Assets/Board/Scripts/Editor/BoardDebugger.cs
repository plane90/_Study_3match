using System;
using Board.Model;
using Board.Presenter;
using UnityEditor;
using UnityEditor.UIElements;
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
        
        private const string BlockStateBorderName = "block-component-debug__state-border";
        private const string BlockTypeName = "block-component-debug__type";

        private ScrollView _scrollView;
        private Label _rows;
        private Label _cols;
        private Button _reloadButton;

        private VisualElement[] _rowParents;
        
        public static void ShowWindow(BoardCtrl boardCtrl)
        {
            var window = GetWindow<BoardDebugger>();
            window.titleContent = new GUIContent("Board Debugger");
            window.position = new Rect(new Vector2(500f, 500f), new Vector2(500f, 500f));

            _boardCtrl = boardCtrl;
            _boardData = boardCtrl.boardData;
        }

        private void CreateGUI()
        {
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
            _reloadButton = rootVisualElement.Q<Button>();
            _reloadButton.RegisterCallback<ClickEvent>(ReloadGUI);
        }

        private void ReloadGUI(ClickEvent evt)
        {
            _boardCtrl = FindObjectOfType(typeof(BoardCtrl)) as BoardCtrl;

            if (_boardCtrl == null)
            {
                Debug.Log("Board Ctrl Not Found");
                return;
            }

            _boardData = _boardCtrl.boardData;
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
                    SetBlockState((_rowParents[y].ElementAt(x)), _boardData.BlockDataArray2D[y, x].state);
                    SetBlockType(_rowParents[y].ElementAt(x), _boardData.BlockDataArray2D[y, x].currentType);
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
                BlockState.Ready => Color.yellow,
                _ => cell.Q(BlockStateBorderName).style.borderBottomColor
            };
            
            cell.Q(BlockStateBorderName).style.borderBottomColor = color;
            cell.Q(BlockStateBorderName).style.borderRightColor = color;
            cell.Q(BlockStateBorderName).style.borderLeftColor = color;
            cell.Q(BlockStateBorderName).style.borderTopColor = color;

            cell.Q<Label>().text = state.ToString();
        }
    }
}