namespace SW.DM;

public class TransactionKanPayFee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Fax { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string Country { get; set; }
    public string Email { get; set; }
    public string Email1 { get; set; }
    public string Email2 { get; set; }
    public string Email3 { get; set; }
    public string Last4Number { get; set; }
    public string TotalAmount { get; set; }
    public string ReceiptDate { get; set; }
    public string ReceiptTime { get; set; }
    public string OrderId { get; set; }
    public string AuthCode { get; set; }
    public string FailMessage { get; set; }
    public string LocalRefId { get; set; }
    public string PayType { get; set; }
    public string ExpirationDate { get; set; }
    public string CreditCardType { get; set; }
    public string BillingName { get; set; }
    public string AvsResponse { get; set; }
    public string CvvResponse { get; set; }
    public string FailCode { get; set; }
    public string AchDate { get; set; }
    public string Token { get; set; }
    public string OrigFeeAmount { get; set; }
    public string OrigCosAmount { get; set; }
    public string OrigTransTotal { get; set; }
    public string CustomerId { get; set; }
    public string TransactionId { get; set; }
    public string CustomerType { get; set; }
    public string IfasObjectCode { get; set; }
    public bool TransactionKanPayFeeDelete { get; set; }
    public string TransactionKanPayFeeAddToi { get; set; }
    public string TransactionKanPayFeeChgToi { get; set; }
    public string TransactionKanPayFeeDelToi { get; set; }
    public DateTime TransactionKanPayFeeAddDateTime { get; set; }
    public DateTime? TransactionKanPayFeeChgDateTime { get; set; }
    public DateTime? TransactionKanPayFeeDelDateTime { get; set; }
    public string SncoFeeAmount { get; set; }
}
