using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec.Legacy
{
    /// <summary>
    /// Summary description for NetUtils.
    /// </summary>
    [Serializable]
    public abstract class NetComponent : System.ComponentModel.Component
    {
        //internal static bool Permit = false;

        internal NetComponent()
        {
            //if (!NetComponent.Permit)
            //{
            //    NetComponent.Permit = MControl.Net.MessagingNet.NetFram("NetUtils", "CTL");
            //}
        }


    }
}
