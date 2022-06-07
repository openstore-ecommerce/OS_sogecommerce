using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Microsoft.Win32.SafeHandles;
using Nevoweb.DNN.NBrightBuy.Components;
using NBrightCore.common;

namespace OS_sogecommerce
{

    public class PayData
    {

        public PayData(OrderData oInfo)
        {
            LoadSettings(oInfo);
        }

        public void LoadSettings(OrderData oInfo)
        {
            var settings = ProviderUtils.GetProviderSettings();
            var appliedtotal = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
            var alreadypaid = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

            certificate = settings.GetXmlProperty("genxml/textbox/certificate");           
            PostUrl = "https://sogecommerce.societegenerale.eu/vads-payment/";

            Email = oInfo.PurchaseInfo.GetXmlProperty("genxml/billaddress/textbox/billaddress");
            if (!Utils.IsEmail(Email)) Email = oInfo.PurchaseInfo.GetXmlProperty("genxml/extrainfo/textbox/cartemailaddress");

            var orderTotal = (appliedtotal - alreadypaid).ToString("0.00");

            vads_version = "V2";
            vads_page_action = "PAYMENT";
            vads_action_mode = "INTERACTIVE";
            vads_payment_config = "SINGLE";
            vads_site_id = settings.GetXmlProperty("genxml/textbox/site");
            vads_ctx_mode = "PRODUCTION";
            if (settings.GetXmlPropertyBool("genxml/checkbox/testmode")) vads_ctx_mode = "TEST";
            vads_trans_date = DateTime.UtcNow.Year.ToString("0000") + DateTime.UtcNow.Month.ToString("00") + DateTime.UtcNow.Day.ToString("00") + DateTime.UtcNow.Hour.ToString("00") + DateTime.UtcNow.Minute.ToString("00") + DateTime.UtcNow.Second.ToString("00");
            vads_amount = orderTotal.Replace(",", "").Replace(".", "");
            vads_currency = settings.GetXmlProperty("genxml/textbox/currency");
            vads_return_mode = "POST";
            vads_order_id = oInfo.PurchaseInfo.ItemID.ToString("");
            vads_order_info = oInfo.Lang;
            vads_order_info2 = oInfo.PurchaseInfo.ItemID.ToString("");
            vads_language = oInfo.Lang.Substring(0, 2);

            // with systempay, a transaction number can only be used once, Therefore we need to create a unique tansaction id on each post.
            var paycount = oInfo.PurchaseInfo.GetXmlPropertyInt("genxml/paycount");
            paycount += 1;
            oInfo.PurchaseInfo.SetXmlPropertyDouble("genxml/paycount",paycount);
            oInfo.SavePurchaseData();
            vads_trans_id = Utils.GetUniqueKey(6);


            var param = new string[3];
            param[0] = "orderid=" + oInfo.PurchaseInfo.ItemID.ToString(""); // return orderid as param, so processing can be done on return.
            vads_url_return = Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);


        }

        public string PostUrl { get; set; }
        public string Email { get; set; }
        public string certificate { get; set; }

        public string vads_version { get; set; }
        public string vads_page_action { get; set; }
        public string vads_action_mode { get; set; }
        public string vads_payment_config { get; set; }
        public string vads_site_id { get; set; }
        public string vads_ctx_mode { get; set; }
        public string vads_trans_id { get; set; }
        public string vads_trans_date { get; set; }
        public string vads_amount { get; set; }
        public string vads_currency { get; set; }
        public string vads_return_mode { get; set; }
        public string vads_order_id { get; set; }
        public string vads_order_info { get; set; }
        public string vads_order_info2 { get; set; }
        public string vads_language { get; set; }
        public string vads_url_return { get; set; }




    }

}
