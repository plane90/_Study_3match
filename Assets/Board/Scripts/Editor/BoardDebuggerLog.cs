using UnityEngine;
using UnityEngine.UIElements;

namespace Board.Editor
{
    public enum LogType
    {
        FindMatchLineOneSide, FindMatchCube, FindMatchLineBothSide, FindMatchL, FindMatchT
    }
    
    public class BoardDebuggerLog
    {
        private GroupBox _groupBox;
        private int _indentLevel;
        
        public BoardDebuggerLog(GroupBox groupBox)
        {
            _groupBox = groupBox;
            _groupBox.text = "";
        }

        public void Clear()
        {
            _groupBox.text = "";
            _indentLevel = 0;
        }

        public void Log(string msg)
        {
            var space = "";
            for (int i = 0; i < _indentLevel; i++)
                space += "  ";
            _groupBox.text += $"\n{space}{msg}";
        }
        
        public void Indent()
        {
            _indentLevel += 1;
            _indentLevel = Mathf.Clamp(_indentLevel, 0, int.MaxValue);
        }
        
        public void Unindent()
        {
            _indentLevel -= 1;
            _indentLevel = Mathf.Clamp(_indentLevel, 0, int.MaxValue);
        }
    }
}