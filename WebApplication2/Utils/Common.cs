using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication2.Utils
{
	public static class Common
	{
        public static string GetRandomInvoiceNumber()
        {
            return new Random().Next(999999).ToString();
        }
	}
}