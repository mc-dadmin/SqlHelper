using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class SqlHelper
{
    private string Server = "";
    private string Username = "";
    private string Password = "";
    private string Database = "";
    private string Command = "";
    private bool Trusted_Connection;
    private string ConnectionString = "";
    private string Error = "";
    private Exception Exception;
    private SqlConnection sqlConn;

    public SqlHelper()
    {
        /* *
        * Instantiate an empty SqlHelper object
        */

        Trusted_Connection = false;
    }

    public SqlHelper(SqlConnection sqlConnection)
    {
        /* *
        * Instantiate a a new SqlHelper object with an existing sqlConnection.
        * This should allow the helper methods to be used with a sqlConnection that is created or provided from outside of this library.
        * Should not require executing the .Connect() method.
        */
        sqlConn = sqlConnection;
    }

    public SqlHelper(string connectionString)
    {
        /* *
        * Instantiate a SqlHelper object using a premade connection string
        */

        Trusted_Connection = false;
        ConnectionString = connectionString;
    }

    public SqlHelper(string server, string db)
    {
        /* *
        * Instantiate a SqlHelper object using Windows Trusted Authentication. 
        */

        Server = server;
        Database = db;
        Trusted_Connection = true;
    }

    public SqlHelper(string server, string db, string user, string pass)
    {
        /* *
        * Instantiate a SqlHelper object with a specified username and password
        */

        Server = server;
        Username = user;
        Password = pass;
        Database = db;
        Trusted_Connection = false;
    }

    public string GetServer()
    { return Server; }

    public string GetSqlServer()
    { return sqlConn.ServerVersion; }

    public string GetDatabase()
    { return Database; }

    public string GetError()
    { return Error; }

    public Exception GetException()
    { return Exception; }

    public void SetServer(string ServerName)
    { Server = ServerName; }

    public void SetDatabase(string db)
    { Database = db; }

    public void SetUser(string User)
    { Username = User; }

    public void SetPassword(string Pass)
    { Password = Pass; }

    public void SetCMD(string cmd)
    { Command = cmd; }

    /// <summary>
    /// Open the sql connection.
    /// </summary>
    /// <returns>Returns True if successful. False if there was an error. Error text can be retrieved using GetError().</returns>
    public bool Connect()
    {
        if (Trusted_Connection)
            ConnectionString = "Server=" + Server + "; Database=" + Database + "; Trusted_Connection=True;";
        else if (ConnectionString == "") // SqlHelper objects instantiated with a connection string will skip this and go straight to a connection attempt.
        {
            ConnectionString = "Server=" + Server + "; Database=" + Database + "; User Id=" + Username + "; Password=" + Password + ";";
        }
        try
        {
            sqlConn = new SqlConnection(ConnectionString);
            sqlConn.Open();
            return true;
        }
        catch (Exception ex)
        {
            Error = ex.Message; // This can be retrieved using .GetError()
            return false;
        }
    }

    /// <summary>
    /// Read-only query against sql database. Pass user inputs as sqlparamters using the inputs objects for sanitization.
    /// </summary>
    /// <returns>Returns a SqlDataReader object with the results. Exceptions can be retreived using GetException()</returns>
    /// <param name="cmd">string representing the sql query to be passed. parameter variables should be listed as @varName </param>
    /// <param name="inputs">An object[x,3] containing the sql parameters to pass. [x][0] is the (string)name of the sql paramter. [x][1] is the sqltype of paramter [optional]. [x][2] is the value of the sqlparamter.</param>
    /// <example>
    /// <code>
    /// string query = "select name from employees where id = @id"
    /// object [,] inputs = new object[0,3]
    /// inputs[0][0] = "id"
    /// inputs[0][2] = 3
    /// SqlDataReader results = sqlconn.Read(query,inputs)
    /// </code>
    /// </example>
    public SqlDataReader Read(string cmd, object[,] inputs)
    {
        SqlCommand command = new SqlCommand(cmd, sqlConn);
        SqlDataReader reader = null;
        for (int i = 0; i < inputs.GetLength(0); i++)
        {
            command.Parameters.AddWithValue((string)inputs[i, 0], inputs[i, 2]);
        }
        try
        {
            reader = command.ExecuteReader();
        } catch (Exception ex)
        {
            Exception = ex;
        }
        return reader;
    }

    /// <summary>
    /// Read-only query against sql database. Pass user inputs as sqlparamters using the inputs objects for sanitization.
    /// </summary>
    /// <returns>Returns results as a List of Lists of strings. Exceptions can be retreived using GetException()</returns>
    /// <param name="cmd">string representing the sql query to be passed. parameter variables should be listed as @varName </param>
    /// <param name="inputs">An object[x,3] containing the sql parameters to pass. [x][0] is the (string)name of the sql paramter. [x][1] is the sqltype of paramter [optional]. [x][2] is the value of the sqlparamter.</param>
    /// <param name="includeHeader">Boolean. True if you want to include column names as the first row. False if you only want data without column names.</param>
    /// <example>
    /// <code>
    /// string query = "select name from employees where id = @id"
    /// object [,] inputs = new object[0,3]
    /// inputs[0][0] = "id"
    /// inputs[0][2] = 3
    /// List&#60;List&#60;string&#62;&#62; results = sqlconn.ReadToList(query,inputs)
    /// </code>
    /// </example>
    public List<List<string>> ReadToList(string cmd, object[,] inputs, bool includeHeader)
    {
        List<List<string>> resultList = new List<List<string>>();
        List<string> tempList;

        SqlDataReader reader = Read(cmd, inputs);

        var columns = new List<string>();

        if (includeHeader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns.Add(reader.GetName(i));
            }
            resultList.Add(columns);
        }
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                tempList = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    tempList.Add(reader[i].ToString());
                }
                resultList.Add(tempList);
            }
        }
        reader.Close();
        return resultList;
    }

    /// <summary>
    /// object[,] inputs - first column is the sql variable name, second column is the variable type, third column is the value
    /// </summary>
    public int Write(string cmd, object[,] inputs)
    {
        SqlCommand CMD = new SqlCommand(cmd, sqlConn);
        for (int i = 0; i < inputs.GetLength(0); i++)
        {
            SqlParameter param = new SqlParameter();
            param.ParameterName = (string)inputs[i, 0];
            param.SqlDbType = (SqlDbType)inputs[i, 1];
            param.Value = inputs[i, 2];
            CMD.Parameters.Add(param);
        }

        return CMD.ExecuteNonQuery();
    }

    public void Disconnect()
    {
        sqlConn.Close();
    }
}