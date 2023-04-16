public class Settings
{
	private const string BASE = "s://genesis-project.herokuapp.com/";

	private static string Base => "s://genesis-project.herokuapp.com/";

	public static string WsUrl => "ws" + Base + "ws";

	public static string APIURL => "http" + Base + "api";

	public static string RegisterUrl => "http" + Base + "register/";

	public static string HomeUrl => "http" + Base;
}
