﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUi.Infrastructure
{
    public static class SessionExtensions
    {
        public static T GetDataFromSession<T>(this HttpSessionStateBase session, string key)
        {
             return (T)session[key];
        }

        public static void SetDataInSession<T>(this HttpSessionStateBase session, string key, T value)
        {
            session[key] = value;
        }
    }
}