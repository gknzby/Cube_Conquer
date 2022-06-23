using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CubeConquer.Components
{
    [System.Serializable]
    public enum GridCellType
    {
        Unreachable = -2,
        Wall = -1,
        Blank = 0,
        PlayerColor = 1,
        ColorA = 2,
        ColorB = 3,
        ColorC = 4
    }
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private GameObject TargetObject;

        public void ChangeColor(Material colorMaterial)
        {
            TargetObject.GetComponent<Renderer>().material = colorMaterial;
            PlacementAnimation();
        }

        public void PlacementAnimation()
        {
            StopAllCoroutines();

            StartCoroutine(SlowlyGrow());
        }

        private IEnumerator SlowlyGrow()
        {
            Vector3 beginScale = Vector3.zero;
            Vector3 finalScale = Vector3.one;
            float animationTime = 0.3f;
            float timer = 0f;

            while(timer < animationTime)
            {
                TargetObject.transform.localScale = Vector3.Lerp(beginScale, finalScale, timer/animationTime);
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
            }

            yield return null;
        }
    }
}
