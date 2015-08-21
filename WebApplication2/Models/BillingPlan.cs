using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication2.Models
{
	public class BillingPlan
	{
		public string Name { get; set; }
		public string Description { get; set; }

		[Display(Name = "Type")]
		public string PlanType { get; set; }

		[Display(Name = "Billing Frequency")]
		public BillingFrequency Frequency { get; set; }

		[Display(Name = "Number of Cycles")]
		public int? NumberOfCycles { get; set; }

		[DataType(DataType.Currency)]
		public decimal Amount { get; set; }
	}

	public enum BillingFrequency
	{
		Day,
		Week,
		Month,
		Year
	};
}