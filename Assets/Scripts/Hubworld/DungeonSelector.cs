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
    private GameObject _playerContainer;

    [SerializeField]
    private GameObject _hubContainer;

    [SerializeField]
    private GameObject _dungeonMap;

    public override void OnActivate()
    {
        Debug.Log("OMG I PRESSED THE USE BUTAN");

        Cursor.visible = true;
        
        _playerContainer.SetActive(false);
        _hubContainer.SetActive(false);
        _dungeonMap.SetActive(true);

        base.OnActivate();
    }
}
