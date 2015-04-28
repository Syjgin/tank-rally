using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstaclesManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _bushPrefab;
    [SerializeField] private GameObject _stonePrefab;
    [SerializeField] private GameObject _puddlePrefab;
    [SerializeField] private GameObject _tank;
    [SerializeField] private TerrainManager _terrain;

    private const string ObjectCount = "ObjectCount";
    private const string ObjectItem = "ObjectItem";
    private const string ObjectType = "ObjectType";
    private const string ObjectRotation = "ObjectRotation";
    private const string ObjectCoordX = "ObjectCoordX";
    private const string ObjectCoordY = "ObjectCoordY";

    private Dictionary<MapObjectType, float> _possibilities; 

    private Vector3 _currentAxisPoint;
    private int _maxVisibleCount;
    private float _bushSpawnInterval;
    private float _minAllowedBushInterval;
    private const float VisibilityCoef = 1.5f;

    private List<GameObject> _spawnedPrefabs;
    private List<GameObject> _spawnedBushes; 

    private List<Vector3> _filledPrefabTiles; 

    private float _displaySizeX;
    private float _displaySizeY;

    private bool _isInitialized;

    private float _lastBushSpawnTime;

    private int _mapObjectCount;

    public enum MapObjectType
    {
        Tree,
        Bush,
        Puddle,
        Stone
    };

    private JSONObject _mapObjectData;

    void Awake()
    {
        _mapObjectData = new JSONObject(DataManager.GetInstance().LoadMapInfo());
        Ray startRayX = _mainCamera.ScreenPointToRay(new Vector3(0, 0));
        Vector3 startProjectionX = startRayX.GetPoint(1);
        Ray endRayX = _mainCamera.ScreenPointToRay(new Vector3(Screen.width, 0));
        Vector3 endProjectionX = endRayX.GetPoint(1);

        Ray startRayY = _mainCamera.ScreenPointToRay(new Vector3(0, 0));
        Vector3 startProjectionY = startRayY.GetPoint(1);
        Ray endRayY = _mainCamera.ScreenPointToRay(new Vector3(0, Screen.height));
        Vector3 endProjectionY = endRayY.GetPoint(1);

        _displaySizeX = Mathf.Abs(startProjectionX.x - endProjectionX.x);
        _displaySizeY = Mathf.Abs(startProjectionY.z - endProjectionY.z);

        _terrain.OnTankPositionLoaded += Init;
        _filledPrefabTiles = new List<Vector3>();
        _possibilities = new Dictionary<MapObjectType, float>();
        Random.seed = Convert.ToInt32(DateTime.UtcNow.Ticks % 100);
        _spawnedPrefabs = new List<GameObject>();
        _spawnedBushes = new List<GameObject>();

        _minAllowedBushInterval = DataManager.GetInstance().GetBushMinimalDistance();
        _bushSpawnInterval = DataManager.GetInstance().GetBushSpawnPeriod();
        _maxVisibleCount = DataManager.GetInstance().GetMaxVisibleObjects();
        _possibilities = DataManager.GetInstance().GetPossibilities();
    }

    void OnDestroy()
    {
        _terrain.OnTankPositionLoaded -= Init;
        _mapObjectData.Bake();
        DataManager.GetInstance().SaveMapInfo(_mapObjectData.str);
    }

    private void Init()
    {
        _currentAxisPoint = _tank.transform.position;
        transform.position = _tank.transform.position;
        transform.rotation = _tank.transform.rotation;

        _mapObjectData.GetField(ref _mapObjectCount, ObjectCount, 0);
        Vector3[] bounds = GenerateVisibilityBounds();
        for (int i = 0; i < _mapObjectCount; i++)
        {
            JSONObject mapObject = _mapObjectData.GetField(ObjectItem + i);
            if (mapObject != null)
            {
                string objType = default(string);
                if(!mapObject.GetField(ref objType, ObjectType))
                    Debug.LogError("object type parse failed");
                MapObjectType mapObjectType = (MapObjectType) Enum.Parse(typeof (MapObjectType), objType);
                float rotation = 0;
                if (!mapObject.GetField(ref rotation, ObjectRotation))
                    Debug.LogError("object rotation parse failed");
                float coordX = 0;
                if (!mapObject.GetField(ref coordX, ObjectCoordX))
                    Debug.LogError("object x coordinate parse failed");
                float coordY = 0;
                if (!mapObject.GetField(ref coordY, ObjectCoordY))
                    Debug.LogError("object x coordinate parse failed");
                GameObject instantiated = GetMapObjectByEnum(mapObjectType);
                instantiated.transform.parent = transform;
                instantiated.transform.position = new Vector3(coordX, 0, coordY);
                instantiated.transform.Rotate(Vector3.up, rotation);
                _spawnedPrefabs.Add(instantiated);
                if (mapObjectType == MapObjectType.Bush)
                    _spawnedBushes.Add(instantiated);
                CheckVisibility(instantiated, bounds);
            }
        }
        
        PlaceObstacles(_currentAxisPoint);
        _lastBushSpawnTime = Time.time;
        _isInitialized = true;
    }

	void Update ()
	{
        if(!_isInitialized)
            return;
        Vector3 newAxisPoint = _tank.transform.position;
        SwapObjectsByDistance();
        PlaceObstacles(_currentAxisPoint);

	    if (newAxisPoint.z - _currentAxisPoint.z >= _displaySizeY / 2 && Mathf.Abs(newAxisPoint.x - _currentAxisPoint.x) <= _displaySizeX / 2)
	    {
	        _currentAxisPoint.z += _displaySizeY;
	    }
        if (_currentAxisPoint.z - newAxisPoint.z >= _displaySizeY / 2 && Mathf.Abs(newAxisPoint.x - _currentAxisPoint.x) <= _displaySizeX / 2)
        {
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x >= _displaySizeX / 2 && Mathf.Abs(newAxisPoint.z - _currentAxisPoint.z) <= _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
        }
        if (_currentAxisPoint.x - newAxisPoint.x >= _displaySizeX / 2 && Mathf.Abs(newAxisPoint.z - _currentAxisPoint.z) <= _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
        }
        if (_currentAxisPoint.x - newAxisPoint.x >= _displaySizeX / 2 && _currentAxisPoint.z - newAxisPoint.z >= _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x >= _displaySizeX / 2 && _currentAxisPoint.z - newAxisPoint.z >= _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x >= _displaySizeX / 2 && newAxisPoint.z - _currentAxisPoint.z >= _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
            _currentAxisPoint.z += _displaySizeY;
        }
        if (_currentAxisPoint.x - newAxisPoint.x >= _displaySizeX / 2 && newAxisPoint.z - _currentAxisPoint.z >= _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
            _currentAxisPoint.z += _displaySizeY;
        }

	    if (Time.time - _lastBushSpawnTime > _bushSpawnInterval)
	    {
	        _lastBushSpawnTime = Time.time;
            SpawnAdditionalBush(newAxisPoint);
	    }
	}

    private void PlaceObstacles(Vector3 axis)
    {
        List<Vector3> tilesToFill = new List<Vector3>();
        tilesToFill.Add(axis);
        tilesToFill.Add(new Vector3(axis.x + _displaySizeX, 0, axis.z));
        tilesToFill.Add(new Vector3(axis.x - _displaySizeX, 0, axis.z));
        tilesToFill.Add(new Vector3(axis.x, 0, axis.z + _displaySizeY));
        tilesToFill.Add(new Vector3(axis.x, 0, axis.z - _displaySizeY));
        tilesToFill.Add(new Vector3(axis.x + _displaySizeX, 0, axis.z + _displaySizeY));
        tilesToFill.Add(new Vector3(axis.x + _displaySizeX, 0, axis.z - _displaySizeY));
        tilesToFill.Add(new Vector3(axis.x - _displaySizeX, 0, axis.z + _displaySizeY));
        tilesToFill.Add(new Vector3(axis.x - _displaySizeX, 0, axis.z - _displaySizeY));
        foreach (var tile in tilesToFill)
        {
            if (!_filledPrefabTiles.Contains(tile))
            {
                int currentVisibleCount = GetNearestMapObjects(tile);
                while (currentVisibleCount < _maxVisibleCount)
                {
                    int attempt = Random.Range(0, 100);
                    float targetPossibility = 0;
                    foreach (var possibility in _possibilities)
                    {
                        targetPossibility += possibility.Value;
                        if (attempt < targetPossibility)
                        {
                            SpawnNewMapObject(tile, possibility.Key);
                            currentVisibleCount++;
                        }
                    }
                }
                _filledPrefabTiles.Add(tile);
            }   
        }
    }

    private int GetNearestMapObjects(Vector3 axis)
    {
        int result = 0;
        Vector3 min = new Vector3(axis.x - _displaySizeX / 2, 0, axis.z - _displaySizeY / 2);
        Vector3 max = new Vector3(axis.x + _displaySizeX / 2, 0, axis.z + _displaySizeY / 2);
        foreach (var spawnedPrefab in _spawnedPrefabs)
        {
            if (spawnedPrefab.transform.position.x > min.x && 
                spawnedPrefab.transform.position.x < max.x && 
                spawnedPrefab.transform.position.z > min.z &&
                spawnedPrefab.transform.position.z < max.z)
            {
                spawnedPrefab.gameObject.SetActive(true);
                result++;
            }
        }
        return result;
    }

    private void SpawnNewMapObject(Vector3 axis, MapObjectType objectType)
    {
        Vector3 position = GenerateRandomPosition(axis);
        GameObject instantiated = GetMapObjectByEnum(objectType);
        instantiated.transform.parent = transform;
        instantiated.transform.position = position;
        instantiated.transform.Rotate(Vector3.up, Random.Range(0, 360));
        _spawnedPrefabs.Add(instantiated);
        if(objectType == MapObjectType.Bush)
            _spawnedBushes.Add(instantiated);
        SaveMapObject(objectType, instantiated);
    }

    private void SaveMapObject(MapObjectType objectType, GameObject instantiated)
    {
        _mapObjectCount++;
        _mapObjectData.SetField(ObjectCount, _mapObjectCount);
        JSONObject objToAdd = new JSONObject();
        objToAdd.AddField(ObjectType, objectType.ToString());
        objToAdd.AddField(ObjectRotation, instantiated.transform.rotation.eulerAngles.y);
        objToAdd.AddField(ObjectCoordX, instantiated.transform.position.x);
        objToAdd.AddField(ObjectCoordY, instantiated.transform.position.z);
        _mapObjectData.AddField(ObjectItem + _mapObjectCount, objToAdd);
    }

    private void SpawnAdditionalBush(Vector3 axis)
    {
        List<Vector3> bushCoordinatesInNeighborhood = new List<Vector3>();
        foreach (var spawnedBush in _spawnedBushes)
        {
            if (spawnedBush.transform.position.x < axis.x + _displaySizeX / 2 &&
                spawnedBush.transform.position.x > axis.x - _displaySizeX / 2 &&
                spawnedBush.transform.position.z < axis.z + _displaySizeX / 2 &&
                spawnedBush.transform.position.z < axis.z + _displaySizeX / 2)
            {
                bushCoordinatesInNeighborhood.Add(spawnedBush.transform.position);
            }
        }
        List<Vector3> candidateCoordinates = new List<Vector3>();
        const int candidateCount = 10;
        for (int i = 0; i < candidateCount; i++)
        {
            candidateCoordinates.Add(GenerateRandomPosition(axis));
        }
        float maxDist = float.MinValue;
        int maxDistIndex = 0;
        int currentIndex = 0;
        foreach (var candidateCoordinate in candidateCoordinates)
        {
            float dist = float.MaxValue;
            foreach (var bushCoord in bushCoordinatesInNeighborhood)
            {
                float currentDist = (bushCoord - candidateCoordinate).magnitude;
                if (currentDist < dist)
                    dist = currentDist;
            }
            if (dist > maxDist)
            {
                maxDist = dist;
                maxDistIndex = currentIndex;
            }
            currentIndex++;
        }
        if (maxDist > _minAllowedBushInterval)
        {
            GameObject instantiated = GetMapObjectByEnum(MapObjectType.Bush);
            instantiated.transform.parent = transform;
            instantiated.transform.position = candidateCoordinates[maxDistIndex];
            _spawnedBushes.Add(instantiated);
            _spawnedPrefabs.Add(instantiated);  
            SaveMapObject(MapObjectType.Bush, instantiated);
        }
    }

    private GameObject GetMapObjectByEnum(MapObjectType targetType)
    {
        switch (targetType)
        {
            case MapObjectType.Tree:
                return Instantiate(_treePrefab);
            case MapObjectType.Bush:
                return Instantiate(_bushPrefab);
            case MapObjectType.Puddle:
                return Instantiate(_puddlePrefab);
            case MapObjectType.Stone:
                return Instantiate(_stonePrefab);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SwapObjectsByDistance()
    {
        Vector3[] bounds = GenerateVisibilityBounds();
        foreach (var spawnedPrefab in _spawnedPrefabs)
        {
            CheckVisibility(spawnedPrefab, bounds);
        }
    }

    private Vector3[] GenerateVisibilityBounds()
    {
        Vector3 min = new Vector3(_currentAxisPoint.x - _displaySizeX * VisibilityCoef, 0, _currentAxisPoint.z - _displaySizeY * VisibilityCoef);
        Vector3 max = new Vector3(_currentAxisPoint.x + _displaySizeX * VisibilityCoef, 0, _currentAxisPoint.z + _displaySizeY * VisibilityCoef);
        return new[] {min, max};
    }

    private static void CheckVisibility(GameObject spawnedPrefab, Vector3[] bounds)
    {
        if (spawnedPrefab.transform.position.x > bounds[0].x &&
            spawnedPrefab.transform.position.x < bounds[1].x &&
            spawnedPrefab.transform.position.z > bounds[0].z &&
            spawnedPrefab.transform.position.z < bounds[1].z)
        {
            spawnedPrefab.gameObject.SetActive(true);
        }
        else
        {
            spawnedPrefab.gameObject.SetActive(false);
        }
    }

    private Vector3 GenerateRandomPosition(Vector3 initialPosition)
    {
        float xPosition = Random.Range(initialPosition.x - _displaySizeX / 2, initialPosition.x + _displaySizeX / 2);
        float yPosition = Random.Range(initialPosition.z - _displaySizeY / 2, initialPosition.z + _displaySizeY / 2);
        return new Vector3(xPosition, 0, yPosition);
    }
}
