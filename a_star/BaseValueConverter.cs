using System;
using System.Windows.Markup;
using System.Globalization;
using System.Windows.Data;

namespace a_star
{
    public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        private static T mConverter = null;

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return mConverter ?? (mConverter = new T());
        }
    }
}
