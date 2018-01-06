using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DAL;

namespace BLL
{
    public class BusinessLayer
    {
        public static DatabaseLayer databaseLayer = new DatabaseLayer();

        public int GetMaxBlockDB()
        {
            return databaseLayer.GetMaxBlockDB();
        }
        public bool SyncBlockTest(JObject data)
        {
            var block = data["result"];

            //first store block transaction
            Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, true);

            return true;
        }
        public bool SyncBlock(JObject data, bool debug)
        {
            try
            {
                var block = data["result"];
                var success = true;

                //first store block transaction
                Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, debug);
                //if tx insertion successfully
                if (result.Item1)
                {
                    //get TX count
                    block["txcount"] = block["tx"].Count();
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
        private string ConvertHash(string txid)
        {
            return txid.Length > 64 ? txid.Substring(2,64) : txid;
        }

        private Tuple<bool, Decimal, Decimal> SyncBlockTransactions(JToken block, bool debug)
        {
            //Store Transaction, Input,Output, Claim, Script, Attribute in different tables
            //Need to pull apart data
            var transactions = block["tx"];
            Decimal sys_fee_total = 0;
            Decimal net_fee_total = 0;
            bool success = true;

            //for every transaction
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
                //transactions[j]["sys_fee"] = float.Parse(transactions[j]["sys_fee"].ToString());
                //transactions[j]["net_fee"] = float.Parse(transactions[j]["net_fee"].ToString());

                transactions[j]["sys_fee"] = Decimal.Parse(transactions[j]["sys_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                transactions[j]["net_fee"] = Decimal.Parse(transactions[j]["net_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);


                sys_fee_total += Decimal.Parse(transactions[j]["sys_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                net_fee_total += Decimal.Parse(transactions[j]["net_fee"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);

                transactions[j]["script"] = transactions[j]["script"];

                //check for empty {} or {[]}

                //go through all VOUT and convert the value
                for (int i = 0; i < vout.Count(); i++)
                {
                    vout[i]["value"] = Decimal.Parse(vout[i]["value"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                    i++;
                }
                transactions[j]["vout"] = vout;

                //convert asset to dec
                transactions[j]["asset"] = transactions[j]["asset"];
                if (transactions[j]["asset"].HasValues)
                {
                    transactions[j]["asset"]["amount"] = Decimal.Parse(transactions[j]["asset"]["amount"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                }
                //gas needs to be converted to dec
                transactions[j]["gas"] = transactions[j]["gas"];
                if (transactions[j]["gas"].Type != JTokenType.Null)
                {
                    transactions[j]["gas"] = Decimal.Parse(transactions[j]["gas"].ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-US").NumberFormat);
                }
            }
            //store transactions
            success = databaseLayer.StoreTransactionsDB(transactions, debug);
            return Tuple.Create(success, sys_fee_total, net_fee_total);
        }
    }
}
