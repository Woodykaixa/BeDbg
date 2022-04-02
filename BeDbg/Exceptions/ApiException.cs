namespace BeDbg.Exceptions;

public class ApiException : Exception
{
	public string ApiMessage { get; set; }
	public uint Code { get; set; }
	public uint Module { get; set; }

	public ApiException(string message, uint code, uint module) : base(
		$"Api Error: Module: {module}, Code: {code}, Message: {message}")
	{
		ApiMessage = message;
		Code = code;
		Module = module;
	}

	public override string ToString()
	{
		var err = base.ToString();
		var custom = $"Api Error: Module: {this.Module}, Code: {this.Code}, Message: {this.ApiMessage}\n";
		Console.Error.WriteLine(custom);
		return custom + err;
	}
}