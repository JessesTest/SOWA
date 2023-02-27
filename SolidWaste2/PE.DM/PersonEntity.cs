using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PE.DM
{
    [Table("PersonEntity")]
    public class PersonEntity
    {
        public PersonEntity()
        {
            Addresses = new HashSet<Address>();
            Emails = new HashSet<Email>();
            Phones = new HashSet<Phone>();
        }

        public int Id { get; set; }

        [Display(Name = "Department")]
        public int SystemCode { get; set; }

        [Display(Name = "Active")]
        public bool Status { get; set; }

        public bool NameTypeFlag { get; set; }

        [StringLength(255)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [StringLength(255)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(255)]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [StringLength(255)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Creation Date")]
        public DateTime WhenCreated { get; set; }

        [StringLength(255)]        
        [Display(Name = "Website")]
        public string WebUrl { get; set; }
        
        [Display(Name = "Paperless")]
        public bool? PaperLess { get; set; }
        
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(255)]
        public string Sex { get; set; }

        public bool? Pab { get; set; }

        [StringLength(255)]
        public string Comments { get; set; }

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

        public virtual Code Code { get; set; }

        public virtual ICollection<Email> Emails { get; set; }

        public virtual ICollection<Phone> Phones { get; set; }

        [StringLength(255)]
        public string Account { get; set; }




        public string FormatName()
        {
            StringBuilder sb = new();

            if (!string.IsNullOrWhiteSpace(FirstName))
                sb.Append(FirstName).Append(' ');

            if (!string.IsNullOrWhiteSpace(MiddleName))
                sb.Append(MiddleName).Append(' ');

            if (!string.IsNullOrWhiteSpace(LastName))
                sb.Append(LastName).Append(' ');

            return sb.ToString();
        }

        public string[] ParseFullName()
        {
            string name = FullName;
            string[] parsedName = new string[3];
            string[] splitComma = name.Split(',');

            // If name format is [Last, First Middle]
            if (splitComma.Length > 1)
            {
                parsedName[2] = splitComma[0];
                string[] splitName = splitComma[1].Trim().Split(' ');

                switch (splitName.Length)
                {
                    case 0:
                        break;
                    case 1:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        break;
                    case 2:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        break;
                    default:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        for (int i = 2; i < splitName.Length; i++)
                        {
                            parsedName[1] += splitName[i];
                        }
                        break;
                }
            }
            // If name format is [First Middle Last]
            else if (splitComma.Length == 1)
            {
                string[] splitName = name.Split(' ');

                switch (splitName.Length)
                {
                    case 0:
                        break;
                    case 1:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        parsedName[2] = string.Empty;
                        break;
                    case 2:
                        parsedName[0] = splitName[0];
                        parsedName[1] = string.Empty;
                        parsedName[2] = splitName[1];
                        break;
                    case 3:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        parsedName[2] = splitName[2];
                        break;
                    default:
                        parsedName[0] = splitName[0];
                        parsedName[1] = splitName[1];
                        parsedName[2] = splitName[2];
                        for (int i = 3; i < splitName.Length; i++)
                        {
                            parsedName[2] += splitName[i];
                        }
                        break;
                }
            }

            return parsedName;
        }

        public override bool Equals(object obj)
        {
            return obj is PersonEntity other &&
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
