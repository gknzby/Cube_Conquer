using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CubeConquer.Managers;
using System;
using CubeConquer.Scriptables;

namespace CubeConquer.Components
{
    public class PlayMechanic : MonoBehaviour, IInputReceiver, INewLevelListener
    {
        private int placeCount = 1;

        private Queue<Vector2Int> openQ = new Queue<Vector2Int>();
        private GridMain gridMain = null;

        private Vector2Int clickCellPos;

        private void Start()
        {
            ManagerProvider.GetManager<IInputManager>().SetDefaultReceiver(this);
            AddSelfToTheList();
        }

        private void OnDestroy()
        {
            ManagerProvider.GetManager<IInputManager>()?.RemoveDefaultReceiver(this);
            RemoveSelfFromTheList();
        }

        public void Cancel()
        {
            return;
        }
        public void Drag(Vector2 dragVec)
        {
            return;
        }
        public void Click()
        {
            if(placeCount < 1)
            {
                return;
            }
            OnClick();
        }
        public void Release()
        {
            if(placeCount < 1)
            {
                return;
            }
            OnRelease();
            if(placeCount == 0)
            {
                StartCoroutine(EndLevel());
            }
        }

        private IEnumerator EndLevel()
        {
            yield return StartCoroutine(SpreadColors());

            ScoreData scoreData = GetScoreData();

            yield return StartCoroutine(DisplayScore(scoreData));

            if(DidPlayerWin(scoreData))
            {
                ManagerProvider.GetManager<IGameManager>().SendGameAction(GameAction.Win);
            }
            else
            {
                ManagerProvider.GetManager<IGameManager>().SendGameAction(GameAction.Lost);
            }
        }


        private bool DidPlayerWin(ScoreData scoreData)
        {
            float playerCount = scoreData.ScoreList[1];
            for(int i = 2; i < scoreData.ScoreList.Count; i++)
            {
                if(playerCount < scoreData.ScoreList[i])
                {
                    return false;
                }
            }

            return true;
        }

        private ScoreData GetScoreData()
        {
            List<float> colorCounts = CountColors();
            ScoreData scoreData = new ScoreData();

            float bigSum = 0;
            for (int i = 0; i < colorCounts.Count; i++)
            {
                bigSum += colorCounts[i];
            }
            bigSum = bigSum < 1 ? 1 : bigSum;

            scoreData.PlayerScore = colorCounts[1]/bigSum;
            scoreData.ScoreList = colorCounts;

            return scoreData;
        }

        private IEnumerator DisplayScore(ScoreData scoreData)
        {
            float displayTime = 2f;

            IUIMenu scoreMenu = ManagerProvider.GetManager<IUIManager>().GetMenu("ScoreMenu");
            scoreMenu.ShowMenu();
            ((UIScoreMenu)scoreMenu).DisplayScore(scoreData);

            yield return new WaitForSeconds(displayTime);

            scoreMenu.HideMenu();
        }

        private IEnumerator SpreadColors()
        {
            yield return null;

            List<Vector2Int> cellPosList = gridMain.GetColoredChilds();
            foreach (Vector2Int cellPos in cellPosList)
            {
                openQ.Enqueue(cellPos);
            }

            Coroutine cr = StartCoroutine(ChangeColors(new List<Vector2Int>()));

            while (openQ.Count > 0)
            {
                Debug.Log(openQ.Count);
                yield return cr;
                cellPosList = new List<Vector2Int>();
                Queue<Vector2Int> tempQuery = new Queue<Vector2Int>(openQ);
                openQ.Clear();
                foreach (Vector2Int cellPos in tempQuery)
                {
                    SpreadAround(ref cellPosList, cellPos);
                }
                cr = StartCoroutine(ChangeColors(cellPosList));
            }
        }

        private List<float> CountColors()
        {
            List<float> colorCount = new List<float>();

            for(int i = 0; i < 5; i++)
            {
                colorCount.Add(0f);
            }

            Vector2Int dimensions = gridMain.GetGridDimensions();

            for(int i = 0; i < dimensions.x; i++)
            {
                for(int j = 0; j < dimensions.y; j++)
                {
                    int enumToInt = (int)gridMain.GetGridCellType(i, j);
                    if (enumToInt >= 0)
                    {
                        colorCount[enumToInt]++;
                    }
                }
            }

            return colorCount;
        }

        private void SpreadAround(ref List<Vector2Int> cellPosList, Vector2Int cellPos)
        {
            SpreadAround(ref cellPosList, cellPos.x, cellPos.y);
        }

        private void SpreadAround(ref List<Vector2Int> cellPosList, int x, int y)
        {
            GridCellType gridCellType = gridMain.GetGridCellType(x, y);

            SpreadChild(ref cellPosList, x - 1, y, gridCellType);
            SpreadChild(ref cellPosList, x, y - 1, gridCellType);
            SpreadChild(ref cellPosList, x + 1, y, gridCellType);
            SpreadChild(ref cellPosList, x, y + 1, gridCellType);
        }

        private void SpreadChild(ref List<Vector2Int> cellPosList, int x, int y, GridCellType gridCellType)
        {
            if (gridMain.IsPlaceable(x, y))
            {
                gridMain.ChangeType(x, y, gridCellType);
                cellPosList.Add(new Vector2Int(x, y));
                openQ.Enqueue(new Vector2Int(x, y));
            }
        }

        private IEnumerator ChangeColors(List<Vector2Int> cellPosList)
        {
            foreach(Vector2Int cellPos in cellPosList)
            {
                gridMain.ApplyColor(cellPos);
            }
            //yield return null;
            yield return new WaitForSeconds(0.2f);
        }

        private void OnClick()
        {
            Vector2Int cellPos;
            if(GetCellPos(out cellPos) 
                && gridMain.IsPlaceable(cellPos))
            {
                clickCellPos = cellPos;
            }
            else
            {
                clickCellPos = new Vector2Int(-1, -1);
                Debug.Log("None");
            }
        }

        private void OnRelease()
        {
            Vector2Int cellPos;
            if (GetCellPos(out cellPos) 
                && gridMain.IsPlaceable(cellPos)
                && clickCellPos == cellPos)
            {
                placeCount--;
                gridMain.ApplyColor(cellPos, GridCellType.PlayerColor);
                openQ.Enqueue(cellPos);
            }
        }

        private Ray GetScreenRay()
        {
            Camera mainCam = Camera.main;
            Vector2 mousePos = Input.mousePosition;

            return mainCam.ScreenPointToRay(mousePos);
        }

        private bool GetCellPos(out Vector2Int cellPos)
        {
            Ray screenRay = GetScreenRay();
            RaycastHit hit;

            if(Physics.Raycast(screenRay, out hit) && hit.transform.TryGetComponent<GridMain>(out gridMain))
            {
                cellPos = gridMain.WorldToCellPos(hit.point);

                return true;
            }

            cellPos = Vector2Int.zero;
            return false;
        }

        public void AddSelfToTheList()
        {
            ManagerProvider.GetManager<ILevelManager>()?.AddNewLevelListener(this);
        }

        public void RemoveSelfFromTheList()
        {
            ManagerProvider.GetManager<ILevelManager>()?.RemoveNewLevelListener(this);
        }

        public void NewLevelData(LevelData levelData)
        {
            StopAllCoroutines();
            openQ.Clear();
            clickCellPos = new Vector2Int(-1, -1);
            placeCount = levelData.PlaceableCount;
        }
    }
}
