using Domain.Models;
using Domain.Emails;
using Memento.Web.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/plans")]
    public class PlanController : BaseApiController
    {
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await PlanService.GetPremiumPlanByUserId(CurrentUserId));
        }

        [HttpGet]
        [Route("options")]
        public async Task<IActionResult> GetPlanOptions()
        {
            var plans = await PlanService.GetPremiumPlanByUserId(CurrentUserId);
            bool isPremium = plans.PlanType == PlanTypes.Premium;
            var expiration = isPremium ? plans.ExpirationDate : DateTime.UtcNow;
            return Ok(new PurchaseOptionModel
            {
                Name = isPremium ? "Extend Your Permium Plan" :
                    "Upgrade to a Premium Plan",
                Amount = Constants.PremiumPlanCost,
                ExpirationDate = expiration.AddYears(1)
            });
        }

        [HttpGet]
        [Route("sharedPlans")]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(await PlanService.GetSharedPlans(CurrentUserId));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddPremiumPlan(PurchasePlanModel purchase)
        {
            var transaction = await TransactionService.CreateStripeTransaction(Decimal.ToInt32(Constants.PremiumPlanCost) * 100, CurrentUserId, purchase.Token, "Purchase Premium Plan");
            var planUpgrade = await PlanService.AddPremiumPlan(CurrentUserId, PlanTypes.Premium, transaction.TransactionId, null, 6, null);
            var email = await UserService.GetEmail(CurrentUserId);
            SendEmailService.SendAsync(email, EmailTemplates.EmailTypes.Receipt, new { });
            SendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.PaymentNotice, new { Email = email });
            return Ok(planUpgrade);
        }

    }
}