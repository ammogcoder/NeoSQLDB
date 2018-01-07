using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;
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
        public bool StoreTransactionsDB(JToken Transaction, bool debug)
        {
            var sp = "StoreTransactions";
            if(debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            try
            {
                using (SqlConnection con = new SqlConnection("Integrated Security=SSPI;Initial Catalog=NEO_MainNet;Data Source=.;"))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter param;
                        param = new SqlParameter();
                        param.ParameterName = "@json";
                        param.SqlDbType = System.Data.SqlDbType.NVarChar;
                        param.Size = -1;
                        param.Value = (object)JsonConvert.SerializeObject(Transaction);
                        cmd.Parameters.Add(param);

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
                success = false;
            }
            return success;
        }
        /// <summary>
        /// Get max block index currently stored in database
        /// </summary>
        /// <returns>int max block height</returns>
        public int GetMaxBlockDB()
        {
            var maxblock = 0;
            try
            {
                using (SqlConnection con = new SqlConnection("Integrated Security=SSPI;Initial Catalog=NEO_MainNet;Data Source=.;"))
                {
                    using (SqlCommand cmd = new SqlCommand("select max([Index]) from Block", con))
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(),out int n))
                        {
                            maxblock = int.Parse(result.ToString());
                        }
                        else
                        {
                            maxblock = -1;
                        }
                    }
                }
            }
            catch (Exception)
            {
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
        public bool StoreBlock(JToken Block, bool debug)
        {
            string json = JsonConvert.SerializeObject(Block);
            var sp = "StoreBlock";
            if (debug)
            {
                sp = "InsertJsonTest";
            }
            var success = true;
            try
            {
                using (SqlConnection con = new SqlConnection("Integrated Security=SSPI;Initial Catalog=NEO_MainNet;Data Source=.;"))
                {
                    using (SqlCommand cmd = new SqlCommand(sp, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        SqlParameter param;
                        param = new SqlParameter();
                        param.ParameterName = "@json";
                        param.SqlDbType = System.Data.SqlDbType.NVarChar;
                        param.Size = -1;
                        param.Value = (object)JsonConvert.SerializeObject(Block);
                        cmd.Parameters.Add(param);

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
            catch (Exception)
            {
                success = false;
            }
            return success;
        }
    }
}
