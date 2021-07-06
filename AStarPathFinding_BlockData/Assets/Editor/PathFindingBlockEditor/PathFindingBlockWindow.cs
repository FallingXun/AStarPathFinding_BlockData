using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;

public class PathFindingBlockWindow : EditorWindow
{
    public int m_Size = 0;
    public int m_Width = 0;
    public int m_Height = 0;
    public bool m_ShowTotalArea = true;
    public bool m_ShowEditArea = true;
    public GameObject m_SelectObject = null;
    public Dictionary<int, bool> m_Block = new Dictionary<int, bool>();
    private float m_Unit = 0.5f;    //单位尺寸
    private int m_Scale = 3;        //倍数

    private Material lineMaterial;
    private Vector2 m_ScrollPos = Vector2.zero;


    [MenuItem("PathFindingTools/寻路Block编辑工具")]
    public static void CreateWindow()
    {
        var window = GetWindow<PathFindingBlockWindow>("PathFindingBlockWindow", true);
        window.Show();
    }


    void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        m_SelectObject = null;
    }

    private void OnSceneGUI(SceneView view)
    {
        if (m_SelectObject == null)
        {
            return;
        }
        DrawGrid();
    }

    private void OnGUI()
    {
        m_SelectObject = Selection.activeGameObject;

        var go = m_SelectObject;
        if (go == null)
        {
            return;
        }

        m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, false, false);
        EditorGUILayout.Space();

        m_Size = Mathf.Max(1, EditorGUILayout.IntField("占地尺寸", m_Size));

        m_Width = m_Size * m_Scale;
        m_Height = m_Size * m_Scale;

        EditorGUILayout.LabelField(string.Format("格子大小 {0}", m_Unit));
        EditorGUILayout.LabelField(string.Format("单位尺寸对应格子数量 {0}", m_Scale));
        EditorGUILayout.LabelField(string.Format("水平格子数 {0}", m_Width));
        EditorGUILayout.LabelField(string.Format("竖直格子数 {0}", m_Height));


        EditorGUILayout.Space();
        EditorGUILayout.Space();
        var btnSize = 40f;
        var rect = GUILayoutUtility.GetRect(btnSize * m_Width, btnSize * m_Height);
        var tile = new Rect(rect.x, rect.y, btnSize, btnSize);
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                tile.x = rect.x + tile.width * i;
                tile.y = rect.y + tile.height * (m_Height - 1 - j);     //从左下角开始
                var btnRect = new Rect(tile.x, tile.y, tile.width + 1f, tile.height + 1f);
                int key = GetBlockKey(i, j);
                m_Block.TryGetValue(key, out bool state);
                if (state)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = Color.white;
                }
                if (GUI.Button(btnRect, string.Empty))
                {
                    m_Block[key] = !state;
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUI.color = Color.white;
        if (GUILayout.Button("自动设置Block"))
        {
            AutoSetData(go);
        }
        EditorGUILayout.Space();

        if (GUILayout.Button("读取Block"))
        {
            LoadData(go);
        }
        EditorGUILayout.Space();

        if (GUILayout.Button("保存Block"))
        {
            SaveData(go);
        }
        EditorGUILayout.Space();

        if (m_ShowTotalArea == false)
        {
            if (GUILayout.Button("显示占地范围"))
            {
                m_ShowTotalArea = true;
            }
        }
        else
        {
            if (GUILayout.Button("隐藏占地范围"))
            {
                m_ShowTotalArea = false;
            }
        }

        EditorGUILayout.Space();

        if (m_ShowEditArea == false)
        {
            if (GUILayout.Button("显示编辑范围"))
            {
                m_ShowEditArea = true;
            }
        }
        else
        {
            if (GUILayout.Button("隐藏编辑范围"))
            {
                m_ShowEditArea = false;
            }
        }

        EditorGUILayout.EndScrollView();

        SceneView.RepaintAll();
    }

    /// <summary>
    /// 自动设置
    /// </summary>
    /// <param name="go"></param>
    private void AutoSetData(GameObject go)
    {
        if (go == null)
        {
            return;
        }
        m_Block.Clear();
        // 自动设置除最外层一圈的数据
        for (int i = 1; i < m_Width - 1; i++)
        {
            for (int j = 1; j < m_Height - 1; j++)
            {
                int key = GetBlockKey(i, j);
                m_Block[key] = true;
            }
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="go"></param>
    private void LoadData(GameObject go)
    {
        m_Block.Clear();
        if (go == null)
        {
            return;
        }
        PathFindingBlockData data = go.GetComponent<PathFindingBlockData>();
        if (data == null)
        {
            return;
        }
        if (data.m_Size > 0)
        {
            m_Size = data.m_Size;
        }
        if (data.m_Width > 0 && data.m_Height > 0)
        {
            m_Width = data.m_Width;
            m_Height = data.m_Height;
            if (data.m_Block != null)
            {
                for (int i = 0; i < data.m_Block.Length; i++)
                {
                    if (data.m_Block[i])
                    {
                        int x = i % data.m_Width;
                        int y = i / data.m_Height;
                        m_Block[GetBlockKey(x, y)] = data.m_Block[i];
                    }
                }
            }
        }
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    /// <param name="go"></param>
    private void SaveData(GameObject go)
    {
        if (go == null)
        {
            return;
        }
        PathFindingBlockData data = go.GetComponent<PathFindingBlockData>();
        if (data == null)
        {
            data = go.AddComponent<PathFindingBlockData>();
        }
        data.m_Size = m_Size;
        data.m_Width = m_Width;
        data.m_Height = m_Height;
        data.m_Block = new bool[m_Width * m_Height];
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                int key = GetBlockKey(i, j);
                if (m_Block.TryGetValue(key, out bool state))
                {
                    data.m_Block[i + j * m_Height] = state;
                }
            }
        }
        m_Block.Clear();
        // 预制体保存数据要加这一步
        var prefabStage = PrefabStageUtility.GetPrefabStage(go);
        if (prefabStage != null)
        {
            EditorSceneManager.MarkSceneDirty(prefabStage.scene);
        }
        EditorUtility.SetDirty(go);
    }

    private void DrawGrid()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);
        GL.PushMatrix();

        if (m_ShowTotalArea)
        {
            GL.Begin(GL.QUADS);
            GL.Color(Color.green);
            GL.Vertex(new Vector3(-m_Width / 2f * m_Unit, 0f, -m_Height / 2f * m_Unit));
            GL.Vertex(new Vector3(-m_Width / 2f * m_Unit, 0f, m_Height / 2f * m_Unit));
            GL.Vertex(new Vector3(m_Width / 2f * m_Unit, 0f, m_Height / 2f * m_Unit));
            GL.Vertex(new Vector3(m_Width / 2f * m_Unit, 0f, -m_Height / 2f * m_Unit));
            GL.End();
        }


        if (m_ShowEditArea)
        {
            foreach (var item in m_Block)
            {
                if (item.Value)
                {
                    int x = item.Key % 100;
                    int y = item.Key / 100;
                    GL.Begin(GL.QUADS);
                    GL.Color(Color.red);
                    GL.Vertex(new Vector3((x + 1 - m_Width / 2f) * m_Unit, 0f, (y - m_Height / 2f) * m_Unit));
                    GL.Vertex(new Vector3((x - m_Width / 2f) * m_Unit, 0f, (y - m_Height / 2f) * m_Unit));
                    GL.Vertex(new Vector3((x - m_Width / 2f) * m_Unit, 0f, (y + 1 - m_Height / 2f) * m_Unit));
                    GL.Vertex(new Vector3((x + 1 - m_Width / 2f) * m_Unit, 0f, (y + 1 - m_Height / 2f) * m_Unit));
                    GL.End();
                }
            }
        }

        GL.PopMatrix();

    }

    /// <summary>
    /// 创建一个材质球
    /// </summary>
    private void CreateLineMaterial()
    {
        //如果材质球不存在
        if (!lineMaterial)
        {
            //用代码的方式实例一个材质球
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            //设置参数
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            //设置参数
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            //设置参数
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    private int GetBlockKey(int x, int y)
    {
        return y * 100 + x;
    }
}
