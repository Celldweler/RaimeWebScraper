using System;

namespace RaimeWebScraper
{
    public class CommandMenu
    {
        public void Run()
        {
            Console.WriteLine("welcome to tricktionary scraper! enter h to see all commands");
            string cmd = string.Empty;
            
            while (cmd != "exit")
            {
                Console.WriteLine(
                    "[1] Get All Tricks Category \n" + 
                    "[2] Get All Tricks Category With Sub \n" + 
                    "[3] Get Tricks By Category \n" + 
                    "[4] Get All Tricks \n" 
                    );
                cmd = Console.ReadLine();
            }

            Console.WriteLine("program finished work with status OK");                
        }
    }

    public class Command
    {
        
    }
}