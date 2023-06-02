using Board.Presenter;
using UnityEditor;
using UnityEngine;

namespace Board.Editor
{
    [CustomEditor(typeof(BoardCtrl))]
    public class BoardCtrlEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var boardCtrl = target as BoardCtrl;
            if (boardCtrl == null)
                return;
            if (GUILayout.Button("InitBoardData"))
            {
                boardCtrl.InitBoardData();
            }
            if (GUILayout.Button("CreateBoardAndFillBlock"))
            {
                boardCtrl.CreateBoardAndFillBlock();
            }
            if (GUILayout.Button("Reset"))
            {
                boardCtrl.Reset();
            }
            if (GUILayout.Button("Open Board Data Debugger"))
            {
                BoardDebugger.ShowWindow(boardCtrl);
            }
        }
    }
}