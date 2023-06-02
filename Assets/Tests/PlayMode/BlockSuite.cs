using System.Collections;
using Board.Model;
using Board.Presenter;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests.PlayMode
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
        [Description("좌측 좌표에서 우측 좌표로 드래그.")]
        [TestCase("[1,1]", "[0,1]", ExpectedResult = null)]
        [TestCase("[1,1]", "[1,0]", ExpectedResult = null)]
        [TestCase("[1,1]", "[1,2]", ExpectedResult = null)]
        [TestCase("[1,1]", "[2,1]", ExpectedResult = null)]
        public IEnumerator DragBlockNoMatch(string from, string to)
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
            yield return new WaitForSeconds(0.5f);

            Assert.That(fromObj.name == from, $"fromObj.name: {fromObj.name}");
            Assert.That(toObj.name == to, $"toObj.name: {toObj.name}");
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTest]
        [Description("보드 바깥으로 드래그 함.")]
        public IEnumerator DragBlockOutOfBound()
        {
            _boardCtrl.InitBoardData(new BoardData(new[,]
            {
                { BlockType.Blue, BlockType.Red, },
                { BlockType.Blue, BlockType.Red, },
            }));
            _boardCtrl.CreateBoardAndFillBlock();

            var targetObj = GameObject.Find("[0,0]");
            Assert.That(targetObj != null);
            SetPosition(targetObj);
            PointerDown(targetObj);
            yield return null;
            SetPosition(targetObj.transform.position + Vector3.left * 100f);
            Drag(targetObj);
            DragEnd(targetObj);
            yield return new WaitForSeconds(0.5f);
            Assert.That(targetObj.name == "[0,0]");

            SetPosition(targetObj);
            PointerDown(targetObj);
            yield return null;
            SetPosition(targetObj.transform.position + Vector3.up * 100f);
            Drag(targetObj);
            DragEnd(targetObj);
            yield return new WaitForSeconds(0.5f);
            Assert.That(targetObj.name == "[0,0]");

            targetObj = GameObject.Find("[1,1]");
            Assert.That(targetObj != null);
            SetPosition(targetObj);
            PointerDown(targetObj);
            yield return null;
            SetPosition(targetObj.transform.position + Vector3.right * 100f);
            Drag(targetObj);
            DragEnd(targetObj);
            yield return new WaitForSeconds(0.5f);
            Assert.That(targetObj.name == "[1,1]");

            SetPosition(targetObj);
            PointerDown(targetObj);
            yield return null;
            SetPosition(targetObj.transform.position + Vector3.down * 100f);
            Drag(targetObj);
            DragEnd(targetObj);
            yield return new WaitForSeconds(0.5f);
            Assert.That(targetObj.name == "[1,1]");
        }

        private void SetPosition(GameObject targetObj) =>
            _eventData.position = Camera.main.WorldToScreenPoint(targetObj.transform.position);

        private void SetPosition(Vector3 worldPos) =>
            _eventData.position = Camera.main.WorldToScreenPoint(worldPos);

        private void PointerDown(GameObject targetObj) =>
            ExecuteEvents.Execute(targetObj, _eventData, ExecuteEvents.pointerDownHandler);

        private void Drag(GameObject targetObj) =>
            ExecuteEvents.Execute(targetObj, _eventData, ExecuteEvents.dragHandler);

        private void DragEnd(GameObject targetObj) =>
            ExecuteEvents.Execute(targetObj, _eventData, ExecuteEvents.endDragHandler);
    }
}