
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameLog.Example.Models
{
    public class ModelWithPrimitiveKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Field { get; set; }
    }
}
