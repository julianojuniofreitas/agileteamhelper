using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace JiraTrayApp
{
    [DisplayName("Custom Field")]
    public class CustomFieldTemplate
    {
        private string _value;
        private string _name;

        [Browsable(false)]
        public int Id { get; set; }

        public string Name
        {
            get 
            { 
                return _name; 
            }
            set 
            { 
                _name = value; 
            }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public CustomFieldTemplate()
        {
        }

        public CustomFieldTemplate(string name, string value)
        {
            this._name = name;
            this._value = value;
        }
    }
}
