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
		private string customerId = "";
		private PlanList plans;
		private APIContext apiContext;

		public PaymentsController()
		{
			this.apiContext = Common.GetApiContext();
			this.customerId = Common.GetCustomerId();
			this.plans = Plan.List(apiContext, status: "ACTIVE");
		}

        // GET: Payments
        public ActionResult Index()
        {
			var payments = Payment.List(apiContext);

            return View(payments);
        }

		public ActionResult Create()
		{
			ViewBag.Cards = CreditCard.List(apiContext, externalCustomerId: customerId);
			ViewBag.Plans = plans;

			AddPaymentDropdowns();
			return View();
		}

		[HttpPost]
		[ActionName("Create")]
		public ActionResult CreatePurchase(PurchaseInfo purchaseInfo)
		{
			try
			{
				string action = "Index";

				var payer = new Payer
				{
					payment_method = "credit_card",
					funding_instruments = new List<FundingInstrument>(),
					payer_info = new PayerInfo
					{
						email = "email@example.com"
					}
				};

				var creditCard = new CreditCard();

				if (!string.IsNullOrEmpty(purchaseInfo.CreditCardId))
				{
					payer.funding_instruments.Add(new FundingInstrument()
					{
						credit_card_token = new CreditCardToken()
						{
							credit_card_id = purchaseInfo.CreditCardId
						}
					});
				}
				else
				{
					creditCard = new CreditCard()
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
						type = Common.GetCardType(purchaseInfo.CreditCardNumber)
					};

					payer.funding_instruments.Add(new FundingInstrument()
					{
						credit_card = creditCard
					});
				}

				if (!purchaseInfo.IsRecurring)
				{
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
						action = "Completed";
					}
					else
					{
						action = "Rejected";
					}
				}
				else
				{
					var agreement = new Agreement()
					{
						name = "Basic profile",
						description = "Monthly basic profile in perpetuity",
						payer = payer,
						plan = new Plan { id = purchaseInfo.BillingPlanId },
						start_date = DateTime.UtcNow.AddDays(1).ToString("u").Replace(" ", "T"),
					};

					var createdAgreement = agreement.Create(apiContext);

					TempData["info"] = createdAgreement.create_time;

					TempData["success"] = "Recurring agreement created";
				}

				if (purchaseInfo.SavePaymentInfo)
				{
					creditCard.external_customer_id = customerId;
					creditCard.Create(apiContext);
				}

				return RedirectToAction(action);

			}
			catch (Exception exc)
			{
				TempData["error"] = exc.Message;
			}

			ViewBag.Cards = CreditCard.List(apiContext, externalCustomerId: customerId);
			ViewBag.Plans = plans;
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