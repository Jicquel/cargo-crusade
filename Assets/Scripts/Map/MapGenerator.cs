using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;


public class MapGenerator : MonoBehaviour
{
    public GameObject[] prefabs;
    bool _validPrefabs;
    int _spriteSize;
        
    [SerializeField]
    int _chunkSize = 10;

    int _maxViewDst;

    int _chunksVisibleInViewDst;

    NoiseGenerator _noiseGen;

    Dictionary<Vector2, MapChunk> _mapChunkDictionary = new Dictionary<Vector2, MapChunk>();

    List<MapChunk> _mapChunksVisibleLastUpdate = new List<MapChunk>();

    public Transform viewerTransform;

    private Vector2 _lastUpdateposition;

    void Start()
    {
        _validPrefabs = CheckPrefabs();
        if (!_validPrefabs)
        {
            Debug.LogError("Invalid prefabs. Aborting Map generator start.");
            return;
        }
        float screenHeight = Camera.main.orthographicSize * 2.0f;
        float screenWidth = screenHeight * Camera.main.aspect;

        _maxViewDst = (int) Mathf.Max(screenHeight, screenWidth);

        _spriteSize = (int)prefabs[0].GetComponent<SpriteRenderer>().size.x;
        _noiseGen = new NoiseGenerator(numberOfIterations: 10000);
        _chunkSize = Mathf.RoundToInt(_chunkSize / _spriteSize) * _spriteSize;

        _chunksVisibleInViewDst = Mathf.RoundToInt(_maxViewDst / _chunkSize);
        _lastUpdateposition = viewerTransform.position;
        InitChunks();
    }

    // Update is called once per frame
    void Update()
    {
    }

    // Ensure that at least one prefab exists, and
    // that all prefabs sprites have the same size.z
    private bool CheckPrefabs()
    {
        Vector2 sprSize;
        SpriteRenderer sprRend;

        if(prefabs.Length == 0)
        {
            Debug.LogError("Map generator must contain at least one prefab.");
            return false;
        }

        sprRend = prefabs[0].GetComponent<SpriteRenderer>();
        if(sprRend == null)
        {
            Debug.LogError("Prefab " + prefabs[0] + " does not have a sprite renderer.");
            return false;
        }
        else
        {
            sprSize = sprRend.size;
        }

        for(int i = 1; i < prefabs.Length; i++)
        {
            sprRend = prefabs[i].GetComponent<SpriteRenderer>();
            if (sprRend == null)
            {
                Debug.LogError("Prefab " + prefabs[i] + " does not have a sprite renderer");
                return false;
            }
            else
            {
                if(sprRend.size != sprSize)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void InitChunks()
    {
        int currentChunkCoordX = Mathf.RoundToInt(viewerTransform.position.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerTransform.position.y / _chunkSize);

        for (int yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                MapChunk mapChunk = new MapChunk(viewedChunkCoord, _chunkSize, _spriteSize, prefabs, _noiseGen);
                mapChunk.SetVisible(true);
                _mapChunkDictionary.Add(viewedChunkCoord, mapChunk);
                _mapChunksVisibleLastUpdate.Add(_mapChunkDictionary[viewedChunkCoord]);
            }
        }
    }
    public void UpdateVisibleChunks()
    {
        if(Vector2.Distance(_lastUpdateposition, viewerTransform.position) <= _chunkSize/2)
        {
            return;
        }
        _lastUpdateposition = viewerTransform.position;

        for (int i = 0; i < _mapChunksVisibleLastUpdate.Count; i++)
        {
            _mapChunksVisibleLastUpdate[i].SetVisible(false);
        }
        _mapChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerTransform.position.x / _chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerTransform.position.y / _chunkSize);

        for (int yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_mapChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    _mapChunkDictionary[viewedChunkCoord].UpdateMapChunk(viewerTransform.position, _maxViewDst);
                    if (_mapChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        _mapChunksVisibleLastUpdate.Add(_mapChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                   _mapChunkDictionary.Add(viewedChunkCoord, new MapChunk(viewedChunkCoord, _chunkSize, _spriteSize, prefabs, _noiseGen));
                }
            }
        }
    }

    public class MapChunk
    {
        private Vector2 position;
        private Bounds bounds;
        private List<GameObject> gameObjects = new List<GameObject>();
        private GameObject parent;

        // Pre-condition: gObjRange objects have all the same size. 
        public MapChunk(Vector2 coord, int size, int objSize, GameObject[] gObjRange, NoiseGenerator noiseGen)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            int objNumberInOneLine = size / objSize;
            float[,] mapNoise = noiseGen.GenerateNoiseMap(objNumberInOneLine, objNumberInOneLine, coord.x * size, coord.y * size);

            parent = new GameObject("Chunk("+coord.x+","+coord.y+")");
            parent.transform.position = position;

            for (int x = 0; x < objNumberInOneLine; x++)
            {
                for (int y = 0; y < objNumberInOneLine; y++)
                {
                    GameObject gObj = gObjRange[(int)(mapNoise[x, y] * (gObjRange.Length - 1))];
                    float objPositionX = x * objSize;
                    float objPositionY = y * objSize;

                    GameObject instance = Instantiate(gObj, Vector3.zero, Quaternion.identity, parent.transform);
                    instance.transform.localPosition = new Vector3(objPositionX, objPositionY, 0);
                    if (instance != null)
                    {
                        gameObjects.Add(instance);
                    }
                }
            }

            SetVisible(false);
        }

        public void UpdateMapChunk(Vector2 viewerPosition, float maxViewDist)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            parent.SetActive(visible);
        }

        public bool IsVisible()
        {
           return parent.activeSelf;
        }
    }
}


