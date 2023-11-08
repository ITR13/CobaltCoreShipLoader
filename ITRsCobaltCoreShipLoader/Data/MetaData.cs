namespace ITRsCobaltCoreShipLoader.Data;

[Serializable]
public struct MetaData
{
    public string Name = "";
    public string Author = "";
    public string Description = "";
    public List<string> RequiredMods = new();
    public Dictionary<string, ShipLocalization> ExtraLocalization = new();

    public MetaData()
    {
    }
}