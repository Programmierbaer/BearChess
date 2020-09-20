using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using www.SoLaNoSoft.com.BearChessBase.Interfaces;

namespace www.SoLaNoSoft.com.BearChessBase.Implementations
{
    /// <inheritdoc />
    public class EngineLoader : IEngineLoader
    {
        /// <inheritdoc />
        public IChessEngine LoadEngine(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            var assembly = Assembly.LoadFrom(fileName);

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass &&
                    type.GetInterface("www.SoLaNoSoft.com.BearChessBase.Interfaces.IEngineProvider") != null)
                {
                    var parameterTypes = new Type[0];
                    var constructorInfo = type.GetConstructor(parameterTypes);
                    var constructorParams = new object[0];
                    if (constructorInfo != null)
                    {
                        var invoke = constructorInfo.Invoke(constructorParams);
                        return ((IEngineProvider)invoke).Engine;
                    }
                }
            }
            throw new FileLoadException("Datei ist keine Engine vom Typ Bear Chess", fileName);
        }
    }
}
