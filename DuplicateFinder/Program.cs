using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace DuplicateFinder
{
    static class Program
    {
        private static readonly object _obj = new object();
        private static readonly List<List<string>> _sameFiles = new List<List<string>>();

        private static string GetFileHash(HashAlgorithm hashAlg, FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                var hash = hashAlg.ComputeHash(fs);

                return BitConverter.ToString(hash);
            }
        }

        private static void ProcessChunk(IGrouping<long, FileInfo> group)
        {
            var hashAlg = MD5.Create();

            var files = group.Select(f => new
            {
                f.FullName,
                Hash = GetFileHash(hashAlg, f)
            }).ToList();

            for (var i = 0; i < files.Count; i++)
            {
                List<string> curGroup;

                lock (_obj)
                {
                    _sameFiles.Add(new List<string>());

                    curGroup = _sameFiles.Last();
                }

                curGroup.Add(files[i].FullName);

                for (var j = i + 1; j < files.Count; j++)
                {
                    if (files[i].Hash != files[j].Hash) continue;

                    curGroup.Add(files[j].FullName);

                    files.Remove(files[j]);
                    j--;
                }
            }
        }

        private static void Main(string[] args)
        {
            var path = args.Length != 0 ? args[0] : Console.ReadLine();

            if (!Directory.Exists(path))
            {
                Console.WriteLine("No such directory");
                Console.ReadKey();
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            string[] fileNames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            var groups = fileNames.Select(n => new FileInfo(n))
                                  .GroupBy(f => f.Length)
                                  .Where(g => g.Skip(1).Any());

            Parallel.ForEach(groups, ProcessChunk);

            stopwatch.Stop();

            foreach (var list in _sameFiles.Where(g => g.Skip(1).Any()))
            {
                foreach (var filename in list)
                {
                    Console.WriteLine(filename);
                }

                Console.WriteLine();
            }

            Console.WriteLine($"Time passed: {stopwatch.Elapsed}");
            Console.ReadKey();
        }
    }
}