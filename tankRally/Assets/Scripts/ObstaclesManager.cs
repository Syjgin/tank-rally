using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class ObstaclesManager : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _treePrefab;
    [SerializeField] private GameObject _bushPrefab;
    [SerializeField] private GameObject _stonePrefab;
    [SerializeField] private GameObject _puddlePrefab;
    [SerializeField] private GameObject _tank;
    [SerializeField] private TerrainManager _terrainLoader;

    private Dictionary<MapObjectType, int> _possibilities; 

    private Vector3 _currentAxisPoint;
    private const int MaxVisibleCount = 50;

    private List<GameObject> _spawnedPrefabs;

    private List<Vector3> _filledPrefabTiles; 

    private float _displaySizeX;
    private float _displaySizeY;

    private bool _isInitialized;

    public enum MapObjectType
    {
        Tree,
        Bush,
        Puddle,
        Stone
    };

    void Awake()
    {
        _terrainLoader.OnTankPositionLoaded += Init;
        _filledPrefabTiles = new List<Vector3>();
        _possibilities = new Dictionary<MapObjectType, int>();
        Random.seed = Convert.ToInt32(DateTime.UtcNow.Ticks % 100);
        _spawnedPrefabs = new List<GameObject>();
    }

    void OnDestroy()
    {
        _terrainLoader.OnTankPositionLoaded -= Init;
    }

    private void Init()
    {
        _currentAxisPoint = _tank.transform.position;
        transform.position = _tank.transform.position;
        transform.rotation = _tank.transform.rotation;
        _possibilities.Add(MapObjectType.Tree, 10);
        _possibilities.Add(MapObjectType.Bush, 30);
        _possibilities.Add(MapObjectType.Puddle, 10);
        _possibilities.Add(MapObjectType.Stone, 50);

        Vector3 start = _mainCamera.ScreenToWorldPoint(new Vector3(0, 0));
        Vector3 end = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        _displaySizeX = Mathf.Abs(start.x - end.x);
        _displaySizeY = Mathf.Abs(start.z - end.z);
        PlaceObstacles(_currentAxisPoint);
        _isInitialized = true;
    }

	void Update ()
	{
        if(!_isInitialized)
            return;
        Vector3 newAxisPoint = _tank.transform.position;
        SwapObjectsByDistance();
        PlaceObstacles(_currentAxisPoint);

	    if (newAxisPoint.z - _currentAxisPoint.z > _displaySizeY/2 && Mathf.Abs(newAxisPoint.x - _currentAxisPoint.x) < _displaySizeX/2)
	    {
	        _currentAxisPoint.z += _displaySizeY;
	    }
        if (_currentAxisPoint.z - newAxisPoint.z > _displaySizeY / 2 && Mathf.Abs(newAxisPoint.x - _currentAxisPoint.x) < _displaySizeX / 2)
        {
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x > _displaySizeX / 2 && Mathf.Abs(newAxisPoint.z - _currentAxisPoint.z) < _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
        }
        if (_currentAxisPoint.x - newAxisPoint.x > _displaySizeX / 2 && Mathf.Abs(newAxisPoint.z - _currentAxisPoint.z) < _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
        }
        if (_currentAxisPoint.x - newAxisPoint.x > _displaySizeX / 2 && _currentAxisPoint.z - newAxisPoint.z > _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x > _displaySizeX / 2 && _currentAxisPoint.z - newAxisPoint.z > _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
            _currentAxisPoint.z -= _displaySizeY;
        }
        if (newAxisPoint.x - _currentAxisPoint.x > _displaySizeX / 2 && newAxisPoint.z - _currentAxisPoint.z > _displaySizeY / 2)
        {
            _currentAxisPoint.x += _displaySizeX;
            _currentAxisPoint.z += _displaySizeY;
        }
        if (_currentAxisPoint.x - newAxisPoint.x > _displaySizeX / 2 && newAxisPoint.z - _currentAxisPoint.z > _displaySizeY / 2)
        {
            _currentAxisPoint.x -= _displaySizeX;
            _currentAxisPoint.z += _displaySizeY;
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
                while (currentVisibleCount < MaxVisibleCount)
                {
                    int attempt = Random.Range(0, 100);
                    int targetPossibility = 0;
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
        float xPosition = Random.Range(axis.x - _displaySizeX/2, axis.x + _displaySizeX/2);
        float yPosition = Random.Range(axis.z - _displaySizeY/2, axis.z + _displaySizeY/2);
        Vector3 position = new Vector3(xPosition, 0, yPosition);
        GameObject instantiated = GetMapObjectByEnum(objectType);
        instantiated.transform.parent = transform;
        instantiated.transform.position = position;
        instantiated.transform.Rotate(Vector3.up, Random.Range(0, 360));
        _spawnedPrefabs.Add(instantiated);
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
        const float displayCoef = 1.5f;
        Vector3 min = new Vector3(_currentAxisPoint.x - _displaySizeX*displayCoef, 0, _currentAxisPoint.z - _displaySizeY*displayCoef);
        Vector3 max = new Vector3(_currentAxisPoint.x + _displaySizeX*displayCoef, 0, _currentAxisPoint.z + _displaySizeY*displayCoef);
        foreach (var spawnedPrefab in _spawnedPrefabs)
        {
            if (spawnedPrefab.transform.position.x > min.x &&
                spawnedPrefab.transform.position.x < max.x &&
                spawnedPrefab.transform.position.z > min.z &&
                spawnedPrefab.transform.position.z < max.z)
            {
                spawnedPrefab.gameObject.SetActive(true);
            }
            else
            {
                spawnedPrefab.gameObject.SetActive(false);
            }
        }
    }
}
