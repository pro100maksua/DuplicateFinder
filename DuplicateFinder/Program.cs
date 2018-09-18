using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DuplicateFinder
{
    class Program
    {
        static HashAlgorithm _hashAlg = MD5.Create();

        static string GetFileHash(FileInfo file)
        {
            using (var fs = file.OpenRead())
            {
                var hash = _hashAlg.ComputeHash(fs);

                return BitConverter.ToString(hash);
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Enter directory path: ");
            var path = Console.ReadLine();

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
                                  .Where(g => g.Count() > 1)
                                  .ToList();

            var sameFiles = new List<List<string>>();
            foreach (var group in groups)
            {
                var files = group.Select(f => new
                {
                    f.FullName,
                    Hash = GetFileHash(f)
                }).ToList();

                for (var i = 0; i < files.Count; i++)
                {
                    sameFiles.Add(new List<string>());
                    sameFiles.Last().Add(files[i].FullName);

                    for (var j = i + 1; j < files.Count; j++)
                    {
                        if (files[i].Hash != files[j].Hash) continue;

                        sameFiles.Last().Add(files[j].FullName);

                        files.Remove(files[j]);
                        j--;
                    }
                }
            }

            stopwatch.Stop();

            foreach (var list in sameFiles.Where(g => g.Count > 1))
            {
                Console.WriteLine("Same files:");
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