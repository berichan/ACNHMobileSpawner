using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to populate a grid
/// </summary>
public class UI_MapGrid : MonoBehaviour
{
    public UI_MapCell PrefabCell;

    public int NumHorizontal = 1;
    public int NumVertical = 1;

    public List<UI_MapCell> SpawnedCells { get; private set; } = new List<UI_MapCell>();

    public void PopulateGrid()
    {
        PrefabCell.gameObject.SetActive(false);

        // delete existing
        foreach (var cell in SpawnedCells)
            Destroy(cell.gameObject);
        SpawnedCells.Clear();

        // populate the grid
        int spawnCount = NumHorizontal * NumVertical;
        for (int i = 0; i < spawnCount; ++i)
        {
            var newCell = Instantiate(PrefabCell.gameObject);
            newCell.transform.SetParent(PrefabCell.transform.parent, false);
            newCell.gameObject.SetActive(true);

            var cellComponent = newCell.GetComponent<UI_MapCell>();
            SpawnedCells.Add(cellComponent);
        }
    }
}
