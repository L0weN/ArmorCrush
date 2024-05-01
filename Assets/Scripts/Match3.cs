using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellSize = 1;
    [SerializeField] Vector3 originPosition = Vector3.zero;
    [SerializeField] bool debug = true;

    [SerializeField] Crush crushPrefab;
    [SerializeField] CrushType[] crushType;
    [SerializeField] Ease ease = Ease.InQuad;
    [SerializeField] GameObject explosion;

    GridSystem2D<GridObject<Crush>> grid;
    
    InputReader inputReader;
    AudioManager audioManager;

    Vector2Int selectedCrush = Vector2Int.one * -1;

    void Awake()
    {
        inputReader = GetComponent<InputReader>();
        audioManager = GetComponent<AudioManager>();
    }

    void Start()
    {
        InitializeGrid();
        inputReader.Fire += OnSelectCrush;
    }

    void OnDestroy()
    {
        inputReader.Fire -= OnSelectCrush;
    }

    void OnSelectCrush()
    {
        var gridPos = grid.GetXY(Camera.main.ScreenToWorldPoint(inputReader.Selected));

        if (!IsValidPosition(gridPos) || IsEmptyPosition(gridPos)) return;

        if (selectedCrush == gridPos)
        {
            DeselectCrush();
            audioManager.PlayDeselect();
        }
        else if (selectedCrush == Vector2Int.one * -1)
        {
            SelectCrush(gridPos);
            audioManager.PlayClick();
        }
        else
        {
            StartCoroutine(RunGameLoop(selectedCrush, gridPos));
        }
    }

    private bool IsValidPosition(Vector2Int gridPos) => gridPos.x >= 0 && gridPos.y >= 0 && gridPos.x < width && gridPos.y < height;

    private bool IsEmptyPosition(Vector2Int gridPos) => grid.GetValue(gridPos.x, gridPos.y) == null;

    void InitializeGrid()
    {
        grid = GridSystem2D<GridObject<Crush>>.VerticalGrid(width, height, cellSize, originPosition, debug);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CreateCrush(x, y);
            }
        }
    }

    void CreateCrush(int x, int y)
    {
        var crush = Instantiate(crushPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
        crush.SetType(crushType[Random.Range(0, crushType.Length)]);
        var gridObject = new GridObject<Crush>(grid, x, y);
        gridObject.SetValue(crush);
        grid.SetValue(x, y, gridObject);
    }

    void SelectCrush(Vector2Int gridPos) => selectedCrush = gridPos;

    void DeselectCrush() => selectedCrush = new Vector2Int(-1, -1);

    IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
    {
        yield return StartCoroutine(SwapCrush(gridPosA, gridPosB));

        List<Vector2Int> matches = FindMatches();

        yield return StartCoroutine(ExplodeCrush(matches));

        yield return StartCoroutine(FallCrush());

        yield return StartCoroutine(FillEmptyCrush());

        DeselectCrush();
    }

    IEnumerator FillEmptyCrush()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (grid.GetValue(x, y) == null)
                {
                    CreateCrush(x, y);
                    audioManager.PlayPop();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    IEnumerator FallCrush()
    {
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (grid.GetValue(x, y) == null)
                {
                    for (var i = y + 1; i < height; i++)
                    {
                        if (grid.GetValue(x, i) != null)
                        {
                            var crush = grid.GetValue(x, i).GetValue();
                            grid.SetValue(x, y, grid.GetValue(x, i));
                            grid.SetValue(x, i, null);
                            crush.transform.DOLocalMove(grid.GetWorldPositionCenter(x, y), 0.5f).SetEase(ease);
                            audioManager.PlayWoosh();
                            yield return new WaitForSeconds(0.1f);
                            break;
                        }
                    }
                }
            }
        }
    }

    IEnumerator ExplodeCrush(List<Vector2Int> matches)
    {
        audioManager.PlayPop();
        foreach (var match in matches)
        {
            var crush = grid.GetValue(match.x, match.y).GetValue();
            grid.SetValue(match.x, match.y, null);

            ExplodeVFX(match);

            crush.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 1, 0.5f);

            yield return new WaitForSeconds(0.1f);

            crush.DestroyCrush();
        }
    }

    private void ExplodeVFX(Vector2Int match)
    {
        var fx = Instantiate(explosion, transform);
        fx.transform.position = grid.GetWorldPositionCenter(match.x, match.y);
        Destroy(fx, 5f);
    }

    private List<Vector2Int> FindMatches()
    {
        HashSet<Vector2Int> matches = new ();
        // Horizontal
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var crushA = grid.GetValue(x, y);
                var crushB = grid.GetValue(x + 1, y);
                var crushC = grid.GetValue(x + 2, y);

                if (crushA == null || crushB == null || crushC == null) continue;

                if (crushA.GetValue().GetType() == crushB.GetValue().GetType() && crushB.GetValue().GetType() == crushC.GetValue().GetType())
                {
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x + 1, y));
                    matches.Add(new Vector2Int(x + 2, y));
                }
            }
        }

        // Vertical
        for (var x = 0; x < height; x++)
        {
            for (var y = 0; y < width; y++)
            {
                var crushA = grid.GetValue(x, y);
                var crushB = grid.GetValue(x, y + 1);
                var crushC = grid.GetValue(x, y + 2);

                if (crushA == null || crushB == null || crushC == null) continue;

                if (crushA.GetValue().GetType() == crushB.GetValue().GetType() && crushB.GetValue().GetType() == crushC.GetValue().GetType())
                {
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x, y + 1));
                    matches.Add(new Vector2Int(x, y + 2));
                }
            }
        }

        if (matches.Count == 0)
        {
            audioManager.PlayNoMatch();
        }
        else
        {
            audioManager.PlayMatch();
        }

        return new List<Vector2Int>(matches);
    }

    IEnumerator SwapCrush(Vector2Int gridPosA, Vector2Int gridPosB)
    {
        var gridObjectA = grid.GetValue(gridPosA.x, gridPosA.y);
        var gridObjectB = grid.GetValue(gridPosB.x, gridPosB.y);

        gridObjectA.GetValue().transform.DOLocalMove(grid.GetWorldPositionCenter(gridPosB.x, gridPosB.y), 0.5f).SetEase(ease);
        gridObjectB.GetValue().transform.DOLocalMove(grid.GetWorldPositionCenter(gridPosA.x, gridPosA.y), 0.5f).SetEase(ease);

        grid.SetValue(gridPosA.x, gridPosA.y, gridObjectB);
        grid.SetValue(gridPosB.x, gridPosB.y, gridObjectA);

        yield return new WaitForSeconds(0.5f);
    }
}
