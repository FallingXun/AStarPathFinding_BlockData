using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class CubeMoveTest : MonoBehaviour
{
    public Vector2Int m_GridPos = new Vector2Int(-1, -1);
    public PathFindingBlockData m_Block;
    private int m_Width;
    private int m_Height;
    private float m_NodeSize;

    private void Start()
    {
        m_Block = GetComponent<PathFindingBlockData>();

        var graph = PathFindingManager.GetInstance().GetGraph();
        m_Width = graph.width;
        m_Height = graph.depth;
        m_NodeSize = graph.nodeSize;
    }

    [ContextMenu("Execute")]
    private void ChangeTilePosition()
    {
        if (m_GridPos.x >= 0 && m_GridPos.y >= 0)
        {
            PathFindingManager.GetInstance().UpdatePathFindingBlockData(m_Block, m_GridPos);

            transform.position = GridPosToUnityPos(m_GridPos,new Vector2Int(m_Block.m_Width, m_Block.m_Height));
        }
    }


    /// <summary>
    /// 获取物体格子坐标数据处在左下角网格时的实际的Unity坐标
    /// </summary>
    /// <param name="gridCount">物体所占格子数</param>
    /// <returns></returns>
    private Vector3 GetStartUnityPos(Vector2Int gridCount)
    {
        var mapSize = new Vector2Int(m_Width, m_Height);
        var mapCellSize = m_NodeSize;

        // 地图网格从左下角格子坐标(1,1)起，到右上角格子坐标(mapSize.x,mapSize.y)
        // 正中间格子坐标((mapSize.x - 1)/2f, (mapSize.y - 1)/2f) 对应Unity的世界坐标原点(0,0,0)
        // 第一个格子的中心点的Unity坐标为：(0, 0) -（地图格子数量 / 2） * 格子的单位尺寸
        float baseUnityPos_x = -1f * (mapSize.x / 2f * mapCellSize);
        float baseUnityPos_z = -1f * (mapSize.y / 2f * mapCellSize);

        // 当前尺寸的起始Unity坐标
        float startUnityPos_x = baseUnityPos_x + gridCount.x / 2f * mapCellSize;
        float startUnityPos_z = baseUnityPos_x + gridCount.y / 2f * mapCellSize;

        return new Vector3(startUnityPos_x, 0, startUnityPos_z);
    }

    /// <summary>
    /// 格子坐标转换成Unity坐标
    /// </summary>
    /// <param name="gridPos">格子坐标</param>
    /// <param name="gridCount">物体所占格子数</param>
    /// <returns></returns>
    private Vector3 GridPosToUnityPos(Vector2Int gridPos, Vector2Int gridCount)
    {
        Vector3 startUnityPos = GetStartUnityPos(gridCount);

        Vector3 offsetUnityPos = new Vector3(gridPos.x * m_NodeSize, 0, gridPos.y * m_NodeSize);

        return startUnityPos + offsetUnityPos;
    }
}
