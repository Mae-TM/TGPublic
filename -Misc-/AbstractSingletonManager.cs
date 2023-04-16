public abstract class AbstractSingletonManager<T> where T : AbstractSingletonManager<T>, new()
{
	private static T _instance;

	public static T Instance => _instance ?? (_instance = new T());
}
