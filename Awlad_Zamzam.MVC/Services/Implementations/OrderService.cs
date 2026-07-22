using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Models.Enums;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class OrderService : IOrderService
{
    private const string PendingCountCacheKey = "orders:pending-count";

    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartService _cartService;
    private readonly IMemoryCache _cache;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ICartService cartService,
        IMemoryCache cache)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _cartService = cartService;
        _cache = cache;
    }

    public async Task<Guid> CreateFromCartAsync(CheckoutViewModel model, Guid? authenticatedCustomerId = null)
    {
        var cart = await _cartService.GetCartAsync();

        if (!cart.Items.Any())
            throw new BusinessException("السلة فارغة", nameof(model));

        Customer customer;

        if (authenticatedCustomerId.HasValue)
        {
            // Logged-in customer: name/phone are already on file, address can optionally be updated
            customer = await _customerRepository.GetByIdAsync(authenticatedCustomerId.Value)
                ?? throw new BusinessException("تعذر العثور على بيانات حسابك", nameof(authenticatedCustomerId));

            if (!string.IsNullOrWhiteSpace(model.Address) && model.Address.Trim() != customer.Address)
            {
                customer.Update(customer.Name, customer.PhoneNumber, model.Address);
                await _customerRepository.UpdateAsync(customer);
                await _customerRepository.SaveChangesAsync();
            }
        }
        else
        {
            // Guest checkout: name/phone/address are required
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Address))
                throw new BusinessException("الاسم ورقم الهاتف والعنوان مطلوبة", nameof(model));

            var existing = await _customerRepository.GetByPhoneAsync(model.PhoneNumber.Trim());

            if (existing == null)
            {
                customer = Customer.Create(model.Name, model.PhoneNumber, model.Address);
                await _customerRepository.AddAsync(customer);
            }
            else
            {
                existing.Update(model.Name, model.PhoneNumber, model.Address);
                customer = existing;
                await _customerRepository.UpdateAsync(customer);
            }

            await _customerRepository.SaveChangesAsync();
        }

        // Cash/credit is decided later by the admin at delivery time, not by the customer
        var order = Order.Create(customer.Id, model.Notes);

        foreach (var item in cart.Items)
            order.AddItem(item.ProductId, item.ProductName, item.UnitPrice, item.SaleUnit, item.Quantity, item.Note);

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        _cartService.Clear();
        InvalidatePendingCountCache();

        return order.Id;
    }

    // The admin logs a credit ("آجل") sale directly against a customer - e.g. a walk-in customer
    // who took goods on credit in person. Recorded immediately as Completed + IsCredit = true.
    public async Task<Guid> CreateCreditOrderByAdminAsync(CreditOrderFormViewModel model)
    {
        Customer customer;

        if (model.CustomerId.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(model.CustomerId.Value)
                ?? throw new BusinessException("العميل غير موجود", nameof(model.CustomerId));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.NewCustomerName) ||
                string.IsNullOrWhiteSpace(model.NewCustomerPhone) ||
                string.IsNullOrWhiteSpace(model.NewCustomerAddress))
                throw new BusinessException("اختر عميل موجود أو أدخل بيانات عميل جديد كاملة", nameof(model));

            var existing = await _customerRepository.GetByPhoneAsync(model.NewCustomerPhone.Trim());

            if (existing != null)
            {
                customer = existing;
            }
            else
            {
                customer = Customer.Create(model.NewCustomerName, model.NewCustomerPhone, model.NewCustomerAddress);
                await _customerRepository.AddAsync(customer);
                await _customerRepository.SaveChangesAsync();
            }
        }

        var selectedItems = model.SelectedProductIds
            .Where(id => model.Quantities.GetValueOrDefault(id, 0) > 0)
            .ToList();

        if (selectedItems.Count == 0)
            throw new BusinessException("اختر منتج واحد على الأقل بكمية أكبر من صفر", nameof(model.SelectedProductIds));

        var order = Order.Create(customer.Id, model.Notes);

        foreach (var productId in selectedItems)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) continue;

            var quantity = model.Quantities[productId];
            order.AddItem(product.Id, product.Name, product.Price, product.SaleUnit, quantity, null);
        }

        // This is a direct admin entry of a credit sale, so it's recorded as already delivered on credit
        order.Complete(isCredit: true);

        await _orderRepository.AddAsync(order);
        await _orderRepository.SaveChangesAsync();

        return order.Id;
    }

    public async Task<List<OrderListItemViewModel>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailsAsync();
        return orders.Select(MapToListItem).ToList();
    }

    public async Task<List<OrderListItemViewModel>> GetByCustomerAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders.Select(MapToListItem).ToList();
    }

    public async Task<List<OrderListItemViewModel>> GetCreditOrdersByCustomerAsync(Guid customerId)
    {
        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
        return orders
            .Where(o => o.IsCredit)
            .Select(MapToListItem)
            .ToList();
    }

    public async Task<OrderDetailsViewModel?> GetDetailsAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        if (order == null) return null;

        return new OrderDetailsViewModel
        {
            Id = order.Id,
            CustomerName = order.Customer?.Name ?? string.Empty,
            CustomerPhone = order.Customer?.PhoneNumber ?? string.Empty,
            CustomerAddress = order.Customer?.Address ?? string.Empty,
            IsCredit = order.IsCredit,
            Notes = order.Notes,
            Status = order.Status,
            OrderDate = order.OrderDate,
            Items = order.Items.Select(i => new OrderItemViewModel
            {
                ProductName = i.ProductName,
                UnitPrice = i.UnitPrice,
                SaleUnit = i.SaleUnit,
                Quantity = i.Quantity,
                Note = i.Note
            }).ToList(),
            Payments = order.Payments.Select(p => new OrderPaymentViewModel
            {
                Amount = p.Amount,
                Notes = p.Notes,
                PaidAt = p.PaidAt
            }).OrderByDescending(p => p.PaidAt).ToList()
        };
    }

    // Only the admin calls this, after physically handing over the order, choosing cash or credit
    public async Task CompleteAsync(Guid id, bool isCredit)
    {
        // Must load Items since Order.Complete() / auto-payment total calc reads them
        var order = await _orderRepository.GetByIdWithDetailsAsync(id)
            ?? throw new BusinessException("الطلب غير موجود", nameof(id));

        order.Complete(isCredit);

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        // A cash order is settled in full immediately - recorded as an isolated payment insert
        if (!isCredit && order.Total > 0)
        {
            var cashPayment = OrderPayment.Create(order.Id, order.Total, "دفع كاش عند التسليم");
            await _orderRepository.AddPaymentAsync(cashPayment);
            await _orderRepository.SaveChangesAsync();
        }

        InvalidatePendingCountCache();
    }

    public async Task AddPaymentAsync(Guid orderId, decimal amount, string? notes)
    {
        // Read-only: used only to validate the amount against the order's current state
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId)
            ?? throw new BusinessException("الطلب غير موجود", nameof(orderId));

        order.EnsureCanAcceptPayment(amount);

        var payment = OrderPayment.Create(orderId, amount, notes);

        await _orderRepository.AddPaymentAsync(payment);
        await _orderRepository.SaveChangesAsync();
    }

    // Pays down a customer's total credit balance in one go: the amount is allocated across
    // their unpaid credit orders oldest-first (FIFO), rather than paying a single order at a time.
    public async Task PayCustomerCreditAsync(Guid customerId, decimal amount, string? notes)
    {
        if (amount <= 0)
            throw new BusinessException("المبلغ يجب أن يكون أكبر من صفر.", nameof(amount));

        var orders = (await _orderRepository.GetByCustomerIdAsync(customerId))
            .Where(o => o.IsCredit && !o.IsFullyPaid)
            .OrderBy(o => o.OrderDate)
            .ToList();

        if (orders.Count == 0)
            throw new BusinessException("لا يوجد على هذا العميل مبالغ آجلة مستحقة.", nameof(customerId));

        var totalDue = orders.Sum(o => o.RemainingBalance);

        if (amount > totalDue)
            throw new BusinessException($"المبلغ أكبر من إجمالي المستحق ({totalDue:0.00} ج.م).", nameof(amount));

        var remainingToAllocate = amount;
        var payments = new List<OrderPayment>();

        foreach (var order in orders)
        {
            if (remainingToAllocate <= 0) break;

            var amountForThisOrder = Math.Min(order.RemainingBalance, remainingToAllocate);

            payments.Add(OrderPayment.Create(order.Id, amountForThisOrder, notes));
            remainingToAllocate -= amountForThisOrder;
        }

        foreach (var payment in payments)
            await _orderRepository.AddPaymentAsync(payment);

        await _orderRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id)
            ?? throw new BusinessException("الطلب غير موجود", nameof(id));

        if (order.IsCredit && !order.IsFullyPaid)
            throw new BusinessException("لا يمكن حذف طلب آجل لم يتم سداده بالكامل", nameof(id));

        await _orderRepository.DeleteAsync(order);
        await _orderRepository.SaveChangesAsync();

        InvalidatePendingCountCache();
    }

    // Cached: read on every admin page load (sidebar notification badge)
    public async Task<int> GetPendingCountAsync()
    {
        if (_cache.TryGetValue(PendingCountCacheKey, out int cachedCount))
            return cachedCount;

        var count = await _orderRepository.CountPendingAsync();
        _cache.Set(PendingCountCacheKey, count, TimeSpan.FromMinutes(2));

        return count;
    }

    private void InvalidatePendingCountCache() => _cache.Remove(PendingCountCacheKey);

    private static OrderListItemViewModel MapToListItem(Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        CustomerName = o.Customer?.Name ?? string.Empty,
        CustomerPhone = o.Customer?.PhoneNumber ?? string.Empty,
        IsCredit = o.IsCredit,
        Status = o.Status,
        OrderDate = o.OrderDate,
        Total = o.Total,
        AmountPaid = o.AmountPaid,
        ItemsCount = o.Items.Count
    };
}
