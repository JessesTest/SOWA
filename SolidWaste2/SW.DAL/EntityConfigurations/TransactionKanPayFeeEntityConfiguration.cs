using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SW.DM;

namespace SW.DAL.EntityConfigurations
{
    internal class TransactionKanPayFeeEntityConfiguration : IEntityTypeConfiguration<TransactionKanPayFee>
    {
        public void Configure(EntityTypeBuilder<TransactionKanPayFee> entity)
        {
            entity.Property(e => e.AchDate).HasMaxLength(255);

            entity.Property(e => e.Address1).HasMaxLength(255);

            entity.Property(e => e.Address2).HasMaxLength(255);

            entity.Property(e => e.AuthCode).HasMaxLength(255);

            entity.Property(e => e.AvsResponse).HasMaxLength(255);

            entity.Property(e => e.BillingName).HasMaxLength(255);

            entity.Property(e => e.City).HasMaxLength(255);

            entity.Property(e => e.Country).HasMaxLength(255);

            entity.Property(e => e.CreditCardType).HasMaxLength(255);

            entity.Property(e => e.CustomerId).HasMaxLength(255);

            entity.Property(e => e.CustomerType).HasMaxLength(255);

            entity.Property(e => e.CvvResponse).HasMaxLength(255);

            entity.Property(e => e.Email).HasMaxLength(255);

            entity.Property(e => e.Email1).HasMaxLength(255);

            entity.Property(e => e.Email2).HasMaxLength(255);

            entity.Property(e => e.Email3).HasMaxLength(255);

            entity.Property(e => e.ExpirationDate).HasMaxLength(255);

            entity.Property(e => e.FailCode).HasMaxLength(255);

            entity.Property(e => e.FailMessage).HasMaxLength(255);

            entity.Property(e => e.Fax).HasMaxLength(255);

            entity.Property(e => e.IfasObjectCode).HasMaxLength(255);

            entity.Property(e => e.Last4Number).HasMaxLength(255);

            entity.Property(e => e.LocalRefId).HasMaxLength(255);

            entity.Property(e => e.Name).HasMaxLength(255);

            entity.Property(e => e.OrderId).HasMaxLength(255);

            entity.Property(e => e.OrigCosAmount)
                .HasMaxLength(255)
                .HasColumnName("Orig_Cos_Amount");

            entity.Property(e => e.OrigFeeAmount)
                .HasMaxLength(255)
                .HasColumnName("Orig_Fee_Amount");

            entity.Property(e => e.OrigTransTotal)
                .HasMaxLength(255)
                .HasColumnName("Orig_Trans_Total");

            entity.Property(e => e.PayType).HasMaxLength(255);

            entity.Property(e => e.Phone).HasMaxLength(255);

            entity.Property(e => e.ReceiptDate).HasMaxLength(255);

            entity.Property(e => e.ReceiptTime).HasMaxLength(255);

            entity.Property(e => e.SncoFeeAmount).HasMaxLength(255);

            entity.Property(e => e.State).HasMaxLength(255);

            entity.Property(e => e.Token).HasMaxLength(255);

            entity.Property(e => e.TotalAmount).HasMaxLength(255);

            entity.Property(e => e.TransactionId).HasMaxLength(255);

            entity.Property(e => e.TransactionKanPayFeeAddDateTime).HasColumnType("datetime");

            entity.Property(e => e.TransactionKanPayFeeAddToi).HasMaxLength(255);

            entity.Property(e => e.TransactionKanPayFeeChgDateTime).HasColumnType("datetime");

            entity.Property(e => e.TransactionKanPayFeeChgToi).HasMaxLength(255);

            entity.Property(e => e.TransactionKanPayFeeDelDateTime).HasColumnType("datetime");

            entity.Property(e => e.TransactionKanPayFeeDelToi).HasMaxLength(255);

            entity.Property(e => e.Zip).HasMaxLength(255);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasKey(e => e.Id);
        }
    }
}
