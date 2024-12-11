using System.Collections.Generic;

[System.Serializable]
public class InventoryData
{
    public List<string> items;

    public InventoryData(List<string> items)
    {
        this.items = items;
    }
}
