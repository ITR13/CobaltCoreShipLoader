using Newtonsoft.Json;

namespace ITRsCobaltCoreShipLoader.Data;

[Serializable]
public class SimpleStarterShip : StarterShip
{
    [JsonProperty("__meta")]
    public MetaData MetaData;
}