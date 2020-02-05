using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LevelGeneration
{
    /// <summary>
    /// Handles generating a 2d array of points that can be interpreted as passable floor tiles.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public Vector2Int dimensions;

        public CellularAutomaton cellularAutomaton;

        public MeshRenderer meshRenderer;

        public float threshold = 0.5f;

        public float scale = 0.1f;

        public Vector4 offset_range = new Vector4(-100, 100, -100, 100);

        Texture2D tex;
        List<Vector2Int> cells = new List<Vector2Int>();
        public List<Region> regions = new List<Region>();

        /// <summary>
        /// For now just creates a 2d binary map to represent tiles being on / off
        /// Later, we can use this map data to create the floor plane from quads and triangles
        /// based on the layout of the map.
        /// 
        /// The meshRenderer is currently just placeholder to prove the concept and for testing purposes.
        /// </summary>
        public void Generate()
        {
            cells.Clear();
            regions.Clear();

            Random.InitState(Random.Range(int.MinValue, int.MaxValue));

            Vector2 offset = new Vector2(Random.Range(offset_range.x, offset_range.y),
                                         Random.Range(offset_range.z, offset_range.w));

            tex = new Texture2D(dimensions.x, dimensions.y);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            int[,] map = new int[dimensions.x, dimensions.y];
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    float f = Mathf.PerlinNoise(x * scale + offset.x, y * scale + offset.y);
                    if (f >= threshold)
                    {
                        map[x, y] = 1;
                    }
                    else
                        map[x, y] = 0;
                }
            }

            bool[] filter = new bool[9];

            //convolve rules over map
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    if (y - 1 < 0)
                    {
                        filter[0] = filter[1] = filter[2] = false;
                    }
                    else
                    {
                        filter[0] = (x - 1 >= 0) && map[x-1, y-1] == 1;
                        filter[1] = map[x, y-1] == 1;
                        filter[2] = (x + 1 < dimensions.x) && map[x+1, y-1] == 1;
                    }

                    filter[3] = (x - 1 >= 0) && map[x-1, y] == 1;
                    filter[4] = map[x, y] == 1;
                    filter[5] = (x + 1 < dimensions.x) && map[x+1, y] == 1;

                    if (y + 1 >= dimensions.y)
                    {
                        filter[6] = filter[7] = filter[8] = false;
                    }
                    else
                    {
                        filter[6] = (x - 1 >= 0) && map[x-1, y+1] == 1;
                        filter[7] = map[x, y+1] == 1;
                        filter[8] = (x + 1 < dimensions.x) && map[x+1, y+1] == 1;
                    }

                    //if we get a match, skip onto the next point after applying the rule output
                    for (int i = 0; i < cellularAutomaton.rules.Count; i++)
                    {
                        bool satisfied = true;
                        for(int k = 0; k < filter.Length; k++)
                        {
                            if (filter[k] != cellularAutomaton.rules[i].condition[k])
                            {
                                satisfied = false;
                                break;
                            }
                        }

                        if(satisfied)
                        {
                            map[x, y] = cellularAutomaton.rules[i].output == true ? 1 : 0;
                            break;
                        }
                    }
                }
            }

            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    Color c = Color.black;
                    if (map[x,y] == 1)
                    {
                        c = Color.white;
                        cells.Add(new Vector2Int(x, y));
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            meshRenderer.sharedMaterial.SetTexture("_MainTex", tex);
        }

        /// <summary>
        /// Incomplete starter code. Finds regions of cells that are all connected, and creates a list of regions.
        /// TODO: use midpoints of islands to find the closest neighbouring island, then find the closest cells
        /// within those regions to create bridges/paths between them.
        /// 
        /// Cells that have been processed are marked red, and the midpoint of regions are marked green.
        /// </summary>
        public void JoinRegions()
        {
            if (cells.Count == 0)
            {
                Debug.LogWarning("You must first generate a level that contains at least one cell.");
                return;
            }

            List<List<Vector2Int>> _regions = new List<List<Vector2Int>>();
            _regions.Add(new List<Vector2Int>());
            _regions[0].Add(cells[0]);

            tex.SetPixel(cells[0].x, cells[0].y, Color.red);

            List<Vector2Int> queue = new List<Vector2Int>();
            queue.Add(cells[0]);

            int counter = 0;
            int region = 0;

            for (; ; )
            {
                if (tex.GetPixel(cells[counter].x, cells[counter].y) == Color.white)
                    queue.Add(cells[counter]);

                while (queue.Count > 0)
                {
                    Vector2Int cur = queue[queue.Count - 1];

                    if (cells.Contains(cur))
                    {
                        _regions[region].Add(cur);
                        tex.SetPixel(cur.x, cur.y, Color.red);
                    }

                    queue.RemoveAt(queue.Count - 1);

                    if (cur.x + 1 < dimensions.x && tex.GetPixel(cur.x + 1, cur.y) == Color.white)
                        queue.Add(cur + Vector2Int.right);

                    if (cur.x - 1 >= 0 && tex.GetPixel(cur.x - 1, cur.y) == Color.white)
                        queue.Add(cur + Vector2Int.left);

                    if (cur.y + 1 < dimensions.y && tex.GetPixel(cur.x, cur.y + 1) == Color.white)
                        queue.Add(cur + Vector2Int.up);

                    if (cur.y - 1 >= 0 && tex.GetPixel(cur.x, cur.y - 1) == Color.white)
                        queue.Add(cur + Vector2Int.down);
                }

                counter++;

                if (counter == cells.Count)
                    break;

                region = -1;
                for (int i = 0; i < _regions.Count; i++)
                {
                    if (_regions[i].Contains(cells[counter]))
                        region = i;
                }

                if (region == -1)
                {
                    region = _regions.Count;
                    _regions.Add(new List<Vector2Int>());
                    _regions[region].Add(cells[counter]);
                }
            }

            foreach (List<Vector2Int> r in _regions)
            {
                regions.Add(new Region());

                foreach (Vector2Int v in r)
                {
                    regions[regions.Count - 1].cells.Add(v);
                }

                regions[regions.Count - 1].Midpoint();
                Vector2Int cur = regions[regions.Count - 1].midpoint;
                tex.SetPixel(cur.x, cur.y, Color.green);
            }

            tex.Apply();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(LevelGenerator))]
    public class Editor_LevelGenerator : Editor
    {
        public LevelGenerator Target { get { return (LevelGenerator)target; } }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate"))
                Target.Generate();

            if (GUILayout.Button("Join Regions"))
                Target.JoinRegions();

            DrawDefaultInspector();
        }
    }

    /// <summary>
    /// Stores map data into islands that can be connected.
    /// Use midpoints to find closest islands, then search for the cells within those islands which are closest.
    /// </summary>
    [System.Serializable]
    public class Region
    {
        public int id;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public Vector2Int midpoint;

        public void Midpoint()
        {
            int minX = cells[0].x;
            int maxX = cells[0].x;

            int minY = cells[0].y;
            int maxY = cells[0].y;

            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].x < minX)
                    minX = cells[i].x;

                if (cells[i].x > maxX)
                    maxX = cells[i].x;

                if (cells[i].y < minY)
                    minY = cells[i].y;

                if (cells[i].y > maxY)
                    maxY = cells[i].y;
            }

            midpoint = new Vector2Int(minX + Mathf.RoundToInt((maxX - minX) / 2.0f),
                                      minY + Mathf.RoundToInt((maxY - minY) / 2.0f));
        }
    }
}