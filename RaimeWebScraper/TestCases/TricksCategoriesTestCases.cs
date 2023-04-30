using System;

namespace RaimeWebScraper.TestCases
{
    public class TricksCategoriesTestCases
    {
        // показать все главные категории которые скрапер должен  извлечь из сайта
        public void Test1_Show_All_Scrapped_Categories()
        {
            Console.WriteLine("All Main Tricks Categories");
            Console.WriteLine(
                "Vertical Kicks"   + "\n" +
                 "Backward Tricks" + "\n" +
                 "Forward Tricks"  + "\n" +
                 "Inside Tricks"   + "\n" +
                 "Outside Tricks"
                );
        }
    }
}