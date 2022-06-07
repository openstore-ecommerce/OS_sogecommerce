using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace OS_sogecommerce
{
    public class OS_sogecommercePaymentProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.PaymentsInterface
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var templ = "";
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_sogecommercepayment", "OS_sogecommercePAYMENT", Utils.GetCurrentCulture());
            var templateName = info.GetXmlProperty("genxml/textbox/checkouttemplate");
            var passSettings = info.ToDictionary();
            foreach (var s in StoreSettings.Current.Settings()) // copy store setting, otherwise we get a byRef assignement
            {
                if (passSettings.ContainsKey(s.Key))
                    passSettings[s.Key] = s.Value;
                else
                    passSettings.Add(s.Key, s.Value);
            }
            templ = NBrightBuyUtils.RazorTemplRender(templateName, 0, "", info, "/DesktopModules/NBright/OS_sogecommerce", "config", Utils.GetCurrentCulture(), passSettings);

            return templ;
        }

        public override string RedirectForPayment(OrderData orderData)
        {
            orderData.OrderStatus = "020";
            orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "");
            orderData.PurchaseInfo.Lang = Utils.GetCurrentCulture();
            orderData.SavePurchaseData();
            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write(ProviderUtils.GetBankRemotePost(orderData));
            }
            catch (Exception ex)
            {
                // rollback transaction
                orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "<div>ERROR: Invalid payment data </div><div>" + ex + "</div>");
                orderData.PaymentFail();
                var param = new string[3];
                param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                return Globals.NavigateURL(StoreSettings.Current.PaymentTabId, "", param);
            }

            try
            {
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                // this try/catch to avoid sending error 'ThreadAbortException'  
            }

            return "";
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            // vads fields are always passed back on return
            var orderid = context.Request.Form.Get("vads_order_id");
            if (!Utils.IsNumeric(orderid))
            {
                orderid = Utils.RequestParam(context, "orderid");
            }
            string clientlang = context.Request.Form.Get("vads_order_info");
            if (Utils.IsNumeric(orderid))
            {
                var status = context.Request.Form.Get("vads_result");
                if (string.IsNullOrEmpty(status))
                {
                    status = Utils.RequestQueryStringParam(context, "status");
                }

                var orderData = new OrderData(Convert.ToInt32(orderid));
                if ((status != "00" || status == "0") && orderData.IsNotPaid())
                {
                    var rtnerr = orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror");
                    if (rtnerr == "") rtnerr = "fail"; // to return this so a fail is activated.

                    orderData.AddAuditMessage(rtnerr, "paymsg", "payment.ascx", "False");
                    orderData.Save();
                    // check we have a waiting for bank status (IPN may have altered status already + help stop hack)
                    if (orderData.OrderStatus == "020")
                    {
                        orderData.PaymentFail(); // paymentfailed will move order back to cart.
                    }
                    return GetReturnTemplate(orderData, false, rtnerr);
                }

                // check we have a waiting for bank status (IPN may have altered status already + help stop hack)
                if (orderData.OrderStatus == "020")
                {
                    orderData.PaymentOk("050"); // order paid, but NOT verified
                }
                return GetReturnTemplate(orderData, true, "");
            }
            return "";
        }


        private string GetReturnTemplate(OrderData orderData, bool paymentok, string paymenterror)
        {
            var info = ProviderUtils.GetProviderSettings();
            info.UserId = UserController.Instance.GetCurrentUserInfo().UserID;
            var templ = "";
            var passSettings = NBrightBuyUtils.GetPassSettings(info);
            if (passSettings.ContainsKey("paymenterror"))
            {
                passSettings.Add("paymenterror", paymenterror);
            }
            var displaytemplate = "payment_ok.cshtml";
            if (paymentok)
            {
                info.SetXmlProperty("genxml/ordernumber", orderData.OrderNumber);
                templ = NBrightBuyUtils.RazorTemplRender(displaytemplate, 0, "", info, "/DesktopModules/NBright/OS_sogecommerce", "config", Utils.GetCurrentCulture(), passSettings);
            }
            else
            {
                displaytemplate = "payment_fail.cshtml";
                templ = NBrightBuyUtils.RazorTemplRender(displaytemplate, 0, "", info, "/DesktopModules/NBright/OS_sogecommerce", "config", Utils.GetCurrentCulture(), passSettings);
            }

            return templ;
        }

    }
}
