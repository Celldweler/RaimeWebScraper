using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace RaimeWebScraper.Utilis
{
    public class SqlScriptGenerator
    {
        private FileManager _fileManager;

        private const string sqlScriptsDir =
            @"C:\Users\Raime\source\repos\RaimeWebScraper\RaimeWebScraper\Utilis\sql-scripts\";

        private const string sqlScriptFileName = "populate-categories-table.sql";

        public SqlScriptGenerator(FileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public SqlScriptGenerator()
        {
            _fileManager = new FileManager();
        }

        public void ExecuteSqlScript(string sqlCommand, Dictionary<string, string> paramsWithValues)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = @"(localdb)\MSSQLLocalDB";
                builder.InitialCatalog = "TrickingLibDbPureSql";
                builder.TrustServerCertificate = true;

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = sqlCommand;
                    command.CommandType = CommandType.Text;

                    foreach (var param in paramsWithValues)
                    {
                        if (param.Value != null)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue(param.Key, DBNull.Value);
                        }
                    }

                    var result = command.ExecuteNonQuery();

                    if (result > 0)
                        Console.WriteLine("categories table successful populated!");
                    else
                        Console.WriteLine("error during executing table populating sql query");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public class SqlCommandResult
        {
            public string CommandText { get; set; }
            public Dictionary<string, string> ParamsWithValues { get; set; }
        }

        public struct Tables
        {
            public const string Tricks = nameof(Tricks);
            public const string TrickRelationship = nameof(TrickRelationship);
            public const string TrickCategories = nameof(TrickCategories);
        }

        public SqlCommandResult CreateSqlScriptForPopulateTricks()
        {
            if (File.Exists($"{sqlScriptsDir}populate_tricks-trickRelationship-trickCategory.sql"))
            {
                Console.Write("File already existed are u sure to delete them and create new? (y/n)>>");

                if (Console.ReadLine() != "y")
                    return null;
            }

            var tricks = _fileManager.LoadTricksFromFile();

            var sqlQueryBuilder = new StringBuilder();
            var tricksSqlQueryBuilder = new StringBuilder();
            var tricksCategoriesSqlQueryBuilder = new StringBuilder();
            var tricksRelationshipsSqlQueryBuilder = new StringBuilder();

            sqlQueryBuilder.Append($"INSERT INTO {Tables.Tricks} ([Id], [Name])");
            sqlQueryBuilder.AppendLine();
            sqlQueryBuilder.Append($"VALUES ");

            // trick-categories
            tricksCategoriesSqlQueryBuilder.Append($"INSERT INTO {Tables.TrickCategories} ([TrickId], [CategoryId])");
            tricksCategoriesSqlQueryBuilder.AppendLine();
            tricksCategoriesSqlQueryBuilder.Append($"VALUES ");

            // trick-relationship
            tricksRelationshipsSqlQueryBuilder.Append(
                $"INSERT INTO {Tables.TrickRelationship} ([PrerequisiteId], [ProgresionId])");
            tricksRelationshipsSqlQueryBuilder.AppendLine();
            tricksRelationshipsSqlQueryBuilder.Append($"VALUES ");

            var counter = 1;
            var additionalCounterForPrereq = 1;
            var paramsWithValues = new Dictionary<string, string>();
            var paramsWithValuesForTrickCategories = new Dictionary<string, string>();
            var paramsWithValuesForTrickRelationships = new Dictionary<string, string>();
            var columnCounter = 1;
            foreach (var trick in tricks)
            {
                sqlQueryBuilder.Append($"( @id{counter}, @name{counter} ), ");
                sqlQueryBuilder.AppendLine();

                paramsWithValues.Add($"@id{counter}", trick.Id);

                // paramsWithValues.Add($"@name{counter}", trick.Name);

                // multiple categories exactly 2 parent and sub
                // tricksCategoriesSqlQueryBuilder.Append($"( @id{counter}, @parentCategoryId{counter} ),");
                // tricksCategoriesSqlQueryBuilder.Append($"( @id{counter}, @subCategoryId{counter} ),");
                // tricksCategoriesSqlQueryBuilder.AppendLine();

                // var categoryName = trick.Categories.First();
                // var fullCategoryName = categoryName == "vertical-kick" ? $"{categoryName}s" : $"{categoryName}-tricks";

                // paramsWithValues.Add($"@parentCategoryId{counter}", fullCategoryName);
                // paramsWithValues.Add($"@subCategoryId{counter}", trick.Categories.Last());

                // prerequisites none or 1 or more one 

                // check are trick have any prerequisites
                if (trick.Prerequisites != null)
                {
                    foreach (var prereq in trick.Prerequisites)
                    {
                        // if (!paramsWithValues.ContainsValue(prereq))
                        // {
                        tricksRelationshipsSqlQueryBuilder.Append(
                            $"( @prerequisiteId{additionalCounterForPrereq}, @id{counter} ), ");
                        if (columnCounter == 5)
                        {
                            tricksRelationshipsSqlQueryBuilder.AppendLine();
                            columnCounter = 1;
                        }

                        columnCounter++;
                        paramsWithValues.Add($"@prerequisiteId{additionalCounterForPrereq}", prereq);
                        additionalCounterForPrereq++;
                        // }
                    }
                }

                counter++;
            }

            var tr = tricksRelationshipsSqlQueryBuilder.ToString();
            tr = tr.Remove(tr.Length - 2);

            var populateTricksQuery = sqlQueryBuilder.ToString();
            populateTricksQuery = populateTricksQuery.Remove(populateTricksQuery.Length - 3);
            var tc = tricksCategoriesSqlQueryBuilder.ToString();
            tc = tc.Remove(tc.Length - 3);


            // Console.WriteLine(populateTricksQuery);
            //
            // Console.ForegroundColor = ConsoleColor.Green;
            // Console.WriteLine("TrickCategories: " + tc);
            //
            // Console.ForegroundColor = ConsoleColor.Red;
            // Console.WriteLine("TrickRelationship:  " + tr);
            //
            // Console.ResetColor();

            var declareParametersWithValuesStr = new StringBuilder();
            declareParametersWithValuesStr.Append("DECLARE ");
            columnCounter = 1;
            foreach (var paramsWithValue in paramsWithValues)
            {
                declareParametersWithValuesStr.Append(
                    $"{paramsWithValue.Key} NVARCHAR(100) = '{paramsWithValue.Value}', ");
                if (columnCounter == 4)
                {
                    declareParametersWithValuesStr.AppendLine();
                    columnCounter = 1;
                }

                columnCounter++;
            }

            // File.WriteAllText($"{sqlScriptsDir}NUM_2_DECLARE-VARS-WITHOUT-TRICK-CATEGORIES.sql",
            //     declareParametersWithValuesStr.ToString());
            // File.WriteAllText($"{sqlScriptsDir}populate-dbo_tricks-relationship.sql", tr);
            
            File.WriteAllText($"{sqlScriptsDir}NUM_3_DECLARE-VARS-WITHOUT-TRICK-CATEGORIES.sql",
                declareParametersWithValuesStr.ToString());
            File.WriteAllText($"{sqlScriptsDir}num-3-populate-dbo_tricks-relationship.sql", tr);

            // Console.WriteLine(declareParametersWithValuesStr.ToString());
            // File.WriteAllText($"{sqlScriptsDir}populate-dbo_tricks.sql", populateTricksQuery);
            // File.WriteAllText($"{sqlScriptsDir}populate-dbo_tricks-categories.sql", tc);
            //
            return new SqlCommandResult
            {
            };
        }

        public SqlCommandResult CreateSqlScriptForPopulateCategories()
        {
            if (_fileManager == null) return null;

            if (File.Exists($"{sqlScriptsDir}{sqlScriptFileName}"))
            {
                Console.Write("File alre4ady existed are u sure to delete them and create new? (y/n)>>");

                if (Console.ReadLine() != "y")
                    return null;
            }

            var categories = _fileManager.LoadCategoriesFromFile();

            var sqlQueryBuilder = new StringBuilder();

            sqlQueryBuilder.Append("INSERT INTO [TrickingLibDbPureSql].[dbo].[Categories] ");
            sqlQueryBuilder.AppendLine();
            sqlQueryBuilder.Append("([Id], [Name], [ParentId])");
            sqlQueryBuilder.AppendLine();
            sqlQueryBuilder.Append("VALUES ");

            var counter = 1;
            Dictionary<string, string> paramsWithValues = new Dictionary<string, string>();
            foreach (var category in categories)
            {
                paramsWithValues.Add($"Id{counter}", category.Id);
                paramsWithValues.Add($"Name{counter}", category.Name);
                paramsWithValues.Add($"ParentId{counter}", category.ParentCategoryId);

                sqlQueryBuilder.Append($"(@Id{counter}, @Name{counter}, @ParentId{counter}),");
                sqlQueryBuilder.AppendLine();

                counter++;
            }

            var sql = sqlQueryBuilder.ToString();
            sql = sql.Remove(sql.Length - 3);

            File.WriteAllText($"{sqlScriptsDir}{sqlScriptFileName}", sql);

            return new SqlCommandResult { CommandText = sql, ParamsWithValues = paramsWithValues };
        }
    }
}