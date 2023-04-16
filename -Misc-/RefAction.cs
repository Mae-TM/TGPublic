public delegate void RefAction<in T1, T2>(T1 arg1, ref T2 arg2);
public delegate void RefAction<T>(ref T arg);
