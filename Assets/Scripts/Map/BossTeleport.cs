using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BossTeleport : InteractiveObject
{
    public bool active;

    private void Awake()
    {
        active = false;
    }

    public override void OnActivate()
    {
        if (!active)
        {
            return;
        }

        GameSceneManager.Instance.LoadBossScene();
    }
}
