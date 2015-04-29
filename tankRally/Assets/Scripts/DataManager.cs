using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private const string TankRotationKey = "TankRotationKey";
    private const string TerrainOffsetKey = "TerrainOffsetKey";
    private const string MapDataObject = "MapDataObject";
    private const string TankPositionKey = "TankPositionKey";
    private const string ConfigName = "config";

    private const float MinTankVelocity = 0.01f;
    private const float MaxTankVelocity = 9;
    private const int MaxVisibleObjectsFallback = 50;
    private const int BushSpawnPeriodFallback = 50;
    private const int MinimalBushDistanceFallback = 50;
    private const float TankVelocityFallback = 0.1f;
    private const float TankAngularVelocityFallback = 1;
    private const float TreePossibilityFallback = 10;
    private const float BushPossibilityFallback = 30;
    private const float PuddlePossibilityFallback = 10;
    private const float StonePossibilityFallback = 50;

    private int _maxVisibleObjects;
    private int _bushSpawnPeriod;
    private int _minimalBushDistance;
    private float _tankVelocity;
    private float _tankAngularVelocity;
    private Dictionary<ObstaclesManager.MapObjectType, float> _possibilities; 

    private static DataManager _instance = null;

    public static DataManager GetInstance()
    {
        if(_instance == null)
            _instance = new DataManager();
        return _instance;
    }

    private DataManager()
    {
        //config reading
        _possibilities = new Dictionary<ObstaclesManager.MapObjectType, float>();
        bool possibilitiesParseFailed = false;
        TextAsset config = Resources.Load(ConfigName) as TextAsset;
        if (config != null)
        {
            string unparsedConfig = config.text;   
            JSONObject parsedConfig = new JSONObject(unparsedConfig);
            if(!parsedConfig.GetField(ref _maxVisibleObjects, "MaxVisibleObjects", MaxVisibleObjectsFallback))
                Debug.LogError("MaxVisibleObjects parse failed"); 
            if(!parsedConfig.GetField(ref _bushSpawnPeriod, "BushSpawnPeriod", BushSpawnPeriodFallback))
                Debug.LogError("BushSpawnPeriod parse failed"); 
            if(!parsedConfig.GetField(ref _minimalBushDistance, "MinimalBushDistance", MinimalBushDistanceFallback))
                Debug.LogError("MinimalBushDistance parse failed"); 
            if(!parsedConfig.GetField(ref _tankVelocity, "TankVelocity", TankVelocityFallback))
                Debug.LogError("TankVelocity parse failed"); 
            if (_tankVelocity < MinTankVelocity)
                _tankVelocity = MinTankVelocity;
            if (_tankVelocity > MaxTankVelocity)
                _tankVelocity = MaxTankVelocity;
            parsedConfig.GetField(ref _tankAngularVelocity, "TankAngularVelocity", TankAngularVelocityFallback);
            JSONObject possibilitiesObject = parsedConfig.GetField("Possibilities");
            float treePossibility = 0;
            float puddlePossibility = 0;
            float bushPossibility = 0;
            float stonePossibility = 0;
            if (possibilitiesObject != null)
            {
                if(!possibilitiesObject.GetField(ref treePossibility, "Tree", TreePossibilityFallback))
                    Debug.LogError("tree possibility parse failed"); 
                if(!possibilitiesObject.GetField(ref puddlePossibility, "Puddle", PuddlePossibilityFallback))
                    Debug.LogError("puddle possibility parse failed"); 
                if(!possibilitiesObject.GetField(ref bushPossibility, "Bush", BushPossibilityFallback))
                    Debug.LogError("bush possibility parse failed"); 
                if(!possibilitiesObject.GetField(ref stonePossibility, "Stone", StonePossibilityFallback))
                    Debug.LogError("stone possibility parse failed"); 

                _possibilities.Add(ObstaclesManager.MapObjectType.Tree, treePossibility);
                _possibilities.Add(ObstaclesManager.MapObjectType.Bush, bushPossibility);
                _possibilities.Add(ObstaclesManager.MapObjectType.Puddle, puddlePossibility);
                _possibilities.Add(ObstaclesManager.MapObjectType.Stone, stonePossibility);
            }
            else
            {
                possibilitiesParseFailed = true;
            }
        }
        else
        {
            _maxVisibleObjects = MaxVisibleObjectsFallback;
            _bushSpawnPeriod = BushSpawnPeriodFallback;
            _minimalBushDistance = MinimalBushDistanceFallback;
            _tankVelocity = TankVelocityFallback;
            _tankAngularVelocity = TankAngularVelocityFallback;
            possibilitiesParseFailed = true;
        }
        if (possibilitiesParseFailed)
        {
            _possibilities.Add(ObstaclesManager.MapObjectType.Tree, TreePossibilityFallback);
            _possibilities.Add(ObstaclesManager.MapObjectType.Bush, BushPossibilityFallback);
            _possibilities.Add(ObstaclesManager.MapObjectType.Puddle, PuddlePossibilityFallback);
            _possibilities.Add(ObstaclesManager.MapObjectType.Stone, StonePossibilityFallback);
        }
    }

    //tank related data

    public float GetTankVelocity()
    {
        return _tankVelocity;
    }

    public float GetTankAngularVelocity()
    {
        return _tankAngularVelocity;
    }

    public float GetTankRotation()
    {
        return PlayerPrefs.GetFloat(TankRotationKey, 0);
    }

    public void SaveTankRotation(float rotation)
    {
        PlayerPrefs.SetFloat(TankRotationKey, rotation);
    }

    public Vector2 GetTankPosition()
    {
        float offsetX = PlayerPrefs.GetFloat(TankPositionKey + "x", 0);
        float offsetY = PlayerPrefs.GetFloat(TankPositionKey + "y", 0);
        return new Vector2(offsetX, offsetY);
    }

    public void SaveTankPosition(Vector2 position)
    {
        PlayerPrefs.SetFloat(TankPositionKey + "x", position.x);
        PlayerPrefs.SetFloat(TankPositionKey + "y", position.y);
    }


    //obstacles related data

    public int GetMaxVisibleObjects()
    {
        return _maxVisibleObjects;
    }

    public float GetBushSpawnPeriod()
    {
        return _bushSpawnPeriod;
    }

    public float GetBushMinimalDistance()
    {
        return _minimalBushDistance;
    }

    public Dictionary<ObstaclesManager.MapObjectType, float> GetPossibilities()
    {
        return _possibilities;
    }

    public void SaveMapInfo(string mapData)
    {
        PlayerPrefs.SetString(MapDataObject, mapData);
    }

    public string LoadMapInfo()
    {
        return PlayerPrefs.GetString(MapDataObject, "");
    }

    //terrain related data

    public Vector2 GetTerrainOffset()
    {
        float offsetX = PlayerPrefs.GetFloat(TerrainOffsetKey + "x", 0);
        float offsetY = PlayerPrefs.GetFloat(TerrainOffsetKey + "y", 0);
        return new Vector2(offsetX, offsetY);
    }

    public void SaveTerrainOffset(float rotation, Vector2 offset)
    {
        PlayerPrefs.SetFloat(TerrainOffsetKey + "x", offset.x);
        PlayerPrefs.SetFloat(TerrainOffsetKey + "y", offset.y);
    }

}
