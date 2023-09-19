using PE.DM;
using SW.DM;

namespace SW.BLL.DTOs
{
    public class CustomerDelinquency
    {
        public Customer Customer { get; set; }

        public PersonEntity PersonEntity { get; set; }

        public decimal PastDue { get; set; }

        public decimal PastDue30Days { get; set; }

        public decimal PastDue60Days { get; set; }

        public decimal PastDue90Days { get; set; }

        public decimal CollectionsBalance { get; set; }

        public decimal CounselorsBalance { get; set; }

        public bool IsDelinquent
        {
            get
            {
                return CollectionsBalance > 0 || CounselorsBalance > 0 || PastDue90Days > 0 || PastDue60Days > 0 || PastDue30Days > 0;
            }
        }
    }
}
