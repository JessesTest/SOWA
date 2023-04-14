using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.MieData;

public class MieDataListViewModel
{
    //public List<MieDataViewModel> ListImages { get; set; }
    [Required]
    public int? CustomerId { get; set; }
}
