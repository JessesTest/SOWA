using Common.Extensions;
using Common.Web.Extensions.Alerts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PE.BL.Services;
using PE.DM;
using SW.BLL.KanPay;
using SW.BLL.Services;
using SW.DM;
using SW.ExternalWeb.Identity;
using SW.ExternalWeb.Models;
using System.Text.RegularExpressions;

namespace SW.ExternalWeb.Controllers
{
    public class KanPayController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPersonEntityService personEntityService;
        private readonly ICustomerService customerService;
        private readonly IBillMasterService billMasterService;
        private readonly ITransactionService transactionService;
        private readonly IKanPayService kanPayService;
        private readonly KanPaySettings kanPaySettings;
        private readonly IPaymentPlanService paymentPlanService;

        public KanPayController(
            UserManager<ApplicationUser> userManager,
            IPersonEntityService personEntityService,
            ICustomerService customerService,
            IBillMasterService billMasterService,
            ITransactionService transactionService,
            IKanPayService kanPayService,
            IOptions<KanPaySettings> kanPayOptions,
            IPaymentPlanService paymentPlanService)
        {
            this.userManager = userManager;
            this.personEntityService = personEntityService;
            this.customerService = customerService;
            this.billMasterService = billMasterService;
            this.transactionService = transactionService;
            this.kanPayService = kanPayService;
            kanPaySettings = kanPayOptions.Value;
            this.paymentPlanService = paymentPlanService;
        }

        [HttpPost]
        public async Task<IActionResult> Index(AccountHomeMenuViewModel x)
        {
            ApplicationUser user = await userManager.FindByIdAsync(User.GetUserId());
            PersonEntity person = await personEntityService.GetById(user.UserId);
            Customer customer = await customerService.GetByPE(person.Id);
            BillMaster currentBillMaster = await billMasterService.GetMostRecentBillMaster(customer.CustomerId);
            Transaction currentBillMasterTransaction = await transactionService.GetById(currentBillMaster.TransactionId);

            bool customerHasKanPay = await kanPayService.AnyByCustomer(currentBillMaster.CustomerId.ToString());
            if (customerHasKanPay)
                return RedirectToAction("BillSummary", "Home").WithInfo(string.Concat("Quick Pay: Payment of $", x.KanPayPayment, " is in process, please inquire in a few minutes."), "");

            string Domain = kanPaySettings.Domain.ToString();
            string KanPayMerchantKey = kanPaySettings.MerchantKey;
            string KanPayMerchantId = kanPaySettings.MerchantId;
            string KanPayServiceCode;// = 
                //ConfigurationManager.AppSettings[
                //    string.Concat(
                //        "KanPayServiceCode",
                //        currentBillMaster.CustomerType.Replace("H", "C"),
                //        x.KanPayPaymentChoice.Replace("CC", "C").Replace("ACH", "E"))].ToString()


            var paymentChoice = x.KanPayPaymentChoice.ToUpper().Trim();
            var paymentChoicess = new string[] { "CC", "ACH" };
            if (!paymentChoicess.Contains(paymentChoice))
                throw new ApplicationException($"Unknown payment choice {paymentChoice}");

            var customerType = currentBillMaster.CustomerType.ToUpper().Trim();
            var commercialCustomerTypes = new string[] { "C", "H" };
            var residentialCustomerTypes = new string[] { "R" };
            if (commercialCustomerTypes.Contains(customerType))
            {
                KanPayServiceCode = paymentChoice == "CC" ?
                    kanPaySettings.ServiceCodeCC :
                    kanPaySettings.ServiceCodeCE;
            }
            else if(residentialCustomerTypes.Contains(customerType))
            {
                KanPayServiceCode = paymentChoice == "CC" ?
                    kanPaySettings.ServiceCodeRC :
                    kanPaySettings.ServiceCodeRE;
            }
            else
            {
                throw new ApplicationException($"Unknown customer type {customerType}");
            }

            string KanPayCommonCheckoutPage = kanPaySettings.CommonCheckoutPage.ToString();

            PaymentInfov2 r = new()
            {
                MERCHANTKEY = KanPayMerchantKey,
                MERCHANTID = KanPayMerchantId,
                SERVICECODE = KanPayServiceCode,
                UNIQUETRANSID = Guid.NewGuid().ToString(),
                STATECD = "KS",
                COUNTRY = "US",
                AMOUNT = x.KanPayPayment,     //string(??)
                CID = currentBillMasterTransaction.CustomerId.ToString(),     //string(20)
                LOCALREFID = Guid.NewGuid().ToString().Replace("-", ""),      //LOCALREFID string(48) used for duplicate payment checking
                PAYTYPE = x.KanPayPaymentChoice,     //string(3)   CC,ACH
                NAME = currentBillMaster.BillingName.Length > 50 ? currentBillMaster.BillingName.Substring(0, 50) : currentBillMaster.BillingName,     //string(50) will appear on panel 3 Payment Info within Address porition
                COMPANYNAME = null,           //string(50) will appear on panel 3 Payment Info within Address porition
                ADDRESS1 = currentBillMaster.BillingAddressStreet.Length > 35 ? currentBillMaster.BillingAddressStreet.Substring(0, 35) : currentBillMaster.BillingAddressStreet,     //string(35) will appear on panel 3 Payment Info within Address porition
                ADDRESS2 = null,              //string(35) will appear on panel 3 Payment Info within Address porition above city,state,zip             
                HREFCANCEL = Domain + "KanPay/CcpReturnCancel",
                HREFFAILURE = Domain + "KanPay/CcpReturnFailure",
                HREFDUPLICATE = Domain + "KanPay/CcpReturnDuplicate",
                HREFSUCCESS = Domain + "KanPay/CcpReturnSuccess"
            };     //instantiate payment information that will appear on the kanpay panel system.

            if (currentBillMaster.BillingAddressCityStateZip == null)
            {
                r.ZIP = null;
                r.STATE = null;
                r.CITY = null;
            }
            else
            {
                string w = currentBillMaster.BillingAddressCityStateZip;
                r.ZIP = w.Remove(0, w.LastIndexOf(" ")).Trim();     //string(16)            
                w = w.Replace(r.ZIP, "").Trim();
                r.CITY = w.Remove(w.LastIndexOf(" ")).Trim().Length > 32 ? w.Remove(w.LastIndexOf(" ")).Trim().Substring(0, 32) : w.Remove(w.LastIndexOf(" ")).Trim();    //string(32)                    
                r.STATE = w.Replace(r.CITY, "").Trim();      //string(2)
            }

            if (person.Phones.Count == 0)   //number(20) accepted format 999-999-9999
            {
                r.PHONE = null;
            }
            else
            {
                r.PHONE = person.Phones.ToList()[0].PhoneNumber.Length > 20 ? person.Phones.ToList()[0].PhoneNumber.Substring(0, 20) : person.Phones.ToList()[0].PhoneNumber;
                Regex phoneRegex = new Regex(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$");
                if (phoneRegex.IsMatch(r.PHONE))
                {
                    r.PHONE = phoneRegex.Replace(r.PHONE, "$1-$2-$3");
                }
                else
                {
                    r.PHONE = null;
                }
            }
            if (person.Emails.Count == 0)     //string(50)
            {
                r.EMAIL = null;
            }
            else
            {
                r.EMAIL = person.Emails.ToList()[0].Email1.Length > 50 ? person.Emails.ToList()[0].Email1.Substring(0, 50) : person.Emails.ToList()[0].Email1;
            }

            using ServiceWebClient proxy = new();
            try
            {
                PreparePaymentv2Result result = await proxy.PreparePaymentv2Async(r);      //request a token from kanpay  

                if (result.ERRORCODE == 0)
                {
                    SW.DM.KanPay token = new()
                    {
                        KanPayTokenId = result.TOKEN,
                        KanPayAmount = r.AMOUNT,
                        KanPayCid = r.CID,
                        KanPayCustomerType = currentBillMaster.CustomerType
                    };

                    await kanPayService.Add(token, User.GetName());

                    return Redirect(string.Concat(KanPayCommonCheckoutPage, result.TOKEN));     //Redirect to KANPAY COMMON CHECK PAGE for Payment Processing...
                }
                else
                {
                    return RedirectToAction("BillSummary", "Home").WithDanger(string.Concat("Quick Pay: unable to return token ", result.ERRORMESSAGE), "");   //.witherrormessage unable to return token
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("BillSummary", "Home").WithDanger(string.Concat("Quick Pay: unable to redirect ", e.Message), "");   //.witherrormessage unable to redirect to KANPAY
            }
        }

        public async Task<IActionResult> CcpReturnCancel(string token)     //CANCEL LandingPage Return from KanPay Common CheckOut Page
        {
            await kanPayService.DeleteByToken(token);

            return RedirectToAction("BillSummary", "Home").WithInfo("Quick Pay: cancelled", "");   //.witherrormessage kanpay process was cancelled
        }

        public async Task<IActionResult> CcpReturnFailure(string token, int? errorcode, string errormessage)     //FAILURE LandingPage Return from KanPay Common CheckOut Page
        {
            await kanPayService.DeleteByToken(token);

            return RedirectToAction("BillSummary", "Home").WithDanger(string.Concat("Quick Pay: failure ", errormessage), "");   //.witherrormessage kanpay process was failure
        }

        public async Task<IActionResult> CcpReturnDuplicate(string token, int? errorcode, string errormessage)     //DUPLICATE LandingPage Return from KanPay Common CheckOut Page
        {
            await kanPayService.DeleteByToken(token);

            return RedirectToAction("BillSummary", "Home").WithInfo(string.Concat("Quick Pay: duplicate ", errormessage), "");   //.witherrormessage kanpay process was duplicate 
        }

        public async Task<IActionResult> CcpReturnSuccess(string token)     //SUCCESS LandingPage Return from KanPay Common CheckOut Page
        {
            //START: the following proxy logic is also expressed in the SW_KanPay_ConsoleApplication(Program.cs)  please keep insync...
            using ServiceWebClient proxy = new();
            try
            {
                GetPaymentInfoResponse response = await proxy.GetPaymentInfoAsync(token);
                if (response.FAILCODE == "N")
                {
                    //post transaction...
                    Transaction tra = new();
                    var tok = await kanPayService.GetByToken(token);
                    tra.CustomerId = int.Parse(tok.First().KanPayCid);
                    var paymentPlan = await paymentPlanService.GetActiveByCustomer(tra.CustomerId, true);
                    tra.TransactionCodeId = paymentPlan == null ? 36 : 24;
                    tra.TransactionAmt = decimal.Parse(tok.First().KanPayAmount);
                    tra.Comment = string.Concat("KanPay ", tok.First().KanPayTokenId);

                    //post fee...
                    TransactionKanPayFee f = new()
                    {
                        Name = response.NAME,
                        Phone = response.PHONE,
                        Fax = response.FAX,
                        Address1 = response.ADDRESS1,
                        Address2 = response.ADDRESS2,
                        City = response.CITY,
                        State = response.STATE,
                        Zip = response.ZIP,
                        Country = response.COUNTRY,
                        Email = response.EMAIL,
                        Email1 = response.EMAIL1,
                        Email2 = response.EMAIL2,
                        Email3 = response.EMAIL3,
                        Last4Number = response.LAST4NUMBER,
                        TotalAmount = response.TOTALAMOUNT,
                        ReceiptDate = response.RECEIPTDATE,
                        ReceiptTime = response.RECEIPTTIME,
                        OrderId = response.ORDERID.ToString(),
                        AuthCode = response.AUTHCODE,
                        FailMessage = response.FAILMESSAGE,
                        LocalRefId = response.LOCALREFID,
                        PayType = response.PAYTYPE,
                        ExpirationDate = response.PAYTYPE.Contains("CC") ? response.ExpirationDate.ToShortDateString() : "NULL",
                        CreditCardType = response.CreditCardType,
                        BillingName = response.BillingName,
                        AvsResponse = response.AVSResponse,
                        CvvResponse = response.CVVResponse,
                        FailCode = response.FAILCODE,
                        AchDate = response.ACHDATE,
                        Token = response.TOKEN,
                        CustomerId = tra.CustomerId.ToString(),
                        //f.TransactionId
                        CustomerType = tok.First().KanPayCustomerType
                    };
                    foreach (var e in response.extendedValues)
                    {
                        if (e.NAME.Contains("_FEE_AMOUNT"))
                        {
                            f.OrigFeeAmount = e.VALUE;
                        }
                        if (e.NAME.Contains("_COS_AMOUNT"))
                        {
                            f.OrigCosAmount = e.VALUE;
                        }
                        if (e.NAME.Contains("_TRANS_TOTAL"))
                        {
                            f.OrigTransTotal = e.VALUE;
                        }
                    }
                    f.SncoFeeAmount = null;
                    f.SncoFeeAmount = (f.CustomerType == "C" && f.PayType == "ACH") ? kanPaySettings.SncoFeeAmountCE.ToString() : f.SncoFeeAmount;
                    f.SncoFeeAmount = (f.CustomerType == "H" && f.PayType == "ACH") ? kanPaySettings.SncoFeeAmountCE.ToString() : f.SncoFeeAmount;
                    f.SncoFeeAmount = (f.CustomerType == "R" && f.PayType == "CC") ?
                        Math.Round(Decimal.Parse(f.OrigCosAmount) * kanPaySettings.SncoFeeAmountRC, 2, MidpointRounding.AwayFromZero).ToString() :
                        f.SncoFeeAmount;

                    await transactionService.AddKanpayTransaction(tra, f, tok.First().KanPayId, User.Identity.Name);     //tra=transaction, f=fee, tok=token

                    return RedirectToAction("BillSummary", "Home").WithSuccess("Quick Pay Successful! A confirmation invoice has been sent to " + response.EMAIL + ".", "");   //.witherrormessage kanpay process was successfull
                }
                else
                {
                    await kanPayService.DeleteByToken(token);

                    return RedirectToAction("BillSummary", "Home").WithDanger(string.Concat("Quick Pay: SUCCESS LandingPage failure ", response.FAILMESSAGE), "");   //.witherrormessage unable to return token
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("BillSummary", "Home").WithDanger(string.Concat("Quick Pay: SUCCESS LandingPage exception ", e.Message), "");   //.witherrormessage unable to return token
            }

            //END: proxy logic is also expressed in the SW_KanPay_ConsoleApplication(Program.cs)  please keep insync...                
        }
    }
}
