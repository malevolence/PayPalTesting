using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using WebApplication2.Models;
using WebApplication2.Utils;
using System.Globalization;

namespace WebApplication2.Controllers
{
    public class PaymentsController : Controller
    {
		private const string customerId = "malevolence";

        // GET: Payments
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult Cards()
		{
			var config = ConfigManager.Instance.GetProperties();
			var accessToken = new OAuthTokenCredential(config).GetAccessToken();
			var apiContext = new APIContext(accessToken);

			var cards = CreditCard.List(apiContext, externalCustomerId: customerId);

			return View(cards);
		}

		public ActionResult DetailsCard(string id)
		{
			var config = ConfigManager.Instance.GetProperties();
			var accessToken = new OAuthTokenCredential(config).GetAccessToken();
			var apiContext = new APIContext(accessToken);

			var creditCard = CreditCard.Get(apiContext, id);

			return View(creditCard);
		}

		[HttpPost]
		public ActionResult SaveCard(CreditCard creditCard)
		{
			var config = ConfigManager.Instance.GetProperties();
			var accessToken = new OAuthTokenCredential(config).GetAccessToken();
			var apiContext = new APIContext(accessToken);

			creditCard.external_customer_id = customerId;
			creditCard.type = "visa";
			creditCard.Create(apiContext);

			TempData["success"] = "Credit card stored successfully";

			return RedirectToAction("Cards");
		}

		[HttpPost]
		public ActionResult DeleteCard(string id)
		{
			var config = ConfigManager.Instance.GetProperties();
			var accessToken = new OAuthTokenCredential(config).GetAccessToken();
			var apiContext = new APIContext(accessToken);

			var creditCard = CreditCard.Get(apiContext, id);

			creditCard.Delete(apiContext);

			TempData["success"] = "Credit card deleted successfully";

			return RedirectToAction("Cards");
		}

		public ActionResult Create()
		{
			AddPaymentDropdowns();
			return View();
		}

		[HttpPost]
		[ActionName("Create")]
		public ActionResult CreatePurchase(PurchaseInfo purchaseInfo)
		{
			try
			{
				var config = ConfigManager.Instance.GetProperties();
				var accessToken = new OAuthTokenCredential(config).GetAccessToken();
				var apiContext = new APIContext(accessToken);

				var payer = new Payer
				{
					payment_method = "credit_card",
					funding_instruments = new List<FundingInstrument>()
					{
						new FundingInstrument()
						{
							credit_card = new CreditCard()
							{
								billing_address = new Address()
								{
									city = "Orlando",
									country_code = "US",
									line1 = "123 Test Way",
									postal_code = "32803",
									state = "FL"
								},
								cvv2 = purchaseInfo.CVV2,
								expire_month = purchaseInfo.ExpMonth,
								expire_year = purchaseInfo.ExpYear,
								first_name = purchaseInfo.FirstName,
								last_name = purchaseInfo.LastName,
								number = purchaseInfo.CreditCardNumber,
								type = "visa"
							}
						}
					},
					payer_info = new PayerInfo
					{
						email = "email@example.com"
					}
				};

				var transaction = new Transaction
				{
					amount = new Amount
					{
						currency = "USD",
						total = purchaseInfo.Amount.ToString()
					},
					description = "Featured Profile on ProductionHUB",
					invoice_number = Common.GetRandomInvoiceNumber()
				};

				var payment = new Payment()
				{
					intent = "sale",
					payer = payer,
					transactions = new List<Transaction>() { transaction }
				};

				var createdPayment = payment.Create(apiContext);

				TempData["info"] = createdPayment.id;

				if (createdPayment.state == "approved")
				{
					return RedirectToAction("Completed");
				}
				else
				{
					return RedirectToAction("Rejected");
				}
			}
			catch (Exception exc)
			{
				TempData["error"] = exc.Message;
			}

			AddPaymentDropdowns();
			return View();
		}

		public ActionResult Completed()
		{
			return View();
		}

		public ActionResult Rejected()
		{
			return View();
		}

		private void AddPaymentDropdowns()
		{
			var currentYear = DateTime.Today.Year;
			var years = new List<SelectListItem>();
			for (int i = 0; i < 10; i++)
			{
				years.Add(new SelectListItem { Text = (i + currentYear).ToString(), Value = (i + currentYear).ToString() });
			}

			ViewBag.PossibleYears = years;
			var months = new List<SelectListItem>();
			for (int i = 1; i <= 12; i++)
			{
				months.Add(new SelectListItem { Value = i.ToString(), Text = string.Format("{0} - {1}", i, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i)) });
			}

			ViewBag.PossibleMonths = months;
		}
    }
}