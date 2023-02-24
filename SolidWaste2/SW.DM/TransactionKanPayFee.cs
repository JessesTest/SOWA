using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class TransactionKanPayFee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [StringLength(255)]
    public string Name { get; set; }
    [StringLength(255)]
    public string Phone { get; set; }
    [StringLength(255)]
    public string Fax { get; set; }
    [StringLength(255)]
    public string Address1 { get; set; }
    [StringLength(255)]
    public string Address2 { get; set; }
    [StringLength(255)]
    public string City { get; set; }
    [StringLength(255)]
    public string State { get; set; }
    [StringLength(255)]
    public string Zip { get; set; }
    [StringLength(255)]
    public string Country { get; set; }
    [StringLength(255)]
    public string Email { get; set; }
    [StringLength(255)]
    public string Email1 { get; set; }
    [StringLength(255)]
    public string Email2 { get; set; }
    [StringLength(255)]
    public string Email3 { get; set; }
    [StringLength(255)]
    public string Last4Number { get; set; }        
    [StringLength(255)]
    public string TotalAmount { get; set; }
    [StringLength(255)]
    public string ReceiptDate { get; set; }
    [StringLength(255)]
    public string ReceiptTime { get; set; }
    [StringLength(255)]
    public string OrderId { get; set; }
    [StringLength(255)]
    public string AuthCode { get; set; }
    [StringLength(255)]
    public string FailMessage { get; set; }
    [StringLength(255)]
    public string LocalRefId { get; set; }
    [StringLength(255)]
    public string PayType { get; set; }
    [StringLength(255)]
    public string ExpirationDate { get; set; }
    [StringLength(255)]
    public string CreditCardType { get; set; }
    [StringLength(255)]
    public string BillingName { get; set; }
    [StringLength(255)]
    public string AvsResponse { get; set; }
    [StringLength(255)]
    public string CvvResponse { get; set; }
    [StringLength(255)]
    public string FailCode { get; set; }
    [StringLength(255)]
    public string AchDate { get; set; }
    [StringLength(255)]
    public string Token { get; set; }
    [StringLength(255)]
    public string Orig_Fee_Amount { get; set; }
    [StringLength(255)]
    public string Orig_Cos_Amount { get; set; }
    [StringLength(255)]
    public string Orig_Trans_Total { get; set; }
    [StringLength(255)]
    public string CustomerId { get; set; }
    [StringLength(255)]
    public string TransactionId { get; set; }
    [StringLength(255)]
    public string CustomerType { get; set; }
    [StringLength(255)]
    public string SncoFeeAmount { get; set; }
    [StringLength(255)]
    public string IfasObjectCode { get; set; }
    public bool TransactionKanPayFeeDelete { get; set; }
    [StringLength(255)]
    public string TransactionKanPayFeeAddToi { get; set; }
    [StringLength(255)]
    public string TransactionKanPayFeeChgToi { get; set; }
    [StringLength(255)]
    public string TransactionKanPayFeeDelToi { get; set; }
    public DateTime TransactionKanPayFeeAddDateTime { get; set; }
    public DateTime? TransactionKanPayFeeChgDateTime { get; set; }
    public DateTime? TransactionKanPayFeeDelDateTime { get; set; }
}
