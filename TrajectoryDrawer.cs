using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrajectoryDrawer : MonoBehaviour
{
    #region variables
    /// <summary>количество преломлений шара для расчета траектории</summary>
    private const int raycount = 3;

    public GameObject reflectionEffect;

    private List<GameObject> allReflections;

    public LineRenderer sightLine;

    [SerializeField] private bool BoundsOnly;
    private const string boundsTag = "bounds";

    [SerializeField] private float fireStrength;

    [SerializeField] private Color renderColor;

    public int segmentCount = 20;

    private const int maxHitCount = 4;

    public Collider hitObject { get; private set; }

    [SerializeField] private PlayerController playerController;

    //Vector3 direction;
    #endregion

    private void Start()
    {
        allReflections = new List<GameObject>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        SetRayasting();
        ClearVisual();
    }

    /// <summary>отрисовать траекторию</summary>
    public void UpdateDirection(int numberOnCollisions = 3, float rayMaxLength = 10f, float lastPartLength = 2f)
    {
        ClearVisual();
        SimulateNewPath(numberOnCollisions, rayMaxLength, lastPartLength);
    }

    public void ClearVisual()
    {
        for (var i = allReflections.Count - 1; i >= 0; i--)
        {
            var r = allReflections[i];
            allReflections.Remove(r);
            Destroy(r);
        }

        ClearVertexData();

        sightLine.enabled = false;
    }

    [SerializeField] private GameObject rendererPrefab;

    private List<LineRenderer> renderers;

    private void SetRayasting()
    {
        renderers = new List<LineRenderer>(raycount);
        for (var i = 0; i < raycount; i++)
        {
            var g = Instantiate(rendererPrefab, transform.position, Quaternion.Euler(Vector3.zero));
            var r = g.GetComponent<LineRenderer>();
            renderers.Add(r);
        }
    }

    //очищаем данные вершин
    private void ClearVertexData()
    {
        foreach (var lr in renderers)
            if (lr)
            {
                lr.positionCount = 0;
                lr.enabled = false;
            }
    }

    //private GameObject[] Balls = new GameObject[3];
    [SerializeField] private float distanceToCollision;
    [SerializeField] private float minimumRayLength;
    private void SimulateNewPath(int collisionsNumber = 3, float rayMaxLength = 10f, float lastPartLength = 2f)
    {
        var pos =  gameObject.transform.position;

        var totalLength = 0f;
        var direction = playerController.direction * 1000f * Time.deltaTime;
        Vector3 prevDirection = Vector3.zero;
        RaycastHit2D hit = new RaycastHit2D();
        RaycastHit2D prevHit = new RaycastHit2D();
        RaycastHit2D[] hits;
        GameObject previousHitObj = null;
        bool firstHitHappened = false;
        for (var i = 0; i < collisionsNumber && i < renderers.Count && totalLength < rayMaxLength; i++)
        {
            if (firstHitHappened) hits = Physics2D.CircleCastAll(pos + new Vector3(hit.normal.x, hit.normal.y, 0f) * 0.2f, 0.2f, direction, 1000f);
            else hits = Physics2D.CircleCastAll(pos, 0.2f, direction, 1000f);

            //сортируем по удаленности (нам нужно первое настоящее столкновение)
            hits = hits.OrderBy(c => c.distance).ToArray();
            for (var j = 0; j < hits.Length; j++)
            {
                hit = hits[j];

                //если объект - триггер, не трогаем его
                if (hit.collider.isTrigger) continue;

                if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer(BallController.lowerBounsTag))) continue;

                //если у объекта невидимый лейер - пропускаем
                if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("invisible"))) continue;
                
                //если выставлена опция только границы, а объект - не граница, пропускаем
                if (BoundsOnly && hit.collider.tag != boundsTag)  continue;
                if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("bossWallBounds"))) continue;
                if (previousHitObj != null && hit.collider.gameObject.Equals(previousHitObj)) continue;
                
                previousHitObj = hit.collider.gameObject;
                if (Vector3.Magnitude(new Vector3(hit.point.x, hit.point.y, 0f) - pos) < minimumRayLength)
                    return;

                //выставляем вертексы для рендера линии
                renderers[i].positionCount = 2;
                renderers[i].enabled = true;
                Vector3 startPosOfRenderer = pos + direction.normalized * distanceToCollision;
                if (firstHitHappened) startPosOfRenderer = pos + SpecialBallRaycastToVectorDeltaTransform(prevHit);
                Vector3 endPosOfRenderer = new Vector3(hit.point.x, hit.point.y, 0f) + SpecialBallRaycastToVectorDeltaTransform(hit);

                prevHit = hit;
                pos = hit.point;

                float distanceBetween = (endPosOfRenderer - startPosOfRenderer).magnitude;
                totalLength += distanceBetween;

                renderers[i].SetPosition(0, startPosOfRenderer);
                renderers[i].SetPosition(1, endPosOfRenderer);

                if(i == collisionsNumber - 1)
                {
                    renderers[i].SetPosition(1, startPosOfRenderer + (endPosOfRenderer - startPosOfRenderer).normalized * lastPartLength);
                    return;
                }

                //если длина (общая) больше длины нашего прицела - не отражает дальше, а просто продолжаем луч до нужной длины
                if (totalLength > rayMaxLength)
                {
                    float rayStartLength = totalLength - distanceBetween;
                    float remainingLength = rayMaxLength - rayStartLength;
                    renderers[i].SetPosition(1, startPosOfRenderer + (endPosOfRenderer - startPosOfRenderer).normalized * remainingLength);

                    return;
                }

                if (i < collisionsNumber - 1)
                {
                    var r = DrawBallOnReflection(pos +  SpecialBallRaycastToVectorDeltaTransform(hit));
                    allReflections.Add(r);
                    if (i == 0)
                        CheckAdditionalCubeFear(hit.collider.gameObject);
                }
                prevDirection = direction;
                direction = Vector2.Reflect(direction, hit.normal);
                firstHitHappened = true;

                //если это последгее столкновение - то мы не ставим отрадение
                if (i == collisionsNumber) return;

                break;
            }
        }
    }

    Vector3 SpecialBallRaycastToVectorDeltaTransform(RaycastHit2D hit)
    {
        return new Vector3(hit.normal.x, hit.normal.y, -0.1f) * 0.2f;
    }

    private GameObject DrawBallOnReflection(Vector3 drawPosition)
    {
        var refl = Instantiate(reflectionEffect, drawPosition, Quaternion.Euler(0f, 0f, 0f));
        return refl;
    }



    CubeController previousCube = null;
    void CheckAdditionalCubeFear(GameObject cubeGO)
    {
        var cubeObj = cubeGO.GetComponent<CubeController>();
        if (cubeObj == null)
            return;

        if(previousCube == null || cubeObj != previousCube)
            cubeObj.PlayFearEffect();

        previousCube = cubeObj;
    }
}