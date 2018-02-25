using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace DAL
{
    public class DatabaseLayer
    {
        /// <summary>
        /// Write all transactions from one block to database
        /// </summary>
        /// <param name="Transaction">one or more transactions json from one block</param>
        /// <param name="debug">if debug is true, the json string will be written to DebuggerTable</param>
        /// <returns>bool success</returns>
        public bool StoreTransactionsDB(JToken Transaction, String Database, bool debug)
        {
            var sp = "StoreTransactions";
            if (debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter param;
                        param = new SqlParameter
                        {
                            ParameterName = "@json",
                            SqlDbType = System.Data.SqlDbType.NVarChar,
                            Size = -1,
                            Value = (object)JsonConvert.SerializeObject(Transaction)
                        };
                        cmd.Parameters.Add(param);
                        cmd.CommandTimeout = 120;
                        SqlParameter returnParam = new SqlParameter(); ;
                        returnParam.ParameterName = "@success";
                        returnParam.SqlDbType = SqlDbType.Bit;
                        returnParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(returnParam);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        success = (bool)cmd.Parameters["@success"].Value;
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                success = false;
            }
            return success;
        }

        public long GetMaxBlockNEPDB(string Database)
        {
            long maxblock = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand("select max(BlockId) from NepTransfer", con))
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && long.TryParse(result.ToString(), out long n))
                        {
                            maxblock = long.Parse(result.ToString());
                        }
                        else
                        {
                            maxblock = -1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                return 0;
            }
            return maxblock;
        }

        /// <summary>
        /// Get max block index currently stored in database
        /// </summary>
        /// <returns>long max block height</returns>
        public long GetMaxBlockDB(string Database)
        {
            long maxblock = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand("select max([Index]) from Block", con))
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && long.TryParse(result.ToString(), out long n))
                        {
                            maxblock = long.Parse(result.ToString());
                        }
                        else
                        {
                            maxblock = -1;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                return 0;
            }
            return maxblock;
        }

        /// <summary>
        /// Store one block in database
        /// </summary>
        /// <param name="Block">block json</param>
        /// <param name="debug">if debug is true, the json string will be written to DebuggerTabl</param>
        /// <returns>bool success</returns>
        public bool StoreBlock(JToken Block, String Database, bool debug)
        {
            string json = JsonConvert.SerializeObject(Block);
            var sp = "StoreBlock";
            if (debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            if (ExecuteStoredProcedure(sp, Block, Database))
            {
                success = true;
            }
            else
            {
                success = false;
            }
            return success;
        }
        public int GetNEPPrecision(string contract, String Database)
        {
            var precision = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand("select [Precision] from Asset where Asset = @hash", con))
                    {
                        con.Open();
                        cmd.Parameters.Add("@hash", SqlDbType.VarChar, 64).Value = contract;
                        var result = int.TryParse(cmd.ExecuteScalar().ToString(), out precision);
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                return precision;
            }
            return precision;
        }
        /// <summary>
        /// Check if the given contract exisits in the database
        /// </summary>
        /// <param name="contract">contract hash</param>
        /// <param name="Database">database string</param>
        /// <param name="debug">debug bool</param>
        /// <returns>bool success</returns>
        public bool ExistsContractDB(string contract, String Database, bool debug)
        {
            var success = false;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand("select Id from Contract where Hash = @hash", con))
                    {
                        con.Open();
                        cmd.Parameters.Add("@hash", SqlDbType.VarChar, 64).Value = contract;
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                return false;
            }
            return success;
        }
        /// <summary>
        /// Save NEP5 contract and asset in database
        /// </summary>
        /// <param name="Contractstate">contract json</param>
        /// <param name="Database">database</param>
        /// <param name="debug">success debug</param>
        /// <returns>success bool</returns>
        public bool StoreContractAssetDB(JObject Contractstate, String Database, bool debug)
        {
            var sp = "StoreContractAsset";
            if (debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            if (ExecuteStoredProcedure(sp,Contractstate,Database))
            {
                success = true;
            }
            else
            {
                success = false;
            }
            return success;
        }
        /// <summary>
        /// Stores a nep5 transfer in database
        /// </summary>
        /// <param name="Blocknep5">nep5 json</param>
        /// <param name="Database">database</param>
        /// <param name="debug">debug bool</param>
        /// <returns>success bool</returns>
        public bool StoreNEPTransferDB(JToken Blocknep5, string Database, bool debug)
        {
            var sp = "StoreNEPTransfer";
            if (debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            if (ExecuteStoredProcedure(sp, Blocknep5, Database))
            {
                success = true;
            }
            else
            {
                success = false;
            }
            return success;
        }
        /// <summary>
        /// Executes a stored procedure with return value true/false. SP needs to accept one input parameter as json string and 
        /// needs to have one output paramter bit.
        /// </summary>
        /// <param name="sp">name of stored procedure</param>
        /// <param name="JsonString">json parameter input</param>
        /// <param name="Database">database string</param>
        /// <returns>bool success</returns>
        private bool ExecuteStoredProcedure(string sp, JToken JsonString, string Database)
        {
            var success = true;
            try
            {
                using (SqlConnection con = new SqlConnection(Database))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter param;
                        param = new SqlParameter
                        {
                            ParameterName = "@json",
                            SqlDbType = SqlDbType.NVarChar,
                            Size = -1,
                            Value = (object)JsonConvert.SerializeObject(JsonString)
                        };
                        cmd.Parameters.Add(param);

                        SqlParameter returnParam = new SqlParameter(); ;
                        returnParam.ParameterName = "@success";
                        returnParam.SqlDbType = SqlDbType.Bit;
                        returnParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(returnParam);
                        cmd.CommandTimeout = 120;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        success = (bool)cmd.Parameters["@success"].Value;
                    }
                }
            }
            catch (Exception e)
            {
                using (TextWriter errorWriter = Console.Error)
                {
                    errorWriter.WriteLine(e.Message);
                }
                success = false;
            }
            return success;
        }
    }
}
