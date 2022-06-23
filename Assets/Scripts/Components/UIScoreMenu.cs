using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeConquer.Components
{
    public class ScoreData
    {
        public float PlayerScore;
        public List<float> ScoreList;
    }

    public class UIScoreMenu : UIMenu
    {
        [SerializeField] private Transform ScoreTextTransform;

        public void DisplayScore(ScoreData scoreData)
        {
            int score = (int)(scoreData.PlayerScore * 100f);
            ScoreTextTransform.GetComponent<TMPro.TextMeshProUGUI>().text = "Your score : " + score.ToString() + "%";
        }
    }
}
