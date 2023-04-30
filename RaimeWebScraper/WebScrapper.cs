using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using RaimeWebScraper.Utilis;

namespace RaimeWebScraper
{
    public class WebScrapper
    {
        /// <summary>
        /// root route
        /// </summary>
        private const string ROOT_URL = "https://www.loopkickstricking.com";
        
        /// <summary>
        /// route to list all tricks
        /// sample: https://www.loopkickstricking.com/tricktionary/explore
        /// </summary>
        private const string EXPLORE_SUB_ROOT = "/tricktionary/explore";
        /// <summary>
        /// route to concrate trick by category>>subCategory
        /// sample: https://www.loopkickstricking.com/tricks/pop-360-shuriken
        /// </summary>
        private const string _routToTrickPage = "https://www.loopkickstricking.com/tricks/{trickName}";

        // private const string FORWARD_TRICK_URL = "https://www.loopkickstricking.com/tricktionary/forward-tricks";
        // private const string FORWARD_TRICK_URL = "https://www.loopkickstricking.com/tricktionary/backward-tricks";
        // private const string FORWARD_TRICK_URL = "https://www.loopkickstricking.com/tricktionary/vertical-kicks";
        private const string FORWARD_TRICK_URL = "https://www.loopkickstricking.com/tricktionary/inside-tricks";
        // private const string FORWARD_TRICK_URL = "https://www.loopkickstricking.com/tricktionary/outside-tricks";

        // https://www.loopkickstricking.com/tricks/trickName

        private HtmlDocument _htmlDocument;

        public WebScrapper()
        {
            _htmlDocument = new HtmlDocument();
        }
        private List<string> SelectCategoryCrumbNodeFromTrickPage(string breadCrumbNode)
        {
            var categories = new List<string>();
            var breadCrumbList = breadCrumbNode.Split("&gt;")
                .Select(x => x.TrimStart())
                .Select(y => y.TrimEnd())
                .Select(q => q.ToUpper())
                .Select(s => s.CreateID())
                .ToArray();

            categories.AddRange(new[] { breadCrumbList[1], breadCrumbList[2] });
            return categories;
        }

        private List<string> SelectPrerequisitesNodeFromTrickPage(string prerequisites)
        {
            var categories = new List<string>();
            // var breadCrumbList = breadCrumbNode.Split("&gt;")
            //     .Select(x => x.TrimStart())
            //     .Select(y => y.TrimEnd())
            //     .ToArray();
            //
            // categories.AddRange(new []{ breadCrumbList[1], breadCrumbList[2]});
            return categories;
        }

        public async Task<List<Trick>> GetTricks(string htmlParam = "", bool isSaveToFile = false)
        {
            var tricks = new List<Trick>();
            Console.WriteLine("Scraper work!");
            Console.WriteLine("awaiting result...");

            var htmlDoc = new HtmlDocument();
            if (string.IsNullOrEmpty(htmlParam))
            {
                var html = await CallUrl(FORWARD_TRICK_URL);
                htmlDoc.LoadHtml(html);
            }
            else
            {
                htmlDoc.LoadHtml(htmlParam);
            }

            // var xpath = 
            //     "//div[@class='all-content w-tab-pane w--tab-active']/div[@class='w-dyn-list']/div[@class='related-grid w-dyn-items']/div[@class='w-dyn-item']";
            var nodeWithClassWTabContent =  htmlDoc.DocumentNode.SelectSingleNode("//div[@class='pseudo-filter w-tabs']").ChildNodes.Last();
            var nodeAllContent = nodeWithClassWTabContent.ChildNodes.First();
            var wDynListNode = nodeAllContent.ChildNodes.First();
            var relatedGridWDynListItemsNode = wDynListNode.ChildNodes.First();
                
            var listItemTrickNodeCollection = relatedGridWDynListItemsNode.ChildNodes;
            var countIteration = 0;
            foreach (var itemTrickNode in listItemTrickNodeCollection)
            {
                countIteration++;
                var aNode = itemTrickNode.ChildNodes["a"];
                var routeToTrick = aNode.Attributes["href"].Value;

                // https://www.loopkickstricking.com/{routeToTrick} | routeToTrick='tricks/trickName'
                var trickPageHtmlResult = await CallUrl(ROOT_URL + routeToTrick);

                var trickPageHtmlDoc = new HtmlDocument();
                trickPageHtmlDoc.LoadHtml(trickPageHtmlResult);

                var trickName = trickPageHtmlDoc.DocumentNode.SelectSingleNode("//h1[@class='heading-8']").InnerText;
                Console.WriteLine(trickName);

                var breadCrumbNode = trickPageHtmlDoc.DocumentNode.SelectSingleNode("//div[@class='breadcrumb']");
                // Console.WriteLine(breadCrumbNode.InnerText);
                var categories = SelectCategoryCrumbNodeFromTrickPage(breadCrumbNode.InnerText);
                var desc = trickPageHtmlDoc.DocumentNode
                    .SelectSingleNode("//p[@class='paragraph-5']")
                    .InnerText.TrimStart().TrimEnd()
                    .Replace("\n", " ");

                // Console.WriteLine(desc);

                var prerequisites = trickPageHtmlDoc.DocumentNode
                    .SelectNodes("//div[@class='prereq-wrapper']")
                    .FirstOrDefault();

                var isHavePrereq = prerequisites.ChildNodes["div"].Attributes["class"].Value
                    .Contains("w-condition-invisible");

                var prerequisiteList = new List<string>();
                if (!isHavePrereq)
                {
                    // sample:  prerequisites.InnerText = 'Prerequisites: prereq1, prereq2, etc'
                    var innerHtmlWithSecondPart =
                        prerequisites.InnerText.Split("Prerequisites:")[1].TrimStart().TrimEnd();

                    if (innerHtmlWithSecondPart.Contains(','))
                    {
                        var prereqsSplited = innerHtmlWithSecondPart.Split(',');

                        foreach (var p in prereqsSplited)
                        {
                            prerequisiteList.Add(p.TrimStart().TrimEnd().CreateID());
                        }
                    }
                    else
                        prerequisiteList.Add(innerHtmlWithSecondPart.CreateID());

                    // Console.WriteLine("Prerequisites: " + innerHtmlWithSecondPart);
                }


                tricks.Add(new Trick
                {
                    Id = trickName.CreateID(),
                    Name = trickName,
                    Description = desc,
                    Categories = new List<string>(categories),
                    Prerequisites = new List<string>(prerequisiteList),
                });
            }

            Console.WriteLine("count iteration: " + countIteration);
            Console.WriteLine("finished!");

            if (isSaveToFile)
            {
                var fileWithTricksPath = @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\data\backward-inside-tricks.txt";
                if (File.Exists(fileWithTricksPath))
                {
                    // append tricks to end file
                    new FileManager().SaveTricksToFile(tricks, "append");
                }
                else
                {
                    // create file and write
                    new FileManager().SaveTricksToFile(tricks, "create");
                }
            }

            return tricks;
        }

        public async Task<List<Trick>> SelectTricksFromMultiplePage(HtmlDocument doc, string _html, string _url)
        {
            var _tricks = new List<Trick>();
            var tempTricks = new List<Trick>();

            var countPage = 1;
            var pagNode = doc.DocumentNode.SelectSingleNode("//a[@class='w-pagination-next']");
            do
            {
                var pageValueQuery = pagNode.Attributes["href"].Value;
                

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Page {countPage} - {pageValueQuery}");
                Console.ResetColor();

                tempTricks = await new WebScrapper().GetTricks(_html, true);

                // after select all tricks in page
                _html = await CallUrl($"{_url}{pageValueQuery}");
                doc.LoadHtml(_html);
                pagNode = doc.DocumentNode.SelectSingleNode("//a[@class='w-pagination-next']");
                if (pagNode == null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Page {++countPage}");
                    Console.ResetColor();

                    tempTricks = await new WebScrapper().GetTricks(_html, true);
                }

                //_tricks.AddRange(tempTricks);
                countPage++;
            } while (pagNode != null);
           
            return _tricks;
        }

        public async Task StartScrapTrickFrom(string _url)
        {
            var _tricks = new List<Trick>();

            // var _url = 
            //     "https://www.loopkickstricking.com/tricktionary/explore";
                // "https://www.loopkickstricking.com/tricktionary/inside-tricks";
            
         // var rootUrl = "https://www.loopkickstricking.com/tricktionary/";
            // foreach (var category in routeCategories)
            // {
            // var _html = await CallUrl(rootUrl + category);
            // Console.WriteLine($"{category}: ");
            
            var _html = await CallUrl(_url);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(_html);

            var res = await new WebScrapper().CheckIsHaveMultiplePage(doc);
            if (res)
            {
                _tricks =  await SelectTricksFromMultiplePage(doc, _html, _url);
            }
            else
            {
                _tricks = await new WebScrapper().GetTricks(_html);
            }
        }
        private static async Task<string> CallUrl(string fullUrl)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(fullUrl);
            return response;
        }

        public async Task<bool> CheckIsHaveMultiplePage(HtmlDocument htmlDoc)
        {
            // "//div[@class='breadcrumb']"
            var pagNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='w-pagination-wrapper']");

            if (pagNode != null && pagNode?.InnerText == "Next")
            {
                return true;
            }

            return false;
        }

        private string ParseSubCategoryNameString(string target)
        {
            var parsedString = target.Replace("&amp;", string.Empty)
                .Replace("  ", " ");
            var charArray = parsedString.Where(c => char.IsLetter(c) || c == ',' || c.Equals(' ')).ToArray();

            string normalizedCategoryName = new string(charArray);
            return normalizedCategoryName;
        }

        private List<string> SplitSubStringToListSub(string target)
        {
            var splitedByComma = target.Split(',')
                .Select(x => x.TrimStart())
                .Select(y => y.TrimEnd())
                .ToArray();

            if (splitedByComma[1].StartsWith("Webster"))
            {
               
            }
            
            var subList = new List<string>();
            foreach (var s in splitedByComma)
            {
                subList.Add(s);
            }
            return subList;
        }

        private Category CreateCategory(string name, List<string> subs)
        {
            return new Category
            {
                Id = name.CreateID(),
                Name = name.TrimStart().TrimEnd(),
                ParentCategoryId = null,
                SubCategoriesVm = new List<string>(subs)
            };
        }
        public async Task<List<Category>> SelectCategoriesWithSub()
        {
            var mainCategories = new List<Category>();

            var categoryPageHtmlDocument = new HtmlDocument();
            var html = await CallUrl($"{ROOT_URL}/tricktionary");
            categoryPageHtmlDocument.LoadHtml(html);

            var xpathToCategoriesGridNode = "//div[@class='w-layout-grid grid-17']";
            var categoriesGridNode = categoryPageHtmlDocument.DocumentNode.SelectSingleNode(xpathToCategoriesGridNode);

            var categoryItemsNodeCollection = categoriesGridNode.ChildNodes;

            foreach (var categoryItemNode in categoryItemsNodeCollection)
            {
                var categoryName = categoryItemNode.ChildNodes["h2"].InnerText;
                var subs = ParseSubCategoryNameString(categoryItemNode.ChildNodes["p"].InnerText);
                var subList = SplitSubStringToListSub(subs);
                
                var category = CreateCategory(categoryName, subList);
                mainCategories.Add(category);
                
                // Console.WriteLine($"{categoryName}: \n\t{subs}");
            }
            
            return mainCategories;
        }
    }
}