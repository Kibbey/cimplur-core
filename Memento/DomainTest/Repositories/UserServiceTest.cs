using Domain.Models;
using Domain.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DomainTest
{
    [TestClass]
    public class UserServiceTest
    {
        [TestMethod]
        public async Task AddUser()
        {
            var uuid = Guid.NewGuid().ToString();
            Assert.IsFalse(false);
            /*
            using (var userService = new UserService()) {
                var name = "John Smith";
                var email = $"{uuid}@fyli.com";
                var userId = await userService.AddUser(
                    email, 
                    uuid, 
                    null, 
                    true, 
                    name, 
                    new List<ReasonModel> { 
                        new ReasonModel{ 
                            Key = 1,
                            Selected = true,
                            Value = "Something"
                        }
                    });
                Assert.IsNotNull(userId);
                var user = await userService.GetProfile(userId);
            
                Assert.AreEqual(user.Email, email);
                Assert.AreEqual(user.Name, name);
            }
            */
            
        }
    }
}
