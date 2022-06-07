using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using DotNetNuke.Common.Utilities;

namespace OS_sogecommerce
{
    public class ProviderUtils
    {

        public static String GetTemplateData(String templatename, NBrightInfo pluginInfo)
        {
            var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/OS_sogecommerce");
            var templCtrl = new NBrightCore.TemplateEngine.TemplateGetter(PortalSettings.Current.HomeDirectoryMapPath, controlMapPath, "Themes\\config", "");
            var templ = templCtrl.GetTemplateData(templatename, Utils.GetCurrentCulture());
            templ = Utils.ReplaceSettingTokens(templ, pluginInfo.ToDictionary());
            templ = Utils.ReplaceUrlTokens(templ);
            return templ;
        }


        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_sogecommercepayment", "OS_sogecommercePAYMENT", Utils.GetCurrentCulture());
            return info;
        }


        public static String GetBankRemotePost(OrderData orderData)
        {
            var rPost = new RemotePost();

            var settings = ProviderUtils.GetProviderSettings();

            var payData = new PayData(orderData);

            // build signature string
            string strMacCalc = "";
            strMacCalc += payData.vads_action_mode + "+";
            strMacCalc += payData.vads_amount + "+";
            strMacCalc += payData.vads_ctx_mode + "+";
            strMacCalc += payData.vads_currency + "+";
            strMacCalc += payData.vads_language + "+";
            strMacCalc += payData.vads_order_id + "+";
            strMacCalc += payData.vads_order_info + "+";
            strMacCalc += payData.vads_order_info2 + "+";
            strMacCalc += payData.vads_page_action + "+";
            strMacCalc += payData.vads_payment_config + "+";
            strMacCalc += payData.vads_return_mode + "+";
            strMacCalc += payData.vads_site_id + "+";
            strMacCalc += payData.vads_trans_date + "+";
            strMacCalc += payData.vads_trans_id + "+";
            strMacCalc += payData.vads_url_return + "+";
            strMacCalc += payData.vads_version + "+";
            strMacCalc += payData.certificate;


            rPost.Url = payData.PostUrl;

            rPost.Add("vads_version", payData.vads_version);
            rPost.Add("vads_page_action", payData.vads_page_action);
            rPost.Add("vads_action_mode", payData.vads_action_mode);
            rPost.Add("vads_payment_config", payData.vads_payment_config);
            rPost.Add("vads_site_id", payData.vads_site_id);
            rPost.Add("vads_ctx_mode", payData.vads_ctx_mode);
            rPost.Add("vads_trans_id", payData.vads_trans_id);
            rPost.Add("vads_trans_date", payData.vads_trans_date);
            rPost.Add("vads_amount", payData.vads_amount);
            rPost.Add("vads_currency", payData.vads_currency);
            rPost.Add("vads_return_mode", payData.vads_return_mode);
            rPost.Add("vads_order_id", payData.vads_order_id);
            rPost.Add("vads_order_info", payData.vads_order_info);
            rPost.Add("vads_order_info2", payData.vads_order_info2);
            rPost.Add("vads_language", payData.vads_language);
            rPost.Add("vads_url_return", payData.vads_url_return);
            rPost.Add("signature", GetSignature(strMacCalc));

            //Build the re-direct html 
            var rtnStr = rPost.GetPostHtml();
            if (settings.GetXmlPropertyBool("genxml/checkbox/debugmode"))
            {
                File.WriteAllText(PortalSettings.Current.HomeDirectoryMapPath + "\\debug_SystemPaypost.html", rtnStr);
            }
            return rtnStr;
        }

        public static string GetStatusCode(OrderData oInfo, HttpRequest request)
        {
            var result = "00";
            var status = request.Form.Get("vads_result");
            if (status != "00") result = "01";
            return result;
        }

        public static string GetSignature(string strMacCalc)
        {
            System.Security.Cryptography.SHA1CryptoServiceProvider objCrypt = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] bytesToHash = System.Text.Encoding.UTF8.GetBytes(strMacCalc);
            string rtnStr = "";
            bytesToHash = objCrypt.ComputeHash(bytesToHash);
            foreach (byte b in bytesToHash)
            {
                rtnStr += b.ToString("x2");
            }
            return rtnStr;
        }

        public static string GetSignatureReturnData(String certificate, HttpRequest htRequest)
        {
            string strMacCalc = "";
            Hashtable rtnFields = new Hashtable();

            //get return field into hastable
            foreach (string strKey in htRequest.Form.Keys)
            {
                if (strKey.ToLower().StartsWith("vads_"))
                {
                    rtnFields.Add(strKey, htRequest.Form.Get(strKey));
                }
            }

            //sort into key order
            ICollection keys = rtnFields.Keys;
            string[] keysArray = new string[rtnFields.Count];
            keys.CopyTo(keysArray, 0);
            Array.Sort(keysArray);
            foreach (string key in keysArray)
            {
                if (key.ToLower().StartsWith("vads_"))
                {
                    strMacCalc += htRequest.Form.Get(key) + "+";
                }
            }

            strMacCalc += certificate;

            return strMacCalc;
        }


    }
}
