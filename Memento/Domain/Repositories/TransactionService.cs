using Domain.Models;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Configuration;
using log4net;

namespace Domain.Repository
{

    public class TransactionService : BaseService
    {
        private string key = ConfigurationManager.AppSettings["Charge"] ?? "";
        private ILog logger = LogManager.GetLogger(nameof(TransactionService));

        public async Task<TransactionModel> GetTransaction(int transactionId, int userId) {
            var transaction = await Context.Transactions.SingleOrDefaultAsync(x => x.TransactionId == transactionId && x.UserId == userId);
            if (transaction == null) throw new NotFoundException(string.Format("TransactionId {0} does not exist.",transactionId));
            return TransactionModelFromTransaction(transaction);
        }

        public async Task<List<TransactionModel>> GetTransactions(int userId) {
            var transactions = await Context.Transactions.Where(x => x.UserId == userId).ToListAsync();
            return transactions.Select(TransactionModelFromTransaction).ToList();
        }

        public async Task<TransactionModel> CreateStripeTransaction(int amountCents, int userId, string token, string description) {
            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.ApiKey = key;

            var options = new ChargeCreateOptions
            {
                Amount = amountCents,
                Currency = "usd",
                Description = "Fyli - " + description,
                Source = token,
            };
            var service = new ChargeService();
            try
            {
                Charge charge;
                try {
                   charge = await service.CreateAsync(options);
                } catch (Exception e) {
                    throw new BadRequestException(e.Message);
                }
                return await CreateTransaction(amountCents, userId, token, description);
            }
            catch (Exception e) {
                if (!(e is BadRequestException)) {
                    logger.Error("HIGH IMPORTANCE ERROR - ALERT!", e);
                }
                throw e;
            }
        }

        public async Task<TransactionModel> CreateTransaction(int amountCents, int userId, string externalChargeId, string description) {
            var transaction = new Transaction {
                UserId = userId,
                AmountCents = amountCents,
                ChargeId = externalChargeId,
                Created = DateTime.UtcNow,
                Description = description
            };
            Context.Transactions.Add(transaction);
            await Context.SaveChangesAsync();
            return new TransactionModel {
                Amount = CentsToDollars(transaction.AmountCents),
                TransactionId = transaction.TransactionId,
                Created = transaction.Created,
                Description = transaction.Description
            };
        }

        private decimal CentsToDollars(int cents) {
            return Decimal.Parse(cents.ToString()) / 100;
        }

        private TransactionModel TransactionModelFromTransaction(Transaction transaction) {
            return new TransactionModel
            {
                TransactionId = transaction.TransactionId,
                Amount = CentsToDollars(transaction.AmountCents),
                Created = transaction.Created,
                Description = transaction.Description
            };
        }

    }
}
