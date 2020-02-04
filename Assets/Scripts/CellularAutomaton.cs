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
        public List<Rule> rules = new List<Rule>();
    }

    [System.Serializable]
    public class Rule
    {
        public bool foldedOut;
        public bool[] condition = new bool[9];
        public bool output;
    }

    [CustomEditor(typeof(CellularAutomaton))]
    public class Editor_CellularAutomaton : Editor
    {
        public CellularAutomaton Target { get { return (CellularAutomaton)target; } }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Add rule"))
            {
                Target.rules.Add(new Rule());
            }

            for (int i = 0; i < Target.rules.Count; i++)
            {
                Target.rules[i].foldedOut = EditorGUILayout.Foldout(Target.rules[i].foldedOut, "Rule " + i, true);
                if(Target.rules[i].foldedOut)
                {

                    if (GUILayout.Button("Remove rule " + i))
                    {
                        Target.rules.RemoveAt(i);
                        break;
                    }

                    EditorGUILayout.BeginVertical();
                    for (int y = 0; y < 3; y++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        for (int x = 0; x < 3; x++)
                        {
                            Target.rules[i].condition[y * 3 + x] = EditorGUILayout.Toggle(Target.rules[i].condition[y * 3 + x]);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    Target.rules[i].output = EditorGUILayout.Toggle("Output", Target.rules[i].output);
                }
            }

            EditorUtility.SetDirty(target);
        }
    }

}