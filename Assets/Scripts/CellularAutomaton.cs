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

                        if(Target.mapRules[i].condition)
                        {
                            Vector2Int size = Target.mapRules[i].condition.size;

                            for(int y = 0; y < size.y; y++)
                            {
                                EditorGUILayout.BeginHorizontal();
                                for(int x = 0; x < size.x; x++)
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
                                            "\nand should be added clockwise.", GUILayout.MinHeight(30));

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
                            Target.tileRules[i].verts = new Vector3[4];

                            Target.tileRules[i].verts[0] = new Vector3(0.5f, 0, -0.5f);
                            Target.tileRules[i].verts[1] = new Vector3(-0.5f, 0, -0.5f);
                            Target.tileRules[i].verts[2] = new Vector3(-0.5f, 0, 0.5f);
                            Target.tileRules[i].verts[0] = new Vector3(0.5f, 0, 0.5f);

                            Target.tileRules[i].tris = new int[6];
                            Target.tileRules[i].tris[0] = -4;
                            Target.tileRules[i].tris[1] = -3;
                            Target.tileRules[i].tris[2] = -2;

                            Target.tileRules[i].tris[3] = -4;
                            Target.tileRules[i].tris[4] = -2;
                            Target.tileRules[i].tris[5] = -1;
                        }
                        EditorGUILayout.Space();

                        int vertLength = Target.tileRules[i].verts.Length;
                        vertLength = EditorGUILayout.IntField("Verts", vertLength);

                        if (vertLength == 0)
                            vertLength = 3;

                        if (vertLength < Target.tileRules[i].verts.Length)
                        {
                            Vector3[] v = new Vector3[vertLength];
                            for (int k = 0; k < vertLength; k++)
                                v[k] = Target.tileRules[i].verts[k];

                            Target.tileRules[i].verts = v;
                        }
                        else if (vertLength > Target.tileRules[i].verts.Length)
                        {
                            Vector3[] v = new Vector3[vertLength];
                            for (int k = 0; k < Target.tileRules[i].verts.Length; k++)
                                v[k] = Target.tileRules[i].verts[k];

                            Target.tileRules[i].verts = v;
                        }

                        for (int k = 0; k < Target.tileRules[i].verts.Length; k++)
                        {
                            Target.tileRules[i].verts[k] 
                                = EditorGUILayout.Vector3Field(k.ToString(), Target.tileRules[i].verts[k]);

                            if (Target.tileRules[i].verts[k].x < -0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(-0.5f,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           Target.tileRules[i].verts[k].z);
                            else if(Target.tileRules[i].verts[k].x > 0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(0.5f,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           Target.tileRules[i].verts[k].z);

                            if (Target.tileRules[i].verts[k].y < -0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           -0.5f,
                                                                           Target.tileRules[i].verts[k].z);
                            else if (Target.tileRules[i].verts[k].y > 0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           0.5f,
                                                                           Target.tileRules[i].verts[k].z);

                            if (Target.tileRules[i].verts[k].z < -0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].x,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           -0.5f);
                            else if (Target.tileRules[i].verts[k].z > 0.5f)
                                Target.tileRules[i].verts[k] = new Vector3(Target.tileRules[i].verts[k].z,
                                                                           Target.tileRules[i].verts[k].y,
                                                                           0.5f);
                        }

                        if (vertLength == 0)
                            Target.tileRules[i].tris = new int[0];

                        int triLength = Target.tileRules[i].tris.Length;
                        triLength = EditorGUILayout.IntField("Tris", triLength);

                        if (triLength < vertLength)
                            triLength = vertLength;

                        if (triLength % 3 != 0)
                            continue;

                        if (triLength < Target.tileRules[i].tris.Length)
                        {
                            int[] t = new int[triLength];
                            for (int k = 0; k < triLength; k++)
                                t[k] = Target.tileRules[i].tris[k];

                            Target.tileRules[i].tris = t;
                        }
                        else if (triLength > Target.tileRules[i].tris.Length)
                        {
                            int[] t = new int[triLength];
                            for (int k = 0; k < Target.tileRules[i].tris.Length; k++)
                                t[k] = Target.tileRules[i].tris[k];

                            Target.tileRules[i].tris = t;
                        }

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
    }

}