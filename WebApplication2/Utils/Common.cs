using PayPal.Api;
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

		public static string GetCustomerId()
		{
			return "malevolence";
		}

		public static APIContext GetApiContext()
		{
			var config = ConfigManager.Instance.GetProperties();
			var accessToken = new OAuthTokenCredential(config).GetAccessToken();
			return new APIContext(accessToken);
		}

		public static bool IsValidCardNumber(string cardNumber)
		{
			// Luhn validator
			int cardSum = 0;

			if (!string.IsNullOrEmpty(cardNumber))
			{
				cardNumber = cardNumber.Trim();

				int currentProcNum = 0;
				int cardLength = cardNumber.Length;
				int currentDigit;

				for (int i = cardLength - 1; i >= 0; i--)
				{
					string idCurrentRightmostDigit = cardNumber.Substring(i, 1);

					if (!int.TryParse(idCurrentRightmostDigit, out currentDigit))
						return false;

					if (currentProcNum % 2 != 0)
					{
						if ((currentDigit *= 2) > 9)
							currentDigit -= 9;
					}
					currentProcNum++;
					cardSum += currentDigit;
				}
			}

			return (cardSum % 10 == 0);
		}

		public static string GetCardType(string cardNumber)
		{
			string response = "Unknown";

			try
			{
				if (!string.IsNullOrEmpty(cardNumber))
				{
					cardNumber = cardNumber.Trim();

					if (Convert.ToInt32(cardNumber.Substring(0, 2)) >= 51 && Convert.ToInt32(cardNumber.Substring(0, 2)) <= 55)
						response = "Mastercard";
					else if (cardNumber.Substring(0, 1) == "4")
						response = "Visa";
					else if (cardNumber.Substring(0, 2) == "34" || cardNumber.Substring(0, 2) == "37")
						response = "Amex";
					else if (cardNumber.Substring(0, 4) == "6011")
						response = "Discover";
				}

			}
			catch {}

			return response.ToLower();
		}
	}
}