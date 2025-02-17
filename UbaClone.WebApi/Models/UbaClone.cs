﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace UbaClone.WebApi.Models
{
    public class UbaClone
    {
        //change id type to guild
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = null!;
        public byte[] PasswordHash { get; set; } = null!;
        public byte[] PasswordSalt { get; set; } = null!;
        public byte[] PinHash { get; set; } = null!;
        public byte[] PinSalt { get; set; } = null!;
        public string Contact { get; set; } = string.Empty;
        public int AccountNumber { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [JsonIgnore]  // Prevent circular reference
        public virtual List<TransactionDetails> TransactionHistory { get; set; } = new List<TransactionDetails>();

    }
}
