using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GameSessionData
{
    public int PlayerMaxHealth;

    public void LoadData()
    {
        SetDefaultData();
    }

    public void UpdateSessionData(Player player)
    {
        PlayerMaxHealth = player.MaxHealth;
    }

    private void SetDefaultData()
    {
        PlayerMaxHealth = 30;
    }
}
