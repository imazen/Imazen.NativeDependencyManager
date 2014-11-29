using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Imazen.NativeDependencyManager
{
    class FileHashProvider
    {
        public FileHashProvider(Func<HashAlgorithm> algorithmPRovider)
        {
            AlgorithmProvider = algorithmPRovider;
        }
        public FileHashProvider() : 
            this(() => new SHA1CryptoServiceProvider()) { }
        public Func<HashAlgorithm> AlgorithmProvider { get; private set; }

        public Task<byte[]> HashStreamAsync(Stream s)
        {
            //Todo, replace this with actual async instead of just scheduling a thread for it.
            return Task.Run<byte[]>(() => HashStream(s));
        }
        public byte[] HashStream(Stream s)
        {
            using (var alg = AlgorithmProvider()){
                return alg.ComputeHash(s);
            }
        }
    }
}
