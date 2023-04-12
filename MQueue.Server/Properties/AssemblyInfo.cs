using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

//#if(CLIENT)
//[assembly: AssemblyTitle("Nistec.Client.Messaging")]
//[assembly: AssemblyFileVersion("3.5.0.0")]
//#else
[assembly: AssemblyTitle("Nistec.Queue.Server")]
[assembly: AssemblyDescription("Messaging Framework")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Nistec.Net")]
[assembly: AssemblyProduct("Nistec.Queue.Server")]
[assembly: AssemblyCopyright("Copyright © Nistec.Net 2006")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("69b53017-4535-406e-98e2-db3e45342427")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:
[assembly: AssemblyVersion("4.0.2.0")]
[assembly: AssemblyFileVersion("4.0.2.4")]



#region MessagingNet

namespace Nistec.Net
{

    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    //using Nistec.Net.License;


    /// <summary>
    /// netDal
    /// </summary>
    [Serializable]
    internal sealed class MessagingNet
    {

        #region Members

        //private static bool _IsNet = false;
        private const string ctlVersion = "3.5.0.0";
        private const string ctlNumber = "3be7ef6f-05b2-4225-974d-1ae3156630bb";
        private const string ctlName = "Framework";

        #endregion

        #region Static

        private static bool IsMControl()
        {
            try
            {
               // byte[] pk1 = Nistec.Net.License.nf_1.nf_4().GetName().GetPublicKeyToken();
               /// byte[] pk2 = System.Reflection.Assembly.GetAssembly(typeof(Nistec.Net.MessagingNet)).GetName().GetPublicKeyToken();

                return true;// Nistec.Net.License.nf_1.ed_7(pk1).Equals(Nistec.Net.License.nf_1.ed_7(pk2));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        internal static bool NetFram(string method, string mode)
        {

            return true;

//            try
//            {

//#if(DEBUG)
//                _IsNet = true;
//#endif

//                if (!_IsNet)
//                {

//                    if (!IsMControl())
//                    {
//                        throw new ArgumentException("Invalid Nistec.Net Reference", "IsMControl");
//                    }
//#if(CLIENT)
//                    System.Reflection.MethodBase methodBase = (System.Reflection.MethodBase)(new System.Diagnostics.StackTrace().GetFrame(2).GetMethod());
//                    //_IsNet = Nistec.Util.Net.nf_1.nf_6(ctlName, ctlVersion, method, mode,methodBase);
//                    _IsNet = Nistec.Net.License.nf_1.nf_6(ctlName, ctlVersion, method, mode,methodBase);
//#else
//                    //_IsNet = Nistec.Util.Net.nf_1.nf_6(ctlName, ctlVersion, method, mode);
//                    _IsNet = Nistec.Net.License.nf_1.nf_6(ctlName, ctlVersion, method, mode);

//#endif

//                    if (!_IsNet)
//                    {
//                        throw new ArgumentException("Invalid Nistec.Net Reference", "Framework");
//                    }
//                }
//                return _IsNet;
//            }
//            catch (ArgumentException ex)
//            {
//                throw ex;
//            }
//            catch (Exception)
//            {
//                throw new ArgumentNullException("Nistec.Net", "Invalid Nistec.Net Reference");
//            }
        }

        #endregion
    }
}
#endregion