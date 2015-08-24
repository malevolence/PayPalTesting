using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using WebApplication2.Utils;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class PlansController : Controller
    {
        public ActionResult Index()
        {
			var apiContext = Common.GetApiContext();

			var plans = Plan.List(apiContext, status: "ACTIVE", pageSize: "20");

			return View(plans);
        }

		public ActionResult Create()
		{
			AddDropdowns();
			return View();
		}

		[HttpPost]
		public ActionResult Create(BillingPlan billingPlan)
		{
			if (ModelState.IsValid)
			{
				var apiContext = Common.GetApiContext();

				var plan = new Plan();
				plan.description = billingPlan.Description;
				plan.name = billingPlan.Name;
				plan.type = billingPlan.PlanType;

				plan.merchant_preferences = new MerchantPreferences
				{
					initial_fail_amount_action = "CANCEL",
					max_fail_attempts = "3",
					cancel_url = "http://localhost:50728/plans",
					return_url = "http://localhost:50728/plans"
				};

				plan.payment_definitions = new List<PaymentDefinition>();

				var paymentDefinition = new PaymentDefinition();
				paymentDefinition.name = "Standard Plan";
				paymentDefinition.amount = new Currency { currency = "USD", value = billingPlan.Amount.ToString() };
				paymentDefinition.frequency = billingPlan.Frequency.ToString();
				paymentDefinition.type = "REGULAR";
				paymentDefinition.frequency_interval = "1";

				if (billingPlan.NumberOfCycles.HasValue)
				{
					paymentDefinition.cycles = billingPlan.NumberOfCycles.Value.ToString();
				}

				plan.payment_definitions.Add(paymentDefinition);

				var created = plan.Create(apiContext);

				if (created.state == "CREATED")
				{
					var patchRequest = new PatchRequest();
					patchRequest.Add(new Patch { path = "/", op = "replace", value = new Plan() { state = "ACTIVE" } });
					created.Update(apiContext, patchRequest);
				}

				TempData["success"] = "Billing plan created.";
				return RedirectToAction("Index");
			}

			AddDropdowns();
			return View(billingPlan);
		}

		public ActionResult Details(string id)
		{
			var apiContext = Common.GetApiContext();

			var plan = Plan.Get(apiContext, id);

			return View(plan);
		}

		[HttpPost]
		public ActionResult Activate(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				var apiContext = Common.GetApiContext();
				var plan = Plan.Get(apiContext, id);
				if (plan != null && plan.state == "CREATED")
				{
					var patchRequest = new PatchRequest();
					var tempPlan = new Plan();
					tempPlan.state = "ACTIVE";
					patchRequest.Add(new Patch { path = "/", op = "replace", value = tempPlan });
					plan.Update(apiContext, patchRequest);

					TempData["success"] = "Plan activated";
				}
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult Delete(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				var apiContext = Common.GetApiContext();
				var plan = Plan.Get(apiContext, id);

				if (plan != null)
				{
					plan.Delete(apiContext);
					TempData["success"] = "Plan deleted";
				}
			}

			return RedirectToAction("Index");
		}

		private void AddDropdowns()
		{
			var types = new List<SelectListItem>
			{
				new SelectListItem { Text = "Fixed", Value = "FIXED" },
				new SelectListItem { Text = "Infinite", Value = "INFINITE" }
			};

			ViewBag.PossibleTypes = types;
		}
    }
}