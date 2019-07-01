using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DungeonSelector : InteractiveObject
{
    [SerializeField]
    public DungeonData dungeonData;

    public override void OnActivate()
    {
        MapManager.Instance.SetDungeonData(dungeonData);
        GameSceneManager.Instance.LoadGameplayScene();

        onActivate?.Invoke();
    }
}
