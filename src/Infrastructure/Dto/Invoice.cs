using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.Infrastructure.Dto;
public class Invoice
{
    public int Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTimeOffset OrderedDate { get; set; }
    public decimal Total { get; set; }
    public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ShippingAddress ShippingAddress { get; set; } = new ShippingAddress();
}

public class InvoiceItem { 
public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Unit { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ShippingAddress
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

}
