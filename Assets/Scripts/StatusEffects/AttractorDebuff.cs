using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AttractorDebuff : StatusEffect
{
    public GameObject target;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnApply(IBuffable owner, GameObject ownerGameObject, SpriteRenderer ownerRenderer, 
        Navigation navigation = null)
    {
        base.OnApply(owner, ownerGameObject, ownerRenderer, navigation);
    }

    public override void Navigation()
    {
        base.Navigation();

        if (target != null && _navigation != null)
        {
            _navigation.MoveTo(target, true);
        }
    }
}
