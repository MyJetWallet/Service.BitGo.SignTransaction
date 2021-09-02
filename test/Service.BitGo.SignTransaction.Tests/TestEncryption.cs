using NUnit.Framework;
using Service.BitGo.SignTransaction.Services;

namespace Service.BitGo.SignTransaction.Tests
{
    public class TestEncryption
    {
        private readonly SymmetricEncryptionService _encryptService;
        private readonly SymmetricEncryptionService _decryptService;

        public TestEncryption()
        {
            _encryptService = new SymmetricEncryptionService("2Yjeh9Rs/cD9tZRifvC1wgcyfNFF4B8JFO2Ou4qjiFM=");
            _decryptService = new SymmetricEncryptionService("2Yjeh9Rs/cD9tZRifvC1wgcyfNFF4B8JFO2Ou4qjiFM=");
        }

        [Test]
        public void Encrypt_And_Decrypt()
        {
            var data = "Test string to encrypt.";

            var encryptedData = _encryptService.Encrypt(data);
            var decryptedData = _decryptService.Decrypt(encryptedData);

            Assert.AreEqual(data, decryptedData);
        }
    }
}