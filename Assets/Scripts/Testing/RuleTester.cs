using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelGeneration
{
    public class RuleTester : MonoBehaviour
    {
        public int index = 0;
        public CellularAutomaton automaton;
        public LevelGenerator levelGenerator;

        private void Start()
        {
            Draw();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                index++;
                Draw();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                index--;
                Draw();
            }
        }

        public void Draw()
        {
            if (index >= automaton.tileRules.Count)
                index %= automaton.tileRules.Count;
            else if (index < 0)
                index = automaton.tileRules.Count - 1;

            levelGenerator.dimensions = new Vector2Int(automaton.tileRules[index].condition.size.x,
                                                       automaton.tileRules[index].condition.size.y);

            levelGenerator.map = new int[levelGenerator.dimensions.x, levelGenerator.dimensions.y];

            levelGenerator.tex = new Texture2D(automaton.tileRules[index].condition.size.x, 
                                               automaton.tileRules[index].condition.size.y);
            levelGenerator.tex.wrapMode = TextureWrapMode.Clamp;
            levelGenerator.tex.filterMode = FilterMode.Point;

            int size = automaton.tileRules[index].condition.size.x * automaton.tileRules[index].condition.size.y;
            for (int i = 0; i < size; i++)
            {
                int x = i % 3;
                int y = i / 3;
                if (automaton.tileRules[index].condition.condition[i])
                {
                    levelGenerator.map[x, y] = 1;
                    levelGenerator.tex.SetPixel(x, y, new Color(1, 1, 1, 0.5f));
                }
                else
                {
                    levelGenerator.map[x, y] = 0;
                    levelGenerator.tex.SetPixel(x, y, Color.clear);
                }
            }

            levelGenerator.tex.Apply();

            levelGenerator.mapRenderer.sharedMaterial.SetTexture("_MainTex", levelGenerator.tex);
            levelGenerator.GenerateMesh();
        }
    }
}

