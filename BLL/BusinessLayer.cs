using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using DAL;
using System.Globalization;
using Neo.Cryptography;
using System.Security.Cryptography;
using Neo;
using System.Collections.Generic;
using NAL;
using System.Text;
using System.Numerics;

namespace BLL
{
    public class BusinessLayer
    {
        /// <summary>
        /// Initialize database access layer
        /// </summary>
        public static DatabaseLayer databaseLayer = new DatabaseLayer();

        public static NodeLayer nodeLayer = new NodeLayer();

        /// <summary>
        /// Get max block from database
        /// </summary>
        /// <returns>int</returns>
        public long GetMaxBlockDB(string Database)
        {
            return databaseLayer.GetMaxBlockDB(Database);
        }
        /// <summary>
        /// Get max block from database for nep sync
        /// </summary>
        /// <returns>int</returns>
        public long GetMaxBlockNEPDB(string Database)
        {
            return databaseLayer.GetMaxBlockNEPDB(Database);
        }
        /// <summary>
        /// Test
        /// </summary>
        /// <param name="data">block json from node</param>
        /// <returns></returns>
        public bool SyncBlockTest(JObject data, String Database)
        {
            var block = data["result"];

            //first store block transaction
            Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, Database, true);

            return true;
        }
        /// <summary>
        /// Sync one block.
        /// </summary>
        /// <param name="data">block json from node</param>
        /// <param name="debug">bool</param>
        /// <returns>bool success</returns>
        public bool SyncBlock(JObject data, String Database, bool debug)
        {
            try
            {
                var block = data["result"];
                var success = true;

                //first store block transaction
                Tuple<bool, Decimal, Decimal> result = SyncBlockTransactions(block, Database, debug);
                //if txs insertion successfull
                if (result.Item1)
                {
                    //get TX count
                    block["txcount"] = block["tx"].Count();
                    //set transactions = null because not needed to block sync
                    block["tx"] = null;
                    block["sys_fee"] = result.Item2;
                    block["net_fee"] = result.Item3;
                    block["hash"] = ConvertHash(block["hash"].ToString(),64);
                    block["previousblockhash"] = ConvertHash(block["previousblockhash"].ToString(),64);
                    block["merkleroot"] = ConvertHash(block["merkleroot"].ToString(),64);

                    block["nextblockhash"] = block["nextblockhash"];
                    if (block["nextblockhash"].Type != JTokenType.Null)
                    {
                        block["nextblockhash"] = ConvertHash(block["nextblockhash"].ToString(),64);
                    }

                    //Store block in database
                    if (databaseLayer.StoreBlock(block, Database, debug))
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
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Syn nep5 tokens/transfers for one block
        /// </summary>
        /// <param name="blocknep5"></param>
        /// <param name="AddressVersion"></param>
        public bool SyncNEP5(JObject data, byte AddressVersion, List<string> nodes, long BlockId, String Database, bool debug)
        {
            try
            {
                var blocknep5 = data["result"];
                var success = false;
                var success2 = false;
                for (int j = 0; j < blocknep5.Count(); j++)
                {
                    if (blocknep5[j]["state"]["value"][0]["value"].ToString() == "7472616e73666572") //transfer
                    {
                        //get contract from db. if null, save asset and contract to db 
                        //always save contract and asset both.
                        //asset needs to be saved in first instance, so the transfer can get the asset id
                        var contract = ConvertHash(blocknep5[j]["contract"].ToString(), 40);
                        var contractorig = blocknep5[j]["contract"].ToString();
                        int precision = 0;

                        if (!databaseLayer.ExistsContractDB(contract, Database, debug))
                        {
                            JObject contractstate = nodeLayer.Invoke("getcontractstate", nodes, contractorig, "1");
                            contractstate = (JObject)contractstate["result"];

                            JObject assetSymbol = nodeLayer.Invoke("invokefunction", nodes, contractorig, "symbol");
                            assetSymbol = (JObject)assetSymbol["result"];

                            JObject assetDecimals = nodeLayer.Invoke("invokefunction", nodes, contractorig, "decimals");
                            assetDecimals = (JObject)assetDecimals["result"];
                            if (assetDecimals["stack"][0]["value"].ToString() == "")
                            {
                                //Try "Decimals" ???
                                assetDecimals = nodeLayer.Invoke("invokefunction", nodes, contractorig, "Decimals");
                                assetDecimals = (JObject)assetDecimals["result"];
                            }
                            if (assetDecimals["stack"][0]["value"].ToString() != "")
                            {
                                if (assetDecimals["stack"][0]["type"].ToString() == "ByteArray")
                                {
                                    //check for unknown operation
                                    if(assetDecimals["stack"][0]["value"].ToString() == "756e6b6e6f776e206f7065726174696f6e")
                                    {
                                        precision = 0;
                                    }
                                    else
                                    {
                                        var hexval = ReverseHex(assetDecimals["stack"][0]["value"].ToString());
                                        precision = (int)Convert.ToInt64(hexval, 16);
                                    }
                                }
                                else
                                {
                                    precision = (int)long.Parse(assetDecimals["stack"][0]["value"].ToString());
                                }
                            }
                            else
                            {
                                precision = 0;
                            } 

                            JObject assetName = nodeLayer.Invoke("invokefunction", nodes, contractorig, "name");
                            assetName = (JObject)assetName["result"];

                            JObject assetSupply = nodeLayer.Invoke("invokefunction", nodes, contractorig, "totalSupply");
                            assetSupply = (JObject)assetSupply["result"];

                            contractstate["hash"] = ConvertHash(contractstate["hash"].ToString(), 40);
                            contractstate["assetname"] = HexToString(assetName["stack"][0]["value"].ToString());
                            contractstate["assetdecimals"] = precision;
                            contractstate["assetSymbol"] = HexToString(assetSymbol["stack"][0]["value"].ToString());
                            contractstate["assetSupply"] = (Decimal)Convert.ToInt64(ReverseHex(assetSupply["stack"][0]["value"].ToString()), 16) / (Decimal)Math.Pow(10, Convert.ToDouble(precision));

                            contractstate["storage"] = null;
                            if (contractstate["properties"]["storage"].Type != JTokenType.Null)
                            {
                                contractstate["storage"] = contractstate["properties"]["storage"];
                            }
                            contractstate["dynamic_invoke"] = null;
                            if (contractstate["properties"]["dynamic_invoke"].Type != JTokenType.Null)
                            {
                                contractstate["dynamic_invoke"] = contractstate["properties"]["dynamic_invoke"];
                            }
                            contractstate["properties"] = null;

                            if (databaseLayer.StoreContractAssetDB(contractstate, Database, debug))
                            {
                                success2 = true;
                            }
                            else
                            {
                                success2 = false;
                            }
                        }
                        else
                        {
                            precision = databaseLayer.GetNEPPrecision(contract, Database);
                            success2 = true;
                        }

                        //reverse value
                        if (blocknep5[j]["state"]["value"][3]["type"].ToString() == "ByteArray")
                        {
                            var valuehex = ReverseHex(blocknep5[j]["state"]["value"][3]["value"].ToString());
                            var value = Convert.ToInt64(valuehex, 16) / (Decimal)Math.Pow(10,Convert.ToDouble(precision));
                            blocknep5[j]["value"] = value;
                        }
                        else
                        {
                            var value = (Decimal)long.Parse(blocknep5[j]["state"]["value"][3]["value"].ToString()) / (Decimal)Math.Pow(10, Convert.ToDouble(precision));
                            blocknep5[j]["value"] = value;
                        }
                        var from = "";
                        if(blocknep5[j]["state"]["value"][1]["value"].ToString() != "")
                        {
                            from = ToAddress(UInt160.Parse(ReverseHex(blocknep5[j]["state"]["value"][1]["value"].ToString())), AddressVersion);
                        }
                        var to = ToAddress(UInt160.Parse(ReverseHex(blocknep5[j]["state"]["value"][2]["value"].ToString())), AddressVersion);

                        blocknep5[j]["txid"] = ConvertHash(blocknep5[j]["txid"].ToString(),64);
                        blocknep5[j]["notifytype"] = "transfer";
                        blocknep5[j]["contract"] = contract;
                        blocknep5[j]["blockid"] = BlockId;
                        blocknep5[j]["addressfrom"] = from;
                        blocknep5[j]["addressto"] = to;
                        blocknep5[j]["state"] = null;                   
                    }
                    else
                    {
                        blocknep5[j].Remove();
                        j = j - 1;
                    }
                }
                if (success2)
                {
                    //Store nep5 transfer in database
                    if (databaseLayer.StoreNEPTransferDB(blocknep5, Database, debug))
                    {
                        success = true;
                    }
                    else
                    {
                        success = false;
                    }
                }
                if (success && success2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                }
            catch (Exception e)
            {
                return false;
            }
        }
        public static string ToAddress(UInt160 scriptHash, byte addressVersion)
        {
            byte[] data = new byte[21];
            data[0] = addressVersion;
            Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
            return data.Base58CheckEncode();
        }
        /// <summary>
        /// Convert hash string (0x) for storage in database
        /// </summary>
        /// <param name="hash">hash</param>
        /// <returns>hash with length n</returns>
        private string ConvertHash(string hash, int n)
        {
            return hash.Length > n ? hash.Substring(2, n) : hash;
        }
        /// <summary>
        /// Sync all block transactions.
        /// </summary>
        /// <param name="block">block json</param>
        /// <param name="debug">success</param>
        /// <returns>tuple: success, sys_fee sum, net_fee sum</returns>
        private Tuple<bool, Decimal, Decimal> SyncBlockTransactions(JToken block, String Database, bool debug)
        {
            //Store Transaction, Input,Output, Claim, Script, Attribute in different tables
            //Need to pull apart data
            var transactions = block["tx"];
            Decimal sys_fee_total = 0;
            Decimal net_fee_total = 0;
            bool success = true;

            //for every transaction do some processing
            for (int j = 0; j < transactions.Count(); j++)
            {
                var vout = transactions[j]["vout"];

                transactions[j]["txid"] = ConvertHash(transactions[j]["txid"].ToString(),64);//convert "0x"
                transactions[j]["blockid"] = block["index"];
                transactions[j]["blockhash"] = ConvertHash(block["hash"].ToString(),64);
                transactions[j]["time"] = block["time"];

                transactions[j]["nonce"] = transactions[j]["nonce"];
                //for enrollment transactions
                transactions[j]["publickey"] = transactions[j]["publickey"];
                //for claimtransaction
                transactions[j]["claims"] = transactions[j]["claims"];
                //for invocation transaction
                transactions[j]["gas"] = transactions[j]["gas"];

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
            success = databaseLayer.StoreTransactionsDB(transactions, Database, debug);
            return Tuple.Create(success, sys_fee_total, net_fee_total);
        }
        public static string ReverseHex(string hex)
        {
            var result = "";
            for (var i = hex.Length - 2; i >= 0; i -= 2)
            {
                result += hex.Substring(i, 2);
            }
            return result;
        }
        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }
        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
        public static string HexToString(string HexValue)
        {
            string StrValue = "";
            while (HexValue.Length > 0)
            {
                StrValue += System.Convert.ToChar(System.Convert.ToUInt32(HexValue.Substring(0, 2), 16)).ToString();
                HexValue = HexValue.Substring(2, HexValue.Length - 2);
            }
            return StrValue;
        }
    }
}
