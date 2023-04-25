﻿using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.CustomerTransactionHistory;

public class CustomerTransactionHistoryViewModel
{
    public List<CustomerTransactionViewModel> Transactions { get; set; }

    public int CustomerID { get; set; }
    [Display(Name = "Start Date")]
    public DateTime startDate { get; set; }
    [Display(Name = "End Date")]
    public DateTime endDate { get; set; }
}
