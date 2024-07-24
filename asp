<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DatabaseQuery.aspx.cs" Inherits="DatabaseQuery" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Database Query</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
        </div>
    </form>
</body>
</html>

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;

public partial class DatabaseQuery : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string dbName = Request.QueryString["db"];
            if (!string.IsNullOrEmpty(dbName))
            {
                ExecuteQuery(dbName);
            }
            else
            {
                lblMessage.Text = "Database name is missing in the query string.";
            }
        }
    }

    private void ExecuteQuery(string dbName)
    {
        string connectionString = $"Server=your_server_name;Database={dbName};Integrated Security=True;";
        string query = "SELECT TOP 1 * FROM your_table_name"; // Replace with your actual query

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    lblMessage.Text = "Query executed successfully.";
                }
                else
                {
                    lblMessage.Text = "Query executed but no data found.";
                }
                reader.Close();
            }
        }
        catch (Exception ex)
        {
            lblMessage.Text = $"Query failed with exception: {ex.Message}";
        }
    }
}

