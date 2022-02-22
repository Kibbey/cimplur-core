using Domain.Models;
using Domain.Emails;
using Domain.Entities;
using Domain.Exceptions;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static Domain.Emails.EmailTemplates;

namespace Domain.Repository
{
    public class PromptService : BaseService
    {
        private SendEmailService sendEmailService;
        private TimelineService timelineService;
        private DropsService dropService;
        private UserService userService;
        
        public PromptService(SendEmailService sendEmailService,
            TimelineService timelineService,
            DropsService dropsService,
            UserService userService
            ) {
            this.sendEmailService = sendEmailService;
            this.timelineService = timelineService;
            this.dropService = dropsService;
            this.userService = userService;
        }

        public async Task<List<PromptModel>> GetActivePrompts(int currentUserId)
        {
            await SyncUserPrompts(currentUserId);
            var prompts = await Context.UserPrompts.Include(i => i.Prompt).Include(i => i.Askers)
                .Where(x => x.UserId == currentUserId && !x.Dismissed.HasValue && !x.Used.HasValue && !x.Prompt.Template)
                .OrderByDescending(o => o.Prompt.Order)
                .ToListAsync();
            prompts = await RelationshipPromptsToTop(prompts, currentUserId); 
            var nameDictionary = await userService.GetNameDictionary(currentUserId);
            return ShuffleTop(prompts.Select(s => MapPromptModel(s, nameDictionary)).ToList());
            
        }

        public async Task<List<PromptModel>> GetPromptsAskedToMe(int currentUserId)
        {
            var prompts = await Context.UserPrompts.Include(i => i.Prompt).Include(i => i.Askers)
                .Where(x => x.UserId == currentUserId 
                    && !x.Dismissed.HasValue 
                    && !x.Used.HasValue 
                    && !x.Prompt.Template)
                .Where(x => x.Askers.Any())
                .OrderByDescending(o => o.UserPromptId)
                .ToListAsync();
            var nameDictionary = await userService.GetNameDictionary(currentUserId);
            return prompts.Select(s => MapPromptModel(s, nameDictionary)).ToList();
            
        }

        public async Task<List<PromptAskedModel>> GetPromptsAskedByMe(int currentUserId)
        {
            var prompts = await Context.Prompts
                .Include(i => i.UserPrompts)
                .Include(i => i.Drops)
                .Where(x => x.UserId == currentUserId
                    && !x.Template)
                .OrderByDescending(o => o.PromptId)
                .ToListAsync();

            var nameDictionary = await userService.GetNameDictionary(currentUserId);

            return prompts.Select(s => new PromptAskedModel
            {
                Question = s.Question,
                PromptId = s.PromptId,
                Custom = s.UserId.HasValue,
                Askeds = s.UserPrompts
                        .Select(up => new Asked
                        {
                            Id = up.UserId,
                            Name = nameDictionary.ContainsKey(up.UserId) ? nameDictionary[up.UserId] : "Unavailable",
                            DropId = s.Drops.FirstOrDefault(d => d.UserId == up.UserId)?.DropId ?? 0,
                            Connection = true
                        })
            }).ToList();
            
        }

        /// <summary>
        /// This is to get a prompt that you have asked others
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="promptId"></param>
        /// <returns></returns>
        public async Task<PromptAskedModel> GetAskedPrompt(int currentUserId, int promptId)
        {
            var nameDictinoary = await userService.GetNameDictionary(currentUserId);
            var prompt = await Context.Prompts
                .Where(p => p.PromptId == promptId && (p.UserId == currentUserId || p.UserId == null))
                .SingleOrDefaultAsync();
            if (prompt == null) throw new Exceptions.NotFoundException();
            var dropsDictionary = dropService.CanViewPrompts(currentUserId, promptId)
                    .Where(x => nameDictinoary.ContainsKey(x.OwnerId))
                    .ToDictionary(k => k.OwnerId, v => v.DropId);

            var asked = await Context.UserPromptAskers.Where(x => x.AskerId == currentUserId && x.UserPrompt.PromptId == promptId)
                .Select(s => s.UserPrompt)
                .ToListAsync();

            var validAsked = asked.Where(x => nameDictinoary.ContainsKey(x.UserId)).ToList();

            return new PromptAskedModel
            {
                Question = prompt.Question,
                PromptId = prompt.PromptId,
                Custom = prompt.UserId.HasValue,
                Askeds = validAsked
                    .Select(s => new Asked
                    {
                        Id = s.UserId,
                        Name = nameDictinoary[s.UserId],
                        DropId = dropsDictionary.ContainsKey(s.UserId) ? dropsDictionary[s.UserId] : 0,
                        Connection = true
                    })
            };
        }

        public async Task<List<PromptModel>> GetAllPrompts(int currentUserId)
        {
            await SyncUserPrompts(currentUserId);
            var prompts = await Context.Prompts
                .Where(x => !x.UserId.HasValue && !x.Template)
                .OrderByDescending(o => o.Order)
                .ToListAsync();
            prompts = await RelationshipPromptsToTop(prompts, currentUserId);
            
            return prompts.Select(s => new PromptModel
            {
                PromptId = s.PromptId,
                Question = s.Question,
                Order = s.Order,
                Askers = new List<Asked>()
            }).ToList();
        }

        public async Task DismissPrompt(int currentUserId, int promptId)
        {
            var prompt = await Context.UserPrompts.Where(x => x.UserId == currentUserId && x.PromptId == promptId
                && !x.Dismissed.HasValue && !x.Used.HasValue)
                .SingleOrDefaultAsync();
            if (prompt != null)
            {
                prompt.Dismissed = DateTime.UtcNow;
                await Context.SaveChangesAsync();
            }
        }

        public async Task UsePrompt(int currentUserId, int promptId)
        {
            var prompt = await Context.UserPrompts.Include(i => i.Prompt).Where(x => x.UserId == currentUserId && x.PromptId == promptId
                && !x.Dismissed.HasValue && !x.Used.HasValue)
                .SingleOrDefaultAsync();
            if (prompt != null)
            {
                prompt.Used = DateTime.UtcNow;
                prompt.Prompt.Order++; // we order based on popularity of use
                await Context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// This is to get a prompt for you have been asked
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="promptId"></param>
        /// <returns></returns>
        public async Task<PromptModel> GetPrompt(int currentUserId, int promptId)
        {
            var nameDictinoary = await userService.GetNameDictionary(currentUserId);
            var userPrompt = await Context.UserPrompts.Include(i => i.Prompt).Include(i => i.Askers)
                // you created it, are asked it, or it is a generic prompt
                .Where(p => p.PromptId == promptId && p.UserId == currentUserId)
                .SingleOrDefaultAsync();
            if (userPrompt == null) throw new Exceptions.NotFoundException();
            return MapPromptModel(userPrompt, nameDictinoary);
            
        }

        public async Task<PromptModel> CreatePrompt(int currentUserId, string question)
        {
            var prompt = new Prompt
            {
                Question = question,
                UserId = currentUserId,
                CreatedAt = DateTime.UtcNow,
                Order = 1
            };
            Context.Prompts.Add(prompt);
            await Context.SaveChangesAsync();
            return new PromptModel
            {
                PromptId = prompt.PromptId,
                Order = prompt.Order,
                Question = prompt.Question
            };
        }

        public async Task<PromptModel> UpdatePrompt(int currentUserId, string question, int promptId)
        {
            if (question == null || question.Length < 10) throw new BadRequestException();
            var existing = await Context.Prompts.
                SingleOrDefaultAsync(x => x.UserId == currentUserId && x.PromptId == promptId);
            if (existing == null) throw new NotFoundException();
            if (!string.IsNullOrWhiteSpace(question)) {
                existing.Question = question;
                await Context.SaveChangesAsync();
            }
            return new PromptModel
            {
                PromptId = existing.PromptId,
                Order = existing.Order,
                Question = existing.Question
            };
        }

        public async Task AskQuestion(int currentUserId, List<int> userIds, int promptId) {
            // look for this prompt where this user has already asked the target user - so we don't double ask
            var currentPrompts = await Context.UserPrompts.Where(x => x.PromptId == promptId).ToListAsync();
            var allreadyAsked = currentPrompts
                .Where(x => x.Askers.Any(a => a.AskerId == currentUserId))
                .Select(s => s.UserId).ToList();
            // missing need removed
            var toRemove = currentPrompts.Where(x => !userIds.Contains(x.UserId));
            Context.UserPromptAskers.RemoveRange(toRemove.SelectMany(s => s.Askers));
            Context.UserPrompts.RemoveRange(toRemove);
            // already there don't need added
            userIds.RemoveAll(x => allreadyAsked.Contains(x));

            userIds.ForEach(userId =>
                {
                    var userPrompt = currentPrompts.FirstOrDefault(x => x.UserId == userId);
                    if (userPrompt == null)
                    {
                        userPrompt = new UserPrompt
                        {
                            PromptId = promptId,
                            UserId = userId,
                        };
                        Context.UserPrompts.Add(userPrompt);
                    }
                    var userPromptAsker = new UserPromptAsker
                    {
                        AskerId = currentUserId,
                        CreatedAt = DateTime.UtcNow,
                        UserPrompt = userPrompt
                    };
                    Context.UserPromptAskers.Add(userPromptAsker);
                }
            );
            await Context.SaveChangesAsync();
            var prompt = await Context.Prompts.SingleAsync(x => x.PromptId == promptId);
            var usersEmailDictionary = await Context.UserProfiles.Where(x => userIds.Contains(x.UserId))
                .ToDictionaryAsync(k => k.UserId, v => v.Email);
            var nameDictionary = await dropService.GetInverseNameDictionary(currentUserId, userIds);

            userIds.ForEach(userId =>
                            sendEmailService.SendAsync(usersEmailDictionary[userId], EmailTypes.Question,
                new { User = nameDictionary[userId], Question = prompt.Question, Id = prompt.PromptId })
            );
            
        }

        public async Task<List<PromptModel>> AskedQuestions(int currentUserId) {
            var askedQuestions = await Context.UserPrompts.Include(x => x.Prompt).Include(x => x.Askers)
                .Where(x => x.UserId == currentUserId && x.Askers.Any())
                .ToListAsync();
            var nameDictionary = await userService.GetNameDictionary(currentUserId);
            return askedQuestions.Select(s => MapPromptModel(s, nameDictionary)).ToList();
            
        }

        public async Task<List<PromptModel>> GetTimelineQuestions(int currentUserId, int timelineId) {
            TimelineModel timeline;
            timeline = await timelineService.GetTimeline(currentUserId, timelineId);
            
            var prompts = await Context.PromptTimelines.Include(i => i.Prompt)
                .Where(x => x.TimelineId == timelineId).ToListAsync();
            // Todo - we may want filter out ones you have already answered
            return prompts.Select(s => new PromptModel {
                PromptId = s.PromptId,
                Question = s.Prompt.Question.Replace("{{name}}",timeline.Name),
                Order = s.Prompt.Order
            }).ToList();
        }

        public async Task<List<PromptModel>> GetAllTimelineQuestions(int currentUserId, int timelineId)
        {
            TimelineModel timeline;
            timeline = await timelineService.GetTimeline(currentUserId, timelineId);
            
            var prompts = await Context.Prompts.Include(i => i.PromptTimelines)
                .Where(x => x.Template
                    || x.PromptTimelines.Any(a => a.TimelineId == timelineId))
                .ToListAsync();
            // Todo - we may want filter out ones you have already answered
            return prompts.Select(s => new PromptModel
            {
                PromptId = s.PromptId,
                Question = s.Question.Replace("{{name}}", timeline.Name),
                Order = s.Order,
                Selected = s.PromptTimelines.Any(a => a.TimelineId == timelineId)
            }).ToList();
        }

        private PromptModel MapPromptModel(UserPrompt userPrompt, Dictionary<int,string> nameDictionary)
        {
            return new PromptModel
            {
                PromptId = userPrompt.PromptId,
                Question = userPrompt.Prompt.Question,
                Order = userPrompt.Prompt.Order,
                Askers = userPrompt.Askers.Where(a => nameDictionary.ContainsKey(a.AskerId))
                    .Select(a => new PersonModelV2 { Id = a.AskerId, Name = nameDictionary[a.AskerId] })
            };
        }

        private async Task SyncUserPrompts(int currentUserId)
        {
            var userPrompts = Context.UserPrompts.Where(p => p.UserId == currentUserId).Select(s => s.PromptId);
            var missingPrompts = await Context.Prompts.Where(p => !p.UserId.HasValue && !userPrompts.Contains(p.PromptId) && !p.Template).ToListAsync();

            Context.UserPrompts.AddRange(missingPrompts.Select(s => new UserPrompt
            {
                PromptId = s.PromptId,
                UserId = currentUserId
            }));
            await Context.SaveChangesAsync();
        }

        /// <summary>
        /// take an order list, and randomly select one item and put it at the top of it.
        /// </summary>
        /// <param name="promptModels"></param>
        /// <returns></returns>
        private List<PromptModel> ShuffleTop(List<PromptModel> promptModels)
        {
            var shuffled = new List<PromptModel>(promptModels);
            if (shuffled.Any())
            {
                var random = new Random();
                var index = random.Next(shuffled.Count - 1);
                var top = shuffled[0];
                shuffled[0] = shuffled[index];
                shuffled[index] = top;
            }
            return shuffled.OrderByDescending(x => x.Askers.Count()).ToList();
        }

        private async Task<List<Prompt>> RelationshipPromptsToTop(List<Prompt> prompts, int currentUserId) {
            var relationships = await userService.GetRelationships(currentUserId);
            var hasRelationships = relationships.Any(x => x.Selected);
            if (hasRelationships)
            {
                var relationshipIds = relationships.Where(x => x.Selected).Select(s => s.Id).ToHashSet();
                prompts = prompts.Where(x => !x.Relationship.HasValue
                    || relationshipIds.Contains((int)x.Relationship.Value))
                    .OrderBy(x => !x.Relationship.HasValue).ToList();
            }
            return prompts;
            
        }

        private async Task<List<UserPrompt>> RelationshipPromptsToTop(List<UserPrompt> prompts, int currentUserId)
        {

            var relationships = await userService.GetRelationships(currentUserId);
            var hasRelationships = relationships.Any(x => x.Selected);
            if (hasRelationships)
            {
                var relationshipIds = relationships.Where(x => x.Selected).Select(s => s.Id).ToHashSet();
                prompts = prompts.Where(x => !x.Prompt.Relationship.HasValue
                    || relationshipIds.Contains((int)x.Prompt.Relationship.Value))
                    .OrderBy(x => !x.Prompt.Relationship.HasValue).ToList();
            }
            return prompts;
            
        }

    }
}
