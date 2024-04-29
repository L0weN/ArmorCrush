using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Match3 : MonoBehaviour
{
    [SerializeField] int width = 8;
    [SerializeField] int height = 8;
    [SerializeField] float cellSize = 1;
    [SerializeField] Vector3 originPosition = Vector3.zero;
    [SerializeField] bool debug = true;

    [SerializeField] Crush gemPrefab;
    [SerializeField] CrushType[] gemTypes;
    [SerializeField] Ease ease = Ease.InQuad;

    GridSystem2D<GridObject<Crush>> grid;
    
    InputReader inputReader;

    Vector2Int selectedCrush = Vector2Int.one * -1;

    void Awake()
    {
        inputReader = GetComponent<InputReader>();
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
        }
        else if (selectedCrush == Vector2Int.one * -1)
        {
            SelectCrush(gridPos);
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
                CreateGem(x, y);
            }
        }
    }

    void CreateGem(int x, int y)
    {
        var gem = Instantiate(gemPrefab, grid.GetWorldPositionCenter(x, y), Quaternion.identity, transform);
        gem.SetType(gemTypes[Random.Range(0, gemTypes.Length)]);
        var gridObject = new GridObject<Crush>(grid, x, y);
        gridObject.SetValue(gem);
        grid.SetValue(x, y, gridObject);
    }

    void SelectCrush(Vector2Int gridPos) => selectedCrush = gridPos;

    void DeselectCrush() => selectedCrush = new Vector2Int(-1, -1);

    IEnumerator RunGameLoop(Vector2Int gridPosA, Vector2Int gridPosB)
    {
        yield return StartCoroutine(SwapCrush(gridPosA, gridPosB));
        DeselectCrush();
        yield return null;
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
