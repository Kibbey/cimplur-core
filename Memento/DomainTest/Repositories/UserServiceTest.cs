using Domain.Models;
using Domain.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace DomainTest
{
    [TestClass]
    public class UserServiceTest
    {
        [TestMethod]
        public void AddUser()
        {
            var uuid = Guid.NewGuid().ToString();
            using (var userService = new UserService()) {
                var user = userService.AddUser(
                    $"{uuid}@fyli.com", 
                    uuid, 
                    null, 
                    true, 
                    "", 
                    new List<ReasonModel> { 
                        new ReasonModel{ 
                            Key = 1,
                            Selected = true,
                            Value = "Something"
                        }
                    });
                Assert.IsNotNull(user);
            }

            Assert.IsTrue(true);
        }
    }
}
