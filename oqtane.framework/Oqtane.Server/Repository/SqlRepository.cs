﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SqlRepository : ISqlRepository
    {

        public void ExecuteScript(Tenant tenant, string script)
        {
            // execute script in curent tenant
            foreach (string query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
            {
                ExecuteNonQuery(tenant, query);
            }
        }

        public bool ExecuteScript(Tenant tenant, Assembly assembly, string filename)
        {
            // script must be included as an Embedded Resource within an assembly
            bool success = true;
            string script = "";

            if (assembly != null)
            {
                string name = assembly.GetManifestResourceNames().FirstOrDefault(item => item.EndsWith("." + filename));
                if (name != null)
                {
                    Stream resourceStream = assembly.GetManifestResourceStream(name);
                    if (resourceStream != null)
                    {
                        using (var reader = new StreamReader(resourceStream))
                        {
                            script = reader.ReadToEnd();
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    ExecuteScript(tenant, script);
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public int ExecuteNonQuery(Tenant tenant, string query)
        {
            SqlConnection conn = new SqlConnection(FormatConnectionString(tenant.DBConnectionString));
            SqlCommand cmd = conn.CreateCommand();
            using (conn)
            {
                PrepareCommand(conn, cmd, query);
                int val = -1;
                try
                {
                    val = cmd.ExecuteNonQuery();
                }
                catch
                {
                    // an error occurred executing the query
                }
                return val;
            }
        }

        public SqlDataReader ExecuteReader(Tenant tenant, string query)
        {
            SqlConnection conn = new SqlConnection(FormatConnectionString(tenant.DBConnectionString));
            SqlCommand cmd = conn.CreateCommand();
            PrepareCommand(conn, cmd, query);
            var dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return dr;
        }

        private void PrepareCommand(SqlConnection conn, SqlCommand cmd, string query)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = query;
            cmd.CommandType = CommandType.Text;
        }

        private string FormatConnectionString(string connectionString)
        {
            return connectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }
    }
}
