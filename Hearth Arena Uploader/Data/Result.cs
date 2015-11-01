using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthArenaUploader.Data
{
	public class Result<ResultDataType, ResultType>
	{
		public Result(ResultDataType resultData, ResultType outcome, string errorMessage)
		{
			ResultData = resultData;
			Outcome = outcome;
			ErrorMessage = errorMessage;
		}

		public ResultDataType ResultData { get; private set; }

		public ResultType Outcome { get; private set; }

		public string ErrorMessage { get; private set; }
	}

	public class Result<ResultType>
	{
		public Result(ResultType outcome, string errorMessage)
		{
			Outcome = outcome;
			ErrorMessage = errorMessage;
		}		

		public ResultType Outcome { get; private set; }

		public string ErrorMessage { get; private set; }
	}
}
