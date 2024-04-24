using System;
using Microsoft.VisualBasic;
using Python.Runtime;

internal sealed class CloudServices
{
    private readonly PyModule scope;
    public CloudServices()
    {
        string[] fileEntries = Directory.GetFiles("./PythonLibs");
        Runtime.PythonDLL = fileEntries[0];
        // Runtime.PythonDLL = @"/usr/local/Library/Frameworks/Python.framework/Versions/3.11/lib/libpython3.11.dylib";
        // Runtime.PythonDLL = "/usr/local/Library/Frameworks/Python.framework/Versions/3.11/Python/"
        // Runtime.PythonDLL = AppContext.BaseDirectory + "../../../PythonLibs/libpython3.11.dylib";
        PythonEngine.Initialize();
        using (Py.GIL())
        {
            scope = Py.CreateScope();
        }
    }
    public void ExecutePython()
    {
        scope.Exec("print('Hello From Python')");

    }
}