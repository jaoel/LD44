using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HubTeleport : InteractiveObject
{
    public override void OnActivate()
    {
        GameSceneManager.Instance.LoadHubScene();
    }
}

