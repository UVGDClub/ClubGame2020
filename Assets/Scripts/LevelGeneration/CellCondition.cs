using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LevelGeneration
{
    /// <summary>
    /// Stores the boolean condition to compare against a map-cell neighbourhood
    /// the condition is a flattened representation of a 2d array (to allow proper serialization in Unity)
    /// 
    /// Currently only 3x3 grids are supported by the level generator.
    /// </summary>
    [CreateAssetMenu(fileName = "new Cell Condition", menuName = "Level Generation/Cell Condition")]
    public class CellCondition : ScriptableObject
    {
        public Vector2Int size = new Vector2Int(3, 3);
        public bool[] condition = new bool[9];

        private int _id = -1;
        public int id
        {
            get
            {
                if (_id == -1)
                {
                    _id = Get3x3AsInt();
                    return _id;
                }
                else
                    return _id;
            }
            set { _id = Get3x3AsInt(); }
        }

        public int Get3x3AsInt()
        {
            if (size.x != 3 || size.y != 3)
                return 0;

            int id = 0;
            for (int i = 0; i < size.x * size.y; i++)
            {
                int val = (condition[i] == true ? 1 : 0);
                id += val << i;
            }

            return id;
        }
    }

    [CustomEditor(typeof(CellCondition))]
    public class Editor_CellCondition : Editor
    {
        public CellCondition Target { get { return (CellCondition)target; } }

        public override void OnInspectorGUI()
        {
            /*Use this to populate rules
             * 
            if(GUILayout.Button("Generate 3x3 Grid Combinations"))
            {
                Generate3x3Grid();
            }*/

            if (GUILayout.Button("Log int id for this layout"))
            {
                Debug.Log(Target.Get3x3AsInt());
            }

            Vector2Int size = EditorGUILayout.Vector2IntField("Dimensions", Target.size);
            if (size.x <= 0 || size.y <= 0 || size.x % 2 != 1 || size.y % 2 != 1)
                return;
            else if (size.x != Target.size.x || size.y != Target.size.y)
            {
                Target.size = size;

                if (size.x * size.y > Target.condition.Length)
                {
                    bool[] c = new bool[size.x * size.y];

                    for (int i = 0; i < Target.condition.Length; i++)
                        c[i] = Target.condition[i];

                    Target.condition = c;
                }
                else if (size.x * size.y < Target.condition.Length)
                {
                    bool[] c = new bool[size.x * size.y];
                    int length = size.x * size.y;
                    for (int i = 0; i < length; i++)
                        c[i] = Target.condition[i];

                    Target.condition = c;
                }
            }

            for (int y = 0; y < size.y; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < size.x; x++)
                {
                    int index = y * size.x + x;
                    bool b = EditorGUILayout.Toggle(Target.condition[index]);
                    if (b != Target.condition[index])
                    {
                        Target.condition[index] = b;
                        Target.id = -1;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Generates 511 assets at target path which are 3x3 cell conditions.
        /// One condition for each state possible in a 3x3 binary matrix.
        /// </summary>
        public void Generate3x3Grid()
        {
            int total = (int)Mathf.Pow(2, 9);

            string path = AssetDatabase.GetAssetPath(Target);
            path = path.Substring(0, path.Length - 7);

            for (int i = 1; i < total; i++)
            {
                bool[] b = new bool[9];
                for (int n = 0; n < 9; n++)
                {
                    b[n] = ((i >> n) & 1) == 1;
                }

                CellCondition cc = new CellCondition();
                cc.size = Vector2Int.one * 3;
                cc.condition = b;
                AssetDatabase.CreateAsset(cc, path + i + ".asset");
            }

        }
    }
}