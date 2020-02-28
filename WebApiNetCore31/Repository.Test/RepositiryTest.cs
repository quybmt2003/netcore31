using Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Repository.Context;
using System;
using Xunit.Sdk;

namespace Repository.Test
{
    [TestClass]
    public class RepositiryTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var uow = new UnitOfWork(new IdentityUserContext());
            uow.Repository<User, string>().Insert(new User { Id = Guid.NewGuid().ToString(), Name = "Quy", Email = "test@gmail.com" });
            uow.Save();
        }
    }
}
