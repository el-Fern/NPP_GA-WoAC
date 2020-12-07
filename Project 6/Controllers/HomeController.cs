using Project_5.Models;
using Project_5.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Project_5.Controllers
{
    public class HomeController : Controller
    {
        private string partitionsDirectory = AppContext.BaseDirectory + "PartitionSets\\";
        //new random variable to use for random mating and mutations
        Random random = new Random();

        public ActionResult Index()
        {
            //the view model that will hold and serve all of the information to the page
            var vm = new HomeIndexViewModel();

            //define the mutation rates
            var mutationRateA = 2;
            var mutationRateB = 5;
            var mutationRateC = 10;

            //for each file, generate a new problem section
            foreach (var file in Directory.GetFiles(partitionsDirectory))
            {
                //read numbers from the provided file(s)
                var numbers = ReadNumbersFromFile(file);

                //randomly generate 4 parent partitions that will be used for breeding for the problems
                var parentPartitions = GenerateRandomPartitions(numbers);

                //generate the paths through genetic algorithm
                var partitionProblemsForFile = new List<PartitionProblemModel>();
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating A & Mutation A", "A", mutationRateA));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating A & Mutation B", "A", mutationRateB));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating A & Mutation C", "A", mutationRateC));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating B & Mutation A", "B", mutationRateA));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating B & Mutation B", "B", mutationRateB));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating B & Mutation C", "B", mutationRateC));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating C & Mutation A", "C", mutationRateA));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating C & Mutation B", "C", mutationRateB));
                partitionProblemsForFile.Add(GeneratePartitionProblem(numbers, parentPartitions, Path.GetFileName(file) + " - Mating C & Mutation C", "C", mutationRateC));

                //add in the wisdom of the crowds TSP problem
                vm.Problems.Add(WisdomOfTheCrowdsPartitionProblem(numbers, partitionProblemsForFile.Select(x => x.Partitions).ToList(), Path.GetFileName(file) + " - Wisdom of the Crowds approach"));

                //add in the problems after the wisdom of the crowds problem
                vm.Problems.AddRange(partitionProblemsForFile);
            }

            return View(vm);
        }

        //generate 2 random paths that will be the parents of all of the mutations
        private List<PartitionSet> GenerateRandomPartitions(List<int> numbers)
        {
            //list of partition sets that will be returned
            var parentPartitions = new List<PartitionSet>();

            for (int i = 0; i < 6; i++)
            {
                var partitionSet = new PartitionSet();

                //keep track of all unused numbers
                var unusedNumbers = numbers.Select(x => x).ToList();

                //iterate through the amount of numbers
                for (int y = 0; y < numbers.Count; y++)
                {
                    //randomly generate next node
                    var nextNumber = unusedNumbers[random.Next(unusedNumbers.Count)];

                    if (random.Next(2) == 0)
                    {
                        partitionSet.Partition1.Add(nextNumber);
                    }
                    else
                    {
                        partitionSet.Partition2.Add(nextNumber);
                    }

                    //remove the new number from the unused list
                    unusedNumbers.Remove(nextNumber);
                }

                parentPartitions.Add(partitionSet);
            }
            return parentPartitions;
        }

        private PartitionProblemModel GeneratePartitionProblem(List<int> numbers, List<PartitionSet> parentPartitions, string problemName, string matingMethod, int mutationRate)
        {
            //trackTimeToRun
            DateTime startTime = DateTime.Now;
            //generate partition problem that will be displayed on the page
            var partitionProblem = new PartitionProblemModel();
            partitionProblem.FileName = problemName;

            //assign parentPartitions to the variable that will hold each generation of partitions
            var currentGenerationPartitions = new List<PartitionSet>();
            //adding in numbers through foreach loop so it has no reference to the parent partitions and does not modify those
            foreach (var parent in parentPartitions)
            {
                var newPartitionSet = new PartitionSet();
                foreach (var num in parent.Partition1)
                    newPartitionSet.Partition1.Add(num);
                foreach (var num in parent.Partition2)
                    newPartitionSet.Partition2.Add(num);

                currentGenerationPartitions.Add(newPartitionSet);
            }

            //order the paths in ascending difference order. Least diffence between partitions is the most fit parent to reproduce
            currentGenerationPartitions = currentGenerationPartitions.OrderBy(x => Math.Abs(x.Partition1.Sum() - x.Partition2.Sum())).ToList();

            //go through 500 generations to mutate and mate the best partition sets
            for (int i = 0; i < 1000; i++)
            {
                //variable to hold all of the children partition sets
                var childPartitionSets = new List<PartitionSet>();

                //select provided mating method
                switch (matingMethod)
                {
                    case "A":
                        //best set mates with the second, third, and fourth best set on both sides and worst two sets are ditched
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[1], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[2], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[3], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[1], currentGenerationPartitions[0], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[2], currentGenerationPartitions[0], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[3], currentGenerationPartitions[0], numbers, mutationRate));
                        break;
                    case "B":
                        //top 4 sets mate together randomly
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(0, 4)], currentGenerationPartitions[random.Next(0, 4)], numbers, mutationRate));
                        break;
                    case "C":
                        //best set mates randomly with the other sets
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[random.Next(1, 5)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[random.Next(1, 5)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[0], currentGenerationPartitions[random.Next(1, 5)], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(1, 5)], currentGenerationPartitions[0], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(1, 5)], currentGenerationPartitions[0], numbers, mutationRate));
                        childPartitionSets.Add(MatePartitions(currentGenerationPartitions[random.Next(1, 5)], currentGenerationPartitions[0], numbers, mutationRate));
                        break;
                    default:
                        break;
                }

                //make a new list for child sets to mutate. Doing this to prevent concurrency issues
                var childPartitionSetsForMutations = new List<PartitionSet>();
                foreach (var child in childPartitionSets)
                {
                    var newPartitionSet = new PartitionSet();
                    foreach (var num in child.Partition1)
                        newPartitionSet.Partition1.Add(num);
                    foreach (var num in child.Partition2)
                        newPartitionSet.Partition2.Add(num);

                    childPartitionSetsForMutations.Add(newPartitionSet);
                }

                //foreach set in the childPartitionSets, apply the random mutation chances
                foreach (var childPartitionSet in childPartitionSetsForMutations)
                    MutatePartitionSet(childPartitionSet, mutationRate);

                //overwrite the currentGenerationPartitions with the childPartitionSets before the next generation starts
                currentGenerationPartitions = childPartitionSetsForMutations.OrderBy(x => Math.Abs(x.Partition1.Sum() - x.Partition2.Sum())).ToList();
            }

            //assign the path to the most fit path
            partitionProblem.Partitions = currentGenerationPartitions[0];
            //calculate overall distance
            partitionProblem.TotalDifference = Math.Abs(partitionProblem.Partitions.Partition1.Sum() - partitionProblem.Partitions.Partition2.Sum());
            //calculate time to run
            partitionProblem.MillisecondsToRun = (DateTime.Now - startTime).TotalMilliseconds;

            return partitionProblem;
        }

        //mate two partition sets together
        private PartitionSet MatePartitions(PartitionSet set1, PartitionSet set2, List<int> numbers, int mutationRate)
        {
            //new variable to hold the set from mating
            var childPartition = new PartitionSet();
            var unusedNumbers = numbers.Select(x => x).ToList();

            //partition 1 size will be in between set1 partition1 size and set2 partition1 size
            var newPartition1Count = Math.Round((set1.Partition1.Count + set2.Partition1.Count) / 2m);
            var newPartition2Count = numbers.Count - newPartition1Count;

            for (int i = 0; i < newPartition1Count; i++)
            {
                var setToUse = set2.Partition1;
                var backupSet = set1.Partition1;
                if (i%2 == 0)
                {
                    setToUse = set1.Partition1;
                    backupSet = set2.Partition1;
                }

                var numberToAdd = 0;
                if (setToUse.Count > i && unusedNumbers.Contains(setToUse[i]))
                    numberToAdd = setToUse[i];
                else if (backupSet.Count > i && unusedNumbers.Contains(backupSet[i]))
                    numberToAdd = backupSet[i];
                else
                    numberToAdd = unusedNumbers[random.Next(unusedNumbers.Count)];

                childPartition.Partition1.Add(numberToAdd);
                unusedNumbers.Remove(numberToAdd);
            }

            //set partition2 to the remaining numbers
            childPartition.Partition2 = unusedNumbers;
            return childPartition;
        }

        //apply the mutation rate to each partition
        private void MutatePartitionSet(PartitionSet partitionSet, int mutationRate)
        {
            //broke this out to follow DRY principal with repeating this on partition1 and partition2
            MutateNumber(partitionSet.Partition1, partitionSet.Partition2, mutationRate);
            MutateNumber(partitionSet.Partition2, partitionSet.Partition1, mutationRate);
        }

        private void MutateNumber(List<int> primaryPartition, List<int> secondaryPartition, int mutationRate)
        {   
            //go through each number in each partition
            for (int x = 0; x < primaryPartition.Count; x++)
            {
                //execute random percentage chance by generating a number 1-99. If it's less than or equal to the mutation rate, Mutate
                if (random.Next(1, 100) <= mutationRate)
                {
                    //mutating is moving the current number from this partition to the other
                    var mutateNumber = primaryPartition[x];

                    //remove the number
                    primaryPartition.Remove(mutateNumber);
                    //add the number to the opposite partition
                    secondaryPartition.Add(mutateNumber);
                }
            }

        }

        private PartitionProblemModel WisdomOfTheCrowdsPartitionProblem(List<int> numbers, List<PartitionSet> partitionSets, string problemName)
        {
            //generate partition problem that will be displayed on the page
            var partitionProblem = new PartitionProblemModel();
            partitionProblem.FileName = problemName;

            //declare partition set that will be returned
            var newPartitionSet = new PartitionSet();

            foreach(var num in numbers)
            {
                //count the occurences of current number in each partition
                var partition1Count = 0;
                var partition2Count = 0;

                foreach(var partitionSet in partitionSets)
                {
                    partition1Count += partitionSet.Partition1.Where(x => x == num).Count();
                    partition2Count += partitionSet.Partition2.Where(x => x == num).Count();
                }

                //insert it in the most common occured partition
                if(partition1Count > partition2Count)
                    newPartitionSet.Partition1.Add(num);
                else if(partition2Count > partition1Count)
                    newPartitionSet.Partition2.Add(num);
                else //count is equal, randomly choose which one to put it in
                    if (random.Next(2) == 0)
                        newPartitionSet.Partition1.Add(num);
            }
            partitionProblem.Partitions = newPartitionSet;
            partitionProblem.TotalDifference = Math.Abs(partitionProblem.Partitions.Partition1.Sum() - partitionProblem.Partitions.Partition2.Sum());

            return partitionProblem;
        }

        //read the numbers from the partition problem files
        private List<int> ReadNumbersFromFile(string file)
        {
            var nums = new List<int>();

            var lines = System.IO.File.ReadAllLines(file);

            //get all numbers from file
            for (var i = 0; i < lines.Count(); i++)
            {
                //add in the number from the current line to the list of numbers
                nums.Add(Convert.ToInt32(lines[i]));
            }
            return nums;
        }
    }
}