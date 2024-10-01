using Azure;
using Azure.Data.Tables;
using System;

namespace Retail.Models
{
    public class CustomerProfile : ITableEntity
    {
    
        public string PartitionKey { get; set; }

        
        public string RowKey { get; set; }

        
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Parameterless constructor
        public CustomerProfile()
        {
        }

        
        public CustomerProfile(string partitionKey, string rowKey, string name, string lastName, string email, string phoneNumber)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Name = name;
            LastName = lastName;
            Email = email;
            PhoneNumber = phoneNumber;
        }
    }
}
