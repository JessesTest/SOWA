using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SW.DM;

public class ServiceAddressNote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int ServiceAddressId { get; set; }
    [ForeignKey("ServiceAddressId")]
    public ServiceAddress ServiceAddress { get; set; }

    [StringLength(1024)]
    public string Note { get; set; }

    public bool DeleteFlag { get; set; }

    public DateTime AddDateTime { get; set; }

    [StringLength(255)]
    public string AddToi { get; set; }

    public DateTime? ChgDateTime { get; set; }

    [StringLength(255)]
    public string ChgToi { get; set; }

    public DateTime? DelDateTime { get; set; }

    [StringLength(255)]
    public string DelToi { get; set; }
}
