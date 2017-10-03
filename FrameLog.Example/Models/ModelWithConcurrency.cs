﻿using System.ComponentModel.DataAnnotations;

namespace FrameLog.Example.Models
{
    public class ModelWithConcurrency
    {
        public int Id { get; set; }

        public int Field { get; set; }

        [Timestamp]
        [ConcurrencyCheck]
        public byte[] Lock { get; set; }
    }
}
