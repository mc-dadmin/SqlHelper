using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;

public class SQL
{
    private string Server = "";
    private string Username = "";
    private string Password = "";
    private string Database = "";
    private string Command = "";
    private bool Trusted_Connection;
    private string ConnectionString = "";
    private string Error = "";
    private SqlConnection sqlConn;


    //constructor
    public SQL()
    {
        Server = "";
        Username = "";
        Password = "";
        Database = "";
        Command = "";
        Trusted_Connection = false;
    }

    public SQL(string server, string db)
    {
        Server = server;
        Database = db;
        Trusted_Connection = true;
        Command = "";
    }

    public SQL(string server, string db, string user, string pass)
    {
        Server = server;
        Username = user;
        Password = pass;
        Database = db;
        Command = "";
        Trusted_Connection = false;
    }

    public string GetServer()
    { return Server; }

    public string GetDatabase()
    { return Database; }

    public string GetError()
    { return Error; }

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

    public bool Connect()
    {
        if (Trusted_Connection)
            ConnectionString = "Server=" + Server + "; Database=" + Database + "; Trusted_Connection=True;";
        else
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
            Error = ex.Message;
            return false;
        }
    }

    public SqlDataReader Read(string cmd)
    {
        SqlCommand CMD = new SqlCommand(cmd, sqlConn);
        return CMD.ExecuteReader();
    }

    /* SafeRead() prevents sql injection attacks and is a new version of Read(). Read() will be left in place as legacy, but new projects should use SafeRead()
     * object[,] inputs - first column is the variable placeholder name, second column is the variable type, third column is the value
     */
    public SqlDataReader SafeRead(string cmd, object[,] inputs)
    {
        SqlCommand command = new SqlCommand(cmd, sqlConn);
        for (int i = 0; i < inputs.GetLength(0); i++)
        {
            //command.Parameters.Add((string)inputs[i, 0], inputs[i, 1]);
            command.Parameters.AddWithValue((string)inputs[i, 0], inputs[i, 2]);
        }
        return command.ExecuteReader();
    }

    public List<List<string>> ReadToList(string cmd)
    {
        List<List<string>> resultList = new List<List<string>>();
        List<string> tempList;
        SqlDataReader reader = Read(cmd);

        var columns = new List<string>();

        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(reader.GetName(i));
        }
        resultList.Add(columns);
        if(reader.HasRows)
        {
            while(reader.Read())
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
    public List<List<string>> ReadToList(string cmd, bool includeHeader)
    {
        List<List<string>> resultList = new List<List<string>>();
        List<string> tempList;

        SqlDataReader reader = Read(cmd);

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

    public List<List<string>> SafeReadToList(string cmd, object[,] inputs, bool includeHeader)
    {
        List<List<string>> resultList = new List<List<string>>();
        List<string> tempList;

        SqlDataReader reader = SafeRead(cmd,inputs);

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

    public int Write(string cmd)
    {
        SqlCommand CMD = new SqlCommand(cmd, sqlConn);
        return CMD.ExecuteNonQuery();
    }

    public int SafeWrite(string cmd, object[,] inputs)
    {
        SqlCommand CMD = new SqlCommand(cmd, sqlConn);
        for (int i = 0; i < inputs.GetLength(0); i++)
        {
            //command.Parameters.Add((string)inputs[i, 0], inputs[i, 1]);
            //CMD.Parameters.AddWithValue((string)inputs[i, 0], inputs[i, 2]);

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