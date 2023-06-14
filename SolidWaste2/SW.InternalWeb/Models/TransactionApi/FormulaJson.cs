namespace SW.InternalWeb.Models.TransactionApi;

public class FormulaJson
{
    public string Name { get; set; }
    public string FormulaString { get; set; }
    public string CommentString { get; set; }

    public IEnumerable<ParameterJson> Parameters { get; set; }

    public class ParameterJson
    {
        public string ParameterId { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public bool Constant { get; set; }
    }
}
