using Nistec.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nistec
{
    [Serializable]
    public class ContactEntity : IEntityItem
    {
        #region properties

        [EntityProperty(EntityPropertyType.Identity, Caption = "Contact ID")]
        public int ContactID
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "FirstName", Caption = "First Name")]
        public string FirstName
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "LastName", Caption = "Last Name")]
        public string LastName
        {
            get;
            set;
        }


        [EntityProperty(EntityPropertyType.Default, false, Column = "EmailAddress", Caption = "Email Address")]
        public string EmailAddress
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "Email Promotion")]
        public int EmailPromotion
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Column = "Phone", Caption = "Phone")]
        public string Phone
        {
            get;
            set;
        }

        [EntityProperty(EntityPropertyType.Default, false, Caption = "Name Style")]
        public bool NameStyle
        {
            get;
            set;
        }
        [EntityProperty(EntityPropertyType.Default, false, Caption = "Modifie dDate")]
        public DateTime ModifiedDate
        {
            get;
            set;
        }



        #endregion

        #region override

        public override string ToString()
        {
            return string.Format("FirstName:{0},LastName:{1},Phone:{2}", FirstName, LastName, Phone);
        }
        #endregion
    }
}
