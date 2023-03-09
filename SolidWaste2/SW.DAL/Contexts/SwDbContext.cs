using Microsoft.EntityFrameworkCore;
using SW.DM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SW.DAL.Contexts
{
    public class SwDbContext : DbContext
    {
        public virtual DbSet<BillBlobs> BillBlobs { get; set; }
        public virtual DbSet<BillContainerDetail> BillContainerDetails { get; set; }
        public virtual DbSet<BillMaster> BillMasters { get; set; }
        public virtual DbSet<BillServiceAddress> BillServiceAddresses { get; set; }
        public virtual DbSet<CodeRef> CodeRefs { get; set; }
        public virtual DbSet<Container> Containers { get; set; }
        public virtual DbSet<ContainerCode> ContainerCodes { get; set; }
        public virtual DbSet<ContainerRate> ContainerRates { get; set; }
        public virtual DbSet<ContainerSubtype> ContainerSubtypes { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<EquipmentLegacy> EquipmentLegacies { get; set; }
        public virtual DbSet<Formula> Formulas { get; set; }
        public virtual DbSet<KanPay> KanPays { get; set; }
        public virtual DbSet<MieData> MieDatas { get; set; }
        public virtual DbSet<MonthlyBalancing> MonthlyBalancings { get; set; }
        public virtual DbSet<Parameter> Parameters { get; set; }
        public virtual DbSet<PastDue> PastDues { get; set; }
        public virtual DbSet<PaymentPlan> PaymentPlans { get; set; }
        public virtual DbSet<PaymentPlanDetail> PaymentPlanDetails { get; set; }
        public virtual DbSet<RouteType> RouteTypes { get; set; }
        public virtual DbSet<ServiceAddress> ServiceAddresses { get; set; }
        public virtual DbSet<ServiceAddressNote> ServiceAddressNotes { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransactionCode> TransactionCodes { get; set; }
        public virtual DbSet<TransactionCodeRule> TransactionCodeRules { get; set; }
        public virtual DbSet<TransactionHolding> TransactionHoldings { get; set; }
        public virtual DbSet<TransactionKanPayFee> TransactionKanPayFees { get; set; }
        public virtual DbSet<WarningLetter> WarningLetters { get; set; }
        public virtual DbSet<WorkOrder> WorkOrders { get; set; }
        public virtual DbSet<WorkOrderLegacy> WorkOrderLegacies { get; set; }

        public SwDbContext(DbContextOptions<SwDbContext> options) : base(options) { }
    }
}
