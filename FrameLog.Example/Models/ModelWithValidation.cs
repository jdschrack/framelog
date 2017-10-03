﻿using System.ComponentModel.DataAnnotations;

namespace FrameLog.Example.Models
{
    public class ModelWithValidation
    {
        public int Id { get; set; }

        public int Field { get; set; }

        // 5 digit pin code
        [Range(10000, 99999)]
        public int PinCode { get; set; }
    }
}
