using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PathFindingBlockData : MonoBehaviour
{
    [Header("占地尺寸")]
    [ReadOnlyTag]
    public int m_Size = 0;
    [Header("水平格子数")]
    [ReadOnlyTag]
    public int m_Width = 0;
    [Header("竖直格子数")]
    [ReadOnlyTag]
    public int m_Height = 0;
    [Header("阻挡数据")]
    [ReadOnlyTag]
    public bool[] m_Block;

    [Header("格子坐标")]
    public Vector2Int m_GridPos = new Vector2Int(-1, -1);

    public void ResetTilePos()
    {
        m_GridPos  = new Vector2Int(-1, -1);
    }
}
