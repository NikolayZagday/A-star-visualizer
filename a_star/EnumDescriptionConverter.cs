using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a_star
{
    class EnumDescriptionConverter : BaseValueConverter<EnumDescriptionConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileinfo = value.GetType().GetField(value.ToString());

            var descAttrs = fileinfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (descAttrs.Length > 0)
            {
                return ((DescriptionAttribute)descAttrs[0]).Description;
            }

            return value.ToString();
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dfds = targetType.Name;
            var eValues = Enum.GetValues(targetType);

            foreach (var val in eValues)
            {
                var fi = val.GetType().GetField(val.ToString());

                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var descVal = ((attributes.Length > 0) && (!String.IsNullOrEmpty(attributes[0].Description)))
                    ? attributes[0].Description : val.ToString();

                if (descVal == (string)value)
                    return val;
            }

            return null;
        }
    }
}
