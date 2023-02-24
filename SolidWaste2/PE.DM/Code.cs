using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PE.DM
{
    [Table("Codes")]
    public class Code
    {
        public Code()
        {
            Addresses = new HashSet<Address>();
            Emails = new HashSet<Email>();
            PersonEntities = new HashSet<PersonEntity>();
            Phones = new HashSet<Phone>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Type { get; set; }

        [Column("Code")]
        [StringLength(255)]
        public string Code1 { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

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

        public virtual ICollection<Address> Addresses { get; set; }

        public virtual ICollection<Email> Emails { get; set; }

        public virtual ICollection<PersonEntity> PersonEntities { get; set; }

        public virtual ICollection<Phone> Phones { get; set; }


        public override bool Equals(object obj)
        {
            return obj is Code other &&
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
