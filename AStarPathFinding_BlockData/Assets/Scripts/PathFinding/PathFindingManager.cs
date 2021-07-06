using UnityEngine;
using System.Collections.Generic;
using Pathfinding;


public class PathFindingManager
{
    private static PathFindingManager m_Manager;

    public const string BUILDING_GRAPH = "BuildingGraph";

    private System.Action<GraphNodeData> m_GraphNodeDataAction;

    public static PathFindingManager GetInstance()
    {
        if(m_Manager == null)
        {
            m_Manager = new PathFindingManager();
            m_Manager.Init();
        }
        return m_Manager;
    }

    private void Init()
    {
        //缓存回调方法，每次转换会产生112b的gc，调用频次高gc会很多
        m_GraphNodeDataAction = GraphNodeDataAction;
    }


    /// <summary>
    /// 更新npc寻路阻挡
    /// </summary>
    /// <param name="data"></param>
    /// <param name="tilePos"></param>
    public void UpdatePathFindingBlockData(PathFindingBlockData data, Vector2Int tilePos)
    {
        if (data == null)
        {
            return;
        }
        if (data.m_GridPos == tilePos)
        {
            return;
        }

        // 如果正在使用中，需要清除上一次的位置信息
        SetPathFindingBlockData(data.m_Block, data.m_GridPos, data.m_Width, data.m_Height, false);
        // 设置本次的位置信息
        SetPathFindingBlockData(data.m_Block, tilePos, data.m_Width, data.m_Height, true);

        data.m_GridPos = tilePos;
    }

    /// <summary>
    /// 删除npc寻路阻挡
    /// </summary>
    /// <param name="data"></param>
    public void DeletePathFindingBlockData(PathFindingBlockData data)
    {
        if (data == null)
        {
            return;
        }

        SetPathFindingBlockData(data.m_Block, data.m_GridPos, data.m_Width, data.m_Height, false);

        data.ResetTilePos();
    }

    public void SetPathFindingBlockData(bool[] block, Vector2Int gridPos, int width, int depth, bool blockState)
    {
        if (block == null)
        {
            return;
        }
        var graph = GetGraph();
        if (graph == null)
        {
            return;
        }
        if (gridPos.x < 0 || gridPos.y < 0)
        {
            return;
        }
        int x = gridPos.x;
        int z = gridPos.y;
        bool walkable = blockState == false;


        GraphNodeData nodeData = new GraphNodeData(block, x, z, width, depth, walkable);

        AstarWorkItem workItem = new AstarWorkItem(m_GraphNodeDataAction, nodeData);

        AstarPath.active.AddWorkItem(workItem);

    }

    private void GraphNodeDataAction(GraphNodeData data)
    {
        if (data.block == null)
        {
            return;
        }
        var graph = GetGraph();
        if (graph != null)
        {
            for (int i = 0; i < data.block.Length; i++)
            {
                if (data.block[i])
                {
                    int node_x = data.pos_x + i % data.width;
                    int node_z = data.pos_z + i / data.depth;


                    var node = graph.GetNode(node_x, node_z);

                    node.Walkable = data.walkable;
                }
            }
            // 重新计算连通状态，需要重新计算整个区域，同GraphUpdateObject一致
            for (int i = 0; i < data.block.Length; i++)
            {
                int node_x = data.pos_x + i % data.width;
                int node_z = data.pos_z + i / data.depth;

                graph.CalculateConnections(node_x, node_z);

            }
        }
    }



    public GridGraph GetGraph()
    {
        NavGraph[] graphs = AstarPath.active.data.graphs;

        GridGraph graph = null;

        for (int i = 0; i < graphs.Length; i++)
        {
            if (graphs[i].name == BUILDING_GRAPH)
            {
                graph = graphs[i] as GridGraph;
            }
        }

        return graph;
    }

}
