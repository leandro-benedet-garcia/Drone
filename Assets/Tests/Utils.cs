using System.Reflection;

public static class Utils
{
    public static object CallPrivateMethod<T>(T instance, string methodName, object[] parameters)
    {
        var methodInfo = typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if(methodInfo == null) throw new($"Method {methodName} does not exist in class {nameof(T)}");
        return methodInfo.Invoke(instance, parameters);
    }
}
