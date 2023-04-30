using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RaimeWebScraper.Utilis
{
    public interface IFileManager
    {
        void SaveCategoriesToFile(List<Category> categories);
        void SaveTricksToFile(List<Trick> tricks, string mode, string fName = null);
        List<Trick> LoadTricksFromFile(string fName = null);
        List<Category> LoadCategoriesFromFile();
    }

    public class FileManager : IFileManager
    {
        private string _saveDataPath = @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\data";
        private string categoriesFileName = "categories.txt";
        private string tricksFileName = "tricks.txt";

        public void SaveCategoriesToFile(List<Category> categories)
        {
            if (File.Exists($"{_saveDataPath}/{categoriesFileName}"))
            {
                Console.WriteLine($"file: {_saveDataPath}\\{categoriesFileName} already exists");
                return;
            }

            var stringSerializeCategoriesBuilder = new StringBuilder();

            foreach (var parentCategory in categories)
            {
                var parentId = parentCategory.Id;

                stringSerializeCategoriesBuilder.Append($"id={parentCategory.Id};");
                stringSerializeCategoriesBuilder.Append($"name={parentCategory.Name};");
                stringSerializeCategoriesBuilder.Append($"parent_id=none");
                stringSerializeCategoriesBuilder.Append(Environment.NewLine);

                Console.WriteLine(stringSerializeCategoriesBuilder.ToString());

                if (parentCategory.SubCategoriesVm != null || parentCategory.SubCategoriesVm.Count > 0)
                {
                    foreach (var sub in parentCategory.SubCategoriesVm)
                    {
                        stringSerializeCategoriesBuilder.Append($"id={sub.CreateID()};");
                        stringSerializeCategoriesBuilder.Append($"name={sub};");
                        stringSerializeCategoriesBuilder.Append($"parent_id={parentId}");
                        stringSerializeCategoriesBuilder.Append(Environment.NewLine);
                    }
                }
            }

            try
            {
                File.WriteAllText($"{_saveDataPath}\\{categoriesFileName}",
                    stringSerializeCategoriesBuilder.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public enum FileMode
        {
            Create,
            Append
        }

        private string TrickToString(Trick t) =>
            $"Id={t.Id};Name={t.Name};Categories={t.CategoriesStrList};Prerequisites={t.PrerequisitesStrList}";

        public void SaveTricksToFile(List<Trick> tricks, string mode, string fName = null)
        {
            if (fName == null)
            {
                if (File.Exists($"{_saveDataPath}\\{tricksFileName}"))
                {
                    Console.WriteLine($"file: {_saveDataPath}\\{tricksFileName} already exists");
                    //return;
                }
            }

            var stringSerializeTricksBuilder = new StringBuilder();

            foreach (var trick in tricks)
            {
                var trickStr = TrickToString(trick);
                stringSerializeTricksBuilder.Append(trickStr);
                stringSerializeTricksBuilder.AppendLine();
            }

            var tricksSerializedToString = stringSerializeTricksBuilder.ToString();
            try
            {
                if (fName == null)
                {
                    if (mode == "create")
                        File.WriteAllText($"{_saveDataPath}\\{tricksFileName}", tricksSerializedToString);

                    else if (mode == "append")
                    {
                        File.AppendAllText($"{_saveDataPath}\\{tricksFileName}", tricksSerializedToString);
                    }
                    else
                    {
                        Console.WriteLine("uncorect mode file open");
                    }
                }
                else
                {
                    if (mode == "create")
                        File.WriteAllText($"{_saveDataPath}\\{fName}", tricksSerializedToString);

                    else if (mode == "append")
                    {
                        File.AppendAllText($"{_saveDataPath}\\{fName}", tricksSerializedToString);
                    }
                    else
                    {
                        Console.WriteLine("uncorect mode file open");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public List<Trick> LoadTricksFromFile(string fName = null)
        {
            // Id=dive-roll;Name=Dive Roll;Categories=forward,frontflip;Prerequisites=none
            // Id=webster-half;Name=Webster Half;Categories=forward,webster;Prerequisites=front-half
            // Id=webster;Name=Webster;Categories=forward,webster;Prerequisites=front-tuck,aerial
            var tricks = new List<Trick>();

            string[] tricksToStrSerialized = Array.Empty<string>();
            if (fName == null)
            {
                tricksToStrSerialized = File.ReadAllLines($"{_saveDataPath}\\tricks.txt");
            }
            else
            {
                tricksToStrSerialized = File.ReadAllLines($"{_saveDataPath}\\{fName}");
            }

            foreach (var item in tricksToStrSerialized)
            {
                var keyValuesString = item.Split(';');
                var categories = keyValuesString[2].Split('=')[1].Split(',').ToList();
                var parsedValuePrerequisites = keyValuesString[3].Split('=')[1];
                var prerequisites = parsedValuePrerequisites != "none"
                    ? parsedValuePrerequisites
                        .Split(',').ToList()
                    : null;

                tricks.Add(new Trick
                {
                    Id = keyValuesString[0].Split('=')[1],
                    Name = keyValuesString[1].Split('=')[1],
                    Categories = categories,
                    Prerequisites = prerequisites
                });
            }

            Console.WriteLine(tricks.Count);
            Console.WriteLine(tricks[2].ToString());
            return tricks;
        }

        private string IfParentIdNoneReturnNull(string parentId) => parentId == "none" ? null : parentId;

        public List<Category> LoadCategoriesFromFile()
        {
            var categories = new List<Category>();

            var stringSerializeCategories = File.ReadAllLines($"{_saveDataPath}\\{categoriesFileName}");

            /*
             id=backward-tricks;name=Backward Tricks;parent_id=none
             id=backflip;name=Backflip;parent_id=backward-tricks
             */
            foreach (var category in stringSerializeCategories)
            {
                var temp = category.Split(';');
                var parentId = temp[2].Split('=')[1];

                categories.Add(new Category
                {
                    Id = temp[0].Split('=')[1],
                    Name = temp[1].Split('=')[1],
                    ParentCategoryId = IfParentIdNoneReturnNull(parentId),
                });
            }

            return categories;
        }
    }
}