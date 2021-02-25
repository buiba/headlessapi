using EPiServer.ContentApi.Core.ContentResult.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.ContentApi.Commerce.Internal.Infrastructure
{
    public static class ApiErrors
    {
        public static Error CartIdMissing => new Error(ErrorCode.InvalidParameter, "Cart id is missing");

        public static Error CartNotFound => new Error(ErrorCode.NotFound, "Cart not found");

        public static Error InvalidCart => new Error(ErrorCode.InvalidParameter, "Invalid Cart");

        public static Error InvalidMarketId => new Error(ErrorCode.InvalidParameter, "Invalid market id");

        public static Error MarketNotFound => new Error(ErrorCode.NotFound, "Market not found");

        public static Error InvalidHeaderValue => new Error(ErrorCode.InvalidHeaderValue, ErrorMessage.InvalidHeaderValue);

        public static Error Forbidden => new Error(ErrorCode.Forbidden, ErrorMessage.Forbidden);

        public static Error NotFound => new Error(ErrorCode.NotFound, ErrorMessage.NotFound);

        public static Error InternalServerError => new Error(ErrorCode.InternalServerError, ErrorMessage.InternalServerError);
        
        public static Error OrderNotFound => new Error(ErrorCode.NotFound, "Order not found");

        public static Error OrderNumberMissing => new Error(ErrorCode.InvalidParameter, "Order number is missing");

        public static Error InvalidOrder => new Error(ErrorCode.InvalidParameter, "Invalid Order");

        public static Error LineItemMultipleValidationIssue => new Error("MultipleValidationIssues", "Multiple validation issues for this item, please check details for more information");

        public static Error OrderValidationFailed => new Error("OrderValidationFailed", "Order validation failed, please check details for more information on reasons.");

        public static Error InvalidCouponCode => new Error("InvalidCouponCode", "Invalid coupon code");
    }
}
