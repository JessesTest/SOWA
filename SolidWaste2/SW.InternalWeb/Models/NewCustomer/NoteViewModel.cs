using System.ComponentModel.DataAnnotations;

namespace SW.InternalWeb.Models.NewCustomer;

public class NoteViewModel
{
    public Guid Id { get; set; }
    public Guid ServiceAddressId { get; set; }

    [Required]
    [StringLength(1024)]
    public string Note { get; set; }

    public override bool Equals(object obj)
    {
        return obj is NoteViewModel other
            && other.Id == Id;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Id);
        return hash.ToHashCode();
    }
}

