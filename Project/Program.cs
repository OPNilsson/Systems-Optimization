using Google.OrTools.Sat;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Project
{
    class Message
    {
        public String Name { get; set; }
        public String Source { get; set; }
        public String Destination { get; set; }
        public uint Size { set; get; }
        public uint Period { get; set; }
        public uint Deadline { get; set; }
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

    class Program
    {
        public static List<Message> ParseMessageXml(String path)
        {
            List<Message> messages = new List<Message>();
            messages.Clear();

            XmlDocument doc = new XmlDocument();

            doc.Load(path);

            var applicationNode = doc.SelectSingleNode("//Application");

            var messageNodes = applicationNode.SelectNodes(".//Message");

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

        static void Main(string[] args)
        {
            List<Message> messages = ParseMessageXml("..\\..\\..\\..\\..\\test_cases\\Small\\TC1\\Input\\Apps.xml");
        }
    }
}
