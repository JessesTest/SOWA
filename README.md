# SOWA2.0
Rewrite of the original solidwaste billing systemSW-Email-ConsoleApplication:

<details>

<summary>List of scheduled jobs</summary>

  | Schadule | Title | Details |
  | --- | --- | --- |
  | 7:00 AM Daily | Container Status | **Delivered.trdp**, Generates an exception report <br /> Recipients:  Deanna, Angie, Debbie George, @Jon Miller and Kyle McDonald |
  | 7:00 AM Daily | Work Order | **WorkOrderException.trdp**, Generates an exception report <br /> Recipients: Deanna, Angie, Debbie George, @Jon Miller and Kyle McDonald |
  | 6:50 AM ?? | Weekly Rental | **SW_Weekly_Rental_ConsoleApplication.trdp**, Creates weekly rental billing transaction records |
  | 7:00 PM Daily | Delinquenct Accounts | **SW-Delinquency.trdp**, Customer delinquency letters are created on the 16th of each month. The collections and counselors csv files are distributed from the 13th through the 15th via email <br /> Recipients: @asim-shaikh1, Jon Miller, Kyle McDonald, Deanna, Angie, Lee Sykes and Tabitha Pusch |
  | 6:00 AM <br />1st working day of month | Monthly Billing | * See detailed monthly billing step below |
  
  > **Note**
  > All jobs use **Telerik** for generating reports and **SendGrid** for email purpose and run as SSIS package where?

  #### Monthly bills
  1. SW_Bill_Generate_Console_Application: Creates table records
  2. SW_Bill_Generate_Console_Application: Runs manually after step 1, generates a batch Billing pdf (For ?) and emails to Jon, Deanna, Angie, Todd, and Kyle
  3. SW_Bill_Save_Console_Application: Runs manually after step 2 on localhost with bacpac from production loaded onto sqlexpress, Generates the bill blob images for online viewing; upon completion, local bacpac is forwarded to Kyle to load the BillBlobs into production
  4. SW_BillEmailer_ConsoleApplication: Final step emails all customers who are flagged as paperless/paper and paperless, informing them about the availability of online bills. As a result, a text file containing customer email addresses and send status is being generated. (Purpose ?)
  
  
</details>






SW_IFAS_Cash_Receipt_Console_App
Creates txt files (Good.txt, Error.txt and Cash_Receipt_Rpt_txt) of payments
entered by SW Department. Emails file to Deanna, Angie, myself and Kyle. Runs
at 12:05 am for previous day processing.
SW_IFAS_KanPay_Cash_Receipt_Console_App
Creates txt files (Good.txt, Error.txt and Cash_Receipt_Rpt_txt) of payments
posted by customers via KanPay. Emails file to Deanna, Angie, myself and Kyle. Runs
at 12:05 am for previous day processing.
SW_KanPay_ConsoleApplication
Gets KanPay bank transactions and posts them into our SW system. Runs 24/7 every 5 mins or
often. No Reports. (edited) 

