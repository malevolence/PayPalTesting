using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication2.Utils;

namespace WebApplication2.Controllers
{
    public class CardsController : Controller
    {
		private string customerId = "";

		public CardsController()
		{
			this.customerId = Common.GetCustomerId();
		}

		public ActionResult Index()
		{
			var apiContext = Common.GetApiContext();

			var cards = CreditCard.List(apiContext, externalCustomerId: customerId);

			return View(cards);
		}

		public ActionResult Details(string id)
		{
			var apiContext = Common.GetApiContext();

			var creditCard = CreditCard.Get(apiContext, id);

			return View(creditCard);
		}

		public ActionResult Edit(string id)
		{
			var apiContext = Common.GetApiContext();

			var creditCard = CreditCard.Get(apiContext, id);

			AddPaymentDropdowns();
			return View(creditCard);
		}

		[HttpPost]
		public ActionResult Edit(string id, CreditCard creditCard)
		{
			if (ModelState.IsValid)
			{
				var apiContext = Common.GetApiContext();

				creditCard.type = Common.GetCardType(creditCard.number);
				var existing = CreditCard.Get(apiContext, id);
				if (existing != null)
				{
					var patchRequest = new PatchRequest();

					// determine what's changed between the existing card
					// and the posted values
					if (creditCard.expire_month != existing.expire_month)
					{
						patchRequest.Add(new Patch { op = "replace", path = "/expire_month", value = creditCard.expire_month });
					}

					if (creditCard.expire_year != existing.expire_year)
					{
						patchRequest.Add(new Patch { op = "replace", path = "/expire_year", value = creditCard.expire_year });
					}

					if (!string.IsNullOrEmpty(creditCard.first_name) && creditCard.first_name != existing.first_name)
					{
						patchRequest.Add(new Patch { op = "replace", path = "/first_name", value = creditCard.first_name });
					}

					if (!string.IsNullOrEmpty(creditCard.last_name) && creditCard.last_name != existing.last_name)
					{
						patchRequest.Add(new Patch { op = "replace", path = "/last_name", value = creditCard.last_name });
					}

					if (patchRequest.Count > 0)
					{
						existing.Update(apiContext, patchRequest);

						TempData["success"] = "Stored Card updated.";

						return RedirectToAction("Details", new { id });
					}
					else
					{
						TempData["info"] = "Nothing to update";
					}
				}
			}
			else
			{
				TempData["info"] = "ModelState is invalid";
			}

			AddPaymentDropdowns();
			return View(creditCard);
		}

		public ActionResult Create()
		{
			AddPaymentDropdowns();
			return View();
		}

		[HttpPost]
		public ActionResult Create(CreditCard creditCard)
		{
			if (ModelState.IsValid)
			{
				var apiContext = Common.GetApiContext();

				creditCard.external_customer_id = customerId;
				creditCard.type = Common.GetCardType(creditCard.number);
				creditCard.Create(apiContext);

				TempData["success"] = "Credit card stored successfully";

				return RedirectToAction("Index");
			}

			AddPaymentDropdowns();
			return View(creditCard);
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			var apiContext = Common.GetApiContext();

			var creditCard = CreditCard.Get(apiContext, id);

			creditCard.Delete(apiContext);

			TempData["success"] = "Credit card deleted successfully";

			return RedirectToAction("Index");
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