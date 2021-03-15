using System.Xml.Serialization;

namespace QuRest.WebUI.Model
{
    [XmlRoot]
    public class QuantumCircuitDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
