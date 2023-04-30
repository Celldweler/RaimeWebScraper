using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using RaimeWebScraper.TestCases;
using RaimeWebScraper.Utilis;

namespace RaimeWebScraper
{
    class Program
    {
        public static void CountTricksWithoutPrerequisites()
        {
            var tricks = new FileManager().LoadTricksFromFile("bacup-tricks.txt");

            var counter = 0;
            foreach (var t in tricks)
            {
                if (t.Prerequisites == null)
                    counter++;
            }

            Console.WriteLine("count tricks: " + tricks.Count);
            Console.WriteLine("Count Tricks Without Prerequisites: " + counter);
        }

        private static List<Trick> SelectTricksFromString(string selrialisedTricksInString)
        {
            var tricks = new List<Trick>();

            var listStings = selrialisedTricksInString.Split("\n");
            Console.WriteLine(listStings.Length);

            foreach (var item in listStings)
            {
                var splited = item.Split(";");
                var prereqs = splited[1].Split("=")[1];
                prereqs = prereqs == "none" ? null : prereqs;
                var listPrereq = new List<string>();
                if (prereqs != null)
                {
                    if (prereqs.Contains(","))
                    {
                        listPrereq = prereqs.Split(",").ToList();
                    }
                    else
                        listPrereq.Add(prereqs);
                }
                else
                {
                    listPrereq = null;
                }

                tricks.Add(new Trick
                {
                    Id = splited[0].Split("=")[1],
                    Prerequisites = listPrereq,
                });
            }

            return tricks;
        }

        public static void RemoveAllPrerequisitesWhichDoesNotExistInFileTricksTxt()
        {
            // for test puropose starting with first 10 records
            // Id=dive-roll;Name=Dive Roll;Categories=forward,frontflip;Prerequisites=none
            // Id=webster-half;Name=Webster Half;Categories=forward,webster;Prerequisites=front-half
            // Id=webster;Name=Webster;Categories=forward,webster;Prerequisites=front-tuck,aerial
            // Id=webster-hyperhook;Name=Webster Hyperhook;Categories=forward,webster;Prerequisites=webster-half
            // Id=front-x-out;Name=Front X-Out;Categories=forward,frontflip;Prerequisites=front-tuck
            // Id=webster-axe;Name=Webster Axe;Categories=forward,webster;Prerequisites=webster
            // Id=janitor-flip;Name=Janitor Flip;Categories=forward,janitor;Prerequisites=butterfly-kick
            // Id=front-handspring;Name=Front Handspring;Categories=forward,frontflip;Prerequisites=none
            // Id=front-pike;Name=Front Pike;Categories=forward,frontflip;Prerequisites=front-tuck
            // Id=front-half;Name=Front Half;Categories=forward,frontflip;Prerequisites=front-tuck

            const string path = @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\data\tricks.txt";
            string tricksTestCase =
                "Id=fake-trick-1;Prerequisites=none" + "\n" +
                "Id=fake-trick-2;Prerequisites=fake-trick-1,fake-trick-3" + "\n" +
                "Id=fake-trick-3;Prerequisites=non-existent-trick,fake-trick-2" + "\n" +
                "Id=fake-trick-4;Prerequisites=fake-trick-5" + "\n" +
                "Id=fake-trick-5;Prerequisites=non-existent-trick";

            // var listExistentTricks = SelectTricksFromString(tricksTestCase);
            var listExistentTricks = new FileManager().LoadTricksFromFile("bacup-tricks.txt");
            var listAllPrerequisites = new List<string>();
            foreach (var trick in listExistentTricks)
            {
                if (trick.Prerequisites != null)
                {
                    listAllPrerequisites.AddRange(trick.Prerequisites);
                }
            }

            var hashSet = new HashSet<string>(listAllPrerequisites);
            var prereqsWithoutDuplicate = hashSet.ToList();
            Console.WriteLine($"Prereqs count without duplicate: {hashSet.Count}");

            Console.WriteLine($"Prereqs count: {listAllPrerequisites.Count}");

            // проверить есть ли в списке трбков трюки из списка пререквизитов

            var count = 0;
            foreach (var prereq in prereqsWithoutDuplicate)
            {
                if (!listExistentTricks.Exists(x => x.Id == prereq))
                {
                    foreach (var trick in listExistentTricks)
                    {
                        if (trick.Prerequisites != null)
                        {
                            if (trick.Prerequisites.Exists(y => y == prereq))
                            {
                                trick.Prerequisites.RemoveAll(x => x == prereq);
                                count++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"COUNT REMOVED PREREQUISITES: {count}");
            // var fName = "tricks-without-non-existent-prerequisites.txt";
            // new FileManager().SaveTricksToFile(listExistentTricks, "create");
            var test = 0;
        }

        private static string path = @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\data";

        public static string TrickToString(Trick t) =>
            $"Id={t.Id};Name={t.Name};Categories={t.CategoriesStrList};Prerequisites={t.PrerequisitesStrList}";


        public static void JoinAndMoveFile()
        {
            // var res = File.ReadAllText($"{path}\\forward-vertical-outside-tricks.txt");
            //
            // File.WriteAllText($"{path}\\tricks.txt", res);

            var res2 = File.ReadAllText($"{path}\\backward-inside-tricks.txt");

            File.AppendAllText($"{path}\\tricks.txt", res2);
        }

        static async Task Main(string[] args)
        {
            // create script for populatin trickCategories

            var tricks = new FileManager().LoadTricksFromFile("bacup-tricks.txt");
            var categories = new FileManager().LoadCategoriesFromFile();

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("INSERT INTO [TrickCategories] (TrickId, CategoryId)");
            sqlBuilder.AppendLine();
            sqlBuilder.Append("VALUES");
            sqlBuilder.AppendLine();

            var counter = 1;
            var declareVariables = new Dictionary<string, string>();
            foreach (var trick in tricks)
            {
                sqlBuilder.Append($"( @trickId{counter}, @categoryId{counter} ), ");
                sqlBuilder.Append($"( @trickId{counter}, @subCategoryId{counter} ), ");
                sqlBuilder.AppendLine();

                var _categories = trick.Categories;
                var parentCategoryID = _categories.First();
                var subCategoryID = _categories.Last();
                declareVariables.Add($"@trickId{counter}", trick.Id);
                declareVariables.Add($"@categoryId{counter}", parentCategoryID);
                declareVariables.Add($"@subCategoryId{counter}", subCategoryID);
                
                counter++;
            }

            var resultQuery = sqlBuilder.ToString();
            
            var resultVariables = new StringBuilder();
            resultVariables.Append("DECLARE ");
            foreach (var item in declareVariables)
            {
                resultVariables.Append($"{item.Key} NVARCHAR(100) = '{item.Value}', ");
                resultVariables.AppendLine();
            }

            resultVariables.Append(resultQuery);
            File.WriteAllText(
                @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\Utilis\sql-scripts\FillCategoryTricks.sql", resultVariables.ToString());
            return;
            // foreach (var s in splitedByComma)
            // {
            //     subList.Add(s);
            // }
            // var cat = await new WebScrapper().SelectCategoriesWithSub();
            // Print(cat);
            // RemoveAllPrerequisitesWhichDoesNotExistInFileTricksTxt();
            // return;
            // CountTricksWithoutPrerequisites();
            // JoinAndMoveFile();
            new SqlScriptGenerator().CreateSqlScriptForPopulateTricks();
            return;
            var _routeCategories = new string[]
            {
                "inside-tricks",
                "backward-tricks",

                //"forward-tricks",
                //"outside-tricks",
                //"vertical-kicks",
            };
            var routePath = "https://www.loopkickstricking.com/tricktionary/";

            // HtmlDocument doc = new HtmlDocument();
            // doc.LoadHtml(_html);

            //await new WebScrapper().GetTricks(_html);
            var webScraper = new WebScrapper();
            //var tempTricks = new List<Trick>();
            //var tricks = new List<Trick>();
            //var stringSerializeTricksBuilder = new StringBuilder();

            foreach (var routeCategory in _routeCategories)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(routeCategory + ": ");
                Console.ResetColor();

                var _html = await CallUrl($"{routePath}{routeCategory}");
                var doc = new HtmlDocument();
                doc.LoadHtml(_html);

                var res = await webScraper.CheckIsHaveMultiplePage(doc);

                if (res)
                {
                    Console.WriteLine("have multi pages");
                    await webScraper.SelectTricksFromMultiplePage(doc, _html, $"{routePath}{routeCategory}");
                }
                //else
                //{
                //    tempTricks = await webScraper.GetTricks(_html);
                //}

                //tricks.AddRange(tempTricks);

                //Console.ForegroundColor = ConsoleColor.Blue;
                //Console.WriteLine("count tricks: " + tricks.Count);
                //Console.ResetColor();


                //await new WebScrapper().StartScrapTrickFrom($"{routePath}{routeCategory}");
            }

            //foreach (var t in tricks)
            //{
            //    var trickStr = TrickToString(t);
            //    stringSerializeTricksBuilder.Append(trickStr);
            //    stringSerializeTricksBuilder.AppendLine();
            //}

            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine(stringSerializeTricksBuilder.ToString());
            //Console.ResetColor();
            //File.WriteAllText(@"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\data\forward-vertical-outside-tricks.txt",
            //    stringSerializeTricksBuilder.ToString());

            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine("count tricks: " + tricks.Count);
            //Console.ResetColor();
            // var categories =  await new WebScrapper().SelectCategoriesWithSub();
            // new FileManager().SaveCategoriesToFile(categories);
            // Print(categories);
            // new TricksCategoriesTestCases().Test1_Show_All_Scrapped_Categories();
            // new CommandMenu().Run();

            return;
        }

        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }

        private static Dictionary<string, string> GetGeneralCategoryFromHtml(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var desctopnav = htmlDoc.DocumentNode.Descendants("div")
                .Where(x =>
                {
                    var t = x.Attributes.FirstOrDefault(y => y.Name == "class");
                    if (t == null)
                        return false;

                    return t.Value == "desktop-nav";
                })
                .FirstOrDefault();

            var kVPairs = new Dictionary<string, string>();

            var sidebarLinks = desctopnav.Descendants("a")
                .Where(x =>
                {
                    var t = x.Attributes.FirstOrDefault(y => y.Name == "class");
                    if (t == null)
                        return false;

                    if (t.Value == "sidebar-link")
                    {
                        var hrefAttribute = x.Attributes.FirstOrDefault(y => y.Name == "href");
                        kVPairs.Add(hrefAttribute.Value, x.InnerHtml);

                        return true;
                    }

                    return false;
                }).ToList();

            var innerHtml = desctopnav.InnerHtml;
            var innerHtml2 = "";
            foreach (var v in sidebarLinks)
            {
                innerHtml2 += v.InnerHtml + Environment.NewLine;
            }

            return kVPairs;
        }

        public static List<string> GetSubCategory(string html)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var scrolling_menu = htmlDoc.DocumentNode.Descendants("div")
                .Where(x =>
                {
                    var t = x.Attributes.FirstOrDefault(y => y.Name == "class");
                    if (t == null)
                        return false;

                    return t.Value.Equals("scrolling-menu w-tab-menu");
                })
                .FirstOrDefault();

            var subCategories = scrolling_menu.Descendants("a");

            var list = new List<string>();
            foreach (var v in subCategories)
            {
                var t = v.Descendants("div");
                var sub = t.FirstOrDefault().InnerHtml;
                if (sub.StartsWith("All "))
                    continue;
                // Console.WriteLine("\t" + sub);
                list.Add(sub);
            }

            return list;
        }

        public static string GetPath() =>
            @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\bin\Debug\netcoreapp3.1\CATEGORIES.txt";

        public static List<Category> ReadFromFile()
        {
            var list = new List<Category>();

            string[] lines = File.ReadAllLines(GetPath());
            Dictionary<string, string> keyValuePairCollection = new Dictionary<string, string>();
            foreach (var l in lines)
            {
                var s = l.Split(";");
                foreach (var v in s)
                {
                    var kVPair = v.Split("=");
                    keyValuePairCollection.Add(kVPair[0], kVPair[1]);
                }

                var category = new Category();
                category.Id = keyValuePairCollection["Id"];
                category.Name = keyValuePairCollection["Name"];
                category.ParentCategoryId = (keyValuePairCollection["ParentCategoryId"] == "none"
                    ? null
                    : keyValuePairCollection["ParentCategoryId"]);

                keyValuePairCollection.Remove("Id");
                keyValuePairCollection.Remove("Name");
                keyValuePairCollection.Remove("ParentCategoryId");

                list.Add(category);
            }

            return list;
        }

        public static void SaveToFile(List<Category> categories)
        {
            var path =
                @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\bin\Debug\netcoreapp3.1\CATEGORIES.txt";

            if (File.Exists(path)) return;

            var stringCategories = "";
            foreach (var c in categories)
            {
                var parId = (c.ParentCategoryId == null ? "none" : c.ParentCategoryId);
                stringCategories += $"Id={c.Id};Name={c.Name};ParentCategoryId={parId}\n";
            }

            File.WriteAllText("CATEGORIES.txt", stringCategories);
        }

        public static void Print(List<Category> categories)
        {
            foreach (var c in categories)
            {
                Console.Write($"{c.Id} - {c.Name}: \n");
                // if (c.ParentCategoryId != null)
                //     Console.Write($" - {c.ParentCategoryId}\n");
                // else
                // {
                //     Console.WriteLine();
                // }
                foreach (var sub in c.SubCategoriesVm)
                {
                    Console.WriteLine($"\t{sub}");
                }
            }
        }

        static void PrintTricks(List<Trick> tricks)
        {
            if (tricks == null || tricks.Count == 0)
                return;

            foreach (var t in tricks)
            {
                Console.WriteLine($"{t.Id} - {t.Name}");
                Console.WriteLine("Categories: ");
                foreach (var c in t.Categories)
                {
                    Console.WriteLine($"\t{c}");
                }

                Console.WriteLine("Prerequisites: ");
                foreach (var prerequisite in t.Prerequisites)
                {
                    Console.WriteLine($"\t{prerequisite}");
                }

                Console.WriteLine("============================================================================");
            }
        }
    }
}