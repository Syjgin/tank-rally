using System;
using UnityEngine;
using System.Collections;

public class DataManager
{
    private float _tankRotation;
    private Vector2 _tankOffset;

    private const string TankRotationKey = "TankRotationKey";
    private const string TankOffsetKey = "TankOffsetKey";

    private static DataManager _instance = null;

    public static DataManager GetInstance()
    {
        if(_instance == null)
            _instance = new DataManager();
        return _instance;
    }

    private DataManager()
    {
        LoadTankInfo();
    }

    private void LoadTankInfo()
    {
        float offsetX = PlayerPrefs.GetFloat(TankOffsetKey + "x", 0);
        float offsetY = PlayerPrefs.GetFloat(TankOffsetKey + "y", 0);
        _tankOffset = new Vector2(offsetX, offsetY);
        _tankRotation = PlayerPrefs.GetFloat(TankRotationKey);
    }

    public void SaveTankInfo(float rotation, Vector2 offset)
    {
        _tankOffset = offset;
        _tankRotation = rotation;
        PlayerPrefs.SetFloat(TankOffsetKey + "x", offset.x);
        PlayerPrefs.SetFloat(TankOffsetKey + "y", offset.y);
        PlayerPrefs.SetFloat(TankRotationKey, rotation);
    }

    public float GetTankRotation()
    {
        return _tankRotation;
    }

    public Vector2 GetTankOffset()
    {
        return _tankOffset;
    }
}
