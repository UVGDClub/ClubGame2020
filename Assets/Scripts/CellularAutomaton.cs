using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LevelGeneration
{
    /// <summary>
    /// Provides a rule set to convolve over a 2d array of bits to generate a level.
    /// </summary>
    [CreateAssetMenu(fileName = "new Cellular Automata", menuName = "Level Generation/Cellular Automaton")]
    public class CellularAutomaton : ScriptableObject
    {
        public List<MapRule> mapRules = new List<MapRule>();
        public List<TileRule> tileRules = new List<TileRule>();

        public bool mapFoldout;
        public bool tileFoldout;

        public string tileRulePath = "";
    }

    /// <summary>
    /// If the condition is satisfied, the center point we are
    /// convolving around is set to the value of the output.
    /// </summary>
    [System.Serializable]
    public class MapRule
    {
        public bool foldedOut;
        public CellCondition condition;
        public bool output;
    }

    /// <summary>
    /// Only rules which have a true value at their centre should be used here.
    /// Verts and tris are used to compute the mesh associated with a tile that satisfies this rule.
    /// Each vertex should be offset by 1 or -1 in the x and z axis to stay on the grid.
    /// Triangles should wind in a clockwise direction, starting from the bottom right corner (1,1).
    /// </summary>
    [System.Serializable]
    public class TileRule
    {
        public bool foldedOut;
        public CellCondition condition;
        public Vector3[] verts;
        public int[] tris;
    }

    /// <summary>
    /// CellCondition toggles are purposefully prevented from being set in this editor.
    /// To prevent mistakes, they must be set in the CellCondition editor.
    /// </summary>
    [CustomEditor(typeof(CellularAutomaton))]
    public class Editor_CellularAutomaton : Editor
    {
        public CellularAutomaton Target { get { return (CellularAutomaton)target; } }

        public override void OnInspectorGUI()
        {
            Target.mapFoldout = EditorGUILayout.Foldout(Target.mapFoldout, "Map Rules", true);
            if (Target.mapFoldout)
            {
                if (GUILayout.Button("Add map rule"))
                {
                    Target.mapRules.Add(new MapRule());
                }

                for (int i = 0; i < Target.mapRules.Count; i++)
                {
                    Target.mapRules[i].foldedOut
                        = EditorGUILayout.Foldout(Target.mapRules[i].foldedOut, "Map Rule " + i, true);
                    if (Target.mapRules[i].foldedOut)
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Remove map rule " + i))
                        {
                            Target.mapRules.RemoveAt(i);
                            break;
                        }
                        EditorGUILayout.Space();

                        Target.mapRules[i].condition = (CellCondition)EditorGUILayout.ObjectField(
                                                         Target.mapRules[i].condition, typeof(CellCondition), false);

                        if (Target.mapRules[i].condition)
                        {
                            Vector2Int size = Target.mapRules[i].condition.size;

                            for (int y = 0; y < size.y; y++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                for (int x = 0; x < size.x; x++)
                                {
                                    int index = y * size.x + x;
                                    EditorGUILayout.Toggle(Target.mapRules[i].condition.condition[index]);
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorUtility.SetDirty(Target.mapRules[i].condition);
                        }

                        Target.mapRules[i].output = EditorGUILayout.Toggle("Output", Target.mapRules[i].output);

                        EditorGUILayout.Separator();
                    }
                }
            }

            Target.tileFoldout = EditorGUILayout.Foldout(Target.tileFoldout, "Tile Rules", true);
            if (Target.tileFoldout)
            {
                EditorGUILayout.LabelField("Tris count back from the length of the vertex array" +
                                            "\nand should be added clockwise. 'Add' presets are exterior to the current cell.", GUILayout.MinHeight(30));

                if (Target.tileRulePath.Length == 0)
                    Target.tileRulePath = AssetDatabase.GetAssetPath(target);

                Target.tileRulePath = EditorGUILayout.TextField("Tile Rule Condition Path", Target.tileRulePath);

                GUI.color = Color.red;
                if (GUILayout.Button("Clear all tiles rules"))
                    Target.tileRules.Clear();

                GUI.color = Color.white;

                if (GUILayout.Button("Add tile rule for each condition with centre == true"))
                {
                    //for testing >> this passed the test :)
                    //List<int> layoutIDs = new List<int>();

                    List<CellCondition> cc = new List<CellCondition>();
                    string[] files = System.IO.Directory.GetFiles(Target.tileRulePath);
                    foreach (string file in files)
                    {
                        if (file.EndsWith(".meta"))
                            continue;

                        string name = System.IO.Path.GetFileName(file);

                        cc.Add(AssetDatabase.LoadAssetAtPath<CellCondition>(Target.tileRulePath + name));

                        int id = cc[cc.Count - 1].Get3x3AsInt();
                        /*if (layoutIDs.Contains(id))
                        {
                            Debug.LogError(id + " >> IDs are not unique!");
                        }
                        else
                            layoutIDs.Add(id);*/
                    }

                    Debug.Log(cc.Count);

                    for (int i = 0; i < cc.Count && cc[i] != null;)
                    {
                        int middle = (cc[i].size.x * cc[i].size.y - 1) / 2;
                        if (!cc[i].condition[middle])
                        {
                            i++;
                            continue;
                        }

                        bool found = false;
                        for (int k = 0; k < Target.tileRules.Count; k++)
                        {
                            if (cc[i] == Target.tileRules[k].condition)
                            {
                                found = true;
                                i++;
                                break;
                            }
                        }

                        if (found == false)
                        {
                            TileRule t = new TileRule();
                            t.condition = cc[i];

                            if (cc[i].condition.Length == 9)
                            {
                                int counter = 0;
                                if (t.condition.condition[1])
                                    counter++;
                                if (t.condition.condition[3])
                                    counter++;
                                if (t.condition.condition[5])
                                    counter++;
                                if (t.condition.condition[7])
                                    counter++;

                                if (counter == 1 && !t.condition.condition[0] && !t.condition.condition[2]
                                    && !t.condition.condition[6] && !t.condition.condition[8])
                                {
                                    if (t.condition.condition[1])
                                        UseBottomQuarterTri(t);
                                    else if (t.condition.condition[3])
                                        UseLeftQuarterTri(t);
                                    else if (t.condition.condition[5])
                                        UseRightQuarterTri(t);
                                    else if (t.condition.condition[7])
                                        UseTopQuarterTri(t);

                                }
                                else
                                {
                                    int id = t.condition.Get3x3AsInt();
                                    if (id == 10)
                                        UseTopLeftTri(t);
                                    else if (id == 34)
                                        UseTopRightTri(t);
                                    else if (id == 136)
                                        UseBottomLeftTri(t);
                                    else if (id == 160)
                                        UseBottomRightTri(t);
                                    else
                                    {
                                        UseDefaultSquare(t);

                                        if (!t.condition.condition[1]
                                                && (t.condition.condition[0] || t.condition.condition[2]))
                                        { AddTopQuarterTile(t); }

                                        if (!t.condition.condition[3]
                                            && (t.condition.condition[0] || t.condition.condition[6]))
                                        { AddLeftQuarterTile(t); }

                                        if (!t.condition.condition[5]
                                            && (t.condition.condition[2] || t.condition.condition[8]))
                                        { AddRightQuarterTile(t); }

                                        if (!t.condition.condition[7]
                                            && (t.condition.condition[6] || t.condition.condition[8]))
                                        { AddBottomQuarterTile(t); }
                                    }
                                }
                            }
                            else
                                UseDefaultSquare(t);

                            Target.tileRules.Add(t);
                            i++;
                        }
                    }
                }

                EditorGUILayout.Separator();

                if (GUILayout.Button("Add tile rule"))
                {
                    Target.tileRules.Add(new TileRule());
                }

                for (int i = 0; i < Target.tileRules.Count; i++)
                {
                    Target.tileRules[i].foldedOut
                        = EditorGUILayout.Foldout(Target.tileRules[i].foldedOut, "Tile Rule " + i, true);
                    if (Target.tileRules[i].foldedOut)
                    {
                        EditorGUILayout.Space();
                        if (GUILayout.Button("Remove tile rule " + i))
                        {
                            Target.tileRules.RemoveAt(i);
                            break;
                        }
                        EditorGUILayout.Space();

                        Target.tileRules[i].condition = (CellCondition)EditorGUILayout.ObjectField(
                                                         Target.tileRules[i].condition, typeof(CellCondition), false);

                        if (Target.tileRules[i].condition)
                        {
                            Vector2Int size = Target.tileRules[i].condition.size;

                            for (int y = 0; y < size.y; y++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                for (int x = 0; x < size.x; x++)
                                {
                                    int index = y * size.x + x;
                                    EditorGUILayout.Toggle(Target.tileRules[i].condition.condition[index]);
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            EditorUtility.SetDirty(Target.tileRules[i].condition);
                        }

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Use default square"))
                        {
                            UseDefaultSquare(Target.tileRules[i]);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Use top-left tri"))
                        {
                            UseTopLeftTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use top-right tri"))
                        {
                            UseTopRightTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use bottom-left tri"))
                        {
                            UseBottomLeftTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use bottom-right tri"))
                        {
                            UseBottomRightTri(Target.tileRules[i]);
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Use top quarter tri"))
                        {
                            UseTopQuarterTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use bottom quarter tri"))
                        {
                            UseBottomQuarterTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use left quarter tri"))
                        {
                            UseLeftQuarterTri(Target.tileRules[i]);
                        }

                        if (GUILayout.Button("Use right quarter tri"))
                        {
                            UseRightQuarterTri(Target.tileRules[i]);
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();

                        EditorGUILayout.Space();
                        EditorGUILayout.BeginHorizontal();
                        GUI.color = Color.yellow;
                        if (GUILayout.Button("Add left quarter-tile"))
                        {
                            AddLeftQuarterTile(Target.tileRules[i]);
                        }
                        EditorGUILayout.Space();

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Add top quarter-tile"))
                        {
                            AddTopQuarterTile(Target.tileRules[i]);
                        }
                        EditorGUILayout.Space();

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Add right quarter-tile"))
                        {
                            AddRightQuarterTile(Target.tileRules[i]);
                        }
                        EditorGUILayout.Space();

                        EditorGUILayout.Space();
                        if (GUILayout.Button("Add bottom quarter-tile"))
                        {
                            AddBottomQuarterTile(Target.tileRules[i]);
                        }
                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Separator();

                        int vertLength = Target.tileRules[i].verts.Length;
                        int triLength = Target.tileRules[i].tris.Length;

                        vertLength = EditorGUILayout.IntField("Verts", vertLength);

                        if (vertLength == 0)
                            vertLength = 3;

                        ResizeVerts(i, vertLength);

                        for (int k = 0; k < Target.tileRules[i].verts.Length; k++)
                        {
                            Target.tileRules[i].verts[k]
                                = EditorGUILayout.Vector3Field(k.ToString(), Target.tileRules[i].verts[k]);

                            if (Target.tileRules[i].verts[k].x < -1f)
                                Target.tileRules[i].verts[k] = new Vector3(-1f,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           Target.tileRules[i].verts[k].z);
                            else if (Target.tileRules[i].verts[k].x > 1f)
                                Target.tileRules[i].verts[k] = new Vector3(1f,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           Target.tileRules[i].verts[k].z);

                            if (Target.tileRules[i].verts[k].y < -1f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           -1f,
                                                                           Target.tileRules[i].verts[k].z);
                            else if (Target.tileRules[i].verts[k].y > 1f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           1f,
                                                                           Target.tileRules[i].verts[k].z);

                            if (Target.tileRules[i].verts[k].z < -1f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           -1f);
                            else if (Target.tileRules[i].verts[k].z > 1f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].z,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           1f);
                        }

                        if (vertLength == 0)
                            Target.tileRules[i].tris = new int[0];

                        triLength = EditorGUILayout.IntField("Tris", triLength);

                        if (triLength < vertLength)
                            triLength = vertLength;

                        if (triLength % 3 != 0)
                            continue;

                        ResizeTris(i, triLength);

                        for (int k = 0; k < Target.tileRules[i].tris.Length; k++)
                        {
                            Target.tileRules[i].tris[k]
                                = EditorGUILayout.IntField(k.ToString(), Target.tileRules[i].tris[k]);

                            if (Target.tileRules[i].tris[k] >= 0)
                                Target.tileRules[i].tris[k] = -1;
                        }
                    }
                }
            }

            EditorUtility.SetDirty(target);
        }

        public void ResizeVerts(int index, int vertLength)
        {
            if (vertLength < Target.tileRules[index].verts.Length)
            {
                Vector3[] v = new Vector3[vertLength];
                for (int k = 0; k < vertLength; k++)
                    v[k] = Target.tileRules[index].verts[k];

                Target.tileRules[index].verts = v;
            }
            else if (vertLength > Target.tileRules[index].verts.Length)
            {
                Vector3[] v = new Vector3[vertLength];
                for (int k = 0; k < Target.tileRules[index].verts.Length; k++)
                    v[k] = Target.tileRules[index].verts[k];

                Target.tileRules[index].verts = v;
            }
        }

        public void ResizeVerts(TileRule t, int vertLength)
        {
            if (vertLength < t.verts.Length)
            {
                Vector3[] v = new Vector3[vertLength];
                for (int k = 0; k < vertLength; k++)
                    v[k] = t.verts[k];

                t.verts = v;
            }
            else if (vertLength > t.verts.Length)
            {
                Vector3[] v = new Vector3[vertLength];
                for (int k = 0; k < t.verts.Length; k++)
                    v[k] = t.verts[k];

                t.verts = v;
            }
        }

        public void ResizeTris(int index, int triLength)
        {
            if (triLength < Target.tileRules[index].tris.Length)
            {
                int[] t = new int[triLength];
                for (int k = 0; k < triLength; k++)
                    t[k] = Target.tileRules[index].tris[k];

                Target.tileRules[index].tris = t;
            }
            else if (triLength > Target.tileRules[index].tris.Length)
            {
                int diff = Target.tileRules[index].tris.Length - triLength;

                int[] t = new int[triLength];
                for (int k = 0; k < Target.tileRules[index].tris.Length; k++)
                    t[k] = Target.tileRules[index].tris[k] + diff;

                Target.tileRules[index].tris = t;
            }
        }

        public void ResizeTris(TileRule t, int triLength)
        {
            if (triLength < t.tris.Length)
            {
                int[] tris = new int[triLength];
                for (int k = 0; k < triLength; k++)
                    tris[k] = t.tris[k];

                t.tris = tris;
            }
            else if (triLength > t.tris.Length)
            {
                int diff = t.tris.Length - triLength;

                int[] tris = new int[triLength];
                for (int k = 0; k < t.tris.Length; k++)
                    tris[k] = t.tris[k] + diff;

                t.tris = tris;
            }
        }

        public void UseDefaultSquare(TileRule t)
        {
            t.verts = new Vector3[4];

            t.verts[0] = new Vector3(0.5f, 0, -0.5f);
            t.verts[1] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);
            t.verts[3] = new Vector3(0.5f, 0, 0.5f);

            t.tris = new int[6];
            t.tris[0] = -4;
            t.tris[1] = -3;
            t.tris[2] = -2;

            t.tris[3] = -4;
            t.tris[4] = -2;
            t.tris[5] = -1;
        }

        /// <summary>
        /// Tri used is interior to the cell in question
        /// </summary>
        /// <param name="t"></param>
        public void UseTopQuarterTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(0, 0, 0);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        /// <summary>
        /// Tri used is interior to the cell in question
        /// </summary>
        /// <param name="t"></param>
        public void UseRightQuarterTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(0, 0, 0);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        /// <summary>
        /// Tri used is interior to the cell in question
        /// </summary>
        /// <param name="t"></param>
        public void UseBottomQuarterTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, -0.5f);
            t.verts[1] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(0, 0, 0);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        /// <summary>
        /// Tri used is interior to the cell in question
        /// </summary>
        /// <param name="t"></param>
        public void UseLeftQuarterTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0, 0, 0);
            t.verts[1] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        public void UseTopLeftTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        public void UseTopRightTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        public void UseBottomLeftTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, 0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        public void UseBottomRightTri(TileRule t)
        {
            t.verts = new Vector3[3];

            t.verts[0] = new Vector3(0.5f, 0, 0.5f);
            t.verts[1] = new Vector3(0.5f, 0, -0.5f);
            t.verts[2] = new Vector3(-0.5f, 0, -0.5f);

            t.tris = new int[3];
            t.tris[0] = -3;
            t.tris[1] = -2;
            t.tris[2] = -1;
        }

        public void AddLeftQuarterTile(TileRule t)
        {
            ResizeVerts(t, t.verts.Length + 3);

            t.verts[t.verts.Length - 3] = new Vector3(-0.5f, 0, -0.5f);
            t.verts[t.verts.Length - 2] = new Vector3(-1f, 0, 0);
            t.verts[t.verts.Length - 1] = new Vector3(-0.5f, 0, 0.5f);

            ResizeTris(t, t.tris.Length + 3);

            t.tris[t.tris.Length - 3] = -3;
            t.tris[t.tris.Length - 2] = -2;
            t.tris[t.tris.Length - 1] = -1;
        }

        public void AddRightQuarterTile(TileRule t)
        {
            ResizeVerts(t, t.verts.Length + 3);

            t.verts[t.verts.Length - 3] = new Vector3(1f, 0, 0);
            t.verts[t.verts.Length - 2] = new Vector3(0.5f, 0, -0.5f);
            t.verts[t.verts.Length - 1] = new Vector3(0.5f, 0, 0.5f);

            ResizeTris(t, t.tris.Length + 3);

            t.tris[t.tris.Length - 3] = -3;
            t.tris[t.tris.Length - 2] = -2;
            t.tris[t.tris.Length - 1] = -1;
        }

        public void AddTopQuarterTile(TileRule t)
        {
            ResizeVerts(t, t.verts.Length + 3);

            t.verts[t.verts.Length - 3] = new Vector3(0.5f, 0, -0.5f);
            t.verts[t.verts.Length - 2] = new Vector3(0, 0, -1f);
            t.verts[t.verts.Length - 1] = new Vector3(-0.5f, 0, -0.5f);

            ResizeTris(t, t.tris.Length + 3);

            t.tris[t.tris.Length - 3] = -3;
            t.tris[t.tris.Length - 2] = -2;
            t.tris[t.tris.Length - 1] = -1;
        }

        public void AddBottomQuarterTile(TileRule t)
        {
            ResizeVerts(t, t.verts.Length + 3);

            t.verts[t.verts.Length - 3] = new Vector3(0.5f, 0, 0.5f);
            t.verts[t.verts.Length - 2] = new Vector3(-0.5f, 0, 0.5f);
            t.verts[t.verts.Length - 1] = new Vector3(0, 0, 1f);

            ResizeTris(t, t.tris.Length + 3);

            t.tris[t.tris.Length - 3] = -3;
            t.tris[t.tris.Length - 2] = -2;
            t.tris[t.tris.Length - 1] = -1;
        }
    }

}