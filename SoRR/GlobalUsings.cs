
// UnityEngine.Debug.Assert doesn't work as you would expect it to.
// Instead of throwing on assertion error, it only logs it instead.
global using Debug = System.Diagnostics.Debug;
