using System;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager Instance { get; private set; }
    private SaveData saveData = new SaveData();

    [Serializable]
    public class SaveData
    {
        public PlayerData playerData;
    }

    [Serializable]
    public class PlayerData
    {

    }
}
