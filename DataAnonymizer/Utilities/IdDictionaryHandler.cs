using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;

namespace DataAnonymizer.Utilities
{
    internal class IdDictionaryHandler : IDisposable
    {
        private readonly Aes _aes;
        private DataWrapper _loadedData;

        public bool KeyWasSet { get; private set; }

        public IdDictionaryHandler()
        {
            _aes = Aes.Create();
            _loadedData = new DataWrapper();
        }

        public Result SetEncryptionKey(string key)
        {
            try
            {
                _aes.Key = Convert.FromBase64String(key);
                KeyWasSet = true;
            } catch (FormatException)
            {
                return Result.Failure($"Incorrect encryption key, it contains invalid characters.");
            }

            return Result.Success();
        }

        public void GenerateNewEncryptionKey()
        {
            _aes.KeySize = 256;
            _aes.GenerateKey();
        }

        public string GetEncryptionKey()
        {
            return Convert.ToBase64String(_aes.Key);
        }

        public void SaveDictionary(Dictionary<string, string> idDictionary, string path, bool encrypt)
        {
            var payload = JsonSerializer.Serialize(idDictionary);
            var iv = "";

            if (encrypt)
            {
                var payloadBytes = Encrypt(payload);
                payload = Convert.ToBase64String(payloadBytes);

                iv = Convert.ToBase64String(_aes.IV);
            }

            var wrappedData = new DataWrapper
            {
                Iv = iv,
                Payload = payload
            };

            var serializedData = JsonSerializer.Serialize(wrappedData);

            try
            {
                var directory = Path.GetDirectoryName(path);
                if (directory is not null)
                    Directory.CreateDirectory(directory);

                File.WriteAllText(path, serializedData, Encoding.GetEncoding("iso-8859-1"));
            }
            catch (IOException ex) 
            {
                Console.WriteLine(ex.ToString());
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Write(ex.ToString());
            }
        }

        public Result<bool> LoadIdDictionary(string path)
        {
            try
            {
                var serializedData = File.ReadAllText(path, Encoding.GetEncoding("iso-8859-1"));
                var wrappedData = JsonSerializer.Deserialize<DataWrapper>(serializedData);

                if (wrappedData is null)
                    return Result.Failure<bool>($"Failed to parse the key data file.");

                if (string.IsNullOrWhiteSpace(wrappedData.Payload))
                    return Result.Failure<bool>("Failed to parse the key data file. The payload is missing.");

                _loadedData = wrappedData;
            }
            catch (Exception ex)
            {
                return Result.Failure<bool>($"Failed to parse the key data file. Error: {ex.Message}");
            }

            return Result.Success(_loadedData.IsEncrypted());
        }

        public Result<Dictionary<string, string>> GetIdDictionary()
        {
            if (string.IsNullOrWhiteSpace(_loadedData.Payload))
                return new Dictionary<string, string>();

            try
            {
                string plaintext = _loadedData.Payload;

                if (_loadedData.IsEncrypted())
                {
                    var iv = Convert.FromBase64String(_loadedData.Iv);
                    var payload = Convert.FromBase64String(_loadedData.Payload);
                    plaintext = Decrypt(payload, iv);
                }

                var data = JsonSerializer.Deserialize<Dictionary<string, string>>(plaintext);

                if (data is null)
                    return Result.Failure<Dictionary<string, string>>("Failed to parse the ID Dictionary");

                return data;
            }
            catch (Exception e)
            {
                return Result.Failure<Dictionary<string, string>>($"Failed to load the ID Dictionary: {e.Message}");
            }
        }

        private byte[] Encrypt(string plainText)
        {
            _aes.GenerateIV();
            using MemoryStream msEncrypt = new();
            using CryptoStream csEncrypt = new(msEncrypt, _aes.CreateEncryptor(), CryptoStreamMode.Write);
            using (StreamWriter swEncrypt = new(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            return msEncrypt.ToArray();
        }

        private string Decrypt(byte[] encrypted, byte[] iv)
        {
            _aes.IV = iv;
            ICryptoTransform decryptor = _aes.CreateDecryptor(_aes.Key, iv);

            using MemoryStream msDecrypt = new(encrypted);
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt);
            return srDecrypt.ReadToEnd();
        }

        public void Dispose()
        {
            _aes.Dispose();
        }

        private class DataWrapper
        {
            public string Iv { get; init; }
            public string Payload { get; init; }

            public bool IsEncrypted() => !string.IsNullOrWhiteSpace(Iv);
        }
    }
}