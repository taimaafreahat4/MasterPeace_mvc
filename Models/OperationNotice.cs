namespace StockMind.Models;

public static class OperationNotice
{
    public static string? Text(string code) => code switch
    {
        "product-submitted" => "Product saved and submitted for approval.",
        "plan-scheduled" => "There is already a scheduled subscription.",
        "plan-requested" => "Subscription requested. Contact support to arrange payment.",
        "analysis-saved" => "Purchase analysis saved.",
        "profile-saved" => "Profile saved.",
        "feedback-submitted" => "Thanks for sharing your feedback.",
        "ticket-submitted" => "Your support ticket was submitted.",
        "quantity-unavailable" => "That quantity is unavailable. Check the minimum and available stock.",
        "stock-changed" => "Stock changed. Update your cart.",
        "order-total-limit" => "This order exceeds the supported total. Split it into smaller orders.",
        "seller-pending" => "Approve the seller before publishing their product.",
        _ => null
    };
}
