﻿using DigitalPlatform.IO;
using DigitalPlatform.LibraryRestClient;
using DigitalPlatform.Text;
using dp2Command.Service;
using dp2weixin;
using dp2weixin.service;
using dp2weixinWeb.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace dp2weixinWeb.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index(string code, string state, string weiXinId)
        {
            return Redirect("/Library/Home?code=" + code
                + "&state=" + state 
                + "&weiXinId=" + weiXinId);
        }

        // 超级管理员登录
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginModel model,string returnUrl)
        {
            Session["supervisor"] = true;

            string error = "";
            if (model.UserName != "1" || model.Password != "1")
            {
                error = "账户或密码不正确。";
            }

            if (error == "")
            {
                if (string.IsNullOrEmpty(returnUrl) == false)
                    return Redirect(returnUrl);
                else
                    return Redirect("/Home/Manager");
            }

            ViewBag.Error = error;
            return View();
        }

        private bool CheckSupervisorLogin()
        {
            if (Session["supervisor"] != null && (bool)Session["supervisor"] == true)
            {
                return true;
            }

            return false;
        }

        // Manager
        public ActionResult Manager()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("/Home/Login?returnUrl=" + HttpUtility.UrlEncode("/Home/Manager"));
            }

            return View();
        }

        // 参数配置
        public ActionResult Setting()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("/Home/Login?returnUrl=" + HttpUtility.UrlEncode("/Home/Setting"));
            }

            ViewBag.success = false;

            SettingModel model = new SettingModel();
            model.dp2MserverUrl = dp2WeiXinService.Instance.dp2MServerUrl;// "";// dp2MServerUrl;
            model.userName = dp2WeiXinService.Instance.userName;// "";//userName;
            model.password = dp2WeiXinService.Instance.password;// "";//password;

            return View(model);
        }
        [HttpPost]
        public ActionResult Setting(SettingModel model)
        {
            ViewBag.success = false;

            string strError = "";
            int nRet = dp2WeiXinService.Instance.SetDp2mserverInfo(model.dp2MserverUrl,
                model.userName,
                model.password,
                out strError); //函数里面会将密码加密
            if (nRet == -1)
            {
                ViewBag.success = false;
                ViewBag.Error = strError;
            }
            else
                ViewBag.success = true;  
            return View(model);
        }

        public ActionResult LibraryM()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("/Home/Login?returnUrl=" + HttpUtility.UrlEncode("/Home/LibraryM"));
            }


            return View();
        }

        //WeixinUser
        public ActionResult WeixinUser(string code, string state)
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("/Home/Login?returnUrl=" + HttpUtility.UrlEncode("/Home/WeixinUser"));
            }

            return View();
        }

        //WeixinMessage
        public ActionResult WeixinMessage()
        {
            if (CheckSupervisorLogin() == false)
            {
                return Redirect("/Home/Login?returnUrl=" + HttpUtility.UrlEncode("/Home/WeixinMessage"));
            }

            MessageModel model = new MessageModel();
            return View(model);
        }

        [HttpPost]
        //WeixinMessage
        public ActionResult WeixinMessage(MessageModel model)
        {
            string msgSend = model.RequestMsg;//Request["txtMessage"];
            string resultStr = "";
            try
            {
                CookieContainer cookies = new System.Net.CookieContainer();
                CookieAwareWebClient client = new CookieAwareWebClient(cookies);
                client.Headers["Content-type"] = "application/xml; charset=utf-8";
                string xml = WeiXinClientUtil.GetPostXmlToWeiXinGZH(msgSend);
                byte[] baData = Encoding.UTF8.GetBytes(xml);
                string url = "http://localhost:15794/weixin/index";
                byte[] result = client.UploadData(url,
                    "POST",
                    baData);
                string strResult = Encoding.UTF8.GetString(result);
                resultStr = strResult;
            }
            catch (Exception ex)
            {
                resultStr = "Exception :" + ex.Message;
            }

            model.ResponseMsg = resultStr;
            return View(model);
        }


        public ActionResult About(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            return View();
        }


        public ActionResult Contact(string code, string state)
        {
            // 检查是否从微信入口进来
            string strError = "";
            int nRet = this.CheckIsFromWeiXin(code, state, out strError);
            if (nRet == -1)
                return Content(strError);

            return View();
        }


	}
}