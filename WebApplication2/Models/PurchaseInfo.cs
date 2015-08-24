using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
	public class PurchaseInfo
	{
		[DataType(DataType.Currency)]
		public decimal? Amount { get; set; }

		[Display(Name = "Is Recurring?")]
		public bool IsRecurring { get; set; }

		[Display(Name = "Billing Plan")]
		public string BillingPlanId { get; set; }

		[Display(Name = "Billing Period")]
		public BillingPeriod? BillingPeriod { get; set; }

		[Display(Name = "Number of Cycles")]
		public int? NumberOfCycles { get; set; }

		[Display(Name = "Saved Credit Card")]
		public string CreditCardId { get; set; }

		[Display(Name = "Save Payment Info")]
		public bool SavePaymentInfo { get; set; }

		[Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Display(Name = "Credit Card Number")]
		public string CreditCardNumber { get; set; }

		public string CVV2 { get; set; }

		[Display(Name = "Expiring Month")]
		public int ExpMonth { get; set; }

		[Display(Name = "Expiring Year")]
		public int ExpYear { get; set; }

	}
}