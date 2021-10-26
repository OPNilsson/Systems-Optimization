using System;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

namespace exercise
{
	public class Task
	{
		int id, deadline, period, wcet, wcrt;

		bool scheduled;

		public Task(int id, int deadline, int period, int wcet)
		{
			this.id = id;
			this.deadline = deadline;
			this.period = period;
			this.wcet = wcet;

			wcrt = 0;
			scheduled = false;
		}

		public int getId() { return id; }
		public int getDeadline() { return deadline; }
		public int getPeriod() { return period; }
		public int getWCET() { return wcet; }
		public int getWCRT() { return wcrt; }
		public bool isScheduled() { return scheduled; }

		public void updateWCRT(int wcrt) { this.wcrt = wcrt; }
		public void schedule() { scheduled = true; }
	}

	public class Core
	{
		int mcp;
		int id;
		float WCETFactor;

		public Core(int mcp, int id, float WCETFactor)
		{
			this.mcp = mcp;
			this.id = id;
			this.WCETFactor = WCETFactor;
		}

		public Core(Core core)
		{
			this.mcp = core.mcp;
			this.id = core.id;
			this.WCETFactor = core.WCETFactor;
		}

		public int getId()
		{
			return id;
		}

		public float getWCETFactor()
		{
			return WCETFactor;
		}

		public int getMcp() { return mcp; }
	}

	public class MCP
	{
		int id;
		List<Core> cores;

		public MCP(int id, List<Core> cores)
		{
			this.id = id;
			this.cores = cores;
		}

		public int getId()
		{
			return id;
		}
		public List<Core> getCores()
		{
			return cores;
		}
	}

	class Program
	{

		public const string PATH = "../../XML/";
		public static string menu()
		{
			bool asking = true;

			while (asking)
			{
				Console.WriteLine("Welcome to our task scheduling program!");
				Console.WriteLine("---------------------------------------");

				Console.WriteLine("The available sample sizes are: ");
				Console.WriteLine("1) small");
				Console.WriteLine("2) medium");
				Console.WriteLine("3) large");

				Console.WriteLine();

				Console.Write("Please Select the size of your Sample Use case: ");

				int response = Int32.Parse(Console.ReadLine());

				switch (response)
				{
					case 1:
						{
							return "small";
						}

					case 2:
						{
							return "medium";
						}
					case 3:
						{
							return "large";
						}
					default:
						{
							Console.Clear();
							Console.WriteLine("**********************************************************");
							Console.WriteLine("Sorry please select one of the options by entering: 1 - 3");
							Console.WriteLine("**********************************************************");
							Console.WriteLine();

							break;
						}
				}
			}

			return "error";
		}

		/**
		 * This is a modified version of ShortestJobFirst initial scheduling algorithm in where larger tasks get priority on cores with low WCETFactor.
		 * The solution also attempts to reduce idle time by checking if there are empty cores first and assigning tasks to those.
		 *
		 * returns map with solution of mapped tasks to cores.
		**/
		public static Dictionary<Core, List<Task>> ShortestJobFirst(Dictionary<Core, List<Task>> map, List<MCP> mCPs, List<Task> tasks)
		{
			// Check which tasks are left unscheduled.
			List<Task> unscheduledTasks = new List<Task>();
			foreach (Task task in tasks)
			{
				if (!task.isScheduled())
				{
					unscheduledTasks.Add(task);
				}
			}

			// Sort the tasks into ascending order of WCET
			unscheduledTasks.Sort((x, y) => x.getWCET().CompareTo(y.getWCET()));

			int mcpId = 0;
			int coreId = 0;

			int highestTaskCount = 0;
			int lowerstTaskCount = 0;

			// Testing variables DELETE when no longer needed.
			int unscheduled = unscheduledTasks.Count();
			int scheduled = tasks.Count() - unscheduled;

			List<Core> cores = new List<Core>();

			// Sort Cores into ascending order of WCETFactor so that larger tasks get the least affected.
			foreach (MCP mcp in mCPs)
			{
				List<Core> mcpCores = mcp.getCores();

				mcpCores.Sort((x, y) => x.getWCETFactor().CompareTo(y.getWCETFactor()));

				foreach (Core core in mcpCores)
				{
					cores.Add(core);
				}
			}
			cores.Sort((x, y) => x.getWCETFactor().CompareTo(y.getWCETFactor()));

			// Sort each task into cores as long as the deadline will be met.
			foreach (Task task in unscheduledTasks)
			{
				foreach (Core core in cores)
				{
					bool available = false;
					mcpId = mCPs.FindIndex(a => a.getId() == core.getMcp());

					List<Core> originalCores = mCPs[mcpId].getCores();
					coreId = originalCores.FindIndex(a => a == core);

					List<Task> coreTasks = map[mCPs[mcpId].getCores()[coreId]];

					if (scheduled == cores.Count() && lowerstTaskCount == 0) // All cores have been assigned atleast one task
					{
						lowerstTaskCount++;

						map[mCPs[mcpId].getCores()[coreId]].Add(task);

						task.schedule();
						scheduled++;

						break;
					}

					// Balance task placement across the cores.
					List<Task> nextCoreTasks;
					List<Task> prevCoreTasks;
					try
					{
						nextCoreTasks = map[mCPs[mcpId].getCores()[coreId + 1]];
					}
					catch
					{
						nextCoreTasks = null;
					}

					try
					{
						prevCoreTasks = map[mCPs[mcpId].getCores()[coreId - 1]];
					}
					catch
					{
						prevCoreTasks = null;
					}

					if (coreTasks.Count() > highestTaskCount) // The core is the most used in the system.
					{
						highestTaskCount = coreTasks.Count();
						continue;
					}
					else if (coreTasks.Count() == 0) // If the core is empty
					{
						available = true;
					}
					else if (lowerstTaskCount == 0) // Makes sure that atleast 1 task is on every core
					{
						continue;
					}
					else if (prevCoreTasks != null || nextCoreTasks != null)
					{
						if (prevCoreTasks == null)
						{
							if (coreTasks.Count() <= nextCoreTasks.Count())
							{
								available = true;
							}
						}
						else if (nextCoreTasks == null)
						{
							if (coreTasks.Count() <= prevCoreTasks.Count())
							{
								available = true;
							}
						}
						else
						{
							if (coreTasks.Count() <= nextCoreTasks.Count() && coreTasks.Count() <= prevCoreTasks.Count()) // If there are no less used cores on the MCP
							{
								available = true;
							}
						}
					}

					if (available)
					{
						// reduce looping.
						if (coreTasks.Count() > 0)
						{
							map[mCPs[mcpId].getCores()[coreId]].Add(task);

							// Checks to see if it's a solution after adding the new task.
							if (DM_guarantee(map))
							{
								task.schedule();
								scheduled++;
								break; // break is used so that the loop moves on to the next task.
							}
							else
							{
								map[mCPs[mcpId].getCores()[coreId]].Remove(task);
							}
						}
						else // The Core is empty of tasks and thus the task can be added.
						{
							map[mCPs[mcpId].getCores()[coreId]].Add(task);

							task.schedule();
							scheduled++;

							break;
						}
					}
				}
			}

			unscheduled = tasks.Count() - scheduled; // Here for testing purposes

			return map;
		}

		/**
		 * Swaps two tasks
		 *
		 **/
		public static Dictionary<Core, List<Task>> swapRandomTasks(Dictionary<Core, List<Task>> map, List<MCP> mcps)
		{
			var random = new Random();
			Dictionary<Core, List<Task>> mapP = new Dictionary<Core, List<Task>>();

			foreach (var entry in map)
			{
				mapP.Add(entry.Key, new List<Task>(entry.Value));
			}

			List<Task> tasks;
			do
			{
				tasks = mapP.ElementAt(random.Next(mapP.Count)).Value;
			} while (tasks.Count <= 0);
			Task task = tasks[random.Next(tasks.Count)];

			int mcpsNum = random.Next(0, mcps.Count);
			Core newCore;

			do
			{
				newCore = mcps[mcpsNum].getCores()[random.Next(mcps[mcpsNum].getCores().Count)];

				if (!mapP.ContainsKey(newCore)) mapP.Add(newCore, new List<Task>());
			} while (mapP[newCore].Contains(task));
			tasks.Remove(task);
			mapP[newCore].Add(task);

			return mapP;
		}

		/**
		 *  Checks if the map is a solution as definied in the books.
		 *
		 *  A mapping is a solution if a task's response time (execution time and waiting time) is smaller than its deadline.
		 *  This takes into account the core's WCETFactor.
		 *
		 *  This formula was taken from the lecture and adapted into code.
		 */
		public static bool DM_guarantee(Dictionary<Core, List<Task>> map)
		{
			int R, Ci;
			foreach (var mapping in map)
			{
				for (int i = 0; i < mapping.Value.Count; i++)
				{
					int I = 0;
					do
					{
						Ci = (int)(mapping.Value[i].getWCET() * mapping.Key.getWCETFactor());
						R = I + Ci;

						// Adds response time to the task
						mapping.Value[i].updateWCRT(R);

						if (R > mapping.Value[i].getDeadline())
						{
							return false;
						}

						I = 0;
						for (int j = 0; j < i; j++)
						{
							decimal rTemp = (decimal)R / (decimal)mapping.Value[j].getPeriod();
							int Cj = (int)(mapping.Value[j].getWCET() * mapping.Key.getWCETFactor());
							I += (int)Math.Ceiling(rTemp) * Cj;
						}
					} while (I + Ci > R);
				}
			}
			return true;
		}

		// Calculates the total laxity of the solution Laxity is a measure of how much time the
		// system has of "slack" thus a higher laxity means the system is less stressed :)
		public static int CalculateTotalLaxity(Dictionary<Core, List<Task>> map)
		{
			int totalLaxity = 0;

			foreach (var mapping in map)
			{
				foreach (Task task in mapping.Value)
				{
					int laxity = task.getDeadline() - task.getWCRT();

					totalLaxity = totalLaxity + laxity;
				}
			}

			return totalLaxity;
		}

		// Calculates the probability of accepting a worse solution.
		public static double acceptingProbability(int laxity, int adjLaxity, double temperature)
		{
			if (adjLaxity > laxity)
				return 1.0;
			else
				return Math.Exp((laxity - adjLaxity) / temperature);
		}

		public static void printXML(Dictionary<Core, List<Task>> map, int laxity)
		{
			bool asking = true;

			Console.WriteLine("---------------------------------------");

			while (asking)
			{
				Console.WriteLine();
				Console.WriteLine("The available options are: ");
				Console.WriteLine("1) YES");
				Console.WriteLine("2) NO");

				Console.WriteLine();

				Console.Write("Please Select if you would like to save the solution as an XML: ");

				int response = Int32.Parse(Console.ReadLine());

				switch (response)
				{
					case 1:
						{
							XDocument doc = new XDocument(new XElement("solution"));

							Console.WriteLine();
							Console.Write("Writing to file ");

							// Get the Tasks and Cores from the map
							foreach (var mapping in map)
							{
								// Get the core
								Core core = mapping.Key;

								foreach (Task task in mapping.Value)
								{
									Console.Write(".");

									// Add the elements to the XML
									doc.Root.Add(new XElement("Task",
									new XAttribute("ID", task.getId()),
									new XAttribute("MCP", core.getMcp()),
									new XAttribute("Core", core.getId()),
									new XAttribute("WCRT", task.getWCRT())));
								}
							}

							doc.Add(new XComment("Total Laxity: " + laxity));

							// Save the xml with name "solution.xml"
							doc.Save(Directory.GetCurrentDirectory() + "//solution.xml");

							Console.WriteLine();
							Console.WriteLine();
							Console.WriteLine("Document saved to: ");
							Console.WriteLine(Directory.GetCurrentDirectory() + "//solution.xml");

							asking = false; // Break the asking loop
							break;
						}

					case 2:
						{
							Console.WriteLine();
							Console.Write("No file saved!");

							asking = false; // Break the asking loop
							break;
						}
					default:
						{
							Console.Clear();
							Console.WriteLine("**********************************************************");
							Console.WriteLine("Sorry please select one of the options by entering: 1 | 2");
							Console.WriteLine("**********************************************************");
							Console.WriteLine();

							break;
						}
				}
			}

			Console.WriteLine();
			Console.WriteLine("Press ENTER to exit.");
		}

		static void Main(string[] args)
		{
			int ITERATION_BUFFER = 0;

			// Read The XML
			XmlDocument doc = new XmlDocument();

			// Sets up the program by asking if the user wants a small | medium | large sample size.
			string size = menu();

			switch (size)
			{
				case "small":
					{
						doc.Load(PATH + "small.xml");
						ITERATION_BUFFER = 5555;
						break;
					}

				case "medium":
					{
						doc.Load(PATH + "medium.xml");
						ITERATION_BUFFER = 1234;
						break;
					}
				case "large":
					{
						doc.Load(PATH + "large.xml");
						ITERATION_BUFFER = 1486;
						break;
					}
				default:
					{
						doc.Load(PATH + "medium.xml");
						ITERATION_BUFFER = 1234;
						break;
					}
			}

			// Read Tasks from XML
			List<Task> tasks = new List<Task>();
			tasks.Clear();
			var nodes = doc.SelectNodes("//Application");
			foreach (XmlNode node in nodes)
			{
				var aNodes = node.SelectNodes(".//Task");
				foreach (XmlNode aNode in aNodes)
					tasks.Add(new Task(int.Parse(aNode.Attributes["Id"].Value), int.Parse(aNode.Attributes["Deadline"].Value), int.Parse(aNode.Attributes["Period"].Value), int.Parse(aNode.Attributes["WCET"].Value)));
			}

			// Read Cores from XML
			List<MCP> mcps = new List<MCP>();
			List<Core> cores = new List<Core>();
			mcps.Clear();
			cores.Clear();
			nodes = doc.SelectNodes("//Platform");
			foreach (XmlNode node in nodes)
			{
				var mcpNodes = node.SelectNodes(".//MCP");
				foreach (XmlNode coreNodes in mcpNodes)
				{
					var coreNode = coreNodes.SelectNodes(".//Core");
					foreach (XmlNode naNode in coreNode)
						cores.Add(new Core(int.Parse(coreNodes.Attributes["Id"].Value), int.Parse(naNode.Attributes["Id"].Value), float.Parse(naNode.Attributes["WCETFactor"].Value)));
					mcps.Add(new MCP(int.Parse(coreNodes.Attributes["Id"].Value), new List<Core>(cores)));
					cores.Clear();
				}
			}

			// Initialize the core lists
			var map = new Dictionary<Core, List<Task>>();
			foreach (MCP mCp in mcps)
			{
				foreach (Core core in mCp.getCores())
				{
					map.Add(core, new List<Task>());
				}
			}

			// Attempt at an initial solution made by sorting tasks into cores based on shortest job first.
			Console.Write("Finding Initial Solution ");
			do
			{
				Console.Write(". ");
				map = ShortestJobFirst(map, mcps, tasks);
			} while (!DM_guarantee(map)); // Here as a saftey to make sure that it was a solution
			Console.WriteLine("\nInitial Solution Found!");

			// Exit Parameters for the Simulated Annealing
			int MAX_ITERATIONS = 151000;
			double MIN_TEMPERATURE = 0.0001;

			int iteration = 0;
			double temperature = 1000.0; // As suggested in the lecture slides
			double coolingRate = 0.9995; // TODO: Not fixed cooling rate?

			Dictionary<Core, List<Task>> bestSolution = map;
			int bestLaxity = CalculateTotalLaxity(map);
			int laxity = bestLaxity;

			var random = new Random();

			int solutionsFound = 0;

			while (iteration < MAX_ITERATIONS && temperature > MIN_TEMPERATURE)
			{
				// Adds a buffering effect to know if computing
				if (iteration % ITERATION_BUFFER == 0) { Console.Clear(); Console.WriteLine("Current iteration: " + iteration); }

				// Swap Two Random Tasks
				var tempSolution = swapRandomTasks(map, mcps); // TODO: better neighbor function?

				// If all tasks meet deadline then it's a solution
				if (DM_guarantee(tempSolution)) // TODO: Make the cost function of the SA to have more than one parameter?
				{
					++solutionsFound;

					// Compute laxity of adjacent state.
					int laxityNeighbor = CalculateTotalLaxity(tempSolution);

					// Check if new solution is new best. A solution is better if there is more
					// laxity in the system.
					if (laxityNeighbor > bestLaxity)
					{
						bestSolution = tempSolution;
						bestLaxity = laxityNeighbor;
					}

					// If neighbor solution is better, accept solution. Otherwise accept with
					// varying probability.
					double p = random.NextDouble();
					if (acceptingProbability(laxity, laxityNeighbor, temperature) > p)
					{
						map = tempSolution;
						laxity = laxityNeighbor;
					}

					// End Iteration by cooling
					temperature = temperature * coolingRate; // TODO: Make the cooling schedule not fixed?
				}

				++iteration;
			}

			Console.Clear();

			// Check why the simulated annealing ended
			if (temperature < MIN_TEMPERATURE)
			{
				Console.WriteLine("Temperature fell below accepted MIN! Ending Simulated Annealing!");
				Console.WriteLine("Temperature: " + temperature);
				Console.WriteLine("MIN TEMP:    " + MIN_TEMPERATURE);
				Console.WriteLine();
			}
			else if (iteration == MAX_ITERATIONS)
			{
				Console.WriteLine("Iteration Limit Hit! Ending Simulated Annealing!");
				Console.WriteLine("Iteration:       " + iteration);
				Console.WriteLine("Max Iterations:  " + MAX_ITERATIONS);
				Console.WriteLine();
			}

			// Give some statistics
			Console.WriteLine("Ran for {0} iterations", iteration);
			Console.WriteLine("Total Solutions found: " + solutionsFound);

			int taskCount = 0;

			/** Print solution **/
			Console.WriteLine();
			Console.WriteLine("Best Solution Found:");
			foreach (var entry in bestSolution)
				foreach (Task task in entry.Value)
				{
					taskCount++;
					Console.WriteLine("Task id " + task.getId() + " mcp id " + entry.Key.getMcp() + " core id " + entry.Key.getId() + " WCRT " + task.getWCRT());
				}

			Console.WriteLine();
			Console.WriteLine("Total Laxity: ");
			Console.WriteLine(bestLaxity);

			// Give some more statistics for testing purposes.
			Console.WriteLine();
			Console.WriteLine("Total Number of Tasks in Sample:   " + tasks.Count());
			Console.WriteLine("Total Number of Tasks in Solution: " + taskCount);

			// Asking and printing of an xml of the solution
			printXML(map, laxity);

			Console.ReadLine();
		}
	}
}