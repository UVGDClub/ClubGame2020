using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LevelGeneration
{
    /// <summary>
    /// Handles generating a 2d array of points that can be interpreted as passable floor tiles.
    /// 
    /// Currently only supports 3x3 cellular automata rules.
    /// 
    /// TODO:
    /// Look at storing the 3x3 grid ids of cells in the map when we resolve regions.
    /// Then when we just need to update cells when we add paths between regions
    /// and look at each cell in the map only once to generate the map.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public Vector2Int dimensions;

        public CellularAutomaton cellularAutomaton;

        public MeshRenderer mapRenderer;
        public MeshFilter meshFilter;

        public float noiseThreshold = 0.5f;
        public float noiseScale = 0.1f;
        public Vector4 noise_offset_range = new Vector4(-100, 100, -100, 100);

        public int minRegionSize = 5;

        public int[,] map;
        List<Region> regions = new List<Region>();
        List<Vector2Int> cells = new List<Vector2Int>();

        [HideInInspector] public Texture2D tex;

        /// <summary>
        /// For now just creates a 2d binary map to represent tiles being on / off
        /// Later, we can use this map data to create the floor plane from quads and triangles
        /// based on the layout of the map.
        /// 
        /// The meshRenderer is currently just placeholder to prove the concept and for testing purposes.
        /// </summary>
        public void GenerateMap()
        {
            cells.Clear();
            regions.Clear();

            Random.InitState(Random.Range(int.MinValue, int.MaxValue));

            Vector2 offset = new Vector2(Random.Range(noise_offset_range.x, noise_offset_range.y),
                                         Random.Range(noise_offset_range.z, noise_offset_range.w));

            tex = new Texture2D(dimensions.x, dimensions.y);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;

            map = new int[dimensions.x, dimensions.y];
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    float f = Mathf.PerlinNoise(x * noiseScale + offset.x, y * noiseScale + offset.y);
                    if (f >= noiseThreshold)
                    {
                        map[x, y] = 1;
                    }
                    else
                        map[x, y] = 0;
                }
            }

            //convolve rules over map
            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    for (int i = 0; i < cellularAutomaton.mapRules.Count; i++)
                    {
                        Vector2Int size = cellularAutomaton.mapRules[i].condition.size;
                        Vector2Int edge = new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);
                        bool[] filter = new bool[size.x * size.y];

                        for (int yf = 0; yf < size.y; yf++)
                        {
                            for (int xf = 0; xf < size.x; xf++)
                            {
                                Vector2Int cur = new Vector2Int(x + xf - edge.x, y + yf - edge.y);

                                if (cur.x < 0 || cur.x >= dimensions.x || cur.y < 0 || cur.y >= dimensions.y)
                                    filter[yf * size.x + xf] = false;
                                else
                                    filter[yf * size.x + xf] = map[cur.x, cur.y] == 1;
                            }
                        }

                        //if we get a match, skip onto the next point after applying the rule output
                        bool satisfied = true;
                        for (int k = 0; k < filter.Length; k++)
                        {
                            if (filter[k] != cellularAutomaton.mapRules[i].condition.condition[k])
                            {
                                satisfied = false;
                                break;
                            }
                        }

                        if (satisfied)
                        {
                            map[x, y] = cellularAutomaton.mapRules[i].output == true ? 1 : 0;
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
                    if (map[x, y] == 1)
                    {
                        c = Color.white;
                        cells.Add(new Vector2Int(x, y));
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            mapRenderer.sharedMaterial.SetTexture("_MainTex", tex);
        }

        /// <summary>
        /// Finds regions of cells that are all connected, and creates a list of regions.
        /// Cells that have been processed are marked red, and the midpoint of regions are marked green.
        /// </summary>
        public void FindRegions()
        {
            if (cells.Count == 0)
            {
                Debug.LogWarning("You must first generate a level that contains at least one cell.");
                return;
            }

            if (regions.Count > 0)
            {
                Debug.LogWarning("Regions have already been found");
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

            int id = 0;
            foreach (List<Vector2Int> r in _regions)
            {
                if (r.Count < minRegionSize)
                {
                    foreach (Vector2Int v in r)
                    {
                        tex.SetPixel(v.x, v.y, Color.black);
                        map[v.x, v.y] = 0;
                    }
                    continue;
                }

                regions.Add(new Region(id++));

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

        /// <summary>
        /// Connects the regions that are closest together until all regions are reachable from
        /// any given region.
        /// </summary>
        public void JoinRegions()
        {
            if (cells.Count == 0)
            {
                Debug.LogWarning("You must first generate a level that contains at least one cell.");
                return;
            }

            if (regions.Count == 0)
            {
                Debug.LogWarning("You must first find regions.");
                return;
            }

            int max = 300;
            int _count = 0;
            while (regions[0].connectsTo.Count < regions.Count && _count++ < max)
            {

                for (int i = 0; i < regions.Count; i++)
                {
                    int other = 0;
                    Vector2Int a, b; a = b = Vector2Int.right * int.MaxValue;

                    float minD = float.MaxValue;
                    for (int k = 0; k < regions.Count; k++)
                    {
                        if (i == k || regions[i].connectsTo.Contains(k))
                            continue;

                        //check distance between cells in region i and region k
                        for (int j = 0; j < regions[i].cells.Count; j++)
                        {
                            for (int q = 0; q < regions[k].cells.Count; q++)
                            {
                                float d = Mathf.Abs(Vector2.SqrMagnitude(regions[i].cells[j] - regions[k].cells[q]));

                                if (d < minD)
                                {
                                    other = k;
                                    a = regions[i].cells[j];
                                    b = regions[k].cells[q];
                                    minD = d;
                                }
                            }
                        }
                    }

                    //check all cells in regions connected to region i too
                    for (int p = 0; p < regions[i].connectsTo.Count; p++)
                    {
                        int id = regions[i].connectsTo[p];
                        for (int k = 0; k < regions.Count; k++)
                        {
                            if (id == k || regions[id].connectsTo.Contains(k))
                                continue;

                            for (int j = 0; j < regions[id].cells.Count; j++)
                            {
                                for (int q = 0; q < regions[k].cells.Count; q++)
                                {
                                    float d = Mathf.Abs(Vector2.SqrMagnitude(regions[id].cells[j] - regions[k].cells[q]));

                                    if (d < minD)
                                    {
                                        other = k;
                                        a = regions[id].cells[j];
                                        b = regions[k].cells[q];
                                        minD = d;
                                    }
                                }
                            }
                        }
                    }

                    if (other == i || a.x == int.MaxValue)
                        continue;

                    for (int q = 0; q < regions[other].connectsTo.Count; q++)
                    {
                        if (regions[i].connectsTo.Contains(regions[other].connectsTo[q]))
                            continue;

                        regions[i].connectsTo.Add(regions[other].connectsTo[q]);
                        regions[regions[other].connectsTo[q]].connectsTo.Add(i);
                    }

                    for (int q = 0; q < regions[i].connectsTo.Count; q++)
                    {
                        if (regions[other].connectsTo.Contains(regions[i].connectsTo[q]))
                            continue;

                        regions[other].connectsTo.Add(regions[i].connectsTo[q]);
                        regions[regions[i].connectsTo[q]].connectsTo.Add(other);
                    }

                    regions[i].connectsTo.Add(other);
                    regions[other].connectsTo.Add(i);

                    //connect from a to b
                    Vector2 dir = b - a;
                    int steps = Mathf.Abs(dir.x) > Mathf.Abs(dir.y) ?
                                Mathf.Abs((int)dir.x) : Mathf.Abs((int)dir.y);
                    dir = dir.normalized;
                    float x = 0; float y = 0;

                    for (int q = 0; q <= steps; q++)
                    {
                        if(x + dir.x < dimensions.x)
                            x += dir.x;
    
                        if(y + dir.y < dimensions.y)
                            y += dir.y;

                        int _x = a.x + Mathf.RoundToInt(x);
                        int _y = a.y + Mathf.RoundToInt(y);

                        tex.SetPixel(_x, _y, Color.blue);
                        map[_x, _y] = 1;
                    }
                }
            }

            tex.Apply();
        }

        /// <summary>
        /// Creates the floor mesh using the tile rules defined in the cellular automaton.
        /// </summary>
        public void GenerateMesh()
        {
            if (map == null)
            {
                Debug.LogError("You must generate a map before you can generate a mesh.");
                return;
            }

            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            for (int y = 0; y < dimensions.y; y++)
            {
                for (int x = 0; x < dimensions.x; x++)
                {
                    //can speed this up a lot if we restrict the condition dimensions
                    Vector2Int size = cellularAutomaton.tileRules[0].condition.size;
                    Vector2Int edge = new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);
                    bool[] filter = new bool[size.x * size.y];

                    for (int yf = 0; yf < size.y; yf++)
                    {
                        for (int xf = 0; xf < size.x; xf++)
                        {
                            Vector2Int cur = new Vector2Int(x + xf - edge.x, y + yf - edge.y);

                            if (cur.x < 0 || cur.x >= dimensions.x || cur.y < 0 || cur.y >= dimensions.y)
                                filter[yf * size.x + xf] = false;
                            else
                                filter[yf * size.x + xf] = map[cur.x, cur.y] == 1;
                        }
                    }

                    bool satisfied = true;
                    for (int i = 0; i < cellularAutomaton.tileRules.Count; i++)
                    {
                        /* more modular, but waaay slower; for now, we won't support filters that aren't 3x3
                        
                        Vector2Int size = cellularAutomaton.tileRules[i].condition.size;
                        Vector2Int edge = new Vector2Int((size.x - 1) / 2, (size.y - 1) / 2);
                        bool[] filter = new bool[size.x * size.y];

                        for (int yf = 0; yf < size.y; yf++)
                        {
                            for (int xf = 0; xf < size.x; xf++)
                            {
                                Vector2Int cur = new Vector2Int(x + xf - edge.x, y + yf - edge.y);

                                if (cur.x < 0 || cur.x >= dimensions.x || cur.y < 0 || cur.y >= dimensions.y)
                                    filter[yf * size.x + xf] = false;
                                else
                                    filter[yf * size.x + xf] = map[cur.x, cur.y] == 1;

                                //testing
                                Debug.Log((yf * size.x + xf) + " : " + filter[yf * size.x + xf]);
                            }
                        }*/

                        //if we get a match, skip onto the next point after applying the rule output
                        for (int k = 0; k < filter.Length; k++)
                        {
                            if (filter[k] != cellularAutomaton.tileRules[i].condition.condition[k])
                            {
                                satisfied = false;
                                break;
                            }
                        }

                        if (satisfied)
                        {
                            //Debug.Log("x " + x + " y " + y + " | Satisfied by rule " + i);

                            //add verts / tris
                            Vector3 pos = new Vector3(x, 0, y);
                            for (int v = 0; v < cellularAutomaton.tileRules[i].verts.Length; v++)
                            {
                                verts.Add(cellularAutomaton.tileRules[i].verts[v] + pos);
                                uvs.Add(new Vector2(cellularAutomaton.tileRules[i].verts[v].x + pos.x,
                                                    cellularAutomaton.tileRules[i].verts[v].z + pos.z));
                            }

                            for (int t = 0; t < cellularAutomaton.tileRules[i].tris.Length; t++)
                                tris.Add(verts.Count + cellularAutomaton.tileRules[i].tris[t]);

                            break;
                        }
                        else if (i < cellularAutomaton.tileRules.Count - 1)
                            satisfied = true;
                    }
                    //default to a basic square if no rule satisfies and the tile is present.
                    if (!satisfied && map[x, y] == 1)
                    {
                        verts.Add(new Vector3(0.5f + x, 0, -0.5f + y));
                        verts.Add(new Vector3(-0.5f + x, 0, -0.5f + y));
                        verts.Add(new Vector3(-0.5f + x, 0, 0.5f + y));
                        verts.Add(new Vector3(0.5f + x, 0, 0.5f + y));

                        uvs.Add(new Vector2(0.5f + x, -0.5f + y));
                        uvs.Add(new Vector2(-0.5f + x, -0.5f + y));
                        uvs.Add(new Vector2(-0.5f + x, 0.5f + y));
                        uvs.Add(new Vector2(0.5f + x, 0.5f + y));

                        tris.Add(verts.Count - 4);
                        tris.Add(verts.Count - 3);
                        tris.Add(verts.Count - 2);

                        tris.Add(verts.Count - 4);
                        tris.Add(verts.Count - 2);
                        tris.Add(verts.Count - 1);
                    }
                }
            }

            Mesh m = new Mesh();

            m.Clear();
            m.SetVertices(verts);
            m.SetUVs(0, uvs);
            m.SetTriangles(tris, 0, true);

            Debug.Log("Mesh generated with " + verts.Count + " verts and " + tris.Count + " tris");

            meshFilter.mesh = m;
        }

        /// <summary>
        /// TODO: 
        /// Look at regions, if the cells.count is > some threshold, use a filter to spawn enemies,
        /// the filter will make sure that enemy spawns aren't too dense
        /// this could be adjusted for difficulty as the game progresses.
        /// 
        /// Spawn a boss mob in the largest region.
        /// </summary>
        public void GenerateMobs()
        {
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(LevelGenerator))]
    public class Editor_LevelGenerator : Editor
    {
        public LevelGenerator Target { get { return (LevelGenerator)target; } }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Generate Map"))
                Target.GenerateMap();

            if (GUILayout.Button("Find Regions"))
                Target.FindRegions();

            if (GUILayout.Button("Join Regions"))
                Target.JoinRegions();

            if (GUILayout.Button("Generate Mesh"))
                Target.GenerateMesh();

            Color c = GUI.backgroundColor;
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Generate All"))
            {
                Target.GenerateMap();
                Target.FindRegions();
                Target.JoinRegions();
                Target.GenerateMesh();
            }
            GUI.backgroundColor = c;

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
        public List<int> connectsTo = new List<int>();

        public List<Vector2Int> cells = new List<Vector2Int>();
        public Vector2Int midpoint;

        public Region(int id)
        {
            this.id = id;
            cells = new List<Vector2Int>();
            midpoint = Vector2Int.zero;
            connectsTo = new List<int>();
        }

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