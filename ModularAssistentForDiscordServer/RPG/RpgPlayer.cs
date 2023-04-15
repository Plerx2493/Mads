// Copyright 2023 Plerx2493
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace MADS.RPG;

public class RpgPlayer
{
    public ulong UserId;
    public string Name;
    public RpgPlayerStats Stats;
    
    public List<RpgItem> Inventory;
    public RpgItem[] Equipment;
    
    public RpgPlayer(ulong userId, string name, RpgPlayerStats stats)
    {
        UserId = userId;
        Name = name;
        Stats = stats;
    }
    
    public void AddItemToInventory(RpgItem item)
    {
        if (Inventory.Count < Stats.InventorySize)
        {
            Inventory.Add(item);
        }
    }
    
    public void RemoveItemFromInventory(RpgItem item)
    {
        Inventory.Remove(item);
    }
    
    public void EquipItem(RpgItem item)
    {
        switch (item.Type)
        {
            case RpgItemType.Weapon:
                Equipment[0] = item;
                break;

            case RpgItemType.Armor:
                Equipment[1] = item;
                break;

            case RpgItemType.Accessory:
                Equipment[2] = item;
                break;
        }
    }
    
    
}