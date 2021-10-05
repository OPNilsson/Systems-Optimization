using Google.OrTools.Sat;
using System;
using System.Collections.Generic;

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
        Message(String name, String source, String destination, uint size, uint period, uint deadline)
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
        public static List<Message> Load_xml(String path)
        {
            List<Message> messages = new List<Message>();

            return messages;
        }

        static void Main(string[] args)
        {
            CpModel model = new CpModel();
            Console.WriteLine("Hello World!");
        }
    }
}
