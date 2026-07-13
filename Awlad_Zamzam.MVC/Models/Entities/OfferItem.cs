namespace Awlad_Zamzam.MVC.Models.Entities;

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

    internal static OfferItem Create(Guid offerId, Guid productId, decimal specialPrice) => new()
    {
        OfferId = offerId,
        ProductId = productId,
        SpecialPrice = specialPrice,
        CreatedAt = DateTime.UtcNow
    };
}
