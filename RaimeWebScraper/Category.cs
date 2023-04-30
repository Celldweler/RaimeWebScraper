using System.Collections.Generic;

namespace RaimeWebScraper
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string ParentCategoryId { get; set; } = null;
        public Category ParentCategory { get; set; } = null;

        public List<string> SubCategoriesVm { get; set; } = new List<string>();
        public List<Category> SubCategories { get; set; } = new List<Category>();
    }
}