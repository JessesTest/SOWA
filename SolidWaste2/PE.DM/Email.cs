using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PE.DM
{
    [Table("Email")]
    public class Email
    {
        public int Id { get; set; }

        public int Type { get; set; }

        public int PersonEntityID { get; set; }

        [Column("Email")]
        [Required]
        [StringLength(255)]
        [Display(Name = "Email")]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessage = "Invalid Email")]
        public string Email1 { get; set; }

        public bool Status { get; set; }

        public bool Delete { get; set; }

        public DateTime? AddDateTime { get; set; }

        [StringLength(50)]
        public string AddToi { get; set; }

        public DateTime? ChgDateTime { get; set; }

        [StringLength(50)]
        public string ChgToi { get; set; }

        public DateTime? DelDateTime { get; set; }

        [StringLength(50)]
        public string DelToi { get; set; }

        public virtual Code Code { get; set; }

        public virtual PersonEntity PersonEntity { get; set; }

        public bool IsDefault { get; set; }


        public override bool Equals(object obj)
        {
            return obj is Email other &&
                Id > 0 &&
                Id == other.Id;
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(Id);
            return hash.ToHashCode();
        }
    }
}
