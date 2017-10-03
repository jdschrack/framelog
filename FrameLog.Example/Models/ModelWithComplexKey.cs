
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrameLog.Example.Models
{
    public class ModelWithComplexKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public int Field { get; set; }
    }
}
