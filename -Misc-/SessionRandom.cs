using UnityEngine;

public static class SessionRandom
{
	public static int seed;

	private static int _currentState;

	public static int CurrentState => _currentState;

	public static void Seed(int localSeed = 0)
	{
		_currentState = seed ^ localSeed;
		Random.InitState(seed ^ localSeed);
	}
}
