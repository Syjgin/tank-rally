using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{

    [SerializeField] private GameObject _anchor;
    [SerializeField] private GameObject _terrainTile;

    private const float MaxDistance = 2;
    
    private Dictionary<Vector3, GameObject> _spawnedTiles;
    private Vector3 _currentTilePos;
    private const float TileBorderCoef = 10;

    private Vector2 _offset;
    private float _rotation;

    public delegate void TankPositionLoadingAction();

    public event TankPositionLoadingAction OnTankPositionLoaded;

    void Awake()
    {
        _spawnedTiles = new Dictionary<Vector3, GameObject>();
    }

	void Start ()
	{
        transform.position = new Vector3();
	    _offset = DataManager.GetInstance().GetTankOffset();
        _anchor.transform.position = new Vector3(_offset.x, 0, _offset.y);
	    _rotation = DataManager.GetInstance().GetTankRotation();
        _anchor.transform.Rotate(Vector3.up, _rotation);
        _currentTilePos = transform.position;
	    AddTile(_currentTilePos);
	    if (OnTankPositionLoaded != null)
	        OnTankPositionLoaded();
	}

    void OnDestroy()
    {
        DataManager.GetInstance().SaveTankInfo(_rotation, _offset);
    }

	void Update () 
    {
        Vector3 delta = _anchor.transform.position - _currentTilePos;
        _offset = new Vector2(delta.x, delta.z);
	    _rotation = _anchor.transform.rotation.eulerAngles.y;
	    if (delta.magnitude > MaxDistance)
	    {
            List<Vector3> newTileCoords = CalculateNewTilePosition(delta);
	        if (delta.magnitude > MaxDistance*TileBorderCoef)
	        {
	            _currentTilePos = newTileCoords[0];
                CalculateInvisibleTiles();
	        }
	        else
	        {
	            if (delta.magnitude > MaxDistance)
	            {
	                foreach (var newTileCoord in newTileCoords)
	                {
                        if (_spawnedTiles.ContainsKey(newTileCoord))
                            _spawnedTiles[newTileCoord].SetActive(true);
                        else
                        {
                            AddTile(newTileCoord);
                        }   
	                }
	            }
	        }
	        
	    }
    }

    public Vector3 GetCurrentTilePos()
    {
        return _currentTilePos;
    }

    private void CalculateInvisibleTiles()
    {
        foreach (var spawnedTile in _spawnedTiles)
        {
            Vector3 dist = spawnedTile.Key - _currentTilePos;
            if (dist.magnitude > MaxDistance*TileBorderCoef)
            {
                spawnedTile.Value.SetActive(false);
            }
        }
    }

    private void AddTile(Vector3 position)
    {
        GameObject spawnedTile = Instantiate(_terrainTile);
        spawnedTile.transform.parent = transform;
        spawnedTile.transform.localPosition = position;
        spawnedTile.transform.localRotation = Quaternion.identity;
        _spawnedTiles.Add(position, spawnedTile);
    }

    private List<Vector3> CalculateNewTilePosition(Vector3 delta)
    {
        Vector3 northTile = new Vector3(_currentTilePos.x, 0, _currentTilePos.z + TileBorderCoef * MaxDistance);
        Vector3 southTile = new Vector3(_currentTilePos.x, 0, _currentTilePos.z - TileBorderCoef * MaxDistance);
        Vector3 eastTile = new Vector3(_currentTilePos.x + TileBorderCoef * MaxDistance, 0, _currentTilePos.z);
        Vector3 westTile = new Vector3(_currentTilePos.x - TileBorderCoef * MaxDistance, 0, _currentTilePos.z);
        Vector3 northEastTile = new Vector3(_currentTilePos.x + TileBorderCoef * MaxDistance, 0, _currentTilePos.z + TileBorderCoef * MaxDistance);
        Vector3 northWestTile = new Vector3(_currentTilePos.x - TileBorderCoef * MaxDistance, 0, _currentTilePos.z + TileBorderCoef * MaxDistance);
        Vector3 southEastTile = new Vector3(_currentTilePos.x + TileBorderCoef * MaxDistance, 0, _currentTilePos.z - TileBorderCoef * MaxDistance);
        Vector3 southWestTile = new Vector3(_currentTilePos.x - TileBorderCoef * MaxDistance, 0, _currentTilePos.z - TileBorderCoef * MaxDistance);

        if (delta.z > MaxDistance && Mathf.Abs(delta.x) < MaxDistance)
        {
            return new List<Vector3>() { northTile };
        }
        if (delta.x > MaxDistance && Mathf.Abs(delta.z) < MaxDistance)
        {
            return new List<Vector3>() { eastTile };
        }
        if (delta.z < -MaxDistance && Mathf.Abs(delta.x) < MaxDistance)
        {
            return new List<Vector3>() { southTile };
        }
        if (delta.x < -MaxDistance && Mathf.Abs(delta.z) < MaxDistance)
        {
            return new List<Vector3>() { westTile };
        }

        if (delta.z > MaxDistance && delta.x > MaxDistance)
        {
            return new List<Vector3>() { northEastTile, northTile, eastTile };
        }
        if (delta.x > MaxDistance && delta.z < -MaxDistance)
        {
            return new List<Vector3>() { southEastTile, southTile, eastTile };
        }
        if (delta.z < -MaxDistance && delta.x < -MaxDistance)
        {
            return new List<Vector3>() { southWestTile, southTile, westTile };
        }
        if (delta.x < -MaxDistance && delta.z > MaxDistance)
        {
            return new List<Vector3>() { northWestTile, northTile, westTile };
        }

        return new List<Vector3>() { };
    }
}
