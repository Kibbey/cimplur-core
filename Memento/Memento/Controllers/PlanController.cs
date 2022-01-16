using Domain.Models;
using Domain.Emails;
using Memento.Web.Models;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Memento.Libs;
using Domain.Repository;

namespace Memento.Web.Controllers
{
    [CustomAuthorization]
    [Route("api/plans")]
    public class PlanController : BaseApiController
    {
        private PlanService planService;
        private TransactionService transactionService;
        private SendEmailService sendEmailService;
        private UserService userService;

        public PlanController(PlanService planService, 
            TransactionService transactionService,
            SendEmailService sendEmailService,
            UserService userService) {
            this.planService = planService;
            this.transactionService = transactionService;
            this.sendEmailService = sendEmailService;
            this.userService = userService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await planService.GetPremiumPlanByUserId(CurrentUserId));
        }

        [HttpGet]
        [Route("options")]
        public async Task<IActionResult> GetPlanOptions()
        {
            var plans = await planService.GetPremiumPlanByUserId(CurrentUserId);
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
            return Ok(await planService.GetSharedPlans(CurrentUserId));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddPremiumPlan(PurchasePlanModel purchase)
        {
            var transaction = await transactionService.CreateStripeTransaction(Decimal.ToInt32(Constants.PremiumPlanCost) * 100, CurrentUserId, purchase.Token, "Purchase Premium Plan");
            var planUpgrade = await planService.AddPremiumPlan(CurrentUserId, PlanTypes.Premium, transaction.TransactionId, null, 6, null);
            var email = await userService.GetEmail(CurrentUserId);
            sendEmailService.SendAsync(email, EmailTemplates.EmailTypes.Receipt, new { });
            sendEmailService.SendAsync(Constants.Email, EmailTemplates.EmailTypes.PaymentNotice, new { Email = email });
            return Ok(planUpgrade);
        }

    }
}