using System;
using System.Collections.Generic;
using System.Text;

namespace AlertStreamReader.Entities
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.hikvision.com/ver20/XMLSchema")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.hikvision.com/ver20/XMLSchema", IsNullable = false)]
    public partial class VideoInput
    {

        private VideoInputVideoInputChannelList videoInputChannelListField;

        private decimal versionField;

        /// <remarks/>
        public VideoInputVideoInputChannelList VideoInputChannelList
        {
            get
            {
                return this.videoInputChannelListField;
            }
            set
            {
                this.videoInputChannelListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.hikvision.com/ver20/XMLSchema")]
    public partial class VideoInputVideoInputChannelList
    {

        private VideoInputVideoInputChannelListVideoInputChannel[] videoInputChannelField;

        private decimal versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("VideoInputChannel")]
        public VideoInputVideoInputChannelListVideoInputChannel[] VideoInputChannel
        {
            get
            {
                return this.videoInputChannelField;
            }
            set
            {
                this.videoInputChannelField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.hikvision.com/ver20/XMLSchema")]
    public partial class VideoInputVideoInputChannelListVideoInputChannel
    {

        private int idField;

        private byte inputPortField;

        private bool videoInputEnabledField;

        private string nameField;

        private string videoFormatField;

        private bool enableWD1Field;

        private bool enableWD1FieldSpecified;

        private string resDescField;

        private decimal versionField;

        /// <remarks/>
        public int id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public byte inputPort
        {
            get
            {
                return this.inputPortField;
            }
            set
            {
                this.inputPortField = value;
            }
        }

        /// <remarks/>
        public bool videoInputEnabled
        {
            get
            {
                return this.videoInputEnabledField;
            }
            set
            {
                this.videoInputEnabledField = value;
            }
        }

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public string videoFormat
        {
            get
            {
                return this.videoFormatField;
            }
            set
            {
                this.videoFormatField = value;
            }
        }

        /// <remarks/>
        public bool enableWD1
        {
            get
            {
                return this.enableWD1Field;
            }
            set
            {
                this.enableWD1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool enableWD1Specified
        {
            get
            {
                return this.enableWD1FieldSpecified;
            }
            set
            {
                this.enableWD1FieldSpecified = value;
            }
        }

        /// <remarks/>
        public string resDesc
        {
            get
            {
                return this.resDescField;
            }
            set
            {
                this.resDescField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
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
