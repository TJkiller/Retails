using Azure;
using Azure.Data.Tables;

namespace Retail.Models
{
    public class Product : ITableEntity
    {
        
        public string PartitionKey { get; set; }

        
        public string RowKey { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

       
        public DateTimeOffset? Timestamp { get; set; } 
        public ETag ETag { get; set; }

        
        public Product()
        {
        }

        
        public Product(string partitionKey, string rowKey, string name, string description, decimal price, int stockQuantity)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
        }
    }
}
