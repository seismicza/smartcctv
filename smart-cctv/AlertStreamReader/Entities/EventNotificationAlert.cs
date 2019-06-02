using System;
using System.Collections.Generic;
using System.Text;

namespace AlertStreamReader.Entities
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://www.hikvision.com/ver20/XMLSchema")]
    [System.Xml.Serialization.XmlRoot(Namespace = "http://www.hikvision.com/ver20/XMLSchema", IsNullable = false)]
    public partial class EventNotificationAlert
    {

        private string ipAddressField;

        private byte portNoField;

        private string protocolField;

        private string macAddressField;

        private int channelIDField;

        private DateTime dateTimeField;

        private int activePostCountField;

        private string eventTypeField;

        private string eventStateField;

        private string eventDescriptionField;

        private decimal versionField;

        /// <remarks/>
        public string ipAddress
        {
            get
            {
                return this.ipAddressField;
            }
            set
            {
                this.ipAddressField = value;
            }
        }

        /// <remarks/>
        public byte portNow
        {
            get
            {
                return this.portNoField;
            }
            set
            {
                this.portNoField = value;
            }
        }

        /// <remarks/>
        public string protocol
        {
            get
            {
                return this.protocolField;
            }
            set
            {
                this.protocolField = value;
            }
        }

        /// <remarks/>
        public string macAddress
        {
            get
            {
                return this.macAddressField;
            }
            set
            {
                this.macAddressField = value;
            }
        }

        /// <remarks/>
        public int channelID
        {
            get
            {
                return this.channelIDField;
            }
            set
            {
                this.channelIDField = value;
            }
        }

        /// <remarks/>
        public DateTime dateTime
        {
            get
            {
                return this.dateTimeField;
            }
            set
            {
                this.dateTimeField = value;
            }
        }

        /// <remarks/>
        public int activePostCount
        {
            get
            {
                return this.activePostCountField;
            }
            set
            {
                this.activePostCountField = value;
            }
        }

        /// <remarks/>
        public string eventType
        {
            get
            {
                return this.eventTypeField;
            }
            set
            {
                this.eventTypeField = value;
            }
        }

        /// <remarks/>
        public string eventState
        {
            get
            {
                return this.eventStateField;
            }
            set
            {
                this.eventStateField = value;
            }
        }

        /// <remarks/>
        public string eventDescription
        {
            get
            {
                return this.eventDescriptionField;
            }
            set
            {
                this.eventDescriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttribute()]
        public decimal version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }
}
