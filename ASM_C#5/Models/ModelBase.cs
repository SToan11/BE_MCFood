using System.ComponentModel.DataAnnotations;

namespace ASM_C_5.Models
{
    public class ModelBase
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; } = DateTime.Now;
        public string? UpdatedBy { get; set; }
    }
}
