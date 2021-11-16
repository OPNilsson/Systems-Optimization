using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    internal class SimulatedAnnealing
    {
        private List<Edge> edges;
        private Dictionary<Message, List<List<Edge>>> message_routes;
        private List<Message> messages;
        private List<Vertex> vertices;

        public SimulatedAnnealing(List<Message> messages, List<Vertex> vertices, List<Edge> edges, Dictionary<Message, List<List<Edge>>> message_routes)
        {
            this.messages = messages;
            this.vertices = vertices;
            this.edges = edges;
            this.message_routes = message_routes;
        }

        public static void printPath(List<Edge> path)
        {
            Console.Write(path[0].Source + ", " + path[0].Destination);
            for (int i = 1; i < path.Count; i++)
            {
                Console.Write(", " + path[i].Destination);
            }
            Console.WriteLine();
        }

        public void Solve()
        {
            foreach (KeyValuePair<Message, List<List<Edge>>> entry in message_routes)
            {
                Console.WriteLine(); // Here for formatting
                Console.WriteLine("Showing Possible Routes for " + entry.Key.Name);

                foreach (List<Edge> route in entry.Value)
                {
                    printPath(route);
                }
            }
        }
    }
}