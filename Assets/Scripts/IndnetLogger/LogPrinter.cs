using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogPrinter : MonoBehaviour
{
    private ulong _frameCnt;
    [SerializeField] private string _filePath = "MyLogs/Test.log";

    private void Start()
    {
        IndentLogger.PrintTo(_filePath, false);
        StartCoroutine(WriteLog());
    }

    private IEnumerator WriteLog()
    {
        while (true)
        {
            IndentLogger.LogHeader(_frameCnt++.ToString());
            yield return null;
            IndentLogger.AppendAndPrintTo(_filePath);
            IndentLogger.Clear();
        }
    }
}