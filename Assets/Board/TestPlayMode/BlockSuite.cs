using System.Collections;
using Board.Model;
using Board.Presenter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Board.TestPlayMode
{
    public class BlockSuite
    {
        private BoardCtrl _boardCtrl;
        private PointerEventData _eventData;
        private Camera _cam;

        [OneTimeSetUp]
        public void SetUp()
        {
            SceneManager.LoadScene("Board/SampleBoard", LoadSceneMode.Single);
        }

        [UnitySetUp]
        public IEnumerator UnitySetup()
        {
            yield return null;
            _boardCtrl = GameObject.Find("BoardCtrl").GetComponent<BoardCtrl>();
            _eventData = new PointerEventData(EventSystem.current);
            _cam = Camera.main;
        }

        [TearDown]
        public void TearDown()
        {
            _boardCtrl.Reset();
        }

        [Test]
        public void VerifyScene()
        {
            Assert.IsTrue(_eventData != null);
            Assert.IsTrue(_boardCtrl != null);
            Assert.IsTrue(_cam != null);
        }

        [UnityTest]
        [Description("좌측 좌표에서 우측 좌표로 드래그하면 블록이 스왑 됨.")]
        [TestCase("[1,1]", "[0,1]", ExpectedResult = null)]
        [TestCase("[1,1]", "[1,0]", ExpectedResult = null)]
        [TestCase("[1,1]", "[1,2]", ExpectedResult = null)]
        [TestCase("[1,1]", "[2,1]", ExpectedResult = null)]
        public IEnumerator DragBlock(string from, string to)
        {
            _boardCtrl.InitBoardData(new BoardData(new[,]
            {
                { BlockType.Blue, BlockType.Green, BlockType.Blue, },
                { BlockType.Blue, BlockType.Red, BlockType.Blue, },
                { BlockType.Green, BlockType.Yellow, BlockType.Green, },
            }));
            _boardCtrl.CreateBoardAndFillBlock();

            var fromObj = GameObject.Find(from);
            Assert.That(fromObj != null, $"to: {from}");
            SetPosition(fromObj);
            PointerDown(fromObj);

            yield return null;

            var toObj = GameObject.Find(to);
            Assert.That(toObj != null, $"to: {to}");
            SetPosition(toObj);
            Drag(fromObj);
        
            Assert.That(fromObj.name == to, $"fromObj.name: {fromObj.name}");
            Assert.That(toObj.name == from, $"toObj.name: {toObj.name}");
            yield return new WaitForSeconds(1.0f);
        
            Assert.That(fromObj.name == from, $"fromObj.name: {fromObj.name}");
            Assert.That(toObj.name == to, $"toObj.name: {toObj.name}");
            yield return new WaitForSeconds(1.0f);
        }

        private void SetPosition(GameObject targetObj) =>
            _eventData.position = Camera.main.WorldToScreenPoint(targetObj.transform.position);

        private void PointerDown(GameObject targetObj) =>
            ExecuteEvents.Execute(targetObj, _eventData, ExecuteEvents.pointerDownHandler);
    
        private void Drag(GameObject targetObj) =>
            ExecuteEvents.Execute(targetObj, _eventData, ExecuteEvents.dragHandler);
    }
}