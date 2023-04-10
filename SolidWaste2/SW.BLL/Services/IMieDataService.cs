using SW.DM;

namespace SW.BLL.Services
{
    public interface IMieDataService
    {
        Task<ICollection<MieDataInfo>> Get(string miedataImageID, bool miedataActive);
        Task<MieData> GetById(int mieDataId);
        Task Add(MieData mieData);
        Task Update(MieData mieData);
        Task Delete(int id);
    }

    // smae as MieData but without MieDataImage
    public class MieDataInfo
    {
        public int MieDataId { get; set; }
        public string MieDataImageId { get; set; }
        public string MieDataImageType { get; set; }
        public int? MieDataImageSize { get; set; }
        public string MieDataImageFileName { get; set; }
        public bool MieDataActive { get; set; }
        public bool MieDataDelete { get; set; }
        public string AddToi { get; set; }
        public DateTime AddDateTime { get; set; }
        public string ChgToi { get; set; }
        public DateTime? ChgDateTime { get; set; }
        public string DelToi { get; set; }
        public DateTime? DelDateTime { get; set; }
    }
}
