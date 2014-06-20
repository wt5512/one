using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
namespace Jujin.SMS
{
    public interface ISMS
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobile">手机号</param>
        /// <param name="content">短信内容</param>
        /// <returns></returns>
        bool SendSingleSMS(string mobile, string content);
        ///// <summary>
        ///// 修改短信密码
        ///// </summary>
        ///// <param name="username">账号</param>
        ///// <param name="oldpassword">原密码</param>
        ///// <param name="newpassword">新密码</param>
        ///// <returns></returns>
        //bool ModifyPassword(string username, string oldpassword, string newpassword);

        ///// <summary>
        ///// 查询余额
        ///// </summary>
        ///// <param name="username">账号</param>
        ///// <param name="password">密码</param>
        ///// <returns></returns>
        //decimal GetCredit(string username, string password);

        ///// <summary>
        ///// 查询发送记录
        ///// </summary>
        ///// <param name="username">账号</param>
        ///// <param name="password">密码</param>
        ///// <param name="page">页码</param>
        ///// <returns></returns>
        //DataTable GetJournal(string username, string password, int page);

        ///// <summary>
        ///// 返回单条短信金额
        ///// </summary>
        ///// <param name="username">账号</param>
        ///// <param name="password">密码</param>
        ///// <returns></returns>
        //decimal GetPrice(string username, string password);

    }
}
