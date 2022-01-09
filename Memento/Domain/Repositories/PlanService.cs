using Domain.Models;
using Domain.Entities;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Domain.Exceptions;
using System.Collections.Generic;

namespace Domain.Repository
{
    public class PlanService : BaseService
    {
        private int planLengthDays = 366; // give them an extra day

        public async Task<PlanModel> GetPremiumPlanByUserId(int userId) {
            var now = DateTime.UtcNow;
            var plan = await Context.PremiumPlans.Where(x => x.UserId == userId && x.ExpirationDate >= now)
                .OrderByDescending(o => o.ExpirationDate).FirstOrDefaultAsync();
            // no plan just return a standard plan
            if (plan == null) {
                return new PlanModel
                {
                    UserId = userId,
                    PlanType = PlanTypes.Standard
                };
            }
            // Premium plans 
            return new PlanModel {
                UserId = userId,
                PlanId = plan.PremiumPlanId,
                PlanType = plan.PlanType,
                ExpirationDate = plan.ExpirationDate
            };
        }

        public async Task<PlanModel> GetPremiumPlanById(int planId)
        {
            var plan = await Context.PremiumPlans.SingleOrDefaultAsync(x => x.PremiumPlanId == planId);
            // no plan just return a standard plan
            if (plan == null)
            {
                throw new NotFoundException();
            }
            // Premium plans 
            return new PlanModel
            {
                UserId = plan.UserId,
                PlanId = plan.PremiumPlanId,
                PlanType = plan.PlanType,
                ExpirationDate = plan.ExpirationDate
            };
        }

        public async Task<PlanModel> AddPremiumPlan(int userId, PlanTypes planType, int? transactionId, int? sharePlanId, int? familyCount, int? parentPremiumPlanId) {
            var now = DateTime.UtcNow;
            var plan = await Context.PremiumPlans.Where(x => x.UserId == userId && x.ExpirationDate >= now)
                .OrderByDescending(o => o.ExpirationDate).FirstOrDefaultAsync();
            DateTime expiration = DateTime.UtcNow.AddDays(planLengthDays);
            if (plan != null){
                familyCount = Math.Max(familyCount ?? 0, plan.FamilyPlanCount);
                // don't down grade them if they already have a friends and family
                planType = PlanTypes.PremiumFriends == plan.PlanType ? PlanTypes.PremiumFriends : planType;
            }
            if (parentPremiumPlanId.HasValue) {
                var rootPlan = await Context.PremiumPlans.SingleOrDefaultAsync(x => x.PremiumPlanId == parentPremiumPlanId);
                if (rootPlan.ExpirationDate < now) throw new NotAuthorizedException("Plan is expired.");
                // set the expiration date to the later of the current plan they are being added to or the root plan.
                expiration = new DateTime(Math.Max(rootPlan.ExpirationDate.Ticks, plan?.ExpirationDate.Ticks ?? DateTime.MinValue.Ticks));
            }

            var newPlan = new PremiumPlan {
                Created = now,
                ExpirationDate = expiration,
                PlanLengthDays = planLengthDays,
                TransactionId = transactionId,
                UserId = userId,
                PlanType = planType,
                ExtendedPlanId = plan?.PremiumPlanId,
                ParentPremiumPlanId = parentPremiumPlanId,
                FamilyPlanCount = familyCount ?? 0
            };

            if (sharePlanId.HasValue) {
                var sharedPlan = await Context.SharedPlans.SingleAsync(x => x.SharedPlanId == sharePlanId.Value);
                sharedPlan.SharedPremiumPlanId = newPlan.PremiumPlanId;
            }

            Context.PremiumPlans.Add(newPlan);
            var userProfile = await Context.UserProfiles.SingleAsync(x => x.UserId == userId);
            userProfile.PremiumExpiration = expiration; // this needs to stay in sync - used to allow sharing
            await Context.SaveChangesAsync();

            return new PlanModel {
                ExpirationDate = newPlan.ExpirationDate,
                UserId = userId,
                PlanType = planType
            };
        }

        public async Task<List<SharedPlanModel>> GetSharedPlans(int userId) {
            return await Context.SharedPlans.Where(x => x.UserId == userId).Select(s =>
                new SharedPlanModel {
                    SharedWith = s.Name,
                    Modified = s.Modified,
                    Pending = !s.SharedPremiumPlanId.HasValue,
                    Revoked = s.Revoked
                }
            ).ToListAsync();
        }

        public async Task AddSharedPlans(int userId, int numberToAdd, int? transactionId) {
            //if it exists extend it
            var now = DateTime.UtcNow;
            var plan = await Context.PremiumPlans.Where(x => x.UserId == userId && x.ExpirationDate >= now)
                .OrderByDescending(o => o.ExpirationDate).FirstOrDefaultAsync();
            // no plan just return a standard plan
            if (plan == null) throw new NotFoundException("You must have a premium plan to do this.");
            var sharedPlans = CreateSharedPlans(userId, numberToAdd, transactionId);
            Context.SharedPlans.AddRange(sharedPlans);
            await Context.SaveChangesAsync();
        }

        public async Task<int> GetAvaliableFamilyPlanCount(int userId) {
            var now = DateTime.UtcNow;
            var plan = await Context.PremiumPlans.Where(x => x.UserId == userId && x.ExpirationDate >= now)
                .OrderByDescending(o => o.ExpirationDate).FirstOrDefaultAsync();
            if (plan == null) return 0;
            var sharedPlans = await Context.ShareRequests.Where(x => x.RequesterUserId == userId &&
                x.PremiumPlanId == plan.PremiumPlanId &&
                !x.Ignored
                ).ToListAsync();
            return plan.FamilyPlanCount - sharedPlans.Count;
        }

        public async Task<int> GetClaimableFamilyPlanCount(int planId)
        {
            var now = DateTime.UtcNow;
            var plan = await Context.PremiumPlans
                .Where(x => x.PremiumPlanId == planId && x.ExpirationDate >= now)
                .SingleOrDefaultAsync();
            if (plan == null) return 0;
            var sharedPlans = await Context.PremiumPlans
                .Where(x => x.ParentPremiumPlanId == plan.PremiumPlanId).ToListAsync();
            return plan.FamilyPlanCount - sharedPlans.Count;
        }

        public async Task<SharedPlanModel> AssignSharedPlan(int userId, string name, int? contactId, string email) {
            // if contactId is not null then do stuff
            var sharedPlan = Context.SharedPlans.FirstOrDefault(x => x.UserId == userId
                && !x.Revoked
                && x.EmailSentTo == null
                && !x.SharedPremiumPlanId.HasValue);
            if (sharedPlan == null) {
                throw new ConflictException("No shared plans available.");
            }
            sharedPlan.Name = name;
            bool pending = true;
            if (contactId.HasValue)
            {
                await AddPremiumPlan(contactId.Value, PlanTypes.Premium, null, sharedPlan.SharedPlanId, 0, sharedPlan.SharedPremiumPlanId);
                pending = false;
            } else {
                sharedPlan.EmailSentTo = email;
            }
            await Context.SaveChangesAsync();

            return new SharedPlanModel {
                SharedWith = name,
                Modified = sharedPlan.Modified,
                Pending = pending
            };
        }

        public async Task<bool> FindAndAcceptSharedPlan(int userId, string email)
        {
            var sharedPlan = Context.SharedPlans.FirstOrDefault(x => x.EmailSentTo == email
                && !x.Revoked
                && !x.SharedPremiumPlanId.HasValue);
            if (sharedPlan != null)
            {
                await AddPremiumPlan(userId, PlanTypes.Premium, null, sharedPlan.SharedPlanId, 2, null);
                return true;
            }
            return false;
        }

        public async Task RevokeSharedPlan(int userId, string email)
        {
            var sharedPlan = Context.SharedPlans.FirstOrDefault(x => x.EmailSentTo == email
                && !x.Revoked
                && !x.SharedPremiumPlanId.HasValue);
            if (sharedPlan == null)
            {
                throw new ConflictException("No shared plans available to revoke.");
            }
            sharedPlan.Revoked = true;
            await Context.SaveChangesAsync();
            // Remove and then re-add a new one
            await AddSharedPlans(userId, 1, sharedPlan.TransactionId);
        }

        private List<SharedPlan> CreateSharedPlans(int userId, int number, int? transactionId)
        {
            var sharedPlans = new List<SharedPlan>();
            var now = DateTime.UtcNow;
            for (var i = 0; i < number; i++)
            {
                sharedPlans.Add(new SharedPlan
                {
                    UserId = userId,
                    TransactionId = transactionId,
                    Created = now
                });
            }
            return sharedPlans;
        }
    }
}
