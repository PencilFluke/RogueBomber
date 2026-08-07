using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public MetaProgressData Meta { get; private set; }

    [Serializable]
    public class MetaProgressData
    {
        public int saveVersion = 1;
        public int highestWaveReached;
        public int currency;
        public List<UpgradeEntry> upgrades = new List<UpgradeEntry>();
    }

    public class UpgradeEntry
    {
        public string upgradeId;
        public int tier;
    }


    [Serializable]
    public class RunStateData
    {
    }
}
