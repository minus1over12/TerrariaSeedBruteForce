using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleApp1;

public static partial class TerrariaBruteForce {
    private static readonly byte[] _salt = Convert.FromBase64String("fT2JQQzNMJl2NRoMbo9RjA==");

    private static readonly Dictionary<string, string> Targets = new() {
        ["2htOIVagY/7JFx7acMpyUR6D3qJDr/u+"] = "paintEverythingGray",
        ["YJayFFSdWEl66+rlFoWJRNvBHJi8gHnx"] = "paintEverythingNegative",
        ["5Czr2vSNyB9hJd1yob+TYo0qqH/5U2P9"] = "coatEverythingEcho",
        ["5YXhKErRZovhjJkrP9fptrVHbNc1oSSn"] = "coatEverythingIlluminant",
        ["cptECrPRxYeNTULJULs4gVoKdRsf3c3n"] = "noSurface",
        ["QQN1FbxlHeUCXPZc51GYvn8G5GXOJcny"] = "extraLivingTrees",
        ["0ebq4RCzI3PVaUPOT0f6/+vkXEaoLz2U"] = "extraFloatingIslands",
        ["GkviuS3QN0pyESRJdjIs6oC8s8hOhUXw"] = "errorWorld",
        ["N8G20sWOkIa7ZP0rS/jopLpe9180N6Tx"] = "graveyardBloodmoonStart",
        ["io2s6kMi4L7ZCDYZGP1Hc8nEWuYW4gp5"] = "surfaceIsInSpace",
        ["xYBNU5Soje9VhQHNQXETDKbwlc+7XZau"] = "rainsForAYear",
        ["vWb/t7nNF+tnjgr5VgY2hi0HcT1j3kvC"] = "biggerAbandonedHouses",
        ["zSwnCH9E121+S6VQdB0k20E7IPdtobls"] = "randomSpawn",
        ["+URq9gxzcyHxAXVqdwl1fz8wgPYYu0Wx"] = "addTeleporters",
        ["6kX2PJe0FWt3i0fp0tVBh5jt84ozLXBo"] = "startInHardmode",
        ["m1gQVuUnIRW083pnfFdnN3DPsg1qFYHZ"] = "noInfection",
        ["KYvKIk2LK0oyNY86m+uPhKQ7QbzFmDsR"] = "hallowOnTheSurface",
        ["kbxnychxHNDcoyFHhxM9OJHRxis6mFF/"] = "worldIsInfected",
        ["e48+tRi5DqzRkBPk3yq9udBG/kaYOQaB"] = "surfaceIsMushrooms",
        ["eyGmBQhQ9QnE7UsIib1QmnNRVBNmQtMi"] = "surfaceIsDesert",
        ["Iubz1XcBvsfPjSZucIJ3hCDFFEpjG57w"] = "pooEverywhere",
        ["SPlOdka0fv8wUovao6u3VB7ZS+IbcPDu"] = "noSpiderCaves",
        ["AoEz0g1XX0V/nJwcaN2RWwUf/6ghr9pT"] = "actuallyNoTraps",
        ["6lK0Tn4t2UlklesGiJ94617yKvk01ICB"] = "rainbowStuff",
        ["MucLvCERZix3rfcwUH68HDtuFYukiTv9"] = "digExtraHoles",
        ["VSN8nV180t6PgabWDl4Uf55I1vu97JRD"] = "roundLandmasses",
        ["ZYO3rUjSeCaaBrCE8Bv0FBtkjigLMz90"] = "extraLiquid",
        ["ALdQZ+bxQA4VdfjVfdhO/sm9q3sZD9dJ"] = "portalGunInChests",
        ["eH2IYQwQyOud0hyoTPaeVsqYlAP7MvbS"] = "worldIsFrozen",
        ["Z4Odmvd5lScy/KGXHUO2nvqA9l3KRvm8"] = "halloweenGen",
        ["KNSxbK83ZXH41aUhWLti9OFMxoMrCV1s"] = "endlessHalloween",
        ["gkN386qfe3u1qqQDpGsUu3DsRkEBpD1R"] = "endlessChristmas",
        ["4eijvDtfcSl66CDifYSVP3WBZm9OLBoW"] = "vampirism",
        ["HnTdmrZ5OT1ldA3r0w3dCgrdLnJBtBSD"] = "teamBasedSpawns",
        ["ypBuvKpqKay//OvhG2COriSpGT7f4YY3"] = "dualDungeons",
    };

    private static readonly ConcurrentDictionary<string, string> Found = new();

    [GeneratedRegex("^[a-z0-9]+$")]
    private static partial Regex ValidInputRegex();

    public static string ToSecret(string plainInput) {
        byte[] inputBytes = new BCrypt().CryptRaw(
            Encoding.UTF8.GetBytes(plainInput), _salt, 4);

        for (int index1 = 0; index1 < 1000; ++index1) {
            int index2 = index1 % inputBytes.Length;
            int index3 = (int)inputBytes[index2] % inputBytes.Length;
            (inputBytes[index2], inputBytes[index3]) = (inputBytes[index3], inputBytes[index2]);
        }

        return Convert.ToBase64String(
            new BCrypt().CryptRaw(inputBytes, _salt, 4));
    }

    private static void CheckCandidate(string candidate, Regex regex) {
        if (!regex.IsMatch(candidate)) return;

        string hash = ToSecret(candidate);
        if (Targets.TryGetValue(hash, out string? seedName)) {
            Found[hash] = candidate;
            Console.WriteLine($"🎉 FOUND: {seedName} = \"{candidate}\"");
            Console.WriteLine($"   Hash: {hash}");
            Console.WriteLine($"   Remaining: {Targets.Count - Found.Count}");
        }
    }

    public static void Main(string[] args) {
        var sw = Stopwatch.StartNew();
        var regex = ValidInputRegex();
        long totalCount = 0;

        // Load wordlist
        string wordlistPath = args.Length > 0 ? args[0] : "/usr/share/dict/words";
        if (!File.Exists(wordlistPath)) {
            Console.WriteLine($"Wordlist not found: {wordlistPath}");
            Console.WriteLine("Usage: dotnet run [wordlist.txt]");
            return;
        }

        var words = File.ReadAllLines(wordlistPath)
            .Select(w => w.ToLower().Trim())
            .Where(w => regex.IsMatch(w))
            .Distinct()
            .ToArray();

        Console.WriteLine($"Loaded {words.Length} valid words");
        Console.WriteLine($"Searching for {Targets.Count} secret seeds...\n");

        // Phase 1: Single words
        Console.WriteLine("=== Phase 1: Single words ===");
        Parallel.ForEach(words, word => {
            CheckCandidate(word, regex);
            long c = Interlocked.Increment(ref totalCount);
            if (c % 10000 == 0)
                Console.Write($"\rTried {c:N0} candidates... Found {Found.Count}/{Targets.Count}");
        });
        Console.WriteLine($"\nPhase 1 complete. Found {Found.Count}/{Targets.Count}\n");

        // Phase 2: Two-word combinations
        Console.WriteLine("=== Phase 2: Two-word combinations ===");
        Parallel.ForEach(words, word1 => {
            foreach (string word2 in words) {
                CheckCandidate(word1 + word2, regex);
                long c = Interlocked.Increment(ref totalCount);
                if (c % 100000 == 0)
                    Console.Write(
                        $"\rTried {c:N0} candidates... Found {Found.Count}/{Targets.Count}");
            }
        });
        Console.WriteLine($"\nPhase 2 complete. Found {Found.Count}/{Targets.Count}\n");

        // Phase 3: Three-word combinations
        if (Found.Count < Targets.Count) {
            Console.WriteLine("=== Phase 3: Three-word combinations ===");
            Parallel.ForEach(words, word1 => {
                foreach (var word2 in words)
                foreach (string word3 in words) {
                    CheckCandidate(word1 + word2 + word3, regex);
                    long c = Interlocked.Increment(ref totalCount);
                    if (c % 500000 == 0)
                        Console.Write(
                            $"\rTried {c:N0} candidates... Found {Found.Count}/{Targets.Count}");
                }
            });
        }

        // Results
        sw.Stop();
        Console.WriteLine("\n========== RESULTS ==========");
        Console.WriteLine($"Time elapsed: {sw.Elapsed}");
        Console.WriteLine($"Total candidates tried: {totalCount:N0}");
        Console.WriteLine($"Seeds found: {Found.Count}/{Targets.Count}\n");

        foreach (KeyValuePair<string, string> kvp in Found) {
            Console.WriteLine($"{Targets[kvp.Key],-30} = \"{kvp.Value}\"");
        }

        Console.WriteLine("\n=== Still missing ===");
        foreach (KeyValuePair<string, string> target in
                 Targets.Where(t => !Found.ContainsKey(t.Key))) {
            Console.WriteLine($"{target.Value}");
        }
    }
}