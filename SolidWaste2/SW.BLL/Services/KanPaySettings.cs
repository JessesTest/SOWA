namespace SW.BLL.Services;

public class KanPaySettings
{
    public Uri Domain { get; set; }
    public string MerchantKey { get; set; }
    public string MerchantId { get; set; }
    public string ServiceCodeCC { get; set; }
    public string ServiceCodeCE { get; set; }
    public string ServiceCodeRC { get; set; }
    public string ServiceCodeRE { get; set; }
    public Uri CommonCheckoutPage { get; set; }
    public decimal SncoFeeAmountCE { get; set; }
    public decimal SncoFeeAmountRC { get; set; }
}
