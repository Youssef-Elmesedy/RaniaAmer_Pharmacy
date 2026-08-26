namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// A single product line inside an Offer, with its special bundle price
public class OfferItem : BaseEntity
{
    public Guid OfferId { get; private set; }

    public Offer? Offer { get; private set; }

    public Guid ProductId { get; private set; }

    public Product? Product { get; private set; }

    public decimal SpecialPrice { get; private set; }

    private OfferItem()
    {
    }

    internal static OfferItem Create(
    Offer offer,
    Guid productId,
    decimal specialPrice)
    {
        return new OfferItem
        {
            Offer = offer,
            OfferId = offer.Id,
            ProductId = productId,
            SpecialPrice = specialPrice,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdatePrice(decimal specialPrice)
    {
        SpecialPrice = specialPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
