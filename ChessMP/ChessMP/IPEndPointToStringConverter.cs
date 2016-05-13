using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace ChessMP
{
    public sealed class IPEndPointToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IPEndPoint)?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string input = value as string;

            // value is null, value is not a string or value is an empty string.
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            try
            {
                return CreateIPEndPoint(input);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        //http://stackoverflow.com/questions/2727609/best-way-to-create-ipendpoint-from-string
        private static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            IPAddress ip;

            if (ep.Length < 2)
            {
                if (!IPAddress.TryParse(endPoint, out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }

                return new IPEndPoint(ip, 0);
            }
            else if (ep.Length > 2)
            {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            else
            {
                if (!IPAddress.TryParse(ep[0], out ip))
                {
                    throw new FormatException("Invalid ip-adress");
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
