# SOWA 2.0
Rewrite of the original solid waste billing system: SW-Email-ConsoleApplication:

<details>

<summary>List of scheduled jobs</summary>

  | Schadule | Title | Details |
  | --- | --- | --- |
  | 7:00 AM Daily | Container Status | **Delivered.trdp**, Generates an exception report <br /> Recipients:  Deanna, Angie, Debbie George, @Jon Miller and Kyle McDonald |
  | 7:00 AM Daily | Work Order | **WorkOrderException.trdp**, Generates an exception report <br /> Recipients: Deanna, Angie, Debbie George, @Jon Miller and Kyle McDonald |
  | 6:50 AM Daily | Weekly Rental | **SW_Weekly_Rental_ConsoleApplication.trdp**, Creates weekly rental billing transaction records |
  | 7:00 PM Daily | Delinquenct Accounts | **SW-Delinquency.trdp**, Customer delinquency letters are created on the 16th of each month. The collections and counselors csv files are distributed from the 13th through the 15th via email <br /> Recipients: @asim-shaikh1, Jon Miller, Kyle McDonald, Deanna, Angie, Lee Sykes and Tabitha Pusch |
  | 6:00 AM <br />1st working day of month | Monthly Billing | * See detailed monthly billing step below |
  | 12:05 AM Daily | IFAS Cash Receipts | Creates text files (Good.txt, Error.txt and SW_Cash_Receipt_Rpt.txt) of payments entered by users. <br /> Recipients: Deanna, Angie, Jonathan Miller and Kyle McDonald. |
  | 12:05 AM Daily | IFAS KanPay Cash Receipts | Creates text files (KPGood.txt, KPError.txt and SW_KanPay_Cash_Receipt_Rpt.txt) of payments posted by customers via KanPay. <br /> Recipients: Deanna, Angie, Jonathan Miller and Kyle McDonald. |
  | Every 5 minutes Daily | KanPay | Obtains KanPay bank transactions and posts them into the system. |
  
  > **Note**
  > All jobs use **Telerik** for generating reports and **SendGrid** for email purpose and run as SSIS package where?

  #### Monthly bills
  1. SW_Billing_Console_Application: Creates monthly billing records for customers.
  2. SW_Bill_Generate_Console_Application: Runs after step 1, generates a batch monthly billing pdf report file (**SW_Bills.trdp**) for Todd to print which KC Presort picks up and mails out to customers. Recipients: Jon, Deanna, Angie, Todd, and Kyle
  3. SW_Bill_Save_Console_Application: Currently runs manually after step 2 on localhost with bacpac from production loaded onto sqlexpress, Generates the bill blob images for online viewing; upon completion, local bacpac is forwarded to Kyle to load the BillBlobs into production.
  4. SW_BillEmailer_ConsoleApplication: Final step emails all customers who are flagged as paperless/paper and paperless, informing them about the availability of online bills. As a result, a text file containing customer email addresses and send status is generated for the purpose of identifying paperless customers from billed customers and resolving any issues or questions regarding any particular customer(s) and them receiving their bills.
  
  
</details>






