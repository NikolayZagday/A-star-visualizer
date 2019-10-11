using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace a_star
{
    /// <summary>
    /// MarkupExtension that converts enum values' description to an array of strings
    /// </summary>
    class EnumDescriptionSource : BaseValueConverter<EnumDescriptionSource>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var eValues = Enum.GetValues((Type)parameter);

            var descValues = new List<string>();


            foreach (var val in eValues)
            {
                var fi = val.GetType().GetField(val.ToString());

                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var descVal = ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description)))
                    ? attributes[0].Description : val.ToString();
                descValues.Add(descVal);
            }

            return descValues;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
