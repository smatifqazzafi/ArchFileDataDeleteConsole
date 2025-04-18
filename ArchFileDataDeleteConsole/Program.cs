using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using System.IO;

class Program
{
    static void Main()
    {
        // Read configuration from appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())  // Ensure correct path
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();



        // Fetch connection string
        string connectionString = config.GetConnectionString("DefaultConnection");
        int ConTimeout =Convert.ToInt32(config.GetConnectionString("ConnectionTimeOut"));
            Console.WriteLine("your connectionString is : \n" + connectionString);
        if(ConTimeout == 0)
        {
            Console.WriteLine("Error: Connection timeout is null or empty.");
            return;
        }
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Error: Connection string is null or empty.");
            return;
        }

        try
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
               
                string deleteQuery = @"DELETE FROM ELT 
                                        WHERE EltELPushDate < DATEADD(DAY, -(
                                        SELECT SourceArchFileAge 
                                        FROM Source 
                                        WHERE Source.SourceId = ELT.SourceId), GETDATE());";
               
               

                using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.CommandTimeout = ConTimeout; // Set command timeout to 30 minutes
                    Console.WriteLine($"Delete Query {deleteQuery}");

                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} records deleted successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
