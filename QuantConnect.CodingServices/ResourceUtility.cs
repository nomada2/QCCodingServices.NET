using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace QuantConnect.CodingServices
{
    public static class ResourceUtility
    {
        public static string GetTextFromEmbeddedFile(string fileName)
        {
            var stream = GetEmbeddedFileFromExecutingAssembly(fileName);
            var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Extracts an embedded file from the given assembly.
        /// </summary>
        /// <param name="fileName">The name of the file to extract.  This need not be the full name.</param>
        /// <returns>A stream containing the file data.</returns>
        public static Stream GetEmbeddedFileFromExecutingAssembly(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return GetEmbeddedFile(assembly, fileName);
        }

        /// <summary>
        /// Extracts an embedded file from the given assembly.
        /// </summary>
        /// <param name="assembly">The namespace of you assembly.</param>
        /// <param name="fileName">The name of the file to extract.  This need not be the full name.</param>
        /// <returns>A stream containing the file data.</returns>
        public static Stream GetEmbeddedFile(Assembly assembly, string fileName)
        {
            string assemblyFullName = assembly.FullName;
            string assemblyName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(","));

            try
            {
                string resourceName = string.Empty;
                string[] names = assembly.GetManifestResourceNames();
                foreach (string name in names)
                {
                    if (name.Contains(fileName))
                    {
                        resourceName = name;
                        break;
                    }
                }

                //System.Reflection.Assembly a = System.Reflection.Assembly.Load(assemblyName);
                if (resourceName == string.Empty)
                    throw new Exception("Could not locate embedded resource '" + fileName + "' in assembly '" + assemblyName + "'");
                Stream str = assembly.GetManifestResourceStream(resourceName);

                if (str == null)
                    throw new Exception("Could not locate embedded resource '" + fileName + "' in assembly '" + assemblyName + "'");
                return str;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("{0} occurred while trying to get resource '{1}' from assembly {2}: {3}",
                    ex.GetType().FullName, fileName, assemblyName, ex.Message), ex);
            }
        }
    }
}
