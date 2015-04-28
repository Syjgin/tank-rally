using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour
{

    [SerializeField] private GameObject _anchor;
    [SerializeField] private GameObject _terrainTile;

    private const float MaxDistance = 20;
    
    private Dictionary<Vector3, GameObject> _spawnedTiles;
    private Vector3 _currentTilePos;

    private Vector2 _offset;
    private float _rotation;
    private Vector2 _tankPosition;
    private Vector3 _tankOffset;

    public delegate void TankPositionLoadingAction();

    public event TankPositionLoadingAction OnTankPositionLoaded;

    void Awake()
    {
        _spawnedTiles = new Dictionary<Vector3, GameObject>();
    }

	void Start ()
	{
	    _tankPosition = DataManager.GetInstance().GetTankPosition();
        _anchor.transform.position = new Vector3(_tankPosition.x, 0, _tankPosition.y);
	    _offset = DataManager.GetInstance().GetTerrainOffset();
        transform.position = new Vector3(_tankPosition.x - _offset.x, 0, _tankPosition.y - _offset.y);
	    _tankOffset = transform.position;
	    _rotation = DataManager.GetInstance().GetTankRotation();
        _anchor.transform.Rotate(Vector3.up, _rotation);
        _currentTilePos = new Vector3();
        Vector3[] newTileCoords = CalculateNeighbors();
        AddTile(new Vector3());
        foreach (var newTileCoord in newTileCoords)
        {
            if(!_spawnedTiles.ContainsKey(newTileCoord))
                AddTile(newTileCoord);
        }
	    if (OnTankPositionLoaded != null)
	        OnTankPositionLoaded();
	}

    void OnDestroy()
    {
        DataManager.GetInstance().SaveTerrainOffset(_rotation, _offset);
        DataManager.GetInstance().SaveTankRotation(_rotation);
        DataManager.GetInstance().SaveTankPosition(_tankPosition);
    }

	void Update () 
    {
        Vector3 delta = (_anchor.transform.position - _tankOffset) - _currentTilePos;
        _tankPosition = new Vector2(_anchor.transform.position.x, _anchor.transform.position.z);
        _offset = new Vector2(delta.x, delta.z);
	    _rotation = _anchor.transform.rotation.eulerAngles.y;
        if ((Mathf.Abs(delta.x) > MaxDistance / 2) || (Mathf.Abs(delta.z) > MaxDistance / 2))
        {
            _currentTilePos = CalcultateNextAxisPoint(delta);
            List<Vector3> newTileCoords = new List<Vector3>();
            newTileCoords.Add(_currentTilePos);
            newTileCoords.AddRange(CalculateNeighbors());
            foreach (var newTileCoord in newTileCoords)
            {
                if (_spawnedTiles.ContainsKey(newTileCoord))
                    _spawnedTiles[newTileCoord].SetActive(true);
                else
                {
                    AddTile(newTileCoord);
                }
            }
            CalculateInvisibleTiles();
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
            if (dist.magnitude > MaxDistance*2)
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

    private Vector3[] CalculateNeighbors()
    {
        Vector3[] neighbors = new Vector3[8];
        //north
        neighbors[0] = new Vector3(_currentTilePos.x, 0, _currentTilePos.z + MaxDistance);
        //south
        neighbors[1] = new Vector3(_currentTilePos.x, 0, _currentTilePos.z - MaxDistance);
        //east
        neighbors[2] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z);
        //west
        neighbors[3] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z);
        //north-east
        neighbors[4] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z + MaxDistance);
        //north-west
        neighbors[5] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z + MaxDistance);
        //south-east
        neighbors[6] = new Vector3(_currentTilePos.x + MaxDistance, 0, _currentTilePos.z - MaxDistance);
        //south-west
        neighbors[7] = new Vector3(_currentTilePos.x - MaxDistance, 0, _currentTilePos.z - MaxDistance);
        return neighbors;
    }

    private Vector3 CalcultateNextAxisPoint(Vector3 delta)
    {
        Vector3[] neighbors = CalculateNeighbors();
        if (delta.z > MaxDistance / 2 && Mathf.Abs(delta.x) < MaxDistance / 2)
        {
            return neighbors[0];
        }
        if (delta.x > MaxDistance / 2 && Mathf.Abs(delta.z) < MaxDistance / 2)
        {
            return neighbors[2];
        }
        if (delta.z < -MaxDistance / 2 && Mathf.Abs(delta.x) < MaxDistance / 2)
        {
            return neighbors[1];
        }
        if (delta.x < -MaxDistance / 2 && Mathf.Abs(delta.z) < MaxDistance / 2)
        {
            return neighbors[3];
        }

        if (delta.z > MaxDistance / 2 && delta.x > MaxDistance / 2)
        {
            return neighbors[4];
        }
        if (delta.x > MaxDistance / 2 && delta.z < -MaxDistance / 2)
        {
            return neighbors[6];
        }
        if (delta.z < -MaxDistance / 2 && delta.x < -MaxDistance / 2)
        {
            return neighbors[7];
        }
        if (delta.x < -MaxDistance / 2 && delta.z > MaxDistance / 2)
        {
            return neighbors[5];
        }
        return new Vector3();
    }
}
