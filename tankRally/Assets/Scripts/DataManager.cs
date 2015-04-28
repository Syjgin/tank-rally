using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class DataManager
{
    private const string TankRotationKey = "TankRotationKey";
    private const string TerrainOffsetKey = "TerrainOffsetKey";
    private const string MapDataObject = "MapDataObject";
    private const string TankPositionKey = "TankPositionKey";

    private static DataManager _instance = null;

    public static DataManager GetInstance()
    {
        if(_instance == null)
            _instance = new DataManager();
        return _instance;
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

    

    public void SaveMapInfo(List<ObstaclesManager.MapObjectData> mapData)
    {
        PlayerPrefs.SetInt(MapDataObject + "Size", mapData.Count);
        int currentIndex = 0;
        foreach (var mapObjectData in mapData)
        {
            PlayerPrefs.SetString(MapDataObject + currentIndex + "Type", mapObjectData.ObjectType.ToString());
            PlayerPrefs.SetFloat(MapDataObject + currentIndex + "Rotation", mapObjectData.Rotation);
            PlayerPrefs.SetFloat(MapDataObject + currentIndex + "XCoord", mapObjectData.XCoord);
            PlayerPrefs.SetFloat(MapDataObject + currentIndex + "YCoord", mapObjectData.YCoord);
            currentIndex++;
        }
    }

    public List<ObstaclesManager.MapObjectData> LoadMapInfo()
    {
        List<ObstaclesManager.MapObjectData> result = new List<ObstaclesManager.MapObjectData>();
        int count = PlayerPrefs.GetInt(MapDataObject + "Size", 0);
        for (int i = 0; i < count; i++)
        {
            ObstaclesManager.MapObjectData objectData = new ObstaclesManager.MapObjectData();
            string objType = PlayerPrefs.GetString(MapDataObject + i + "Type");
            objectData.ObjectType =
                (ObstaclesManager.MapObjectType) Enum.Parse(typeof (ObstaclesManager.MapObjectType), objType);
            objectData.Rotation = PlayerPrefs.GetFloat(MapDataObject + i + "Rotation");
            objectData.XCoord = PlayerPrefs.GetFloat(MapDataObject + i + "XCoord");
            objectData.YCoord = PlayerPrefs.GetFloat(MapDataObject + i + "YCoord");
            result.Add(objectData);
        }
        return result;
    }
}
