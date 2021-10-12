using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Xml;
using Vertex = System.String;

namespace Project
{
    class Message
    {
        public String Name { get; }
        public String Source { get; }
        public String Destination { get; }
        public uint Size { get; }
        public uint Period { get; }
        public uint Deadline { get; }
        public Message(String name, String source, String destination, uint size, uint period, uint deadline)
        {
            this.Name = name;
            this.Source = source;
            this.Destination = destination;
            this.Size = size;
            this.Period = period;
            this.Deadline = deadline;
        }
    }

    class Edge
    {
        public String Id { get; }
        public uint BW { get; }
        public uint PropDelay { get; }
        String Source { get; }
        String Destination { get; }

        public Edge(String Id, uint BW, uint PropDelay, String Source, String Destination)
        {
            this.Id = Id;
            this.BW = BW;
            this.PropDelay = PropDelay;
            this.Source = Source;
            this.Destination = Destination;
        }
    }
 
    

    class Program
    {
        public static List<Message> ParseMessageXml(String path)
        {
            List<Message> messages = new();
            messages.Clear();

            XmlDocument doc = new();

            doc.Load(path);

            XmlNode applicationNode = doc.SelectSingleNode("//Application");

            XmlNodeList messageNodes = applicationNode.SelectNodes(".//Message");

            foreach (XmlNode messageNode in messageNodes)
            {
                String name = messageNode.Attributes["Name"].Value;
                String source = messageNode.Attributes["Source"].Value;
                String destination = messageNode.Attributes["Destination"].Value;
                uint size = uint.Parse(messageNode.Attributes["Size"].Value);
                uint period = uint.Parse(messageNode.Attributes["Period"].Value);
                uint deadline = uint.Parse(messageNode.Attributes["Deadline"].Value);

                messages.Add(new Message(name, source, destination, size, period, deadline));
            }


            return messages;
        }

        public static (List<Vertex>, List<Edge>) ParseArchiteure(String path)
        {
            List<Vertex> vertices = new List<Vertex>();
            vertices.Clear();

            List<Edge> edges = new();
            edges.Clear();

            XmlDocument doc = new();

            doc.Load(path);

            XmlNode architectureNode = doc.SelectSingleNode("//Architecture");

            XmlNodeList vertexNodes = architectureNode.SelectNodes(".//Vertex");

            foreach (XmlNode vertexNode in vertexNodes)
            {
                vertices.Add(vertexNode.Attributes["Name"].Value);
            }

            XmlNodeList edgeNodes = architectureNode.SelectNodes(".//Edge");

            foreach (XmlNode edgeNode in edgeNodes)
            {
                String id = edgeNode.Attributes["Id"].Value;
                uint BW = uint.Parse(edgeNode.Attributes["BW"].Value);
                uint propDelay = uint.Parse(edgeNode.Attributes["PropDelay"].Value);
                String source = edgeNode.Attributes["Source"].Value;
                String destination = edgeNode.Attributes["Destination"].Value;

                edges.Add(new Edge(id, BW, propDelay, source, destination));
            }

            return (vertices, edges);
        }

        static void Main(string[] args)
        {
            List<Message> messages = ParseMessageXml("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Apps.xml");
            (List<Vertex> vertices, List<Edge> edges) = ParseArchiteure("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Config.xml");
        }
    }
}
