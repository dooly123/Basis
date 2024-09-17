/*
Copyright (c) 2022 Unordinal AB

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
///     Various shared methods for use in UnityServer and UnityServerEditor.
/// </summary>
public static class UnityServerHelper
{
    /// <summary>
    ///     Searches the app domain for plugin types. 
    /// </summary>
    /// <returns>The plugin types in the app domain.</returns>
    public static IEnumerable<Type> SearchForPlugins()
    {
        //Omit DarkRift server assembly so internal plugins aren't loaded twice
        Assembly[] omit = new Assembly[]
        {
            Assembly.GetAssembly(typeof(DarkRiftServer))
        };

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Except(omit))
        {
            IEnumerable<Type> types = new Type[0];
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                Debug.LogWarning("An assembly could not be loaded while searching for plugins. This could be because it is an unmanaged library.");
            }

            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(PluginBase)) && !type.IsAbstract)
                    yield return type;
            }
        }
    }
}
