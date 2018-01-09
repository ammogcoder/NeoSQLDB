using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DAL;
using System.Globalization;

namespace BLL
{
    public class BusinessLayer
    {
        /// <summary>
        /// Initialize database access layer
        /// </summary>
        public static DatabaseLayer databaseLayer = new DatabaseLayer();

        /// <summary>
        /// Get max block from database
        /// </summary>
        /// <returns>int</returns>
        public int GetMaxBlockDB()
        {
            return databaseLayer.GetMaxBlockDB();
        }
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="data">block json from node</param>
        /// <returns></returns>
        public bool SyncBlockTest(JObject data)
        {
            var block = data["result"];

            //first store block transaction
            Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, true);

            return true;
        }
        /// <summary>
        /// Sync one block.
        /// </summary>
        /// <param name="data">block json from node</param>
        /// <param name="debug">bool</param>
        /// <returns>bool success</returns>
        public bool SyncBlock(JObject data, bool debug)
        {
            try
            {
                var block = data["result"];
                var success = true;

                //first store block transaction
                Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, debug);
                //if txs insertion successfull
                if (result.Item1)
                {
                    //get TX count
                    block["txcount"] = block["tx"].Count();
                    //set transactions = null because not needed to block sync
                    block["tx"] = null;
                    block["sys_fee"] = result.Item2;
                    block["net_fee"] = result.Item3;
                    block["hash"] = ConvertHash(block["hash"].ToString());
                    block["previousblockhash"] = ConvertHash(block["previousblockhash"].ToString());
                    block["merkleroot"] = ConvertHash(block["merkleroot"].ToString());

                    block["nextblockhash"] = block["nextblockhash"];
                    if (block["nextblockhash"].Type != JTokenType.Null)
                    {
                        block["nextblockhash"] = ConvertHash(block["nextblockhash"].ToString());
                    }

                    //Store block in database
                    if (databaseLayer.StoreBlock(block, debug))
                    {
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                }
                else
                {
                    success = false;
                }

                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Convert hash string (0x) for storage in database
        /// </summary>
        /// <param name="hash">hash</param>
        /// <returns>hash with length 64</returns>
        private string ConvertHash(string hash)
        {
            return hash.Length > 64 ? hash.Substring(2,64) : hash;
        }
        /// <summary>
        /// Sync all block transactions.
        /// </summary>
        /// <param name="block">block json</param>
        /// <param name="debug">success</param>
        /// <returns>tuple: success, sys_fee sum, net_fee sum</returns>
        private Tuple<bool, Decimal, Decimal> SyncBlockTransactions(JToken block, bool debug)
        {
            //Store Transaction, Input,Output, Claim, Script, Attribute in different tables
            //Need to pull apart data
            var transactions = block["tx"];
            Decimal sys_fee_total = 0;
            Decimal net_fee_total = 0;
            bool success = true;

            //for every transaction do some processing
            for (int j = 0; j<transactions.Count(); j++)
            {
                var vout = transactions[j]["vout"];

                transactions[j]["txid"] = ConvertHash(transactions[j]["txid"].ToString());//convert "0x"
                transactions[j]["blockid"] = block["index"];
                transactions[j]["blockhash"] = ConvertHash(block["hash"].ToString());
                transactions[j]["time"] = block["time"];

                transactions[j]["nonce"] = transactions[j]["nonce"];
                //for enrollment transactions
                transactions[j]["publickey"] = transactions[j]["publickey"];
                //for claimtransaction
                transactions[j]["claims"] = transactions[j]["claims"];
                //for invocation transaction
                transactions[j]["gas"] = transactions[j]["gas"];

                //transactions[j]["sys_fee"] = Decimal.Parse(transactions[j]["sys_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                //transactions[j]["net_fee"] = Decimal.Parse(transactions[j]["net_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);

                decimal value_net;
                if (Decimal.TryParse(transactions[j]["net_fee"].ToString(), NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, out value_net))
                {
                    net_fee_total += value_net;
                }
                decimal value_sys;
                if (Decimal.TryParse(transactions[j]["sys_fee"].ToString(), NumberStyles.Any, System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat, out value_sys))
                {
                    sys_fee_total += value_sys;
                }
                transactions[j]["sys_fee"] = value_sys;
                transactions[j]["net_fee"] = value_net;

                transactions[j]["script"] = transactions[j]["script"];

                //go through all vout and convert the value
                for (int i = 0; i < vout.Count(); i++)
                {
                    vout[i]["value"] = Decimal.Parse(vout[i]["value"].ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    i++;
                }
                transactions[j]["vout"] = vout;

                //convert asset amount to dec
                transactions[j]["asset"] = transactions[j]["asset"];
                if (transactions[j]["asset"].HasValues)
                {
                    transactions[j]["asset"]["amount"] = Decimal.Parse(transactions[j]["asset"]["amount"].ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
                }
                //"gas" needs to be converted to dec
                transactions[j]["gas"] = transactions[j]["gas"];
                if (transactions[j]["gas"].Type != JTokenType.Null)
                {
                    transactions[j]["gas"] = Decimal.Parse(transactions[j]["gas"].ToString(), CultureInfo.GetCultureInfo("en-US").NumberFormat);
                }
            }
            //store transactions in database
            success = databaseLayer.StoreTransactionsDB(transactions, debug);
            return Tuple.Create(success, sys_fee_total, net_fee_total);
        }
    }
}
