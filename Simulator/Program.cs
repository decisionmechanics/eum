namespace Simulator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using ExpectedUtilityModel;

    internal class Program
    {
        /* Private instance methods */

        private static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                IList<Party> parties = ReadParties(args[0]).ToList();

                int rounds = int.Parse(args[1]);
                double statusQuoProbability = double.Parse(args[2]);

                Issue issue = CreateIssue(parties, statusQuoProbability);

                Console.WriteLine();
                Console.WriteLine($"Initial");
                Console.WriteLine("-------");

                DisplayPositions(parties, issue);
                DisplayCentralPositions(issue);

                for (int round = 0; round < rounds; round++)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Round {round + 1}");
                    Console.WriteLine("-------".PadRight((round + 1) / 10));

                    issue.SimulateRound();

                    DisplayPositions(parties, issue);
                    DisplayCentralPositions(issue);
                }
            }
            else
            {
                Console.WriteLine("Arguments: <data file path> <number of rounds> <status quo probability (q)>");
            }
        }

        /* Private static methods */

        private static Issue CreateIssue(ICollection<Party> parties, double statusQuoProbability)
        {
            IEnumerable<double> positions = parties.Select(x => x.Position).ToList();
            IEnumerable<double> capabilities = parties.Select(x => x.Capability).ToList();
            IEnumerable<double> saliences = parties.Select(x => x.Salience).ToList();

            return new Issue(positions, capabilities, saliences, statusQuoProbability);
        }

        private static void DisplayCentralPositions(Issue issue)
        {
            Console.WriteLine($"Median position: {issue.MedianPosition}");
            Console.WriteLine($"Mean position: {issue.MeanPosition}");
        }

        private static void DisplayPositions(IList<Party> parties, Issue issue)
        {
            for (int i = 0; i < parties.Count; i++)
            {
                Console.WriteLine($"{parties[i].Name}\t{issue.Positions[i]}");
            }
        }

        private static IEnumerable<Party> ReadParties(string filePath)
        {
            return File.ReadAllLines(filePath)
                .Skip(1)
                .Select(x => x.Split('\t'))
                .Select(x => new Party(x[0].Trim(), double.Parse(x[1]), double.Parse(x[2]), double.Parse(x[3])))
                .ToList();
        }
    }
}