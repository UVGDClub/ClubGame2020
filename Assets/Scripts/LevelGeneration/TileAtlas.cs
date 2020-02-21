using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LevelGeneration
{
    /// <summary>
    /// Given a tile category, the atlas maps a cell condition to its UV coordinates in the map.
    /// 
    /// TODO: Use scriptable object to define UV mapping of each tile
    ///         uv mappings can be reused between tile rules
    ///         they can also be defined in like manner to the verts of triangles in the tile rules.
    ///         use enums to define in a human readable way what piece the uv maps to
    ///         i.e. ###  a cell in the 3x3 edged tile group, or  ^  a piece of the diamond-shaped tile group
    ///              ###                                         < >
    ///              ###                                          v
    /// </summary>
    [CreateAssetMenu(fileName = "new TileAtlas", menuName = "Level Generation/Tile Atlas")]
    public class TileAtlas : ScriptableObject
    {
        public Material previewMaterial;
        public Material overlayMaterial;
        public Texture2D atlas;
        //public CellularAutomaton tileRules;
        //public TileSet_Dictionary tileSets = new TileSet_Dictionary();
        public List<TileSet> tileSets = new List<TileSet>();
        public float previewScale = 5;
        public Vector2Int gridSize = new Vector2Int(8,8);
        public Color gridColour = new Color(1,1,1,1f);
        public Color selectionHighlight = new Color(1, 0, 1, 1f);
        public ScaleMode previewScaleMode = ScaleMode.StretchToFill;

        [HideInInspector] public TileCategory category;
        [HideInInspector] public TileShape shape;
        [HideInInspector] public int tileVariant = 0;
        [HideInInspector] public int conditionIndex;
        [HideInInspector] public CellCondition condition;
        [HideInInspector] public int uvID = 0;
    }

    /// <summary>
    /// Given a cell condition, the tileset maps a tile category to the bucket of shapes within.
    /// </summary>
    [System.Serializable]
    public class TileSet
    {
        //public UV_Dictionary uvMap = new UV_Dictionary();
        public List<TileBucket> tileShapeBuckets = new List<TileBucket>();
    }

    [System.Serializable]
    public class TileBucket
    {
        public List<TileUVs> variantUVs = new List<TileUVs>();
    }

    [System.Serializable]
    public class TileUVs
    {
        public List<Vector2> uvMap = new List<Vector2>();
    }

    [System.Serializable]
    public class UV_Dictionary : Dictionary<int, List<Vector2>> { }

    [System.Serializable]
    public class TileSet_Dictionary : Dictionary<TileCategory, List<TileSet>> { }

    [System.Serializable]
    public enum TileCategory
    {
        floor,
        wall,
        LENGTH
    }

    /// <summary>
    /// Tile shapes are defined by their corresponding 3x3 Cell Condition filter
    /// </summary>
    [System.Serializable]
    public enum TileShape
    {
        topLeftCorner,
        topEdge,
        topRightCorner,
        leftEdge,
        centre,
        rightEdge,
        bottomLeftCorner,
        bottomEdge,
        bottomRightCorner,
        topQuarter,
        bottomQuarter,
        leftQuarter,
        rightQuarter,
        topPeninsula,
        bottomPeninsula,
        leftPeninsula,
        rightPeninsula,
        horizontalEdges,
        verticalEdges,
        leftDiagEdges,
        rightDiagEdges,
        LENGTH
    }

    [CustomEditor(typeof(TileAtlas))]
    public class Editor_TileAtlas : Editor
    {
        Texture2D overlayTex;
        Vector2Int curPoint = Vector2Int.zero;
        TileAtlas Target { get { return (TileAtlas)target; } }

        const float delay = 0.1f;
        float lastKeyInput;

        bool showVerts;

        private void OnEnable()
        {
            lastKeyInput = Time.time;
            Target.atlas.wrapMode = TextureWrapMode.Clamp;
            Target.atlas.filterMode = FilterMode.Point;

            if (overlayTex == null)
            {
                overlayTex = new Texture2D(Target.atlas.width, Target.atlas.height);
                overlayTex.wrapMode = TextureWrapMode.Clamp;
                overlayTex.filterMode = FilterMode.Point;

                Color[] overlayC = new Color[Target.atlas.width * Target.atlas.height];

                for (int x = 0; x < Target.atlas.width; x++)
                {
                    for (int y = 0; y < Target.atlas.height; y++)
                    {
                        if (x % Target.gridSize.x == 0 || y % Target.gridSize.y == 0)
                            overlayC[y * Target.atlas.width + x] = Target.gridColour;
                        else
                            overlayC[y * Target.atlas.width + x] = Color.clear;
                    }
                }

                overlayTex.SetPixels(overlayC);
                overlayTex.Apply();
                Target.overlayMaterial.SetTexture("_MainTex", overlayTex);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Target.atlas || !Target.overlayMaterial || !overlayTex)
                return;

            EditorUtility.SetDirty(target);

            Target.category = (TileCategory)EditorGUILayout.EnumPopup(Target.category);

            if((int)Target.category >= Target.tileSets.Count)
            {
                for (int i = Target.tileSets.Count; i < (int)TileCategory.LENGTH; i++)
                    Target.tileSets.Add(new TileSet());
            }

            List<TileBucket> tileBucket = Target.tileSets[(int)Target.category].tileShapeBuckets;

            Target.shape = (TileShape)EditorGUILayout.EnumPopup(Target.shape);

            if((int)Target.shape >= tileBucket.Count)
            {
                for (int i = tileBucket.Count; i < (int)TileShape.LENGTH; i++)
                    tileBucket.Add(new TileBucket());
            }

            TileBucket bucket = tileBucket[(int)Target.shape];

            if (GUILayout.Button("Add variant"))
                bucket.variantUVs.Add(new TileUVs());

            Target.tileVariant = EditorGUILayout.IntSlider("Tile Variant", Target.tileVariant, 0, bucket.variantUVs.Count - 1);

            if (Target.tileVariant >= bucket.variantUVs.Count)
                return;

            List<Vector2> uvs = bucket.variantUVs[Target.tileVariant].uvMap;

            if(GUILayout.Button("Add uv"))
            {
                uvs.Add(new Vector2());
                Target.uvID = uvs.Count - 1;
            }

            if (GUILayout.Button("Insert uv") 
                || (delay < Time.time - lastKeyInput 
                    && Event.current.type == EventType.KeyUp && Event.current.type != EventType.KeyDown
                    && Event.current.keyCode == KeyCode.Plus))
            {
                lastKeyInput = Time.time;

                uvs.Add(new Vector2());

                for(int i = Target.uvID; i < uvs.Count - 1; i++)
                {
                    uvs[i + 1] = uvs[i]; 
                }

                uvs[Target.uvID] = Vector2.zero;
            }

            if (uvs.Count == 0)
                return;

            if(Event.current.control)
            {
                if (delay < Time.time - lastKeyInput && Event.current.type == EventType.KeyUp && Event.current.type != EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.RightArrow)
                    {
                        Target.uvID = (Target.uvID + 1) % uvs.Count;
                        lastKeyInput = Time.time;
                    }
                    else if (Event.current.keyCode == KeyCode.LeftArrow)
                    {
                        Target.uvID--;
                        if (Target.uvID < 0)
                            Target.uvID = uvs.Count - 1;

                        lastKeyInput = Time.time;
                    }
                }
            }

            EditorGUILayout.Space();
            GUI.color = Color.red;
            if (GUILayout.Button("Remove uv"))
            {
                uvs.RemoveAt(Target.uvID);

                if (uvs.Count == 0)
                    return;
            }
            GUI.color = Color.white;
            EditorGUILayout.Space();

            Target.uvID = EditorGUILayout.IntSlider("UV Index", Target.uvID, 0, uvs.Count - 1);
            uvs[Target.uvID] = EditorGUILayout.Vector2Field("UV Coord", uvs[Target.uvID]);

            if (curPoint.x % Target.gridSize.x == 0 || curPoint.y % Target.gridSize.y == 0)
                overlayTex.SetPixel(curPoint.x, curPoint.y, Target.gridColour);
            else
                overlayTex.SetPixel(curPoint.x, curPoint.y, Color.clear);

            curPoint = new Vector2Int(Mathf.RoundToInt(uvs[Target.uvID].x * Target.atlas.width),
                                  Mathf.RoundToInt(uvs[Target.uvID].y * Target.atlas.height));

            overlayTex.SetPixel(curPoint.x, curPoint.y, Target.selectionHighlight);
            overlayTex.Apply();

            Rect pRect = GUILayoutUtility.GetRect(Target.previewScale * Target.atlas.width,
                                                  Target.previewScale * Target.atlas.height);
                                          
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (pRect.Contains(Event.current.mousePosition))
                {
                    if (curPoint.x % Target.gridSize.x == 0 || curPoint.y % Target.gridSize.y == 0)
                        overlayTex.SetPixel(curPoint.x, curPoint.y, Target.gridColour);
                    else
                        overlayTex.SetPixel(curPoint.x, curPoint.y, Color.clear);

                    float x = (Event.current.mousePosition.x - pRect.xMin) / (pRect.xMax - pRect.xMin);
                    float y = 1 - (Event.current.mousePosition.y - pRect.yMin) / (pRect.yMax - pRect.yMin);

                    curPoint = new Vector2Int(Mathf.RoundToInt(x * Target.atlas.width),
                                              Mathf.RoundToInt(y * Target.atlas.height));

                    uvs[Target.uvID] = new Vector2((float)curPoint.x / Target.atlas.width, 
                                                   (float)curPoint.y / Target.atlas.height);

                    overlayTex.SetPixel(curPoint.x, curPoint.y, Target.selectionHighlight);
                    overlayTex.Apply();
                }
            }

            EditorGUI.DrawPreviewTexture(pRect, Target.atlas, Target.previewMaterial, Target.previewScaleMode, 0, -1);

            Rect oRect = new Rect(pRect.x, pRect.y, pRect.width, pRect.height);
            EditorGUI.DrawPreviewTexture(oRect, overlayTex, Target.overlayMaterial, Target.previewScaleMode);
        }
    }

}