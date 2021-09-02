using System;
using Newtonsoft.Json;
using NUnit.Framework;
using Service.BitGo.SignTransaction.Domain.Models;
using Service.BitGo.SignTransaction.Utils;

namespace Service.BitGo.SignTransaction.Tests
{
    public class TestJsonConverter
    {
        [Test]
        public void HideApiKey()
        {
            var data = new BitGoUser()
            {
                Id = "UserId",
                ApiKey = "TestApiKey",
                BrokerId = "Broker",
                RegisterDate = DateTime.Now,
                UpdatedBy = "Test",
                UpdatedDate = DateTime.Now,
                BitGoId = "BitGoId"
            };

            string json = JsonConvert.SerializeObject(data, new ApiKeyHiddenJsonConverter(typeof(BitGoUser)));
            Console.WriteLine(json);
            Assert.False(json.Contains("TestApiKey"));
        }
    }
}